using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.DirectoryServices.ActiveDirectory;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using FFRMapEditorMono.MysticQuest;
using FFRMapEditorMono.FFR;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;

namespace FFRMapEditorMono
{
	public enum PositionIndicatorMode
	{ 
		None,
		Cursor,
		Corner,
	}

	public class Canvas
	{
		protected GraphicsDevice graphicsDevice;
		protected SpriteBatch spriteBatch;
		protected MouseState mouse;
		protected KeyboardState keyboard;
		protected FileManager fileManager;
		protected TaskManager taskManager;

		public Texture2D TileSet { get; set; }
		protected RenderTarget2D mapTexture;
		protected byte[] mapCanvas { get => targetMaps[owMapCurrentTarget]; }
		protected Vector2 viewOffset;
		protected SpriteFont font;
		public float Zoom { get; set; }
		public int GridSize { get; set; }
		protected int MapSizeX;
		protected int MapSizeY;
		protected PositionIndicatorMode positionMode;

		protected int owMapCurrentTarget;
		protected int owMapBackSteps;
		protected int owMapForwardSteps;
		protected int owMapUndoDepth;
		protected bool currentlyDrawing;
		protected bool currentlySelecting;
		protected List<byte[]> targetMaps;
		
		protected byte[,] pasteBin;
		protected Selector selector;

		protected Point currentTileCoordinates => new Point((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom), (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));
		protected Point previousTileCoordinates;
		public bool UnsavedChanges { get; set; }
		public Canvas(Dictionary<string, Texture2D> _textures, SpriteFont _font, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, FileManager _fileManager, TaskManager _taskmanager, MouseState _mouse, KeyboardState _keyboard)
		{
			BaseInitialization(_textures, _font, _graphicsDevice, _spriteBatch, _fileManager, _taskmanager, _mouse, _keyboard);
		}
		public void BaseInitialization(Dictionary<string, Texture2D> _textures, SpriteFont _font, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, FileManager _fileManager, TaskManager _taskmanager, MouseState _mouse, KeyboardState _keyboard)
		{
			// Globals
			graphicsDevice = _graphicsDevice;
			spriteBatch = _spriteBatch;

			fileManager = _fileManager;
			taskManager = _taskmanager;
			mouse = _mouse;
			keyboard = _keyboard;
			font = _font;

			// Parameters
			owMapCurrentTarget = 0;
			owMapBackSteps = 0;
			owMapForwardSteps = 0;

			GridSize = 32;
			MapSizeX = 256;
			MapSizeY = 256;
			currentlyDrawing = false;
			currentlySelecting = false;
			viewOffset = new(0, 0);
			Zoom = 1.0f;
			positionMode = PositionIndicatorMode.None;
			previousTileCoordinates = new Point(currentTileCoordinates.X, currentTileCoordinates.Y);
			UnsavedChanges = false;

			// Initialize
			selector = new Selector();
			owMapUndoDepth = _fileManager.Settings.GetUndoDepth();
			targetMaps = Enumerable.Range(0, owMapUndoDepth + 1).Select(i => new byte[MapSizeX * MapSizeY]).ToList();
			mapTexture = new(graphicsDevice, 4096, 4096, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
		}
		protected void CreateBackup()
		{
			int nextTarget = owMapCurrentTarget < owMapUndoDepth ? owMapCurrentTarget + 1 : 0;
			if (owMapBackSteps < owMapUndoDepth)
			{
				owMapBackSteps++;
			}

			owMapForwardSteps = 0;
			targetMaps[owMapCurrentTarget].CopyTo(targetMaps[nextTarget], 0);
			owMapCurrentTarget = nextTarget;
		}
		public virtual void Undo()
		{
			if (owMapBackSteps == 0)
			{
				return;
			}

			int previousTarget = owMapCurrentTarget > 0 ? owMapCurrentTarget - 1 : owMapUndoDepth;
			owMapBackSteps--;
			if (owMapForwardSteps < owMapUndoDepth)
			{
				owMapForwardSteps++;
			}

			owMapCurrentTarget = previousTarget;
			DrawMap();
		}
		public virtual void UpdateTileCoordinates(TaskManager tasks)
		{
			if (currentTileCoordinates != previousTileCoordinates)
			{
				previousTileCoordinates = new(currentTileCoordinates.X, currentTileCoordinates.Y);

				tasks.Add(new EditorTask(EditorTasks.InfoBoxUpdateCoordinates, currentTileCoordinates.Y * 256 + currentTileCoordinates.X));
			}
		}
		public virtual void Copy(CurrentTool tool)
		{

			if (!taskManager.Pop(EditorTasks.Copy) || (tool.Tool != ToolAction.Selector))
			{
				return;
			}
			
			var selection = selector.GetRectangle();
			pasteBin = new byte[selection.Height, selection.Width];
			for (int y = 0; y < selection.Height; y++)
			{
				for (int x = 0; x < selection.Width; x++)
				{
					pasteBin[y, x] = (byte)(mapCanvas[x + selection.Left + ((y + selection.Top) * MapSizeX)] & 0x7F);
				}
			}
		}
		public virtual void Paste(CurrentTool tool)
		{
			if (tool.Tool != ToolAction.Selector)
			{
				return;
			}

			var selection = selector.GetRectangle();

			pasteBin = new byte[selection.Width, selection.Height];

			for (int y = 0; y < selection.Height; y++)
			{
				for (int x = 0; x < selection.Width; x++)
				{
					pasteBin[x, y] = mapCanvas[x + selection.Left + ((y + selection.Top) * selection.Width)];
				}
			}
		}
		public void Paste(Point windowSize, CurrentTool tool)
		{
			if (!mouse.LeftDown && !mouse.LeftClick || tool.Tool != ToolAction.Paster)
			{
				return;
			}

			if (mouse.Position.X < 0 || mouse.Position.Y < 0 || mouse.Position.X > windowSize.X || mouse.Position.Y > windowSize.Y)
			{
				return;
			}

			int middlex = (int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom);
			int middley = (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom);

			if (middlex >= MapSizeX || middley >= MapSizeY || middlex < 0 || middley < 0)
			{
				return;
			}

			if (currentlyDrawing == false)
			{
				CreateBackup();
				currentlyDrawing = true;
			}

			int sizex = pasteBin.GetLength(1);
			int sizey = pasteBin.GetLength(0);

			int minsizex = sizex / 2;
			int minsizey = sizey / 2;

			graphicsDevice.SetRenderTarget(mapTexture);

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			for (int y = 0; y < sizey; y++)
			{
				for (int x = 0; x < sizex; x++)
				{
					int targetx = middlex + x - minsizex;
					int targety = middley + y - minsizey;
					if (targetx >= 0 && targetx < MapSizeX && targety >= 0 && targety < MapSizeY)
					{
						mapCanvas[targety * MapSizeX + targetx] = AdjustPutTile(pasteBin[y, x]);
						spriteBatch.Draw(TileSet, new Vector2(targetx * 16, targety * 16), GetTileRectangle(pasteBin[y, x]), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
					}
				}
			}

			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);

			taskManager.Add(EditorTasks.RestoreTool);
			UnsavedChanges = true;
		}
		public virtual void Redo()
		{
			if (owMapForwardSteps == 0)
			{
				return;
			}
			owMapForwardSteps--;
			if (owMapBackSteps < owMapUndoDepth)
			{
				owMapBackSteps++;
			}

			int nextTarget = owMapCurrentTarget < owMapUndoDepth ? owMapCurrentTarget + 1 : 0;
			owMapCurrentTarget = nextTarget;
			DrawMap();
		}

		public virtual void LoadData() { }

		public virtual void ProcessTasks()
		{
			EditorTask task;

			if (taskManager.Pop(EditorTasks.OverworldLoadMap, out task))
			{
				LoadData();
				taskManager.Add(EditorTasks.ReloadPicker);
			}

			if (taskManager	.Pop(EditorTasks.OverworldBlueMap, out task))
			{
				BlueMap();
			}

			if (taskManager.Pop(EditorTasks.PaintingUndo, out task))
			{
				Undo();
			}

			if (taskManager.Pop(EditorTasks.PaintingRedo, out task))
			{
				Redo();
			}

			if (taskManager.Pop(EditorTasks.UpdateGridsize, out task))
			{
				GridSize *= 2;
				if (GridSize >= MapSizeX || GridSize >= MapSizeY)
				{
					GridSize = 8;
				}
			}

			if (taskManager.Pop(EditorTasks.TogglePositionIndicator, out task))
			{
				positionMode++;

				if (positionMode > PositionIndicatorMode.Corner)
				{
					positionMode = PositionIndicatorMode.None;
				}
			}
		}

		public byte[] GetOwBytes()
		{
			return mapCanvas;
		}
		public Vector2 GetViewOffset()
		{
			return viewOffset;
		}
		public void UpdateView(Vector2 offset, Point windowSize)
		{
			Vector2 maparea = new Vector2(MapSizeX * 16 * Zoom - windowSize.X, MapSizeY * 16 * Zoom - windowSize.Y);
			Vector2 max = new Vector2(-Math.Max(maparea.X + (MapSizeX / 2), 0), -Math.Max(maparea.Y + (MapSizeY / 2), 0));

			viewOffset = new(Math.Max(max.X, Math.Min(Math.Max((MapSizeX / 2), -(maparea.X)), viewOffset.X - (offset.X))), Math.Max(max.Y, Math.Min(Math.Max((MapSizeY / 2), -(maparea.Y)), viewOffset.Y - (offset.Y))));
		}
		public void JumpToLocationView(Vector2 offset, Point windowSize)
		{
			int targetx = ((int)(offset.X - viewOffset.X) / (int)(16 * Zoom));
			int targety = ((int)(offset.Y - viewOffset.Y) / (int)(16 * Zoom));

			viewOffset = new(-(targetx * (int)(16 * Zoom) - windowSize.X / 2), -(targety * (int)(16 * Zoom) - windowSize.Y / 2));
		}
		public void UpdateZoom(Point windowSize)
		{
			if (mouse.ScrollUp && Zoom < 4.0f)
			{
				Zoom = Math.Min(4.0f, Zoom * 2);
				viewOffset = new(viewOffset.X * 2 - mouse.Position.X, viewOffset.Y * 2 - mouse.Position.Y);
			}
			else if (mouse.ScrollDown && Zoom > 0.25f)
			{
				Zoom = Math.Max(0.25f, Zoom / 2);
				viewOffset = new(viewOffset.X * 0.5f + (windowSize.X * 0.25f), viewOffset.Y * 0.5f + (windowSize.Y * 0.25f));
				UpdateView(new Vector2(0, 0), windowSize);

			}
		}
		public virtual void DrawMap() { }
		public virtual void BlueMap() {	}
		public virtual JsonMap ExportJsonMap()
		{
			return new JsonMap();
		}
		public virtual OwMapExchangeData ExportMapExchange()
		{
			return new OwMapExchangeData();
		}
		public virtual void Selector(Point windowSize, CurrentTool tool)
		{
			if (tool.Tool != ToolAction.Selector)
			{
				selector.Enable = false;
				return;
			}

			selector.Enable = true;
			/*
			if ((!mouse.LeftDown && !mouse.LeftClick))
			{
				return;
			}*/

			int targetx = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int targety = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

			if (currentlySelecting)
			{
				targetx = Math.Max(Math.Min(targetx, MapSizeX), 0);
				targety = Math.Max(Math.Min(targety, MapSizeY), 0);

				selector.FinalPosition = new Vector2(targetx, targety);
			}

			if (!mouse.LeftDown)
			{
				currentlySelecting = false;
			}

			if (mouse.Position.X < 0 || mouse.Position.Y < 0 || mouse.Position.X > windowSize.X || mouse.Position.Y > windowSize.Y)
			{
				return;
			}

			if (mouse.LeftClick && !currentlySelecting)
			{
				currentlySelecting = true;

				targetx = Math.Max(Math.Min(targetx, MapSizeX), 0);
				targety = Math.Max(Math.Min(targety, MapSizeY), 0);

				selector.InitialPosition = new Vector2(targetx, targety);
				selector.FinalPosition = new Vector2(targetx, targety);
			}
		}

		public virtual void UpdateTile(Point windowSize, CurrentTool tool)
		{
			if ((!mouse.LeftDown && !mouse.LeftClick) || ((tool.Tool != ToolAction.Pencil && tool.Tool != ToolAction.Brush)))
			{
				if (!mouse.LeftDown && currentlyDrawing && (tool.Tool == ToolAction.Pencil || tool.Tool == ToolAction.Brush))
				{
					currentlyDrawing = false;
				}
				
				return;
			}

			if (mouse.Position.X < 0 || mouse.Position.Y < 0 || mouse.Position.X > windowSize.X || mouse.Position.Y > windowSize.Y)
			{
				return;
			}

			int middlex = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int middley = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

			if (middlex >= MapSizeX || middley >= MapSizeY || middlex < 0 || middley < 0)
			{
				return;
			}

			if (currentlyDrawing == false)
			{
				CreateBackup();
				currentlyDrawing = true;
			}

			int minsize = 0;
			int maxsize = 0;

			int brushsize = (tool.Tool == ToolAction.Brush) ? tool.BrushSize : 1;

			if (tool.Tool == ToolAction.Brush)
			{
				minsize = -(tool.BrushSize / 2);
				maxsize = (tool.BrushSize / 2);
			}

			graphicsDevice.SetRenderTarget(mapTexture);

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			for (int y = minsize; y <= maxsize; y++)
			{
				for (int x = minsize; x <= maxsize; x++)
				{
					int targetx = middlex + x;
					int targety = middley + y;
					if (targetx >= 0 && targetx < MapSizeX && targety >= 0 && targety < MapSizeY)
					{
						mapCanvas[targety * MapSizeX + targetx] = AdjustPutTile(tool.Tile);
						spriteBatch.Draw(TileSet, new Vector2(targetx * 16, targety * 16), GetTileRectangle(tool.Tile), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
					}
				}
			}

			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);

			UnsavedChanges = true;
		}
		protected virtual byte AdjustPutTile(byte tile)
		{
			return tile;
		}
		protected virtual byte AdjustGetTile(byte tile)
		{
			return tile;
		}
		public List<EditorTask> GetTile()
		{
			int targetx = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int targety = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

			if (targetx >= MapSizeX || targety >= MapSizeY || targetx < 0 || targety < 0)
			{
				return new();
			}

			int tilevalue = AdjustGetTile(mapCanvas[targety * MapSizeX + targetx]);
			
			List<EditorTask> mouseTasks = new();

			if (mouse.RightClick)
			{
				mouseTasks.Add(new EditorTask(EditorTasks.TilesPickerUpdate, tilevalue));
				mouseTasks.Add(new EditorTask(EditorTasks.TilesUpdate, tilevalue));
			}

			if (currentTileCoordinates != previousTileCoordinates)
			{
				previousTileCoordinates = new(currentTileCoordinates.X, currentTileCoordinates.Y);

				mouseTasks.Add(new EditorTask(EditorTasks.InfoBoxUpdateCoordinates, currentTileCoordinates.Y * 256 + currentTileCoordinates.X));
			}

			return mouseTasks;
		}
		
		protected Rectangle GetTileRectangle(byte tile)
		{
			int tilex = tile % 0x10;
			int tiley = tile / 0x10;

			return new Rectangle(tilex * 16, tiley * 16, 16, 16);
		}
		public void Draw(WindowsManager manager, CurrentTool tool, Point windowSize)
		{
			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			DrawMapLayer(manager, tool, windowSize);
			DrawOverlayLayer(manager, tool, windowSize);
			DrawCursorLayer(manager, tool, windowSize);

			spriteBatch.End();
		}
		public virtual void DrawMapLayer(WindowsManager manager, CurrentTool tool, Point windowSize)
		{
			spriteBatch.Draw(mapTexture, viewOffset, new Rectangle(0, 0, mapTexture.Width, mapTexture.Height), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);

			if (manager.ShowGridlines || manager.ShowDomainOverlay)
			{
				DrawGrid(manager.ShowDomainOverlay);
			}
		}
		public virtual void DrawOverlayLayer(WindowsManager manager, CurrentTool tool, Point windowSize)
		{
			DrawSelection(selector.ShowSelection);
		}
		public virtual void DrawCursorLayer(WindowsManager manager, CurrentTool tool, Point windowSize)
		{
			DrawBrush(tool);
			DrawPasteBin(tool);
		}
		public void DrawSelection(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			Rectangle maparea = selector.GetRectangle();
			Rectangle drawarea = new Rectangle(0, 0, (int)(maparea.Width * 16), (int)(maparea.Height * 16));

			if (maparea.Height <= 1 && maparea.Width <= 1 && !currentlySelecting)
			{
				return;
			}

			// Draw Background
			Texture2D background = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			background.SetData(new[] { Color.DarkSlateGray });
			
			spriteBatch.Draw(background, new Vector2(viewOffset.X + (maparea.Left * 16) * Zoom, viewOffset.Y + (maparea.Top * 16) * Zoom), drawarea, new Color(255, 255, 255, 205), 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);
		}
		public void DrawGrid(bool showdomains)
		{
			int gridsize = showdomains ? 32 : GridSize;

			Texture2D line = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			line.SetData(new[] { Color.Red });

			int liney = MapSizeY / gridsize;
			int linex = MapSizeX / gridsize;

			for (int y = 0; y < liney; y++)
			{
				for (int x = 0; x < linex; x++)
				{
					spriteBatch.Draw(line, new Vector2(viewOffset.X + (x * gridsize * 16) * Zoom, viewOffset.Y + (y * gridsize * 16) * Zoom), new Rectangle(0, 0, (int)(gridsize * 16 * Zoom), 2), Color.White);
					spriteBatch.Draw(line, new Vector2(viewOffset.X + (x * gridsize * 16) * Zoom, viewOffset.Y + (y * gridsize * 16) * Zoom), new Rectangle(0, 0, 2, (int)(gridsize * 16 * Zoom)), Color.White);
				}
			}

			spriteBatch.Draw(line, new Vector2(viewOffset.X + ((linex * gridsize * 16) * Zoom - 2), viewOffset.Y), new Rectangle(0, 0, 2, (int)(MapSizeY * 16 * Zoom)), Color.White);
			spriteBatch.Draw(line, new Vector2(viewOffset.X, viewOffset.Y + ((liney * gridsize * 16) * Zoom - 2)), new Rectangle(0, 0, (int)(MapSizeX * 16 * Zoom), 2), Color.White);
		}
		public void DrawBrush(CurrentTool tool)
		{
			if (tool.Tool != ToolAction.Brush && tool.Tool != ToolAction.Pencil)
			{
				return;
			}
			
			int minsize = 0;
			int maxsize = 0;

			if (tool.Tool == ToolAction.Brush)
			{
				minsize = -(tool.BrushSize / 2);
				maxsize = (tool.BrushSize / 2);
			}

			int middlex = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int middley = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

			for (int y = minsize; y <= maxsize; y++)
			{
				for (int x = minsize; x <= maxsize; x++)
				{
					spriteBatch.Draw(TileSet, new Vector2(viewOffset.X + (middlex + x) * 16 * Zoom, viewOffset.Y + (middley + y) * 16 * Zoom), GetTileRectangle(tool.Tile), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);
				}
			}
		}
		public void DrawPasteBin(CurrentTool tool)
		{
			if (tool.Tool != ToolAction.Paster)
			{
				return;
			}

			int sizex = pasteBin.GetLength(1);
			int sizey = pasteBin.GetLength(0);

			int minsizex = sizex / 2;
			int minsizey = sizey / 2;

			int middlex = (int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom);
			int middley = (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom);

			for (int y = 0; y < sizey; y++)
			{
				for (int x = 0; x < sizex; x++)
				{
					spriteBatch.Draw(TileSet, new Vector2(viewOffset.X + (middlex + x - minsizex) * 16 * Zoom, viewOffset.Y + (middley + y - minsizey) * 16 * Zoom), GetTileRectangle(pasteBin[y, x]), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);
				}
			}
		}
	}
}