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
			sharedThreeDFloorsCheckBox.Checked = false;
			FillThreeDFloorPanel(threedfloors);
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			threedfloors = new List<ThreeDFloor>();

			foreach (ThreeDFloorHelperControl ctrl in threeDFloorPanel.Controls.OfType<ThreeDFloorHelperControl>())
			{
				ctrl.ApplyToThreeDFloor();
				threedfloors.Add(ctrl.ThreeDFloor);
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		public void addThreeDFloorButton_Click(object sender, EventArgs e)
		{
			ThreeDFloorHelperControl ctrl = new ThreeDFloorHelperControl();

			no3dfloorspanel.Hide();

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
			ClearThreeDFloorPanel();

			if (threedfloors.Count > 0)
			{
				// Create a new controller instance for each linedef and set its properties
				foreach (ThreeDFloor tdf in threedfloors.OrderByDescending(o => o.TopHeight).ToList())
				{
					ThreeDFloorHelperControl ctrl = new ThreeDFloorHelperControl(tdf);

					threeDFloorPanel.Controls.Add(ctrl);
				}

				no3dfloorspanel.Hide();
			}
			else
			{
				no3dfloorspanel.Show();
			}
		}

		private void ClearThreeDFloorPanel()
		{
			// Get rid of dummy sectors and clear existing controls
			foreach (ThreeDFloorHelperControl ctrl in threeDFloorPanel.Controls.OfType<ThreeDFloorHelperControl>().ToList())
			{
				ctrl.Sector.Dispose();
				threeDFloorPanel.Controls.Remove(ctrl);
			}
		}

		private void sharedThreeDFloorsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			ICollection<Sector> selectedSectors = General.Map.Map.GetSelectedSectors(true);

			if (selectedSectors.Count > 1 && sharedThreeDFloorsCheckBox.Checked)
			{
				var hideControls = new List<ThreeDFloorHelperControl>();

				foreach (Sector s in selectedSectors)
				{
					foreach (ThreeDFloorHelperControl ctrl in threeDFloorPanel.Controls.OfType<ThreeDFloorHelperControl>())
					{
						// If the selected sector is not in the control's tagged sectors the control
						// should be hidden
						if (!ctrl.ThreeDFloor.TaggedSectors.Contains(s))
							hideControls.Add(ctrl);
					}
				}

				foreach (ThreeDFloorHelperControl ctrl in hideControls)
				{
					// Hide controls, unless they are new
					if(ctrl.IsNew == false)
						ctrl.Visible = false;
				}
			}
			else
			{
				foreach (ThreeDFloorHelperControl ctrl in threeDFloorPanel.Controls.OfType<ThreeDFloorHelperControl>())
					ctrl.Show();
			}
		}

		private void ThreeDFloorEditorWindow_FormClosed(object sender, FormClosedEventArgs e)
		{
			ClearThreeDFloorPanel();
		}
	}
}
