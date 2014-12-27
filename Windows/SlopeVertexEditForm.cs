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

			positionx.Text = sv.Pos.x.ToString();
			positiony.Text = sv.Pos.y.ToString();

			if (sv.Floor)
				floorz.Text = sv.FloorZ.ToString();

			if (sv.Ceiling)
				ceilingz.Text = sv.CeilingZ.ToString();
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
