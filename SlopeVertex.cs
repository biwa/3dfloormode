#region ================== Namespaces

using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public class SlopeVertex
	{
		#region ================== Variables

		private Vector2D pos;
		private float floorz;
		private float ceilingz;
		private bool floor;
		private bool ceiling;
		private bool selected;

		#endregion

		#region ================== Constructors

		public SlopeVertex(Vector2D p, bool f, float fz, bool c, float cz)
		{
			this.pos = new Vector2D(p);
			this.floorz = fz;
			this.ceilingz = cz;
			this.floor = f;
			this.ceiling = c;
			this.selected = false;
		}

		#endregion

		#region ================== Properties

		public Vector2D Pos { get { return pos; } set { pos = value; } }
		public float FloorZ { get { return floorz; } set { floorz = value; } }
		public float CeilingZ { get { return ceilingz; } set { ceilingz = value; } }
		public bool Floor { get { return floor; } set { floor = value; } }
		public bool Ceiling { get { return ceiling; } set { ceiling = value; } }
		public bool Selected { get { return selected; } set { selected = value; } }

		#endregion
	}
}