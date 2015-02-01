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

		public void FindSectors()
		{
			sectors.Clear();

			foreach (Sector s in General.Map.Map.Sectors)
			{
				if (s.Fields.GetValue("floorplane_id", -1) == id || s.Fields.GetValue("ceilingplane_id", -1) == id)
					sectors.Add(s);
			}
		}

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
			List<Vector3D> planevertices = new List<Vector3D>();
			List<Sector> removesectors = new List<Sector>();

			foreach (SlopeVertex sv in vertices)
			{
				planevertices.Add(new Vector3D(sv.Pos, sv.Z));
			}

			// Only 2 vertices, so create the 3rd one we need for the plane
			if (planevertices.Count == 2)
			{
				float z = planevertices[0].z;

				// Create a line between the two points we got...
				Line2D line = new Line2D(planevertices[0], planevertices[1]);

				// ... and the the perpendicular
				Vector3D perpendicular = line.GetPerpendicular();

				// Adding the perpendicular to one of the original points will result
				// in the third point we need
				Vector2D v = planevertices[0] + perpendicular;

				planevertices.Add(new Vector3D(v, z));
			}

			Plane floorplane = new Plane(planevertices[0], planevertices[1], planevertices[2], floor ? true : false);

			foreach (Sector s in sectors)
			{
				bool hasplane = false;

				if (floor)
				{
					hasplane = true;

					if (s.Fields.ContainsKey("floorplane_id"))
						s.Fields["floorplane_id"] = new UniValue(UniversalType.Integer, id);
					else
						s.Fields.Add("floorplane_id", new UniValue(UniversalType.Integer, id));
				}
				else if (s.Fields.ContainsKey("floorplane_id") && s.Fields.GetValue("floorplane_id", -1) == id)
				{
					s.Fields.Remove("floorplane_id");
				}

				if (ceiling)
				{
					hasplane = true;

					if (s.Fields.ContainsKey("ceilingplane_id"))
						s.Fields["ceilingplane_id"] = new UniValue(UniversalType.Integer, id);
					else
						s.Fields.Add("ceilingplane_id", new UniValue(UniversalType.Integer, id));
				}
				else if (s.Fields.ContainsKey("ceilingplane_id") && s.Fields.GetValue("ceilingplane_id", -1) == id)
				{
					s.Fields.Remove("ceilingplane_id");
				}

				if (!hasplane)
					removesectors.Add(s);
			}

			foreach (Sector s in removesectors)
				sectors.Remove(s);

			foreach (Sector s in sectors)
				BuilderPlug.Me.UpdateSlopes(s);
		}

		#endregion
	}
}