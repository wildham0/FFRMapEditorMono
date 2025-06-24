using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono.MysticQuest
{
	public class TilesViewer : OptionPicker
	{
		private CanvasMQ canvas;
		public TilesViewer(Texture2D _selector, Texture2D _placedicons, CanvasMQ _canvas, SpriteFont _font, SpriteBatch _spriteBatch, TaskManager _tasks, MouseState _mouse) : base(_font, _spriteBatch, _tasks, _mouse)
		{
			optionsWindow = _canvas.TilesGraphics;
			optionSelector = _selector;
			optionIcons = _placedicons;
			canvas = _canvas;

			Show = false;
			ToggleTask = EditorTasks.ToggleTileViewer;
			Position = new Vector2(64, 0);
			zoom = 2.0f;
			optionsRows = 0x08;
			optionsColumns = 0x20;
			optionsSize = 8;

			options = new();
			/*
			var tiles = _canvas.Tiles;
			options = tiles.Select((t, i) => ($"{t.PropertyByte1:X2} {t.PropertyByte2:X2}",
				new List<EditorTask>() { },
				new List<EditorTask>() )).ToList();*/

			//SetOptionTextLength();
			lastSelection = 0x00;
			placedOptions = new();
			unplacedOptions = new();
			showPlaced = false;
		}
		public override void ProcessTasks()
		{
			EditorTask task;

			if (taskManager.Pop(EditorTasks.ReloadTileViewer, out task))
			{
				optionsWindow = canvas.TilesGraphics;
			}

		}
	}
}
