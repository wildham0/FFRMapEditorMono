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
using System.Windows.Markup;

namespace FFRMapEditorMono.FFR
{
	public enum MapObject
	{ 
		StartingPosition = 0,
		Bridge,
		Canal,
		Ship,
		Airship
	}
	public enum TileGroup
	{
		Land = 0,
		Forest,
		Mountain,
		Sea,
		Desert,
		River,
		Grass,
		Marsh,
		Other,
		SeaRiver,
		MountainCave,
		SpecialDesert
	}

	public enum TileType
	{ 
		Center = 0,
		NorthWest,
		North,
		NorthEast,
		West,
		East,
		SouthWest,
		South,
		SouthEast,
	}

	public class CanvasFFR : Canvas
	{
		private Texture2D domainGroupIcons;
		private Texture2D docksIcons;
		private Texture2D mapObjectsIcons;
		private List<(int id, OwEncounterGroup group)> domains;
		private List<(OverworldTeleportIndex location, SCCoords coord)> docks;
		private List<(MapObject mapobject, SCCoords coord)> mapObjects;
		private List<SmartBrush> smartBrushes;
		public bool UpdatePlacedMapObjects { get; set; }
		public bool UpdatePlacedDocks { get; set; }
		public bool UpdatePlacedRequiredTiles { get; set; }
		public List<MapObject> MissingMapObjects => requiredMapObjects.Except(mapObjects.Select(o => o.mapobject)).ToList();
		public List<string> MissingRequiredTiles => TileInfo.Required.Select(t => (int)t).Except(GetOwBytes().ToList().Intersect(TileInfo.Required).Select(t => (int)t).ToList()).ToList().Select(t => TileInfo.Names[t]).ToList();
		public bool DefaultDockPlaced => docks.Where(d => d.location == OverworldTeleportIndex.None).Any();

		private List<MapObject> requiredMapObjects = new() { MapObject.StartingPosition, MapObject.Canal, MapObject.Bridge, MapObject.Airship };
		public CanvasFFR(Dictionary<string, Texture2D> _textures, SpriteFont _font, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, FileManager _fileManager, TaskManager _taskmanager, MouseState _mouse, KeyboardState _keyboard) : base(_textures, _font, _graphicsDevice, _spriteBatch, _fileManager, _taskmanager, _mouse, _keyboard)
		{
			TileSet = _textures["tileset"];

			domainGroupIcons = _textures["domainsicons"];
			docksIcons = _textures["docksicons"];
			mapObjectsIcons = _textures["mapobjects"];

			docks = new();
			mapObjects = new();
			domains = Enumerable.Range(0, 64).Select(x => (x, OwEncounterGroup.OopsAllImpsGroup)).ToList();
			
			DefineSmarthBrushes();
			BlueMap();

			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
		}
		public override void Undo()
		{
			base.Undo();
			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
		}
		public override void Redo()
		{
			base.Redo();
			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
		}
		private void DefineSmarthBrushes()
		{
			smartBrushes = new()
			{
				new SmartBrush(TileGroup.Land, new() { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),
				new SmartBrush(TileGroup.Sea, new() { 0x17, 0x06, 0x07, 0x08, 0x16, 0x18, 0x26, 0x27, 0x28 }),
				new SmartBrush(TileGroup.Forest, new() { 0x14, 0x03, 0x04, 0x05, 0x13, 0x15, 0x23, 0x24, 0x25 }),
				new SmartBrush(TileGroup.Mountain, new() { 0x21, 0x10, 0x11, 0x12, 0x20, 0x22, 0x30, 0x31, 0x33 }),
				new SmartBrush(TileGroup.River, new() { 0x44, 0x40, 0x44, 0x41, 0x44, 0x44, 0x50, 0x44, 0x51 }),
				new SmartBrush(TileGroup.Desert, new() { 0x45, 0x42, 0x45, 0x43, 0x45, 0x45, 0x52, 0x45, 0x53 }),
				new SmartBrush(TileGroup.Grass, new() { 0x54, 0x60, 0x54, 0x61, 0x60, 0x61, 0x70, 0x54, 0x71 }),
				new SmartBrush(TileGroup.Marsh, new() { 0x55, 0x62, 0x55, 0x63, 0x55, 0x55, 0x72, 0x55, 0x73 }),
			};
		}
		public override void LoadData()
		{
			var mapdata = fileManager.MapDataFF;

			mapdata.DecodeMap().CopyTo(mapCanvas, 0);
			domains = mapdata.DomainUpdates.Select(d => ((int)d.To, OwDataGroup.OwDomainsGroup[d.From])).ToList();
			for (int i = 0; i < 64; i++)
			{
				var index = domains.FindIndex(d => d.id == i);
				if (index < 0)
				{
					domains.Add((i, OwDataGroup.OwDomainsGroup[i]));
				}
			}
			docks = mapdata.ShipLocations.Select(l => ((OverworldTeleportIndex)l.TeleporterIndex, new SCCoords(l.X, l.Y))).ToList();
			mapObjects = new()
			{
				(MapObject.StartingPosition, (SCCoords)mapdata.StartingLocation),
				(MapObject.Bridge, (SCCoords)mapdata.BridgeLocation),
				(MapObject.Canal, (SCCoords)mapdata.CanalLocation),
				(MapObject.Airship, (SCCoords)mapdata.AirShipLocation)
			};
			mapTexture = new(graphicsDevice, 4096, 4096, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
			DrawMap();
			viewOffset = new(0, 0);
			Zoom = 1.0f;

			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
		}
		public override void ProcessTasks()
		{
			base.ProcessTasks();

			EditorTask task;

			if (taskManager.Pop(EditorTasks.DocksRemove, out task))
			{
				int dockToRemove = task.Value == (int)OverworldTeleportIndex.DefaultLocation ? (int)OverworldTeleportIndex.None : task.Value;

				docks.RemoveAll(d => d.location == (OverworldTeleportIndex)dockToRemove);
				UpdatePlacedDocks = true;
			}

			if (taskManager.Pop(EditorTasks.MapObjectsRemove, out task))
			{
				mapObjects.RemoveAll(o => o.mapobject == (MapObject)task.Value);
				UpdatePlacedMapObjects = true;
			}

			if (UpdatePlacedMapObjects)
			{
				taskManager.Add(new EditorTask(EditorTasks.UpdatePlacedObjectsOverlay));
				UpdatePlacedMapObjects = false;
			}

			if (UpdatePlacedDocks)
			{
				taskManager.Add(new EditorTask(EditorTasks.UpdatePlacedDocksOverlay));
				UpdatePlacedDocks = false;
			}

			if (UpdatePlacedRequiredTiles)
			{
				taskManager.Add(new EditorTask(EditorTasks.UpdatePlacedTilesOverlay));
				UpdatePlacedRequiredTiles = false;
			}
		}
		public List<DomainFixup> GetDomainsData()
		{
			return domains.Select(d => new DomainFixup() { To = (byte)d.id, From = (byte)d.group }).ToList();
		}
		public List<ShipLocation> GetShipData()
		{
			return docks.Select(d => new ShipLocation(d.coord.X, d.coord.Y, (byte)d.location)).ToList();
		}
		public SCCoords GetMapObjectPosition(MapObject mapobject)
		{
			var index = mapObjects.FindIndex(o => o.mapobject == mapobject);
			if (index >= 0)
			{
				return mapObjects[index].coord;
			}
			else
			{
				return new SCCoords(0, 0);
			}
		}
		public List<MapObject> GetPlacedMapObjects()
		{
			return mapObjects.Select(o => o.mapobject).ToList();
		}
		public (bool defaultdock, List<MapObject> missingmapobjects) ValidateObjects()
		{
			List<MapObject> requiredMapObjects = new() { MapObject.StartingPosition, MapObject.Canal, MapObject.Bridge, MapObject.Airship };
			return (docks.Where(d => d.location == OverworldTeleportIndex.None).Any(), requiredMapObjects.Except(mapObjects.Select(o => o.mapobject)).ToList());
		}
		public override void DrawMap()
		{
			graphicsDevice.SetRenderTarget(mapTexture);
			graphicsDevice.Clear(Color.AliceBlue);

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			for (int y = 0; y < MapSizeY; y++)
			{
				for (int x = 0; x < MapSizeX; x++)
				{
					int tilevalue = mapCanvas[MapSizeX * y + x];
					int xoffset = tilevalue % 0x10;
					int yoffset = tilevalue / 0x10;

					spriteBatch.Draw(TileSet, new Vector2(x * 16, y * 16), new Rectangle(xoffset * 16, yoffset * 16, 16, 16), Color.White);
				}
			}

			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);
		}
		public override void BlueMap()
		{
			graphicsDevice.SetRenderTarget(mapTexture);
			graphicsDevice.Clear(Color.AliceBlue);

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			for (int y = 0; y < MapSizeY; y++)
			{
				for (int x = 0; x < MapSizeX; x++)
				{
					mapCanvas[MapSizeX * y + x] = 0x17;
					int tilevalue = 0x17;
					int xoffset = tilevalue % 0x10;
					int yoffset = tilevalue / 0x10;

					spriteBatch.Draw(TileSet, new Vector2(x * 16, y * 16), new Rectangle(xoffset * 16, yoffset * 16, 16, 16), Color.White);
				}
			}

			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);

			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
		}
		public override void UpdateTile(Point windowSize, CurrentTool tool)
		{
			base.UpdateTile(windowSize, tool);

			if (tool.Tool == ToolAction.Brush)
			{
				int middlex = (int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom);
				int middley = (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom);
				int brushsize = tool.Tool == ToolAction.Brush ? tool.BrushSize : 1;

				graphicsDevice.SetRenderTarget(mapTexture);
				spriteBatch.Begin(samplerState: SamplerState.PointClamp);

				ProcessSmartBrush(new Point(middlex, middley), brushsize);

				spriteBatch.End();
				graphicsDevice.SetRenderTarget(null);
			}

			UpdatePlacedRequiredTiles = true;
		}
		public void PlaceTemplate(Point windowSize, CurrentTool tool)
		{
			if (!mouse.LeftDown && !mouse.LeftClick || tool.Tool != ToolAction.Templates)
			{
				if (!mouse.LeftDown && currentlyDrawing && tool.Tool == ToolAction.Templates)
				{
					currentlyDrawing = false;
				}

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

			int sizex = tool.Template.GetLength(1);
			int sizey = tool.Template.GetLength(0);

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
						mapCanvas[targety * MapSizeX + targetx] = tool.Template[y, x];
						spriteBatch.Draw(TileSet, new Vector2(targetx * 16, targety * 16), GetTileRectangle(tool.Template[y, x]), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
					}
				}
			}

			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);

			UpdatePlacedRequiredTiles = true;
			UnsavedChanges = true;
		}
		private void SmartBrushAdjustRow(int y, int centerx, int size)
		{
			int minsize = -(size / 2);
			int maxsize = size / 2;

			if (y < 1 || y > MapSizeY - 2)
			{
				return;
			}

			for (int x = Math.Max(1, centerx + minsize); x < Math.Min(centerx + maxsize + 1, MapSizeX - 1); x++)
			{
				List<List<byte>> cluster = new()
				{
					mapCanvas[((y - 1) * MapSizeX + x - 1)..((y - 1) * MapSizeX + x + 2)].ToList(),
					mapCanvas[((y + 0) * MapSizeX + x - 1)..((y + 0) * MapSizeX + x + 2)].ToList(),
					mapCanvas[((y + 1) * MapSizeX + x - 1)..((y + 1) * MapSizeX + x + 2)].ToList(),
				};
				TileGroup tileType = OwDataGroup.TileByteToGroup.TryGetValue(mapCanvas[y * MapSizeX + x], out var currentile) ? currentile : TileGroup.Other;

				if (tileType == TileGroup.Other || tileType == TileGroup.MountainCave || tileType == TileGroup.SeaRiver || tileType == TileGroup.SpecialDesert)
				{
					return;
				}

				SmartBrush currentBrush = smartBrushes.Find(s => s.Group == tileType);

				byte newtile = currentBrush.GetTile(cluster);
				mapCanvas[y * MapSizeX + x] = newtile;
				spriteBatch.Draw(TileSet, new Vector2(x * 16, y * 16), GetTileRectangle(newtile), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
			}
		}
		private void SmartBrushAdjustColumn(int centery, int x, int size)
		{
			int minsize = -(size / 2);
			int maxsize = size / 2;

			if (x < 1 || x > MapSizeX - 2)
			{
				return;
			}

			for (int y = Math.Max(1, centery + minsize); y < Math.Min(centery + maxsize + 1, MapSizeY - 1); y++)
			{
				List<List<byte>> cluster = new()
				{
					mapCanvas[((y - 1) * MapSizeX + x - 1)..((y - 1) * MapSizeX + x + 2)].ToList(),
					mapCanvas[((y + 0) * MapSizeX + x - 1)..((y + 0) * MapSizeX + x + 2)].ToList(),
					mapCanvas[((y + 1) * MapSizeX + x - 1)..((y + 1) * MapSizeX + x + 2)].ToList(),
				};

				TileGroup tileType = OwDataGroup.TileByteToGroup.TryGetValue(mapCanvas[y * MapSizeX + x], out var currentile) ? currentile : TileGroup.Other;

				if (tileType == TileGroup.Other || tileType == TileGroup.MountainCave || tileType == TileGroup.SeaRiver || tileType == TileGroup.SpecialDesert)
				{
					return;
				}

				SmartBrush currentBrush = smartBrushes.Find(s => s.Group == tileType);

				byte newtile = currentBrush.GetTile(cluster);
				mapCanvas[y * MapSizeX + x] = newtile;
				spriteBatch.Draw(TileSet, new Vector2(x * 16, y * 16), GetTileRectangle(newtile), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
			}
		}
		private void ProcessSmartBrush(Point center, int size)
		{
			int minsize = -((size + 2) / 2);
			int maxsize = (size + 2) / 2;

			SmartBrushAdjustRow(center.Y + minsize, center.X, size + 2);
			SmartBrushAdjustRow(center.Y + minsize + 1, center.X, size);

			SmartBrushAdjustRow(center.Y + maxsize, center.X, size + 2);
			SmartBrushAdjustRow(center.Y + maxsize - 1, center.X, size);

			SmartBrushAdjustColumn(center.Y, center.X + minsize, size + 2);
			SmartBrushAdjustColumn(center.Y, center.X + minsize + 1, size);

			SmartBrushAdjustColumn(center.Y, center.X + maxsize, size + 2);
			SmartBrushAdjustColumn(center.Y, center.X + maxsize - 1, size);
		}
		public void UpdateDomain(CurrentTool tool)
		{
			if (!mouse.LeftDown && !mouse.LeftClick || tool.Tool != ToolAction.Domains)
			{
				return;
			}

			int middlex = (int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom) / 32;
			int middley = (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom) / 32;

			if (middlex >= MapSizeX || middley >= MapSizeY || middlex < 0 || middley < 0)
			{
				return;
			}

			domains[middley * 8 + middlex] = (middley * 8 + middlex, tool.EncounterGroup);

			UnsavedChanges = true;
		}
		public void UpdateDock(CurrentTool tool)
		{
			if (!mouse.LeftDown && !mouse.LeftClick || tool.Tool != ToolAction.Docks)
			{
				return;
			}

			int middlex = (int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom);
			int middley = (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom);

			if (middlex >= MapSizeX || middley >= MapSizeY || middlex < 0 || middley < 0)
			{
				return;
			}

			var dockindex = docks.FindIndex(d => d.location == tool.DockLocation);
			if (dockindex < 0)
			{
				docks.Add((tool.DockLocation, new SCCoords(middlex, middley)));
			}
			else
			{
				docks[dockindex] = (tool.DockLocation, new SCCoords(middlex, middley));
			}

			UpdatePlacedDocks = true;
			UnsavedChanges = true;
		}
		public void UpdateMapObject(CurrentTool tool)
		{
			if (!mouse.LeftDown && !mouse.LeftClick || tool.Tool != ToolAction.MapObjects)
			{
				return;
			}

			if (tool.MapObject == MapObject.Ship)
			{
				return;
			}

			int middlex = (int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom);
			int middley = (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom);

			if (middlex >= MapSizeX || middley >= MapSizeY || middlex < 0 || middley < 0)
			{
				return;
			}

			var objectindex = mapObjects.FindIndex(o => o.mapobject == tool.MapObject);
			if (objectindex < 0)
			{
				mapObjects.Add((tool.MapObject, new SCCoords(middlex, middley)));
			}
			else
			{
				mapObjects[objectindex] = (tool.MapObject, new SCCoords(middlex, middley));
			}

			UpdatePlacedMapObjects = true;
			UnsavedChanges = true;
		}
		public override void DrawOverlayLayer(WindowsManager manager, CurrentTool tool, Point windowSize)
		{
			if (manager.ShowDomainOverlay)
			{
				DrawDomainsOverlay();
			}

			if (manager.ShowDockOverlay)
			{
				DrawDocksOverlay();
			}

			if (manager.ShowMapObjectsOverlay)
			{
				DrawMapObjectsOverlay();
			}

			DrawSelection(selector.ShowSelection);
		}
		public override void DrawCursorLayer(WindowsManager manager, CurrentTool tool, Point windowSize)
		{
			DrawBrush(tool);
			DrawTemplate(tool);
			DrawPasteBin(tool);
		}
		public void DrawDomainsOverlay()
		{
			Texture2D line = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			line.SetData(new[] { Color.Red });

			Dictionary<OwEncounterGroup, int> domainOrdered = Enum.GetValues<OwEncounterGroup>().Select((o, i) => (o, i)).ToDictionary(g => g.o, g => g.i);

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					string coordstring = "(" + x + ", " + y + ")";

					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + x * 32 * 16 * Zoom + 3, viewOffset.Y + y * 32 * 16 * Zoom + 4), Color.Black);
					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + x * 32 * 16 * Zoom + 5, viewOffset.Y + y * 32 * 16 * Zoom + 4), Color.Black);
					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + x * 32 * 16 * Zoom + 4, viewOffset.Y + y * 32 * 16 * Zoom + 3), Color.Black);
					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + x * 32 * 16 * Zoom + 4, viewOffset.Y + y * 32 * 16 * Zoom + 5), Color.Black);
					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + x * 32 * 16 * Zoom + 4, viewOffset.Y + y * 32 * 16 * Zoom + 4), Color.White);

					var domaintype = domainOrdered[domains[x + y * 8].group];
					int domaintypex = domaintype % 8;
					int domaintypey = domaintype / 8;

					spriteBatch.Draw(domainGroupIcons, new Vector2(viewOffset.X + x * 32 * 16 * Zoom + 8, viewOffset.Y + y * 32 * 16 * Zoom + 24), new Rectangle(domaintypex * 32, domaintypey * 32, 32, 32), Color.White);
				}
			}
		}
		public void DrawTemplate(CurrentTool tool)
		{
			if (tool.Tool != ToolAction.Templates)
			{
				return;
			}

			int sizex = tool.Template.GetLength(1);
			int sizey = tool.Template.GetLength(0);

			int minsizex = sizex / 2;
			int minsizey = sizey / 2;

			int middlex = (int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom);
			int middley = (int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom);

			for (int y = 0; y < sizey; y++)
			{
				for (int x = 0; x < sizex; x++)
				{
					spriteBatch.Draw(TileSet, new Vector2(viewOffset.X + (middlex + x -minsizex) * 16 * Zoom, viewOffset.Y + (middley + y -minsizey) * 16 * Zoom), GetTileRectangle(tool.Template[y,x]), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);
				}
			}
		}
		public void DrawDocksOverlay()
		{
			Dictionary<SCCoords, int> previouslyplaced = new();

			foreach (var dock in docks)
			{
				int dockx = (int)dock.location % 8;
				int docky = (int)dock.location / 8;

				if (dock.location == OverworldTeleportIndex.None)
				{
					dockx = 6;
					docky = 3;
				}
				
				int count = 0;

				if (previouslyplaced.TryGetValue(dock.coord, out count))
				{
					previouslyplaced[dock.coord]++;
				}
				else
				{
					previouslyplaced.Add(dock.coord, 1);
				}

				spriteBatch.Draw(docksIcons, new Vector2(viewOffset.X + dock.coord.X * 16 * Zoom + count * 16 * Math.Max(Zoom / 2, 1f), viewOffset.Y + dock.coord.Y * 16 * Zoom), new Rectangle(dockx * 32, docky * 32, 32, 32), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Math.Max(Zoom / 2, 1f), SpriteEffects.None, 0.0f);
			}
		}
		public void DrawMapObjectsOverlay()
		{
			Dictionary<SCCoords, int> previouslyplaced = new();

			foreach (var mapobject in mapObjects)
			{
				int mapobjectx = (int)mapobject.mapobject % 8;
				int mapobjecty = (int)mapobject.mapobject / 8;

				int count = 0;

				if (previouslyplaced.TryGetValue(mapobject.coord, out count))
				{
					previouslyplaced[mapobject.coord]++;
				}
				else
				{
					previouslyplaced.Add(mapobject.coord, 1);
				}

				spriteBatch.Draw(mapObjectsIcons, new Vector2(viewOffset.X + mapobject.coord.X * 16 * Zoom + count * 16 * Math.Max(Zoom, 2f), viewOffset.Y + mapobject.coord.Y * 16 * Zoom), new Rectangle(mapobjectx * 16, mapobjecty * 16, 16, 16), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Math.Max(Zoom, 2f), SpriteEffects.None, 0.0f);
			}
		}
	}
}