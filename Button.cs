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
	public class Button
	{
		public Vector2 Position { get; set; }
		public bool Show { get; set; }
		protected SpriteFont font;
		protected Texture2D button;
		protected float zoom;
		protected List<EditorTask> tasks;
		protected Vector2 textDimensions;
		protected Vector2 buttonDimensions;
		protected Color buttonColor;
		protected string buttonText;
		public Button(SpriteFont _font, string _text, Texture2D _buttonTexture, List<EditorTask> _tasks)
		{
			textDimensions = _font.MeasureString(_text);
			Show = false;
			zoom = 1.0f;
			buttonText = _text;
			font = _font;
			tasks = _tasks;
			Position = new();
			buttonDimensions = new Vector2(Math.Max((int)textDimensions.X + 20, 100), 32);
			button = _buttonTexture;
			buttonColor = new Color(255, 255, 255, 0);
		}
		public bool MouseHovering(Vector2 mousecursor)
		{
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)buttonDimensions.X, (int)buttonDimensions.Y);

			if (window.Contains(mousecursor) && Show)
			{
				buttonColor = new Color(100, 100, 100, 100);
				return true;
			}
			else
			{
				buttonColor = new Color(100, 100, 100, 50);
				return false;
			}
		}
		public List<EditorTask> OnClick(MouseState mouse)
		{
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)buttonDimensions.X, (int)buttonDimensions.Y);

			if (window.Contains(mouse.Position) && mouse.LeftClick && Show)
			{
				return tasks;
			}
			else
			{
				return new();
			}
		}
		public void Draw(SpriteBatch spriteBatch)
		{
			if (!Show)
			{
				return;
			}

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			// Draw Corners
			Vector2 cornerzoom = new Vector2(1.0f, 1.0f);
			spriteBatch.Draw(button, Position, new Rectangle(0, 0, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), cornerzoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(button, new Vector2(Position.X + buttonDimensions.X - 16, Position.Y), new Rectangle(32, 0, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), cornerzoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(button, new Vector2(Position.X, Position.Y + buttonDimensions.Y - 16), new Rectangle(0, 32, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), cornerzoom, SpriteEffects.None, 0.0f);
			spriteBatch.Draw(button, new Vector2(Position.X + buttonDimensions.X - 16, Position.Y + buttonDimensions.Y - 16), new Rectangle(32, 32, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), cornerzoom, SpriteEffects.None, 0.0f);

			// Draw Borders
			float borderwidth = Math.Max(0, (buttonDimensions.X - 32) / 16);
			float borderheight = Math.Max(0, (buttonDimensions.Y - 32) / 16);
			spriteBatch.Draw(button, new Vector2(Position.X + 16, Position.Y), new Rectangle(16, 0, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(borderwidth, 1.0f), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(button, new Vector2(Position.X + 16, Position.Y + buttonDimensions.Y - 16), new Rectangle(16, 32, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(borderwidth, 1.0f), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(button, new Vector2(Position.X, Position.Y + 16), new Rectangle(0, 16, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(1.0f, borderheight), SpriteEffects.None, 0.0f);
			spriteBatch.Draw(button, new Vector2(Position.X + buttonDimensions.X - 16, Position.Y + 16), new Rectangle(32, 16, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(1.0f, borderheight), SpriteEffects.None, 0.0f);

			// Draw Center
			spriteBatch.Draw(button, new Vector2(Position.X + 16, Position.Y + 16), new Rectangle(16, 16, 16, 16), buttonColor, 0.0f, new Vector2(0.0f, 0.0f), new Vector2(borderwidth, borderheight), SpriteEffects.None, 0.0f);

			// Draw Text
			spriteBatch.DrawString(font, buttonText, new Vector2(Position.X + (buttonDimensions.X - textDimensions.X) / 2, Position.Y + 2 + (buttonDimensions.Y - textDimensions.Y) / 2), Color.White);

			spriteBatch.End();
		}
	}
}