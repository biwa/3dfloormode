
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
              ButtonImage = "ThreeDFloorIcon.png",	// Image resource name for the button
              ButtonOrder = int.MinValue + 501,	// Position of the button (lower is more to the left)
              ButtonGroup = "000_editing",
              UseByDefault = true,
              SafeStartMode = true)]

    public class ThreeDSlopeMode : ClassicMode
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Highlighted item
		private Thing highlighted;
        private SlopeObject highlightedslope;
		private int[] hightlightedslopepoint = new int[] { -1, -1 };
		private Association[] association = new Association[Thing.NUM_ARGS];
		private Association highlightasso = new Association();

		// Interface
		private bool editpressed;
		private bool thinginserted;

        private List<ThreeDFloor> threedfloors;
        private List<SlopeObject> slopeobjects;
		bool dragging = false;

		private List<TextLabel> labels;
		private FlatVertex[] overlayGeometry;

		private Vector2D dragstartmappos;
		private List<Vector2D> oldpositions;
		
		#endregion

		#region ================== Properties

		public override object HighlightedObject { get { return highlighted; } }

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

            // Convert geometry selection to linedefs selection
            General.Map.Map.ConvertSelection(SelectionType.Linedefs);
            General.Map.Map.SelectionType = SelectionType.Things;

            // Get all 3D floors in the map
            threedfloors = BuilderPlug.GetThreeDFloors(General.Map.Map.Sectors.ToList());

			SetupLabels();

			UpdateSlopeObjects();
		}

		// Mode disengages
		public override void OnDisengage()
		{
			base.OnDisengage();
			
			// Going to EditSelectionMode?
			if(General.Editing.NewMode is EditSelectionMode)
			{
				// Not pasting anything?
				EditSelectionMode editmode = (General.Editing.NewMode as EditSelectionMode);
				if(!editmode.Pasting)
				{
					// No selection made? But we have a highlight!
					if((General.Map.Map.GetSelectedThings(true).Count == 0) && (highlighted != null))
					{
						// Make the highlight the selection
						highlighted.Selected = true;
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
			if(renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);

				if (highlightedslope != null)
				{
					foreach(Sector s in highlightedslope.ThreeDFloor.TaggedSectors)
						renderer.PlotSector(s, General.Colors.Highlight);
				}

				//for(int i = 0; i < Thing.NUM_ARGS; i++) BuilderPlug.Me.PlotAssociations(renderer, association[i]);
				//if((highlighted != null) && !highlighted.IsDisposed) BuilderPlug.Me.PlotReverseAssociations(renderer, highlightasso);
				renderer.Finish();
			}

			// Render things
			if(renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.ThingsFilter.HiddenThings, Presentation.THINGS_HIDDEN_ALPHA);
				renderer.RenderThingSet(General.Map.ThingsFilter.VisibleThings, 1.0f);
				//for(int i = 0; i < Thing.NUM_ARGS; i++) BuilderPlug.Me.RenderAssociations(renderer, association[i]);
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					//BuilderPlug.Me.RenderReverseAssociations(renderer, highlightasso);
					renderer.RenderThing(highlighted, General.Colors.Highlight, 1.0f);
				}
				renderer.Finish();
			}

			// Selecting?
			if(selecting)
			{
				// Render selection
				if(renderer.StartOverlay(true))
				{
					RenderMultiSelection();
					renderer.Finish();
				}
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

			foreach (KeyValuePair<int, List<SlopeVertex>> kvp in BuilderPlug.Me.SlopeVertices)
			{
				for(int i=0; i < kvp.Value.Count; i++) {
					float x = kvp.Value[i].Pos.x;
					float y = kvp.Value[i].Pos.y - 14f * (1 / renderer.Scale);

					SlopeVertex sv = kvp.Value[i];

					/*
					if (i == 0)
					{
						Line2D line = new Line2D(kvp.Value[0].pos, kvp.Value[1].pos);

						Vector2D v = -line.GetDelta().GetNormal();
						v *= 14 * (1 / renderer.Scale);

						x = v.x + kvp.Value[0].pos.x;
						y = v.y + kvp.Value[0].pos.y;
					}
					if (i == 1)
					{
						Line2D line = new Line2D(kvp.Value[0].pos, kvp.Value[1].pos);

						Vector2D v = -line.GetDelta().GetNormal();
						v *= -14 * (1 / renderer.Scale);

						x = v.x + kvp.Value[1].pos.x;
						y = v.y + kvp.Value[1].pos.y;
					}
					*/

					TextLabel label = new TextLabel(20);
					label.TransformCoords = true;
					label.Rectangle = new RectangleF(x, y, 0.0f, 0.0f);
					label.AlignX = TextAlignmentX.Center;
					label.AlignY = TextAlignmentY.Middle;
					label.Scale = 14f;
					label.Color = General.Colors.Highlight.WithAlpha(255);
					label.Backcolor = General.Colors.Background.WithAlpha(255);
					label.Text = "";

					if (sv.Ceiling)
					{
						label.Text += String.Format("C: {0}", sv.CeilingZ);

						if (sv.Floor)
							label.Text += "; ";
					}

					if (sv.Floor)
						label.Text += String.Format("F: {0}", sv.FloorZ);

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
				if(overlayGeometry != null)
					renderer.RenderHighlight(overlayGeometry, General.Colors.Selection.WithAlpha(64).ToInt());

				foreach (KeyValuePair<int, List<SlopeVertex>> kvp in BuilderPlug.Me.SlopeVertices)
				{
					for (int i = 1; i < kvp.Value.Count; i++)
					{
						renderer.RenderLine(kvp.Value[0].Pos, kvp.Value[i].Pos, 1, new PixelColor(255, 255, 255, 255), true);
					}

					for (int i = 0; i < kvp.Value.Count; i++)
					{
						PixelColor c = General.Colors.Indication;
						Vector3D v = kvp.Value[i].Pos;

						if (kvp.Key == hightlightedslopepoint[0] && i == hightlightedslopepoint[1])
							c = General.Colors.Highlight;
						else if (kvp.Value[i].Selected)
							c = General.Colors.Selection;

						renderer.RenderRectangleFilled(new RectangleF(v.x - size / 2, v.y - size / 2, size, size), General.Colors.Background, true);
						renderer.RenderRectangle(new RectangleF(v.x - size / 2, v.y - size / 2, size, size), 2, c, true);
					}
				}

				foreach (TextLabel l in labels)
					renderer.RenderText(l);

                renderer.Finish();
            }           
        }

		private void updateOverlaySurfaces()
		{
			int s = hightlightedslopepoint[0];
			string[] fieldnames = new string[] { "floorplane_id", "ceilingplane_id" };
			ICollection<Sector> orderedselection = General.Map.Map.GetSelectedSectors(true);
			List<FlatVertex> vertsList = new List<FlatVertex>();

			if (s != -1)
			{
				foreach (Sector sector in General.Map.Map.Sectors)
				{
					foreach (string fn in fieldnames)
					{
						int id = sector.Fields.GetValue(fn, -1);

						if (id == -1 || id != s) continue;

						// Go for all selected sectors
						vertsList.AddRange(sector.FlatVertices);
					}
				}
			}

			overlayGeometry = vertsList.ToArray();
		}
		
		// This highlights a new item
		protected void Highlight(Thing t)
		{
			bool completeredraw = false;
			LinedefActionInfo action = null;

			// Often we can get away by simply undrawing the previous
			// highlight and drawing the new highlight. But if associations
			// are or were drawn we need to redraw the entire display.

			// Previous association highlights something?
			if((highlighted != null) && (highlighted.Tag > 0)) completeredraw = true;
			
			// Set highlight association
			if(t != null)
				highlightasso.Set(t.Position, t.Tag, UniversalType.ThingTag);
			else
                highlightasso.Set(new Vector2D(), 0, 0);

			// New association highlights something?
			if((t != null) && (t.Tag > 0)) completeredraw = true;

			if(t != null)
			{
				// Check if we can find the linedefs action
				if((t.Action > 0) && General.Map.Config.LinedefActions.ContainsKey(t.Action))
					action = General.Map.Config.LinedefActions[t.Action];
			}
			
			// Determine linedef associations
			for(int i = 0; i < Thing.NUM_ARGS; i++)
			{
				// Previous association highlights something?
				if((association[i].type == UniversalType.SectorTag) ||
				   (association[i].type == UniversalType.LinedefTag) ||
				   (association[i].type == UniversalType.ThingTag)) completeredraw = true;
				
				// Make new association
				if(action != null)
					association[i].Set(t.Position, t.Args[i], action.Args[i].Type);
				else
					association[i].Set(new Vector2D(), 0, 0);
				
				// New association highlights something?
				if((association[i].type == UniversalType.SectorTag) ||
				   (association[i].type == UniversalType.LinedefTag) ||
				   (association[i].type == UniversalType.ThingTag)) completeredraw = true;
			}
			
			// If we're changing associations, then we
			// need to redraw the entire display
			if(completeredraw)
			{
				// Set new highlight and redraw completely
				highlighted = t;
				General.Interface.RedrawDisplay();
			}
			else
			{
				// Update display
				if(renderer.StartThings(false))
				{
					// Undraw previous highlight
					if((highlighted != null) && !highlighted.IsDisposed)
						renderer.RenderThing(highlighted, renderer.DetermineThingColor(highlighted), 1.0f);

					// Set new highlight
					highlighted = t;

					// Render highlighted item
					if((highlighted != null) && !highlighted.IsDisposed)
						renderer.RenderThing(highlighted, General.Colors.Highlight, 1.0f);

					// Done
					renderer.Finish();
					renderer.Present();
				}
			}
			
			// Show highlight info
			if((highlighted != null) && !highlighted.IsDisposed)
				General.Interface.ShowThingInfo(highlighted);
			else
				General.Interface.HideInfo();
		}

		// Selection
		protected override void OnSelectBegin()
		{
			// Item highlighted?
			if((highlighted != null) && !highlighted.IsDisposed)
			{
				// Flip selection
				highlighted.Selected = !highlighted.Selected;

				// Update display
				if(renderer.StartThings(false))
				{
					// Redraw highlight to show selection
					renderer.RenderThing(highlighted, renderer.DetermineThingColor(highlighted), 1.0f);
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
			// Not ending from a multi-selection?
			if(!selecting)
			{
				// Item highlighted?
				if((highlighted != null) && !highlighted.IsDisposed)
				{
					// Update display
					if(renderer.StartThings(false))
					{
						// Render highlighted item
						renderer.RenderThing(highlighted, General.Colors.Highlight, 1.0f);
						renderer.Finish();
						renderer.Present();
					}
				}
			}

			base.OnSelectEnd();
		}

		// Start editing
		protected override void OnEditBegin()
		{
			base.OnEditBegin();
		}

		// Done editing
		protected override void OnEditEnd()
		{
			base.OnEditEnd();

			if (dragging) return;

			int s = hightlightedslopepoint[0];
			int p = hightlightedslopepoint[1];

			if (s == -1) return;

			SlopeVertexEditForm svef = new SlopeVertexEditForm(BuilderPlug.Me.SlopeVertices[s][p]);

			DialogResult result = svef.ShowDialog((Form)General.Interface);

			if (result == DialogResult.OK)
			{
				SlopeVertex old = BuilderPlug.Me.SlopeVertices[s][p];
				float floorz = svef.floorz.GetResultFloat(old.FloorZ);
				float ceilingz = svef.ceilingz.GetResultFloat(old.CeilingZ);
				float x = svef.positionx.GetResultFloat(old.Pos.x);
				float y = svef.positiony.GetResultFloat(old.Pos.y);

				BuilderPlug.Me.SlopeVertices[s][p] = new SlopeVertex(new Vector2D(x, y), old.Floor, floorz, old.Ceiling, ceilingz);

				BuilderPlug.Me.UpdateSlopes();
			}
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			// Not holding any buttons?
			if(e.Button == MouseButtons.None)
			{
				// Find the nearest thing within highlight range
				Thing t = MapSet.NearestThingSquareRange(General.Map.ThingsFilter.VisibleThings, mousemappos, BuilderPlug.Me.HighlightSlopeRange / renderer.Scale);

				// Highlight if not the same
				if(t != highlighted) Highlight(t);

                float distance = float.MaxValue;
                float d;

				SlopeObject hso = null;

				int s = -1;
				int p = -1;

				foreach (KeyValuePair<int, List<SlopeVertex>> kvp in BuilderPlug.Me.SlopeVertices)
				{
					for (int i = 0; i < kvp.Value.Count; i++)
					{
						d = Vector2D.Distance(kvp.Value[i].Pos, mousemappos);

						if (d <= BuilderModes.BuilderPlug.Me.HighlightRange / renderer.Scale && d < distance)
						{
							distance = d;
							s = kvp.Key;
							p = i;
						}
					}
				}

				if (s != hightlightedslopepoint[0] && p != hightlightedslopepoint[1])
				{
					hightlightedslopepoint[0] = s;
					hightlightedslopepoint[1] = p;

					UpdateOverlay();
					updateOverlaySurfaces();
					General.Interface.RedrawDisplay();
				}

                foreach (SlopeObject so in slopeobjects)
                {
                    d = Vector2D.Distance(so.Position, mousemappos);

                    if (d <= BuilderModes.BuilderPlug.Me.HighlightRange / renderer.Scale && d < distance)
                    {
                        distance = d;
                        hso = so;
                    }
                }

				if (hso != highlightedslope)
				{
					highlightedslope = hso;
					UpdateOverlay();
					General.Interface.RedrawDisplay();
				}
			}
			else if (dragging && hightlightedslopepoint[0] != -1)
			{
				int s = hightlightedslopepoint[0];
				int p = hightlightedslopepoint[1];
				int i = 0;

				Vector2D newpos = GridSetup.SnappedToGrid(mousemappos, General.Map.Grid.GridSizeF, 1.0f / General.Map.Grid.GridSizeF);
				Vector2D snappedstartpos = GridSetup.SnappedToGrid(dragstartmappos, General.Map.Grid.GridSizeF, 1.0f / General.Map.Grid.GridSizeF);

				// BuilderPlug.Me.SlopeVertices[s][p] = new SlopeVertex(newpos, BuilderPlug.Me.SlopeVertices[s][p].Floor, BuilderPlug.Me.SlopeVertices[s][p].FloorZ, BuilderPlug.Me.SlopeVertices[s][p].Ceiling, BuilderPlug.Me.SlopeVertices[s][p].CeilingZ);

				foreach (SlopeVertex sl in GetSelectedSlopeVertices())
				{
					sl.Pos = oldpositions[i] + newpos - snappedstartpos;
					i++;
				}

				BuilderPlug.Me.SlopeVertices[s][p].Pos = oldpositions[i] + newpos - snappedstartpos;

				UpdateOverlay();
				updateOverlaySurfaces();
				General.Interface.RedrawDisplay();
			}
			else if (selecting)
			{
				//UpdateOverlay();
				//updateOverlaySurfaces();
				//RenderMultiSelection();
				//General.Interface.RedrawDisplay();
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

			if (e.Button == MouseButtons.Right)
			{
				dragging = true;
				dragstartmappos = mousemappos;

				oldpositions = new List<Vector2D>();

				foreach(SlopeVertex sl in GetSelectedSlopeVertices())
					if(sl.Selected)
						oldpositions.Add(sl.Pos);


				int s = hightlightedslopepoint[0];
				int p = hightlightedslopepoint[1];
				oldpositions.Add(BuilderPlug.Me.SlopeVertices[s][p].Pos);
			}
		}

		// Mouse wants to drag
		protected override void OnDragStop(MouseEventArgs e)
		{
			base.OnDragStop(e);

			BuilderPlug.Me.UpdateSlopes();

			if (highlightedslope != null)
			{
				if (highlightedslope.V == 1)
				{
					highlightedslope.ThreeDFloor.BottomSlope.V1 = highlightedslope.Position;
				}
				else if (highlightedslope.V == 2)
				{
					highlightedslope.ThreeDFloor.BottomSlope.V2 = highlightedslope.Position;
				}
				else if (highlightedslope.V == 3)
				{
					highlightedslope.ThreeDFloor.BottomSlope.V3 = highlightedslope.Position;
				}

				highlightedslope.ThreeDFloor.Rebuild = true;

				BuilderPlug.ProcessThreeDFloors(new List<ThreeDFloor> { highlightedslope.ThreeDFloor }, highlightedslope.ThreeDFloor.TaggedSectors);

				UpdateOverlay();
				General.Interface.RedrawDisplay();
			}

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
			if((General.Map.Map.GetSelectedThings(true).Count == 0) && (highlighted != null))
			{
				// Make the highlight the selection
				highlighted.Selected = true;
			}

			return base.OnCopyBegin();
		}

		public List<SlopeVertex> GetSelectedSlopeVertices()
		{
			List<SlopeVertex> selected = new List<SlopeVertex>();

			foreach (KeyValuePair<int, List<SlopeVertex>> kvp in BuilderPlug.Me.SlopeVertices)
			{
				for (int i = 0; i < kvp.Value.Count; i++)
				{
					if (kvp.Value[i].Selected)
						selected.Add(kvp.Value[i]);
				}
			}

			return selected;
		}

		public List<SlopeVertex> GetAllSlopeVertices()
		{
			List<SlopeVertex> selected = new List<SlopeVertex>();

			foreach (KeyValuePair<int, List<SlopeVertex>> kvp in BuilderPlug.Me.SlopeVertices)
			{
				for (int i = 0; i < kvp.Value.Count; i++)
				{
					selected.Add(kvp.Value[i]);
				}
			}

			return selected;
		}

		#endregion

		#region ================== Actions

		[BeginAction("threedflipslope")]
		public void FlipSlope()
		{
			if (highlightedslope == null)
				return;

			Vector2D origin;
			Vector2D direction;

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
			// Clear selection
			foreach (KeyValuePair<int, List<SlopeVertex>> kvp in BuilderPlug.Me.SlopeVertices)
			{
				for (int i = 0; i < kvp.Value.Count; i++)
				{
					kvp.Value[i].Selected = false;
				}
			}

			// Redraw
			General.Interface.RedrawDisplay();
		}
		
		// This creates a new thing
		private Thing InsertThing(Vector2D pos)
		{
			if (pos.x < General.Map.Config.LeftBoundary || pos.x > General.Map.Config.RightBoundary ||
				pos.y > General.Map.Config.TopBoundary || pos.y < General.Map.Config.BottomBoundary)
			{
				General.Interface.DisplayStatus(StatusType.Warning, "Failed to insert thing: outside of map boundaries.");
				return null;
			}

			// Create thing
			Thing t = General.Map.Map.CreateThing();
			if(t != null)
			{
				General.Settings.ApplyDefaultThingSettings(t);
				
				t.Move(pos);
				
				t.UpdateConfiguration();

				// Update things filter so that it includes this thing
				General.Map.ThingsFilter.Update();

				// Snap to grid enabled?
				if(General.Interface.SnapToGrid)
				{
					// Snap to grid
					t.SnapToGrid();
				}
				else
				{
					// Snap to map format accuracy
					t.SnapToAccuracy();
				}
			}
			
			return t;
		}

		[BeginAction("deleteitem", BaseAction = true)]
		public void DeleteItem()
		{
			// Make list of selected things
			List<Thing> selected = new List<Thing>(General.Map.Map.GetSelectedThings(true));
			if((selected.Count == 0) && (highlighted != null) && !highlighted.IsDisposed) selected.Add(highlighted);
			
			// Anything to do?
			if(selected.Count > 0)
			{
				// Make undo
				if(selected.Count > 1)
				{
					General.Map.UndoRedo.CreateUndo("Delete " + selected.Count + " things");
					General.Interface.DisplayStatus(StatusType.Action, "Deleted " + selected.Count + " things.");
				}
				else
				{
					General.Map.UndoRedo.CreateUndo("Delete thing");
					General.Interface.DisplayStatus(StatusType.Action, "Deleted a thing.");
				}

				// Dispose selected things
				foreach(Thing t in selected) t.Dispose();
				
				// Update cache values
				General.Map.IsChanged = true;
				General.Map.ThingsFilter.Update();

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
