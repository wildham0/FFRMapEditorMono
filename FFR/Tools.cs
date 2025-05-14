using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FFRMapEditorMono.FFR
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
		Grideline,
		Selector,
		Paster,

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
			
			if (mouse.ScrollUp && brushSize < brushSizes.Count - 1)
			{
				brushSize++;
			}
			else if (mouse.ScrollDown && brushSize > 0)
			{
				brushSize--;
			}
		}
		public void GetTemplate(TaskManager tasks, byte[,] template)
		{
			EditorTask task;

			if (tasks.Pop(EditorTasks.TemplatesUpdate, out task))
			{
				Tool = ToolAction.Templates;
				Template = template;
			}
		}
		private Dictionary<EditorTasks, ToolAction> taskToTool = new()
		{
			{ EditorTasks.TilesSetTool, ToolAction.Pencil },
			{ EditorTasks.BrushesSetTool, ToolAction.Brush },
			{ EditorTasks.DocksSetTool, ToolAction.Docks },
			{ EditorTasks.DomainsSetTool, ToolAction.Domains },
			{ EditorTasks.MapObjectsSetTool, ToolAction.MapObjects },
			{ EditorTasks.SelectorSetTool, ToolAction.Selector },

		};
		public void Update(TaskManager tasks)
		{
			EditorTask task;

			foreach (var tool in taskToTool)
			{
				if (tasks.Pop(tool.Key))
				{
					Tool = tool.Value;
				}
			}

			if (tasks.Pop(EditorTasks.TilesUpdate, out task))
			{
				Tile = (byte)task.Value;
			}

			if (tasks.Pop(EditorTasks.BrushesUpdate, out task))
			{
				Tile = OwDataGroup.BrushToTile[task.Value];
			}

			if (tasks.Pop(EditorTasks.BrushesUpdateSize, out task))
			{
				if (Tool == ToolAction.Brush)
				{
					UpdateBrushSize();
				}
			}

			if (tasks.Pop(EditorTasks.PasterSetTool, out task))
			{
				previousTool = Tool;
				Tool = ToolAction.Paster;
			}

			if (tasks.Pop(EditorTasks.RestoreTool, out task))
			{
				Tool = previousTool;
			}

			if (tasks.Pop(EditorTasks.DocksUpdate, out task))
			{
				Tool = ToolAction.Docks;
				DockLocation = task.Value == (int)OverworldTeleportIndex.DefaultLocation ? OverworldTeleportIndex.None : (OverworldTeleportIndex)task.Value;
			}

			if (tasks.Pop(EditorTasks.DomainsUpdate, out task))
			{
				Tool = ToolAction.Domains;
				EncounterGroup = OwDataGroup.OwEncounterIdToGroup[task.Value];
			}

			if (tasks.Pop(EditorTasks.MapObjectsUpdate, out task))
			{
				Tool = ToolAction.MapObjects;
				MapObject = (MapObject)task.Value;
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
			optionsRows = 10;
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
			options[14] = ("Toggle Gridlines: " + size, options[14].lefttasks, options[14].righttasks);
		}
		private List<(string, List<EditorTask>, List<EditorTask>)> toolsTasks = new()
		{
			("New Map", new() { new EditorTask(EditorTasks.FileCreateNewMap, (int)WarningSetting.Trigger) }, new() { new EditorTask(EditorTasks.None) }),
			("Load Map", new() { new EditorTask(EditorTasks.FileLoadMap,(int) WarningSetting.Trigger) } , new() { new EditorTask(EditorTasks.None) }),
			("Save Map", new() { new EditorTask(EditorTasks.FileSaveMap,(int) SavingMode.Save) } , new() { new EditorTask(EditorTasks.None) }),
			("Save Map As", new() { new EditorTask(EditorTasks.FileSaveMap,(int) SavingMode.SaveAs) } , new() { new EditorTask(EditorTasks.None) }),
			("Pencil", new() { new EditorTask(EditorTasks.TilesSetTool), new EditorTask(EditorTasks.WindowsClose) }, new() { new EditorTask(EditorTasks.TilesToggle), new EditorTask(EditorTasks.TilesSetTool) }),
			("Brush: XX", new() { new EditorTask(EditorTasks.BrushesUpdateSize), new EditorTask(EditorTasks.BrushesSetTool), new EditorTask(EditorTasks.WindowsClose) }, new() { new EditorTask(EditorTasks.BrushesToggle), new EditorTask(EditorTasks.BrushesSetTool) }),
			("Templates", new() { new EditorTask(EditorTasks.TemplatesOpen), new EditorTask(EditorTasks.TemplatesUpdate) }, new() { new EditorTask(EditorTasks.None) }),
			("Domains", new() { new EditorTask(EditorTasks.DomainsOpen), new EditorTask(EditorTasks.DomainsSetTool) } , new() { new EditorTask(EditorTasks.None) }),
			("Docks", new() { new EditorTask(EditorTasks.DocksOpen), new EditorTask(EditorTasks.DocksSetTool) } , new() { new EditorTask(EditorTasks.None) }),
			("MapObjects", new() { new EditorTask(EditorTasks.MapObjectsOpen), new EditorTask(EditorTasks.MapObjectsSetTool) } , new() { new EditorTask(EditorTasks.None) }),
			("Undo", new() { new EditorTask(EditorTasks.PaintingUndo) }, new() ),
			("Redo", new() { new EditorTask(EditorTasks.PaintingRedo) }, new() ),
			("Selector", new() { new EditorTask(EditorTasks.SelectorSetTool) }, new()),
			("", new(), new() ),
			("Toggle Gridlines: XX", new() { new EditorTask(EditorTasks.ToggleGridlines) }, new() { new EditorTask(EditorTasks.UpdateGridsize) } ),
			("Toggle InfoBox", new() { new EditorTask(EditorTasks.ToggleInfoBox) }, new() ),
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