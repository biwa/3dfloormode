#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public class SlopeVertexGroup
	{
		#region ================== Variables

		private List<SlopeVertex> vertices;
		private List<Sector> sectors;
		private int id;
		private bool ceiling;
		private bool floor;

		#endregion

		#region ================== Constructors

		public SlopeVertexGroup(int id, List<SlopeVertex> vertices, bool floor, bool ceiling)
		{
			this.vertices = vertices;
			this.id = id;
			this.floor = floor;
			this.ceiling = ceiling;
			sectors = new List<Sector>();
		}

		#endregion

		#region ================== Properties

		public List<SlopeVertex> Vertices { get { return vertices; } set { vertices = value; } }
		public List<Sector> Sectors { get { return sectors; } set { sectors = value; } }
		public int Id { get { return id; } }
		public bool Ceiling { get { return ceiling; } set { ceiling = value; } }
		public bool Floor { get { return floor; } set { floor = value; } }

		#endregion

		#region ================== Methods

		public void RemoveFromSectors()
		{
			foreach (Sector s in sectors)
			{
				if (floor)
				{
					s.FloorSlope = new Vector3D();
					s.FloorSlopeOffset = 0;
					s.Fields.Remove("floorplane_id");
				}

				if (ceiling)
				{
					s.CeilSlope = new Vector3D();
					s.CeilSlopeOffset = 0;
					s.Fields.Remove("ceilingplane_id");
				}
			}
		}

		public void ApplyToSectors()
		{
			List<Vector3D> floorvertices = new List<Vector3D>();
			List<Vector3D> ceilingvertices = new List<Vector3D>();

			foreach (SlopeVertex sv in vertices)
			{
				floorvertices.Add(new Vector3D(sv.Pos, sv.FloorZ));
				ceilingvertices.Add(new Vector3D(sv.Pos, sv.CeilingZ));
			}

			// Only 2 vertices, so create the 3rd one we need for the plane
			if (floorvertices.Count == 2)
			{
				float floorz = floorvertices[0].z;
				float ceilingz = ceilingvertices[0].z;

				// Create a line between the two points we got...
				Line2D line = new Line2D(floorvertices[0], floorvertices[1]);

				// ... and the the perpendicular
				Vector3D perpendicular = line.GetPerpendicular();

				// Adding the perpendicular to one of the original points will result
				// in the third point we need
				Vector2D v = floorvertices[0] + perpendicular;

				floorvertices.Add(new Vector3D(v, floorz));
				ceilingvertices.Add(new Vector3D(v, ceilingz));
			}

			Plane floorplane = new Plane(floorvertices[0], floorvertices[1], floorvertices[2], true);
			Plane ceilingplane = new Plane(ceilingvertices[0], ceilingvertices[1], ceilingvertices[2], false);

			foreach (Sector s in sectors)
			{
				if (s.Fields.GetValue("floorplane_id", -1) == id)
				{
					if (s.Fields.ContainsKey("floorplane_id"))
						s.Fields.Remove("floorplane_id");

					if (floor)
					{
						s.Fields.Add("floorplane_id", new UniValue(UniversalType.Integer, id));

						s.FloorSlope = new Vector3D(floorplane.a, floorplane.b, floorplane.c);
						s.FloorSlopeOffset = floorplane.d;
					}
					else
					{
						s.FloorSlope = new Vector3D();
						s.FloorSlopeOffset = 0;
					}
				}

				if (s.Fields.GetValue("ceilingplane_id", -1) == id)
				{
					if (s.Fields.ContainsKey("ceilingplane_id"))
						s.Fields.Remove("ceilingplane_id");

					if (ceiling)
					{
						s.Fields.Add("ceilingplane_id", new UniValue(UniversalType.Integer, id));

						s.CeilSlope = new Vector3D(ceilingplane.a, ceilingplane.b, ceilingplane.c);
						s.CeilSlopeOffset = ceilingplane.d;
					}
					else
					{
						s.CeilSlope = new Vector3D();
						s.CeilSlopeOffset = 0;
					}
				}
			}
		}

		#endregion
	}
}