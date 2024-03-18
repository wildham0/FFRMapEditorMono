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
		ExitProgram

	}

	public struct EditorTask
	{ 
		public EditorTasks Type { get; set; }
		public int Value { get; set; }
	}
}