
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Drawing;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Controls;
using System.Linq;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorHelper
{
	//[EditMode(DisplayName = "Stair Sector Builder",
	//          Volatile = true)]

	[EditMode(DisplayName = "3D Floor Editing Mode",
			  SwitchAction = "threedfloorhelpermode",		// Action name used to switch to this mode
			  ButtonImage = "ThreeDFloorIcon.png",	// Image resource name for the button
			  ButtonOrder = int.MinValue + 500,	// Position of the button (lower is more to the left)
			  ButtonGroup = "000_editing",
			  UseByDefault = true,
			  SafeStartMode = false,
              Volatile = false )]

	public sealed class ThreeDFloorHelperMode : ClassicMode
	{
		#region ================== Constants

		private const float LINE_THICKNESS = 0.6f;
		private const float CONTROLPOINT_SIZE = 10.0f;
		private const int INNER_SPLINE = 0;
		private const int OUTER_SPLINE = 1;

		#endregion

		#region ================== Variables


		#endregion

		#region ================== Structures

		private struct CatmullRomSplineData
		{
			public Line2D line;
			public List<Vector2D> controlpoints;
			public List<Vector2D> tangents;
		}

		private struct CatmullRomSplineGroup
		{
			public CatmullRomSplineData[] splines;
			public Linedef sourcelinedef;
		}

		private struct ControlpointPointer
		{
			public int crsg;
			public int crsd;
			public int cp;
		}

		private struct ConnectedLinedefs
		{
			public bool closed;
			public int sector;
			public List<Linedef> linedefs;
			public Linedef firstlinedef;
		}

        private struct StairInfo
        {
            public int floorheight;
            public int ceilingheight;
            public List<List<Vector2D>> sectors;
        }


		#endregion

		#region ================== Properties

		// Just keep the base mode button checked
		public override string EditModeButtonName { get { return General.Editing.PreviousStableMode.Name; } }

		#endregion

		#region ================== Constructor / Disposer

		#endregion

		#region ================== Methods

		#endregion

		#region ================== Events

		public override void OnHelp()
		{
			General.ShowHelp("e_curvelinedefs.html");
		}

		// Cancelled
		public override void OnCancel()
		{
			// Cancel base class
			base.OnCancel();

			// stairsectorbuilderform.Close();

			// Return to base mode
			General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
		}

		// Mode engages
		public override void OnEngage()
		{
			base.OnEngage();

			BuilderPlug.SelectedSectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));

			if (BuilderPlug.SelectedSectors.Count <= 0 && General.Editing.Mode.HighlightedObject is Sector)
				BuilderPlug.SelectedSectors.Add((Sector)General.Editing.Mode.HighlightedObject);

			if (BuilderPlug.SelectedSectors.Count <= 0)
			{
				// Show a warning in the status bar
				General.Interface.DisplayStatus(StatusType.Warning, "Please select a sector to edit its 3D floor(s)");
				return;
			}

			BuilderPlug.TaggedLines = BuilderPlug.GetTaggedLinedefs(BuilderPlug.SelectedSectors).OrderByDescending(o => o.Front.Sector.CeilHeight).ToList();
			BuilderPlug.ThreeDFloors = BuilderPlug.GetThreeDFloors(BuilderPlug.SelectedSectors);

			General.Interface.RedrawDisplay();

			renderer.SetPresentation(Presentation.Standard);

			BuilderPlug.TDFEW.Show((Form)General.Interface);
		}

		// Disenagaging
		public override void OnDisengage()
		{
			base.OnDisengage();

			// Sort of work around so that the DB2 window won't lose focus
			General.Interface.Focus();
		}

		// This applies the curves and returns to the base mode
		public override void OnAccept()
		{
			// Return to base mode
			General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
		}

		// Redrawing display
		public override void OnRedrawDisplay()
		{
			base.OnRedrawDisplay();

			

			renderer.RedrawSurface();

			// Render lines
			if (renderer.StartPlotter(true))
			{
				renderer.PlotLinedefSet(General.Map.Map.Linedefs);
				renderer.PlotVerticesSet(General.Map.Map.Vertices);
				renderer.Finish();
			}

			// Render things
			if (renderer.StartThings(true))
			{
				renderer.RenderThingSet(General.Map.Map.Things, 1.0f);
				renderer.Finish();
			}

			// Render overlay
			if (renderer.StartOverlay(true))
			{
				renderer.RenderRectangle(ThreeDFloor.controlsectorarea, 2, new PixelColor(255, 0, 0, 255), true);
				renderer.Finish();
			}

			renderer.Present();
		}

		protected override void OnSelectBegin()
		{
			base.OnSelectBegin();
		}

		// When selected button is released
		protected override void OnSelectEnd()
		{
			base.OnSelectEnd();

			// Redraw
			General.Map.Map.Update();
			General.Interface.RedrawDisplay();
		}

		// Mouse moves
		public override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
		}

		#endregion
	}
}
