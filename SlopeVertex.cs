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
		private bool selected;

		#endregion

		#region ================== Constructors

		public SlopeVertex(Vector2D p, float fz, float cz)
		{
			this.pos = new Vector2D(p);
			this.floorz = fz;
			this.ceilingz = cz;
			this.selected = false;
		}

		#endregion

		#region ================== Properties

		public Vector2D Pos { get { return pos; } set { pos = value; } }
		public float FloorZ { get { return floorz; } set { floorz = value; } }
		public float CeilingZ { get { return ceilingz; } set { ceilingz = value; } }
		public bool Selected { get { return selected; } set { selected = value; } }

		#endregion
	}
}