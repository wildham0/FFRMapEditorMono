using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono.FFR
{
	public class BrushPicker : OptionPicker
	{
		public BrushPicker(Texture2D _window, Texture2D _selector, SpriteFont _font)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionFont = _font;
			ToggleTask = EditorTasks.BrushesToggle;

			Show = false;
			Position = new Vector2(64, 0);
			zoom = 2.0f;
			optionsRows = 1;
			optionsColumns = 8;
			optionsSize = 16;

			options = brushNames.Select((b, i) => (b,
				new List<EditorTask>() {
					new EditorTask(EditorTasks.BrushesUpdate, i),
					new EditorTask(EditorTasks.WindowsClose, 10) },
				new List<EditorTask>() {
					new EditorTask(EditorTasks.BrushesUpdate, i) }
			)).ToList();

			lastSelection = 0x00;
			SetOptionTextLength();
			showPlaced = false;
		}
		public override void ProcessTasks(TaskManager tasks)
		{
			EditorTask task;

			if (tasks.Pop(EditorTasks.BrushesPickerUpdate, out task))
			{
				int validselection = 0;

				if (OwDataGroup.TileByteToGroup.TryGetValue((byte)task.Value, out var result))
				{
					if (result <= TileGroup.Marsh)
					{
						validselection = (int)result;
					}
				}

				lastSelection = validselection;
			}
		}
		public void UpdateTile(byte tile)
		{
			lastSelection = tile;
		}
		private List<string> brushNames = new()
		{
			"LAND",
			"FOREST",
			"MOUNTAIN",
			"SEA",
			"DESERT",
			"RIVER",
			"GRASS",
			"MARSH"
		};
	}
	public class TilePicker : OptionPicker
	{
		private Canvas overworld;
		public TilePicker(Texture2D _window, Texture2D _selector, Texture2D _placedicons, SpriteFont _font, Canvas _overworld)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionIcons = _placedicons;
			optionFont = _font;
			overworld = _overworld;
			ToggleTask = EditorTasks.TilesToggle;

			Show = false;
			Position = new Vector2(64, 0);
			zoom = 2.0f;
			optionsRows = 0x08;
			optionsColumns = 0x10;
			optionsSize = 16;

			options = TileInfo.Names.Select((t, i) => (t,
				new List<EditorTask>() {
					new EditorTask(EditorTasks.TilesUpdate, i),
					new EditorTask(EditorTasks.WindowsClose, 10) },
				new List<EditorTask>() {
					new EditorTask(EditorTasks.TilesUpdate, i) }
				)).ToList();

			SetOptionTextLength();
			lastSelection = 0x00;
			placedOptions = new();
			unplacedOptions = TileInfo.Required.Select(t => (int)t).ToList();
			showPlaced = true;
		}
		public override void ProcessTasks(TaskManager tasks)
		{
			EditorTask task;

			if (tasks.Pop(EditorTasks.TilesPickerUpdate, out task))
			{
				lastSelection = task.Value;
			}

			if (tasks.Pop(EditorTasks.UpdatePlacedTilesOverlay, out task))
			{
				placedOptions = overworld.GetOwBytes().ToList().Intersect(TileInfo.Required).Select(t => (int)t).ToList();
				unplacedOptions = TileInfo.Required.Select(t => (int)t).Except(placedOptions).ToList();
			}
		}
		public void UpdateTile(byte tile)
		{
			lastSelection = tile;
		}
		public List<string> GetUnplacedTiles()
		{
			return unplacedOptions.Select(t => TileInfo.Names[t]).ToList();
		}
	}

	public static class TileInfo
	{
		public static List<string> Names = new()
		{
			"LAND",
			"CONERIA_CASTLE_ENTRANCE_W",
			"CONERIA_CASTLE_ENTRANCE_E",
			"FOREST_NW",
			"FOREST_N",
			"FOREST_NE",
			"SHORE_SE",
			"SHORE_S",
			"SHORE_SW",
			"CONERIA_CASTLE_TOP_W",
			"CONERIA_CASTLE_TOP_E",
			"SMALL_CASTLE_TOP_W",
			"SMALL_CASTLE_TOP_E",
			"MIRAGE_TOP",
			"EARTH_CAVE",
			"DOCK_W",
			"MOUNTAIN_NW",
			"MOUNTAIN_N",
			"MOUNTAIN_NE",
			"FOREST_W",
			"FOREST",
			"FOREST_E",
			"SHORE_E",
			"OCEAN",
			"SHORE_W",
			"CONERIA_CASTLE_MID_W",
			"CONERIA_CASTLE_MID_E",
			"ELFLAND_CASTLE_W",
			"ELFLAND_CASTLE_E",
			"MIRAGE_BOTTOM",
			"MIRAGE_SHADOW",
			"DOCK_E",
			"MOUNTAIN_W",
			"MOUNTAIN",
			"MOUNTAIN_E",
			"FOREST_SW",
			"FOREST_S",
			"FOREST_SE",
			"SHORE_NE",
			"SHORE_N",
			"SHORE_NW",
			"ASTOS_CASTLE_W",
			"ASTOS_CASTLE_E",
			"ICE_CAVE",
			"CITY_WALL_NW",
			"CITY_WALL_N",
			"CITY_WALL_NE",
			"DWARF_CAVE",
			"MOUNTAIN_SW",
			"MOUNTAIN_S",
			"MATOYAS_CAVE",
			"MOUNTAIN_SE",
			"TITAN_CAVE_E",
			"TITAN_CAVE_W",
			"CARAVAN_DESERT",
			"AIRSHIP_DESERT",
			"ORDEALS_CASTLE_W",
			"ORDEALS_CASTLE_E",
			"SARDAS_CAVE",
			"CITY_WALL_W1",
			"CITY_WALL_W2",
			"CITY_PAVED",
			"CITY_WALL_E2",
			"CITY_WALL_E1",
			"RIVER_NW",
			"RIVER_NE",
			"DESERT_NW",
			"DESERT_NE",
			"RIVER",
			"DESERT",
			"WATERFALL",
			"TOF_TOP_W",
			"TOF_TOP_E",
			"CONERIA",
			"PRAVOKA",
			"CITY_WALL_W3",
			"ELFLAND",
			"MELMOND",
			"CRESCENT_LAKE",
			"CITY_WALL_E3",
			"RIVER_SW",
			"RIVER_SE",
			"DESERT_SW",
			"DESERT_SE",
			"GRASS",
			"MARSH",
			"TOF_BOTTOM_W",
			"TOF_ENTRANCE_W",
			"TOF_ENTRANCE_E",
			"TOF_BOTTOM_E",
			"GAIA",
			"CITY_WALL_W4",
			"CITY_WALL_SW1",
			"ONRAC",
			"CITY_WALL_SE1",
			"CITY_WALL_E4",
			"GRASS_NW",
			"GRASS_NE",
			"MARSH_NW",
			"MARSH_NE",
			"VOLCANO_TOP_W",
			"VOLCANO_TOP_E",
			"CARDIA_GRASS",
			"CARDIA_MARSH",
			"CARDIA_SMALL",
			"CARDIA_FOREST",
			"CARDIA_NORTH",
			"CITY_WALL_W5",
			"BAHAMUTS_CAVE",
			"LEFEIN",
			"MARSH_CAVE",
			"CITY_WALL_E5",
			"GRASS_SW",
			"GRASS_SE",
			"MARSH_SW",
			"MARSH_SE",
			"VOLCANO_BASE_W",
			"VOLCANO_BASE_E",
			"LAND_NO_FIGHT",
			"DOCK_SE",
			"DOCK_S",
			"DOCK_SW",
			"DOCK_SQ",
			"CITY_WALL_SW2",
			"CITY_WALL_S",
			"CITY_WALL_GATE_W",
			"CITY_WALL_GATE_E",
			"CITY_WALL_SE2",
		};

		public static List<byte> Required = new()
		{
			0x01,
			0x02,
			0x0E,
			0x1B,
			0x1C,
			0x1D,
			0x29,
			0x2A,
			0x2B,
			0x2F,
			0x32,
			0x34,
			0x35,
			0x36,
			0x37,
			0x38,
			0x39,
			0x3A,
			0x46,
			0x49,
			0x4A,
			0x4C,
			0x4D,
			0x4E,
			0x57,
			0x58,
			0x5A,
			0x5D,
			0x64,
			0x65,
			0x66,
			0x67,
			0x68,
			0x69,
			0x6A,
			0x6C,
			0x6D,
			0x6E
		};
	}
}