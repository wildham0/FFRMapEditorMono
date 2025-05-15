using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Drawing;

namespace FFRMapEditorMono.FFR
{
	public class MapObjectPicker : OptionPicker
	{
		private CanvasFFR overworld;
		public MapObjectPicker(Texture2D _window, Texture2D _selector, Texture2D _placingicons, Canvas _overworld, SpriteFont _font, SpriteBatch _spriteBatch, TaskManager _tasks, MouseState _mouse) : base(_font, _spriteBatch, _tasks, _mouse)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionIcons = _placingicons;


			overworld = (CanvasFFR)_overworld;
			Position = new Vector2(64, 0);
			zoom = 2.0f;
			optionsRows = 1;
			optionsColumns = 5;
			optionsSize = 16;

			options = mapObjectsNames.Select((o, i) => (o.Item1,
				new List<EditorTask>() {
					new EditorTask(EditorTasks.MapObjectsUpdate, i) },
				new List<EditorTask>() {
					new EditorTask(EditorTasks.MapObjectsRemove, i) }
				)).ToList();

			Show = false;
			lastSelection = 0x00;

			placedOptions = new();
			unplacedOptions = new();
			SetOptionTextLength();
			showPlaced = true;
		}

		private List<(string, EditorTasks, EditorTasks)> mapObjectsNames = new()
		{
			("Starting Position", EditorTasks.None, EditorTasks.None),
			("Bridge", EditorTasks.None, EditorTasks.None),
			("Canal", EditorTasks.None, EditorTasks.None),
			("Ship (Unused)", EditorTasks.None, EditorTasks.None),
			("Airship", EditorTasks.None, EditorTasks.None),
		};
		public override void ProcessTasks()
		{
			EditorTask task;

			if (taskManager.Pop(EditorTasks.UpdatePlacedObjectsOverlay, out task))
			{
				placedOptions = overworld.GetPlacedMapObjects().Select(o => (int)o).ToList();
			}
		}
	}
}