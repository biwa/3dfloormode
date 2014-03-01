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

namespace CodeImp.DoomBuilder.ThreeDFloorMode
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
			OuterTopLeft,
			OuterTopRight,
			OuterBottomLeft,
			OuterBottomRight,
			InnerTopLeft,
			InnerTopRight,
			InnerBottomLeft,
			InnerBottomRight,
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
		private Dictionary<Highlight, Vector2D> points;
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
			set { outerleft = value; UpdateLinesAndPoints();	}
		}

		public float OuterRight
		{
			get { return outerright; }
			set { outerright = value; UpdateLinesAndPoints(); }
		}

		public float OuterTop
		{
			get { return outertop; }
			set { outertop = value; UpdateLinesAndPoints(); }
		}

		public float OuterBottom
		{
			get { return outerbottom; }
			set { outerbottom = value; UpdateLinesAndPoints(); }
		}

		public float InnerLeft
		{
			get { return innerleft; }
			set { innerleft = value; UpdateLinesAndPoints(); }
		}

		public float InnerRight
		{
			get { return innerright; }
			set { innerright = value; UpdateLinesAndPoints(); }
		}

		public float InnerTop
		{
			get { return innertop; }
			set { innertop = value; UpdateLinesAndPoints(); }
		}

		public float InnerBottom
		{
			get { return innerbottom; }
			set { innerbottom = value; UpdateLinesAndPoints(); }
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
			points = new Dictionary<Highlight, Vector2D>();

			this.gridsize = gridsize;
			gridsizeinv = 1.0f / gridsize;

			this.sectorsize = sectorsize;

			UpdateLinesAndPoints();
		}

		#endregion

		#region ================== Methods

		public void UpdateLinesAndPoints()
		{
			lines[Highlight.OuterLeft] = new Line2D(outerleft, outertop, outerleft, outerbottom);
			lines[Highlight.OuterRight] = new Line2D(outerright, outertop, outerright, outerbottom);
			lines[Highlight.OuterTop] = new Line2D(outerleft, outertop, outerright, outertop);
			lines[Highlight.OuterBottom] = new Line2D(outerleft, outerbottom, outerright, outerbottom);

			lines[Highlight.InnerLeft] = new Line2D(innerleft, innertop, innerleft, innerbottom);
			lines[Highlight.InnerRight] = new Line2D(innerright, innertop, innerright, innerbottom);
			lines[Highlight.InnerTop] = new Line2D(innerleft, innertop, innerright, innertop);
			lines[Highlight.InnerBottom] = new Line2D(innerleft, innerbottom, innerright, innerbottom);

			points[Highlight.OuterTopLeft] = new Vector2D(outerleft, outertop);
			points[Highlight.OuterTopRight] = new Vector2D(outerright, outertop);
			points[Highlight.OuterBottomLeft] = new Vector2D(outerleft, outerbottom);
			points[Highlight.OuterBottomRight] = new Vector2D(outerright, outerbottom);

			points[Highlight.InnerTopLeft] = new Vector2D(innerleft, innertop);
			points[Highlight.InnerTopRight] = new Vector2D(innerright, innertop);
			points[Highlight.InnerBottomLeft] = new Vector2D(innerleft, innerbottom);
			points[Highlight.InnerBottomRight] = new Vector2D(innerright, innerbottom);

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
			if (highlight >= Highlight.OuterLeft && highlight <= Highlight.InnerBottom)
				renderer.RenderLine(lines[highlight].v1, lines[highlight].v2, 1.0f, borderhighlightcolor, true);
			else
			{
				// Highlight the corners
				switch (highlight)
				{
					// Outer corners
					case Highlight.OuterTopLeft:
						renderer.RenderLine(lines[Highlight.OuterTop].v1, lines[Highlight.OuterTop].v2, 1.0f, borderhighlightcolor, true);
						renderer.RenderLine(lines[Highlight.OuterLeft].v1, lines[Highlight.OuterLeft].v2, 1.0f, borderhighlightcolor, true);
						break;
					case Highlight.OuterTopRight:
						renderer.RenderLine(lines[Highlight.OuterTop].v1, lines[Highlight.OuterTop].v2, 1.0f, borderhighlightcolor, true);
						renderer.RenderLine(lines[Highlight.OuterRight].v1, lines[Highlight.OuterRight].v2, 1.0f, borderhighlightcolor, true);
						break;
					case Highlight.OuterBottomLeft:
						renderer.RenderLine(lines[Highlight.OuterBottom].v1, lines[Highlight.OuterBottom].v2, 1.0f, borderhighlightcolor, true);
						renderer.RenderLine(lines[Highlight.OuterLeft].v1, lines[Highlight.OuterLeft].v2, 1.0f, borderhighlightcolor, true);
						break;
					case Highlight.OuterBottomRight:
						renderer.RenderLine(lines[Highlight.OuterBottom].v1, lines[Highlight.OuterBottom].v2, 1.0f, borderhighlightcolor, true);
						renderer.RenderLine(lines[Highlight.OuterRight].v1, lines[Highlight.OuterRight].v2, 1.0f, borderhighlightcolor, true);
						break;

					// Inner corners
					case Highlight.InnerTopLeft:
						renderer.RenderLine(lines[Highlight.InnerTop].v1, lines[Highlight.InnerTop].v2, 1.0f, borderhighlightcolor, true);
						renderer.RenderLine(lines[Highlight.InnerLeft].v1, lines[Highlight.InnerLeft].v2, 1.0f, borderhighlightcolor, true);
						break;
					case Highlight.InnerTopRight:
						renderer.RenderLine(lines[Highlight.InnerTop].v1, lines[Highlight.InnerTop].v2, 1.0f, borderhighlightcolor, true);
						renderer.RenderLine(lines[Highlight.InnerRight].v1, lines[Highlight.InnerRight].v2, 1.0f, borderhighlightcolor, true);
						break;
					case Highlight.InnerBottomLeft:
						renderer.RenderLine(lines[Highlight.InnerBottom].v1, lines[Highlight.InnerBottom].v2, 1.0f, borderhighlightcolor, true);
						renderer.RenderLine(lines[Highlight.InnerLeft].v1, lines[Highlight.InnerLeft].v2, 1.0f, borderhighlightcolor, true);
						break;
					case Highlight.InnerBottomRight:
						renderer.RenderLine(lines[Highlight.InnerBottom].v1, lines[Highlight.InnerBottom].v2, 1.0f, borderhighlightcolor, true);
						renderer.RenderLine(lines[Highlight.InnerRight].v1, lines[Highlight.InnerRight].v2, 1.0f, borderhighlightcolor, true);
						break;
				}
			}
		}

		public Highlight CheckHighlight(Vector2D pos, float scale)
		{
			float distance = float.MaxValue;
			float d;
			Highlight highlight = Highlight.None;

			// Find a line to highlight
			foreach (Highlight h in (Highlight[])Enum.GetValues(typeof(Highlight)))
			{
				if (h >= Highlight.OuterLeft && h <= Highlight.InnerBottom)
				{
					d = Line2D.GetDistanceToLine(lines[h].v1, lines[h].v2, pos, true);

					if (d <= BuilderModes.BuilderPlug.Me.HighlightRange / scale && d < distance)
					{
						distance = d;
						highlight = h;
					}
				}
			}

			distance = float.MaxValue;

			// Find a corner to highlight
			foreach (Highlight h in (Highlight[])Enum.GetValues(typeof(Highlight)))
			{
				if (h >= Highlight.OuterTopLeft && h <= Highlight.InnerBottomRight)
				{
					d = Vector2D.Distance(pos, points[h]);

					if (d <= BuilderModes.BuilderPlug.Me.HighlightRange / scale && d < distance)
					{
						distance = d;
						highlight = h;
					}
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
				// Outer border
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

				// Inner border
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

				// Outer corners
				case Highlight.OuterTopLeft:
					if (newpos.x < innerleft) outerleft = newpos.x;
					if (newpos.y > innertop) outertop = newpos.y;
					break;
				case Highlight.OuterTopRight:
					if (newpos.x > innerright) outerright = newpos.x;
					if (newpos.y > innertop) outertop = newpos.y;
					break;
				case Highlight.OuterBottomLeft:
					if (newpos.x < innerleft) outerleft = newpos.x;
					if (newpos.y < innerbottom) outerbottom = newpos.y;
					break;
				case Highlight.OuterBottomRight:
					if (newpos.x > innerright) outerright = newpos.x;
					if (newpos.y < innerbottom) outerbottom = newpos.y;
					break;

				// Inner corners
				case Highlight.InnerTopLeft:
					if (newpos.x > outerleft && newpos.x < innerright) innerleft = newpos.x;
					if (newpos.y < outertop && newpos.y > innerbottom) innertop = newpos.y;
					break;
				case Highlight.InnerTopRight:
					if (newpos.x < outerright && newpos.x > innerleft) innerright = newpos.x;
					if (newpos.y < outertop && newpos.y > innerbottom) innertop = newpos.y;
					break;
				case Highlight.InnerBottomLeft:
					if (newpos.x > outerleft && newpos.x < innerright) innerleft = newpos.x;
					if (newpos.y > outerbottom && newpos.y < innertop) innerbottom = newpos.y;
					break;
				case Highlight.InnerBottomRight:
					if (newpos.x < outerright && newpos.x > innerleft) innerright = newpos.x;
					if (newpos.y > outerbottom && newpos.y < innertop) innerbottom = newpos.y;
					break;
			}

			UpdateLinesAndPoints();
		}

		public List<List<DrawnVertex>> GetNewControlSectorPosition()
		{
			CreateBlockmap();

			int margin = (int)((gridsize - sectorsize) / 2);

			// find position for new control sector
			for (int x = (int)outerleft; x < (int)outerright; x += (int)gridsize)
			{
				for (int y = (int)outertop; y > (int)outerbottom; y -= (int)gridsize)
				{
					if (x >= (int)innerleft && x < (int)innerright && y <= (int)innertop && y > (int)innerbottom)
						continue;

					List<BlockEntry> blocks = blockmap.GetLineBlocks(
						//new Vector2D(x + margin, y - margin),
						//new Vector2D(x + sectorsize - (2 * margin), y - sectorsize - (2 * margin))
						new Vector2D(x + 1, y - 1),
						new Vector2D(x + sectorsize - 1, y - sectorsize - 1)
					);


					// no elements in the area yet
					if (blocks.Count == 0)
					{
						List<DrawnVertex> dv = new List<DrawnVertex>();
						Point p = new Point(x + margin, y - margin);

						dv.Add(SectorVertex(p.X, p.Y));
						dv.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y));
						dv.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
						dv.Add(SectorVertex(p.X, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
						dv.Add(SectorVertex(p.X, p.Y));

						return new List<List<DrawnVertex>> { dv };
					}
					else
					{
						foreach (BlockEntry be in blocks)
						{
							if (be.Sectors.Count == 0)
							{
								List<DrawnVertex> dv = new List<DrawnVertex>();
								Point p = new Point(x + margin, y - margin);

								dv.Add(SectorVertex(p.X, p.Y));
								dv.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y));
								dv.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
								dv.Add(SectorVertex(p.X, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
								dv.Add(SectorVertex(p.X, p.Y));

								return new List<List<DrawnVertex>> { dv };
							}
						}
					}
				}
			}

			throw new Exception("No space left for control sectors");
		}

		public List<List<DrawnVertex>> GetNewControlSectorPosition(Vector2D pos, Vector2D vector, out Vector2D slopethingpos)
		{
			Line2D line = new Line2D(pos, pos + vector);
			Vector2D perpendicular = line.GetPerpendicular();
			int margin = (int)((gridsize - sectorsize) / 2);
			float half = sectorsize / 2;

			float xstep;
			float ystep;

			CreateBlockmap();

			if (perpendicular.x == 0)
			{
				xstep = 0;
				ystep = 1;
			}
			else
			{
				xstep = 1;
				ystep = perpendicular.y / perpendicular.x;
			}

			RectangleF a = new RectangleF(0, 0, 128, 128);
			RectangleF b = new RectangleF(32, 32, 64, 64);

			for (int i = 0; i < int.MaxValue; i++)
			{
				float x = i * xstep + pos.x;
				float y = i * ystep + pos.y;

				if (!(x % 1 < float.Epsilon || y % 1 < float.Epsilon))
					continue;

				Vector2D tl = new Vector2D(x - half, y + half);
				Vector2D br = new Vector2D(x + half, y - half);

				if (!(Inside(tl.x, tl.y) && Inside(tl.x, br.y) && Inside(br.x, tl.y) && Inside(br.x, br.y)))
					continue;

				List<BlockEntry> blocks = blockmap.GetLineBlocks(
					new Vector2D(x - half + 1, y + half - 1),
					new Vector2D(x + half - 1, y - half - 1)
				);


				// no elements in the area yet
				if (blocks.Count == 0)
				{
					Point p = new Point((int)(x - half), (int)(y + half));
					List<DrawnVertex> dv = new List<DrawnVertex>();

					slopethingpos = new Vector2D(x, y);

					dv.Add(SectorVertex(p.X, p.Y));
					dv.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y));
					dv.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
					dv.Add(SectorVertex(p.X, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
					dv.Add(SectorVertex(p.X, p.Y));

					return new List<List<DrawnVertex>> { dv };
				}
				else
				{
					foreach (BlockEntry be in blocks)
					{
						if (be.Sectors.Count == 0)
						{
							Point p = new Point((int)(x - half), (int)(y + half));
							List<DrawnVertex> dv = new List<DrawnVertex>();

							slopethingpos = new Vector2D(x, y);

							dv.Add(SectorVertex(p.X, p.Y));
							dv.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y));
							dv.Add(SectorVertex(p.X + BuilderPlug.Me.ControlSectorArea.SectorSize, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
							dv.Add(SectorVertex(p.X, p.Y - BuilderPlug.Me.ControlSectorArea.SectorSize));
							dv.Add(SectorVertex(p.X, p.Y));

							return new List<List<DrawnVertex>> { dv };
						}
					}
				}
			}

			throw new Exception("No space left for control sectors");
		}

		public List<List<DrawnVertex>> GetNewControlSectorPosition(ThreeDFloor tdf, out Vector3D slopethingpos, out Line2D slopeline)
		{
			Line2D line = new Line2D(tdf.Slope.Origin, tdf.Slope.Origin + tdf.Slope.Direction);
			Vector2D perpendicular = line.GetPerpendicular();
			float half = sectorsize / 2;
			List<Vector2D> verts = new List<Vector2D>(4);
			List<int> vals = new List<int>
			{
				Math.Abs(Math.Abs(tdf.TopHeight - tdf.Slope.TopHeight)), // difference between top heights on start and end of slope
				Math.Abs(Math.Abs(tdf.BottomHeight - tdf.Slope.BottomHeight)), // difference between bottom heights on start and end of slope
				Math.Abs((int)tdf.Slope.Direction.x), // difference between start and end of slope on the x axis
				Math.Abs((int)tdf.Slope.Direction.y) // difference between start and end of slope on the y axis
			};

			float xstep;
			float ystep;

			CreateBlockmap();

			if (perpendicular.x == 0)
			{
				xstep = 0;
				ystep = 1;
			}
			else
			{
				xstep = 1;
				ystep = perpendicular.y / perpendicular.x;
			}

			int gcd = GCD(vals.OrderBy(o=>o).ToArray());

			verts.Add(new Vector2D(0, 0));
			verts.Add(tdf.Slope.Direction * 2);
			verts.Add((tdf.Slope.Direction + perpendicular) * 2);
			verts.Add(perpendicular * 2);
			verts.Add(tdf.Slope.Direction + perpendicular); // Position of the slope things
			verts.Add(new Vector2D(vals[0], vals[1]));

			/*
			for (int i = 0; i < verts.Count; i++)
			{
				verts[i] /= gcd;
			}
			*/

			/*
			while (new Line2D(verts[0], verts[1]).GetLength() < sectorsize)
			{
				for (int i = 0; i < verts.Count; i++)
				{
					verts[i] *= 2;
				}
			}
			*/

			while (new Line2D(verts[0], verts[1]).GetLength() > sectorsize)
			{
				for (int i = 0; i < verts.Count; i++)
				{
					verts[i] /= 2;
				}
			}

			if (tdf.BottomHeight > tdf.Slope.BottomHeight)
				verts[5] = new Vector2D(verts[5].x * -1, verts[5].y);

			if (tdf.TopHeight > tdf.Slope.TopHeight)
				verts[5] = new Vector2D(verts[5].x, verts[5].y * -1);

			for (int i = 0; i < int.MaxValue; i++)
			{
				float x = i * xstep + tdf.Slope.Origin.x;
				float y = i * ystep + tdf.Slope.Origin.y;

				if (!(x % 1 < float.Epsilon || y % 1 < float.Epsilon))
					continue;

				Vector2D tl = new Vector2D(x - half, y + half);
				Vector2D br = new Vector2D(x + half, y - half);

				if (!(Inside(tl.x, tl.y) && Inside(tl.x, br.y) && Inside(br.x, tl.y) && Inside(br.x, br.y)))
					continue;

				List<BlockEntry> blocks = blockmap.GetLineBlocks(
					new Vector2D(x - half + 1, y + half - 1),
					new Vector2D(x + half - 1, y - half - 1)
				);

				// no elements in the area yet
				if (blocks.Count == 0)
				{
					List<DrawnVertex> dv = new List<DrawnVertex>();

					dv.Add(SectorVertex(verts[0].x + x, verts[0].y + y));
					dv.Add(SectorVertex(verts[1].x + x, verts[1].y + y));
					dv.Add(SectorVertex(verts[2].x + x, verts[2].y + y));
					dv.Add(SectorVertex(verts[3].x + x, verts[3].y + y));
					dv.Add(SectorVertex(verts[0].x + x, verts[0].y + y));

					slopethingpos = new Vector3D(verts[4].x, verts[4].y, verts[5].x);
					slopeline = new Line2D(verts[0], verts[1]);

					return new List<List<DrawnVertex>> { dv };
				}
				else
				{
					foreach (BlockEntry be in blocks)
					{
						if (be.Sectors.Count == 0)
						{
							List<DrawnVertex> dv = new List<DrawnVertex>();

							dv.Add(SectorVertex(verts[0].x + x, verts[0].y + y));
							dv.Add(SectorVertex(verts[1].x + x, verts[1].y + y));
							dv.Add(SectorVertex(verts[2].x + x, verts[2].y + y));
							dv.Add(SectorVertex(verts[3].x + x, verts[3].y + y));
							dv.Add(SectorVertex(verts[0].x + x, verts[0].y + y));

							slopethingpos = new Vector3D(verts[4].x + x, verts[4].y + y, verts[5].x);
							slopeline = new Line2D(new Vector2D(verts[0].x + x, verts[0].y + y), new Vector2D(verts[3].x + x, verts[3].y + y));

							return new List<List<DrawnVertex>> { dv };
						}
					}
				}
			}


			throw new Exception("No space left for control sectors");
		}

		public bool Inside(float x, float y)
		{
			return Inside(new Vector2D(x, y));
		}

		public bool Inside(Vector2D pos)
		{
			if (
				(pos.x > outerleft && pos.x < outerright && pos.y < outertop && pos.y > outerbottom) &&
				!(pos.x > innerleft && pos.x < innerright && pos.y < innertop && pos.y > innerbottom)
			)
				return true;

			return false;
		}

		// Aligns the area to the grid, expanding the area if necessary
		private RectangleF AlignAreaToGrid(RectangleF area)
		{
			List<float> f = new List<float>
			{
				area.Left,
				area.Top,
				area.Right,
				area.Bottom
			};

			for (int i = 0; i < f.Count; i++)
			{
				if (f[i] < 0)
					f[i] = (float)Math.Floor(f[i] / gridsize) * gridsize;
				else
					f[i] = (float)Math.Ceiling(f[i] / gridsize) * gridsize;
			}


			float l = f[0];
			float t = f[1];
			float r = f[2];
			float b = f[3];

			return new RectangleF(l, t, r - l, b - t);
		}

		private void CreateBlockmap()
		{
			// Make blockmap
			RectangleF area = MapSet.CreateArea(General.Map.Map.Vertices);
			area = MapSet.IncreaseArea(area, General.Map.Map.Things);
			area = MapSet.IncreaseArea(area, new Vector2D(outerleft, outertop));
			area = MapSet.IncreaseArea(area, new Vector2D(outerright, outerbottom));

			area = AlignAreaToGrid(area);

			if (blockmap != null) blockmap.Dispose();
			blockmap = new BlockMap<BlockEntry>(area, (int)gridsize);
			blockmap.AddLinedefsSet(General.Map.Map.Linedefs);
			blockmap.AddSectorsSet(General.Map.Map.Sectors);
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

			UpdateLinesAndPoints();
		}

		public int GetNewSectorTag()
		{
			if (usecustomtagrange)
			{
				for (int i = firsttag; i <= lasttag; i++)
				{
					if (General.Map.Map.GetSectorsByTag(i).Count == 0)
						return i;
				}

				throw new Exception("No free tags in the custom range between " + firsttag.ToString() + " and " + lasttag.ToString() + ".");
			}

			return General.Map.Map.GetNewTag();
		}

		public int GetNewLineID()
		{
			return General.Map.Map.GetNewTag();
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

		static int GCD(int[] numbers)
		{
			return numbers.Aggregate(GCD);
		}

		static int GCD(int a, int b)
		{
			return b == 0 ? a : GCD(b, a % b);
		}

		#endregion
	}
}
