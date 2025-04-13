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

namespace FFRMapEditorMono
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

	public enum PositionIndicatorMode
	{ 
		None,
		Cursor,
		Corner,
	}

	public class Overworld
	{
		private Texture2D tileSet;
		private Texture2D domainGroupIcons;
		private Texture2D docksIcons;
		private Texture2D mapObjectsIcons;
		private RenderTarget2D mapTexture;
		private byte[] overworldMap { get => targetMaps[owMapCurrentTarget]; }
		private List<(int id, OwEncounterGroup group)> domains;
		private List<(OverworldTeleportIndex location, SCCoords coord)> docks;
		private List<(MapObject mapobject, SCCoords coord)> mapObjects;
		private Vector2 viewOffset;
		private SpriteFont font;
		public float Zoom { get; set; }
		public int GridSize { get; set; }
		private int MapSizeX;
		private int MapSizeY;
		private PositionIndicatorMode positionMode;
		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;
		private List<SmartBrush> smartBrushes;
		private int owMapCurrentTarget;
		private int owMapBackSteps;
		private int owMapForwardSteps;
		private int owMapUndoDepth;
		private bool currentlyDrawing;
		private List<byte[]> targetMaps;
		private List<List<(int id, OwEncounterGroup group)>> targetDomains;

		public bool UpdatePlacedMapObjects { get; set; }
		public bool UpdatePlacedDocks { get; set; }
		public bool UpdatePlacedRequiredTiles { get; set; }
		public bool UnsavedChanges { get; set; }
		public Overworld(Texture2D _tileset, SpriteFont _font, Texture2D _domaingroups, Texture2D _docks, Texture2D _mapobjects, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, int _undodepth)
		{
			graphicsDevice = _graphicsDevice;
			spriteBatch = _spriteBatch;
			tileSet = _tileset;
			owMapCurrentTarget = 0;
			owMapBackSteps = 0;
			owMapForwardSteps = 0;
			GridSize = 32;
			MapSizeX = 256;
			MapSizeY = 256;
			currentlyDrawing = false;
			domainGroupIcons = _domaingroups;
			docksIcons = _docks;
			mapObjectsIcons = _mapobjects;
			owMapUndoDepth = _undodepth;
			targetMaps = Enumerable.Range(0, owMapUndoDepth + 1).Select(i => new byte[MapSizeX * MapSizeY]).ToList();
			docks = new();
			mapObjects = new();
			mapTexture = new(graphicsDevice, 4096, 4096, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
			domains = Enumerable.Range(0, 64).Select(x => (x, OwEncounterGroup.OopsAllImpsGroup)).ToList();
			DefineSmarthBrushes();
			BlueMap();
			viewOffset = new(0, 0);
			font = _font;
			Zoom = 1.0f;
			positionMode = PositionIndicatorMode.None;

			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
			UnsavedChanges = false;
		}
		private void CreateBackup()
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
		public void Undo()
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
			GenerateInitialMap();
			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
		}
		public void Redo()
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
			GenerateInitialMap();
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
		public void LoadData(OwMapExchangeData mapdata)
		{
			mapdata.DecodeMap().CopyTo(overworldMap, 0);
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
			GenerateInitialMap();
			viewOffset = new(0, 0);
			Zoom = 1.0f;

			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
		}
		public void ProcessTasks(OwMapExchangeData mapdata, List<EditorTask> tasks)
		{
			var validtask = tasks.ToList();

			foreach (var task in validtask)
			{
				if (task.Type == EditorTasks.OverworldLoadMap)
				{
					mapObjects = new();
					LoadData(mapdata);
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.OverworldBlueMap)
				{
					mapObjects = new();
					docks = new();
					BlueMap();
					UpdatePlacedMapObjects = true;
					UpdatePlacedDocks = true;
					UpdatePlacedRequiredTiles = true;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.PaintingUndo)
				{
					Undo();
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.PaintingRedo)
				{
					Redo();
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.DocksRemove)
				{
					int dockToRemove = (task.Value == (int)OverworldTeleportIndex.DefaultLocation) ? (int)OverworldTeleportIndex.None : task.Value;

					docks.RemoveAll(d => d.location == (OverworldTeleportIndex)dockToRemove);
					UpdatePlacedDocks = true;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.MapObjectsRemove)
				{
					mapObjects.RemoveAll(o => o.mapobject == (MapObject)task.Value);
					UpdatePlacedMapObjects = true;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.UpdateGridsize)
				{
					GridSize *= 2;
					if (GridSize >= MapSizeX || GridSize >= MapSizeY)
					{
						GridSize = 8;
					}
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.TogglePositionIndicator)
				{
					positionMode++;

					if (positionMode > PositionIndicatorMode.Corner)
					{
						positionMode = PositionIndicatorMode.None;
					}
					tasks.Remove(task);
				}
			}

			if (UpdatePlacedMapObjects)
			{
				tasks.Add(new EditorTask() { Type = EditorTasks.UpdatePlacedObjectsOverlay });
				UpdatePlacedMapObjects = false;
			}

			if (UpdatePlacedDocks)
			{
				tasks.Add(new EditorTask() { Type = EditorTasks.UpdatePlacedDocksOverlay });
				UpdatePlacedDocks = false;
			}

			if (UpdatePlacedRequiredTiles)
			{
				tasks.Add(new EditorTask() { Type = EditorTasks.UpdatePlacedTilesOverlay });
				UpdatePlacedRequiredTiles = false;
			}

		}

		public byte[] GetOwBytes()
		{
			return overworldMap;
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
		private void GenerateInitialMap()
		{
			graphicsDevice.SetRenderTarget(mapTexture);
			graphicsDevice.Clear(Color.AliceBlue);

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			for (int y = 0; y < MapSizeY; y++)
			{
				for (int x = 0; x < MapSizeX; x++)
				{
					int tilevalue = overworldMap[MapSizeX * y + x];
					int xoffset = tilevalue % 0x10;
					int yoffset = tilevalue / 0x10;

					spriteBatch.Draw(tileSet, new Vector2(x * 16, y * 16), new Rectangle(xoffset * 16, yoffset * 16, 16, 16), Color.White);
				}
			}

			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);
		}
		public void BlueMap()
		{
			graphicsDevice.SetRenderTarget(mapTexture);
			graphicsDevice.Clear(Color.AliceBlue);

			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			for (int y = 0; y < MapSizeY; y++)
			{
				for (int x = 0; x < MapSizeX; x++)
				{
					overworldMap[MapSizeX * y + x] = 0x17;
					int tilevalue = 0x17;
					int xoffset = tilevalue % 0x10;
					int yoffset = tilevalue / 0x10;

					spriteBatch.Draw(tileSet, new Vector2(x * 16, y * 16), new Rectangle(xoffset * 16, yoffset * 16, 16, 16), Color.White);
				}
			}

			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);

			UpdatePlacedMapObjects = true;
			UpdatePlacedDocks = true;
			UpdatePlacedRequiredTiles = true;
		}
		public void UpdateTile(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MouseState mouse, Point windowSize, CurrentTool tool)
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
						overworldMap[targety * MapSizeX + targetx] = tool.Tile;
						spriteBatch.Draw(tileSet, new Vector2(targetx * 16, targety * 16), GetTileRectangle(tool.Tile), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
					}
				}
			}

			if (tool.Tool == ToolAction.Brush)
			{
				ProcessSmartBrush(new Point(middlex, middley), brushsize, spriteBatch);
			}


			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);

			UpdatePlacedRequiredTiles = true;
			UnsavedChanges = true;
		}
		public void PlaceTemplate(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MouseState mouse, Point windowSize, CurrentTool tool)
		{
			if ((!mouse.LeftDown && !mouse.LeftClick) || ((tool.Tool != ToolAction.Templates)))
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
						overworldMap[targety * MapSizeX + targetx] = tool.Template[y, x];
						spriteBatch.Draw(tileSet, new Vector2(targetx * 16, targety * 16), GetTileRectangle(tool.Template[y, x]), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
					}
				}
			}

			spriteBatch.End();

			graphicsDevice.SetRenderTarget(null);

			UpdatePlacedRequiredTiles = true;
			UnsavedChanges = true;
		}
		private void SmartBrushAdjustRow(int y, int centerx, int size, SpriteBatch spriteBatch)
		{
			int minsize = -(size / 2);
			int maxsize = (size / 2);

			if (y < 1 || y > (MapSizeY - 2))
			{
				return;
			}

			for (int x = Math.Max(1, centerx + minsize); x < Math.Min(centerx + maxsize + 1, (MapSizeX - 1)); x++)
			{
				List<List<byte>> cluster = new()
				{
					overworldMap[((y - 1) * MapSizeX + x - 1)..((y - 1) * MapSizeX + x + 2)].ToList(),
					overworldMap[((y + 0) * MapSizeX + x - 1)..((y + 0) * MapSizeX + x + 2)].ToList(),
					overworldMap[((y + 1) * MapSizeX + x - 1)..((y + 1) * MapSizeX + x + 2)].ToList(),
				};
				TileGroup tileType = OwDataGroup.TileByteToGroup.TryGetValue(overworldMap[y * MapSizeX + x], out var currentile) ? currentile : TileGroup.Other;

				if (tileType == TileGroup.Other || tileType == TileGroup.MountainCave || tileType == TileGroup.SeaRiver || tileType == TileGroup.SpecialDesert)
				{
					return;
				}

				SmartBrush currentBrush = smartBrushes.Find(s => s.Group == tileType);

				byte newtile = currentBrush.GetTile(cluster);
				overworldMap[y * MapSizeX + x] = newtile;
				spriteBatch.Draw(tileSet, new Vector2(x * 16, y * 16), GetTileRectangle(newtile), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
			}
		}
		private void SmartBrushAdjustColumn(int centery, int x, int size, SpriteBatch spriteBatch)
		{
			int minsize = -(size / 2);
			int maxsize = (size / 2);

			if (x < 1 || x > (MapSizeX - 2))
			{
				return;
			}

			for (int y = Math.Max(1, centery + minsize); y < Math.Min(centery + maxsize + 1, (MapSizeY - 1)); y++)
			{
				List<List<byte>> cluster = new()
				{
					overworldMap[((y - 1) * MapSizeX + x - 1)..((y - 1) * MapSizeX + x + 2)].ToList(),
					overworldMap[((y + 0) * MapSizeX + x - 1)..((y + 0) * MapSizeX + x + 2)].ToList(),
					overworldMap[((y + 1) * MapSizeX + x - 1)..((y + 1) * MapSizeX + x + 2)].ToList(),
				};

				TileGroup tileType = OwDataGroup.TileByteToGroup.TryGetValue(overworldMap[y * MapSizeX + x], out var currentile) ? currentile : TileGroup.Other;

				if (tileType == TileGroup.Other || tileType == TileGroup.MountainCave || tileType == TileGroup.SeaRiver || tileType == TileGroup.SpecialDesert)
				{
					return;
				}

				SmartBrush currentBrush = smartBrushes.Find(s => s.Group == tileType);

				byte newtile = currentBrush.GetTile(cluster);
				overworldMap[y * MapSizeX + x] = newtile;
				spriteBatch.Draw(tileSet, new Vector2(x * 16, y * 16), GetTileRectangle(newtile), Color.White, 0.0f, new Vector2(0.0f, 0.0f), 1.0f, SpriteEffects.None, 0.0f);
			}
		}
		private void ProcessSmartBrush(Point center, int size, SpriteBatch spriteBatch)
		{
			int minsize = -((size + 2) / 2);
			int maxsize = ((size + 2) / 2);

			SmartBrushAdjustRow(center.Y + minsize, center.X, size + 2, spriteBatch);
			SmartBrushAdjustRow(center.Y + minsize + 1, center.X, size, spriteBatch);

			SmartBrushAdjustRow(center.Y + maxsize, center.X, size + 2, spriteBatch);
			SmartBrushAdjustRow(center.Y + maxsize - 1, center.X, size, spriteBatch);

			SmartBrushAdjustColumn(center.Y, center.X + minsize, size + 2, spriteBatch);
			SmartBrushAdjustColumn(center.Y, center.X + minsize + 1, size, spriteBatch);

			SmartBrushAdjustColumn(center.Y, center.X + maxsize, size + 2, spriteBatch);
			SmartBrushAdjustColumn(center.Y, center.X + maxsize - 1, size, spriteBatch);
		}
		public void UpdateDomain(MouseState mouse, CurrentTool tool)
		{
			if ((!mouse.LeftDown && !mouse.LeftClick) || tool.Tool != ToolAction.Domains)
			{
				return;
			}

			int middlex = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom)) / 32;
			int middley = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom)) / 32;

			if (middlex >= MapSizeX || middley >= MapSizeY || middlex < 0 || middley < 0)
			{
				return;
			}

			domains[middley * 8 + middlex] = (middley * 8 + middlex, tool.EncounterGroup);

			UnsavedChanges = true;
		}
		public void UpdateDock(MouseState mouse, CurrentTool tool)
		{
			if ((!mouse.LeftDown && !mouse.LeftClick) || tool.Tool != ToolAction.Docks)
			{
				return;
			}

			int middlex = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int middley = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

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
		public void UpdateMapObject(MouseState mouse, CurrentTool tool)
		{
			if ((!mouse.LeftDown && !mouse.LeftClick) || tool.Tool != ToolAction.MapObjects)
			{
				return;
			}

			if (tool.MapObject == MapObject.Ship)
			{
				return;
			}

			int middlex = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int middley = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

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
		public List<EditorTask> GetTile(MouseState mouse)
		{
			if (!mouse.RightClick)
			{
				return new();
			}

			int targetx = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int targety = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

			if (targetx >= MapSizeX || targety >= MapSizeY || targetx < 0 || targety < 0)
			{
				return new();
			}

			int tilevalue = overworldMap[targety * MapSizeX + targetx];

			return new List<EditorTask>() {
				new EditorTask() { Type = EditorTasks.TilesPickerUpdate, Value = tilevalue },
				new EditorTask() { Type = EditorTasks.TilesUpdate, Value = tilevalue },
			};
		}
		private Rectangle GetTileRectangle(byte tile)
		{
			int tilex = tile % 0x10;
			int tiley = tile / 0x10;

			return new Rectangle(tilex * 16, tiley * 16, 16, 16);
		}
		public void Draw(SpriteBatch spriteBatch, WindowsManager manager, CurrentTool tool, MouseState mouse, Point windowSize)
		{
			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			spriteBatch.Draw(mapTexture, viewOffset, new Rectangle(0, 0, mapTexture.Width, mapTexture.Height), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);

			DrawBrush(spriteBatch, tool, mouse);
			DrawTemplate(spriteBatch, tool, mouse);

			if (manager.ShowDomainOverlay)
			{
				DrawDomainsOverlay(spriteBatch);
			}
			else if (manager.ShowGridlines)
			{
				DrawGrid(spriteBatch);
			}

			if (manager.ShowDockOverlay)
			{
				DrawDocksOverlay(spriteBatch);
			}

			if (manager.ShowMapObjectsOverlay)
			{
				DrawMapObjectsOverlay(spriteBatch);
			}

			DrawCoordinate(spriteBatch, windowSize, mouse);


			spriteBatch.End();

		}
		public void DrawGrid(SpriteBatch spriteBatch)
		{
			Texture2D line = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			line.SetData(new[] { Color.Red });

			int liney = MapSizeY / GridSize;
			int linex = MapSizeX / GridSize;


			for (int y = 0; y < liney; y++)
			{
				for (int x = 0; x < linex; x++)
				{
					spriteBatch.Draw(line, new Vector2(viewOffset.X + (x * GridSize * 16) * Zoom, viewOffset.Y + (y * GridSize * 16) * Zoom), new Rectangle(0, 0, (int)(GridSize * 16 * Zoom), 2), Color.White);
					spriteBatch.Draw(line, new Vector2(viewOffset.X + (x * GridSize * 16) * Zoom, viewOffset.Y + (y * GridSize * 16) * Zoom), new Rectangle(0, 0, 2, (int)(GridSize * 16 * Zoom)), Color.White);
				}
			}

			spriteBatch.Draw(line, new Vector2(viewOffset.X + ((linex * GridSize * 16) * Zoom - 2), viewOffset.Y), new Rectangle(0, 0, 2, (int)(MapSizeY * 16 * Zoom)), Color.White);
			spriteBatch.Draw(line, new Vector2(viewOffset.X, viewOffset.Y + ((liney * GridSize * 16) * Zoom - 2)), new Rectangle(0, 0, (int)(MapSizeX * 16 * Zoom), 2), Color.White);
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

		public void DrawDomainsOverlay(SpriteBatch spriteBatch)
		{
			Texture2D line = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
			line.SetData(new[] { Color.Red });

			Dictionary<OwEncounterGroup, int> domainOrdered = Enum.GetValues<OwEncounterGroup>().Select((o, i) => (o, i)).ToDictionary(g => g.o, g => g.i);

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					spriteBatch.Draw(line, new Vector2(viewOffset.X + (x * 32 * 16) * Zoom, viewOffset.Y + (y * 32 * 16) * Zoom), new Rectangle(0, 0, (int)(32 * 16 * Zoom), 2), Color.White);
					spriteBatch.Draw(line, new Vector2(viewOffset.X + (x * 32 * 16) * Zoom, viewOffset.Y + (y * 32 * 16) * Zoom), new Rectangle(0, 0, 2, (int)(32 * 16 * Zoom)), Color.White);

					string coordstring = "(" + x + ", " + y + ")";

					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + (x * 32 * 16) * Zoom + 3, viewOffset.Y + (y * 32 * 16) * Zoom + 4), Color.Black);
					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + (x * 32 * 16) * Zoom + 5, viewOffset.Y + (y * 32 * 16) * Zoom + 4), Color.Black);
					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + (x * 32 * 16) * Zoom + 4, viewOffset.Y + (y * 32 * 16) * Zoom + 3), Color.Black);
					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + (x * 32 * 16) * Zoom + 4, viewOffset.Y + (y * 32 * 16) * Zoom + 5), Color.Black);
					spriteBatch.DrawString(font, coordstring, new Vector2(viewOffset.X + (x * 32 * 16) * Zoom + 4, viewOffset.Y + (y * 32 * 16) * Zoom + 4), Color.White);

					var domaintype = domainOrdered[domains[x + (y * 8)].group];
					int domaintypex = domaintype % 8;
					int domaintypey = domaintype / 8;

					spriteBatch.Draw(domainGroupIcons, new Vector2(viewOffset.X + (x * 32 * 16) * Zoom + 8, viewOffset.Y + (y * 32 * 16) * Zoom + 24), new Rectangle(domaintypex * 32, domaintypey * 32, 32, 32), Color.White);
				}
			}

			spriteBatch.Draw(line, new Vector2(viewOffset.X + ((8 * 32 * 16) * Zoom - 2), viewOffset.Y), new Rectangle(0, 0, 2, (int)(256 * 16 * Zoom)), Color.White);
			spriteBatch.Draw(line, new Vector2(viewOffset.X, viewOffset.Y + ((8 * 32 * 16) * Zoom - 2)), new Rectangle(0, 0, (int)(256 * 16 * Zoom), 2), Color.White);
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
					spriteBatch.Draw(tileSet, new Vector2(viewOffset.X + (middlex + x) * 16 * Zoom, viewOffset.Y + (middley + y) * 16 * Zoom), GetTileRectangle(tool.Tile), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);
				}
			}
		}
		public void DrawTemplate(SpriteBatch spriteBatch, CurrentTool tool, MouseState mouse)
		{
			if (tool.Tool != ToolAction.Templates)
			{
				return;
			}

			int sizex = tool.Template.GetLength(1);
			int sizey = tool.Template.GetLength(0);

			int minsizex = sizex / 2;
			int minsizey = sizey / 2;

			int middlex = ((int)(mouse.Position.X - viewOffset.X) / (int)(16 * Zoom));
			int middley = ((int)(mouse.Position.Y - viewOffset.Y) / (int)(16 * Zoom));

			for (int y = 0; y < sizey; y++)
			{
				for (int x = 0; x < sizex; x++)
				{
					spriteBatch.Draw(tileSet, new Vector2(viewOffset.X + (middlex + x -minsizex) * 16 * Zoom, viewOffset.Y + (middley + y -minsizey) * 16 * Zoom), GetTileRectangle(tool.Template[y,x]), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Zoom, SpriteEffects.None, 0.0f);
				}
			}
		}
		public void DrawDocksOverlay(SpriteBatch spriteBatch)
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

				spriteBatch.Draw(docksIcons, new Vector2(viewOffset.X + (dock.coord.X * 16) * Zoom + (count * 16 * Math.Max(Zoom / 2, 1f)), viewOffset.Y + (dock.coord.Y * 16) * Zoom), new Rectangle(dockx * 32, docky * 32, 32, 32), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Math.Max(Zoom / 2, 1f), SpriteEffects.None, 0.0f);
			}
		}
		public void DrawMapObjectsOverlay(SpriteBatch spriteBatch)
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

				spriteBatch.Draw(mapObjectsIcons, new Vector2(viewOffset.X + (mapobject.coord.X * 16) * Zoom + (count * 16 * Math.Max(Zoom, 2f)), viewOffset.Y + (mapobject.coord.Y * 16) * Zoom), new Rectangle(mapobjectx * 16, mapobjecty * 16, 16, 16), Color.White, 0.0f, new Vector2(0.0f, 0.0f), Math.Max(Zoom, 2f), SpriteEffects.None, 0.0f);
			}
		}
	}
}