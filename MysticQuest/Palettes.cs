using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono.MysticQuest
{
	public class SnesColor
	{
		public int Red { get; set; }
		public int Green { get; set; }
		public int Blue { get; set; }
		public SnesColor()
		{ }
	}
	class MapPalettes
	{
		public List<List<Microsoft.Xna.Framework.Color>> Palettes;

		public MapPalettes()
		{
			var paletteJson = ReadResource("FFMQR_MapPalettes.json");
			var systemPalettes = JsonSerializer.Deserialize<List<List<SnesColor>>>(paletteJson);
			Palettes = systemPalettes.Select(p => p.Select(c => new Microsoft.Xna.Framework.Color(c.Red, c.Green, c.Blue)).ToList()).ToList();
		}
		private string ReadResource(string name)
		{
			// Determine path
			var assembly = Assembly.GetExecutingAssembly();
			string resourcePath = name;
			resourcePath = assembly.GetManifestResourceNames()
					.Single(str => str.EndsWith(name));

			using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}

	public class GraphicTile
	{
		public byte[] GraphicData { get; set; }
		public int Palette { get; set; }
		public GraphicTile() {	}
	}
	public class GraphicRows
	{
		public List<List<GraphicTile>> Rows { get; set; }
		public GraphicRows()
		{

			var rowsJson = ReadResource("FFMQR_GraphicRows.json");
			Rows = JsonSerializer.Deserialize<List<List<GraphicTile>>>(rowsJson);
		}
		private string ReadResource(string name)
		{
			// Determine path
			var assembly = Assembly.GetExecutingAssembly();
			string resourcePath = name;
			resourcePath = assembly.GetManifestResourceNames()
					.Single(str => str.EndsWith(name));

			using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}

	public class TilesProperties
	{
		public List<List<SingleTile>> Tiles;

		public TilesProperties()
		{
			var tilesJson = ReadResource("FFMQR_TilesProperties.json");
			Tiles = JsonSerializer.Deserialize<List<List<SingleTile>>>(tilesJson);
		}
		private string ReadResource(string name)
		{
			// Determine path
			var assembly = Assembly.GetExecutingAssembly();
			string resourcePath = name;
			resourcePath = assembly.GetManifestResourceNames()
					.Single(str => str.EndsWith(name));

			using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}

	public class GraphicTileProp
	{
		public byte Tile { get; set; }
		public bool HorizontalFlip { get; set; }
		public GraphicTileProp() { }
		public GraphicTileProp(byte tile, bool hflip)
		{
			Tile = tile;
			HorizontalFlip = hflip;
		}
	}

	public enum GraphicTiles
	{
		UpperLeft = 0,
		UpperRight,
		LowerLeft,
		LowerRight
	}

	public class SingleTile
	{
		public byte PropertyByte1 { get; set; }
		public byte PropertyByte2 { get; set; }
		public List<GraphicTileProp> GraphicTiles { get; set; }
		private List<byte> flipmask = new() { 0x01, 0x02, 0x04, 0x08 };
		public SingleTile() { }
		public SingleTile(byte[] tileprop, byte[] graphic, byte hflip)
		{
			PropertyByte1 = tileprop[0];
			PropertyByte2 = tileprop[1];

			GraphicTiles = new();

			for (int i = 0; i < 4; i++)
			{
				GraphicTiles.Add(new GraphicTileProp(graphic[i], (hflip & flipmask[i]) > 0));
			}
		}

		public byte[] GetPropBytes()
		{
			return new byte[] { PropertyByte1, PropertyByte2 };
		}
		public byte[] GetGraphicBytes()
		{
			return GraphicTiles.Select(t => t.Tile).ToArray();
		}
		public byte GetFlipByte()
		{
			int flipbyte = 0x00;

			for (int i = 0; i < 4; i++)
			{
				flipbyte |= GraphicTiles[i].HorizontalFlip ? flipmask[i] : 0x00;
			}

			return (byte)flipbyte;
		}
	}
}