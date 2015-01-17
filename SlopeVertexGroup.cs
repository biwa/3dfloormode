#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public class SlopeVertexGroup
	{
		#region ================== Variables

		private List<SlopeVertex> vertices;
		private int id;

		#endregion

		#region ================== Constructors

		public SlopeVertexGroup(int id, List<SlopeVertex> vertices)
		{
			this.vertices = vertices;
			this.id = id;
		}

		#endregion

		#region ================== Properties

		public List<SlopeVertex> Vertices { get { return vertices; } set { vertices = value; } }
		public int Id { get { return id; } }

		#endregion
	}
}