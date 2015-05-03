
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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
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
using CodeImp.DoomBuilder.VisualModes;

#endregion

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
	//
	// MANDATORY: The plug!
	// This is an important class to the Doom Builder core. Every plugin must
	// have exactly 1 class that inherits from Plug. When the plugin is loaded,
	// this class is instantiated and used to receive events from the core.
	// Make sure the class is public, because only public classes can be seen
	// by the core.
	//
	[Flags]
	public enum PlaneType
	{
		Floor = 1,
		Ceiling = 2,
	}

	public class BuilderPlug : Plug
	{
		#region ================== Variables

		private bool additiveselect;
		private bool autoclearselection;
		private MenusForm menusform;
		private bool usehighlight;
		private bool viewselectionnumbers;
		private ControlSectorArea controlsectorarea;
        private float highlightsloperange;
		private List<SlopeVertexGroup> slopevertexgroups;
		private float stitchrange;
		private Sector dummysector;
		private bool updateafteraction;

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
        public float HighlightSlopeRange { get { return highlightsloperange; } }
		public List<SlopeVertexGroup> SlopeVertexGroups { get { return slopevertexgroups; } set { slopevertexgroups = value; } }
		public float StitchRange { get { return stitchrange; } }

		public Sector DummySector { get { return dummysector; } set { dummysector = value; } }

		#endregion

 		// Static instance. We can't use a real static class, because BuilderPlug must
		// be instantiated by the core, so we keep a static reference. (this technique
		// should be familiar to object-oriented programmers)
		private static BuilderPlug me;

		// Lines tagged to the selected sectors
		private static ThreeDFloorEditorWindow tdfew = new ThreeDFloorEditorWindow();

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

			slopevertexgroups = new List<SlopeVertexGroup>();

			//controlsectorarea = new ControlSectorArea(-512, 0, 512, 0, -128, -64, 128, 64, 64, 56);

			// This binds the methods in this class that have the BeginAction
			// and EndAction attributes with their actions. Without this, the
			// attributes are useless. Note that in classes derived from EditMode
			// this is not needed, because they are bound automatically when the
			// editing mode is engaged.
            General.Actions.BindMethods(this);

			menusform = new MenusForm();

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

		public override void OnMapNewEnd()
		{
			base.OnMapNewEnd();

			controlsectorarea = new ControlSectorArea(-512, 0, 512, 0, -128, -64, 128, 64, 64, 56);
			BuilderPlug.Me.ControlSectorArea.LoadConfig();

			slopevertexgroups.Clear();

			// Create the dummy sector used to store the slope vertex group info
			dummysector = General.Map.Map.CreateSector();
			General.Map.Map.Update();
		}

		public override void OnMapOpenEnd()
		{
			base.OnMapOpenEnd();

			controlsectorarea = new ControlSectorArea(-512, 0, 512, 0, -128, -64, 128, 64, 64, 56);
			BuilderPlug.Me.ControlSectorArea.LoadConfig();

			LoadSlopesFromDBS();

			// Create the dummy sector used to store the slope vertex group info
			dummysector = General.Map.Map.CreateSector();
			General.Map.Map.Update();

			foreach (SlopeVertexGroup svg in slopevertexgroups)
				svg.StoreInSector(dummysector);
		}

		// Write the slope data to the .dbs file when the map is saved
		public override void OnMapSaveBegin(SavePurpose purpose)
		{
			base.OnMapSaveBegin(purpose);

			ListDictionary slopedata = new ListDictionary();

			foreach (SlopeVertexGroup svg in BuilderPlug.Me.SlopeVertexGroups)
			{
				ListDictionary data = new ListDictionary();

				if (svg.Floor)
					data.Add("planetype", "floor");
				else
					data.Add("planetype", "ceiling");

				for (int i = 0; i < svg.Vertices.Count; i++)
				{
					string name = String.Format("vertex{0}.", i+1);
					data.Add(name + "x", svg.Vertices[i].Pos.x);
					data.Add(name + "y", svg.Vertices[i].Pos.y);
					data.Add(name + "z", svg.Vertices[i].Z);
				}

				slopedata.Add("slope" + svg.Id.ToString(), data);
			}

			General.Map.Options.WritePluginSetting("slopes", slopedata);
		}

		public override void OnUndoEnd()
		{
			base.OnUndoEnd();

			// Load slope vertex data from the dummy sector
			LoadSlopeVertexGroupsFromSector();
		}

		public override void OnRedoEnd()
		{
			base.OnRedoEnd();

			// Load slope vertex data from the dummy sector
			LoadSlopeVertexGroupsFromSector();
		}
	
		public override void OnActionBegin(CodeImp.DoomBuilder.Actions.Action action)
		{
			base.OnActionBegin(action);

			string[] monitoractions = {
				"buildermodes_raisesector8", "buildermodes_lowersector8", "buildermodes_raisesector1",
				"buildermodes_lowersector1", "builder_visualedit", "builder_classicedit"
			};

			if (monitoractions.Contains(action.Name))
				updateafteraction = true;
			else
				updateafteraction = false;
		}

		public override void OnActionEnd(CodeImp.DoomBuilder.Actions.Action action)
		{
			base.OnActionEnd(action);

			if (!updateafteraction)
				return;

			List<SlopeVertexGroup> updatesvgs = new List<SlopeVertexGroup>();

			// Find SVGs that needs to be updated, and change the SV z positions
			foreach (SlopeVertexGroup svg in slopevertexgroups)
			{
				int diff = 0;
				bool update = false;

				foreach (Sector s in svg.Sectors)
				{
					if (svg.Floor)
					{
						if (svg.Height != s.FloorHeight)
						{
							diff = s.FloorHeight - svg.Height;
							update = true;
							break;
						}
					}
					
					if(svg.Ceiling)
					{
						if (svg.Height != s.CeilHeight)
						{
							diff = s.CeilHeight - svg.Height;
							update = true;
							break;
						}
					}
				}

				if (update)
				{
					foreach (SlopeVertex sv in svg.Vertices)
						sv.Z += diff;

					updatesvgs.Add(svg);
				}
			}

			// Update the slopes, and also update the view if in visual mode
			foreach (SlopeVertexGroup svg in updatesvgs)
			{
				foreach (Sector s in svg.Sectors)
					UpdateSlopes(s);

				if (General.Editing.Mode is BaseVisualMode)
				{
					List<Sector> sectors = new List<Sector>();
					List<VisualSector> visualsectors = new List<VisualSector>();
					BaseVisualMode mode = ((BaseVisualMode)General.Editing.Mode);

					foreach (Sector s in svg.Sectors)
					{
						sectors.Add(s);

						// Get neighbouring sectors and add them to the list
						foreach (Sidedef sd in s.Sidedefs)
						{
							if (sd.Other != null && !sectors.Contains(sd.Other.Sector))
								sectors.Add(sd.Other.Sector);
						}
					}

					foreach (Sector s in sectors)
						visualsectors.Add(mode.GetVisualSector(s));

					foreach (VisualSector vs in visualsectors) vs.UpdateSectorGeometry(true);
					foreach (VisualSector vs in visualsectors) vs.UpdateSectorData();
				}
			}
		}

		public override bool OnModeChange(EditMode oldmode, EditMode newmode)
		{
			if (newmode != null && oldmode != null)
			{
				if (newmode.GetType().Name == "DragSectorsMode")
				{
					foreach (SlopeVertexGroup svg in slopevertexgroups)
						if (svg.Reposition)
							svg.GetAnchor();
				}
				else if(oldmode.GetType().Name == "DragSectorsMode")
				{
					foreach (SlopeVertexGroup svg in slopevertexgroups)
						if (svg.Reposition)
							svg.RepositionByAnchor();
				}

			}

			return base.OnModeChange(oldmode, newmode);
		}

        #region ================== Actions
		
        #endregion

		#region ================== Methods

		public DialogResult ThreeDFloorEditor()
		{
			List<Sector> selectedSectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));

			if (selectedSectors.Count <= 0 && General.Editing.Mode.HighlightedObject is Sector)
				selectedSectors.Add((Sector)General.Editing.Mode.HighlightedObject);

			tdfew.ThreeDFloors = GetThreeDFloors(selectedSectors);

			DialogResult result = tdfew.ShowDialog((Form)General.Interface);

			return result;
		}

		// Use the same settings as the BuilderModes plugin
		private void LoadSettings()
		{
			additiveselect = General.Settings.ReadPluginSetting("BuilderModes", "additiveselect", false);
			autoclearselection = General.Settings.ReadPluginSetting("BuilderModes", "autoclearselection", false);
			viewselectionnumbers = General.Settings.ReadPluginSetting("BuilderModes", "viewselectionnumbers", true);
            highlightsloperange = (float)General.Settings.ReadPluginSetting("BuilderModes", "highlightthingsrange", 10);
			stitchrange = (float)General.Settings.ReadPluginSetting("BuilderModes", "stitchrange", 20);
		}

		private void LoadSlopesFromDBS()
		{
			slopevertexgroups.Clear();

			Regex vertexregex = new Regex(@"vertex(\d)\.(\w+)", RegexOptions.IgnoreCase);
			Regex sloperegex = new Regex(@"slope(\d+)", RegexOptions.IgnoreCase);

			ListDictionary slopedata = (ListDictionary)General.Map.Options.ReadPluginSetting("slopes", new ListDictionary());

			foreach (DictionaryEntry slopeentry in slopedata)
			{
				float[] values = new float[9];
				int counter = 0;

				Match slopematch = sloperegex.Match((string)slopeentry.Key);

				if (slopematch.Success)
				{
					int slopenum = Convert.ToInt32(slopematch.Groups[1].ToString());
					bool floor = false;
					bool ceiling = false;

					foreach (DictionaryEntry entry in (ListDictionary)slopeentry.Value)
					{
						int voffset = 0;
						int fcoffset = 0;

						Match vertexmatch = vertexregex.Match((string)entry.Key);

						if (vertexmatch.Success)
						{
							int pointnum = Convert.ToInt32(vertexmatch.Groups[1].ToString()) - 1;

							if (pointnum > counter)
								counter = pointnum;

							voffset = pointnum * 3;
							fcoffset = pointnum * 2;

							if (vertexmatch.Groups[2].ToString() == "y") voffset += 1;
							if (vertexmatch.Groups[2].ToString() == "z") voffset += 2;

							values[voffset] = (float)entry.Value;
						}
						else if ((string)entry.Key == "planetype")
						{
							if ((string)entry.Value == "floor")
								floor = true;
							else if ((string)entry.Value == "ceiling")
								ceiling = true;
						}
					}

					List<SlopeVertex> vertices = new List<SlopeVertex>();

					for (int i = 0; i <= counter; i++)
					{
						vertices.Add(new SlopeVertex(new Vector2D(values[i * 3], values[i * 3 + 1]), values[i * 3 + 2]));
					}

					slopevertexgroups.Add(new SlopeVertexGroup(slopenum, vertices, floor, ceiling));
				}
			}

			foreach (SlopeVertexGroup svg in slopevertexgroups)
			{
				svg.FindSectors();
			}
		}

		public void StoreSlopeVertexGroupsInSector()
		{
			foreach (SlopeVertexGroup svg in slopevertexgroups)
				svg.StoreInSector(dummysector);
		}

		public void LoadSlopeVertexGroupsFromSector()
		{
			Regex svgregex = new Regex(@"user_svg(\d+)_v0_x", RegexOptions.IgnoreCase);

			slopevertexgroups.Clear();

			foreach (KeyValuePair<string, UniValue> kvp in dummysector.Fields)
			{
				Match svgmatch = svgregex.Match((string)kvp.Key);

				if (svgmatch.Success)
				{
					int svgid = Convert.ToInt32(svgmatch.Groups[1].ToString());

					slopevertexgroups.Add(new SlopeVertexGroup(svgid, dummysector));
				}
			}

			General.Map.Map.Update();
		}

		public void UpdateSlopes()
		{
			foreach (Sector s in General.Map.Map.Sectors)
				UpdateSlopes(s);
		}

		public void UpdateSlopes(Sector s)
		{
			string[] fieldnames = new string[] { "user_floorplane_id", "user_ceilingplane_id" };

			foreach (string fn in fieldnames)
			{
				int id = s.Fields.GetValue(fn, -1);

				if (id == -1)
				{
					if (fn == "user_floorplane_id")
					{
						s.FloorSlope = new Vector3D();
						s.FloorSlopeOffset = 0;
					}
					else
					{
						s.CeilSlope = new Vector3D();
						s.CeilSlopeOffset = 0;
					}

					continue;
				}

				List<Vector3D> sp = new List<Vector3D>();
				SlopeVertexGroup svg = GetSlopeVertexGroup(id);

				for (int i = 0; i < svg.Vertices.Count; i++)
				{
					sp.Add(new Vector3D(svg.Vertices[i].Pos.x, svg.Vertices[i].Pos.y, svg.Vertices[i].Z));
				}

				if (svg.Vertices.Count == 2)
				{
					float z = sp[0].z;
					Line2D line = new Line2D(sp[0], sp[1]);
					Vector3D perpendicular = line.GetPerpendicular();

					Vector2D v = sp[0] + perpendicular;

					sp.Add(new Vector3D(v.x, v.y, z));
				}

				if (fn == "user_floorplane_id")
				{
					Plane p = new Plane(sp[0], sp[1], sp[2], true);

					s.FloorSlope = new Vector3D(p.a, p.b, p.c);
					s.FloorSlopeOffset = p.d;
					s.FloorHeight = Convert.ToInt32(p.GetZ(GetCircumcenter(sp)));
					svg.Height = s.FloorHeight;
					svg.FloorHeight = s.FloorHeight;
				}
				else
				{
					Plane p = new Plane(sp[0], sp[1], sp[2], false);

					s.CeilSlope = new Vector3D(p.a, p.b, p.c);
					s.CeilSlopeOffset = p.d;
					s.CeilHeight = Convert.ToInt32(p.GetZ(GetCircumcenter(sp)));
					svg.Height = s.CeilHeight;
					svg.CeilingHeight = s.CeilHeight;
				}
			}
		}

		private Vector2D GetCircumcenter(List<Vector3D> points)
		{
			float u_ray;

			Line2D line1 = new Line2D(points[0], points[1]);
			Line2D line2 = new Line2D(points[2], points[0]);

			// Perpendicular bisectors
			Line2D bisector1 = new Line2D(line1.GetCoordinatesAt(0.5f), line1.GetCoordinatesAt(0.5f) + line1.GetPerpendicular());
			Line2D bisector2 = new Line2D(line2.GetCoordinatesAt(0.5f), line2.GetCoordinatesAt(0.5f) + line2.GetPerpendicular());

			bisector1.GetIntersection(bisector2, out u_ray);

			return bisector1.GetCoordinatesAt(u_ray);
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

		public static void ProcessThreeDFloors(List<ThreeDFloor> threedfloors)
		{
			ProcessThreeDFloors(threedfloors, null);
		}

		public static void ProcessThreeDFloors(List<ThreeDFloor> threedfloors, List<Sector> selectedSectors)
		{
			// List<Sector> selectedSectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));
			var sectorsByTag = new Dictionary<int, List<Sector>>();
			var sectorsToThreeDFloors = new Dictionary<Sector, List<ThreeDFloor>>();
			var sectorGroups = new List<List<Sector>>();

			if(selectedSectors == null)
				selectedSectors = new List<Sector>(General.Map.Map.GetSelectedSectors(true));

			var tmpSelectedSectors = new List<Sector>(selectedSectors);

			foreach (ThreeDFloor tdf in GetThreeDFloors(selectedSectors))
			{
				bool add = true;

				foreach (ThreeDFloor tdf2 in threedfloors)
				{
					if (tdf.Sector == tdf2.Sector)
					{
						add = false;
						break;
					}
				}

				if (add)
				{
					threedfloors.Add(tdf);
				}
			}

			tmpSelectedSectors = new List<Sector>(selectedSectors);

			General.Map.UndoRedo.CreateUndo("Modify 3D floors");

			try
			{
				foreach (ThreeDFloor tdf in threedfloors)
				{
					if (tdf.Rebuild)
						tdf.DeleteControlSector();

					if (tdf.IsNew || tdf.Rebuild)
						tdf.CreateGeometry();

					tdf.UpdateGeometry();
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\nPlease increase the size of the control sector area.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				General.Map.UndoRedo.WithdrawUndo();
				return;
			}

			// Fill the sectorsToThreeDFloors dictionary, with a selected sector as key
			// and a list of all 3D floors, that should be applied to to this sector, as value
			foreach (Sector s in selectedSectors)
			{
				if (!sectorsToThreeDFloors.ContainsKey(s))
					sectorsToThreeDFloors.Add(s, new List<ThreeDFloor>());

				foreach (ThreeDFloor tdf in threedfloors)
				{
					if (tdf.TaggedSectors.Contains(s))
						sectorsToThreeDFloors[s].Add(tdf);
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

				foreach (Sector s in delsectors)
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
					try
					{
						newtag = BuilderPlug.Me.ControlSectorArea.GetNewSectorTag();
					}
					catch (Exception e)
					{
						MessageBox.Show(e.Message + "\nPlease increase the custom tag range.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						General.Map.UndoRedo.WithdrawUndo();
						return;
					}


				foreach (Sector s in sectors)
					s.Tag = newtag;

				try
				{
					foreach (ThreeDFloor tdf in sectorsToThreeDFloors[sectors.First()])
						tdf.BindTag(newtag);
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					General.Map.UndoRedo.WithdrawUndo();
					return;
				}
			}

			// Remove unused tags from the 3D floors
			foreach (ThreeDFloor tdf in threedfloors)
				tdf.Cleanup();
		}

		public SlopeVertexGroup AddSlopeVertexGroup(List<SlopeVertex> vertices, out int id, bool floor, bool ceiling)
		{
			for (int i = 1; i < int.MaxValue; i++)
			{
				if (!slopevertexgroups.Exists(x => x.Id == i))
				{
					SlopeVertexGroup svg = new SlopeVertexGroup(i, (List<SlopeVertex>)vertices, floor, ceiling);
					
					slopevertexgroups.Add(svg);

					id = i;

					return svg;
				}
			}

			throw new Exception("No free slope vertex group ids");
		}

		public SlopeVertexGroup GetSlopeVertexGroup(SlopeVertex sv)
		{
			foreach (SlopeVertexGroup svg in slopevertexgroups)
			{
				if (svg.Vertices.Contains(sv))
					return svg;
			}

			return null;
		}

		public SlopeVertexGroup GetSlopeVertexGroup(int id)
		{
			foreach (SlopeVertexGroup svg in slopevertexgroups)
			{
				if (svg.Id == id)
					return svg;
			}

			return null;
		}

		public SlopeVertexGroup GetSlopeVertexGroup(Sector s)
		{
			foreach (SlopeVertexGroup svg in slopevertexgroups)
			{
				if (svg.Sectors.Contains(s))
					return svg;
			}

			return null;
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

		// Taken from http://stackoverflow.com/questions/10816803/finding-next-available-key-in-a-dictionary-or-related-collection
		// Add item to sortedList (numeric key) to next available key item, and return key
		public static int AddNext<T>(this SortedList<int, T> sortedList, T item)
		{
			int key = 1; // Make it 0 to start from Zero based index
			int count = sortedList.Count;

			int counter = 0;
			do
			{
				if (count == 0) break;
				int nextKeyInList = sortedList.Keys[counter++];

				if (key != nextKeyInList) break;

				key = nextKeyInList + 1;

				if (count == 1 || counter == count) break;


				if (key != sortedList.Keys[counter])
					break;

			} while (true);

			sortedList.Add(key, item);
			return key;
		}
	}
}
