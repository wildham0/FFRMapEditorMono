using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;

namespace FFRMapEditorMono.FFR
{
	public class DockPicker : OptionPicker
	{
		private CanvasFFR overworld;
		public DockPicker(Texture2D _window, Texture2D _selector, Texture2D _placedicons, SpriteFont _font, Canvas _overworld)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionIcons = _placedicons;
			optionFont = _font;
			overworld = (CanvasFFR)_overworld;

			Position = new Vector2(64, 0);
			zoom = 1.0f;
			optionsRows = 2;
			optionsColumns = 8;
			optionsSize = 32;

			options = OwTpIndexToName.Select(d => (d.Value, 
				new List<EditorTask>() {
					new EditorTask(EditorTasks.DocksUpdate,(int) d.Key) },
				new List<EditorTask>() {
					new EditorTask(EditorTasks.DocksRemove,(int) d.Key) }
				)).ToList();

			Show = false;
			lastSelection = 0x00;
			placedOptions = new();
			unplacedOptions = new();
			SetOptionTextLength();
			showPlaced = true;
		}
		public override void ProcessTasks(TaskManager tasks)
		{
			EditorTask task;

			if (tasks.Pop(EditorTasks.UpdatePlacedDocksOverlay, out task))
			{
				placedOptions = overworld.GetShipData().Select(d => (int)d.TeleporterIndex).ToList();
				if (placedOptions.Contains((int)OverworldTeleportIndex.None))
				{
					placedOptions.RemoveAll(o => o == (int)OverworldTeleportIndex.None);
					placedOptions.Add((int)OverworldTeleportIndex.DefaultLocation);
					unplacedOptions = new();
				}
				else
				{
					unplacedOptions.Add((int)OverworldTeleportIndex.DefaultLocation);
				}
			}
		}
		private Dictionary<OverworldTeleportIndex, string> OwTpIndexToName = new()
		{
			{ OverworldTeleportIndex.Cardia1, "Cardia North" },
			{ OverworldTeleportIndex.Coneria, "Coneria" },
			{ OverworldTeleportIndex.Pravoka, "Pravoka" },
			{ OverworldTeleportIndex.Elfland, "Elfland" },
			{ OverworldTeleportIndex.Melmond, "Melmond" },
			{ OverworldTeleportIndex.CrescentLake, "Crescent Lake" },
			{ OverworldTeleportIndex.Gaia, "Gaia" },
			{ OverworldTeleportIndex.Onrac, "Onrac" },
			{ OverworldTeleportIndex.Lefein, "Lefein" },
			{ OverworldTeleportIndex.ConeriaCastle1, "Coneria Castle" },
			{ OverworldTeleportIndex.ElflandCastle, "Elfland Castle" },
			{ OverworldTeleportIndex.NorthwestCastle, "Northwest Castle" },
			{ OverworldTeleportIndex.CastleOrdeals1, "Castle of Ordeals" },
			{ OverworldTeleportIndex.TempleOfFiends1, "Temple Of Fiends" },
			{ OverworldTeleportIndex.EarthCave1, "Earth Cave" },
			{ OverworldTeleportIndex.GurguVolcano1, "Gurgu Volcano" },
			{ OverworldTeleportIndex.IceCave1, "Ice Cave" },
			{ OverworldTeleportIndex.Cardia2, "Cardia Grass" },
			{ OverworldTeleportIndex.BahamutCave1, "Bahamut Cave" },
			{ OverworldTeleportIndex.Waterfall, "Waterfall" },
			{ OverworldTeleportIndex.DwarfCave, "Dwarf Cave" },
			{ OverworldTeleportIndex.MatoyasCave, "Matoya's Cave" },
			{ OverworldTeleportIndex.SardasCave, "Sarda's Cave" },
			{ OverworldTeleportIndex.MarshCave1, "Marsh Cave" },
			{ OverworldTeleportIndex.MirageTower1, "Mirage Tower" },
			{ OverworldTeleportIndex.TitansTunnelEast, "Titan's Tunnel East" },
			{ OverworldTeleportIndex.TitansTunnelWest, "Titan's Tunnel West" },
			{ OverworldTeleportIndex.Cardia4, "Cardia Marsh" },
			{ OverworldTeleportIndex.Cardia5, "Cardia Small" },
			{ OverworldTeleportIndex.Cardia6, "Cardia Forest" },
			{ OverworldTeleportIndex.DefaultLocation, "Default Location" },
			{ OverworldTeleportIndex.Unused, "Unused" },
			{ OverworldTeleportIndex.None, "None" },
		};
	}
}