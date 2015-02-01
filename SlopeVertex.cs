#region ================== Namespaces

using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public class SlopeVertex
	{
		#region ================== Variables

		private Vector2D pos;
		private float z;
		private bool selected;

		#endregion

		#region ================== Constructors

		public SlopeVertex(Vector2D p, float z)
		{
			this.pos = new Vector2D(p);
			this.z = z;
			this.selected = false;
		}

		#endregion

		#region ================== Properties

		public Vector2D Pos { get { return pos; } set { pos = value; } }
		public float Z { get { return z; } set { z = value; } }
		public bool Selected { get { return selected; } set { selected = value; } }

		#endregion
	}
}