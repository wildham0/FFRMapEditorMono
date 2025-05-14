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
		protected GraphicsDevice graphicsDevice;
		protected SpriteBatch spriteBatch;
		protected MouseState mouse;
		protected int owMapCurrentTarget;
		protected int owMapBackSteps;
		protected int owMapForwardSteps;
		protected int owMapUndoDepth;
		protected bool currentlyDrawing;
		protected bool currentlySelecting;
		protected List<byte[]> targetMaps;
		//private JsonMap jsonMapData;
		//private OwMapExchangeData exchMapData;
		//private object mapData;
		protected FileManager fileManager;
		protected byte[,] pasteBin;
		protected Selector selector;

		protected Point currentTileCoordinates => new Point((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom), (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));
		protected Point previousTileCoordinates;

		//private List<List<(int id, OwEncounterGroup group)>> targetDomains;

		//public bool UpdatePlacedMapObjects { get; set; }
		//public bool UpdatePlacedDocks { get; set; }
		//public bool UpdatePlacedRequiredTiles { get; set; }
		public bool UnsavedChanges { get; set; }
		public Canvas(Dictionary<string, Texture2D> _textures, SpriteFont _font, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, FileManager _fileManager, MouseState _mouse)
		{
			BaseInitialization(_textures, _font, _graphicsDevice, _spriteBatch, _fileManager, _mouse);
		}
		public void BaseInitialization(Dictionary<string, Texture2D> _textures, SpriteFont _font, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, FileManager _fileManager, MouseState _mouse)
		{
			//Texture2D _tileset, , Texture2D _domaingroups, Texture2D _docks, Texture2D _mapobjects,
			fileManager = _fileManager;
			graphicsDevice = _graphicsDevice;
			spriteBatch = _spriteBatch;
			//tileSet = _textures["tileset"];
			owMapCurrentTarget = 0;
			owMapBackSteps = 0;
			owMapForwardSteps = 0;
			mouse = _mouse;
			GridSize = 32;
			MapSizeX = 256;
			MapSizeY = 256;
			currentlyDrawing = false;
			currentlySelecting = false;
			selector = new Selector();
			//domainGroupIcons = _domaingroups;
			//docksIcons = _docks;
			//mapObjectsIcons = _mapobjects;
			owMapUndoDepth = _fileManager.Settings.GetUndoDepth();
			targetMaps = Enumerable.Range(0, owMapUndoDepth + 1).Select(i => new byte[MapSizeX * MapSizeY]).ToList();
			//docks = new();
			//mapObjects = new();
			mapTexture = new(graphicsDevice, 4096, 4096, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
			//domains = Enumerable.Range(0, 64).Select(x => (x, OwEncounterGroup.OopsAllImpsGroup)).ToList();
			//DefineSmarthBrushes();
			//BlueMap();

			viewOffset = new(0, 0);
			font = _font;
			Zoom = 1.0f;
			positionMode = PositionIndicatorMode.None;

			//currentTileCoordinates = new Point(0, 0);
			previousTileCoordinates = new Point(currentTileCoordinates.X, currentTileCoordinates.Y);

			UnsavedChanges = false;
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
		public virtual void Copy(CurrentTool tool, TaskManager taskManager)
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
		public void Paste(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MouseState mouse, Point windowSize, CurrentTool tool, TaskManager tasks)
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

			tasks.Add(EditorTasks.RestoreTool);
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

		public virtual void ProcessTasks(TaskManager tasks)
		{
			EditorTask task;

			if (tasks.Pop(EditorTasks.OverworldLoadMap, out task))
			{
				LoadData();
				tasks.Add(EditorTasks.ReloadPicker);
			}

			if (tasks.Pop(EditorTasks.OverworldBlueMap, out task))
			{
				BlueMap();
			}

			if (tasks.Pop(EditorTasks.PaintingUndo, out task))
			{
				Undo();
			}

			if (tasks.Pop(EditorTasks.PaintingRedo, out task))
			{
				Redo();
			}

			if (tasks.Pop(EditorTasks.UpdateGridsize, out task))
			{
				GridSize *= 2;
				if (GridSize >= MapSizeX || GridSize >= MapSizeY)
				{
					GridSize = 8;
				}
			}

			if (tasks.Pop(EditorTasks.TogglePositionIndicator, out task))
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
		public void UpdateZoom(MouseState mouse, Point windowSize)
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
		public virtual void Selector(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MouseState mouse, Point windowSize, CurrentTool tool)
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

		public virtual void UpdateTile(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MouseState mouse, Point windowSize, CurrentTool tool)
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
		public List<EditorTask> GetTile(MouseState mouse)
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
		public void Draw(SpriteBatch spriteBatch, WindowsManager manager, CurrentTool tool, MouseState mouse, Point windowSize)
		{
			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			DrawMapLayer(spriteBatch, manager, tool, mouse, windowSize);
			DrawOverlayLayer(spriteBatch, manager, tool, mouse, windowSize);
			DrawCursorLayer(spriteBatch, manager, tool, mouse, windowSize);

			spriteBatch.End();
		}
		public virtual void DrawMapLayer(SpriteBatch spriteBatch, WindowsManager manager, CurrentTool tool, MouseState mouse, Point windowSize)
		{
			spriteBatch.Draw(mapTexture, viewOffset, new Rectangle(0, 0, mapTexture.Width, mapTexture.Height), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);

			if (manager.ShowGridlines || manager.ShowDomainOverlay)
			{
				DrawGrid(spriteBatch, manager.ShowDomainOverlay);
			}
		}
		public virtual void DrawOverlayLayer(SpriteBatch spriteBatch, WindowsManager manager, CurrentTool tool, MouseState mouse, Point windowSize)
		{
			DrawSelection(spriteBatch, selector.ShowSelection);
		}
		public virtual void DrawCursorLayer(SpriteBatch spriteBatch, WindowsManager manager, CurrentTool tool, MouseState mouse, Point windowSize)
		{
			DrawBrush(spriteBatch, tool, mouse);
			DrawPastBin(spriteBatch, tool, mouse);
			//DrawCoordinate(spriteBatch, windowSize, mouse);
		}
		public void DrawSelection(SpriteBatch spriteBatch, bool enabled)
		{
			if (!enabled)
			{
				return;
			}
			Rectangle maparea = selector.GetRectangle();
			Rectangle drawarea = new Rectangle(0, 0, (int)(maparea.Width * 16), (int)(maparea.Height * 16));

			//spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			// Draw Background
			Texture2D background = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			background.SetData(new[] { Color.DarkSlateGray });
			//new Vector2(viewOffset.X + (x * gridsize * 16) * Zoom, viewOffset.Y + (y * gridsize * 16) * Zoom)
			
			spriteBatch.Draw(background, new Vector2(viewOffset.X + (maparea.Left * 16) * Zoom, viewOffset.Y + (maparea.Top * 16) * Zoom), drawarea, new Color(255, 255, 255, 205), 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);

			//spriteBatch.Draw(background, new Vector2(maparea.Left * 16, maparea.Top * 16), drawarea, new Color(255, 255, 255, 205), 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);

			//spriteBatch.End();
		}
		public void DrawGrid(SpriteBatch spriteBatch, bool showdomains)
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

		public void DrawCoordinate(SpriteBatch spriteBatch, Point windowSize, MouseState mouse)
		{
			int targetx = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int targety = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

			if (targetx >= MapSizeX || targety >= MapSizeY || targetx < 0 || targety < 0 || (positionMode == PositionIndicatorMode.None))
			{
				return;
			}

			string coordstring = "(" + targetx + ", " + targety + ")";

			float positionx;
			float positiony;

			if (positionMode == PositionIndicatorMode.Cursor)
			{
				positionx = mouse.Position.X + 32;
				positiony = mouse.Position.Y;
			}
			else
			{
				positionx = 16;
				positiony = windowSize.Y - 32;
			}

			spriteBatch.DrawString(font, coordstring, new Vector2(positionx - 1, positiony), Color.Black);
			spriteBatch.DrawString(font, coordstring, new Vector2(positionx + 1, positiony), Color.Black);
			spriteBatch.DrawString(font, coordstring, new Vector2(positionx, positiony - 1), Color.Black);
			spriteBatch.DrawString(font, coordstring, new Vector2(positionx, positiony + 1), Color.Black);
			spriteBatch.DrawString(font, coordstring, new Vector2(positionx, positiony), Color.White);
		}
		public void DrawBrush(SpriteBatch spriteBatch, CurrentTool tool, MouseState mouse)
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
		public void DrawPastBin(SpriteBatch spriteBatch, CurrentTool tool, MouseState mouse)
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