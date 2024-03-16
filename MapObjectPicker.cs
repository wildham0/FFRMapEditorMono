using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono
{
	public class MapObjectPicker : OptionPicker
	{
		public MapObjectPicker(Texture2D _window, Texture2D _selector, Texture2D _placingicons, SpriteFont _font)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionIcons = _placingicons;
			optionFont = _font;

			Position = new Vector2(64, 0);
			zoom = 2.0f;
			optionsRows = 1;
			optionsColumns = 5;
			optionsSize = 16;

			options = mapObjectsNames.Select((o, i) => (o.Item1,
				new List<EditorTask>() {
					new EditorTask() { Type = EditorTasks.MapObjectsUpdate, Value = i },
					new EditorTask() { Type = EditorTasks.WindowsClose, Value = 10 } },
				new List<EditorTask>() {
					new EditorTask() { Type = EditorTasks.MapObjectsUpdate, Value = i } }
				)).ToList();

			Show = false;
			lastSelection = 0x00;

			placedOptions = new();
			unplacedOptions = new();
			SetOptionTextLength();
			showPlaced = true;
		}

		private List<(string, EditorTasks, EditorTasks)> mapObjectsNames = new()
		{
			("Starting Position", EditorTasks.None, EditorTasks.None),
			("Bridge", EditorTasks.None, EditorTasks.None),
			("Canal", EditorTasks.None, EditorTasks.None),
			("Ship (Unused)", EditorTasks.None, EditorTasks.None),
			("Airship", EditorTasks.None, EditorTasks.None),
		};
		public void UpdatePlaced(Overworld overworld)
		{
			if (!overworld.UpdatePlacedMapObjects)
			{
				return;
			}
			
			placedOptions = overworld.GetPlacedMapObjects().Select(o => (int)o).ToList();
			overworld.UpdatePlacedMapObjects = false;
		}
	}
}