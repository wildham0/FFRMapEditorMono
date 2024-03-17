using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System;

namespace FFRMapEditorMono
{
	public enum WarningType
	{ 
		Exit,
		NewMap,
		LoadMap,
		SaveValidation

	}

	public class WarningWindow
	{
		public Vector2 Position { get; set; }
		public bool Show { get; set; }
		public WarningType Type { get; set; }

		protected Texture2D windowTexture;
		protected SpriteFont font;
		protected float zoom;
		protected Button okButton;
		protected Button cancelButton;
		protected SavingMode saveMode;
		protected string warningText;
		protected Vector2 windowDimensions;
		protected int windowWidth;
		protected List<Button> buttons;
		public WarningWindow(Texture2D _window, SpriteFont _font, Texture2D _buttonTexture, Point resolution)
		{
			windowTexture = _window;
			font = _font;

			Show = false;
			zoom = 3.0f;
			windowWidth = 28 * 8;
			windowDimensions = new(windowWidth, 100);
			UpdatePosition(resolution);
			buttons = new();
		}
		public WarningWindow() { }
		public void UpdatePosition(Point newres)
		{
			Position = new Vector2((newres.X - (windowDimensions.X * zoom)) / 2, (newres.Y - (windowDimensions.Y * zoom)) / 2);
		}
		public List<EditorTask> ProcessInput(MouseState mouse)
		{
			List<EditorTask> buttontasks = new();
			foreach (var button in buttons)
			{
				button.Show = Show;
				buttontasks.AddRange(button.OnClick(mouse));
			}

			return buttontasks;
		}
		public bool MouseHovering(Vector2 mousecursor)
		{
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)(windowDimensions.X * zoom), (int)(windowDimensions.Y * zoom));

			foreach (var button in buttons)
			{
				button.MouseHovering(mousecursor);
			}

			return window.Contains(mousecursor) && Show;
		}
		public void Draw(SpriteBatch spriteBatch)
		{
			if (!Show)
			{
				return;
			}

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			// Draw Window
			// Draw Corners
			spriteBatch.Draw(windowTexture, Position, new Rectangle(0, 0, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(windowTexture, new Vector2(Position.X + (windowDimensions.X - 8) * zoom, Position.Y), new Rectangle(16, 0, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(windowTexture, new Vector2(Position.X, Position.Y + (windowDimensions.Y - 8) * zoom), new Rectangle(0, 16, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(windowTexture, new Vector2(Position.X + (windowDimensions.X - 8) * zoom, Position.Y + (windowDimensions.Y - 8) * zoom), new Rectangle(16, 16, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);

			// Draw Borders
			float borderwidth = Math.Max(0, (windowDimensions.X - 16) / 8);
			float borderheight = Math.Max(0, (windowDimensions.Y - 16) / 8);
			spriteBatch.Draw(windowTexture, new Vector2(Position.X + (8 * zoom), Position.Y), new Rectangle(8, 0, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom * borderwidth, zoom), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(windowTexture, new Vector2(Position.X + (8 * zoom), Position.Y + (windowDimensions.Y - 8) * zoom), new Rectangle(8, 16, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom * borderwidth, zoom), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(windowTexture, new Vector2(Position.X, Position.Y + (8 * zoom)), new Rectangle(0, 8, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom, zoom * borderheight), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(windowTexture, new Vector2(Position.X + (windowDimensions.X - 8) * zoom, Position.Y + (8 * zoom)), new Rectangle(16, 8, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom, zoom * borderheight), SpriteEffects.None, 0.0f);

			// Draw Center
			spriteBatch.Draw(windowTexture, new Vector2(Position.X + (8 * zoom), Position.Y + (8 * zoom)), new Rectangle(8, 8, 8, 8), Color.White, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(zoom * borderwidth, zoom * borderheight), SpriteEffects.None, 0.0f);

			// Draw Text
			spriteBatch.DrawString(font, warningText, new Vector2(Position.X + 20, Position.Y + 20), Color.White);

			spriteBatch.End();

			// Draw Buttons
			int ydecal = (buttons.Count() - 1) * 40;
			foreach (var button in buttons)
			{ 
				button.Position = new Vector2(Position.X + 20, Position.Y + (windowDimensions.Y * zoom - (64 + ydecal)));
				button.Draw(spriteBatch);
				ydecal -= 40;
			}
		}
	}
}