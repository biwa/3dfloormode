using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public partial class SlopeVertexEditForm : Form
	{
		public SlopeVertexEditForm(SlopeVertex sv)
		{
			InitializeComponent();

			positionx.Text = sv.pos.x.ToString();
			positiony.Text = sv.pos.y.ToString();

			if (sv.floor)
				floorz.Text = sv.floorz.ToString();

			if (sv.ceiling)
				ceilingz.Text = sv.ceilingz.ToString();
		}

		private void apply_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}
	}
}
