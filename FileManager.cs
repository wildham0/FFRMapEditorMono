using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.IO;
using System.Text.Json;
using System.Threading;
using FFRMapEditorMono.MysticQuest;
using FFRMapEditorMono.FFR;

namespace FFRMapEditorMono
{
	public enum SavingMode
	{ 
		Save = 0,
		SaveAs
	}
	public enum WarningSetting
	{
		Trigger = 0,
		Disabled
	}
	public enum WriteFormat
	{ 
		Binary,
		Json
	}
	public enum GameMode
	{ 
		FFR,
		FFMQ
	}

	public struct Setting
	{ 
		public string Name { get; set; }
		public int Value { get; set; }
	}
	public class SettingsManager
	{
		public List<Setting> Settings { get; set; }
		private int defaultUndoDepth = 4;
		private Point defaultResolution = new Point(1200, 800);
		private int defaultDelayForBackup = 5;
		public SettingsManager()
		{
			Settings = new();
		}
		public Point GetResolution()
		{
			var resx = Settings.Where(s => s.Name == "Resolution X").ToList();
			var resy = Settings.Where(s => s.Name == "Resolution Y").ToList();

			if (resx.Any() && resy.Any())
			{
				return new Point(resx.First().Value, resy.First().Value);
			}
			else
			{
				SetResolution(new Point(1200, 800));
				return new Point(1200, 800);
			}
		}
		public void SetResolution(Point resolution)
		{
			Settings.RemoveAll(s => s.Name == "Resolution X");
			Settings.RemoveAll(s => s.Name == "Resolution Y");

			Settings.Add(new Setting() { Name = "Resolution X", Value = resolution.X });
			Settings.Add(new Setting() { Name = "Resolution Y", Value = resolution.Y });
		}
		public int GetUndoDepth()
		{
			var depth = Settings.Where(s => s.Name == "Undo Depth").ToList();
			if (depth.Any())
			{
				return depth.First().Value;
			}
			else
			{
				SetUndoDepth(defaultUndoDepth);
				return defaultUndoDepth;
			}
		}
		public void SetUndoDepth(int depth)
		{
			Settings.RemoveAll(s => s.Name == "Undo Depth");
			Settings.Add(new Setting() { Name = "Undo Depth", Value = depth });
		}
		public int GetBackupDelay()
		{
			var minutes = Settings.Where(s => s.Name == "Backup Delay").ToList();
			if (minutes.Any())
			{
				return minutes.First().Value * 60 * 60;
			}
			else
			{
				SetBackupDelay(defaultUndoDepth);
				return defaultUndoDepth * 60 * 60;
			}
		}
		public void SetBackupDelay(int minutes)
		{
			Settings.RemoveAll(s => s.Name == "Backup Delay");
			Settings.Add(new Setting() { Name = "Backup Delay", Value = minutes });
		}
	}
	
	public class FileManager
	{
		protected string LoadedMapPath;
		protected string LoadedMapName;
		protected string FileFilter;
		private GameMode ManagerMode;
		public JsonMap MapDataMQ { get; set; }
		public OwMapExchangeData MapDataFF { get; set; }
		public bool FilenameUpdated { get; set; }
		//public virtual object MapData { get; set; }
		public SettingsManager Settings { get; set; }
		public FileManager(GameMode _mode)
		{
			LoadedMapPath = "";
			LoadedMapName = "New Map";
			FilenameUpdated = false;
			Settings = new();
			ManagerMode = _mode;
			MapDataMQ = new();
			MapDataFF = new();

			if (ManagerMode == GameMode.FFR)
			{
				FileFilter = "FFR Json Overworld Map (*.json)|*.json|FFHackster Overworld Map (*.ffm)|*.ffm|All files (*.*)|*.*";
			}
			else if (ManagerMode == GameMode.FFMQ)
			{
				FileFilter = "FFMQ Json Map (*.json)|*.json|All files (*.*)|*.*";
			}

		}
		public virtual void ResetMapData()
		{
			if (ManagerMode == GameMode.FFR)
			{
				MapDataFF = new();
			}
			else if (ManagerMode == GameMode.FFMQ)
			{
				MapDataMQ = new();
			}
		}
		public virtual void LoadMapData(CanvasFFR canvas)
		{
			MapDataFF.EncodeMap(canvas);
		}
		public virtual void LoadMapData(CanvasMQ canvas)
		{
			MapDataMQ = canvas.ExportJsonMap();
		}
		public virtual void LoadMapData(string json)
		{
			if (ManagerMode == GameMode.FFR)
			{
				MapDataFF = JsonSerializer.Deserialize<OwMapExchangeData>(json);
			}
			else if (ManagerMode == GameMode.FFMQ)
			{
				MapDataMQ = new(json);
			}
		}
		public virtual string GetJsonString()
		{
			if (ManagerMode == GameMode.FFR)
			{
				return JsonSerializer.Serialize<OwMapExchangeData>(MapDataFF, new JsonSerializerOptions { WriteIndented = true });
			}
			else if (ManagerMode == GameMode.FFMQ)
			{
				return MapDataMQ.ToJson();
			}
			else
			{
				return "";
			}
		}
		public virtual WriteFormat GetFileFormat(int index)
		{
			if (ManagerMode == GameMode.FFR)
			{
				var filename = LoadedMapName.Split('.');

				if (index == 0)
				{
					if (filename.Last() == "ffm")
					{
						return WriteFormat.Binary;
					}
					else
					{
						return WriteFormat.Json;
					}
				}
				else if (index == 2)
				{
					return WriteFormat.Binary;
				}
				else
				{
					return WriteFormat.Json;
				}
			}
			else if (ManagerMode == GameMode.FFMQ)
			{
				return WriteFormat.Json;
			}
			else
			{
				return WriteFormat.Json;
			}
		}
		public virtual void WriteFile(Stream file, WriteFormat format)
		{
			if (ManagerMode == GameMode.FFR)
			{
				if (format == WriteFormat.Binary)
				{
					using var stream = new BinaryWriter(file);
					stream.Write(MapDataFF.DecodeMap());
				}
				else
				{
					string serializedOwData = JsonSerializer.Serialize<OwMapExchangeData>(MapDataFF, new JsonSerializerOptions { WriteIndented = true });
					using var stream = new StreamWriter(file);
					stream.Write(serializedOwData);
				}
			}
			else if (ManagerMode == GameMode.FFMQ)
			{
				string serializedOwData = GetJsonString();
				using var stream = new StreamWriter(file);
				stream.Write(serializedOwData);
			}
		}
		public virtual void ReadFile(Stream file, WriteFormat format)
		{
			if (ManagerMode == GameMode.FFR)
			{
				if (format == WriteFormat.Binary)
				{
					using var stream = new BinaryReader(file);
					var dataarray = stream.ReadBytes(0x10000);

					MapDataFF = new();
					MapDataFF.EncodeMapFromBytes(dataarray);
				}
				else
				{
					using var stream = new StreamReader(file);
					var jsonstring = stream.ReadToEnd();

					MapDataFF = JsonSerializer.Deserialize<OwMapExchangeData>(jsonstring);
				}
			}
			else if (ManagerMode == GameMode.FFMQ)
			{
				using var stream = new StreamReader(file);
				var jsonstring = stream.ReadToEnd();
				LoadMapData(jsonstring);
			}
		}
		private void ProcessTasksFF(CanvasFFR map, TaskManager tasks)
		{
			bool filesaved = false;
			EditorTask task;

			if (tasks.Pop(EditorTasks.FileCreateNewMap, out task))
			{
				if (task.Value == (int)WarningSetting.Trigger && map.UnsavedChanges)
				{
					tasks.Add(EditorTasks.NewMapWarningOpen);
				}
				else
				{
					ResetMapData();
					LoadedMapName = "New Map";
					LoadedMapPath = "";
					tasks.Add(EditorTasks.OverworldBlueMap);
					tasks.Add(EditorTasks.ResetBackupCounter);
				}
			}

			if (tasks.Pop(EditorTasks.FileSaveMap, out task))
			{
				if (map.MissingMapObjects.Any() || !map.DefaultDockPlaced || map.MissingRequiredTiles.Any())
				{
					tasks.Add(new EditorTask(EditorTasks.SaveWarningOpen));
					tasks.Add(new EditorTask(EditorTasks.SaveWarningUpdate, task.Value));
				}
				else
				{
					tasks.Add(new EditorTask(EditorTasks.SaveNoWarning, task.Value));
				}
			}

			if (tasks.Pop(EditorTasks.FileLoadMap, out task))
			{
				if (task.Value == (int)WarningSetting.Trigger && map.UnsavedChanges)
				{
					tasks.Add(EditorTasks.LoadMapWarningOpen);
				}
				else
				{
					bool fileLoaded = OpenFile();
					if (fileLoaded)
					{
						tasks.Add(EditorTasks.OverworldLoadMap);
						tasks.Add(EditorTasks.ResetBackupCounter);
					}
				}
			}

			if (tasks.Pop(EditorTasks.SaveNoWarning, out task))
			{
				if (task.Value == (int)SavingMode.Save)
				{
					if (FileSelected())
					{
						LoadMapData(map);
						filesaved = SaveFile();
					}
					else
					{
						LoadMapData(map);
						filesaved = SaveFileAs();
					}
				}
				else if (task.Value == (int)SavingMode.SaveAs)
				{
					LoadMapData(map);
					filesaved = SaveFileAs();
				}
			}

			if (tasks.Pop(EditorTasks.SaveBackupMap, out task))
			{
				LoadMapData(map);
				SaveBackup();
			}

			if (filesaved)
			{
				map.UnsavedChanges = false;
			}
		}

		private void ProcessTasksMQ(CanvasMQ map, TaskManager tasks)
		{
			bool filesaved = false;
			EditorTask task;

			if (tasks.Pop(EditorTasks.FileCreateNewMap, out task))
			{
				if (task.Value == (int)WarningSetting.Trigger && map.UnsavedChanges)
				{
					tasks.Add(EditorTasks.NewMapWarningOpen);
				}
				else
				{
					ResetMapData();
					LoadedMapName = "New Map";
					LoadedMapPath = "";
					tasks.Add(EditorTasks.OverworldBlueMap);
					tasks.Add(EditorTasks.ResetBackupCounter);
				}
			}

			if (tasks.Pop(EditorTasks.FileSaveMap, out task))
			{
				tasks.Add(new EditorTask(EditorTasks.SaveNoWarning, task.Value));
			}

			if (tasks.Pop(EditorTasks.FileLoadMap, out task))
			{
				if (task.Value == (int)WarningSetting.Trigger && map.UnsavedChanges)
				{
					tasks.Add(EditorTasks.LoadMapWarningOpen);
				}
				else
				{
					bool fileLoaded = OpenFile();
					if (fileLoaded)
					{
						tasks.Add(EditorTasks.OverworldLoadMap);
						tasks.Add(EditorTasks.ResetBackupCounter);
					}
				}
			}

			if (tasks.Pop(EditorTasks.SaveNoWarning, out task))
			{
				if (task.Value == (int)SavingMode.Save)
				{
					if (FileSelected())
					{
						LoadMapData(map);
						filesaved = SaveFile();
					}
					else
					{
						LoadMapData(map);
						filesaved = SaveFileAs();
					}
				}
				else if (task.Value == (int)SavingMode.SaveAs)
				{
					LoadMapData(map);
					filesaved = SaveFileAs();
				}
			}

			if (tasks.Pop(EditorTasks.SaveBackupMap, out task))
			{
				LoadMapData(map);
				SaveBackup();
			}

			if (filesaved)
			{
				map.UnsavedChanges = false;
			}
		}

		public virtual void ProcessTasks(CanvasFFR mapFF, CanvasMQ mapMQ, TaskManager tasks)
		{
			if (ManagerMode == GameMode.FFR)
			{
				ProcessTasksFF(mapFF, tasks);
			}
			else if (ManagerMode == GameMode.FFMQ)
			{
				ProcessTasksMQ(mapMQ, tasks);
			}
		}
		public string GetFileName()
		{
			return LoadedMapName == "" ? "" : " - " + LoadedMapName;
		}
		public bool OpenFile()
		{
			DialogResult result;
			var t = new Thread((ThreadStart)(() => {
				OpenFileDialog fbd = new OpenFileDialog();
				fbd.InitialDirectory = System.Environment.SpecialFolder.MyComputer.ToString();
				fbd.Filter = FileFilter;
				fbd.FilterIndex = 1;
				fbd.RestoreDirectory = true;
				result = fbd.ShowDialog();
				if (result == DialogResult.OK)
				{
					var splittedFileName = fbd.FileName.Split('\\');
					LoadedMapPath = String.Join('\\', splittedFileName.Take(splittedFileName.Length - 1));
					LoadedMapName = splittedFileName.Last();

					var writeformat = GetFileFormat(0);
					ReadFile(fbd.OpenFile(), writeformat);
					FilenameUpdated = true;
				}
			}));

			t.SetApartmentState(ApartmentState.STA);
			t.Start();
			t.Join();

			if (LoadedMapPath != "")
			{
				return true;
			}
			else
			{
				return false;
			}

		}
		public bool SaveFileAs()
		{
			bool existingpath = LoadedMapPath != "";
			bool filesaved = false;

			DialogResult result;
			var t = new Thread((ThreadStart)(() => {
				SaveFileDialog fbd = new SaveFileDialog();
				fbd.FileName = LoadedMapName;
				fbd.InitialDirectory = existingpath ? LoadedMapPath : System.Environment.SpecialFolder.MyComputer.ToString();
				fbd.Filter = FileFilter;
				fbd.FilterIndex = 1;
				fbd.RestoreDirectory = true;
				result = fbd.ShowDialog();
				if (result == DialogResult.OK)
				{
					var writeformat = GetFileFormat(fbd.FilterIndex);
					WriteFile(fbd.OpenFile(), writeformat);

					filesaved = true;
					var splittedFileName = fbd.FileName.Split('\\');
					LoadedMapName = splittedFileName.Last();
					FilenameUpdated = true;
				}
			}));

			t.SetApartmentState(ApartmentState.STA);
			t.Start();
			t.Join();

			return filesaved;
		}
		public bool SaveFile()
		{
			var writeformat = GetFileFormat(0);
			WriteFile(new FileStream(LoadedMapPath + "\\" + LoadedMapName, FileMode.OpenOrCreate), writeformat);

			return true;
		}
		public bool SaveBackup()
		{
			string serializedOwData = GetJsonString();
			using var stream = new StreamWriter("backupowmap.json");
			stream.Write(serializedOwData);

			return true;
		}
		public bool FileSelected()
		{
			return LoadedMapName != "New Map";
		}
		public void SaveSettings()
		{
			string serializedData = JsonSerializer.Serialize<SettingsManager>(Settings, new JsonSerializerOptions { WriteIndented = true });
			using var stream = new StreamWriter("settings.json");
			stream.Write(serializedData);
		}
		public void LoadSettings()
		{
			if (!File.Exists("settings.json"))
			{
				return;
			}

			using var stream = new StreamReader("settings.json");
				var jsonstring = stream.ReadToEnd();

			Settings = JsonSerializer.Deserialize<SettingsManager>(jsonstring);
			
			if (Settings == null)
			{
				Settings = new();
			}
		}
	}
}