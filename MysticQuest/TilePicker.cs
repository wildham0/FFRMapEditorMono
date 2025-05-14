using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono.MysticQuest
{
	public class TilePickerMQ : OptionPicker
	{
		private CanvasMQ overworld;
		public TilePickerMQ(Texture2D _selector, Texture2D _placedicons, SpriteFont _font, CanvasMQ _overworld)
		{
			optionsWindow = _overworld.TileSet;
			optionSelector = _selector;
			optionIcons = _placedicons;
			optionFont = _font;
			overworld = _overworld;

			Show = false;
			ToggleTask = EditorTasks.TilesToggle;
			Position = new Vector2(64, 0);
			zoom = 2.0f;
			optionsRows = 0x08;
			optionsColumns = 0x10;
			optionsSize = 16;

			//var tiles = Enumerable.Range(0, 0x80).ToList();

			var tiles = _overworld.Tiles;
			options = tiles.Select((t, i) => ($"{t.PropertyByte1:X2} {t.PropertyByte2:X2}",
				new List<EditorTask>() {
					new EditorTask(EditorTasks.TilesUpdate, i),
					new EditorTask(EditorTasks.WindowsClose, 10) },
				new List<EditorTask>() {
					new EditorTask(EditorTasks.TilesUpdate, i) }
				)).ToList();

			SetOptionTextLength();
			lastSelection = 0x00;
			placedOptions = new();
			unplacedOptions = requiredTiles.Select(t => (int)t).ToList();
			showPlaced = false;
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
				placedOptions = overworld.GetOwBytes().ToList().Intersect(requiredTiles).Select(t => (int)t).ToList();
				unplacedOptions = requiredTiles.Select(t => (int)t).Except(placedOptions).ToList();
			}

			if (tasks.Pop(EditorTasks.ReloadPicker, out task))
			{
				optionsWindow = overworld.TileSet;
				var tiles = overworld.Tiles;
				options = tiles.Select((t, i) => ($"{t.PropertyByte1:X2} {t.PropertyByte2:X2}",
					new List<EditorTask>() {
						new EditorTask(EditorTasks.TilesUpdate, i),
						new EditorTask(EditorTasks.WindowsClose, 10) },
					new List<EditorTask>() {
						new EditorTask(EditorTasks.TilesUpdate, i) }
					)).ToList();

				SetOptionTextLength();
			}
		}
		public void UpdateTile(byte tile)
		{
			lastSelection = tile;
		}
		private List<byte> requiredTiles = new()
		{

		};
		public List<string> GetUnplacedTiles()
		{
			return unplacedOptions.Select(t => TileNames[t]).ToList();
		}
		private List<string> TileNames = new()
		{

		};

	}

}
