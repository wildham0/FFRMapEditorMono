using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System;
using System.Windows.Forms;
using FFRMapEditorMono.MysticQuest;

namespace FFRMapEditorMono
{
	public enum InfoType
	{ 
		TileCoordinates,
		CurrentLayer,
		BrushSize,

	}

	public class InfoBox
	{
		public bool Show { get; set; }
		public EditorTasks ToggleTask { get; set; }
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
		Dictionary<InfoType, string> infoText;
		public InfoBox(SpriteFont _font)
		{
			//windowTexture = _window;
			font = _font;

			ToggleTask = EditorTasks.ToggleInfoBox;

			Show = true;
			zoom = 3.0f;
			windowWidth = 28 * 8;
			windowDimensions = new(windowWidth, 100);
			//UpdatePosition(resolution);
			buttons = new();
			infoText = new()
			{
				//{ InfoType.CurrentLayer, "Layer : " + layernames[(MapLayers)0] }
			};
		}
		public InfoBox() { }
		public void ProcessTasks(TaskManager tasks)
		{
			EditorTask task;

			if (tasks.Pop(EditorTasks.InfoBoxUpdateCoordinates, out task))
			{
				int x = task.Value % 256;
				int y = task.Value / 256;

				string text = "Tile Coordinates: (" + x + ", " + y + ")";
				if (infoText.ContainsKey(InfoType.TileCoordinates))
				{
					infoText[InfoType.TileCoordinates] = text;
				}
				else
				{
					infoText.Add(InfoType.TileCoordinates, text);
				}
			}

			if (tasks.Pop(EditorTasks.InfoBoxUpdateLayer, out task))
			{
				string text = "Layer : " + layernames[(MapLayers)task.Value];
				if (infoText.ContainsKey(InfoType.CurrentLayer))
				{
					infoText[InfoType.CurrentLayer] = text;
				}
				else
				{
					infoText.Add(InfoType.CurrentLayer, text);
				}
			}

			if (tasks.Pop(EditorTasks.ToggleInfoBox, out task))
			{
				Show = !Show;
			}
		}

		static Dictionary<MapLayers, string> layernames = new()
				{
					{ MapLayers.ViewLayer1, "Layer 1 (View)" },
					{ MapLayers.ViewLayer2, "Layer 2 (View)" },
					{ MapLayers.EditLayer1, "Layer 1 (Edit)" },
					{ MapLayers.EditLayer2, "Layer 2 (Edit)" }
				};
		public void Draw(SpriteBatch spriteBatch, Point windowSize)
		{
			if (!Show || infoText.Count == 0)
			{
				return;
			}

			int maxlength = 0;
			foreach (var line in infoText)
			{
				maxlength = Math.Max(maxlength, (int)font.MeasureString(line.Value).X);
			}

			if (maxlength == 0)
			{
				return;
			}

			int initialy = (int)windowSize.Y - 10;
			int initialx = 0;

			Texture2D background = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			background.SetData(new[] { Color.DarkSlateGray });

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);
			spriteBatch.Draw(background, new Vector2(initialx, initialy), new Rectangle(0, 0, maxlength + 28, 10), new Color(255, 255, 255, 225), 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
			spriteBatch.End();

			initialy -= 20;

			foreach (var line in infoText)
			{
				spriteBatch.Begin(samplerState: SamplerState.PointClamp);

				spriteBatch.Draw(background, new Vector2(initialx, initialy), new Rectangle(0, 0, maxlength + 28, 20), new Color(255, 255, 255, 225), 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);

				spriteBatch.End();
				spriteBatch.Begin(samplerState: SamplerState.LinearClamp);

				spriteBatch.DrawString(font, line.Value, new Vector2(initialx + 10, initialy + 2), Color.White);
				spriteBatch.End();

				initialy -= 20;
			}

			initialy += 10;

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);
			spriteBatch.Draw(background, new Vector2(initialx, initialy), new Rectangle(0, 0, maxlength + 28, 10), new Color(255, 255, 255, 225), 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
			spriteBatch.End();

			//spriteBatch.End();
		}
	}
}