using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System;

namespace FFRMapEditorMono
{
	public class LoadMapWarningWindow : WarningWindow
	{
		public LoadMapWarningWindow(Texture2D _window, SpriteFont _font, Texture2D _buttonTexture, Point resolution)
		{
			Type = WarningType.LoadMap;

			windowTexture = _window;
			font = _font;

			Show = false;
			zoom = 3.0f;
			windowWidth = 28 * 8;
			windowDimensions = new(windowWidth, 100);
			warningText = "*** Warning *** \n\nYou have unsaved changes.";
			
			var windowHeight = warningText.Count(c => c == '\n') * 8 + 64;
			windowDimensions = new Vector2(windowWidth, windowHeight);

			buttons = new()
			{
				new(font, "Load a new map anyway", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.FileLoadMap, Value = (int)WarningSetting.Disabled }, new EditorTask() { Type = EditorTasks.LoadMapWarningClose, Value = 10 } }),
				new(font, "Return to editor", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.LoadMapWarningClose, Value = 10 } })
			};
			
			UpdatePosition(resolution);
		}
	}
}