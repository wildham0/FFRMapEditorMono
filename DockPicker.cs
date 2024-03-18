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
		private Overworld overworld;
		public DockPicker(Texture2D _window, Texture2D _selector, Texture2D _placedicons, SpriteFont _font, Overworld _overworld)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionIcons = _placedicons;
			optionFont = _font;
			overworld = _overworld;

			Position = new Vector2(64, 0);
			zoom = 1.0f;
			optionsRows = 2;
			optionsColumns = 8;
			optionsSize = 32;

			options = Enum.GetNames<OverworldTeleportIndex>().Select((d, i) => (Regex.Replace(d, "([A-Z0-9]+)", " $1").Trim(),
				new List<EditorTask>() {
					new EditorTask() { Type = EditorTasks.DocksUpdate, Value = i } },
				new List<EditorTask>() {
					new EditorTask() { Type = EditorTasks.DocksRemove, Value = i } }
				)).ToList();

			Show = false;
			lastSelection = 0x00;
			placedOptions = new();
			unplacedOptions = new();
			SetOptionTextLength();
			showPlaced = true;
		}
		public override void ProcessTasks(List<EditorTask> tasks)
		{
			var validtasks = tasks.ToList();

			foreach (var task in validtasks)
			{
				if (task.Type == EditorTasks.UpdatePlacedDocksOverlay)
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
					tasks.Remove(task);
				}
			}
		}
	}
}