
#region ================== Copyright (c) 2014 Boris Iwanski

/*
 * Copyright (c) 2014 Boris Iwanski
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Linq;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using System.Drawing;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.BuilderModes;
using CodeImp.DoomBuilder.GZBuilder.Tools;
using CodeImp.DoomBuilder.GZBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorHelper
{
	//
	// MANDATORY: The plug!
	// This is an important class to the Doom Builder core. Every plugin must
	// have exactly 1 class that inherits from Plug. When the plugin is loaded,
	// this class is instantiated and used to receive events from the core.
	// Make sure the class is public, because only public classes can be seen
	// by the core.
	//

	public class BuilderPlug : Plug
	{
		#region ================== Variables

		private bool additiveselect;
		private bool autoclearselection;
		private MenusForm menusform;
		private bool usehighlight;
		private bool viewselectionnumbers;
		private ControlSectorArea controlsectorarea;

		#endregion

		#region ================== Properties

		public bool AdditiveSelect { get { return additiveselect; } }
		public bool AutoClearSelection { get { return autoclearselection; } }
		public MenusForm MenusForm { get { return menusform; } }
		public bool ViewSelectionNumbers { get { return viewselectionnumbers; } set { viewselectionnumbers = value; } }
		public bool UseHighlight
		{
			get
			{
				return usehighlight;
			}
			set
			{
				usehighlight = value;
				General.Map.Renderer3D.ShowSelection = usehighlight;
				General.Map.Renderer3D.ShowHighlight = usehighlight;
			}
		}
		public ControlSectorArea ControlSectorArea { get { return controlsectorarea; } }

		#endregion

 		// Static instance. We can't use a real static class, because BuilderPlug must
		// be instantiated by the core, so we keep a static reference. (this technique
		// should be familiar to object-oriented programmers)
		private static BuilderPlug me;

		// Lines tagged to the selected sectors
		private static List<Linedef> taggedLines = new List<Linedef>();
		private static List<ThreeDFloor> threeDFloors = new List<ThreeDFloor>();
		private static List<Sector> selectedSectors;
		private static ThreeDFloorEditorWindow tdfew = new ThreeDFloorEditorWindow();

		public static List<Linedef> TaggedLines { get { return taggedLines; } set { taggedLines = value; } }
		public static List<Sector> SelectedSectors { get { return selectedSectors; } set { selectedSectors = value; } }
		public static List<ThreeDFloor> ThreeDFloors { get { return threeDFloors; } set { threeDFloors = value; } }
		public static ThreeDFloorEditorWindow TDFEW { get { return tdfew; } }

		// Static property to access the BuilderPlug
		public static BuilderPlug Me { get { return me; } }

      	// This plugin relies on some functionality that wasn't there in older versions
		public override int MinimumRevision { get { return 1310; } }

		// This event is called when the plugin is initialized
		public override void OnInitialize()
		{
			base.OnInitialize();

			usehighlight = true;

			LoadSettings();

			controlsectorarea = new ControlSectorArea(-512, 0, 512, 0, -128, -64, 128, 64, 64, 56);

			// This binds the methods in this class that have the BeginAction
			// and EndAction attributes with their actions. Without this, the
			// attributes are useless. Note that in classes derived from EditMode
			// this is not needed, because they are bound automatically when the
			// editing mode is engaged.
            General.Actions.BindMethods(this);

  			// TODO: Add DB2 version check so that old DB2 versions won't crash
			// General.ErrorLogger.Add(ErrorType.Error, "zomg!");

			// Keep a static reference
            me = this;
		}

		// This is called when the plugin is terminated
		public override void Dispose()
		{
			base.Dispose();

			// This must be called to remove bound methods for actions.
            General.Actions.UnbindMethods(this);
        }

        #region ================== Actions

		
		// [BeginAction("threedfloorhelper")]
		public void ThreeDFloorEditor()
		{
			// Only make this action possible if a map is actually opened and the we are in sectors mode
			/*
			if (General.Editing.Mode == null || General.Editing.Mode.Attributes.SwitchAction != "sectorsmode")
			{
				return;
			}
			*/

			selectedSectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));

			if (selectedSectors.Count <= 0 && General.Editing.Mode.HighlightedObject is Sector)
				selectedSectors.Add((Sector)General.Editing.Mode.HighlightedObject);

			if (selectedSectors.Count <= 0)
			{
				// Show a warning in the status bar
				General.Interface.DisplayStatus(StatusType.Warning, "Please highlight a sector to copy the properties from");
				return;
			}

			taggedLines = GetTaggedLinedefs(selectedSectors).OrderByDescending(o => o.Front.Sector.CeilHeight).ToList();
			threeDFloors = GetThreeDFloors(selectedSectors);

			tdfew.ShowDialog((Form)General.Interface);
		}

        #endregion

		#region ================== Methods

		// Use the same settings as the BuilderModes plugin
		private void LoadSettings()
		{
			additiveselect = General.Settings.ReadPluginSetting("BuilderModes", "additiveselect", false);
			autoclearselection = General.Settings.ReadPluginSetting("BuilderModes", "autoclearselection", false);
			viewselectionnumbers = General.Settings.ReadPluginSetting("BuilderModes", "viewselectionnumbers", true);
		}

		public static void ProcessSectors(Control.ControlCollection controls)
		{
			var sectorsByTag = new Dictionary<int, List<Sector>>();
			var sectorsToThreeDFloors = new Dictionary<Sector, List<ThreeDFloor>>();
			var sectorGroups = new List<List<Sector>>();
			var tmpSelectedSectors = new List<Sector>(selectedSectors);

			General.Map.UndoRedo.CreateUndo("Modify 3D floors");

			foreach (ThreeDFloorHelperControl ctrl in controls)
			{
				ctrl.ApplyToThreeDFloor();

				if (ctrl.IsNew)
					if(ctrl.ThreeDFloor.CreateGeometry())
						ctrl.ThreeDFloor.UpdateGeometry();
			}

			// Fill the sectorsToThreeDFloors dictionary, with a selected sector as key
			// and a list of all 3D floors, that should be applied to to this sector, as value
			foreach(Sector s in selectedSectors)
			{
				if (!sectorsToThreeDFloors.ContainsKey(s))
					sectorsToThreeDFloors.Add(s, new List<ThreeDFloor>());

				foreach (ThreeDFloorHelperControl ctrl in controls)
				{
					for (int i = 0; i < ctrl.checkedListBoxSectors.Items.Count; i++)
					{
						string text = ctrl.checkedListBoxSectors.Items[i].ToString();
						bool ischecked = ctrl.checkedListBoxSectors.GetItemChecked(i);

						if (ischecked && text == "Sector " + s.Index.ToString())
						{
							sectorsToThreeDFloors[s].Add(ctrl.ThreeDFloor);
						}
					}
				}
			}

			// Group all selected sectors by their 3D floors. I.e. each element of sectorGroups
			// is a list of sectors that have the same 3D floors
			while (tmpSelectedSectors.Count > 0)
			{
				Sector s1 = tmpSelectedSectors.First();
				var list = new List<Sector>();
				var delsectors = new List<Sector>();

				foreach (Sector s2 in tmpSelectedSectors)
				{
					if (sectorsToThreeDFloors[s1].ContainsAllElements(sectorsToThreeDFloors[s2]))
					{
						list.Add(s2);
						delsectors.Add(s2);
					}
				}

				foreach(Sector s in delsectors)
					tmpSelectedSectors.Remove(s);

				tmpSelectedSectors.Remove(s1);

				sectorGroups.Add(list);
			}

			// Bind the 3D floors to the selected sectors
			foreach (List<Sector> sectors in sectorGroups)
			{
				int newtag;

				// Just use sectors.First(), all elements in sectors have the same 3D floors anyway
				// If there are no 3D floors associated set the tag to 0
				if (sectorsToThreeDFloors[sectors.First()].Count == 0)
					newtag = 0;
				else
					newtag = BuilderPlug.me.ControlSectorArea.GetNewTag();

				foreach (Sector s in sectors)
					s.Tag = newtag;
								
				foreach (ThreeDFloor tdf in sectorsToThreeDFloors[sectors.First()])
					tdf.BindTag(newtag);
			}

			// Remove unused tags from the 3D floors
			foreach (ThreeDFloor tdf in threeDFloors)
				tdf.Cleanup();
		}

		public static List<Linedef> GetTaggedLinedefs(List<Sector> sectors)
		{
			List<Linedef> linedefs = new List<Linedef>();

			foreach (Linedef ld in General.Map.Map.Linedefs)
				if (ld.Action == 160)
					foreach (Sector s in sectors)
						if (ld.Args[0] == s.Tag)
							linedefs.Add(ld);

			return linedefs;
		}

		public static List<ThreeDFloor> GetThreeDFloors(List<Sector> sectors)
		{
			List<ThreeDFloor> tdf = new List<ThreeDFloor>();
			List<Sector> tmpsectors = new List<Sector>();

			// Immediately return if the list is empty
			if (sectors.Count == 0)
				return tdf;

			foreach (Linedef ld in General.Map.Map.Linedefs)
				if (ld.Action == 160)
					foreach (Sector s in sectors)
						if (s != null && ld.Args[0] == s.Tag && !tmpsectors.Contains(ld.Front.Sector))
							tmpsectors.Add(ld.Front.Sector);
							
			foreach(Sector s in tmpsectors)
				if(s != null)
					tdf.Add(new ThreeDFloor(s));

			return tdf;
		}

		private List<Sector> GetControlSectors(List<Sector> sectors)
		{
			List<Sector> controlsectors = new List<Sector>();

			foreach (Linedef ld in General.Map.Map.Linedefs)
				if (ld.Action == 160)
					foreach (Sector s in sectors)
						if (ld.Args[0] == s.Tag)
							controlsectors.Add(ld.Front.Sector);

			return controlsectors;
		}

		private static List<Sector> GetControlSectors(int tag)
		{
			List<Sector> controlsectors = new List<Sector>();

			foreach (Linedef ld in General.Map.Map.Linedefs)
				if (ld.Action == 160)
					if (ld.Args[0] == tag && !controlsectors.Contains(ld.Front.Sector))
						controlsectors.Add(ld.Front.Sector);

			return controlsectors;
		}

		#endregion
	}

	public static class ThreeDFloorHelpers
	{
		public static List<Sector> GetSectorsByTag(this MapSet ms, int tag)
		{
			List<Sector> sectors = new List<Sector>();

			foreach (Sector s in ms.Sectors)
				if (s.Tag == tag)
					sectors.Add(s);

			return sectors;
		}

		public static bool ContainsAllElements<T>(this List<T> list1, List<T> list2)
		{
			if (list1.Count != list2.Count)
				return false;

			foreach (T i in list1)
				if (!list2.Contains(i))
					return false;

			return true;
		}
	}
}
