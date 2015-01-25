#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public class SlopeVertexGroup
	{
		#region ================== Variables

		private List<SlopeVertex> vertices;
		private List<Sector> sectors;
		private int id;

		#endregion

		#region ================== Constructors

		public SlopeVertexGroup(int id, List<SlopeVertex> vertices)
		{
			this.vertices = vertices;
			this.id = id;
			sectors = new List<Sector>();
		}

		#endregion

		#region ================== Properties

		public List<SlopeVertex> Vertices { get { return vertices; } set { vertices = value; } }
		public List<Sector> Sectors { get { return sectors; } set { sectors = value; } }
		public int Id { get { return id; } }

		#endregion

		#region ================== Methods

		public void RemoveFromSectors()
		{
			foreach (Sector s in sectors)
			{
				s.CeilSlope = new Vector3D();
				s.CeilSlopeOffset = 0;
				s.FloorSlope = new Vector3D();
				s.FloorSlopeOffset = 0;

				s.Fields.Remove("floorplane_id");
				s.Fields.Remove("ceilingplane_id");
			}
		}

		#endregion
	}
}