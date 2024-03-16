using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;

namespace FFRMapEditorMono
{
	public class DockPicker : OptionPicker
	{
		public DockPicker(Texture2D _window, Texture2D _selector, Texture2D _placedicons, SpriteFont _font)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionIcons = _placedicons;
			optionFont = _font;

			Position = new Vector2(64, 0);
			zoom = 1.0f;
			optionsRows = 2;
			optionsColumns = 8;
			optionsSize = 32;

			options = Enum.GetNames<OverworldTeleportIndex>().Select((d, i) => (Regex.Replace(d, "([A-Z0-9]+)", " $1").Trim(),
				new List<EditorTask>() {
					new EditorTask() { Type = EditorTasks.DocksUpdate, Value = i } },
				new List<EditorTask>() {
					new EditorTask() { Type = EditorTasks.DocksUpdate, Value = i } }
				)).ToList();

			Show = false;
			lastSelection = 0x00;
			placedOptions = new();
			unplacedOptions = new();
			SetOptionTextLength();
			showPlaced = true;
		}
		public void UpdatePlaced(Overworld overworld)
		{
			if (!overworld.UpdatePlacedDocks)
			{
				return;
			}

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

			overworld.UpdatePlacedDocks = false;
		}
	}
}