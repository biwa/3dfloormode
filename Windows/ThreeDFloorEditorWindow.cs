using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
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
		List<Sector> selectedsectors;

		public List<Sector> SelectedSectors { get { return selectedsectors; } }

		public List<ThreeDFloor> ThreeDFloors { get { return threedfloors; } set { threedfloors = value; } }

		public ThreeDFloorEditorWindow()
		{
			this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width/2, Screen.PrimaryScreen.WorkingArea.Height/2);
			InitializeComponent();
		}

		private void ThreeDFloorEditorWindow_Load(object sender, EventArgs e)
		{
			selectedsectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));
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
			var controls = new List<ThreeDFloorHelperControl>();
			int startat = 1;
			int numsplits = 0;

			for (int i = 0; i < ctrl.checkedListBoxSectors.Items.Count; i++)
			{
				// if (ctrl.checkedListBoxSectors.GetItemChecked(i))
				if(ctrl.checkedListBoxSectors.GetItemCheckState(i) == CheckState.Checked)
					items.Add(i);
			}

			int useitem = items.Count - 1;

			/*
			Case 1: all tagged sectors are also selected sectors. In this case we can reuse
				the original control, so one less additional control is needed
			Case 2: multiple tagged sectors are also selected sectors. In this case we can
				reuse the original control, so one less additional control is needed
			Case 3: only one tagged sector is also the selected sector. In this case we
				have to add exactly one additional control
			*/

			controls.Add(ctrl);

			if (items.Count == 1)
			{
				numsplits = 1;
			}
			else
			{
				numsplits = items.Count - 1;
			}

			if (items.Count == 1)
				startat = 0;

			// Start at 1 because we can keep the original 3D floor and just uncheck all but
			// one checkbox
			/*
			for (int i = 0; i < numsplits; i++)
			{
				ThreeDFloorHelperControl split = new ThreeDFloorHelperControl(ctrl);

				for (int j = 1; j < split.checkedListBoxSectors.Items.Count; j++)
				{
					split.checkedListBoxSectors.SetItemChecked(j, false);
				}

				split.checkedListBoxSectors.SetItemChecked(items[i], true);

				threeDFloorPanel.Controls.Add(split);

				threeDFloorPanel.ScrollControlIntoView(split);
			}
			*/

			for (int i = 0; i < numsplits; i++)
			{
				var newctrl = new ThreeDFloorHelperControl(ctrl);
				controls.Add(newctrl);
				threeDFloorPanel.Controls.Add(newctrl);
			}

			for (int i = controls.Count - 1; i >= 0 ; i--)
			{
				for (int j = 0; j < items.Count; j++)
				{
					controls[i].checkedListBoxSectors.SetItemChecked(j, false);
				}

				if (useitem >= 0)
					controls[i].checkedListBoxSectors.SetItemChecked(items[useitem], true);

				useitem--;
			}

			/*

			if (items.Count == 1)
			{
				ctrl.checkedListBoxSectors.SetItemChecked(items[0], false);
			}
			else
			{
				// Uncheck the checkboxes from the original 3D floor
				for (int i = 1; i < items.Count; i++)
				{
					ctrl.checkedListBoxSectors.SetItemChecked(items[i], false);
				}
			}
			*/
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
			Regex r = new Regex(@"\d+");

			if (selectedSectors.Count > 1 && sharedThreeDFloorsCheckBox.Checked)
			{
				var hideControls = new List<ThreeDFloorHelperControl>();

				foreach(ThreeDFloorHelperControl ctrl in threeDFloorPanel.Controls)
				{
					bool allChecked = true;

					// Ignore new 3D floors
					if (ctrl.IsNew)
						continue;

					foreach (int s in ctrl.CheckedSectors)
					{
						foreach (ThreeDFloorHelperControl tdfhc in threeDFloorPanel.Controls)
						{
							if (ctrl == tdfhc)
								continue;

							if (!tdfhc.CheckedSectors.Contains(s))
							{
								allChecked = false;
								break;
							}
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
