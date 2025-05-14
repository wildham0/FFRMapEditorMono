using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.DirectoryServices.ActiveDirectory;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FFRMapEditorMono.MysticQuest
{
	public enum MapLayers
	{
		EditLayer1,
		EditLayer2,
		ViewLayer1,
		ViewLayer2,
	}
	public class MapAttributes
	{
		public int TilesProperties { get; set; }
		public List<byte> GraphicRows { get; set; }
		public int MapDimensionId { get; set; }
		public byte Palette { get; set; }
		public MapAttributes()
		{
			TilesProperties = 0;
			MapDimensionId = 0;
			Palette = 0;
			GraphicRows = new() { 0, 0, 0, 0, 0, 0, 0, 0 };
		}

		public MapAttributes(MapAttributes attributes)
		{
			GraphicRows = attributes.GraphicRows.ToList();
			TilesProperties = attributes.TilesProperties;
			MapDimensionId = attributes.MapDimensionId;
			Palette = attributes.Palette;
		}
	}
	public class JsonMap
	{
		public string Map { get; set; }
		public MapAttributes Attributes { get; set; }
		public JsonMap()
		{
			Attributes = new();
		}
		public byte[] ToBytes()
		{
			return Convert.FromBase64String(Map);
		}
		public JsonMap(MapAttributes mapattributes, string maptiles)
		{
			Attributes = new MapAttributes(mapattributes);
			Map = maptiles;
		}
		public JsonMap(string json)
		{
			JsonMap jsonvalues = JsonSerializer.Deserialize<JsonMap>(json);
			Attributes = new(jsonvalues.Attributes);
			Map = jsonvalues.Map;
		}
		public string ToJson()
		{
			return JsonSerializer.Serialize(this);
		}
		
	}

	public class CanvasMQ : Canvas
	{
		private List<(int x, int y)> mapDimensions = new() { (0x10, 0x10), (0x20, 0x10), (0x30, 0x10), (0x40, 0x10), (0x10, 0x20), (0x20, 0x20), (0x30, 0x20), (0x40, 0x20), (0x10, 0x30), (0x20, 0x30), (0x30, 0x30), (0x40, 0x30), (0x10, 0x40), (0x20, 0x40), (0x30, 0x40), (0x40, 0x40) };

		//private Texture2D tileSet;
		//private RenderTarget2D mapTexture;
		private Texture2D pixelTexture;

		public RenderTarget2D tilesGraphics;
		//public RenderTarget2D tileSet;
		private MapAttributes attributes;
		private MapPalettes MapPalettes;
		private GraphicRows GraphicRows;
		private TilesProperties TilesProperties;
		public List<SingleTile> Tiles => TilesProperties.Tiles[attributes.TilesProperties];
		private MapLayers CurrentLayer = MapLayers.EditLayer1;
		//private MapLayers DrawingLayer = MapLayers.Layer1;
		//private Dictionary<MapLayers, int> layerMasks = new() { { MapLayers.Layer1, 0x7F }, { MapLayers.Layer1, 0xFF }, }
		//public CanvasMQ(Dictionary<string, Texture2D> textures, SpriteFont _font, Texture2D _domaingroups, Texture2D _docks, Texture2D _mapobjects, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, FileManager _fileManager)
		public CanvasMQ(Dictionary<string, Texture2D> _textures, SpriteFont _font, GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, FileManager _fileManager, MouseState _mouse) : base(_textures, _font, _graphicsDevice, _spriteBatch, _fileManager, _mouse)
		{
			pixelTexture = _textures["pixel"];
			//fileManager = _fileManager;

			attributes = new();
			MapPalettes = new();
			GraphicRows = new();
			TilesProperties = new();

			DrawTilesGraphics();
			BlueMap();
		}
		private void DrawTilesGraphics()
		{
			tilesGraphics = new(graphicsDevice, 0x20*8, 0x20*8, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
			var palette = MapPalettes.Palettes[attributes.Palette];
			var tilesProp = TilesProperties.Tiles[attributes.TilesProperties];

			graphicsDevice.SetRenderTarget(tilesGraphics);
			graphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			for (int i = 0; i < 8; i++)
			{
				var rowId = attributes.GraphicRows[i];
				if (rowId == 255)
				{
					continue;
				}
				var row = GraphicRows.Rows[rowId];
				for (var tiley = 0; tiley < 2; tiley++)
				{
					for (var tilex = 0; tilex < 0x10; tilex++)
					{
						var currentile = row[tilex + tiley * 0x10];

						for (int y = 0; y < 8; y++)
						{
							for (int x = 0; x < 8; x++)
							{
								var targetx = tilex * 8 + x;
								var targety = i * 16 + tiley * 8 + y;
								var color = palette[currentile.GraphicData[x * 8 + y] + 8 * currentile.Palette];
								color = new Color(color.R * 8, color.G * 8, color.B * 8);

								spriteBatch.Draw(pixelTexture, new Vector2(targetx, targety), color);
							}
						}
					}
				}
			}

			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);

			RenderTarget2D tileSetRender = new(graphicsDevice, 0x10 * 16, 0x08 * 16, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
			graphicsDevice.SetRenderTarget(tileSetRender);
			graphicsDevice.Clear(Color.White);
			spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			for (int y = 0; y < 0x08; y++)
			{
				for (int x = 0; x < 0x10; x++)
				{
					var currentile = tilesProp[y * 0x10 + x];

					var tiley = currentile.GraphicTiles[0].Tile / 0x10;
					var tilex = currentile.GraphicTiles[0].Tile % 0x10;
					spriteBatch.Draw(tilesGraphics, new Vector2(x * 16, y * 16), new Rectangle(tilex * 8, tiley * 8, 8, 8), Color.White, 0.0f, new Vector2(0,0), new Vector2(1.0f, 1.0f), currentile.GraphicTiles[0].HorizontalFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1.0f);

					tiley = currentile.GraphicTiles[1].Tile / 0x10;
					tilex = currentile.GraphicTiles[1].Tile % 0x10;
					spriteBatch.Draw(tilesGraphics, new Vector2(x * 16 + 8, y * 16), new Rectangle(tilex * 8, tiley * 8, 8, 8), Color.White, 0.0f, new Vector2(0, 0), new Vector2(1.0f, 1.0f), currentile.GraphicTiles[1].HorizontalFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1.0f);

					tiley = currentile.GraphicTiles[2].Tile / 0x10;
					tilex = currentile.GraphicTiles[2].Tile % 0x10;
					spriteBatch.Draw(tilesGraphics, new Vector2(x * 16, y * 16 + 8), new Rectangle(tilex * 8, tiley * 8, 8, 8), Color.White, 0.0f, new Vector2(0, 0), new Vector2(1.0f, 1.0f), currentile.GraphicTiles[2].HorizontalFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1.0f);

					tiley = currentile.GraphicTiles[3].Tile / 0x10;
					tilex = currentile.GraphicTiles[3].Tile % 0x10;
					spriteBatch.Draw(tilesGraphics, new Vector2(x * 16 + 8, y * 16 + 8), new Rectangle(tilex * 8, tiley * 8, 8, 8), Color.White, 0.0f, new Vector2(0, 0), new Vector2(1.0f, 1.0f), currentile.GraphicTiles[3].HorizontalFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1.0f);
				}
			}

			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);

			TileSet = tileSetRender;
		}
		public override void LoadData()
		{

			attributes = new(fileManager.MapDataMQ.Attributes);

			MapSizeX = mapDimensions[attributes.MapDimensionId].x;
			MapSizeY = mapDimensions[attributes.MapDimensionId].y;

			targetMaps = Enumerable.Range(0, owMapUndoDepth + 1).Select(i => new byte[MapSizeX * MapSizeY]).ToList();
			owMapCurrentTarget = 0;
			targetMaps[owMapCurrentTarget] = new byte[MapSizeX * MapSizeY];
			fileManager.MapDataMQ.ToBytes().CopyTo(mapCanvas, 0);

			mapTexture = new(graphicsDevice, MapSizeX * 16, MapSizeY * 16, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

			DrawTilesGraphics();
			DrawMap();
			viewOffset = new(0, 0);
			Zoom = 1.0f;
		}
		public override JsonMap ExportJsonMap()
		{
			return new JsonMap(attributes, Convert.ToBase64String(mapCanvas));
		}
		public override void ProcessTasks(TaskManager tasks)
		{
			base.ProcessTasks(tasks);

			EditorTask task;

			if (tasks.Pop(EditorTasks.ToggleLayer, out task))
			{
				CurrentLayer += task.Value;
				if (CurrentLayer > MapLayers.ViewLayer2) CurrentLayer = MapLayers.EditLayer1;
				if (CurrentLayer < MapLayers.EditLayer1) CurrentLayer = MapLayers.ViewLayer2;

				tasks.Add(new EditorTask(EditorTasks.InfoBoxUpdateLayer, (int)CurrentLayer));
				DrawMap();


			}

			if (tasks.Pop(EditorTasks.ResizeMap, out task))
			{
				ResizeMap(task.Value);
				DrawMap();
			}
		}
		private void ResizeMap(int targetsize)
		{
			int tempx = mapDimensions[targetsize].x;
			int tempy = mapDimensions[targetsize].y;

			var tempmap = new byte[tempx * tempy];

			int maxx = Math.Min(tempx, MapSizeX);
			int maxy = Math.Min(tempy, MapSizeY);

			//targetMaps = Enumerable.Range(0, owMapUndoDepth + 1).Select(i => new byte[MapSizeX * MapSizeY]).ToList();

			for (int y = 0; y < maxy; y++)
			{
				for (int x = 0; x < maxx; x++)
				{
					tempmap[x + y * tempx] = targetMaps[owMapCurrentTarget][x + y * MapSizeX];
				}
			}

			MapSizeX = tempx;
			MapSizeY = tempy;

			attributes.MapDimensionId = targetsize;

			targetMaps = Enumerable.Range(0, owMapUndoDepth + 1).Select(i => new byte[MapSizeX * MapSizeY]).ToList();
			owMapCurrentTarget = 0;
			tempmap.CopyTo(targetMaps[owMapCurrentTarget], 0);
			mapTexture = new(graphicsDevice, MapSizeX * 16, MapSizeY * 16, true, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
		}
		protected override byte AdjustPutTile(byte tile)
		{
			return (CurrentLayer == MapLayers.EditLayer2 || CurrentLayer == MapLayers.ViewLayer2) ? (byte)(tile | 0x80) : tile;
		}
		protected override byte AdjustGetTile(byte tile)
		{
			return (byte)(tile & 0x7F);
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
					bool layer2 = (tilevalue & 0x80) > 0;
					bool translucent = false;
					tilevalue = tilevalue & 0x7F;

					if (CurrentLayer == MapLayers.EditLayer1 && layer2)
					{
						translucent = true;
					}
					else if (CurrentLayer == MapLayers.EditLayer2 && !layer2)
					{
						translucent = true;
					}
					else if (CurrentLayer == MapLayers.ViewLayer1 && layer2)
					{
						tilevalue = 0x00;
					}
					else if (CurrentLayer == MapLayers.ViewLayer2 && !layer2)
					{
						tilevalue = 0x00;
					}

					int xoffset = tilevalue % 0x10;
					int yoffset = tilevalue / 0x10;


					if (translucent)
					{
						spriteBatch.Draw(TileSet, new Vector2(x * 16, y * 16), new Rectangle(xoffset * 16, yoffset * 16, 16, 16), new Color(100, 100, 100, 255));
					}
					else
					{
						spriteBatch.Draw(TileSet, new Vector2(x * 16, y * 16), new Rectangle(xoffset * 16, yoffset * 16, 16, 16), Color.White);
					}
					
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
					mapCanvas[MapSizeX * y + x] = 0x00;
					int tilevalue = 0x00;
					int xoffset = tilevalue % 0x10;
					int yoffset = tilevalue / 0x10;

					spriteBatch.Draw(TileSet, new Vector2(x * 16, y * 16), new Rectangle(xoffset * 16, yoffset * 16, 16, 16), Color.White);
				}
			}

			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);
		}
	}
}