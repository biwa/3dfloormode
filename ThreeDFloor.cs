#region ================== Copyright (c) 2014 Boris Iwanski

/*
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public struct SlopeInfo
	{
		private bool topsloped;
		private bool bottomsloped;
		private int topheight;
		private int bottomheight;
		Vector2D origin;
		Vector2D direction;

		public bool TopSloped { get { return topsloped; } set { topsloped = value; } }
		public bool BottomSloped { get { return bottomsloped; } set { bottomsloped = value; } }
		public int TopHeight { get { return topheight; } set { topheight = value; } }
		public int BottomHeight { get { return bottomheight; } set { bottomheight = value; } }
		public Vector2D Origin { get { return origin; } set { origin = value; } }
		public Vector2D Direction { get { return direction; } set { direction = value; } }
	}

	public class ThreeDFloor
	{
		private Sector sector;
		private List<Sector> taggedsectors;
		private string bordertexture;
		private string topflat;
		private string bottomflat;
		private int type;
		private int flags;
		private int alpha;
		private int topheight;
		private int bottomheight;
		private bool isnew;
		private SlopeInfo slope;

		public static Rectangle controlsectorarea = new Rectangle(-512, 512, 512, -512);

		public Sector Sector { get { return sector; } }
		public List<Sector> TaggedSectors { get { return taggedsectors; } set { taggedsectors = value; } }
		public string BorderTexture { get { return bordertexture; } set { bordertexture = value; } }
		public string TopFlat { get { return topflat; } set { topflat = value; } }
		public string BottomFlat { get { return bottomflat; } set { bottomflat = value; } }
		public int Type { get { return type; } set { type = value; } }
		public int Flags { get { return flags; } set { flags = value; } }
		public int Alpha { get { return alpha; } set { alpha = value; } }
		public int TopHeight { get { return topheight; } set { topheight = value; } }
		public int BottomHeight { get { return bottomheight; } set { bottomheight = value; } }
		public bool IsNew { get { return isnew; } set { isnew = value; } }
		public SlopeInfo Slope { get { return slope; } set { slope = value; } }
		
		public ThreeDFloor()
		{
			sector = null;
			taggedsectors = new List<Sector>();
			topflat = General.Settings.DefaultCeilingTexture;
			bottomflat = General.Settings.DefaultFloorTexture;
			topheight = General.Settings.DefaultCeilingHeight;
			bottomheight = General.Settings.DefaultFloorHeight;
			bordertexture = General.Settings.DefaultTexture;
			type = 1;
			flags = 0;
			
			slope = new SlopeInfo();
			alpha = 255;
		}

		public ThreeDFloor(Sector sector)
		{
			if (sector == null)
				throw new Exception("Sector can't be null");

			this.sector = sector;
			taggedsectors = new List<Sector>();
			topflat = sector.CeilTexture;
			bottomflat = sector.FloorTexture;
			topheight = sector.CeilHeight;
			bottomheight = sector.FloorHeight;

			foreach (Sidedef sd in sector.Sidedefs)
			{
				if (sd.Line.Action == 160)
				{
					bordertexture = sd.MiddleTexture;
					type = sd.Line.Args[1];
					flags = sd.Line.Args[2];
					alpha = sd.Line.Args[3];

					foreach (Sector s in General.Map.Map.GetSectorsByTag(sd.Line.Args[0]))
					{
						if(!taggedsectors.Contains(s))
							taggedsectors.Add(s);
					}
				}
			}

			slope.TopSloped = sector.Fields.GetValue("tdfh_slope_top", false);
			slope.BottomSloped = sector.Fields.GetValue("tdfh_slope_bottom", false);

			slope.Origin = new Vector2D(
				sector.Fields.GetValue("tdfh_slope_origin_x", 0),
				sector.Fields.GetValue("tdfh_slope_origin_y", 0)
			);

			slope.Direction = new Vector2D(
				sector.Fields.GetValue("tdfh_slope_direction_x", 0),
				sector.Fields.GetValue("tdfh_slope_direction_y", 0)
			);

			slope.TopHeight = sector.Fields.GetValue("tdfh_slope_top_height", 0);
			slope.BottomHeight = sector.Fields.GetValue("tdfh_slope_bottom_height", 0);
		}

		public void BindTag(int tag)
		{
			Linedef line = null;

			// try to find an line without an action
			foreach (Sidedef sd in sector.Sidedefs)
			{
				if (sd.Line.Action == 0 && line == null)
					line = sd.Line;

				// if a line of the control sector already has the tag
				// nothing has to be done
				if (sd.Line.Args[0] == tag)
				{
					return;
				}
			}

			// no lines without an action, so a line has to get split
			// find the longest line to split
			if (line == null)
			{
				line = sector.Sidedefs.First().Line;

				foreach (Sidedef sd in sector.Sidedefs)
				{
					if (sd.Line.Length > line.Length)
						line = sd.Line;
				}

				// Lines may not have a length of less than 1 after splitting
				if (line.Length / 2 < 1)
					throw new Exception("Can't split more lines in Sector " + line.Front.Sector.Index.ToString() + ".");

				Vertex v = General.Map.Map.CreateVertex(line.Line.GetCoordinatesAt(0.5f));
				v.SnapToAccuracy();

				line = line.Split(v);

				General.Map.Map.Update();
				General.Interface.RedrawDisplay();
			}

			line.Action = 160;
			line.Args[0] = tag;
			line.Args[1] = type;
			line.Args[2] = flags;
			line.Args[3] = alpha;
		}

		public void UpdateGeometry()
		{
			if (sector == null)
				throw new Exception("3D floor has no geometry");

			sector.CeilHeight = topheight;
			sector.FloorHeight = bottomheight;
			sector.SetCeilTexture(topflat);
			sector.SetFloorTexture(bottomflat);

			foreach (Sidedef sd in sector.Sidedefs)
			{
				sd.SetTextureMid(bordertexture);

				if (sd.Line.Action == 160)
				{					
					sd.Line.Args[1] = type;
					sd.Line.Args[2] = flags;
					sd.Line.Args[3] = alpha;
				}
			}
		}

		public bool CreateGeometry()
		{
			List<DrawnVertex> drawnvertices = new List<DrawnVertex>();
			List<Vertex> vertices = new List<Vertex>();
			Point p;
			Vector2D slopethingpos = new Vector2D(0, 0);

			if (slope.BottomSloped || slope.TopSloped)
			{
				List<Linedef> lds = new List<Linedef>();
				Linedef ld1 = null;
				Linedef ld2 = null;
				float length = 0;

				foreach (Sector s in taggedsectors)
				{
					foreach (Sidedef sd in s.Sidedefs)
					{
						if (!lds.Contains(sd.Line)) lds.Add(sd.Line);
					}
				}

				foreach (Linedef l1 in lds)
				{
					foreach (Linedef l2 in lds)
					{
						if (l1 == l2) continue;

						Vector2D v1 = l1.Line.GetCoordinatesAt(0.5f);
						Vector2D v2 = l2.Line.GetCoordinatesAt(0.5f);
						float l = new Line2D(v1, v2).GetLength();

						if (l > length)
						{
							length = l;
							ld1 = l1;
							ld2 = l2;
						}
					}
				}

				slope.Origin = ld1.Line.GetCoordinatesAt(0.5f);
				slope.Direction = ld2.Line.GetCoordinatesAt(0.5f) - slope.Origin;

				p = BuilderPlug.Me.ControlSectorArea.GetNewControlSectorPosition(slope.Origin, slope.Direction, out slopethingpos);
			}
			else
			{
				p = BuilderPlug.Me.ControlSectorArea.GetNewControlSectorPosition();
			}

			drawnvertices.Add(SectorVertex(p.X, p.Y));
			drawnvertices.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y));
			drawnvertices.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
			drawnvertices.Add(SectorVertex(p.X, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
			drawnvertices.Add(SectorVertex(p.X, p.Y));

			List<Sector> oldsectors = new List<Sector>(General.Map.Map.Sectors);

			Tools.DrawLines(drawnvertices);

			foreach (Sector s in General.Map.Map.Sectors)
			{
				// this is a new control sector
				if (!oldsectors.Contains(s))
				{
					s.FloorHeight = bottomheight;
					s.CeilHeight = topheight;
					s.SetFloorTexture(bottomflat);
					s.SetCeilTexture(topflat);

					foreach (Sidedef sd in s.Sidedefs)
					{
						sd.Line.Front.SetTextureMid(bordertexture);
					}

					if(!s.Fields.ContainsKey("tdfh_managed"))
						s.Fields.Add("tdfh_managed", new UniValue(UniversalType.Boolean, true));

					sector = s;
				}
			}

			if (slope.BottomSloped || slope.TopSloped)
			{
				Line2D l = new Line2D(slope.Origin, slope.Origin + slope.Direction);
				int az = (int)Angle2D.RadToDeg(new Line2D(0.0f, sector.CeilHeight, l.GetLength(), slope.TopHeight).GetAngle());
				int axy = Angle2D.RealToDoom(l.GetAngle());
				Thing t;

				MessageBox.Show(Angle2D.RadToDeg(new Line2D(0.0f, sector.CeilHeight, l.GetLength(), slope.TopHeight).GetAngle()).ToString());

				// Ceiling slope
				t = General.Map.Map.CreateThing();
				General.Settings.ApplyDefaultThingSettings(t);
				t.Move(slopethingpos);
				t.Rotate(axy);
				t.Type = 9503;
				t.Args[0] = az;

				// Floor slope
				t = General.Map.Map.CreateThing();
				General.Settings.ApplyDefaultThingSettings(t);
				t.Move(slopethingpos);
				t.Rotate(axy);
				t.Type = 9502;
				t.Args[0] = az;

				t.UpdateConfiguration();

				General.Map.ThingsFilter.Update();

				if(slope.TopSloped)
					sector.Fields.Add("tdfh_slope_top", new UniValue(UniversalType.Boolean, true));

				if (slope.BottomSloped)
					sector.Fields.Add("tdfh_slope_bottom", new UniValue(UniversalType.Boolean, true));

				sector.Fields.Add("tdfh_slope_origin_x", new UniValue(UniversalType.Integer, (int)slope.Origin.x));
				sector.Fields.Add("tdfh_slope_origin_y", new UniValue(UniversalType.Integer, (int)slope.Origin.y));
				sector.Fields.Add("tdfh_slope_direction_x", new UniValue(UniversalType.Integer, (int)slope.Direction.x));
				sector.Fields.Add("tdfh_slope_direction_y", new UniValue(UniversalType.Integer, (int)slope.Direction.y));
				sector.Fields.Add("tdfh_slope_top_height", new UniValue(UniversalType.Integer, (int)slope.TopHeight));
				sector.Fields.Add("tdfh_slope_bottom_height", new UniValue(UniversalType.Integer, (int)slope.BottomHeight));
			}


			// Snap to map format accuracy
			General.Map.Map.SnapAllToAccuracy();

			General.Map.Map.BeginAddRemove();
			//MapSet.JoinVertices(vertices, vertices, false, MapSet.STITCH_DISTANCE);
			General.Map.Map.EndAddRemove();

			// Update textures
			General.Map.Data.UpdateUsedTextures();

			// Update caches
			General.Map.Map.Update();
			General.Interface.RedrawDisplay();
			General.Map.IsChanged = true;

			return true;
		}

		// Turns a position into a DrawnVertex and returns it
		private DrawnVertex SectorVertex(float x, float y)
		{
			DrawnVertex v = new DrawnVertex();

			v.stitch = true;
			v.stitchline = true;
			v.pos = new Vector2D((float)Math.Round(x, General.Map.FormatInterface.VertexDecimals), (float)Math.Round(y, General.Map.FormatInterface.VertexDecimals));

			return v;
		}

		private DrawnVertex SectorVertex(Vector2D v)
		{
			return SectorVertex(v.x, v.y);
		}

		public void Cleanup()
		{
			int taggedLines = 0;

			foreach (Sidedef sd in sector.Sidedefs)
			{
				if (sd.Line.Action == 160 && General.Map.Map.GetSectorsByTag(sd.Line.Args[0]).Count == 0)
				{
					sd.Line.Action = 0;

					for (int i = 0; i < 5; i++)
						sd.Line.Args[i] = 0;
				}

				if (sd.Line.Action != 0)
					taggedLines++;
			}

			if (taggedLines == 0)
			{
				DeleteSector(sector);
			}
		}

		private void DeleteSector(Sector sector)
		{
			if (sector == null)
				return;

			General.Map.Map.BeginAddRemove(); //mxd

			//mxd. Get all the linedefs
			List<Linedef> lines = new List<Linedef>(sector.Sidedefs.Count);
			foreach (Sidedef side in sector.Sidedefs) lines.Add(side.Line);

			// Dispose the sector
			sector.Dispose();

			// Check all the lines
			for (int i = lines.Count - 1; i >= 0; i--)
			{
				// If the line has become orphaned, remove it
				if ((lines[i].Front == null) && (lines[i].Back == null))
				{
					// Remove line
					lines[i].Dispose();
				}
				else
				{
					// If the line only has a back side left, flip the line and sides
					if ((lines[i].Front == null) && (lines[i].Back != null))
					{
						lines[i].FlipVertices();
						lines[i].FlipSidedefs();
					}

					//mxd. Check textures.
					if (lines[i].Front.MiddleRequired() && (lines[i].Front.MiddleTexture.Length == 0 || lines[i].Front.MiddleTexture == "-"))
					{
						if (lines[i].Front.HighTexture.Length > 0 && lines[i].Front.HighTexture != "-")
						{
							lines[i].Front.SetTextureMid(lines[i].Front.HighTexture);
						}
						else if (lines[i].Front.LowTexture.Length > 0 && lines[i].Front.LowTexture != "-")
						{
							lines[i].Front.SetTextureMid(lines[i].Front.LowTexture);
						}
					}

					//mxd. Do we still need high/low textures?
					lines[i].Front.RemoveUnneededTextures(false);

					// Update sided flags
					lines[i].ApplySidedFlags();
				}
			}

			General.Map.Map.EndAddRemove(); //mxd

			// Update cache values
			General.Map.IsChanged = true;
			General.Map.Map.Update();

		}
	}
}
