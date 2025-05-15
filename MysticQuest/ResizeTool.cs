using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono.MysticQuest
{
	public class MapResize : OptionPicker
	{
		public MapResize(Texture2D _window, Texture2D _selector, SpriteFont _font, SpriteBatch _spriteBatch, TaskManager _tasks, MouseState _mouse) : base(_font, _spriteBatch, _tasks, _mouse)
		{
			optionsWindow = _window;
			optionSelector = _selector;

			Show = false;
			Position = new Vector2(64, 0);
			zoom = 1.0f;
			optionsRows = 0x02;
			optionsColumns = 0x08;
			optionsSize = 32;

			ToggleTask = EditorTasks.ResizeToggle;

			options = optionName.Select((t, i) => (t,
				new List<EditorTask>() {
					new EditorTask(EditorTasks.ResizeMap, i),
					new EditorTask(EditorTasks.WindowsClose, 10) },
				new List<EditorTask>() {
					new EditorTask(EditorTasks.None) }
				)).ToList();

			SetOptionTextLength();
			lastSelection = 0x00;
			placedOptions = new();
			showPlaced = false;
		}

		private List<string> optionName = new()
		{
			"10x10",
			"20x10",
			"30x10",
			"40x10",
			"10x20",
			"20x20",
			"30x20",
			"40x20",
			"10x30",
			"20x30",
			"30x30",
			"40x30",
			"10x40",
			"20x40",
			"30x40",
			"40x40",
		};

		public override void ProcessTasks()
		{
			EditorTask task;

			if (taskManager.Pop(EditorTasks.ResizeUpdateSelection, out task))
			{
				lastSelection = task.Value;
			}
		}
	}
}
