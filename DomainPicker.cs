using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono
{
	public class DomainPicker : OptionPicker
	{
		public DomainPicker(Texture2D _window, Texture2D _selector, SpriteFont _font)
		{
			optionsWindow = _window;
			optionSelector = _selector;
			optionFont = _font;

			Position = new Vector2(64, 0);
			zoom = 1.0f;
			optionsRows = 2;
			optionsColumns = 8;
			optionsSize = 32;

			options = Domainsname.Select((d, i) => (d,
				new List<EditorTask>() {
					new EditorTask() { Type = EditorTasks.DomainsUpdate, Value = i } },
				new List<EditorTask>() {
					new EditorTask() { Type = EditorTasks.DomainsUpdate, Value = i } }
				)).ToList();

			Show = false;
			lastSelection = 0x00;
			SetOptionTextLength();
			showPlaced = false;
		}
		private List<string> Domainsname = new()
		{
			"Upper Onrac Group",
			"Northern Edge Group",
			"Ordeals Group",
			"Lower Onrac Group",
			"Cardia Group",
			"Mirage Desert Group",
			"Lefein Group",
			"Oops All Imps Group",
			"Temple of Fiends Group",
			"Elfland Group",
			"Melmond Group",
			"Coneria Group",
			"Pravoka Group",
			"Ice Cave Group",
			"Asp-Wolf Group",
			"Crescent Lake Group"
		};

	}
}