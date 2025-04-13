using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FFRMapEditorMono
{
	public class WindowsManager
	{
		private ToolsMenu ToolsWindow;
		private InfoWindow InfoWindow;
		public bool ShowDomainOverlay { get => showDomainOverlay || optionPickers.OfType<DomainPicker>().First().Show; }
		public bool ShowDockOverlay { get => showDockOverlay || optionPickers.OfType<DockPicker>().First().Show; }
		public bool ShowGridlines { get => showGrid; }
		public bool ShowMapObjectsOverlay { get => showMapObjectsOverlay; }
		private bool showDomainOverlay;
		private bool showGrid;
		private bool showDockOverlay;
		private bool showMapObjectsOverlay;
		private List<WarningWindow> warningWindows;
		private List<OptionPicker> optionPickers;
		public WindowsManager(ToolsMenu _toolsmenu, InfoWindow _infowindow)
		{
			ToolsWindow = _toolsmenu;
			InfoWindow = _infowindow;

			showDomainOverlay = false;
			showDockOverlay = false;
			showGrid = false;
			showMapObjectsOverlay = true;
		}
		public void RegisterWarningWindows(List<WarningWindow> _windows)
		{
			warningWindows = _windows;
		}
		public void RegisterOptionPickers(List<OptionPicker> _pickers)
		{
			optionPickers = _pickers;
		}
		public bool CanInteractWithMap(Vector2 mousecursor)
		{
			bool cursorinwindow = false;
			foreach (var window in warningWindows)
			{
				cursorinwindow |= window.MouseHovering(mousecursor);
			}

			foreach (var picker in optionPickers)
			{
				cursorinwindow |= picker.MouseHovering(mousecursor);
			}

			bool inTools = ToolsWindow.MouseHovering(mousecursor);
			bool inInfoWindow = InfoWindow.MouseHovering(mousecursor);

			return (!inTools && !inInfoWindow && !cursorinwindow);
		}
		private void HideAllWindows()
		{
			foreach (var picker in optionPickers)
			{
				picker.Show = false;
			}

			InfoWindow.Show = false;
		}
		public void ProcessTasks(List<EditorTask> tasks, Overworld overworld)
		{
			var validtasks = tasks.ToList();

			foreach (var task in validtasks)
			{
				if (task.Type == EditorTasks.ToggleInfoWindow)
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
				else if (toggleTaskToType.ContainsKey(task.Type))
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						optionPickers.ForEach(w => w.Show = w.GetType() == toggleTaskToType[task.Type] && !w.Show);
						tasks.Remove(task);
					}
				}
				else if (closeTaskToType.ContainsKey(task.Type))
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask() { Type = task.Type, Value = task.Value - 1 });
						tasks.Remove(task);
					}
					else
					{
						warningWindows.ForEach(w => w.Show = w.GetType() != closeTaskToType[task.Type] && w.Show);
						optionPickers.ForEach(w => w.Show = w.GetType() != closeTaskToType[task.Type] && w.Show);
						tasks.Remove(task);
					}
				}
				else if (openTaskToType.ContainsKey(task.Type))
				{
					HideAllWindows();
					warningWindows.ForEach(w => w.Show = w.GetType() == openTaskToType[task.Type]);
					optionPickers.ForEach(w => w.Show = w.GetType() == openTaskToType[task.Type]);
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.ToggleGridlines)
				{
					showGrid = !showGrid;
					tasks.Remove(task);
				}
			}
		}
		static Dictionary<EditorTasks, Type> closeTaskToType = new()
		{
			{ EditorTasks.ExitWarningClose, typeof(ExitWarningWindow) },
			{ EditorTasks.SaveWarningClose, typeof(SaveWarningWindow) },
			{ EditorTasks.LoadMapWarningClose, typeof(LoadMapWarningWindow) },
			{ EditorTasks.NewMapWarningClose, typeof(NewMapWarningWindow) },
		};
		static Dictionary<EditorTasks, Type> toggleTaskToType = new()
		{
			{ EditorTasks.BrushesToggle, typeof(BrushPicker) },
			{ EditorTasks.TilesToggle, typeof(TilePicker) },
		};
		static Dictionary<EditorTasks, Type> openTaskToType = new()
		{
			{ EditorTasks.DomainsOpen, typeof(DomainPicker) },
			{ EditorTasks.BrushesOpen, typeof(BrushPicker) },
			{ EditorTasks.TilesOpen, typeof(TilePicker) },
			{ EditorTasks.TemplatesOpen, typeof(TemplatePicker) },
			{ EditorTasks.DocksOpen, typeof(DockPicker) },
			{ EditorTasks.MapObjectsOpen, typeof(MapObjectPicker) },
			{ EditorTasks.ExitWarningOpen, typeof(ExitWarningWindow) },
			{ EditorTasks.SaveWarningOpen, typeof(SaveWarningWindow) },
			{ EditorTasks.LoadMapWarningOpen, typeof(LoadMapWarningWindow) },
			{ EditorTasks.NewMapWarningOpen, typeof(NewMapWarningWindow) },
		};

	}
}