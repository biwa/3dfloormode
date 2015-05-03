using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public partial class MenusForm : Form
	{
		public ToolStripButton FloorSlope { get { return floorslope; } }
		public ToolStripButton CeilingSlope { get { return ceilingslope; } }
		public ToolStripButton FloorAndCeilingSlope { get { return floorandceilingslope; } }
		public ToolStripButton UpdateSlopes { get { return updateslopes; } }
		public ContextMenuStrip AddSectorsContextMenu { get { return addsectorscontextmenu; } }

		public MenusForm()
		{
			InitializeComponent();
		}

		private void InvokeTaggedAction(object sender, EventArgs e)
		{
			General.Interface.InvokeTaggedAction(sender, e);
		}

		private void floorslope_Click(object sender, EventArgs e)
		{
			if (floorslope.Checked)
				return;

			General.Interface.InvokeTaggedAction(sender, e);
		}

		private void ceilingslope_Click(object sender, EventArgs e)
		{
			if (ceilingslope.Checked)
				return;

			General.Interface.InvokeTaggedAction(sender, e);
		}

		private void floorandceilingslope_Click(object sender, EventArgs e)
		{
			if (floorandceilingslope.Checked)
				return;

			General.Interface.InvokeTaggedAction(sender, e);
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			BuilderPlug.Me.UpdateSlopes();
		}

		private void floorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			List<SlopeVertexGroup> svgs = ((SlopeMode)General.Editing.Mode).GetSelectedSlopeVertexGroups();

			// Can only add sectors to one slope vertex group
			if (svgs.Count != 1)
				return;

			foreach (Sector s in General.Map.Map.GetSelectedSectors(true).ToList())
			{
				svgs[0].AddSector(s, PlaneType.Floor);
				BuilderPlug.Me.UpdateSlopes(s);
			}
		}

		private void removeSlopeFromFloorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (Sector s in General.Map.Map.GetSelectedSectors(true).ToList())
			{
				SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(s);
				svg.RemoveSector(s, PlaneType.Floor);
			}
		}

		private void ceilingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			List<SlopeVertexGroup> svgs = ((SlopeMode)General.Editing.Mode).GetSelectedSlopeVertexGroups();

			// Can only add sectors to one slope vertex group
			if (svgs.Count != 1)
				return;

			foreach (Sector s in General.Map.Map.GetSelectedSectors(true).ToList())
			{
				svgs[0].AddSector(s, PlaneType.Ceiling);
				BuilderPlug.Me.UpdateSlopes(s);
			}
		}

		private void removeSlopeFromCeilingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (Sector s in General.Map.Map.GetSelectedSectors(true).ToList())
			{
				SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(s);
				svg.RemoveSector(s, PlaneType.Ceiling);
			}
		}

		private void addsectorscontextmenu_Opening(object sender, CancelEventArgs e)
		{
			// Disable adding if more than one slope vertex group is selected,
			// otherwise enable adding
			List<SlopeVertexGroup> svgs = ((SlopeMode)General.Editing.Mode).GetSelectedSlopeVertexGroups();

			floorToolStripMenuItem.Enabled = svgs.Count == 1;
			ceilingToolStripMenuItem.Enabled = svgs.Count == 1;
		}
	}
}
