
#region ================== Copyright (c) 2007 Pascal vd Heiden, 2014 Boris Iwanski

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * Copyright (c) 2014 Boris Iwanski
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Linq;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using System.Drawing;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.BuilderModes;
using CodeImp.DoomBuilder.BuilderModes.Interface;
using CodeImp.DoomBuilder.Controls;

#endregion

// WICHTIG
//BuilderModes.BuilderPlug.Me.HighlightRange

// namespace CodeImp.DoomBuilder.BuilderModes
namespace CodeImp.DoomBuilder.ThreeDFloorHelper
{
	[EditMode(DisplayName = "3D Floor Editing Mode",
			  SwitchAction = "threedfloorhelpermode",		// Action name used to switch to this mode
			  ButtonImage = "ThreeDFloorIcon.png",	// Image resource name for the button
			  ButtonOrder = int.MinValue + 500,	// Position of the button (lower is more to the left)
			  ButtonGroup = "000_editing",
			  UseByDefault = true,
			  SafeStartMode = false,
			  Volatile = false)]

	public class ThreeDFloorHelperMode : ClassicMode
	{
		#region ================== Constants

		#endregion

		#region ================== Variables
		
		// Highlighted item
		protected Sector highlighted;
		private Association highlightasso = new Association();
		private FlatVertex[] overlayGeometry;

		// Interface
		protected bool editpressed;
		private ThreeDFloorPanel panel;
		private Docker docker;

		// Labels
		private Dictionary<Sector, TextLabel[]> labels;

		ThreeDFloorHelperTooltipElementControl ctrl = new ThreeDFloorHelperTooltipElementControl();
		List<ThreeDFloorHelperTooltipElementControl> tooltipelements;

		ControlSectorArea.Highlight csahighlight = ControlSectorArea.Highlight.None;
		bool dragging = false;

		#endregion

		#region ================== Properties

		public override object HighlightedObject { get { return highlighted; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public ThreeDFloorHelperMode()
		{
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Dispose old labels
				foreach(KeyValuePair<Sector, TextLabel[]> lbl in labels)
					foreach(TextLabel l in lbl.Value) l.Dispose();

				// Dispose base
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		public static void ProcessSectors(List<ThreeDFloor> threedfloors)
		{
			List<Sector> selectedSectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));
			var sectorsByTag = new Dictionary<int, List<Sector>>();
			var sectorsToThreeDFloors = new Dictionary<Sector, List<ThreeDFloor>>();
			var sectorGroups = new List<List<Sector>>();
			var tmpSelectedSectors = new List<Sector>(selectedSectors);

			General.Map.UndoRedo.CreateUndo("Modify 3D floors");

			foreach (ThreeDFloor tdf in threedfloors)
			{
				if (tdf.IsNew)
					if (tdf.CreateGeometry())
						tdf.UpdateGeometry();
			}

			// Fill the sectorsToThreeDFloors dictionary, with a selected sector as key
			// and a list of all 3D floors, that should be applied to to this sector, as value
			foreach (Sector s in selectedSectors)
			{
				if (!sectorsToThreeDFloors.ContainsKey(s))
					sectorsToThreeDFloors.Add(s, new List<ThreeDFloor>());

				foreach (ThreeDFloor tdf in threedfloors)
				{
					if(tdf.TaggedSectors.Contains(s))
						sectorsToThreeDFloors[s].Add(tdf);

					/*
					for (int i = 0; i < ctrl.checkedListBoxSectors.Items.Count; i++)
					{
						string text = ctrl.checkedListBoxSectors.Items[i].ToString();
						bool ischecked = ctrl.checkedListBoxSectors.GetItemChecked(i);

						if (ischecked && text == "Sector " + s.Index.ToString())
						{
							sectorsToThreeDFloors[s].Add(ctrl.ThreeDFloor);
						}
					}
					*/
				}
			}

			// Group all selected sectors by their 3D floors. I.e. each element of sectorGroups
			// is a list of sectors that have the same 3D floors
			while (tmpSelectedSectors.Count > 0)
			{
				Sector s1 = tmpSelectedSectors.First();
				var list = new List<Sector>();
				var delsectors = new List<Sector>();

				foreach (Sector s2 in tmpSelectedSectors)
				{
					if (sectorsToThreeDFloors[s1].ContainsAllElements(sectorsToThreeDFloors[s2]))
					{
						list.Add(s2);
						delsectors.Add(s2);
					}
				}

				foreach (Sector s in delsectors)
					tmpSelectedSectors.Remove(s);

				tmpSelectedSectors.Remove(s1);

				sectorGroups.Add(list);
			}

			// Bind the 3D floors to the selected sectors
			foreach (List<Sector> sectors in sectorGroups)
			{
				int newtag;

				// Just use sectors.First(), all elements in sectors have the same 3D floors anyway
				// If there are no 3D floors associated set the tag to 0
				if (sectorsToThreeDFloors[sectors.First()].Count == 0)
					newtag = 0;
				else
					newtag = BuilderPlug.Me.ControlSectorArea.GetNewTag();

				foreach (Sector s in sectors)
					s.Tag = newtag;

				foreach (ThreeDFloor tdf in sectorsToThreeDFloors[sectors.First()])
					tdf.BindTag(newtag);
			}

			// Remove unused tags from the 3D floors
			foreach (ThreeDFloor tdf in threedfloors)
				tdf.Cleanup();
		}

		// This makes a CRC for the selection
		public int CreateSelectionCRC()
		{
			CRC crc = new CRC();
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			crc.Add(orderedselection.Count);
			foreach(Sector s in orderedselection)
			{
				crc.Add(s.FixedIndex);
			}
			return (int)(crc.Value & 0xFFFFFFFF);
		}

		// This sets up new labels
		private void SetupLabels()
		{
			if(labels != null)
			{
				// Dispose old labels
				foreach(KeyValuePair<Sector, TextLabel[]> lbl in labels)
					foreach(TextLabel l in lbl.Value) l.Dispose();
			}

			// Make text labels for sectors
			labels = new Dictionary<Sector, TextLabel[]>(General.Map.Map.Sectors.Count);
			foreach(Sector s in General.Map.Map.Sectors)
			{
				// Setup labels
				TextLabel[] labelarray = new TextLabel[s.Labels.Count];
				for(int i = 0; i < s.Labels.Count; i++)
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
			if(renderer.StartOverlay(true))
			{
				if (BuilderPlug.Me.UseHighlight)
				{
					renderer.RenderHighlight(overlayGeometry, General.Colors.Selection.WithAlpha(64).ToInt());
				}

				if (BuilderPlug.Me.UseHighlight && highlighted != null)
				{
					renderer.RenderHighlight(highlighted.FlatVertices, General.Colors.Highlight.WithAlpha(64).ToInt());
				}


				if(BuilderPlug.Me.ViewSelectionNumbers)
				{
					// Go for all selected sectors
					ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
					foreach(Sector s in orderedselection)
					{
						// Render labels
						TextLabel[] labelarray = labels[s];
						for(int i = 0; i < s.Labels.Count; i++)
						{
							TextLabel l = labelarray[i];

							// Render only when enough space for the label to see
							float requiredsize = (l.TextSize.Height / 2) / renderer.Scale;
							if(requiredsize < s.Labels[i].radius) renderer.RenderText(l);
						}
					}
				}

				// renderer.RenderLine(new Vector2D(ThreeDFloor.controlsectorarea.Left, ThreeDFloor.controlsectorarea.Top), new Vector2D(ThreeDFloor.controlsectorarea.Left, ThreeDFloor.controlsectorarea.Bottom), 10.0f, new PixelColor(128, 255, 0, 0), true);
				// renderer.RenderRectangleFilled(ThreeDFloor.controlsectorarea, new PixelColor(128, 255, 0, 0), true);
				// renderer.RenderRectangle(ThreeDFloor.controlsectorarea, 1.5f, new PixelColor(255, 255, 0, 0), true);
				BuilderPlug.Me.ControlSectorArea.Draw(renderer, csahighlight);

				// DrawTooltipOverlay();

				renderer.Finish();
			}
		}

		// Generates the tooltip for the 3D floors
		private void UpdateDocker(Sector s)
		{
			List<ThreeDFloor> tdf = BuilderPlug.GetThreeDFloors(new List<Sector> { s });
			int numfloors = tdf.Count;
			int count = 0;

			// Hide all controls if no sector is selected or selected sector has no 3D floors
			if (s == null || numfloors == 0)
			{
				foreach (Control c in tooltipelements)
				{
					c.Visible = false;
				}

				return;
			}

			foreach (ThreeDFloor f in tdf.OrderByDescending(o => o.TopHeight).ToList())
			{
				// Add another control if the list if full
				if (count >= tooltipelements.Count)
				{
					var tte = new ThreeDFloorHelperTooltipElementControl();
					panel.flowLayoutPanel1.Controls.Add(tte);
					tooltipelements.Add(tte);
				}

				General.DisplayZoomedImage(tooltipelements[count].sectorBottomFlat, General.Map.Data.GetFlatImage(f.BottomFlat).GetPreview());
				General.DisplayZoomedImage(tooltipelements[count].sectorBorderTexture, General.Map.Data.GetFlatImage(f.BorderTexture).GetPreview());
				General.DisplayZoomedImage(tooltipelements[count].sectorTopFlat, General.Map.Data.GetFlatImage(f.TopFlat).GetPreview());

				tooltipelements[count].bottomHeight.Text = f.BottomHeight.ToString();
				tooltipelements[count].topHeight.Text = f.TopHeight.ToString();

				tooltipelements[count].Visible = true;

				count++;
			}

			// Hide superfluous controls
			for (; count < tooltipelements.Count; count++)
			{
				tooltipelements[count].Visible = false;
			}
		}

		private void updateOverlaySurfaces()
		{
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			List<FlatVertex> vertsList = new List<FlatVertex>();

			// Go for all selected sectors
			foreach (Sector s in orderedselection) vertsList.AddRange(s.FlatVertices);
			overlayGeometry = vertsList.ToArray();
		}
		
		// Support function for joining and merging sectors
		private void JoinMergeSectors(bool removelines)
		{
			// Remove lines in betwen joining sectors?
			if(removelines)
			{
				// Go for all selected linedefs
				List<Linedef> selectedlines = new List<Linedef>(General.Map.Map.GetSelectedLinedefs(true));
				foreach(Linedef ld in selectedlines)
				{
					// Front and back side?
					if((ld.Front != null) && (ld.Back != null))
					{
						// Both a selected sector, but not the same?
						if(ld.Front.Sector.Selected && ld.Back.Sector.Selected &&
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
			foreach(Sector s in orderedselection)
				if(!s.IsDisposed) { first = s; break; }
			
			// Join all selected sectors with the first
			for(int i = 0; i < orderedselection.Count; i++)
				if((orderedselection[i] != first) && !orderedselection[i].IsDisposed)
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
			bool completeredraw = false;

			// Often we can get away by simply undrawing the previous
			// highlight and drawing the new highlight. But if associations
			// are or were drawn we need to redraw the entire display.

			// Previous association highlights something?
			if((highlighted != null) && (highlighted.Tag > 0)) completeredraw = true;

			// Update the panel with the 3D floors
			UpdateDocker(s);

			// Set highlight association
			if (s != null)
			{
				Vector2D center = (s.Labels.Count > 0 ? s.Labels[0].position : new Vector2D(s.BBox.X + s.BBox.Width / 2, s.BBox.Y + s.BBox.Height / 2));
				highlightasso.Set(center, s.Tag, UniversalType.SectorTag);
			}
			else
				highlightasso.Set(new Vector2D(), 0, 0);

			// New association highlights something?
			if((s != null) && (s.Tag > 0)) completeredraw = true;

			// Change label color
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				TextLabel[] labelarray = labels[highlighted];
				foreach(TextLabel l in labelarray) l.Color = General.Colors.Selection;
			}
			
			// Change label color
			if((s != null) && !s.IsDisposed)
			{
				TextLabel[] labelarray = labels[s];
				foreach(TextLabel l in labelarray) l.Color = General.Colors.Highlight;
			}
			
			// If we're changing associations, then we
			// need to redraw the entire display
			if(completeredraw)
			{
				// Set new highlight and redraw completely
				highlighted = s;
				General.Interface.RedrawDisplay();
			}
			else
			{
				// Update display
				if(renderer.StartPlotter(false))
				{
					// Undraw previous highlight
					if((highlighted != null) && !highlighted.IsDisposed)
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
					if((highlighted != null) && !highlighted.IsDisposed)
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
			if((highlighted != null) && !highlighted.IsDisposed)
				General.Interface.ShowSectorInfo(highlighted);
			else
				General.Interface.HideInfo();
		}

		// This selectes or deselects a sector
		protected void SelectSector(Sector s, bool selectstate, bool update)
		{
			bool selectionchanged = false;

			if(!s.IsDisposed)
			{
				// Select the sector?
				if(selectstate && !s.Selected)
				{
					s.Selected = true;
					selectionchanged = true;
					
					// Setup labels
					ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
					TextLabel[] labelarray = labels[s];
					foreach(TextLabel l in labelarray)
					{
						l.Text = orderedselection.Count.ToString();
						l.Color = General.Colors.Selection;
					}
				}
				// Deselect the sector?
				else if(!selectstate && s.Selected)
				{
					s.Selected = false;
					selectionchanged = true;

					// Clear labels
					TextLabel[] labelarray = labels[s];
					foreach(TextLabel l in labelarray) l.Text = "";

					// Update all other labels
					UpdateSelectedLabels();
				}

				// Selection changed?
				if(selectionchanged)
				{
					// Make update lines selection
					foreach(Sidedef sd in s.Sidedefs)
					{
						bool front, back;
						if(sd.Line.Front != null) front = sd.Line.Front.Sector.Selected; else front = false;
						if(sd.Line.Back != null) back = sd.Line.Back.Sector.Selected; else back = false;
						sd.Line.Selected = front | back;
					}
				}

				if(update)
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
			foreach(Sector s in orderedselection)
			{
				TextLabel[] labelarray = labels[s];
				foreach(TextLabel l in labelarray)
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

			tooltipelements = new List<ThreeDFloorHelperTooltipElementControl>();

			// Add docker
			panel = new ThreeDFloorPanel();
			docker = new Docker("threedfloorhelper", "3D floors", panel);
			General.Interface.AddDocker(docker);
			General.Interface.SelectDocker(docker);

			/*
			for (int i = 0; i < 10; i++)
			{
				var tt = new ThreeDFloorHelperTooltipElementControl();
				tt.Visible = false;
				tooltipelements.Add(tt);
				panel.flowLayoutPanel1.Controls.Add(tt);
			}
			*/


			// Add toolbar buttons
			/*
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.CopyProperties);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.PasteProperties);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SeparatorCopyPaste);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.ViewSelectionNumbers);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.SeparatorSectors1);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientBrightness);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientFloors);
			General.Interface.AddButton(BuilderPlug.Me.MenusForm.MakeGradientCeilings);
			*/

			// Convert geometry selection to sectors only
			General.Map.Map.ConvertSelection(SelectionType.Sectors);

			// Make text labels for sectors
			SetupLabels();
			
			// Update
			UpdateSelectedLabels();
			updateOverlaySurfaces();
			UpdateOverlay();

			BuilderPlug.Me.ControlSectorArea.LoadConfig();
		}
		
		// Mode disengages
		public override void OnDisengage()
		{
			base.OnDisengage();

			// Remove docker
			General.Interface.RemoveDocker(docker);

			// Remove toolbar buttons
			/*
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.CopyProperties);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.PasteProperties);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SeparatorCopyPaste);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.ViewSelectionNumbers);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.SeparatorSectors1);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientBrightness);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientFloors);
			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.MakeGradientCeilings);
			*/
			
			// Keep only sectors selected
			General.Map.Map.ClearSelectedLinedefs();
			
			// Going to EditSelectionMode?
			if(General.Editing.NewMode is EditSelectionMode)
			{
				// Not pasting anything?
				EditSelectionMode editmode = (General.Editing.NewMode as EditSelectionMode);
				if(!editmode.Pasting)
				{
					// No selection made? But we have a highlight!
					if((General.Map.Map.GetSelectedSectors(true).Count == 0) && (highlighted != null))
					{
						// Make the highlight the selection
						SelectSector(highlighted, true, false);
					}
				}
			}

			BuilderPlug.Me.ControlSectorArea.SaveConfig();
			
			// Hide highlight info
			General.Interface.HideInfo();
		}

		// This redraws the display
		public override void OnRedrawDisplay()
		{
			renderer.RedrawSurface();
			
			// Render lines and vertices
			if(renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					renderer.PlotSector(highlighted, General.Colors.Highlight);
					// BuilderPlug.Me.PlotReverseAssociations(renderer, highlightasso);
				}

				renderer.Finish();
			}

			// Render things
			if(renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, Presentation.THINGS_HIDDEN_ALPHA);
				renderer.RenderThingSet(General.Map.ThingsFilter.VisibleThings, 1.0f);
				renderer.Finish();
			}

			// Render selection
			if(renderer.StartOverlay(true))
			{

				// if((highlighted != null) && !highlighted.IsDisposed) BuilderPlug.Me.RenderReverseAssociations(renderer, highlightasso);
				if(selecting) RenderMultiSelection();

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
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				// Flip selection
				SelectSector(highlighted, !highlighted.Selected, true);

				// Update display
				if(renderer.StartPlotter(false))
				{
					// Redraw highlight to show selection
					renderer.PlotSector(highlighted);
					renderer.Finish();
					renderer.Present();
				}
			}
			else
			{
				// Start making a selection
				StartMultiSelection();
			}

			base.OnSelectBegin();
		}

		// End selection
		protected override void OnSelectEnd()
		{
			// Not stopping from multiselection?
			if(!selecting)
			{
				// Item highlighted?
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					// Update display
					if(renderer.StartPlotter(false))
					{
						// Render highlighted item
						renderer.PlotSector(highlighted, General.Colors.Highlight);
						renderer.Finish();
						renderer.Present();
					}

					// Update overlay
					TextLabel[] labelarray = labels[highlighted];
					foreach(TextLabel l in labelarray) l.Color = General.Colors.Highlight;
					updateOverlaySurfaces();
					UpdateOverlay();
					renderer.Present();
				}
			}

			base.OnSelectEnd();
		}

		// Start editing
		protected override void OnEditBegin()
		{
			// Item highlighted?
			if (((highlighted != null) && !highlighted.IsDisposed) || csahighlight == ControlSectorArea.Highlight.Body)
			{
				// Edit pressed in this mode
				editpressed = true;

				if (csahighlight != ControlSectorArea.Highlight.Body)
				{
					// Highlighted item not selected?
					if (!highlighted.Selected && (BuilderPlug.Me.AutoClearSelection || (General.Map.Map.SelectedSectorsCount == 0)))
					{
						// Make this the only selection
						General.Map.Map.ClearSelectedSectors();
						General.Map.Map.ClearSelectedLinedefs();
						SelectSector(highlighted, true, false);
						updateOverlaySurfaces();
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
			}
			
			base.OnEditBegin();
		}

		// Done editing
		protected override void OnEditEnd()
		{
			// Edit pressed in this mode?
			if(editpressed)
			{
				if (csahighlight == ControlSectorArea.Highlight.Body)
				{
					BuilderPlug.Me.ControlSectorArea.Edit();
				}
				else
				{
					// Anything selected?
					ICollection<Sector> selected = General.Map.Map.GetSelectedSectors(true);
					if (selected.Count > 0)
					{
						if (General.Interface.IsActiveWindow)
						{
							// Show sector edit dialog
							// General.Interface.ShowEditSectors(selected);
							DialogResult result = BuilderPlug.Me.ThreeDFloorEditor();

							if (result == DialogResult.OK)
							{
								ProcessSectors(BuilderPlug.TDFEW.ThreeDFloors);
								General.Map.Map.Update();
							}

							// When a single sector was selected, deselect it now
							if (selected.Count == 1)
							{
								General.Map.Map.ClearSelectedSectors();
								General.Map.Map.ClearSelectedLinedefs();
							}

							// Update entire display
							updateOverlaySurfaces();
							General.Interface.RedrawDisplay();
						}
					}

					// Make text labels for sectors
					SetupLabels();
					UpdateSelectedLabels();
				}
			}

			editpressed = false;
			base.OnEditEnd();
		}
		
		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			// Not holding any buttons?
			if (e.Button == MouseButtons.None)
			{
				csahighlight = BuilderPlug.Me.ControlSectorArea.CheckHighlight(mousemappos, renderer.Scale);

				if (csahighlight != ControlSectorArea.Highlight.None)
				{
					Highlight(null);
					return;
				}

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
							Highlight(null);
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
							Highlight(null);
						}
					}
				}
				else
				{
					// Highlight nothing
					Highlight(null);
				}
			}
			else if (dragging && csahighlight != ControlSectorArea.Highlight.None)
			{
				BuilderPlug.Me.ControlSectorArea.SnapToGrid(csahighlight, mousemappos);
				Highlight(null);
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

			if(e.Button == MouseButtons.Right)
				dragging = true;

			/*
			// Edit button used?
			if(General.Actions.CheckActionActive(null, "classicedit"))
			{
				// Anything highlighted?
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					// Highlighted item not selected?
					if(!highlighted.Selected)
					{
						// Select only this sector for dragging
						General.Map.Map.ClearSelectedSectors();
						SelectSector(highlighted, true, true);
					}

					// Start dragging the selection
					General.Editing.ChangeMode(new DragSectorsMode(mousedownmappos));
				}
			}
			*/
		}

		protected override void OnDragStop(MouseEventArgs e)
		{
			dragging = false;
		}

		// This is called wheh selection ends
		protected override void OnEndMultiSelection()
		{
			bool selectionvolume = ((Math.Abs(base.selectionrect.Width) > 0.1f) && (Math.Abs(base.selectionrect.Height) > 0.1f));

			if(BuilderPlug.Me.AutoClearSelection && !selectionvolume)
			{
				General.Map.Map.ClearSelectedLinedefs();
				General.Map.Map.ClearSelectedSectors();
			}

			if(selectionvolume)
			{
				if(General.Interface.ShiftState ^ BuilderPlug.Me.AdditiveSelect)
				{
					// Go for all lines
					foreach(Linedef l in General.Map.Map.Linedefs)
					{
						l.Selected |= ((l.Start.Position.x >= selectionrect.Left) &&
									   (l.Start.Position.y >= selectionrect.Top) &&
									   (l.Start.Position.x <= selectionrect.Right) &&
									   (l.Start.Position.y <= selectionrect.Bottom) &&
									   (l.End.Position.x >= selectionrect.Left) &&
									   (l.End.Position.y >= selectionrect.Top) &&
									   (l.End.Position.x <= selectionrect.Right) &&
									   (l.End.Position.y <= selectionrect.Bottom));
					}
				}
				else
				{
					// Go for all lines
					foreach(Linedef l in General.Map.Map.Linedefs)
					{
						l.Selected = ((l.Start.Position.x >= selectionrect.Left) &&
									  (l.Start.Position.y >= selectionrect.Top) &&
									  (l.Start.Position.x <= selectionrect.Right) &&
									  (l.Start.Position.y <= selectionrect.Bottom) &&
									  (l.End.Position.x >= selectionrect.Left) &&
									  (l.End.Position.y >= selectionrect.Top) &&
									  (l.End.Position.x <= selectionrect.Right) &&
									  (l.End.Position.y <= selectionrect.Bottom));
					}
				}
				
				// Go for all sectors
				foreach(Sector s in General.Map.Map.Sectors)
				{
					// Go for all sidedefs
					bool allselected = true;
					foreach(Sidedef sd in s.Sidedefs)
					{
						if(!sd.Line.Selected)
						{
							allselected = false;
							break;
						}
					}
					
					// Sector completely selected?
					SelectSector(s, allselected, false);
				}
				
				// Make sure all linedefs reflect selected sectors
				foreach(Sidedef sd in General.Map.Map.Sidedefs)
					if(!sd.Sector.Selected && ((sd.Other == null) || !sd.Other.Sector.Selected))
						sd.Line.Selected = false;

				updateOverlaySurfaces();
			}
			
			base.OnEndMultiSelection();
			if(renderer.StartOverlay(true)) renderer.Finish();
			General.Interface.RedrawDisplay();
		}

		// This is called when the selection is updated
		protected override void OnUpdateMultiSelection()
		{
			base.OnUpdateMultiSelection();

			// Render selection
			if(renderer.StartOverlay(true))
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
			if((General.Map.Map.GetSelectedSectors(true).Count == 0) && (highlighted != null))
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
		}
		
		#endregion

		#region ================== Actions

		// This clears the selection
		[BeginAction("clearselection", BaseAction = true)]
		public void ClearSelection()
		{
			// Clear selection
			General.Map.Map.ClearAllSelected();

			// Clear labels
			foreach (TextLabel[] labelarray in labels.Values)
				foreach (TextLabel l in labelarray) l.Text = "";

			updateOverlaySurfaces();

			// Redraw
			General.Interface.RedrawDisplay();
		}
		#endregion
	}
}
