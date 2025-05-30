﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FFRMapEditorMono.FFR
{
	public enum Templates
	{ 
		ConeriaCastle = 0,
		TempleOfFiend,
		Pravoka,
		Elfland,
		NorthWestCastle,
		Melmond,
		CrescentLake,
		Volcano,
		Ordeals,
		Onrac,
		Gaia,
		MirageTower,
		Lefein,
		Caravan,
		DockVertical,
		DockHorizontal
	}
	public class TemplatePicker : OptionPicker
	{
		public byte[,] CurrentTemplate { get => templates[lastSelection]; }
		public TemplatePicker(Texture2D _window, Texture2D _selector, SpriteFont _font, SpriteBatch _spriteBatch, TaskManager _tasks, MouseState _mouse) : base(_font, _spriteBatch, _tasks, _mouse)
		{
			optionsWindow = _window;
			optionSelector = _selector;

			Position = new Vector2(64, 0);
			zoom = 1.0f;
			optionsRows = 2;
			optionsColumns = 8;
			optionsSize = 32;
			
			Show = false;
			lastSelection = 0x00;
			options = templatesNames.Select((t, i) => (t,
				new List<EditorTask>() {
					new EditorTask(EditorTasks.TemplatesUpdate, i),
					new EditorTask(EditorTasks.WindowsClose, 10) },
				new List<EditorTask>() {
					new EditorTask(EditorTasks.TemplatesUpdate, i) }
				)).ToList();

			placedOptions = new();
			unplacedOptions = new();
			showPlaced = false;
			SetOptionTextLength();
		}

		private List<string> templatesNames = new()
		{
			"Coneria Castle",
			"Temple of Fiends",
			"Pravoka",
			"Elfland",
			"Northwest Castle",
			"Melmond",
			"Crescent Lake",
			"Volcano",
			"Castle of Ordeals",
			"Onrac",
			"Gaia",
			"Mirage Tower",
			"Lefein",
			"Caravan",
			"Dock (Vertical)",
			"Dock (Horizontal",
		};
		private List<byte[,]> templates = new()
		{
			new byte[,]
			{
				{ 0x00, 0x00, 0x09, 0x0A, 0x00, 0x00 },
				{ 0x00, 0x2C, 0x19, 0x1A, 0x2E, 0x00 },
				{ 0x3B, 0x3C, 0x01, 0x02, 0x3E, 0x3F },
				{ 0x4B, 0x49, 0x3D, 0x3D, 0x49, 0x4F },
				{ 0x5B, 0x49, 0x3D, 0x3D, 0x49, 0x5F },
				{ 0x6B, 0x49, 0x3D, 0x3D, 0x49, 0x6F },
				{ 0x7B, 0x7D, 0x3D, 0x3D, 0x7E, 0x7F },
			},
			new byte[,]
			{
				{ 0x00, 0x47, 0x48, 0x00 },
				{ 0x56, 0x57, 0x58, 0x59 },
			},
			new byte[,]
			{
				{ 0x00, 0x2C, 0x2D, 0x2E, 0x00 },
				{ 0x3B, 0x3C, 0x4A, 0x3E, 0x3F },
				{ 0x4B, 0x4A, 0x3D, 0x4A, 0x4F },
				{ 0x5C, 0x7D, 0x3D, 0x7E, 0x5E },
			},
			new byte[,]
			{
				{ 0x00, 0x0B, 0x0C, 0x00 },
				{ 0x4C, 0x1B, 0x1C, 0x04C},
				{ 0x4C, 0x00, 0x00, 0x04C},
			},
			new byte[,]
			{
				{ 0x09, 0x0A },
				{ 0x29, 0x2A },
			},
			new byte[,]
			{
				{ 0x4D, 0x00 },
				{ 0x4D, 0x4D },
			},
			new byte[,]
			{
				{ 0x00, 0x2C, 0x2D, 0x2E, 0x00 },
				{ 0x3B, 0x3C, 0x3D, 0x3E, 0x3F },
				{ 0x4B, 0x3D, 0x4E, 0x4E, 0x4F },
				{ 0x5B, 0x4E, 0x3D, 0x4E, 0x5F },
				{ 0x6B, 0x4E, 0x3D, 0x3D, 0x6F },
				{ 0x7B, 0x7D, 0x3D, 0x7E, 0x7F },
			},
			new byte[,]
			{
				{ 0x64, 0x65 },
				{ 0x74, 0x75 },
			},
			new byte[,]
			{
				{ 0x0B, 0x0C },
				{ 0x38, 0x39 },
			},
			new byte[,]
			{
				{ 0x5D, 0x5D },
				{ 0x5D, 0x5D },
			},
			new byte[,]
			{
				{ 0x00, 0x5A, 0x5A },
				{ 0x5A, 0x5A, 0x00 },
			},
			new byte[,]
			{
				{ 0x0D, 0x45 },
				{ 0x1D, 0x1E },
			},
			new byte[,]
			{
				{ 0x00, 0x2C, 0x2D, 0x2E, 0x00 },
				{ 0x3B, 0x3C, 0x6D, 0x3E, 0x3F },
				{ 0x4B, 0x6D, 0x6D, 0x6D, 0x4F },
				{ 0x5C, 0x7D, 0x3D, 0x7E, 0x5E },
			},
			new byte[,]
			{
				{ 0x00, 0x42, 0x43 },
				{ 0x42, 0x36, 0x53 },
				{ 0x52, 0x53, 0x00 },
			},
			new byte[,]
			{
				{ 0x77, 0x78 },
				{ 0x1F, 0x17 },
				{ 0x1F, 0x17 },
			},
			new byte[,]
			{
				{ 0x77, 0x78, 0x78 },
				{ 0x1F, 0x17, 0x17 },
			},
		};
	}
}