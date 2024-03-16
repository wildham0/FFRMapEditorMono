using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System;

namespace FFRMapEditorMono
{
	public class ExitWarningWindow
	{
		public Vector2 Position { get; set; }
		public bool Show { get; set; }
		protected Texture2D infoTexture;
		protected SpriteFont font;
		protected float zoom;
		protected Button okButton;
		protected Button cancelButton;
		protected string warningText;
		protected Vector2 windowDimensions;
		protected int windowWidth;
		public ExitWarningWindow(Texture2D _window, SpriteFont _font, Texture2D _buttonTexture, Point resolution)
		{
			infoTexture = _window;
			font = _font;

			Show = false;
			zoom = 3.0f;
			windowWidth = 28 * 8;
			windowDimensions = new(windowWidth, 100);
			warningText = "*** Warning *** \n\nYou have unsaved changes.";
			
			var windowHeight = warningText.Count(c => c == '\n') * 8 + 64;
			windowDimensions = new Vector2(windowWidth, windowHeight);

			UpdatePosition(resolution);
			okButton = new(font, "Exit anyway", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.ExitProgramHard } });
			cancelButton = new(font, "Return to editor", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.ExitWarningClose, Value = 10 } });
		}
		public void UpdatePosition(Point newres)
		{
			Position = new Vector2((newres.X - (windowDimensions.X * zoom)) / 2, (newres.Y - (windowDimensions.Y * zoom)) / 2);
		}
		public List<EditorTask> ProcessInput(MouseState mouse)
		{
			List<EditorTask> buttontasks = new();

			okButton.Show = Show;
			cancelButton.Show = Show;
			buttontasks.AddRange(okButton.OnClick(mouse));
			buttontasks.AddRange(cancelButton.OnClick(mouse));

			return buttontasks;
		}
		public bool MouseHovering(Vector2 mousecursor)
		{
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)(windowDimensions.X * zoom), (int)(windowDimensions.Y * zoom));

			okButton.MouseHovering(mousecursor);
			cancelButton.MouseHovering(mousecursor);

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

			// Draw Text
			spriteBatch.DrawString(font, warningText, new Vector2(Position.X + 20, Position.Y + 20), Color.White);

			spriteBatch.End();

			// Draw Buttons
			okButton.Position = new Vector2(Position.X + 20, Position.Y + (windowDimensions.Y * zoom - 104));
			cancelButton.Position = new Vector2(Position.X + 20, Position.Y + (windowDimensions.Y * zoom - 64));
			okButton.Draw(spriteBatch);
			cancelButton.Draw(spriteBatch);
		}
	}

	public class SaveWarningWindow
	{
		public Vector2 Position { get; set; }
		public bool Show { get; set; }

		protected Texture2D infoTexture;
		protected SpriteFont font;
		protected float zoom;
		protected Button okButton;
		protected Button cancelButton;
		protected SavingMode saveMode;
		protected string warningText;
		protected Vector2 windowDimensions;
		protected int windowWidth;
		public SaveWarningWindow(Texture2D _window, SpriteFont _font, Texture2D _buttonTexture, Point resolution)
		{
			infoTexture = _window;
			font = _font;

			Show = false;
			zoom = 3.0f;
			windowWidth = 28 * 8;
			windowDimensions = new(windowWidth, 100);
			UpdatePosition(resolution);
			okButton = new(font, "Save anyway", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.SaveNoWarning, Value = (int)saveMode }, new EditorTask() { Type = EditorTasks.SaveWarningClose, Value = 10 } });
			cancelButton = new(font, "Return to editor", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.SaveWarningClose, Value = 10 } });
		}
		public void UpdatePosition(Point newres)
		{
			Position = new Vector2((newres.X - (windowDimensions.X * zoom)) / 2, (newres.Y - (windowDimensions.Y * zoom)) / 2);
		}
		public void ProcessTasks(Overworld overworld, List<string> missingTiles, Point resolution, List<EditorTask> tasks)
		{
			var validtasks = tasks.ToList();
			foreach (var task in validtasks)
			{
				if (task.Type == EditorTasks.SetSavingMode)
				{
					saveMode = (SavingMode)task.Value;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.SaveWarningUpdate)
				{

					saveMode = (SavingMode)task.Value;

					var validationresult = overworld.ValidateObjects();

					warningText = "*** Warning *** \n\n";

					if (!validationresult.defaultdock)
					{
						warningText += "Default dock hasn't been placed.\n\n";
					}

					var missingobjects = validationresult.missingmapobjects;
					if (missingobjects.Any())
					{
						string currentline = "";
						//int

						currentline += "Missing Map Objects: ";
						for (int i = 0; i < missingobjects.Count; i++)
						{
							string objectname = Enum.GetName(missingobjects[i]);

							if (currentline.Length + objectname.Length + 2 > 60)
							{
								warningText += currentline + "\n";
								currentline = "  " + objectname;
							}
							else
							{
								currentline += objectname;
							}

							if (i < (missingobjects.Count - 1))
							{
								currentline += ", ";
							}
							else
							{
								currentline += ".";
							}
						}

						warningText += currentline + "\n\n";
					}
					//var missingtiles = validationresult.missingmapobjects;
					if (missingTiles.Any())
					{
						string currentline = "";

						currentline += "Missing Required Tiles: ";
						for (int i = 0; i < missingTiles.Count; i++)
						{
							string tilename = missingTiles[i];

							if (currentline.Length + tilename.Length + 2 > 60)
							{
								warningText += currentline + "\n";
								currentline = "  " + tilename;
							}
							else
							{
								currentline += tilename;
							}

							if (i < (missingTiles.Count - 1))
							{
								currentline += ", ";
							}
							else
							{
								currentline += ".";
							}
						}

						warningText += currentline + "\n\n";
					}

					var windowHeight = warningText.Count(c => c == '\n') * 8 + 32;
					windowDimensions = new Vector2(windowWidth, windowHeight);
					UpdatePosition(resolution);
					tasks.Remove(task);
				}
				

			}
		}
		public List<EditorTask> ProcessInput(MouseState mouse)
		{
			List<EditorTask> buttontasks = new();
			okButton.Show = Show;
			cancelButton.Show = Show;
			buttontasks.AddRange(okButton.OnClick(mouse));
			buttontasks.AddRange(cancelButton.OnClick(mouse));

			return buttontasks;
		}
		public bool MouseHovering(Vector2 mousecursor)
		{
			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)(windowDimensions.X * zoom), (int)(windowDimensions.Y * zoom));

			okButton.MouseHovering(mousecursor);
			cancelButton.MouseHovering(mousecursor);

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

			// Draw Text
			spriteBatch.DrawString(font, warningText, new Vector2(Position.X + 20, Position.Y + 20), Color.White);

			spriteBatch.End();

			// Draw Buttons
			okButton.Position = new Vector2(Position.X + 20, Position.Y + (windowDimensions.Y * zoom - 104));
			cancelButton.Position = new Vector2(Position.X + 20, Position.Y + (windowDimensions.Y * zoom - 64));
			okButton.Draw(spriteBatch);
			cancelButton.Draw(spriteBatch);
		}
	}
}