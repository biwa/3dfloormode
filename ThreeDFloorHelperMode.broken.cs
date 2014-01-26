
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using System.Drawing;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.BuilderModes;
using CodeImp.DoomBuilder.BuilderModes.Interface;
using CodeImp.DoomBuilder.GZBuilder.Tools;
using CodeImp.DoomBuilder.Config;


#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorHelper
{
	[EditMode(DisplayName = "3D Floor Editing Mode",
			  SwitchAction = "threedfloorhelpermode",		// Action name used to switch to this mode
			  ButtonImage = "ThreeDFloorIcon.png",	// Image resource name for the button
			  ButtonOrder = int.MinValue + 500,	// Position of the button (lower is more to the left)
			  ButtonGroup = "000_editing",
			  UseByDefault = true,
			  SafeStartMode = false,
              Volatile = false )]

	public class ThreeDFloorHelperMode : BaseClassicMode
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Highlighted item
		protected Sector highlighted;
		private Association highlightasso = new Association();

		// Interface
		protected bool editpressed;

		// Labels
		private Dictionary<Sector, TextLabel[]> labels;

		//mxd. Effects
		private Dictionary<int, string[]> effects;

		#endregion

		#region ================== Properties

		public override object HighlightedObject { get { return highlighted; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public ThreeDFloorHelperMode()
		{
			//mxd
			effects = new Dictionary<int, string[]>();
			foreach (SectorEffectInfo info in General.Map.Config.SortedSectorEffects)
			{
				string name = info.Index + ": " + info.Title;
				effects.Add(info.Index, new[] { name, "E" + info.Index });
			}
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if (!isdisposed)
			{
				// Dispose old labels
				foreach (KeyValuePair<Sector, TextLabel[]> lbl in labels)
					foreach (TextLabel l in lbl.Value) l.Dispose();

				// Dispose base
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		// This makes a CRC for the selection
		public int CreateSelectionCRC()
		{
			CRC crc = new CRC();
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			crc.Add(orderedselection.Count);
			foreach (Sector s in orderedselection)
			{
				crc.Add(s.FixedIndex);
			}
			return (int)(crc.Value & 0xFFFFFFFF);
		}

		// This sets up new labels
		private void SetupLabels()
		{
			if (labels != null)
			{
				// Dispose old labels
				foreach (KeyValuePair<Sector, TextLabel[]> lbl in labels)
					foreach (TextLabel l in lbl.Value) l.Dispose();
			}

			// Make text labels for sectors
			labels = new Dictionary<Sector, TextLabel[]>(General.Map.Map.Sectors.Count);
			foreach (Sector s in General.Map.Map.Sectors)
			{
				// Setup labels
				TextLabel[] labelarray = new TextLabel[s.Labels.Count];
				for (int i = 0; i < s.Labels.Count; i++)
				{
					Vector2D v = s.Labels[i].position;
					labelarray[i] = new TextLabel(20);
					labelarray[i].TransformCoords = true;
					labelarray[i].Rectangle = new RectangleF(v.x, v.y, 0.0f, 0.0f);
					labelarray[i].AlignX = TextAlignmentX.Center;
					labelarray[i].AlignY = TextAlignmentY.Middle;
					labelarray[i].Scale = 14f;
					labelarray[i].Color = General.Colors.Highlight.WithAlpha(255);
					labelarray[i].Backcolor = General.Colors.Background.WithAlpha(255);
				}
				labels.Add(s, labelarray);
			}
		}

		// This updates the overlay
		private void UpdateOverlay()
		{
			if (renderer.StartOverlay(true))
			{
				//mxd. Render highlighted sector
				if (BuilderPlug.Me.UseHighlight)
				{
					if (highlighted != null)
					{
						int highlightedColor = General.Colors.Highlight.WithAlpha(64).ToInt();
						FlatVertex[] verts = new FlatVertex[highlighted.FlatVertices.Length];
						highlighted.FlatVertices.CopyTo(verts, 0);
						for (int i = 0; i < verts.Length; i++)
							verts[i].c = highlightedColor;
						renderer.RenderGeometry(verts, null, true);
					}
				}

				// Go for all selected sectors
				ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);

				//mxd. Render selected sectors
				if (BuilderPlug.Me.UseHighlight)
				{
					int selectedColor = General.Colors.Selection.WithAlpha(64).ToInt(); //mxd
					foreach (Sector s in orderedselection)
					{
						if (s != highlighted)
						{
							FlatVertex[] verts = new FlatVertex[s.FlatVertices.Length];
							s.FlatVertices.CopyTo(verts, 0);
							for (int i = 0; i < verts.Length; i++)
								verts[i].c = selectedColor;
							renderer.RenderGeometry(verts, null, true);
						}
					}
				}

				if (BuilderPlug.Me.ViewSelectionNumbers)
				{
					foreach (Sector s in orderedselection)
					{
						// Render labels
						TextLabel[] labelarray = labels[s];
						for (int i = 0; i < s.Labels.Count; i++)
						{
							TextLabel l = labelarray[i];

							// Render only when enough space for the label to see
							float requiredsize = (l.TextSize.Height / 2) / renderer.Scale;
							if (requiredsize < s.Labels[i].radius) renderer.RenderText(l);
						}
					}
				}

				if (BuilderPlug.Me.ViewSelectionEffects)
				{
					//mxd. Render effect labels
					if (!BuilderPlug.Me.ViewSelectionNumbers)
						renderEffectLabels(orderedselection);
					renderEffectLabels(General.Map.Map.GetSelectedSectors(false));
				}

				renderer.Finish();
			}
		}

		//mxd
		private void renderEffectLabels(ICollection<Sector> selection)
		{
			foreach (Sector s in selection)
			{
				string label = string.Empty;
				string labelShort = string.Empty;

				if (s.Effect != 0)
				{
					if (effects.ContainsKey(s.Effect))
					{
						if (s.Tag != 0)
						{
							label = "Tag " + s.Tag + ", " + effects[s.Effect][0];
							labelShort = "T" + s.Tag + " " + "E" + s.Effect;
						}
						else
						{
							label = effects[s.Effect][0];
							labelShort = "E" + s.Effect;
						}
					}
					else
					{
						if (s.Tag != 0)
						{
							label = "Tag " + s.Tag + ", Effect " + s.Effect;
							labelShort = "T" + s.Tag + " " + "E" + s.Effect;
						}
						else
						{
							label = "Effect " + s.Effect;
							labelShort = "E" + s.Effect;
						}
					}
				}
				else if (s.Tag != 0)
				{
					label = "Tag " + s.Tag;
					labelShort = "T" + s.Tag;
				}

				if (string.IsNullOrEmpty(label)) continue;

				TextLabel[] labelarray = labels[s];
				for (int i = 0; i < s.Labels.Count; i++)
				{
					TextLabel l = labelarray[i];
					l.Color = General.Colors.InfoLine;
					float requiredsize = (General.Map.GetTextSize(label, l.Scale).Width) / renderer.Scale;

					if (requiredsize > s.Labels[i].radius)
					{
						requiredsize = (General.Map.GetTextSize(labelShort, l.Scale).Width) / renderer.Scale;
						l.Text = (requiredsize > s.Labels[i].radius ? "+" : labelShort);
					}
					else
					{
						l.Text = label;
					}

					renderer.RenderText(l);
				}
			}
		}

		// Support function for joining and merging sectors
		private void JoinMergeSectors(bool removelines)
		{
			// Remove lines in betwen joining sectors?
			if (removelines)
			{
				// Go for all selected linedefs
				List<Linedef> selectedlines = new List<Linedef>(General.Map.Map.GetSelectedLinedefs(true));
				foreach (Linedef ld in selectedlines)
				{
					// Front and back side?
					if ((ld.Front != null) && (ld.Back != null))
					{
						// Both a selected sector, but not the same?
						if (ld.Front.Sector.Selected && ld.Back.Sector.Selected &&
						   (ld.Front.Sector != ld.Back.Sector))
						{
							// Remove this line
							ld.Dispose();
						}
					}
				}
			}

			// Find the first sector that is not disposed
			List<Sector> orderedselection = new List<Sector>(General.Map.Map.GetSelectedSectors(true));
			Sector first = null;
			foreach (Sector s in orderedselection)
				if (!s.IsDisposed) { first = s; break; }

			// Join all selected sectors with the first
			for (int i = 0; i < orderedselection.Count; i++)
				if ((orderedselection[i] != first) && !orderedselection[i].IsDisposed)
					orderedselection[i].Join(first);

			// Clear selection
			General.Map.Map.ClearAllSelected();

			// Update
			General.Map.Map.Update();

			// Make text labels for sectors
			SetupLabels();
			UpdateSelectedLabels();
		}

		// This highlights a new item
		protected void Highlight(Sector s)
		{
			// Often we can get away by simply undrawing the previous
			// highlight and drawing the new highlight. But if associations
			// are or were drawn we need to redraw the entire display.

			// Previous association highlights something?
			bool completeredraw = (highlighted != null) && (highlighted.Tag > 0);

			// Set highlight association
			if (s != null)
			{
				Vector2D center = (s.Labels.Count > 0 ? s.Labels[0].position : new Vector2D(s.BBox.X + s.BBox.Width / 2, s.BBox.Y + s.BBox.Height / 2));
				highlightasso.Set(center, s.Tag, UniversalType.SectorTag);
			}
			else
			{
				highlightasso.Set(new Vector2D(), 0, 0);
			}

			// New association highlights something?
			if ((s != null) && (s.Tag > 0)) completeredraw = true;

			// Change label color
			if ((highlighted != null) && !highlighted.IsDisposed)
			{
				TextLabel[] labelarray = labels[highlighted];
				foreach (TextLabel l in labelarray) l.Color = General.Colors.Selection;
			}

			// Change label color
			if ((s != null) && !s.IsDisposed)
			{
				TextLabel[] labelarray = labels[s];
				foreach (TextLabel l in labelarray) l.Color = General.Colors.Highlight;
			}

			// If we're changing associations, then we
			// need to redraw the entire display
			if (completeredraw)
			{
				// Set new highlight and redraw completely
				highlighted = s;
				General.Interface.RedrawDisplay();
			}
			else
			{
				// Update display
				if (renderer.StartPlotter(false))
				{
					// Undraw previous highlight
					if ((highlighted != null) && !highlighted.IsDisposed)
						renderer.PlotSector(highlighted);

					/*
					// Undraw highlighted things
					if(highlighted != null)
						foreach(Thing t in highlighted.Things)
							renderer.RenderThing(t, renderer.DetermineThingColor(t));
					*/

					// Set new highlight
					highlighted = s;

					// Render highlighted item
					if ((highlighted != null) && !highlighted.IsDisposed)
						renderer.PlotSector(highlighted, General.Colors.Highlight);

					/*
					// Render highlighted things
					if(highlighted != null)
						foreach(Thing t in highlighted.Things)
							renderer.RenderThing(t, General.Colors.Highlight);
					*/

					// Done
					renderer.Finish();
				}

				UpdateOverlay();
				renderer.Present();
			}

			// Show highlight info
			if ((highlighted != null) && !highlighted.IsDisposed)
				General.Interface.ShowSectorInfo(highlighted);
			else
				General.Interface.HideInfo();
		}

		// This selectes or deselects a sector
		protected void SelectSector(Sector s, bool selectstate, bool update)
		{
			bool selectionchanged = false;

			if (!s.IsDisposed)
			{
				// Select the sector?
				if (selectstate && !s.Selected)
				{
					s.Selected = true;
					selectionchanged = true;

					// Setup labels
					ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
					TextLabel[] labelarray = labels[s];
					foreach (TextLabel l in labelarray)
					{
						l.Text = orderedselection.Count.ToString();
						l.Color = General.Colors.Selection;
					}
				}
				// Deselect the sector?
				else if (!selectstate && s.Selected)
				{
					s.Selected = false;
					selectionchanged = true;

					// Clear labels
					TextLabel[] labelarray = labels[s];
					foreach (TextLabel l in labelarray) l.Text = "";

					// Update all other labels
					UpdateSelectedLabels();
				}

				// Selection changed?
				if (selectionchanged)
				{
					// Make update lines selection
					foreach (Sidedef sd in s.Sidedefs)
					{
						bool front, back;
						if (sd.Line.Front != null) front = sd.Line.Front.Sector.Selected; else front = false;
						if (sd.Line.Back != null) back = sd.Line.Back.Sector.Selected; else back = false;
						sd.Line.Selected = front | back;
					}
				}

				if (update)
				{
					UpdateOverlay();
					renderer.Present();
				}
			}
		}

		// This updates labels from the selected sectors
		private void UpdateSelectedLabels()
		{
			// Go for all labels in all selected sectors
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			int index = 0;
			foreach (Sector s in orderedselection)
			{
				TextLabel[] labelarray = labels[s];
				foreach (TextLabel l in labelarray)
				{
					// Make sure the text and color are right
					int labelnum = index + 1;
					l.Text = labelnum.ToString();
					l.Color = General.Colors.Selection;
				}
				index++;
			}
		}

		#endregion

		#region ================== Events

		public override void OnHelp()
		{
			General.ShowHelp("e_sectors.html");
		}

		// Cancel mode
		public override void OnCancel()
		{
			base.OnCancel();

			// Return to this mode
			General.Editing.ChangeMode(new SectorsMode());
		}

		// Mode engages
		public override void OnEngage()
		{
			base.OnEngage();
			renderer.SetPresentation(Presentation.Standard);

			// Add toolbar buttons
			/* [BI;start]
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.CopyProperties);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.PasteProperties);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SeparatorCopyPaste);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.ViewSelectionNumbers);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.ViewSelectionEffects);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SeparatorSectors1);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientBrightness);
			if (General.Map.UDMF) General.Interface.AddButton(BuilderPlug.Me.MenusForm.BrightnessGradientMode); //mxd
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientFloors);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientCeilings);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MarqueSelectTouching); //mxd
			if (General.Map.UDMF) General.Interface.AddButton(BuilderPlug.Me.MenusForm.TextureOffsetLock, ToolbarSection.Geometry); //mxd
			[BI;end] */

			// Convert geometry selection to sectors only
			General.Map.Map.ConvertSelection(SelectionType.Sectors);

			// Make text labels for sectors
			SetupLabels();

			// Update
			UpdateSelectedLabels();
			UpdateOverlay();
		}

		// Mode disengages
		public override void OnDisengage()
		{
			base.OnDisengage();

			// Remove toolbar buttons
			/* [BI;start]
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.CopyProperties);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.PasteProperties);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SeparatorCopyPaste);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.ViewSelectionNumbers);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.ViewSelectionEffects);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SeparatorSectors1);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientBrightness);
			if (General.Map.UDMF) General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.BrightnessGradientMode); //mxd
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientFloors);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientCeilings);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MarqueSelectTouching); //mxd
			if (General.Map.UDMF) General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.TextureOffsetLock); //mxd
			[BI;end] */

			// Keep only sectors selected
			General.Map.Map.ClearSelectedLinedefs();

			// Going to EditSelectionMode?
			if (General.Editing.NewMode is EditSelectionMode)
			{
				// Not pasting anything?
				EditSelectionMode editmode = (General.Editing.NewMode as EditSelectionMode);
				if (!editmode.Pasting)
				{
					// No selection made? But we have a highlight!
					if ((General.Map.Map.GetSelectedSectors(true).Count == 0) && (highlighted != null))
					{
						// Make the highlight the selection
						SelectSector(highlighted, true, false);
					}
				}
			}

			// Hide highlight info
			General.Interface.HideInfo();
		}

		// This redraws the display
		public override void OnRedrawDisplay()
		{
			renderer.RedrawSurface();

			// Render lines and vertices
			if (renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);
				if ((highlighted != null) && !highlighted.IsDisposed)
				{
					renderer.PlotSector(highlighted, General.Colors.Highlight);
					BuilderPlug.Me.PlotReverseAssociations(renderer, highlightasso);
				}
				renderer.Finish();
			}

			// Render things
			if (renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, Presentation.THINGS_HIDDEN_ALPHA);
				renderer.RenderThingSet(General.Map.ThingsFilter.VisibleThings, 1.0f);
				renderer.Finish();
			}

			// Render selection
			if (renderer.StartOverlay(true))
			{
				if (highlighted != null && !highlighted.IsDisposed) BuilderPlug.Me.RenderReverseAssociations(renderer, highlightasso); //mxd
				if (selecting) RenderMultiSelection();
				renderer.Finish();
			}

			// Render overlay
			UpdateOverlay();

			renderer.Present();
		}

		// Selection
		protected override void OnSelectBegin()
		{
			// Item highlighted?
			if ((highlighted != null) && !highlighted.IsDisposed)
			{
				// Update display
				if (renderer.StartPlotter(false))
				{
					// Redraw highlight to show selection
					renderer.PlotSector(highlighted);
					renderer.Finish();
					renderer.Present();
				}
			}

			base.OnSelectBegin();
		}

		// End selection
		protected override void OnSelectEnd()
		{
			// Not stopping from multiselection?
			if (!selecting)
			{
				// Item highlighted?
				if ((highlighted != null) && !highlighted.IsDisposed)
				{
					//mxd. Flip selection
					SelectSector(highlighted, !highlighted.Selected, true);

					// Update display
					if (renderer.StartPlotter(false))
					{
						// Render highlighted item
						renderer.PlotSector(highlighted, General.Colors.Highlight);
						renderer.Finish();
						renderer.Present();
					}

					// Update overlay
					TextLabel[] labelarray = labels[highlighted];
					foreach (TextLabel l in labelarray) l.Color = General.Colors.Highlight;
					UpdateOverlay();
					renderer.Present();
					//mxd
				}
				else if (BuilderPlug.Me.AutoClearSelection && General.Map.Map.SelectedSectorsCount > 0)
				{
					General.Map.Map.ClearSelectedLinedefs();
					General.Map.Map.ClearSelectedSectors();
					General.Interface.RedrawDisplay();
				}
			}

			base.OnSelectEnd();
		}

		// Start editing
		protected override void OnEditBegin()
		{
			// Item highlighted?
			if ((highlighted != null) && !highlighted.IsDisposed)
			{
				// Edit pressed in this mode
				editpressed = true;

				// Highlighted item not selected?
				if (!highlighted.Selected && (BuilderPlug.Me.AutoClearSelection || (General.Map.Map.SelectedSectorsCount == 0)))
				{
					// Make this the only selection
					General.Map.Map.ClearSelectedSectors();
					General.Map.Map.ClearSelectedLinedefs();
					SelectSector(highlighted, true, false);
					General.Interface.RedrawDisplay();
				}

				// Update display
				if (renderer.StartPlotter(false))
				{
					// Redraw highlight to show selection
					renderer.PlotSector(highlighted);
					renderer.Finish();
					renderer.Present();
				}
			}
			else if (!selecting) //mxd. We don't want to draw while multiselecting
			{
				// Start drawing mode
				DrawGeometryMode drawmode = new DrawGeometryMode();
				bool snaptogrid = General.Interface.ShiftState ^ General.Interface.SnapToGrid;
				bool snaptonearest = General.Interface.CtrlState ^ General.Interface.AutoMerge;
				DrawnVertex v = DrawGeometryMode.GetCurrentPosition(mousemappos, snaptonearest, snaptogrid, renderer, new List<DrawnVertex>());

				if (drawmode.DrawPointAt(v))
					General.Editing.ChangeMode(drawmode);
				else
					General.Interface.DisplayStatus(StatusType.Warning, "Failed to draw point: outside of map boundaries.");
			}

			base.OnEditBegin();
		}

		// Done editing
		protected override void OnEditEnd()
		{
			// Edit pressed in this mode?
			if (editpressed)
			{
				// Anything selected?
				ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
				if (selected.Count > 0)
				{
					if (General.Interface.IsActiveWindow)
					{
						//mxd. Show realtime vertex edit dialog
						General.Interface.OnEditFormValuesChanged += new EventHandler(sectorEditForm_OnValuesChanged);
						General.Interface.ShowEditSectors(selected);
						General.Interface.OnEditFormValuesChanged -= sectorEditForm_OnValuesChanged;

						General.Map.Renderer2D.UpdateExtraFloorFlag(); //mxd

						// When a single sector was selected, deselect it now
						if (selected.Count == 1)
						{
							General.Map.Map.ClearSelectedSectors();
							General.Map.Map.ClearSelectedLinedefs();
							General.Interface.RedrawDisplay();
						}
					}
				}
			}

			editpressed = false;
			base.OnEditEnd();
		}

		//mxd
		private void sectorEditForm_OnValuesChanged(object sender, EventArgs e)
		{
			// Update entire display
			General.Map.Map.Update();
			General.Interface.RedrawDisplay();
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (panning) return; //mxd. Skip all this jazz while panning

			//mxd
			if (selectpressed && !editpressed && !selecting)
			{
				// Check if moved enough pixels for multiselect
				Vector2D delta = mousedownpos - mousepos;
				if ((Math.Abs(delta.x) > 2) ||
				   (Math.Abs(delta.y) > 2))
				{
					// Start multiselecting
					StartMultiSelection();
				}
			}
			else if (e.Button == MouseButtons.None) // Not holding any buttons?
			{
				// Find the nearest linedef within highlight range
				Linedef l = General.Map.Map.NearestLinedef(mousemappos);
				if (l != null)
				{
					// Check on which side of the linedef the mouse is
					float side = l.SideOfLine(mousemappos);
					if (side > 0)
					{
						// Is there a sidedef here?
						if (l.Back != null)
						{
							// Highlight if not the same
							if (l.Back.Sector != highlighted) Highlight(l.Back.Sector);
						}
						else
						{
							// Highlight nothing
							if (highlighted != null) Highlight(null);
						}
					}
					else
					{
						// Is there a sidedef here?
						if (l.Front != null)
						{
							// Highlight if not the same
							if (l.Front.Sector != highlighted) Highlight(l.Front.Sector);
						}
						else
						{
							// Highlight nothing
							if (highlighted != null) Highlight(null);
						}
					}
				}
				else
				{
					// Highlight nothing
					if (highlighted != null) Highlight(null);
				}
			}
		}

		// Mouse leaves
		public override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			// Highlight nothing
			Highlight(null);
		}

		// Mouse wants to drag
		protected override void OnDragStart(MouseEventArgs e)
		{
			base.OnDragStart(e);

			// Edit button used?
			if (General.Actions.CheckActionActive(null, "classicedit"))
			{
				// Anything highlighted?
				if ((highlighted != null) && !highlighted.IsDisposed)
				{
					// Highlighted item not selected?
					if (!highlighted.Selected)
					{
						// Select only this sector for dragging
						General.Map.Map.ClearSelectedSectors();
						SelectSector(highlighted, true, true);
					}

					// Start dragging the selection
					if (!BuilderPlug.Me.DontMoveGeometryOutsideMapBoundary || canDrag()) //mxd
						General.Editing.ChangeMode(new DragSectorsMode(mousedownmappos));
				}
			}
		}

		//mxd. Check if any selected sector is outside of map boundary
		private bool canDrag()
		{
			ICollection<Sector> selectedsectors = General.Map.Map.GetSelectedSectors(true);
			int unaffectedCount = 0;

			foreach (Sector s in selectedsectors)
			{
				// Make sure the sector is inside the map boundary
				foreach (Sidedef sd in s.Sidedefs)
				{
					if (sd.Line.Start.Position.x < General.Map.Config.LeftBoundary || sd.Line.Start.Position.x > General.Map.Config.RightBoundary
						|| sd.Line.Start.Position.y > General.Map.Config.TopBoundary || sd.Line.Start.Position.y < General.Map.Config.BottomBoundary
						|| sd.Line.End.Position.x < General.Map.Config.LeftBoundary || sd.Line.End.Position.x > General.Map.Config.RightBoundary
						|| sd.Line.End.Position.y > General.Map.Config.TopBoundary || sd.Line.End.Position.y < General.Map.Config.BottomBoundary)
					{

						SelectSector(s, false, true);
						unaffectedCount++;
						break;
					}
				}
			}

			if (unaffectedCount == selectedsectors.Count)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Unable to drag selection: " + (selectedsectors.Count == 1 ? "selected sector is" : "all of selected sectors are") + " outside of map boundary!");
				General.Interface.RedrawDisplay();
				return false;
			}

			if (unaffectedCount > 0)
				General.Interface.DisplayStatus(StatusType.Warning, unaffectedCount + " of selected sectors " + (unaffectedCount == 1 ? "is" : "are") + " outside of map boundary!");

			return true;
		}

		// This is called wheh selection ends
		protected override void OnEndMultiSelection()
		{
			bool selectionvolume = ((Math.Abs(base.selectionrect.Width) > 0.1f) && (Math.Abs(base.selectionrect.Height) > 0.1f));

			if (selectionvolume)
			{
				//mxd. collect changed sectors
				if (marqueSelectionMode == MarqueSelectionMode.SELECT)
				{
					if (BuilderPlug.Me.MarqueSelectTouching)
					{
						//select sectors fully and partially inside selection, deselect all other sectors
						foreach (Sector s in General.Map.Map.Sectors)
						{
							bool select = false;

							foreach (Sidedef sd in s.Sidedefs)
							{
								if (selectionrect.Contains(sd.Line.Start.Position.x, sd.Line.Start.Position.y) || selectionrect.Contains(sd.Line.End.Position.x, sd.Line.End.Position.y))
								{
									select = true;
									break;
								}
							}

							if (select && !s.Selected)
								SelectSector(s, true, false);
							else if (!select && s.Selected)
								SelectSector(s, false, false);
						}
					}
					else
					{
						//select sectors fully inside selection, deselect all other sectors
						foreach (Sector s in General.Map.Map.Sectors)
						{
							bool select = true;

							foreach (Sidedef sd in s.Sidedefs)
							{
								if (!selectionrect.Contains(sd.Line.Start.Position.x, sd.Line.Start.Position.y) || !selectionrect.Contains(sd.Line.End.Position.x, sd.Line.End.Position.y))
								{
									select = false;
									break;
								}
							}

							if (select && !s.Selected)
								SelectSector(s, true, false);
							else if (!select && s.Selected)
								SelectSector(s, false, false);
						}
					}
				}
				else if (marqueSelectionMode == MarqueSelectionMode.ADD)
				{ //additive selection
					if (BuilderPlug.Me.MarqueSelectTouching)
					{
						//select sectors fully and partially inside selection, leave others untouched 
						foreach (Sector s in General.Map.Map.Sectors)
						{
							if (s.Selected) continue;
							bool select = false;

							foreach (Sidedef sd in s.Sidedefs)
							{
								if (selectionrect.Contains(sd.Line.Start.Position.x, sd.Line.Start.Position.y) || selectionrect.Contains(sd.Line.End.Position.x, sd.Line.End.Position.y))
								{
									select = true;
									break;
								}
							}

							if (select) SelectSector(s, true, false);
						}
					}
					else
					{
						//select sectors fully inside selection, leave others untouched 
						foreach (Sector s in General.Map.Map.Sectors)
						{
							if (s.Selected) continue;
							bool select = true;

							foreach (Sidedef sd in s.Sidedefs)
							{
								if (!selectionrect.Contains(sd.Line.Start.Position.x, sd.Line.Start.Position.y) || !selectionrect.Contains(sd.Line.End.Position.x, sd.Line.End.Position.y))
								{
									select = false;
									break;
								}
							}

							if (select) SelectSector(s, true, false);
						}
					}

				}
				else if (marqueSelectionMode == MarqueSelectionMode.SUBTRACT)
				{
					if (BuilderPlug.Me.MarqueSelectTouching)
					{
						//deselect sectors fully and partially inside selection, leave others untouched 
						foreach (Sector s in General.Map.Map.Sectors)
						{
							if (!s.Selected) continue;
							bool deselect = false;

							foreach (Sidedef sd in s.Sidedefs)
							{
								if (selectionrect.Contains(sd.Line.Start.Position.x, sd.Line.Start.Position.y) || selectionrect.Contains(sd.Line.End.Position.x, sd.Line.End.Position.y))
								{
									deselect = true;
									break;
								}
							}

							if (deselect) SelectSector(s, false, false);
						}
					}
					else
					{
						//deselect sectors fully inside selection, leave others untouched 
						foreach (Sector s in General.Map.Map.Sectors)
						{
							if (!s.Selected) continue;
							bool deselect = true;

							foreach (Sidedef sd in s.Sidedefs)
							{
								if (!selectionrect.Contains(sd.Line.Start.Position.x, sd.Line.Start.Position.y) || !selectionrect.Contains(sd.Line.End.Position.x, sd.Line.End.Position.y))
								{
									deselect = false;
									break;
								}
							}

							if (deselect) SelectSector(s, false, false);
						}
					}

				}
				else
				{ //should be Intersect
					if (BuilderPlug.Me.MarqueSelectTouching)
					{
						//deselect sectors which are fully outside selection
						foreach (Sector s in General.Map.Map.Sectors)
						{
							if (!s.Selected) continue;
							bool keep = false;

							foreach (Sidedef sd in s.Sidedefs)
							{
								if (selectionrect.Contains(sd.Line.Start.Position.x, sd.Line.Start.Position.y) || selectionrect.Contains(sd.Line.End.Position.x, sd.Line.End.Position.y))
								{
									keep = true;
									break;
								}
							}

							if (!keep) SelectSector(s, false, false);
						}
					}
					else
					{
						//deselect sectors which are fully and partially outside selection
						foreach (Sector s in General.Map.Map.Sectors)
						{
							if (!s.Selected) continue;
							bool keep = true;

							foreach (Sidedef sd in s.Sidedefs)
							{
								if (!selectionrect.Contains(sd.Line.Start.Position.x, sd.Line.Start.Position.y) || !selectionrect.Contains(sd.Line.End.Position.x, sd.Line.End.Position.y))
								{
									keep = false;
									break;
								}
							}

							if (!keep) SelectSector(s, false, false);
						}
					}
				}

				// Make sure all linedefs reflect selected sectors
				foreach (Sidedef sd in General.Map.Map.Sidedefs)
					sd.Line.Selected = sd.Sector.Selected || (sd.Other != null && sd.Other.Sector.Selected);
			}

			base.OnEndMultiSelection();
			if (renderer.StartOverlay(true)) renderer.Finish();
			General.Interface.RedrawDisplay();
		}

		// This is called when the selection is updated
		protected override void OnUpdateMultiSelection()
		{
			base.OnUpdateMultiSelection();

			// Render selection
			if (renderer.StartOverlay(true))
			{
				RenderMultiSelection();
				renderer.Finish();
				renderer.Present();
			}
		}

		// When copying
		public override bool OnCopyBegin()
		{
			// No selection made? But we have a highlight!
			if ((General.Map.Map.GetSelectedSectors(true).Count == 0) && (highlighted != null))
			{
				// Make the highlight the selection
				SelectSector(highlighted, true, true);
			}

			return base.OnCopyBegin();
		}

		// When undo is used
		public override bool OnUndoBegin()
		{
			// Clear ordered selection
			General.Map.Map.ClearAllSelected();

			return base.OnUndoBegin();
		}

		// When undo is performed
		public override void OnUndoEnd()
		{
			// Clear labels
			SetupLabels();
		}

		// When redo is used
		public override bool OnRedoBegin()
		{
			// Clear ordered selection
			General.Map.Map.ClearAllSelected();

			return base.OnRedoBegin();
		}

		// When redo is performed
		public override void OnRedoEnd()
		{
			// Clear labels
			SetupLabels();
			base.OnRedoEnd(); //mxd
		}

		#endregion

		#region ================== Actions

		// This clears the selection
		[BeginAction("clearselection", BaseAction = true)]
		public void ClearSelection()
		{
			// Clear selection
			General.Map.Map.ClearAllSelected();

			General.Interface.DisplayStatus(StatusType.Selection, string.Empty); //mxd

			// Clear labels
			foreach (TextLabel[] labelarray in labels.Values)
				foreach (TextLabel l in labelarray) l.Text = "";

			// Redraw
			General.Interface.RedrawDisplay();
		}

		#endregion
	}
}
