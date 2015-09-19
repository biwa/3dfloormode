
#region ================== Copyright (c) 2007 Pascal vd Heiden

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
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.BuilderModes;
using CodeImp.DoomBuilder.GZBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
    public class SlopeObject
    {
        private ThreeDFloor threedfloor;
        private Vector2D position;
		private int v;

        public ThreeDFloor ThreeDFloor { get { return threedfloor; } set { threedfloor = value; } }
        public Vector2D Position { get { return position; } set { position = value; } }
		public int V { get { return v; } set { v = value; } }
    }

    [EditMode(DisplayName = "Slope Mode",
              SwitchAction = "threedslopemode",		// Action name used to switch to this mode
              ButtonImage = "SlopeModeIcon.png",	// Image resource name for the button
              ButtonOrder = int.MinValue + 501,	// Position of the button (lower is more to the left)
              ButtonGroup = "000_editing",
			  SupportedMapFormats = new[] { "UniversalMapSetIO" },
              UseByDefault = true,
              SafeStartMode = true)]

    public class SlopeMode : ClassicMode
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Highlighted item
        private SlopeVertex highlightedslope;
		private Sector highlightedsector;
		private Association[] association = new Association[Thing.NUM_ARGS];
		private List<SlopeVertexGroup> copyslopevertexgroups;

		// Interface
		private bool editpressed;

        private List<ThreeDFloor> threedfloors;
        private List<SlopeObject> slopeobjects;
		bool dragging = false;

		private List<TextLabel> labels;
		private FlatVertex[] overlaygeometry;
		private FlatVertex[] overlaytaggedgeometry;
		private FlatVertex[] selectedsectorgeometry;
		private FlatVertex[] selectedtaggedsectorgeometry;

		private Vector2D dragstartmappos;
		private List<Vector2D> oldpositions;

		private static PlaneType slopemode = PlaneType.Floor;
		
		#endregion

		#region ================== Properties

		public Sector HighlightedSector { get { return highlightedsector; } }

		#endregion

		#region ================== Constructor / Disposer

		#endregion

		#region ================== Methods

		public override void OnHelp()
		{
			General.ShowHelp("e_things.html");
		}

		// Cancel mode
		public override void OnCancel()
		{
			base.OnCancel();

			// Return to this mode
			General.Editing.ChangeMode(new ThingsMode());
		}

		// Mode engages
		public override void OnEngage()
		{
            base.OnEngage();
            renderer.SetPresentation(Presentation.Things);

			General.Interface.AddButton(BuilderPlug.Me.MenusForm.UpdateSlopes);

            // Convert geometry selection to sectors
            General.Map.Map.ConvertSelection(SelectionType.Sectors);

            // Get all 3D floors in the map
            threedfloors = BuilderPlug.GetThreeDFloors(General.Map.Map.Sectors.ToList());

			SetupLabels();

			foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
			{
				svg.FindSectors();
			}

			// Update overlay surfaces, so that selected sectors are drawn correctly
			updateOverlaySurfaces();

			UpdateSlopeObjects();
		}

		// Mode disengages
		public override void OnDisengage()
		{
			base.OnDisengage();

			General.Interface.RemoveButton(BuilderPlug.Me.MenusForm.UpdateSlopes);
			
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

				foreach (Sector s in General.Map.Map.GetSelectedSectors(true).ToList())
					renderer.PlotSector(s, General.Colors.Selection);

				if ((highlightedsector != null) && !highlightedsector.IsDisposed)
					renderer.PlotSector(highlightedsector, General.Colors.Highlight);

				renderer.Finish();
			}

			// Render things
			if(renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, Presentation.THINGS_HIDDEN_ALPHA);
				renderer.RenderThingSet(General.Map.ThingsFilter.VisibleThings, 1.0f);

				renderer.Finish();
			}

            UpdateOverlay();

			renderer.Present();
		}

		private void UpdateSlopeObjects()
		{
			slopeobjects = new List<SlopeObject>();

			foreach (ThreeDFloor tdf in threedfloors)
			{
				if (!tdf.TopSloped || !tdf.BottomSloped)
					continue;

				SlopeObject so = new SlopeObject();
				so.ThreeDFloor = tdf;
				so.Position = tdf.BottomSlope.V1;
				so.V = 1;
				slopeobjects.Add(so);

				so = new SlopeObject();
				so.ThreeDFloor = tdf;
				so.Position = tdf.BottomSlope.V2;
				so.V = 2;
				slopeobjects.Add(so);

				if (!tdf.BottomSlope.IsSimple)
				{
					so = new SlopeObject();
					so.ThreeDFloor = tdf;
					so.Position = tdf.BottomSlope.V3;
					so.V = 3;
					slopeobjects.Add(so);
				}

			}
		}

		private void SetupLabels()
		{
			labels = new List<TextLabel>();
			Dictionary<Sector, List<TextLabel>> sectorlabels = new Dictionary<Sector, List<TextLabel>>();
			PixelColor white = new PixelColor(255, 255, 255, 255);

			Dictionary<Sector, Dictionary<PlaneType, SlopeVertexGroup>> requiredlabels = new Dictionary<Sector, Dictionary<PlaneType, SlopeVertexGroup>>();

			// Go through all sectors that belong to a SVG and set which SVG their floor and
			// ceiling belongs to
			foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
			{
				foreach (Sector s in svg.Sectors)
				{
					if (!requiredlabels.ContainsKey(s))
					{
						requiredlabels.Add(s, new Dictionary<PlaneType, SlopeVertexGroup>());
						requiredlabels[s][PlaneType.Floor] = null;
						requiredlabels[s][PlaneType.Ceiling] = null;
					}

					if ((svg.SectorPlanes[s] & PlaneType.Floor) == PlaneType.Floor)
						requiredlabels[s][PlaneType.Floor] = svg;

					if ((svg.SectorPlanes[s] & PlaneType.Ceiling) == PlaneType.Ceiling)
						requiredlabels[s][PlaneType.Ceiling] = svg;
				}
			}

			foreach (KeyValuePair<Sector, Dictionary<PlaneType, SlopeVertexGroup>> element in requiredlabels)
			{
				int numlabels = 0;
				int counter = 0;
				Sector sector = element.Key;
				Dictionary<PlaneType, SlopeVertexGroup> dict = element.Value;

				// How many planes of this sector have a SVG?
				if (dict[PlaneType.Floor] != null) numlabels++;
				if (dict[PlaneType.Ceiling] != null) numlabels++;

				TextLabel[] labelarray = new TextLabel[sector.Labels.Count * numlabels];

				foreach(PlaneType pt in  Enum.GetValues(typeof(PlaneType)))
				{
					if (dict[pt] == null) continue;

					// If we're in the second iteration of the loop both the ceiling and
					// floor of the sector have a SVG, so change to alignment of the
					// existing labels from center to left
					if (counter == 1)
					{
						for (int i = 0; i < sector.Labels.Count; i++)
						{
							labelarray[i].AlignX = TextAlignmentX.Left;
						}
					}

					for (int i = 0; i < sector.Labels.Count; i++)
					{
						int apos = sector.Labels.Count * counter + i;
						Vector2D v = sector.Labels[i].position;
						labelarray[apos] = new TextLabel(20);
						labelarray[apos].TransformCoords = true;
						labelarray[apos].AlignY = TextAlignmentY.Middle;
						labelarray[apos].Scale = 14f;
						labelarray[apos].Backcolor = General.Colors.Background.WithAlpha(255);
						labelarray[apos].Rectangle = new RectangleF(v.x, v.y, 0.0f, 0.0f);

						if (dict[pt].Vertices.Contains(highlightedslope))
							labelarray[apos].Color = General.Colors.Highlight.WithAlpha(255);
						else
							labelarray[apos].Color = white;

						if (pt == PlaneType.Floor)
							labelarray[apos].Text = "F";
						else
							labelarray[apos].Text = "C";

						// First iteration of loop -> may be the only label needed, so
						// set it to be in the center
						if(counter == 0)
							labelarray[apos].AlignX = TextAlignmentX.Center;
						// Second iteration of the loop so set it to be aligned at the right
						else if(counter == 1)
							labelarray[apos].AlignX = TextAlignmentX.Right;
					}

					counter++;
				}

				labels.AddRange(labelarray);
			}

			
			foreach(SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
			{
				for (int i = 0; i < svg.Vertices.Count; i++)
				{
					SlopeVertex sv = svg.Vertices[i];
					float x = sv.Pos.x;
					float y = sv.Pos.y - 14 * (1 / renderer.Scale);

					TextLabel label = new TextLabel(20);
					label.TransformCoords = true;
					label.Rectangle = new RectangleF(x, y, 0.0f, 0.0f);
					label.AlignX = TextAlignmentX.Center;
					label.AlignY = TextAlignmentY.Middle;
					label.Scale = 14f;
					label.Backcolor = General.Colors.Background.WithAlpha(255);
					label.Text = "";

					// Rearrange labels if they'd be (exactly) on each other
					// TODO: do something like that also for overlapping labels
					foreach (TextLabel l in labels)
					{
						if (l.Rectangle.X == label.Rectangle.X && l.Rectangle.Y == label.Rectangle.Y)
							label.Rectangle = new RectangleF(x, l.Rectangle.Y - 14.0f * (1 / renderer.Scale), 0.0f, 0.0f);
					}

					if (svg.Vertices.Contains(highlightedslope))
						label.Color = General.Colors.Highlight.WithAlpha(255);
					else if (sv.Selected)
						label.Color = General.Colors.Selection.WithAlpha(255);
					else
						label.Color = white;

					label.Text += String.Format("Z: {0}", sv.Z);

					labels.Add(label);
				}
			}
		}

		// This updates the overlay
        private void UpdateOverlay()
        {
            float size = 9 / renderer.Scale;

			SetupLabels();

            if (renderer.StartOverlay(true))
            {
				if(overlaygeometry != null)
					renderer.RenderHighlight(overlaygeometry, General.Colors.ModelWireframe.WithAlpha(64).ToInt());

				if (overlaytaggedgeometry != null)
					renderer.RenderHighlight(overlaytaggedgeometry, General.Colors.Vertices.WithAlpha(64).ToInt());

				if (selectedsectorgeometry != null)
					renderer.RenderHighlight(selectedsectorgeometry, General.Colors.Selection.WithAlpha(64).ToInt());

				if (BuilderPlug.Me.UseHighlight && highlightedsector != null)
				{
					renderer.RenderHighlight(highlightedsector.FlatVertices, General.Colors.Highlight.WithAlpha(64).ToInt());
				}

				List<SlopeVertex> vertices = new List<SlopeVertex>();

				// Store all slope vertices and draw the lines between them
				foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
				{
					for (int i = 0; i < svg.Vertices.Count; i++)
					{
						vertices.Add(svg.Vertices[i]);

						if (i < svg.Vertices.Count - 1)
							renderer.RenderLine(svg.Vertices[0].Pos, svg.Vertices[i+1].Pos, 1, new PixelColor(255, 255, 255, 255), true);
					}
				}

				// Sort the slope vertex list and draw them. The sorting ensures that selected vertices are always drawn on top
				foreach(SlopeVertex sv in vertices.OrderBy(o=>o.Selected))
				{
					PixelColor c = General.Colors.Indication;
					Vector3D v = sv.Pos;

					if (sv.Selected)
						c = General.Colors.Selection;

					renderer.RenderRectangleFilled(new RectangleF(v.x - size / 2, v.y - size / 2, size, size), General.Colors.Background, true);
					renderer.RenderRectangle(new RectangleF(v.x - size / 2, v.y - size / 2, size, size), 2, c, true);
				}

				// Draw highlighted slope vertex
				if (highlightedslope != null)
				{
					renderer.RenderRectangleFilled(new RectangleF(highlightedslope.Pos.x - size / 2, highlightedslope.Pos.y - size / 2, size, size), General.Colors.Background, true);
					renderer.RenderRectangle(new RectangleF(highlightedslope.Pos.x - size / 2, highlightedslope.Pos.y - size / 2, size, size), 2, General.Colors.Highlight, true);
				}

				foreach (TextLabel l in labels)
					renderer.RenderText(l);

				if (selecting)
					RenderMultiSelection();

                renderer.Finish();
            }           
        }

		private void updateOverlaySurfaces()
		{
			string[] fieldnames = new string[] { "user_floorplane_id", "user_ceilingplane_id" };
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			List<FlatVertex> vertslist = new List<FlatVertex>();
			List<Sector> highlightedsectors = new List<Sector>();
			List<Sector> highlightedtaggedsectors = new List<Sector>();

			// Highlighted slope
			if (highlightedslope != null)
			{
				SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(highlightedslope);

				// All sectors the slope applies to
				foreach (Sector s in svg.Sectors)
				{
					if (s != null && !s.IsDisposed)
					{
						vertslist.AddRange(s.FlatVertices);
						highlightedsectors.Add(s);
					}
				}

				overlaygeometry = vertslist.ToArray();

				// All sectors that are tagged because of 3D floors
				vertslist = new List<FlatVertex>();

				foreach (Sector s in svg.TaggedSectors)
				{
					if (s != null && !s.IsDisposed)
					{
						vertslist.AddRange(s.FlatVertices);
						highlightedtaggedsectors.Add(s);
					}
				}

				overlaytaggedgeometry = vertslist.ToArray();
			}
			else
			{
				overlaygeometry = new FlatVertex[0];
				overlaytaggedgeometry = new FlatVertex[0];
			}

			// Selected sectors
			vertslist = new List<FlatVertex>();

			foreach (Sector s in orderedselection)
				if(!highlightedsectors.Contains(s))
					vertslist.AddRange(s.FlatVertices);

			selectedsectorgeometry = vertslist.ToArray();
		}

		// This highlights a new item
		protected void HighlightSector(Sector s)
		{
			// Update display

			highlightedsector = s;
			/*
			if (renderer.StartPlotter(false))
			{
				// Undraw previous highlight
				if ((highlightedsector != null) && !highlightedsector.IsDisposed)
					renderer.PlotSector(highlightedsector);

				// Set new highlight
				highlightedsector = s;

				// Render highlighted item
				if ((highlightedsector != null) && !highlightedsector.IsDisposed)
					renderer.PlotSector(highlightedsector, General.Colors.Highlight);

				// Done
				renderer.Finish();
			}

			UpdateOverlay();
			renderer.Present();
			*/
			General.Interface.RedrawDisplay();

			// Show highlight info
			if ((highlightedsector != null) && !highlightedsector.IsDisposed)
				General.Interface.ShowSectorInfo(highlightedsector);
			else
				General.Interface.HideInfo();
		}

		// This selectes or deselects a sector
		protected void SelectSector(Sector s, bool selectstate)
		{
			bool selectionchanged = false;

			if (!s.IsDisposed)
			{
				// Select the sector?
				if (selectstate && !s.Selected)
				{
					s.Selected = true;
					selectionchanged = true;
				}
				// Deselect the sector?
				else if (!selectstate && s.Selected)
				{
					s.Selected = false;
					selectionchanged = true;
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

					//mxd. Also (de)select things?
					if (General.Interface.AltState)
					{
						foreach (Thing t in General.Map.ThingsFilter.VisibleThings)
						{
							t.DetermineSector();
							if (t.Sector != s) continue;
							t.Selected = s.Selected;
						}
					}
				}
			}
		}
		
		// Selection
		protected override void OnSelectBegin()
		{
			// Item highlighted?
			if(highlightedslope != null)
			{
				// Flip selection
				highlightedslope.Selected = !highlightedslope.Selected;

				updateOverlaySurfaces();
				UpdateOverlay();
			}

			base.OnSelectBegin();
		}

		// End selection
		protected override void OnSelectEnd()
		{
			// Not ending from a multi-selection?
			if(!selecting)
			{
				// Item highlighted?
				if (highlightedslope != null)
				{
					updateOverlaySurfaces();
					UpdateOverlay();
				}

				if (highlightedsector != null)
				{
					SelectSector(highlightedsector, !highlightedsector.Selected);
					// highlightedsector.Selected = !highlightedsector.Selected;

					updateOverlaySurfaces();
					General.Interface.RedrawDisplay();
				}
			}

			base.OnSelectEnd();
		}

		// Done editing
		protected override void OnEditEnd()
		{
			base.OnEditEnd();

			if (dragging) return;

			if (highlightedslope != null)
			{
				SlopeVertex sv = highlightedslope;

				List<SlopeVertex> vertices = GetSelectedSlopeVertices();

				if (!vertices.Contains(highlightedslope))
					vertices.Add(highlightedslope);

				SlopeVertexEditForm svef = new SlopeVertexEditForm();
				svef.Setup(vertices);

				DialogResult result = svef.ShowDialog((Form)General.Interface);

				if (result == DialogResult.OK)
				{
					General.Map.IsChanged = true;

					BuilderPlug.Me.UpdateSlopes();
				}

				highlightedslope = null;
			}
			else if(highlightedsector != null)
			{
				if (!highlightedsector.Selected && General.Map.Map.SelectedSectorsCount == 0)
					highlightedsector.Selected = true;

				BuilderPlug.Me.MenusForm.AddSectorsContextMenu.Show(Cursor.Position);
			}

			updateOverlaySurfaces();
			UpdateOverlay();

			General.Interface.RedrawDisplay();
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (selectpressed && !editpressed && !selecting)
			{
				// Check if moved enough pixels for multiselect
				Vector2D delta = mousedownpos - mousepos;
				if ((Math.Abs(delta.x) > 2) || (Math.Abs(delta.y) > 2))
				{
					// Start multiselecting
					StartMultiSelection();
				}
			}
			else if(e.Button == MouseButtons.None)
			{
				SlopeVertex oldhighlight = highlightedslope;
				Sector oldhighlightedsector = highlightedsector;

                float distance = float.MaxValue;
                float d;

				highlightedslope = null;

				foreach(SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups) {
					foreach(SlopeVertex sv in svg.Vertices)
					{
						d = Vector2D.Distance(sv.Pos, mousemappos);

						if (d <= BuilderModes.BuilderPlug.Me.HighlightRange / renderer.Scale && d < distance)
						{
							distance = d;
							highlightedslope = sv;
						}
					}
				}

				// If no slope vertex is highlighted, check if a sector should be
				if (highlightedslope == null)
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
								if (l.Back.Sector != highlightedsector) HighlightSector(l.Back.Sector);
							}
							else
							{
								// Highlight nothing
								if (highlightedsector != null) HighlightSector(null);
							}
						}
						else
						{
							// Is there a sidedef here?
							if (l.Front != null)
							{
								// Highlight if not the same
								if (l.Front.Sector != highlightedsector) HighlightSector(l.Front.Sector);
							}
							else
							{
								// Highlight nothing
								if (highlightedsector != null) HighlightSector(null);
							}
						}
					}
				}
				else
				{
					HighlightSector(null);
				}

				if (highlightedslope != oldhighlight)
				{
					updateOverlaySurfaces();
					UpdateOverlay();
					General.Interface.RedrawDisplay();
				}
			}
			else if (dragging && highlightedslope != null)
			{
				int i = 0;

				Vector2D newpos = GridSetup.SnappedToGrid(mousemappos, General.Map.Grid.GridSizeF, 1.0f / General.Map.Grid.GridSizeF);
				Vector2D snappedstartpos = GridSetup.SnappedToGrid(dragstartmappos, General.Map.Grid.GridSizeF, 1.0f / General.Map.Grid.GridSizeF);

				foreach (SlopeVertex sl in GetSelectedSlopeVertices())
				{
					sl.Pos = oldpositions[i] + newpos - snappedstartpos;
					i++;
				}

				highlightedslope.Pos = oldpositions[i] + newpos - snappedstartpos;

				General.Map.IsChanged = true;

				updateOverlaySurfaces();
				UpdateOverlay();
				General.Interface.RedrawDisplay();
			}
			else if (selecting)
			{
				UpdateOverlay();
				General.Interface.RedrawDisplay();
			}
		}

		// Mouse leaves
		public override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			// Highlight nothing
			highlightedslope = null;
		}

		// Mouse wants to drag
		protected override void OnDragStart(MouseEventArgs e)
		{
			base.OnDragStart(e);

			if (e.Button == MouseButtons.Right)
			{
				dragging = true;
				dragstartmappos = mousemappos;

				oldpositions = new List<Vector2D>();

				foreach(SlopeVertex sl in GetSelectedSlopeVertices())
					if(sl.Selected)
						oldpositions.Add(sl.Pos);


				if(highlightedslope != null)
					oldpositions.Add(highlightedslope.Pos);
			}
		}

		// Mouse wants to drag
		protected override void OnDragStop(MouseEventArgs e)
		{
			base.OnDragStop(e);

			General.Map.UndoRedo.CreateUndo("Drag slope vertex");

			BuilderPlug.Me.StoreSlopeVertexGroupsInSector();
			General.Map.Map.Update();

			BuilderPlug.Me.UpdateSlopes();

			dragging = false;
		}


		// This is called wheh selection ends
		protected override void OnEndMultiSelection()
		{
			bool selectionvolume = ((Math.Abs(base.selectionrect.Width) > 0.1f) && (Math.Abs(base.selectionrect.Height) > 0.1f));

			if(BuilderPlug.Me.AutoClearSelection && !selectionvolume)
				General.Map.Map.ClearSelectedThings();

			if(selectionvolume)
			{
				if(General.Interface.ShiftState ^ BuilderPlug.Me.AdditiveSelect)
				{
					// Go for all slope vertices
					foreach (SlopeVertex sl in GetAllSlopeVertices())
					{
						sl.Selected |= ((sl.Pos.x >= selectionrect.Left) &&
										(sl.Pos.y >= selectionrect.Top) &&
										(sl.Pos.x <= selectionrect.Right) &&
										(sl.Pos.y <= selectionrect.Bottom));
					}
				}
				else
				{
					// Go for all slope vertices
					foreach (SlopeVertex sl in GetAllSlopeVertices())
					{
						sl.Selected |= ((sl.Pos.x >= selectionrect.Left) &&
										(sl.Pos.y >= selectionrect.Top) &&
										(sl.Pos.x <= selectionrect.Right) &&
										(sl.Pos.y <= selectionrect.Bottom));
					}
				}
			}
			
			base.OnEndMultiSelection();

			// Clear overlay
			if(renderer.StartOverlay(true)) renderer.Finish();

			// Redraw
			General.Interface.RedrawDisplay();
		}

		// This is called when the selection is updated
		protected override void OnUpdateMultiSelection()
		{
			base.OnUpdateMultiSelection();

			UpdateOverlay();
		}

		public override bool OnCopyBegin()
		{
			copyslopevertexgroups = new List<SlopeVertexGroup>();

			foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
			{
				bool copy = false;

				// Check if the current SVG has to be copied
				foreach (SlopeVertex sv in svg.Vertices)
				{
					if (sv.Selected)
					{
						copy = true;
						break;
					}
				}

				if (copy)
				{
					List<SlopeVertex> newsv = new List<SlopeVertex>();

					foreach (SlopeVertex sv in svg.Vertices)
						newsv.Add(new SlopeVertex(sv.Pos, sv.Z));

					// Use -1 for id, since a real id will be assigned when pasting
					copyslopevertexgroups.Add(new SlopeVertexGroup(-1, newsv, svg.Floor, svg.Ceiling));
				}
			}

			return true;
		}

		public override bool OnPasteBegin(PasteOptions options)
		{
			if (copyslopevertexgroups == null || copyslopevertexgroups.Count == 0)
				return false;

			// Unselect all slope vertices, so the pasted vertices can be selected
			foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
				svg.SelectVertices(false);

			float l = copyslopevertexgroups[0].Vertices[0].Pos.x;
			float r = copyslopevertexgroups[0].Vertices[0].Pos.x;
			float t = copyslopevertexgroups[0].Vertices[0].Pos.y;
			float b = copyslopevertexgroups[0].Vertices[0].Pos.y;

			// Find the outer dimensions of all SVGs to paste
			foreach (SlopeVertexGroup svg in copyslopevertexgroups)
			{
				foreach (SlopeVertex sv in svg.Vertices)
				{
					if (sv.Pos.x < l) l = sv.Pos.x;
					if (sv.Pos.x > r) r = sv.Pos.x;
					if (sv.Pos.y > t) t = sv.Pos.y;
					if (sv.Pos.y < b) b = sv.Pos.y;
				}
			}

			Vector2D center = new Vector2D(l + ((r - l) / 2), b + ((t - b) / 2));
			Vector2D diff = center - General.Map.Grid.SnappedToGrid(mousemappos);

			foreach (SlopeVertexGroup svg in copyslopevertexgroups)
			{
				int id;
				List<SlopeVertex> newsv = new List<SlopeVertex>();

				foreach (SlopeVertex sv in svg.Vertices)
				{
					newsv.Add(new SlopeVertex(new Vector2D(sv.Pos.x - diff.x, sv.Pos.y - diff.y), sv.Z));
				}

				SlopeVertexGroup newsvg = BuilderPlug.Me.AddSlopeVertexGroup(newsv, out id, svg.Floor, svg.Ceiling);
				newsvg.SelectVertices(true);
			}

			// Redraw the display, so that pasted SVGs are shown immediately
			General.Interface.RedrawDisplay();

			// Don't go into the standard process for pasting, so tell the core that
			// pasting should not proceed
			return false;
		}

		public List<SlopeVertex> GetSelectedSlopeVertices()
		{
			List<SlopeVertex> selected = new List<SlopeVertex>();

			foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
			{
				foreach (SlopeVertex sv in svg.Vertices)
				{
					if (sv.Selected)
						selected.Add(sv);
				}
			}

			return selected;
		}

		public List<SlopeVertex> GetAllSlopeVertices()
		{
			List<SlopeVertex> selected = new List<SlopeVertex>();

			foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
			{
				foreach (SlopeVertex sv in svg.Vertices)
				{
					selected.Add(sv);
				}
			}

			return selected;
		}

		public List<SlopeVertexGroup> GetSelectedSlopeVertexGroups()
		{
			List<SlopeVertexGroup> svgs = new List<SlopeVertexGroup>();

			foreach (SlopeVertex sv in GetSelectedSlopeVertices())
			{
				SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(sv);

				if (!svgs.Contains(svg))
					svgs.Add(svg);
			}

			return svgs;
		}

		#endregion

		#region ================== Actions

		[BeginAction("drawfloorslope")]
		public void DrawFloorSlope()
		{
			slopemode = PlaneType.Floor;

			BuilderPlug.Me.MenusForm.CeilingSlope.Checked = false;
			BuilderPlug.Me.MenusForm.FloorSlope.Checked = true;
			BuilderPlug.Me.MenusForm.FloorAndCeilingSlope.Checked = false;

			General.Interface.DisplayStatus(StatusType.Info, "Applying drawn slope to floor");
		}

		[BeginAction("drawceilingslope")]
		public void DrawCeilingSlope()
		{
			slopemode = PlaneType.Ceiling;

			BuilderPlug.Me.MenusForm.CeilingSlope.Checked = true;
			BuilderPlug.Me.MenusForm.FloorSlope.Checked = false;
			BuilderPlug.Me.MenusForm.FloorAndCeilingSlope.Checked = false;

			General.Interface.DisplayStatus(StatusType.Info, "Applying drawn slope to ceiling");
		}

		[BeginAction("drawfloorandceilingslope")]
		public void DrawFloorAndCeilingSlope()
		{
			slopemode = PlaneType.Floor | PlaneType.Ceiling;

			BuilderPlug.Me.MenusForm.CeilingSlope.Checked = false;
			BuilderPlug.Me.MenusForm.FloorSlope.Checked = false;
			BuilderPlug.Me.MenusForm.FloorAndCeilingSlope.Checked = true;

			General.Interface.DisplayStatus(StatusType.Info, "Applying drawn slope to floor and ceiling");
		}

		[BeginAction("threedflipslope")]
		public void FlipSlope()
		{
			if (highlightedslope == null)
				return;

			MessageBox.Show("Flipping temporarily removed");

			/*
			if (highlightedslope.IsOrigin)
			{
				origin = highlightedslope.ThreeDFloor.Slope.Origin + highlightedslope.ThreeDFloor.Slope.Direction;
				direction = highlightedslope.ThreeDFloor.Slope.Direction * (-1);
			}
			else 
			{
				origin = highlightedslope.ThreeDFloor.Slope.Origin + highlightedslope.ThreeDFloor.Slope.Direction;
				direction = highlightedslope.ThreeDFloor.Slope.Direction * (-1);
			}

			highlightedslope.ThreeDFloor.Slope.Origin = origin;
			highlightedslope.ThreeDFloor.Slope.Direction = direction;

			highlightedslope.ThreeDFloor.Rebuild = true;

			BuilderPlug.ProcessThreeDFloors(new List<ThreeDFloor> { highlightedslope.ThreeDFloor }, highlightedslope.ThreeDFloor.TaggedSectors);

			UpdateSlopeObjects();

			// Redraw
			General.Interface.RedrawDisplay();
			*/
		}

		// This clears the selection
		[BeginAction("clearselection", BaseAction = true)]
		public void ClearSelection()
		{
			int numselected = 0;
			// Clear selection
			foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
			{
				foreach (SlopeVertex sv in svg.Vertices)
				{
					if (sv.Selected)
					{
						sv.Selected = false;
						numselected++;
					}
					
				}
			}

			// Clear selected sectors when no SVGs are selected
			if (numselected == 0)
				General.Map.Map.ClearAllSelected();
			
			// Redraw
			updateOverlaySurfaces();
			UpdateOverlay();
			General.Interface.RedrawDisplay();
		}
		

		[BeginAction("deleteitem", BaseAction = true)]
		public void DeleteItem()
		{
			// Make list of selected things
			List<SlopeVertex> selected = new List<SlopeVertex>(GetSelectedSlopeVertices());

			if(highlightedslope != null)
			{
				selected.Add(highlightedslope);
			}
			
			// Anything to do?
			if(selected.Count > 0)
			{
				List<SlopeVertexGroup> groups = new List<SlopeVertexGroup>();

				General.Map.UndoRedo.CreateUndo("Delete slope");

				foreach (SlopeVertex sv in selected)
				{
					SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(sv);

					if (!groups.Contains(svg))
						groups.Add(svg);
				}

				foreach (SlopeVertexGroup svg in groups)
				{
					svg.RemoveFromSectors();

					BuilderPlug.Me.SlopeVertexGroups.Remove(svg);
				}				

				General.Map.IsChanged = true;

				// Invoke a new mousemove so that the highlighted item updates
				MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, (int)mousepos.x, (int)mousepos.y, 0);
				OnMouseMove(e);

				// Redraw screen
				General.Interface.RedrawDisplay();
			}
		}
		
		#endregion
	}
}
