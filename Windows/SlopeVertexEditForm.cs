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

			string desc = "slope vertex";
			if (vertices.Count > 1) desc = vertices.Count + " slope vertices";
			General.Map.UndoRedo.CreateUndo("Edit " + desc);

			positionx.Text = fv.Pos.x.ToString();
			positiony.Text = fv.Pos.y.ToString();

			positionz.Text = fv.Z.ToString();

			if (fsvg.Floor)
				planetype.Text = "Floor";
			else
				planetype.Text = "Ceiling";

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

					if (sv.Z.ToString() != positionz.Text)
						positionz.Text = "";
				}
			}
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

				if (planetype.Text != "")
				{
					svg.Floor = planetype.Text == "Floor" ? true : false;
					svg.Ceiling = !svg.Floor;

					sv.Z = positionz.GetResultFloat(sv.Z);
				}

				if (!groups.Contains(svg))
					groups.Add(svg);
			}

			foreach (SlopeVertexGroup svg in groups)
				svg.ApplyToSectors();

			BuilderPlug.Me.StoreSlopeVertexGroupsInSector();

			this.DialogResult = DialogResult.OK;
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			General.Map.UndoRedo.WithdrawUndo();

			this.DialogResult = DialogResult.Cancel;
		}
	}
}
