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
	public struct Setting
	{ 
		public string Name { get; set; }
		public int Value { get; set; }
	}
	public class SettingsManager
	{
		public List<Setting> Settings { get; set; }
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
	}
	
	public class FileManager
	{
		private string LoadedMapPath;
		private string LoadedMapName;
		public bool FilenameUpdated { get; set; }
		public OwMapExchangeData OverworldData { get; set; }
		public SettingsManager Settings { get; set; }
		public FileManager()
		{
			LoadedMapPath = "";
			LoadedMapName = "";
			FilenameUpdated = false;
			OverworldData = new();
			Settings = new();
		}
		public void ProcessTasks(Overworld overworld, List<string> missingTiles, List<EditorTask> tasks)
		{
			var validtask = tasks.ToList();
			bool filesaved = false;

			foreach (var task in validtask)
			{
				if (task.Type == EditorTasks.FileCreateNewMap)
				{
					if (task.Value == (int)WarningSetting.Trigger && overworld.UnsavedChanges)
					{
						tasks.Add(new EditorTask() { Type = EditorTasks.NewMapWarningOpen });
						tasks.Remove(task);
					}
					else
					{
						OverworldData = new();
						LoadedMapName = "";
						LoadedMapPath = "";
						tasks.Remove(task);
						tasks.Add(new EditorTask() { Type = EditorTasks.OverworldBlueMap });

					}
				}
				else if (task.Type == EditorTasks.FileLoadMap)
				{
					if (task.Value == (int)WarningSetting.Trigger && overworld.UnsavedChanges)
					{
						tasks.Add(new EditorTask() { Type = EditorTasks.LoadMapWarningOpen });
						tasks.Remove(task);
					}
					else
					{ 
						bool fileLoaded = OpenFile();
						if (fileLoaded)
						{
							tasks.Add(new EditorTask() { Type = EditorTasks.OverworldLoadMap });
						}
						tasks.Remove(task);
					}
				}
				else if (task.Type == EditorTasks.FileSaveMap)
				{
					var missingStuff = overworld.ValidateObjects();
					if (missingTiles.Any() || !missingStuff.defaultdock || missingStuff.missingmapobjects.Any())
					{
						tasks.Add(new EditorTask() { Type = EditorTasks.SaveWarningOpen });
						tasks.Add(new EditorTask() { Type = EditorTasks.SaveWarningUpdate, Value = task.Value });
					}
					else
					{
						tasks.Add(new EditorTask() { Type = EditorTasks.SaveNoWarning, Value = task.Value });
					}

					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.SaveNoWarning && task.Value == (int)SavingMode.Save)
				{
					if (FileSelected())
					{
						OverworldData.EncodeMap(overworld);
						filesaved = SaveFile();
					}
					else
					{
						OverworldData.EncodeMap(overworld);
						filesaved = SaveFileAs();
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.SaveNoWarning && task.Value == (int)SavingMode.SaveAs)
				{
					OverworldData.EncodeMap(overworld);
					filesaved = SaveFileAs();
					tasks.Remove(task);
				}
			}

			if (filesaved)
			{
				overworld.UnsavedChanges = false;
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
				fbd.Filter = "FFR Json Overworld Map (*.json)|*.json|FFHackster Overworld Map (*.ffm)|*.ffm|All files (*.*)|*.*";
				fbd.FilterIndex = 1;
				fbd.RestoreDirectory = true;
				result = fbd.ShowDialog();
				if (result == DialogResult.OK)
				{
					var splittedFileName = fbd.FileName.Split('\\');
					LoadedMapPath = String.Join('\\', splittedFileName.Take(splittedFileName.Length - 1));
					LoadedMapName = splittedFileName.Last();
					var filname = LoadedMapName.Split('.');

					if (filname.Last() == "ffm")
					{
						using var stream = new BinaryReader(fbd.OpenFile());
							var dataarray = stream.ReadBytes(0x10000);

						OverworldData = new();
						OverworldData.EncodeMapFromBytes(dataarray);
					}
					else
					{
						using var stream = new StreamReader(fbd.OpenFile());
							var jsonstring = stream.ReadToEnd();

						OverworldData = JsonSerializer.Deserialize<OwMapExchangeData>(jsonstring);
					}

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
				fbd.Filter = "FFR Json Overworld Map (*.json)|*.json|FFHackster Overworld Map (*.ffm)|*.ffm|All files (*.*)|*.*";
				fbd.FilterIndex = 1;
				fbd.RestoreDirectory = true;
				result = fbd.ShowDialog();
				if (result == DialogResult.OK)
				{
					if (fbd.FilterIndex == 2)
					{
						using var stream = new BinaryWriter(fbd.OpenFile());
							stream.Write(OverworldData.DecodeMap());
					}
					else
					{
						string serializedOwData = JsonSerializer.Serialize<OwMapExchangeData>(OverworldData, new JsonSerializerOptions { WriteIndented = true });
						using var stream = new StreamWriter(fbd.OpenFile());
							stream.Write(serializedOwData);
					}
					filesaved = true;
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
			var filname = LoadedMapName.Split('.');

			if (filname.Last() == "ffm")
			{
				using var stream = new BinaryWriter(new FileStream(LoadedMapPath + "\\" + LoadedMapName, FileMode.OpenOrCreate));
					stream.Write(OverworldData.DecodeMap());
			}
			else
			{
				string serializedOwData = JsonSerializer.Serialize<OwMapExchangeData>(OverworldData, new JsonSerializerOptions { WriteIndented = true });
				using var stream = new StreamWriter(LoadedMapPath + "\\" + LoadedMapName);
					stream.Write(serializedOwData);
			}

			return true;
		}
		public bool FileSelected()
		{
			return LoadedMapName != "";
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