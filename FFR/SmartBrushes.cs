using System.Collections.Generic;
using System.Linq;
using System;

namespace FFRMapEditorMono.FFR
{
	public class SmartBrush
	{
		public TileGroup Group { get; set; }

		private List<byte> tiles;

		public SmartBrush(TileGroup _group, List<byte> _tiles)
		{
			Group = _group;
			tiles = _tiles;
		}
		public byte GetTile(List<List<byte>> mapcluster)
		{
			List<List<TileGroup>> clustergroup = new();
			List<TileType> validTiles = Enum.GetValues<TileType>().ToList();
			TileGroup currentile = TileGroup.Other;
			TileGroup mainType = OwDataGroup.TileGroupCompatibility[Group];

			if (OwDataGroup.TileByteToGroup.TryGetValue(mapcluster[0][1], out currentile))
			{
				currentile = OwDataGroup.TileGroupCompatibility[currentile];

				if (currentile == mainType)
				{
					validTiles.Remove(TileType.North);
					validTiles.Remove(TileType.NorthEast);
					validTiles.Remove(TileType.NorthWest);
				}
			}

			if (OwDataGroup.TileByteToGroup.TryGetValue(mapcluster[2][1], out currentile))
			{
				currentile = OwDataGroup.TileGroupCompatibility[currentile];

				if (currentile == mainType)
				{
					validTiles.Remove(TileType.South);
					validTiles.Remove(TileType.SouthEast);
					validTiles.Remove(TileType.SouthWest);
				}
			}

			if (OwDataGroup.TileByteToGroup.TryGetValue(mapcluster[1][0], out currentile))
			{
				currentile = OwDataGroup.TileGroupCompatibility[currentile];

				if (currentile == mainType)
				{
					validTiles.Remove(TileType.West);
					validTiles.Remove(TileType.NorthWest);
					validTiles.Remove(TileType.SouthWest);
				}
			}

			if (OwDataGroup.TileByteToGroup.TryGetValue(mapcluster[1][2], out currentile))
			{
				currentile = OwDataGroup.TileGroupCompatibility[currentile];

				if (currentile == mainType)
				{
					validTiles.Remove(TileType.East);
					validTiles.Remove(TileType.NorthEast);
					validTiles.Remove(TileType.SouthEast);
				}
			}

			if (validTiles.Count > 1)
			{
				validTiles.Remove(TileType.Center);
			}

			List<TileType> priorityTiles = new() { TileType.NorthWest, TileType.NorthEast, TileType.SouthEast, TileType.SouthWest };

			validTiles = validTiles.OrderByDescending(t => priorityTiles.Contains(t)).ToList();

			return tiles[(int)validTiles.First()];
		}
	}
}