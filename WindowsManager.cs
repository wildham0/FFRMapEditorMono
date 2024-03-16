﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FFRMapEditorMono
{
	public enum EditorTasks
	{
		None,

		ToggleInfoWindow,
		HideAllWindows,

		FileCreateNewMap,
		FileLoadMap,
		FileSaveMap,
		//FileSaveMapAs,

		OverworldBlueMap,
		OverworldLoadMap,

		TilesOpen,
		TilesToggle,
		TilesSetTool,
		TilesUpdate,
		TilesPickerUpdate,

		BrushesOpen,
		BrushesToggle,
		BrushesSetTool,
		BrushesUpdate,
		BrushesPickerUpdate,
		BrushesUpdateSize,

		DocksOpen,
		DocksUpdate,
		DocksSetTool,

		DomainsOpen,
		DomainsUpdate,
		DomainsSetTool,

		TemplatesOpen,
		TemplatesUpdate,
		//TemplatesSetTool,

		MapObjectsOpen,
		MapObjectsUpdate,
		MapObjectsSetTool,

		WindowsClose,

		ExitWarningOpen,
		ExitWarningClose,

		SaveWarningOpen,
		SaveWarningClose,
		SaveNoWarning,
		SetSavingMode,
		SaveWarningUpdate,

		ExitProgramHard,
		ExitProgram

	}

	public struct EditorTask
	{ 
		public EditorTasks Type { get; set; }
		public int Value { get; set; }
	}

	public class WindowsManager
	{
		private ToolsMenu ToolsWindow;
		private TilePicker TilesWindow;
		private DomainPicker DomainsWindow;
		private DockPicker DocksWindow;
		private MapObjectPicker MapObjectsWindow;
		private BrushPicker BrushWindow;
		private TemplatePicker TemplateWindow;
		private InfoWindow InfoWindow;
		private ExitWarningWindow ExitWarningWindow;
		private SaveWarningWindow SaveWarningWindow;
		public bool ShowDomainOverlay { get => showDomainOverlay || DomainsWindow.Show; }
		public bool ShowDockOverlay { get => showDockOverlay || DocksWindow.Show; }
		public bool ShowMapObjectsOverlay { get => showMapObjectsOverlay; }
		private bool showDomainOverlay;
		private bool showDockOverlay;
		private bool showMapObjectsOverlay;
		public WindowsManager(ToolsMenu _toolsmenu, TilePicker _tilepicker, BrushPicker _brushpicker, DomainPicker _domainpicker, DockPicker _dockppicker, MapObjectPicker _mapobjectpicker, TemplatePicker _templatepicker, InfoWindow _infowindow, ExitWarningWindow _exitwindow, SaveWarningWindow _saveWarningWindow)
		{
			ToolsWindow = _toolsmenu;
			TilesWindow = _tilepicker;
			DomainsWindow = _domainpicker;
			DocksWindow = _dockppicker;
			BrushWindow = _brushpicker;
			MapObjectsWindow = _mapobjectpicker;
			TemplateWindow = _templatepicker;
			InfoWindow = _infowindow;
			ExitWarningWindow = _exitwindow;
			SaveWarningWindow = _saveWarningWindow;

			showDomainOverlay = false;
			showDockOverlay = false;
			showMapObjectsOverlay = true;
			
		}
		public bool CanInteractWithMap(Vector2 mousecursor)
		{
			bool inTools = ToolsWindow.MouseHovering(mousecursor);
			bool inTiles = TilesWindow.MouseHovering(mousecursor);
			bool inDomains = DomainsWindow.MouseHovering(mousecursor);
			bool inDocks = DocksWindow.MouseHovering(mousecursor);
			bool inMapObjects = MapObjectsWindow.MouseHovering(mousecursor);
			bool inBrushes = BrushWindow.MouseHovering(mousecursor);
			bool inInfoWindow = InfoWindow.MouseHovering(mousecursor);
			bool inTemplates = TemplateWindow.MouseHovering(mousecursor);
			bool inExitWarning = ExitWarningWindow.MouseHovering(mousecursor);
			bool inSaveWarning = SaveWarningWindow.MouseHovering(mousecursor);

			return (!inTools && !inTiles && !inDomains && !inDocks && !inMapObjects && !inBrushes && !inInfoWindow && !inTemplates && !inExitWarning && !inSaveWarning);
		}
		private void HideAllWindows()
		{
			TilesWindow.Show = false;
			DomainsWindow.Show = false;
			DocksWindow.Show = false;
			MapObjectsWindow.Show = false;
			BrushWindow.Show = false;
			InfoWindow.Show = false;
			TemplateWindow.Show = false;
		}
		public void ProcessTasks(List<EditorTask> tasks, Overworld overworld)
		{
			var validtasks = tasks.ToList();

			foreach (var task in validtasks)
			{
				if (task.Type == EditorTasks.MapObjectsOpen)
				{
					if (!MapObjectsWindow.Show)
					{
						HideAllWindows();
						MapObjectsWindow.Show = true;
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.DocksOpen)
				{
					if (!DocksWindow.Show)
					{
						HideAllWindows();
						DocksWindow.Show = true;
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.ToggleInfoWindow)
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						if (!InfoWindow.Show)
						{
							HideAllWindows();
						}

						InfoWindow.Show = !InfoWindow.Show;
						tasks.Remove(task);
					}
				}
				else if (task.Type == EditorTasks.HideAllWindows)
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						HideAllWindows();
						tasks.Remove(task);
					}
				}
				else if (task.Type == EditorTasks.WindowsClose)
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						HideAllWindows();
						tasks.Remove(task);
					}
				}
				else if (task.Type == EditorTasks.TemplatesOpen)
				{
					if (!TemplateWindow.Show)
					{
						HideAllWindows();
						TemplateWindow.Show = true;
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.TilesOpen)
				{
					if (!TilesWindow.Show)
					{
						HideAllWindows();
						TilesWindow.Show = true;
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.BrushesOpen)
				{
					if (!BrushWindow.Show)
					{
						HideAllWindows();
						BrushWindow.Show = true;
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.DomainsOpen)
				{
					if (!DomainsWindow.Show)
					{
						HideAllWindows();
						DomainsWindow.Show = true;
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.TilesToggle)
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						bool currentState = TilesWindow.Show;
						HideAllWindows();
						TilesWindow.Show = !currentState;
						tasks.Remove(task);
					}
				}
				else if (task.Type == EditorTasks.BrushesToggle)
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						bool currentState = BrushWindow.Show;
						HideAllWindows();
						BrushWindow.Show = !currentState;
						tasks.Remove(task);
					}
				}
				else if (task.Type == EditorTasks.ExitWarningClose)
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						ExitWarningWindow.Show = false;
						tasks.Remove(task);
					}
				}
				else if (task.Type == EditorTasks.ExitWarningOpen)
				{
					HideAllWindows();
					ExitWarningWindow.Show = true;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.SaveWarningClose)
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						SaveWarningWindow.Show = false;
						tasks.Remove(task);
					}
				}
				else if (task.Type == EditorTasks.SaveWarningOpen)
				{
					HideAllWindows();
					SaveWarningWindow.Show = true;
					tasks.Remove(task);
				}
			}
		}
	}
}