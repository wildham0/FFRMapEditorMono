using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FFRMapEditorMono
{
	public class InfoWindow
	{
		public Vector2 Position { get; set; }
		public bool Show { get; set; }
		protected Texture2D infoTexture;
		protected SpriteFont font;

		protected float zoom;
		protected List<(string name, EditorTasks lefttask, EditorTasks righttask)> options;
		protected Button okButton;
		protected Vector2 windowDimensions;
		protected int windowWidth;
		protected string windowText;
		public InfoWindow(Texture2D _window, SpriteFont _font, Texture2D _buttonTexture, Point resolution)
		{
			infoTexture = _window;
			font = _font;

			Show = false;
			zoom = 3.0f;
			windowWidth = 28 * 8;
			okButton = new(_font, "OK", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.ToggleInfoWindow, Value = 10 } });

			windowText = "FFR Map Editor v1.0\n\n" +
				"Main Developer\n   wildham\n\n" +
				"Icons Designer\n   DarkmoonEX\n\n" +
				"Based on the work of\n" +
				"   tetron (Overworld Map Format)\n" +
				"   madmartin (Enabling Logic)\n\n" +
				"Source code at\n" +
				"   http...\n\n" +
				"Randomizer\n" +
				"   ffrando.com";

			var windowHeight = windowText.Count(c => c == '\n') * 8;
			windowDimensions = new Vector2(windowWidth, windowHeight);
			UpdatePosition(resolution);
		}
		public void UpdatePosition(Point newres)
		{
			Position = new Vector2((newres.X - (windowDimensions.X * zoom)) / 2, (newres.Y - (windowDimensions.Y * zoom)) / 2);
		}
		public List<EditorTask> ProcessInput(MouseState mouse)
		{
			okButton.Show = Show;
			return okButton.OnClick(mouse);
		}
		public bool MouseHovering(Vector2 mousecursor)
		{
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)(windowDimensions.X * zoom), (int)(windowDimensions.Y * zoom));
			
			okButton.MouseHovering(mousecursor);

			return window.Contains(mousecursor) && Show;
		}
		public void Draw(SpriteBatch spriteBatch)
		{
			if (!Show)
			{
				return;
			}
			
			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			// Draw Corners
			spriteBatch.Draw(infoTexture, Position, new Rectangle(0, 0, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(infoTexture, new Vector2(Position.X + (windowDimensions.X - 8) * zoom, Position.Y), new Rectangle(16, 0, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(infoTexture, new Vector2(Position.X, Position.Y + (windowDimensions.Y - 8) * zoom), new Rectangle(0, 16, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(infoTexture, new Vector2(Position.X + (windowDimensions.X - 8) * zoom, Position.Y + (windowDimensions.Y - 8) * zoom), new Rectangle(16, 16, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);

			// Draw Borders
			float borderwidth = Math.Max(0, (windowDimensions.X - 16) / 8);
			float borderheight = Math.Max(0, (windowDimensions.Y - 16) / 8);
			spriteBatch.Draw(infoTexture, new Vector2(Position.X + (8 * zoom), Position.Y), new Rectangle(8, 0, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom * borderwidth, zoom), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(infoTexture, new Vector2(Position.X + (8 * zoom), Position.Y + (windowDimensions.Y - 8) * zoom), new Rectangle(8, 16, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom * borderwidth, zoom), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(infoTexture, new Vector2(Position.X, Position.Y + (8 * zoom)), new Rectangle(0, 8, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom, zoom * borderheight), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(infoTexture, new Vector2(Position.X + (windowDimensions.X - 8) * zoom, Position.Y + (8 * zoom)), new Rectangle(16, 8, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom, zoom * borderheight), SpriteEffects.None, 0.0f);

			// Draw Center
			spriteBatch.Draw(infoTexture, new Vector2(Position.X + (8 * zoom), Position.Y + (8 * zoom)), new Rectangle(8, 8, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom * borderwidth, zoom * borderheight), SpriteEffects.None, 0.0f);

			spriteBatch.DrawString(font, windowText, new Vector2(Position.X + 20, Position.Y + 20), Color.White);
			//spriteBatch.DrawString(font, infotextright, new Vector2(Position.X + (infoTexture.Width * zoom / 2), Position.Y + 20), Color.White);

			spriteBatch.End();
			
			okButton.Position = new Vector2(Position.X + (windowDimensions.X * zoom - 144), Position.Y + (windowDimensions.Y * zoom - 64));
			okButton.Draw(spriteBatch);
		}
	}
}