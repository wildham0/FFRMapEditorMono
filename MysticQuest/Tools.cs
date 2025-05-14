using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono.MysticQuest
{
	public class ToolsMenu : OptionPicker
	{
		public ToolsMenu(Texture2D _toolstexture, Texture2D _selector, SpriteFont _font)
		{
			optionsWindow = _toolstexture;
			optionSelector = _selector;
			optionFont = _font;

			Show = true;
			Position = new Vector2(0, 0);
			zoom = 1.0f;
			optionsRows = 5;
			optionsColumns = 2;
			optionsSize = 32;

			options = toolsTasks;

			Show = true;
			showPlaced = false;
			SetOptionTextLength();
			lastSelection = 0x00;
		}
		public void UpdateBrushSize(int size)
		{
			options[5] = ("Brush: " + size, options[5].lefttasks, options[5].righttasks);
		}
		public void UpdateGridSize(int size)
		{
			options[12] = ("Toggle Gridlines: " + size, options[12].lefttasks, options[12].righttasks);
		}
		private List<(string, List<EditorTask>, List<EditorTask>)> toolsTasks = new()
		{
			("New Map", new() { new EditorTask(EditorTasks.FileCreateNewMap, (int)WarningSetting.Trigger) }, new() { new EditorTask(EditorTasks.None) }),
			("Load Map", new() { new EditorTask(EditorTasks.FileLoadMap, (int)WarningSetting.Trigger) }, new() { new EditorTask(EditorTasks.None) }),
			("Save Map", new() { new EditorTask(EditorTasks.FileSaveMap, (int)SavingMode.Save) }, new() { new EditorTask(EditorTasks.None) }),
			("Save Map As", new() { new EditorTask(EditorTasks.FileSaveMap, (int)SavingMode.SaveAs) }, new() { new EditorTask(EditorTasks.None) }),
			("Pencil", new() { new EditorTask(EditorTasks.TilesSetTool), new EditorTask(EditorTasks.WindowsClose) }, new() { new EditorTask(EditorTasks.TilesToggle), new EditorTask(EditorTasks.TilesSetTool) }),
			("Toggle Layer", new() { new EditorTask(EditorTasks.ToggleLayer, 1) }, new() { new EditorTask(EditorTasks.ToggleLayer, -1) }),
			("Resize", new() { new EditorTask(EditorTasks.ResizeToggle) }, new() { new EditorTask(EditorTasks.None) }),
			("Selector", new() { new EditorTask(EditorTasks.SelectorSetTool) }, new() { new EditorTask(EditorTasks.None) }),
			("Docks", new() { new EditorTask(EditorTasks.DocksOpen), new EditorTask(EditorTasks.DocksSetTool) }, new() { new EditorTask(EditorTasks.None) }),
			("MapObjects", new() { new EditorTask(EditorTasks.MapObjectsOpen), new EditorTask(EditorTasks.MapObjectsSetTool) }, new() { new EditorTask(EditorTasks.None) }),
			("Undo", new() { new EditorTask(EditorTasks.PaintingUndo) }, new() ),
			("Redo", new() { new EditorTask(EditorTasks.PaintingRedo) }, new() ),
			("Toggle Gridlines: XX", new() { new EditorTask(EditorTasks.ToggleGridlines) }, new() { new EditorTask(EditorTasks.UpdateGridsize) } ),
			("Toggle Coordinates", new() { new EditorTask(EditorTasks.TogglePositionIndicator) }, new() ),
			("", new(), new() ),
			("", new(), new() ),
			("", new(), new() ),
			("", new(), new() ),
			("Exit", new() { new EditorTask(EditorTasks.ExitProgram) }, new() { new EditorTask(EditorTasks.None) }),
			("Info", new() { new EditorTask(EditorTasks.ToggleInfoWindow) }, new() { new EditorTask(EditorTasks.None) }),
		};

		public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Vector2 mouseCursor)
		{

			if (!Show)
			{
				return;
			}

			Rectangle window = new Rectangle((int)Position.X, (int)Position.Y, (int)(optionsWindow.Width * zoom), (int)(optionsWindow.Height * zoom));

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			// Draw Background
			Texture2D background = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			background.SetData(new[] { Color.DarkSlateGray });


			spriteBatch.Draw(background, Position, new Rectangle(0, 0, optionsWindow.Width, optionsWindow.Height), new Color(255, 255, 255, 225), 0.0f, new Vector2(0.0f, 0.0f), zoom, SpriteEffects.None, 0.0f);

			spriteBatch.End();

			base.Draw(spriteBatch, font, mouseCursor);
		}
	}
}
