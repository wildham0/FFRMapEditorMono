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
		private CanvasMQ canvas;
		public TilePickerMQ(Texture2D _selector, Texture2D _placedicons, CanvasMQ _canvas, SpriteFont _font, SpriteBatch _spriteBatch, TaskManager _tasks, MouseState _mouse) : base(_font, _spriteBatch, _tasks, _mouse)
		{
			optionsWindow = _canvas.TileSet;
			optionSelector = _selector;
			optionIcons = _placedicons;
			canvas = _canvas;

			Show = false;
			ToggleTask = EditorTasks.TilesToggle;
			Position = new Vector2(64, 0);
			zoom = 2.0f;
			optionsRows = 0x08;
			optionsColumns = 0x10;
			optionsSize = 16;

			var tiles = _canvas.Tiles;
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
			unplacedOptions = new();
			showPlaced = false;
		}
		public override void ProcessTasks()
		{
			EditorTask task;

			if (taskManager.Pop(EditorTasks.TilesPickerUpdate, out task))
			{
				lastSelection = task.Value;
			}

			if (taskManager.Pop(EditorTasks.UpdatePlacedTilesOverlay, out task))
			{

			}

			if (taskManager.Pop(EditorTasks.ReloadPicker, out task))
			{
				optionsWindow = canvas.TileSet;
				var tiles = canvas.Tiles;
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
	}
}
