using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public partial class MenusForm : Form
	{
		public ToolStripButton FloorSlope { get { return floorslope; } }
		public ToolStripButton CeilingSlope { get { return ceilingslope; } }
		public ToolStripButton FloorAndCeilingSlope { get { return floorandceilingslope; } }

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
	}
}
