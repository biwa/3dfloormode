using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public partial class SlopeVertexEditForm : Form
	{
		private List<SlopeVertex> vertices;

		public SlopeVertexEditForm()
		{
			InitializeComponent();
		}

		public void Setup(List<SlopeVertex> vertices)
		{
			this.vertices = vertices;

			SlopeVertex fv = vertices[0];
			SlopeVertexGroup fsvg = BuilderPlug.Me.GetSlopeVertexGroup(fv);

			positionx.Text = fv.Pos.x.ToString();
			positiony.Text = fv.Pos.y.ToString();

			floor.Checked = fsvg.Floor;
			ceiling.Checked = fsvg.Ceiling;

			if (fsvg.Floor)
				floorz.Text = fv.FloorZ.ToString();

			if (fsvg.Ceiling)
				ceilingz.Text = fv.CeilingZ.ToString();

			if (vertices.Count > 1)
			{
				this.Text = "Edit slope vertices (" + vertices.Count.ToString() + ")";

				foreach (SlopeVertex sv in vertices)
				{
					SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(sv);

					if (sv.Pos.x.ToString() != positionx.Text)
						positionx.Text = "";

					if (sv.Pos.y.ToString() != positiony.Text)
						positiony.Text = "";

					if (sv.FloorZ.ToString() != floorz.Text)
						floorz.Text = "";

					if (sv.CeilingZ.ToString() != ceilingz.Text)
						ceilingz.Text = "";

					if (svg.Floor != floor.Checked)
						floor.CheckState = CheckState.Indeterminate;

					if (svg.Ceiling != ceiling.Checked)
						ceiling.CheckState = CheckState.Indeterminate;
				}
			}

			floorz.Enabled = floor.Checked;
			ceilingz.Enabled = ceiling.Checked;
		}		

		private void apply_Click(object sender, EventArgs e)
		{
			List<SlopeVertexGroup> groups = new List<SlopeVertexGroup>();

			foreach (SlopeVertex sv in vertices)
			{
				float x = positionx.GetResultFloat(sv.Pos.x);
				float y = positiony.GetResultFloat(sv.Pos.y);
				SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(sv);

				sv.Pos = new Vector2D(x, y);

				if (floor.CheckState != CheckState.Indeterminate)
				{
					svg.Floor = floor.Checked;

					if (svg.Floor)
						sv.FloorZ = floorz.GetResultFloat(sv.FloorZ);
				}

				if (ceiling.CheckState != CheckState.Indeterminate)
				{
					svg.Ceiling = ceiling.Checked;

					if (svg.Ceiling)
						sv.CeilingZ = ceilingz.GetResultFloat(sv.CeilingZ);
				}

				if (!groups.Contains(svg))
					groups.Add(svg);
			}

			foreach (SlopeVertexGroup svg in groups)
				svg.ApplyToSectors();

			this.DialogResult = DialogResult.OK;
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void ceiling_CheckedChanged(object sender, EventArgs e)
		{
			ceilingz.Enabled = ceiling.Checked;
		}

		private void floor_CheckedChanged(object sender, EventArgs e)
		{
			floorz.Enabled = floor.Checked;
		}
	}
}
