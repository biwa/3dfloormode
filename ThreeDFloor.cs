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

namespace CodeImp.DoomBuilder.ThreeDFloorHelper
{
	public class ThreeDFloor
	{
		private Sector sector;
		private List<Sector> taggedSectors;
		private string borderTexture;
		private string topFlat;
		private string bottomFlat;
		private int type;
		private int flags;
		private int alpha;
		private int topHeight;
		private int bottomHeight;
		private bool isNew;

		public static Rectangle controlsectorarea = new Rectangle(-512, 512, 512, -512);

		public Sector Sector { get { return sector; } }
		public List<Sector> TaggedSectors { get { return taggedSectors; } set { taggedSectors = value; } }
		public string BorderTexture { get { return borderTexture; } set { borderTexture = value; } }
		public string TopFlat { get { return topFlat; } set { topFlat = value; } }
		public string BottomFlat { get { return bottomFlat; } set { bottomFlat = value; } }
		public int Type { get { return type; } set { type = value; } }
		public int Flags { get { return flags; } set { flags = value; } }
		public int Alpha { get { return alpha; } set { alpha = value; } }
		public int TopHeight { get { return topHeight; } set { topHeight = value; } }
		public int BottomHeight { get { return bottomHeight; } set { bottomHeight = value; } }
		public bool IsNew { get { return isNew; } set { isNew = value; } }


		public ThreeDFloor()
		{
			sector = null;
			taggedSectors = new List<Sector>();
			topFlat = General.Settings.DefaultCeilingTexture;
			bottomFlat = General.Settings.DefaultFloorTexture;
			topHeight = General.Settings.DefaultCeilingHeight;
			bottomHeight = General.Settings.DefaultFloorHeight;
			borderTexture = General.Settings.DefaultTexture;
			type = 1;
			flags = 0;
			alpha = 255;
		}

		public ThreeDFloor(Sector sector)
		{
			if (sector == null)
				throw new Exception("Sector can't be null");

			this.sector = sector;
			taggedSectors = new List<Sector>();
			topFlat = sector.CeilTexture;
			bottomFlat = sector.FloorTexture;
			topHeight = sector.CeilHeight;
			bottomHeight = sector.FloorHeight;

			foreach (Sidedef sd in sector.Sidedefs)
			{
				if (sd.Line.Action == 160)
				{
					borderTexture = sd.MiddleTexture;
					type = sd.Line.Args[1];
					flags = sd.Line.Args[2];
					alpha = sd.Line.Args[3];

					foreach (Sector s in General.Map.Map.GetSectorsByTag(sd.Line.Args[0]))
					{
						if(!taggedSectors.Contains(s))
							taggedSectors.Add(s);
					}
				}
			}
		}

		public void BindTag(int tag)
		{
			Linedef line = null;
			bool isBound = false;

			// try to find an line without an action
			foreach (Sidedef sd in sector.Sidedefs)
			{
				if (sd.Line.Action == 0 && line == null)
					line = sd.Line;

				// if a line of the control sector already has the tag
				// nothing has to be done
				if (sd.Line.Args[0] == tag)
				{
					isBound = true;
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

			sector.CeilHeight = topHeight;
			sector.FloorHeight = bottomHeight;
			sector.SetCeilTexture(topFlat);
			sector.SetFloorTexture(bottomFlat);

			foreach (Sidedef sd in sector.Sidedefs)
			{
				sd.SetTextureMid(borderTexture);

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

			try
			{
				p = BuilderPlug.Me.ControlSectorArea.GetNewControlSectorPosition();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
				return false;
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
					s.FloorHeight = bottomHeight;
					s.CeilHeight = topHeight;
					s.SetFloorTexture(bottomFlat);
					s.SetCeilTexture(topFlat);

					foreach (Sidedef sd in s.Sidedefs)
					{
						sd.Line.Front.SetTextureMid(borderTexture);
					}

					s.Fields.Add("tdfh_managed", new UniValue(UniversalType.Boolean, true));

					sector = s;
				}
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
				// delete geometry here
			}
		}
	}
}
