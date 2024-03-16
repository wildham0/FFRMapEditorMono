﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FFRMapEditorMono
{
	public struct Option
	{ 
		public string Name { get; set; }
		public EditorTasks Task { get; set; }
	}
	public class OptionPicker
	{
		public Vector2 Position { get; set; }
		public bool Show { get; set; }

		protected Texture2D optionsWindow;
		protected Texture2D optionSelector;
		protected Texture2D optionIcons;
		protected SpriteFont optionFont;
		protected float zoom;
		protected int optionsRows;
		protected int optionsColumns;
		protected int optionsSize;
		protected List<(string name, EditorTasks lefttask, EditorTasks righttask)> optionsBack;
		protected List<(string name, List<EditorTask> lefttasks, List<EditorTask> righttasks)> options;
		protected int lastSelection;
		protected List<int> placedOptions;
		protected List<int> unplacedOptions;
		protected bool showPlaced;
		protected List<int> optionTextLength;
		public OptionPicker(Texture2D _window, Texture2D _selector, Texture2D _optionicons, int _optionsrows, int _optionscolumns, int _optionssize, List<string> _optionnames)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionIcons = _optionicons;

			Show = true;
			Position = new Vector2(0, 0);
			zoom = 2.0f;
			optionsRows = _optionsrows;
			optionsColumns = _optionscolumns;
			optionsSize = _optionssize;
		}
		public OptionPicker() { }
		public OptionPicker(Texture2D _window, Texture2D _selector)
		{
			optionsWindow = _window;
			optionSelector = _selector;

			Show = true;
			Position = new Vector2(0, 0);
			zoom = 2.0f;
		}
		public void SetOptionTextLength()
		{
			optionTextLength = new();

			foreach (var option in options)
			{
				optionTextLength.Add((int)optionFont.MeasureString(option.name).X);
			}
		}
		public List<EditorTask> PickOption(MouseState mouse)
		{
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)(optionsWindow.Width * zoom), (int)(optionsWindow.Height * zoom));

			if (!Show || !window.Contains(mouse.Position))
			{
				return new();
			}

			if (mouse.LeftClick)
			{
				lastSelection = (int)(mouse.Position.Y - Position.Y) / (int)(optionsSize * zoom) * optionsColumns + ((int)(mouse.Position.X - Position.X) / (int)(optionsSize * zoom));
				return options[lastSelection].lefttasks.ToList();
			}
			else if (mouse.RightClick)
			{
				lastSelection = (int)(mouse.Position.Y - Position.Y) / (int)(optionsSize * zoom) * optionsColumns + ((int)(mouse.Position.X - Position.X) / (int)(optionsSize * zoom));
				return options[lastSelection].righttasks.ToList();
			}
			else
			{
				return new();
			}
		}
		public bool MouseHovering(Vector2 mousecursor)
		{
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)(optionsWindow.Width * zoom), (int)(optionsWindow.Height * zoom));

			return window.Contains(mousecursor) && Show;
		}
		public void Draw(SpriteBatch spriteBatch, SpriteFont font, Vector2 mouseCursor)
		{
			if (!Show)
			{
				return;
			}
			
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)(optionsWindow.Width * zoom), (int)(optionsWindow.Height * zoom));

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			// Draw Option Windows
			spriteBatch.Draw(optionsWindow, Position, new Rectangle(0, 0, optionsWindow.Width, optionsWindow.Height), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);

			// Draw last selection Marker
			spriteBatch.Draw(optionSelector, new Vector2(Position.X + (lastSelection % optionsColumns) * optionsSize * zoom, Position.Y + (lastSelection / optionsColumns) * optionsSize * zoom), new Rectangle(optionSelector.Height, 0, optionSelector.Height, optionSelector.Height), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);

			
			// Draw Placed Options
			if (showPlaced)
			{
				spriteBatch.End();
				var placedIcon = new Rectangle(0, 0, 16, 16);
				var unplacedIcon = new Rectangle(16, 0, 16, 16);

				spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
				foreach (var option in placedOptions)
				{
					int optionx = option % optionsColumns;
					int optiony = option / optionsColumns;

					spriteBatch.Draw(optionIcons, new Vector2(Position.X + (optionx * (optionsSize * zoom)) + ((optionsSize * zoom) / 2), Position.Y + (optiony * (optionsSize * zoom))), placedIcon, Color.White);
				}
				foreach (var option in unplacedOptions)
				{
					int optionx = option % optionsColumns;
					int optiony = option / optionsColumns;

					spriteBatch.Draw(optionIcons, new Vector2(Position.X + (optionx * (optionsSize * zoom)) + ((optionsSize * zoom) / 2), Position.Y + (optiony * (optionsSize * zoom))), unplacedIcon, Color.White);
				}
				spriteBatch.End();
				spriteBatch.Begin(samplerState: SamplerState.PointClamp);
			}

			// Draw ToolTip
			if (window.Contains(mouseCursor))
			{
				int selectionx = ((int)(mouseCursor.X - Position.X) / (int)(optionsSize * zoom));
				int selectiony = ((int)(mouseCursor.Y - Position.Y) / (int)(optionsSize * zoom));
				int currentselection = selectiony * optionsColumns + selectionx;

				spriteBatch.Draw(optionSelector, new Vector2(Position.X + selectionx * (optionsSize * zoom), Position.Y + selectiony * (optionsSize * zoom)), new Rectangle(0, 0, optionSelector.Height, optionSelector.Height), Color.White, 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);

				string optionname = options[currentselection].name;

				if (optionname != "")
				{
					Texture2D background = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
					background.SetData(new[] { Color.SandyBrown });

					Texture2D border = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
					border.SetData(new[] { Color.Black });

					int optionheight = 20;
					int optionwidth = optionTextLength[currentselection] + 8;

					spriteBatch.Draw(border, new Vector2(Position.X + selectionx * (optionsSize * zoom), Position.Y + (selectiony + 1) * (optionsSize * zoom)), new Rectangle(0, 0, optionwidth, optionheight), Color.White);
					spriteBatch.Draw(background, new Vector2(Position.X + selectionx * (optionsSize * zoom) + 1, Position.Y + (selectiony + 1) * (optionsSize * zoom) + 1), new Rectangle(0, 0, optionwidth - 2, optionheight - 2), Color.White);

					spriteBatch.End();
				
					spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
					spriteBatch.DrawString(font, optionname, new Vector2(Position.X + selectionx * (optionsSize * zoom) + 4, Position.Y + (selectiony + 1) * (optionsSize * zoom) + 2), Color.Black);
				}
			}
			spriteBatch.End();
		}
	}
}