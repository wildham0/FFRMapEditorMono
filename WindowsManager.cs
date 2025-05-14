using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FFRMapEditorMono.MysticQuest;
using FFRMapEditorMono.FFR;

namespace FFRMapEditorMono
{
	public class WindowsManager
	{
		private OptionPicker ToolsWindow;
		private InfoWindow InfoWindow;
		public bool ShowDomainOverlay { get => showDomainOverlay || (optionPickers.OfType<DomainPicker>().Any() ? optionPickers.OfType<DomainPicker>().First().Show : false); }
		public bool ShowDockOverlay { get => showDockOverlay || (optionPickers.OfType<DockPicker>().Any() ? optionPickers.OfType<DockPicker>().First().Show : false); }
		public bool ShowGridlines { get => showGrid; }
		public bool ShowMapObjectsOverlay { get => showMapObjectsOverlay; }
		private bool showDomainOverlay;
		private bool showGrid;
		private bool showDockOverlay;
		private bool showMapObjectsOverlay;
		private List<WarningWindow> warningWindows;
		private List<OptionPicker> optionPickers;
		public WindowsManager(OptionPicker _toolsmenu, InfoWindow _infowindow)
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
		public void ProcessTasks(TaskManager tasks)
		{
			EditorTask task;

			if (tasks.Pop(EditorTasks.ToggleInfoWindow, out task))
			{
				if (task.Value > 0)
				{
					tasks.Add(new EditorTask(task.Type, task.Value - 1));
				}
				else
				{
					if (!InfoWindow.Show)
					{
						HideAllWindows();
					}

					InfoWindow.Show = !InfoWindow.Show;
				}
			}

			if (tasks.Pop(EditorTasks.HideAllWindows, out task))
			{
				if (task.Value > 0)
				{
					tasks.Add(new EditorTask(task.Type, task.Value - 1));
				}
				else
				{
					HideAllWindows();
				}
			}

			if (tasks.Pop(EditorTasks.WindowsClose, out task))
			{
				if (task.Value > 0)
				{
					tasks.Add(new EditorTask(task.Type, task.Value - 1));
				}
				else
				{
					HideAllWindows();
				}
			}

			foreach (var toggletask in toggleTaskToType)
			{
				if (tasks.Pop(toggletask, out task))
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask(task.Type, task.Value - 1));
					}
					else
					{
						optionPickers.ForEach(w => w.Show = ((w.ToggleTask == task.Type) && !w.Show));
					}
				}
			}

			foreach (var closetask in closeTaskToType)
			{
				if (tasks.Pop(closetask.Key, out task))
				{
					if (task.Value > 0)
					{
						tasks.Add(new EditorTask(task.Type, task.Value - 1));
					}
					else
					{
						warningWindows.ForEach(w => w.Show = w.GetType() != closeTaskToType[task.Type] && w.Show);
						optionPickers.ForEach(w => w.Show = w.GetType() != closeTaskToType[task.Type] && w.Show);
					}
				}
			}

			foreach (var opentask in openTaskToType)
			{
				if (tasks.Pop(opentask.Key, out task))
				{
					HideAllWindows();
					warningWindows.ForEach(w => w.Show = openTaskToType[task.Type].Contains(w.GetType()));
					optionPickers.ForEach(w => w.Show = openTaskToType[task.Type].Contains(w.GetType()));
				}
			}

			if (tasks.Pop(EditorTasks.ToggleGridlines, out task))
			{
				showGrid = !showGrid;
			}
		}
		static Dictionary<EditorTasks, Type> closeTaskToType = new()
		{
			{ EditorTasks.ExitWarningClose, typeof(ExitWarningWindow) },
			{ EditorTasks.SaveWarningClose, typeof(SaveWarningWindow) },
			{ EditorTasks.LoadMapWarningClose, typeof(LoadMapWarningWindow) },
			{ EditorTasks.NewMapWarningClose, typeof(NewMapWarningWindow) },
		};
		
		static List<EditorTasks> toggleTaskToType = new()
		{
			EditorTasks.BrushesToggle, EditorTasks.TilesToggle, EditorTasks.ResizeToggle

		};
		static Dictionary<EditorTasks, List<Type>> openTaskToType = new()
		{
			{ EditorTasks.DomainsOpen, new() { typeof(DomainPicker) } },
			{ EditorTasks.BrushesOpen, new() { typeof(BrushPicker) } },
			{ EditorTasks.TilesOpen, new() { typeof(TilePicker), typeof(TilePickerMQ) } },
			{ EditorTasks.TemplatesOpen, new() { typeof(TemplatePicker) } },
			{ EditorTasks.DocksOpen, new() { typeof(DockPicker) } },
			{ EditorTasks.MapObjectsOpen, new() { typeof(MapObjectPicker) } },
			{ EditorTasks.ExitWarningOpen, new() { typeof(ExitWarningWindow) } },
			{ EditorTasks.SaveWarningOpen, new() { typeof(SaveWarningWindow) } },
			{ EditorTasks.LoadMapWarningOpen, new() { typeof(LoadMapWarningWindow) } },
			{ EditorTasks.NewMapWarningOpen, new() { typeof(NewMapWarningWindow) } },
			//{ EditorTasks.ResizeToggle, new() { typeof(MapResize) } },
		};

	}
}