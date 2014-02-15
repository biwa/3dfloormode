using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	public partial class ThreeDFloorEditorWindow : Form
	{
		List<ThreeDFloor> threedfloors;

		public List<ThreeDFloor> ThreeDFloors { get { return threedfloors; } set { threedfloors = value; } }

		public ThreeDFloorEditorWindow()
		{
			this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width/2, Screen.PrimaryScreen.WorkingArea.Height/2);
			InitializeComponent();
		}

		private void ThreeDFloorEditorWindow_Load(object sender, EventArgs e)
		{
			FillThreeDFloorPanel(threedfloors);
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			threedfloors = new List<ThreeDFloor>();

			foreach (ThreeDFloorHelperControl ctrl in threeDFloorPanel.Controls)
			{
				ctrl.ApplyToThreeDFloor();
				threedfloors.Add(ctrl.ThreeDFloor);
				ctrl.Sector.Dispose();
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			foreach (ThreeDFloorHelperControl ctrl in threeDFloorPanel.Controls)
				ctrl.Sector.Dispose();

			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void addThreeDFloorButton_Click(object sender, EventArgs e)
		{
			ThreeDFloorHelperControl ctrl = new ThreeDFloorHelperControl();

			threeDFloorPanel.Controls.Add(ctrl);

			threeDFloorPanel.ScrollControlIntoView(ctrl);
		}

		public void DuplicateThreeDFloor(ThreeDFloorHelperControl ctrl)
		{
			ThreeDFloorHelperControl dup = new ThreeDFloorHelperControl(ctrl);

			threeDFloorPanel.Controls.Add(dup);

			threeDFloorPanel.ScrollControlIntoView(dup);
		}

		public void SplitThreeDFloor(ThreeDFloorHelperControl ctrl)
		{
			var items = new List<int>();

			for (int i = 0; i < ctrl.checkedListBoxSectors.Items.Count; i++)
			{
				if (ctrl.checkedListBoxSectors.GetItemChecked(i))
					items.Add(i);
			}

			// Start at 1 because we can keep the original 3D floor and just uncheck all but
			// one checkbox
			for (int i = 1; i < items.Count; i++)
			{
				ThreeDFloorHelperControl split = new ThreeDFloorHelperControl(ctrl);

				for (int j = 0; j < ctrl.checkedListBoxSectors.Items.Count; j++)
				{
					split.checkedListBoxSectors.SetItemChecked(j, false);
				}

				split.checkedListBoxSectors.SetItemChecked(items[i], true);

				threeDFloorPanel.Controls.Add(split);

				threeDFloorPanel.ScrollControlIntoView(split);
			}

			// Uncheck the checkboxes from the original 3D floor
			for (int i = 1; i < items.Count; i++)
			{
				ctrl.checkedListBoxSectors.SetItemChecked(items[i], false);
			}
		}

		private void FillThreeDFloorPanel(List<ThreeDFloor> threedfloors)
		{
			// Clear all existing controls
			threeDFloorPanel.Controls.Clear();

			// Create a new controller instance for each linedef and set its properties
			foreach (ThreeDFloor tdf in threedfloors.OrderByDescending(o => o.TopHeight).ToList())
			{
				ThreeDFloorHelperControl ctrl = new ThreeDFloorHelperControl(tdf);

				threeDFloorPanel.Controls.Add(ctrl);
			}
		}

		private void sharedThreeDFloorsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			List<Sector> selectedSectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));

			if (selectedSectors.Count > 1 && sharedThreeDFloorsCheckBox.Checked)
			{
				var hideControls = new List<ThreeDFloorHelperControl>();

				foreach(ThreeDFloorHelperControl ctrl in threeDFloorPanel.Controls)
				{
					bool allChecked = true;

					// Ignore new 3D floors
					if (ctrl.IsNew)
						continue;

					for (int i = 0; i < ctrl.checkedListBoxSectors.Items.Count; i++)
					{
						if (!ctrl.checkedListBoxSectors.GetItemChecked(i))
						{
							allChecked = false;
							break;
						}
					}

					if(!allChecked)
						hideControls.Add(ctrl);
				}

				foreach (ThreeDFloorHelperControl ctrl in hideControls)
				{
					ctrl.Visible = false;
				}
			}
			else
			{
				FillThreeDFloorPanel(threedfloors);
			}
		}
	}
}
