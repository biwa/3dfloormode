using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public partial class SlopeVertexEditForm : Form
	{
		private List<SlopeVertex> vertices;
		private List<Sector> sectors;
		private string undodescription;
		private bool canaddsectors;
		private bool canremovesectors;

		public SlopeVertexEditForm()
		{
			InitializeComponent();
		}

		public void Setup(List<SlopeVertex> vertices)
		{
			this.vertices = vertices;

			SlopeVertex fv = vertices[0];
			SlopeVertexGroup fsvg = BuilderPlug.Me.GetSlopeVertexGroup(fv);

			sectors = new List<Sector>();

			undodescription = "Edit slope vertex";

			if (vertices.Count > 1)
				undodescription = "Edit " + vertices.Count + " slope vertices";
			
			positionx.Text = fv.Pos.x.ToString();
			positiony.Text = fv.Pos.y.ToString();
			positionz.Text = fv.Z.ToString();

			if (fsvg.Floor)
				planetype.Text = "Floor";
			else
				planetype.Text = "Ceiling";

			foreach (Sector s in fsvg.Sectors)
				if (!sectors.Contains(s))
					sectors.Add(s);

			reposition.Checked = fsvg.Reposition;

			canaddsectors = true;
			canremovesectors = true;

			if (vertices.Count > 1)
			{
				List<SlopeVertexGroup> listsvgs = new List<SlopeVertexGroup>();

				this.Text = "Edit slope vertices (" + vertices.Count.ToString() + ")";

				foreach (SlopeVertex sv in vertices)
				{
					SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(sv);

					if (!listsvgs.Contains(svg))
						listsvgs.Add(svg);

					if (sv.Pos.x.ToString() != positionx.Text)
						positionx.Text = "";

					if (sv.Pos.y.ToString() != positiony.Text)
						positiony.Text = "";

					if (sv.Z.ToString() != positionz.Text)
						positionz.Text = "";

					if ((svg.Floor != fsvg.Floor || svg.Ceiling != fsvg.Ceiling) && (string)planetype.Items[0] != "")
					{
						planetype.Items.Insert(0, "");
						planetype.SelectedIndex = 0;
					}

					if (svg.Reposition != reposition.Checked)
					{
						reposition.CheckState = CheckState.Indeterminate;
					}

					foreach (Sector s in svg.Sectors)
						if (!sectors.Contains(s))
							sectors.Add(s);
				}

				// Only allow adding/removing sectors if the plane type of the selected SVGs don't clash.
				// For example you can't have two floor slopes applied to one sector
				if (listsvgs.Count == 2)
				{
					if (listsvgs[0].Floor == listsvgs[1].Floor || listsvgs[0].Ceiling == listsvgs[1].Ceiling)
					{
						canaddsectors = false;
						canremovesectors = false;
					}
				}
				else if (listsvgs.Count > 2)
				{
					canaddsectors = false;
					canremovesectors = false;
				}
			}

			foreach (Sector s in sectors.OrderBy(x => x.Index))
			{
				checkedListBoxSectors.Items.Add(s);
			}

			if (General.Map.Map.SelectedSectorsCount == 0)
			{
				addselectedsectors.Enabled = false;
				removeselectedsectors.Enabled = false;
			}
			else
			{
				addselectedsectors.Enabled = canaddsectors;
				removeselectedsectors.Enabled = canremovesectors;
			}
		}

		private void apply_Click(object sender, EventArgs e)
		{
			List<SlopeVertexGroup> groups = new List<SlopeVertexGroup>();

			// undodescription was set in the Setup method
			General.Map.UndoRedo.CreateUndo(undodescription);

			foreach (SlopeVertex sv in vertices)
			{
				SlopeVertexGroup svg = BuilderPlug.Me.GetSlopeVertexGroup(sv);
				float x = positionx.GetResultFloat(sv.Pos.x);
				float y = positiony.GetResultFloat(sv.Pos.y);

				sv.Pos = new Vector2D(x, y);

				sv.Z = positionz.GetResultFloat(sv.Z);

				if (planetype.Text != "")
				{
					svg.Floor = planetype.Text == "Floor" ? true : false;
					svg.Ceiling = !svg.Floor;
				}

				if (!groups.Contains(svg))
					groups.Add(svg);
			}

			foreach (SlopeVertexGroup svg in groups)
			{
				if (reposition.CheckState != CheckState.Indeterminate)
					svg.Reposition = reposition.Checked;

				if (addselectedsectors.Checked)
					foreach (Sector s in General.Map.Map.GetSelectedSectors(true).ToList())
						if (!svg.Sectors.Contains(s))
							svg.Sectors.Add(s);

				if (removeselectedsectors.Checked)
					foreach (Sector s in General.Map.Map.GetSelectedSectors(true).ToList())
						if (svg.Sectors.Contains(s))
						{
							svg.RemoveFromSector(s);
							svg.Sectors.Remove(s);
						}

				foreach (Sector s in checkedListBoxSectors.CheckedItems)
				{
					if (svg.Sectors.Contains(s))
					{
						svg.RemoveFromSector(s);
						svg.Sectors.Remove(s);
					}
				}
					
				svg.ApplyToSectors();
			}

			BuilderPlug.Me.StoreSlopeVertexGroupsInSector();

			this.DialogResult = DialogResult.OK;
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void addselectedsectors_CheckedChanged(object sender, EventArgs e)
		{
			// Adding and removing selected sectors at the same time doesn't make sense,
			// so make sure only one of the checkboxes is checked at most
			if (addselectedsectors.Checked)
				removeselectedsectors.Checked = false;
		}

		private void removeselectedsectors_CheckedChanged(object sender, EventArgs e)
		{
			// Adding and removing selected sectors at the same time doesn't make sense,
			// so make sure only one of the checkboxes is checked at most
			if (removeselectedsectors.Checked)
				addselectedsectors.Checked = false;
		}
	}
}
