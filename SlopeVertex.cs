#region ================== Namespaces

using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public struct SlopeVertex
	{
		#region ================== Variables

		public Vector2D pos;
		public float floorz;
		public float ceilingz;
		public bool floor;
		public bool ceiling;

		#endregion

		#region ================== Constructors

		public SlopeVertex(Vector2D p, bool f, float fz, bool c, float cz)
		{
			this.pos = new Vector2D(p);
			this.floorz = fz;
			this.ceilingz = cz;
			this.floor = f;
			this.ceiling = c;
		}

		#endregion
	}
}