using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
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
	public partial class ThreeDFloorHelperControl : UserControl
	{
		private ThreeDFloor threeDFloor;
		public Linedef linedef;
		private bool isnew;
		private Sector sector;
		private bool settingup;
		private List<int> checkedsectors;

		public ThreeDFloor ThreeDFloor { get { return threeDFloor; } }
		public bool IsNew { get { return isnew; } }
		public Sector Sector { get { return sector; } }
		public List<int> CheckedSectors { get { return checkedsectors; } }

		// Create the control from an existing linedef
		public ThreeDFloorHelperControl(ThreeDFloor threeDFloor)
		{
			InitializeComponent();

			isnew = false;
			settingup = true;

			this.threeDFloor = threeDFloor;

			sectorBorderTexture.TextureName = threeDFloor.BorderTexture;
			sectorTopFlat.TextureName = threeDFloor.TopFlat;
			sectorBottomFlat.TextureName = threeDFloor.BottomFlat;
			sectorCeilingHeight.Text = threeDFloor.TopHeight.ToString();
			sectorFloorHeight.Text = threeDFloor.BottomHeight.ToString();

			typeArgument.Setup(General.Map.Config.LinedefActions[160].Args[1]);
			flagsArgument.Setup(General.Map.Config.LinedefActions[160].Args[2]);
			alphaArgument.Setup(General.Map.Config.LinedefActions[160].Args[3]);

			typeArgument.SetValue(threeDFloor.Type);
			flagsArgument.SetValue(threeDFloor.Flags);
			alphaArgument.SetValue(threeDFloor.Alpha);

			AddSectorCheckboxes();

			sector = General.Map.Map.CreateSector();

			if (threeDFloor.Sector != null)
				threeDFloor.Sector.CopyPropertiesTo(sector);

			settingup = false;
		}

		// Create a duplicate of the given control
		public ThreeDFloorHelperControl(ThreeDFloorHelperControl ctrl) : this()
		{
			settingup = true;

			sectorBorderTexture.TextureName = threeDFloor.BorderTexture = ctrl.threeDFloor.BorderTexture;
			sectorTopFlat.TextureName = threeDFloor.TopFlat = ctrl.threeDFloor.TopFlat;
			sectorBottomFlat.TextureName = threeDFloor.BottomFlat = ctrl.threeDFloor.BottomFlat;
			sectorCeilingHeight.Text = ctrl.threeDFloor.TopHeight.ToString();
			sectorFloorHeight.Text = ctrl.threeDFloor.BottomHeight.ToString();

			threeDFloor.TopHeight = ctrl.threeDFloor.TopHeight;
			threeDFloor.BottomHeight = ctrl.threeDFloor.BottomHeight;

			typeArgument.SetValue(ctrl.threeDFloor.Type);
			flagsArgument.SetValue(ctrl.threeDFloor.Flags);
			alphaArgument.SetValue(ctrl.threeDFloor.Alpha);

			for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
				checkedListBoxSectors.SetItemChecked(i, ctrl.checkedListBoxSectors.GetItemChecked(i));

			settingup = false;
		}

		// Create a blank control for a new 3D floor
		public ThreeDFloorHelperControl()
		{
			InitializeComponent();

			isnew = true;
			settingup = true;

			threeDFloor = new ThreeDFloor();

			this.BackColor = Color.FromArgb(128, Color.Green);

			sectorBorderTexture.TextureName = General.Settings.DefaultTexture;
			sectorTopFlat.TextureName = General.Settings.DefaultCeilingTexture;
			sectorBottomFlat.TextureName = General.Settings.DefaultFloorTexture;
			sectorCeilingHeight.Text = General.Settings.DefaultCeilingHeight.ToString();
			sectorFloorHeight.Text = General.Settings.DefaultFloorHeight.ToString();

			typeArgument.Setup(General.Map.Config.LinedefActions[160].Args[1]);
			flagsArgument.Setup(General.Map.Config.LinedefActions[160].Args[2]);
			alphaArgument.Setup(General.Map.Config.LinedefActions[160].Args[3]);

			typeArgument.SetDefaultValue();
			flagsArgument.SetDefaultValue();
			alphaArgument.SetDefaultValue();

			AddSectorCheckboxes();

			for(int i=0; i < checkedListBoxSectors.Items.Count; i++)
				checkedListBoxSectors.SetItemChecked(i, true);

			sector = General.Map.Map.CreateSector();

			settingup = false;
		}

		public void ApplyToThreeDFloor()
		{
			Regex r = new Regex(@"\d+");
			bool guessslope = false;

			threeDFloor.TopHeight = sectorCeilingHeight.GetResult(threeDFloor.TopHeight);
			threeDFloor.BottomHeight = sectorFloorHeight.GetResult(threeDFloor.BottomHeight);
			threeDFloor.TopFlat = sectorTopFlat.TextureName;
			threeDFloor.BottomFlat = sectorBottomFlat.TextureName;
			threeDFloor.BorderTexture = sectorBorderTexture.TextureName;

			threeDFloor.Type = int.Parse(typeArgument.Text);
			threeDFloor.Flags = int.Parse(flagsArgument.Text);
			threeDFloor.Alpha = int.Parse(alphaArgument.Text);

			threeDFloor.IsNew = isnew;

			if(threeDFloor.Sector != null)
				sector.CopyPropertiesTo(threeDFloor.Sector);

			threeDFloor.TaggedSectors = new List<Sector>();

			for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
			{
				string text = checkedListBoxSectors.Items[i].ToString();
				bool ischecked = !(checkedListBoxSectors.GetItemCheckState(i) == CheckState.Unchecked);

				if (ischecked)
				{
					var matches = r.Matches(text);
					Sector s = General.Map.Map.GetSectorByIndex(int.Parse(matches[0].ToString()));
					threeDFloor.TaggedSectors.Add(s);
				}
			}

			if (guessslope)
			{
				threeDFloor.GuessSlopeVector();
			}
		}

		private void AddSectorCheckboxes()
		{
			List<Sector> sectors = new List<Sector>(BuilderPlug.TDFEW.SelectedSectors);

			checkedsectors = new List<int>();

			foreach (Sector s in ThreeDFloor.TaggedSectors)
			{
				if (!sectors.Contains(s))
					sectors.Add(s);
			}

			if (sectors == null)
				return;

			foreach (Sector s in sectors)
			{
				int i = checkedListBoxSectors.Items.Add("Sector " + s.Index.ToString(), ThreeDFloor.TaggedSectors.Contains(s));

				if(ThreeDFloor.TaggedSectors.Contains(s))
					checkedsectors.Add(s.Index);

				if (!BuilderPlug.TDFEW.SelectedSectors.Contains(s))
				{
					checkedListBoxSectors.SetItemCheckState(i, CheckState.Indeterminate);
				}
			}
		}

		private void buttonDuplicate_Click(object sender, EventArgs e)
		{
			((ThreeDFloorEditorWindow)this.ParentForm).DuplicateThreeDFloor(this);
		}

		private void buttonSplit_Click(object sender, EventArgs e)
		{
			((ThreeDFloorEditorWindow)this.ParentForm).SplitThreeDFloor(this);
		}

		private void buttonCheckAll_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
				checkedListBoxSectors.SetItemChecked(i, true);
		}

		private void buttonUncheckAll_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
				checkedListBoxSectors.SetItemChecked(i, false);
		}

		private void buttonEditSector_Click(object sender, EventArgs e)
		{
			sector.SetCeilTexture(sectorTopFlat.TextureName);
			sector.SetFloorTexture(sectorBottomFlat.TextureName);
			sector.CeilHeight = sectorCeilingHeight.GetResult(sector.CeilHeight);
			sector.FloorHeight = sectorFloorHeight.GetResult(sector.FloorHeight);

			DialogResult result = General.Interface.ShowEditSectors(new List<Sector> { sector });

			if (result == DialogResult.OK)
			{
				sectorTopFlat.TextureName = sector.CeilTexture;
				sectorBottomFlat.TextureName = sector.FloorTexture;
				sectorCeilingHeight.Text = sector.CeilHeight.ToString();
				sectorFloorHeight.Text = sector.FloorHeight.ToString();
			}
		}

		private void checkedListBoxSectors_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (e.CurrentValue == CheckState.Indeterminate)
			{
				e.NewValue = CheckState.Indeterminate;
			}
			else
			{
				Regex r = new Regex(@"\d+");

				if (((ListBox)sender).SelectedItem == null)
					return;

				var matches = r.Matches(((ListBox)sender).SelectedItem.ToString());

				int sectornum = int.Parse(matches[0].ToString());

				if (e.NewValue == CheckState.Checked)
					checkedsectors.Add(sectornum);
				else
					checkedsectors.Remove(sectornum);
			}
		}

        private void bottomSlopeHeight_WhenTextChanged(object sender, EventArgs e)
        {
            if (!settingup)
            {
                threeDFloor.Rebuild = true;
            }
        }

        private void topSlopeHeight_WhenTextChanged(object sender, EventArgs e)
        {
            if (!settingup)
            {
                threeDFloor.Rebuild = true;
            }
        }

		private void buttonDrawSlope_Click(object sender, EventArgs e)
		{
			General.Editing.ChangeMode("DrawSlopesMode");
		}
	}
}
