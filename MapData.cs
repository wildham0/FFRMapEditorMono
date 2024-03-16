using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Security.Cryptography;

namespace FFRMapEditorMono
{
	public enum OverworldTeleportIndex : byte
	{
		CardiaNorth = 0,
		Coneria = 1,
		Pravoka = 2,
		Elfland = 3,
		Melmond = 4,
		CrescentLake = 5,
		Gaia = 6,
		Onrac = 7,
		Lefein = 8,
		ConeriaCastle = 9,
		ElflandCastle = 10,
		NorthwestCastle = 11,
		CastleOrdeals = 12,
		TempleOfFiends = 13,
		EarthCave = 14,
		GurguVolcano = 15,
		IceCave = 16,
		CardiaGrass = 17,
		BahamutCave = 18,
		Waterfall = 19,
		DwarfCave = 20,
		MatoyasCave = 21,
		SardasCave = 22,
		MarshCave = 23,
		MirageTower = 24,
		TitansTunnelEast = 25,
		TitansTunnelWest = 26,
		CardiaMarsh = 27,
		CardiaSmall = 28,
		CardiaForest = 29,
		DefaultLocation = 30,
		Unused = 31,
		None = 0xff
	}
	public enum ExitTeleportIndex : byte
	{
		ExitTitanE = 0,
		ExitTitanW = 1,
		ExitIceCave = 2,
		ExitCastleOrdeals = 3,
		ExitCastleConeria = 4,
		ExitEarthCave = 5,
		ExitGurguVolcano = 6,
		ExitSeaShrine = 7,
		ExitSkyPalace = 8,
		ExitUnused1 = 9,
		ExitUnused2 = 10,
		ExitUnused3 = 11,
		ExitUnused4 = 12,
		ExitUnused5 = 13,
		ExitUnused6 = 14,
		ExitUnused7 = 15,
		None = 255
	}
	public enum OwEncounterGroup
	{
		UpperOnracGroup = 0,
		NorthernEdgeGroup = 2,
		OrdealsGroup = 3,
		LowerOnracGroup = 8,
		CardiaGroup = 10,
		MirageDesertGroup = 13,
		LefeinGroup = 15,
		OopsAllImpsGroup = 18,
		TempleOfFiendsGroup = 27,
		ElflandGroup = 29,
		MelmondGroup = 33,
		ConeriaGroup = 36,
		PravokaGroup = 37,
		IceCaveGroup = 39,
		AspWolfGroup = 45,
		CrescentLakeGroup = 54,
	}
	public struct SCCoords
	{
		public byte X { get; set; }

		public byte Y { get; set; }

		public SCCoords(int x, int y)
		{
			X = (byte)x;
			Y = (byte)y;
		}
	}
	public class ShipLocation
	{
		public ShipLocation() { }

		public ShipLocation(byte x, byte y, byte teleporterIndex)
		{
			X = x;
			Y = y;
			TeleporterIndex = teleporterIndex;
		}
		public byte TeleporterIndex { get; set; }

		public byte X { get; set; }

		public byte Y { get; set; }
	}
	public enum TeleportType
	{
		Enter,
		Exit,
		Tele
	}

	public class TeleportFixup
	{
		public TeleportFixup() { }
		public TeleportFixup(TeleportType tp, int idx, TeleData to)
		{
			this.Type = tp;
			this.Index = idx;
			this.To = to;
		}

		public TeleportType Type { get; set; }

		public int? Index { get; set; }

		public TeleData? From { get; set; }

		public TeleData To { get; set; }

	}
	public enum MapId
	{ 
		Overworld = 0,
		None = 255,
	}

	public struct TeleData
	{
		public TeleData(MapId m, byte x, byte y)
		{
			Map = m;
			X = x;
			Y = y;
		}
		public MapId Map { get; set; }
		public byte X { get; set; }
		public byte Y { get; set; }

		public void FlipXcoordinate()
		{
			var x1 = X & 0x3F;
			var x2 = X & 0xC0;

			X = (byte)((64 - x1 - 1) | x2);
		}

		public void FlipYcoordinate()
		{
			var y1 = Y & 0x3F;
			var y2 = Y & 0xC0;

			Y = (byte)((64 - y1 - 1) | y2);
		}
	}
	public class DomainFixup
	{
		public byte From { get; set; }

		public byte To { get; set; }
	}
	public class OwMapExchangeData
	{
		public OwMapExchangeData(OwMapExchangeData copy)
		{
			this.StartingLocation = copy.StartingLocation;
			this.AirShipLocation = copy.AirShipLocation;
			this.BridgeLocation = copy.BridgeLocation;
			this.CanalLocation = copy.CanalLocation;
			this.ShipLocations = copy.ShipLocations;
			this.TeleporterFixups = copy.TeleporterFixups;
			this.DomainFixups = copy.DomainFixups;
			this.DomainUpdates = copy.DomainUpdates;
			this.OverworldCoordinates = copy.OverworldCoordinates;
			this.DecompressedMapRows = copy.DecompressedMapRows;
			this.FFRVersion = copy.FFRVersion;
			this.Checksum = copy.Checksum;
			this.Seed = copy.Seed;
			this.HorizontalBridge = copy.HorizontalBridge;
		}
		public string FFRVersion { get; set; }
		public string Checksum { get; set; }
		public int Seed { get; set; }
		public SCCoords? StartingLocation { get; set; }
		public SCCoords? AirShipLocation { get; set; }
		public SCCoords? BridgeLocation { get; set; }
		public SCCoords? CanalLocation { get; set; }
		public ShipLocation[] ShipLocations { get; set; }
		public TeleportFixup[] TeleporterFixups { get; set; }
		public DomainFixup[] DomainFixups { get; set; }
		public DomainFixup[] DomainUpdates { get; set; }
		public Dictionary<string, SCCoords> OverworldCoordinates { get; set; }
		public List<string> DecompressedMapRows { get; set; }
		public bool HorizontalBridge { get; set; }
		public OwMapExchangeData()
		{
			Seed = 0xFFFFFF;
			FFRVersion = "1.0";
			Checksum = "";
			StartingLocation = new SCCoords(0, 0);
			AirShipLocation = new SCCoords(0, 0);
			BridgeLocation = new SCCoords(0, 0);
			CanalLocation = new SCCoords(0, 0);
			ShipLocations = new ShipLocation[0];
			TeleporterFixups = new TeleportFixup[0];
			DomainFixups = new DomainFixup[0];
			DomainUpdates = new DomainFixup[0];
			OverworldCoordinates = new();
			DecompressedMapRows = new();
			HorizontalBridge = false;
		}
		public string ComputeChecksum()
		{
			var copy = new OwMapExchangeData(this);
			copy.FFRVersion = "";
			copy.Checksum = "";
			copy.Seed = 0;

			var content = JsonSerializer.Serialize<OwMapExchangeData>(copy);
			//content = "\'" + content.Replace("\\u002B", "+") + "\'";
			content = content.Replace("\\u002B", "+");

			using (SHA256 hasher = SHA256.Create())
			{
				byte[] JsonBlob = Encoding.UTF8.GetBytes(content);
				byte[] hash = hasher.ComputeHash(JsonBlob);
				return Convert.ToHexString(hash).Substring(0, 32);
			}
		}
		public byte[] DecodeMap()
		{
			var rows = new List<List<byte>>();
			foreach (var c in DecompressedMapRows)
			{
				rows.Add(new List<byte>(Convert.FromBase64String(c)));
			}

			return rows.SelectMany(x => x).ToArray();
		}
		public void EncodeMapFromBytes(byte[] data)
		{
			List<string> rows = new();
			var map = data;

			for (int y = 0; y < 256; y++)
			{
				rows.Add(Convert.ToBase64String(map.Skip(256 * y).Take(256).ToArray()));
			}

			DecompressedMapRows = rows;
		}
		public void EncodeMap(Overworld overworld)
		{
			var map = overworld.GetOwBytes();
			EncodeMapFromBytes(map);

			OverworldCoordinates = new();
			List<(OverworldTeleportIndex id, SCCoords coord)> coordList = new();

			foreach (var entry in OwDataGroup.OwTeleportTiles)
			{
				var foundtiles = map.Select((x, i) => (i, x)).Where(b => b.x == entry.tile).ToList();
				if (!foundtiles.Any())
				{
					continue;
				}
				var index = foundtiles.Last().i;


				OverworldCoordinates.Add(Enum.GetName<OverworldTeleportIndex>(entry.id), new SCCoords(index % 256, index / 256));
				coordList.Add((entry.id, new SCCoords(index % 256, index / 256)));
			}

			List<TeleportFixup> tempFixups = new();

			foreach (var exit in OwDataGroup.ExitTeleportTiles.Select(x => x.id).ToList())
			{
				var coord = coordList.Find(x => x.id == OwDataGroup.ExitTeleportOwTeleporter[exit]).coord;

				tempFixups.Add(new TeleportFixup(TeleportType.Exit, (int)exit, new TeleData(MapId.None, coord.X, coord.Y)));
			}

			TeleporterFixups = tempFixups.OrderBy(t => t.Index).ToArray();

			List<byte> dockTiles = new() { 0x0F, 0x1F, 0x77, 0x78, 0x79, 0x7A };
			List<byte> seaTiles = new() { 0x07, 0x16, 0x17, 0x18, 0x27 };

			DomainUpdates = overworld.GetDomainsData().ToArray();

			ShipLocations = overworld.GetShipData().ToArray();

			StartingLocation = overworld.GetMapObjectPosition(MapObject.StartingPosition);
			BridgeLocation = overworld.GetMapObjectPosition(MapObject.Bridge);
			CanalLocation = overworld.GetMapObjectPosition(MapObject.Canal);
			AirShipLocation = overworld.GetMapObjectPosition(MapObject.Airship);

			Checksum = ComputeChecksum();
		}
	}
}