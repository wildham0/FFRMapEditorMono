using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Content;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.VisualBasic.Devices;

namespace FFRMapEditorMono.FFR
{

	public class EditorMode
	{
		public virtual bool UnsavedChanges { get => false; }
		public EditorMode() { }
		public virtual void LoadContent(SpriteBatch spriteBatch, ContentManager content, MouseState mouse, GraphicsDevice graphicsDevice, FileManager fileManager, SpriteFont font, TaskManager tasks) { }
		public virtual void Update(FileManager fileManager, TaskManager editorTasks, KeyboardState keyboard, Point windowSize, bool windowResized) { }
		public virtual void Draw(MouseState mouse, Point windowSize) { }
	}

	public class FFREditorMode: EditorMode
	{
		private CanvasFFR Overworld;
		private WindowsManager WindowsManager;
		private ToolsMenu ToolsMenu;
		private InfoWindow InfoWindow;
		private CurrentTool CurrentTool;
		private List<OptionPicker> OptionPickers;
		private List<WarningWindow> WarningWindows;
		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;
		private List<string> unplacedTiles;
		private InfoBox InfoBox;
		private SpriteFont font;
		private MouseState mouse;
		public override bool UnsavedChanges { get => Overworld.UnsavedChanges; }
		public override void LoadContent(SpriteBatch _spriteBatch, ContentManager content, MouseState _mouse, GraphicsDevice _graphicsDevice, FileManager fileManager, SpriteFont _font, TaskManager tasks)
		{
			graphicsDevice = _graphicsDevice;
			spriteBatch = _spriteBatch;
			unplacedTiles = new();
			font = _font;
			mouse = _mouse;

			//Load Textures
			Dictionary<string, Texture2D> textures = new()
			{
				{ "tileset", content.Load<Texture2D>("maptiles") },
				{ "domainsicons", content.Load<Texture2D>("domainsicons") },
				{ "docksicons", content.Load<Texture2D>("docksicons") },
				{ "mapobjects", content.Load<Texture2D>("mapobjects") },
				{ "smartbrushes", content.Load<Texture2D>("smarthbrushes") },
				{ "templates", content.Load<Texture2D>("templatesicons") },
				{ "selectors16", content.Load<Texture2D>("cursorsmerge16") },
				{ "selectors32", content.Load<Texture2D>("cursorsmerge32") },
				{ "infowindow", content.Load<Texture2D>("windowborder") },
				{ "tools", content.Load<Texture2D>("tools") },
				{ "placingicons", content.Load<Texture2D>("placingicons") },
				{ "resizeicons", content.Load<Texture2D>("ffmq_resizetool") },
				{ "button", content.Load<Texture2D>("button") },
				{ "pixel", content.Load<Texture2D>("pixel") },
			};

			var windowSize = fileManager.Settings.GetResolution();

			// Create overworld
			Overworld = new(textures, font, graphicsDevice, spriteBatch, fileManager, mouse);

			// Create menus
			ToolsMenu = new(textures["tools"], textures["selectors32"], font);
			InfoWindow = new(textures["infowindow"], font, textures["button"], windowSize);
			CurrentTool = new();
			InfoBox = new(font);

			OptionPickers = new()
			{
				new DomainPicker(textures["domainsicons"], textures["selectors32"], font),
				new TemplatePicker(textures["templates"], textures["selectors32"], font),
				new DockPicker(textures["docksicons"], textures["selectors32"], textures["placingicons"], font, Overworld),
				new MapObjectPicker(textures["mapobjects"], textures["selectors16"], textures["placingicons"], font, Overworld),
				new BrushPicker(textures["smartbrushes"], textures["selectors16"], font),
				new TilePicker(textures["tileset"], textures["selectors16"], textures["placingicons"], font, Overworld)
			};

			// Warning Windows
			WarningWindows = new()
			{
				new ExitWarningWindow(textures["infowindow"], font, textures["button"], windowSize),
				new SaveWarningWindow(textures["infowindow"], font, textures["button"], windowSize),
				new NewMapWarningWindow(textures["infowindow"], font, textures["button"], windowSize),
				new LoadMapWarningWindow(textures["infowindow"], font, textures["button"], windowSize)
			};

			// Create Windows manager
			WindowsManager = new(ToolsMenu, InfoWindow);
			WindowsManager.RegisterWarningWindows(WarningWindows);
			WindowsManager.RegisterOptionPickers(OptionPickers);
		}

		public override void Update(FileManager fileManager, TaskManager editorTasks, KeyboardState keyboard, Point windowSize, bool windowResized)
		{
			if (windowResized)
			{
				InfoWindow.UpdatePosition(windowSize);
			}

			// Select Options
			editorTasks.AddRange(ToolsMenu.PickOption(mouse));
			editorTasks.AddRange(InfoWindow.ProcessInput(mouse));

			foreach (var picker in OptionPickers)
			{
				editorTasks.AddRange(picker.PickOption(mouse));
			}

			foreach (var warning in WarningWindows)
			{
				editorTasks.AddRange(warning.ProcessInput(mouse));
			}

			// Update Selected Tool
			CurrentTool.Update(editorTasks);
			CurrentTool.GetTemplate(editorTasks, OptionPickers.OfType<TemplatePicker>().First().CurrentTemplate);
			ToolsMenu.UpdateBrushSize(CurrentTool.BrushSize + 1);
			ToolsMenu.UpdateGridSize(Overworld.GridSize);

			// Interact with the map
			if (WindowsManager.CanInteractWithMap(mouse.Position))
			{
				Overworld.UpdateTile(graphicsDevice, spriteBatch, mouse, windowSize, CurrentTool);
				Overworld.PlaceTemplate(graphicsDevice, spriteBatch, mouse, windowSize, CurrentTool);
				Overworld.UpdateDomain(mouse, CurrentTool);
				Overworld.UpdateDock(mouse, CurrentTool);
				Overworld.UpdateMapObject(mouse, CurrentTool);
				Overworld.Selector(graphicsDevice, spriteBatch, mouse, windowSize, CurrentTool);
				Overworld.Copy(CurrentTool, editorTasks);
				Overworld.Paste(graphicsDevice, spriteBatch, mouse, windowSize, CurrentTool, editorTasks);
				editorTasks.AddRange(Overworld.GetTile(mouse));
			}

			// Update selected tiles
			foreach (var picker in OptionPickers)
			{
				picker.ProcessTasks(editorTasks);
			}

			// Update Windows
			WindowsManager.ProcessTasks(editorTasks);

			// Update File management
			WarningWindows.OfType<SaveWarningWindow>().First().ProcessTasks(Overworld, windowSize, editorTasks);

			fileManager.ProcessTasks(Overworld, null, editorTasks);
			Overworld.ProcessTasks(editorTasks);
			InfoBox.ProcessTasks(editorTasks);

			// Process Middle Mouse Button
			if (keyboard.LCTRL)
			{
				CurrentTool.UpdateBrushScroll(mouse);
			}
			else
			{
				Overworld.UpdateZoom(mouse, windowSize);
			}

			if (mouse.MiddleDown)
			{
				Overworld.UpdateView(mouse.GetHoldOffset(), windowSize);
			}
			else if (mouse.MiddleClick)
			{
				mouse.SetHoldOffset();
			}
		}

		public override void Draw(MouseState mouse, Point windowSize)
		{
			// Draw base canvas
			graphicsDevice.Clear(Color.DarkSlateGray);

			// Draw map+overlays
			Overworld.Draw(spriteBatch, WindowsManager, CurrentTool, mouse, windowSize);

			// Draw menus
			ToolsMenu.Draw(spriteBatch, font, mouse.Position);
			InfoWindow.Draw(spriteBatch);

			foreach (var picker in OptionPickers)
			{
				picker.Draw(spriteBatch, font, mouse.Position);
			}

			InfoBox.Draw(spriteBatch, windowSize);

			foreach (var warning in WarningWindows)
			{
				warning.Draw(spriteBatch);
			}
		}
	}
}