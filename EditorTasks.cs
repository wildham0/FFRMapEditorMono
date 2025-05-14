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
		DocksRemove,

		DomainsOpen,
		DomainsUpdate,
		DomainsSetTool,

		TemplatesOpen,
		TemplatesUpdate,
		//TemplatesSetTool,

		MapObjectsOpen,
		MapObjectsUpdate,
		MapObjectsSetTool,
		MapObjectsRemove,

		WindowsClose,

		ExitWarningOpen,
		ExitWarningClose,

		SaveWarningOpen,
		SaveWarningClose,
		SaveNoWarning,
		SetSavingMode,
		SaveWarningUpdate,

		NewMapWarningOpen,
		NewMapWarningClose,

		LoadMapWarningOpen,
		LoadMapWarningClose,

		PaintingUndo,
		PaintingRedo,

		UpdatePlacedObjectsOverlay,
		UpdatePlacedDocksOverlay,
		UpdatePlacedTilesOverlay,

		SaveBackupMap,
		ResetBackupCounter,

		ExitProgramHard,
		ExitProgram,

		ToggleGridlines,
		UpdateGridsize,
		TogglePositionIndicator,

		// Mystic Quest Specific Tasks
		ReloadPicker,
		ToggleLayer,
		ToggleDrawingLayer,
		ToggleInfoBox,

		ResizeMap,
		ResizeToggle,
		ResizeUpdateSelection,

		SelectorSetTool,
		PasterSetTool,
		RestoreTool,

		InfoBoxUpdateLayer,
		InfoBoxUpdateCoordinates,

		Undo,
		Redo,
		Copy,
		Paste,



	}

	public class EditorTask
	{ 
		public EditorTasks Type { get; set; }
		public int Value { get; set; }

		public EditorTask(EditorTasks type, int value = 0)
		{
			Type = type;
			Value = value;
		}
	}

	public class TaskManager
	{
		private List<EditorTask> tasks;

		public TaskManager()
		{
			tasks = new();
		}
		public void Add(EditorTask task)
		{
			tasks.Add(task);
		}
		public void Add(EditorTasks task)
		{
			tasks.Add(new EditorTask(task));
		}
		public void AddRange(List<EditorTask> tasklist)
		{
			tasks.AddRange(tasklist);
		}
		public void Prune(EditorTasks task)
		{
			tasks.RemoveAll(t => t.Type == task);
		}
		public bool Pop(EditorTasks type, out EditorTask task)
		{
			int resultIndex = tasks.FindIndex(t => t.Type == type);
			if (resultIndex < 0)
			{
				task = null;
				return false;
			}
			else
			{
				task = tasks[resultIndex];
				tasks.RemoveAt(resultIndex);
				return true;
			}
		}
		public bool Pop(EditorTasks type)
		{
			int resultIndex = tasks.FindIndex(t => t.Type == type);
			if (resultIndex < 0)
			{
				return false;
			}
			else
			{
				tasks.RemoveAt(resultIndex);
				return true;
			}
		}
	}
}