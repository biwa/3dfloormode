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
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.ThreeDFloorHelper
{
	public class ControlSectorArea
	{

		#region ================== Enums

		public enum Highlight
		{
			None,
			OuterLeft,
			OuterRight,
			OuterTop,
			OuterBottom,
			InnerLeft,
			InnerRight,
			InnerTop,
			InnerBottom,
			Body
		};

		#endregion

		#region ================== Variables

		private RectangleF outerborder;
		private RectangleF innerborder;
		private PixelColor bordercolor = new PixelColor(255, 0, 192, 0);
		private PixelColor fillcolor = new PixelColor(128, 0, 128, 0);
		private PixelColor borderhighlightcolor = new PixelColor(255, 0, 192, 0);
		private PixelColor fillhighlightcolor = new PixelColor(128, 0, 192, 0);
		private Dictionary<Highlight, Line2D> lines;
		private float gridsize;
		private float gridsizeinv;
		private float sectorsize;

		private float outerleft;
		private float outerright;
		private float outertop;
		private float outerbottom;
		private float innerleft;
		private float innerright;
		private float innertop;
		private float innerbottom;

		private bool usecustomtagrange;
		private int firsttag;
		private int lasttag;

		private static BlockMap<BlockEntry> blockmap;

		#endregion

		#region ================== Properties

		public float GridSize { get { return gridsize; } }
		public float SectorSize { get { return sectorsize; } }

		public RectangleF OuterBorder { get { return outerborder; } }
		public RectangleF InnerBorder { get { return innerborder; } }

		public float OuterLeft
		{
			get { return outerleft; }
			set { outerleft = value; UpdateLines();	}
		}

		public float OuterRight
		{
			get { return outerright; }
			set { outerright = value; UpdateLines(); }
		}

		public float OuterTop
		{
			get { return outertop; }
			set { outertop = value; UpdateLines(); }
		}

		public float OuterBottom
		{
			get { return outerbottom; }
			set { outerbottom = value; UpdateLines(); }
		}

		public float InnerLeft
		{
			get { return innerleft; }
			set { innerleft = value; UpdateLines(); }
		}

		public float InnerRight
		{
			get { return innerright; }
			set { innerright = value; UpdateLines(); }
		}

		public float InnerTop
		{
			get { return innertop; }
			set { innertop = value; UpdateLines(); }
		}

		public float InnerBottom
		{
			get { return innerbottom; }
			set { innerbottom = value; UpdateLines(); }
		}

		public bool UseCustomTagRnage { get { return usecustomtagrange; } set { usecustomtagrange = value; } }
		public int FirstTag { get { return firsttag; } set { firsttag = value; } }
		public int LastTag { get { return lasttag; } set { lasttag = value; } }

		#endregion

		#region ================== Constructor / Disposer

		public ControlSectorArea(float outerleft, float outerright, float outertop, float outerbottom, float innerleft, float innerright, float innertop, float innerbottom, float gridsize, float sectorsize)
		{
			this.outerleft = outerleft;
			this.outerright = outerright;
			this.outertop = outertop;
			this.outerbottom = outerbottom;
			this.innerleft = innerleft;
			this.innerright = innerright;
			this.innertop = innertop;
			this.innerbottom = innerbottom;

			lines = new Dictionary<Highlight, Line2D>();

			this.gridsize = gridsize;
			gridsizeinv = 1.0f / gridsize;

			this.sectorsize = sectorsize;

			UpdateLines();
		}

		#endregion

		#region ================== Methods

		public void UpdateLines()
		{
			lines[Highlight.OuterLeft] = new Line2D(outerleft, outertop, outerleft, outerbottom);
			lines[Highlight.OuterRight] = new Line2D(outerright, outertop, outerright, outerbottom);
			lines[Highlight.OuterTop] = new Line2D(outerleft, outertop, outerright, outertop);
			lines[Highlight.OuterBottom] = new Line2D(outerleft, outerbottom, outerright, outerbottom);

			lines[Highlight.InnerLeft] = new Line2D(innerleft, innertop, innerleft, innerbottom);
			lines[Highlight.InnerRight] = new Line2D(innerright, innertop, innerright, innerbottom);
			lines[Highlight.InnerTop] = new Line2D(innerleft, innertop, innerright, innertop);
			lines[Highlight.InnerBottom] = new Line2D(innerleft, innerbottom, innerright, innerbottom);

			outerborder = new RectangleF(outerleft, outertop, outerright - outerleft, outerbottom - outertop);
			innerborder = new RectangleF(innerleft, innertop, innerright - innerleft, innerbottom - innertop);
		}

		public void Draw(IRenderer2D renderer, Highlight highlight)
		{
			PixelColor fcolor = highlight == Highlight.Body ? fillhighlightcolor : fillcolor;

			// The area is drawn in 4 steps as shown below (e stands for an empty space/hole)
			//
			// |-----------|
			// |   | 3 |   |
			// |   |---|   |
			// | 1 | e | 2 |
			// |   |---|   |
			// |   | 4 |   |
			// |-----------|

			renderer.RenderRectangleFilled(
				new RectangleF(outerleft, outertop, innerleft - outerleft, outerbottom - outertop),
				fcolor,
				true
			);

			renderer.RenderRectangleFilled(
				new RectangleF(outerright, outertop, innerright - outerright, outerbottom - outertop),
				fcolor,
				true
			);

			renderer.RenderRectangleFilled(
				new RectangleF(innerleft, outertop, innerright - innerleft, innertop - outertop),
				fcolor,
				true
			);

			renderer.RenderRectangleFilled(
				new RectangleF(innerleft, innerbottom, innerright - innerleft, outerbottom - innerbottom),
				fcolor,
				true
			);

			// Draw the borders
			renderer.RenderRectangle(outerborder, 1.0f, bordercolor, true);
			renderer.RenderRectangle(innerborder, 1.0f, bordercolor, true);

			// Highlight a border if necessary
			if(highlight != Highlight.None && highlight != Highlight.Body)
				renderer.RenderLine(lines[highlight].v1, lines[highlight].v2, 1.0f, borderhighlightcolor, true);
		}

		public Highlight CheckHighlight(Vector2D pos, float scale)
		{
			float distance = float.MaxValue;
			Highlight highlight = Highlight.None;

			foreach (Highlight h in (Highlight[])Enum.GetValues(typeof(Highlight)))
			{
				if (h == Highlight.None || h == Highlight.Body)
					continue;

				float d = Line2D.GetDistanceToLine(lines[h].v1, lines[h].v2, pos, true);

				if (d <= BuilderModes.BuilderPlug.Me.HighlightRange / scale && d < distance)
				{
					distance = d;
					highlight = h;
				}
			}

			if (highlight != Highlight.None)
				return highlight;

			if ((OuterLeft < pos.x && OuterRight > pos.x && OuterTop > pos.y && OuterBottom < pos.y) &&	!(InnerLeft < pos.x && InnerRight > pos.x && InnerTop > pos.y && InnerBottom < pos.y))
				return Highlight.Body;

			return Highlight.None;
		}

		public void SnapToGrid(Highlight highlight, Vector2D pos)
		{
			Vector2D newpos = GridSetup.SnappedToGrid(pos, gridsize, gridsizeinv);

			switch (highlight)
			{
				case Highlight.OuterLeft:
					if (newpos.x < innerleft) outerleft = newpos.x;
					break;
				case Highlight.OuterRight:
					if(newpos.x > innerright) outerright = newpos.x;
					break;
				case Highlight.OuterTop:
					if (newpos.y > innertop) outertop = newpos.y;
					break;
				case Highlight.OuterBottom:
					if (newpos.y < innerbottom) outerbottom = newpos.y;
					break;

				case Highlight.InnerLeft:
					if (newpos.x > outerleft && newpos.x < innerright) innerleft = newpos.x;
					break;
				case Highlight.InnerRight:
					if (newpos.x < outerright && newpos.x > innerleft) innerright = newpos.x;
					break;
				case Highlight.InnerTop:
					if (newpos.y < outertop && newpos.y > innerbottom) innertop = newpos.y;
					break;
				case Highlight.InnerBottom:
					if (newpos.y > outerbottom && newpos.y < innertop) innerbottom = newpos.y;
					break;
			}

			UpdateLines();
		}

		public Point GetNewControlSectorPosition()
		{
			CreateBlockmap();

			// find position for new control sector
			for (int x = (int)outerleft; x < (int)outerright; x += (int)gridsize)
			{
				for (int y = (int)outertop; y > (int)outerbottom; y -= (int)gridsize)
				{
					if (x >= (int)innerleft && x < (int)innerright && y <= (int)innertop && y > (int)innerbottom)
						continue;

					List<BlockEntry> blocks = blockmap.GetLineBlocks(
						new Vector2D(x + 1, y - 1),
						new Vector2D(x + 2, y - 2)
						);

					// no elements in the area yet
					if (blocks.Count == 0)
					{
						return new Point(x, y);
					}
					else
					{
						foreach (BlockEntry be in blocks)
						{
							if (be.Lines.Count == 0)
							{
								return new Point(x, y);
							}
						}
					}
				}
			}

			throw new Exception("No space left for control sectors");
		}

		private void CreateBlockmap()
		{
			// Make blockmap
			RectangleF area = MapSet.CreateArea(General.Map.Map.Vertices);
			area = MapSet.IncreaseArea(area, General.Map.Map.Things);
			area = MapSet.IncreaseArea(area, new Vector2D(outerleft, outertop));
			area = MapSet.IncreaseArea(area, new Vector2D(outerright, outerbottom));
			if (blockmap != null) blockmap.Dispose();
			blockmap = new BlockMap<BlockEntry>(area, (int)gridsize);
			blockmap.AddLinedefsSet(General.Map.Map.Linedefs);
			blockmap.AddSectorsSet(General.Map.Map.Sectors);
			// blockmap.AddThingsSet(General.Map.Map.Things);
		}

		public void Edit()
		{
			ControlSectorAreaConfig csacfg = new ControlSectorAreaConfig(this);

			csacfg.ShowDialog((Form)General.Interface);
		}

		public void SaveConfig()
		{
			ListDictionary config = new ListDictionary();

			config.Add("usecustomtagrange", usecustomtagrange);

			if (usecustomtagrange)
			{
				config.Add("firsttag", firsttag);
				config.Add("lasttag", lasttag);
			}

			config.Add("outerleft", outerleft);
			config.Add("outerright", outerright);
			config.Add("outertop", outertop);
			config.Add("outerbottom", outerbottom);

			config.Add("innerleft", innerleft);
			config.Add("innerright", innerright);
			config.Add("innertop", innertop);
			config.Add("innerbottom", innerbottom);

			General.Map.Options.WritePluginSetting("controlsectorarea", config);
		}

		public void LoadConfig()
		{
			ListDictionary config = (ListDictionary)General.Map.Options.ReadPluginSetting("controlsectorarea", new ListDictionary());

			usecustomtagrange = General.Map.Options.ReadPluginSetting("controlsectorarea.usecustomtagrange", false);
			firsttag = General.Map.Options.ReadPluginSetting("controlsectorarea.firsttag", 0);
			lasttag = General.Map.Options.ReadPluginSetting("controlsectorarea.lasttag", 0);

			outerleft = General.Map.Options.ReadPluginSetting("controlsectorarea.outerleft", outerleft);
			outerright = General.Map.Options.ReadPluginSetting("controlsectorarea.outerright", outerright);
			outertop = General.Map.Options.ReadPluginSetting("controlsectorarea.outertop", outertop);
			outerbottom = General.Map.Options.ReadPluginSetting("controlsectorarea.outerbottom", outerbottom);

			innerleft = General.Map.Options.ReadPluginSetting("controlsectorarea.innerleft", innerleft);
			innerright = General.Map.Options.ReadPluginSetting("controlsectorarea.innerright", innerright);
			innertop = General.Map.Options.ReadPluginSetting("controlsectorarea.innertop", innertop);
			innerbottom = General.Map.Options.ReadPluginSetting("controlsectorarea.innerbottom", innerbottom);

			UpdateLines();
		}

		public int GetNewTag()
		{
			if (usecustomtagrange)
			{
				for (int i = firsttag; i <= lasttag; i++)
				{
					if (General.Map.Map.GetSectorsByTag(i).Count == 0)
						return i;
				}

				throw new Exception("No free tags in the range between " + firsttag.ToString() + " and " + lasttag.ToString() + ".");
			}

			return General.Map.Map.GetNewTag();
		}

		#endregion
	}
}
