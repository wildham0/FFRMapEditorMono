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
	public enum ToolAction
	{ 
		NewFile,
		LoadFile,
		SaveFile,
		SaveFileAs,
		Pencil,
		Brush,
		Tiles,
		Domains,
		Templates,
		Docks,
		MapObjects,
		None,
	}
	public class CurrentTool
	{ 
		public ToolAction Tool { get; set; }
		public int BrushSize { get => brushSizes[brushSize]; }
		private int brushSize;
		public OwEncounterGroup EncounterGroup { get; set; }
		public OverworldTeleportIndex DockLocation { get; set; }
		public MapObject MapObject { get; set; }
		public byte[,] Template { get; set; }
		public byte Tile { get; set; }
		private ToolAction previousTool;
		private List<int> brushSizes = new()
		{
			0, 2, 4, 6,	8, 10, 16, 32
		};
		public CurrentTool()
		{
			Tool = ToolAction.Pencil;
			previousTool = ToolAction.Pencil;
			brushSize = 2;
			Tile = 0x00;
			Template = new byte[0,0];
		}
		private Rectangle GetTileRectangle(byte tile)
		{
			int tilex = tile % 0x10;
			int tiley = tile / 0x10;

			return new Rectangle(tilex * 16, tiley * 16, 16, 16);
		}
		public void UpdateBrushSize()
		{
			brushSize++;
			if (brushSize >= brushSizes.Count)
			{
				brushSize = 0;
			}
		}
		public void UpdateBrushScroll(MouseState mouse)
		{
			if (Tool != ToolAction.Brush)
			{
				return;
			}
			
			if (mouse.ScrollUp && brushSize < (brushSizes.Count - 1))
			{
				brushSize++;
			}
			else if (mouse.ScrollDown && brushSize > 0)
			{
				brushSize--;
			}
		}
		public void GetTemplate(List<EditorTask> tasks, byte[,] template)
		{
			var validtasks = tasks.ToList();

			foreach (var task in validtasks)
			{
				if (task.Type == EditorTasks.TemplatesUpdate)
				{
					Tool = ToolAction.Templates;
					Template = template;
					tasks.Remove(task);
				}
			}
		}
		public void Update(List<EditorTask> tasks)
		{
			var validtasks = tasks.ToList();

			foreach (var task in validtasks)
			{
				if (task.Type == EditorTasks.TilesUpdate)
				{
					Tile = (byte)task.Value;
					tasks.Remove(task);
				}
				if (task.Type == EditorTasks.TilesSetTool)
				{
					Tool = ToolAction.Pencil;
					tasks.Remove(task);
				}
				if (task.Type == EditorTasks.BrushesUpdate)
				{
					Tile = OwDataGroup.BrushToTile[task.Value];
					tasks.Remove(task);
				}
				if (task.Type == EditorTasks.BrushesSetTool)
				{
					Tool = ToolAction.Brush;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.BrushesUpdateSize)
				{
					if (Tool == ToolAction.Brush)
					{
						UpdateBrushSize();
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.DocksSetTool)
				{
					Tool = ToolAction.Docks;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.DocksUpdate)
				{
					Tool = ToolAction.Docks;
					DockLocation = (task.Value == (int)OverworldTeleportIndex.DefaultLocation) ? OverworldTeleportIndex.None : (OverworldTeleportIndex)task.Value;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.DomainsUpdate)
				{
					Tool = ToolAction.Domains;
					EncounterGroup = OwDataGroup.OwEncounterIdToGroup[task.Value];
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.DomainsSetTool)
				{
					Tool = ToolAction.Domains;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.MapObjectsUpdate)
				{
					Tool = ToolAction.MapObjects;
					MapObject = (MapObject)task.Value;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.MapObjectsSetTool)
				{
					Tool = ToolAction.MapObjects;
					tasks.Remove(task);
				}
			}
		}
	}
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
		private List<(string, List<EditorTask>, List<EditorTask>)> toolsTasks = new()
		{
			("New Map", new() { new EditorTask() { Type = EditorTasks.FileCreateNewMap, Value = (int)WarningSetting.Trigger } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("Load Map", new() { new EditorTask() { Type = EditorTasks.FileLoadMap, Value = (int)WarningSetting.Trigger } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("Save Map", new() { new EditorTask() { Type = EditorTasks.FileSaveMap, Value = (int)SavingMode.Save } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("Save Map As", new() { new EditorTask() { Type = EditorTasks.FileSaveMap, Value = (int)SavingMode.SaveAs } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("Pencil", new() { new EditorTask() { Type = EditorTasks.TilesSetTool }, new EditorTask() { Type = EditorTasks.WindowsClose } }, new() { new EditorTask() { Type = EditorTasks.TilesToggle }, new EditorTask() { Type = EditorTasks.TilesSetTool } }),
			("Brush: XX", new() { new EditorTask() { Type = EditorTasks.BrushesUpdateSize }, new EditorTask() { Type = EditorTasks.BrushesSetTool }, new EditorTask() { Type = EditorTasks.WindowsClose } }, new() { new EditorTask() { Type = EditorTasks.BrushesToggle }, new EditorTask() { Type = EditorTasks.BrushesSetTool } }),
			("Templates", new() { new EditorTask() { Type = EditorTasks.TemplatesOpen }, new EditorTask() { Type = EditorTasks.TemplatesUpdate } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("Domains", new() { new EditorTask() { Type = EditorTasks.DomainsOpen }, new EditorTask() { Type = EditorTasks.DomainsSetTool } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("Docks", new() { new EditorTask() { Type = EditorTasks.DocksOpen }, new EditorTask() { Type = EditorTasks.DocksSetTool } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("MapObjects", new() { new EditorTask() { Type = EditorTasks.MapObjectsOpen }, new EditorTask() { Type = EditorTasks.MapObjectsSetTool } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("Undo", new() { new EditorTask() { Type = EditorTasks.PaintingUndo } }, new() ),
			("Redo", new() { new EditorTask() { Type = EditorTasks.PaintingRedo } }, new() ),
			("", new(), new() ),
			("", new(), new() ),
			("Exit", new() { new EditorTask() { Type = EditorTasks.ExitProgram } }, new() { new EditorTask() { Type = EditorTasks.None } }),
			("Info", new() { new EditorTask() { Type = EditorTasks.ToggleInfoWindow } }, new() { new EditorTask() { Type = EditorTasks.None } }),
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