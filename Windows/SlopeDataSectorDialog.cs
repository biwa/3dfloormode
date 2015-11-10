using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public partial class SlopeDataSectorDialog : Form
	{
		public SlopeDataSectorDialog()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (General.Map.Map.SelectedSectorsCount == 0)
				MessageBox.Show("No sectors selected. Please select exactly one sector", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else if (General.Map.Map.SelectedSectorsCount > 1)
				MessageBox.Show(General.Map.Map.SelectedSectorsCount.ToString() + " sectors selected. Please select exactly one sector", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else
			{
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
		}

		private void SlopeDataSectorDialog_Load(object sender, EventArgs e)
		{
			webBrowser1.DocumentText = string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"), @"
<style>
	body {{ background-color:rgb({0},{1},{2}); font-family:{3}; font-size:12 }}
</style>

The map does not contain a slope data sector. This sector is required by the slope mode to keep track of undo and redo operations.

You have to options:
<ul>
<li>Use selected sector: usese the currently selected sector to store slope data. <span style='color:red'>Make sure that this sector is only
used for this purpose! Do not edit or delete it!</span></li>
<li>Create sector in CSA: automatically created a new sector in the control sector area. <span style='color:red'>Do not edit or delete it!</span></li>
</ul>", this.BackColor.R, this.BackColor.G, this.BackColor.B, this.Font.Name);
		}

		private void createnewsector_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Not yet implemented.");
		}
	}
}
