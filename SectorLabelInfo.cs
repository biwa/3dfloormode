using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
    class SectorLabelInfo
    {
        private bool floor;
        private bool ceiling;
        private bool bottom;
        private bool top;
        private Dictionary<PlaneType, List<SlopeVertexGroup>> slopevertexgroups;

        SectorLabelInfo()
        {
            floor = false;
            ceiling = false;
            bottom = false;
            top = false;

            slopevertexgroups = new Dictionary<PlaneType, List<SlopeVertexGroup>>();

            foreach (PlaneType pt in Enum.GetValues(typeof(PlaneType)))
            {
                slopevertexgroups.Add(pt, new List<SlopeVertexGroup>());
            }
        }

        void AddSlopeVertexGroup(PlaneType pt, SlopeVertexGroup svg)
        {
            if (!slopevertexgroups[pt].Contains(svg))
                slopevertexgroups[pt].Add(svg);
                
        }
    }
}
