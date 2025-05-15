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

	public class FFREditorMode: EditorMode
	{
		private CanvasFFR Overworld;
		private List<string> unplacedTiles;
		public override bool UnsavedChanges { get => Overworld.UnsavedChanges; }
		public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MouseState mouse, KeyboardState keyboard, FileManager fileManager, TaskManager tasks)
		{
			GraphicsDevice = graphicsDevice;
			SpriteBatch = spriteBatch;

			FileManager = fileManager;
			TaskManager = tasks;
			Mouse = mouse;
			Keyboard = keyboard;
			
			SpriteFont font = content.Load<SpriteFont>("File");

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

			var windowSize = FileManager.Settings.GetResolution();
			unplacedTiles = new();

			// Create overworld
			Overworld = new(textures, font, GraphicsDevice, SpriteBatch, FileManager, TaskManager, Mouse, Keyboard);

			// Create menus
			ToolsMenu = new FFR.ToolsMenu(textures["tools"], textures["selectors32"], font, SpriteBatch, TaskManager, Mouse);
			InfoWindow = new(textures["infowindow"], font, textures["button"], windowSize);
			CurrentTool = new();
			InfoBox = new(font);

			OptionPickers = new()
			{
				new DomainPicker(textures["domainsicons"], textures["selectors32"], font, SpriteBatch, TaskManager, Mouse),
				new TemplatePicker(textures["templates"], textures["selectors32"], font, SpriteBatch, TaskManager, Mouse),
				new DockPicker(textures["docksicons"], textures["selectors32"], textures["placingicons"], Overworld, font, SpriteBatch, TaskManager, Mouse),
				new MapObjectPicker(textures["mapobjects"], textures["selectors16"], textures["placingicons"], Overworld, font, SpriteBatch, TaskManager, Mouse),
				new BrushPicker(textures["smartbrushes"], textures["selectors16"], font, SpriteBatch, TaskManager, Mouse),
				new TilePicker(textures["tileset"], textures["selectors16"], textures["placingicons"], Overworld, font, SpriteBatch, TaskManager, Mouse)
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
			editorTasks.AddRange(ToolsMenu.PickOption());
			editorTasks.AddRange(InfoWindow.ProcessInput(Mouse));

			foreach (var picker in OptionPickers)
			{
				editorTasks.AddRange(picker.PickOption());
			}

			foreach (var warning in WarningWindows)
			{
				editorTasks.AddRange(warning.ProcessInput(Mouse));
			}

			// Update Selected Tool
			CurrentTool.Update(editorTasks);
			CurrentTool.GetTemplate(editorTasks, OptionPickers.OfType<TemplatePicker>().First().CurrentTemplate);
			ToolsMenu.Update(Overworld, CurrentTool);

			// Interact with the map
			if (WindowsManager.CanInteractWithMap(Mouse.Position))
			{
				Overworld.UpdateTile(windowSize, CurrentTool);
				Overworld.PlaceTemplate(windowSize, CurrentTool);
				Overworld.UpdateDomain(CurrentTool);
				Overworld.UpdateDock(CurrentTool);
				Overworld.UpdateMapObject(CurrentTool);
				Overworld.Selector(windowSize, CurrentTool);
				Overworld.Copy(CurrentTool);
				Overworld.Paste(windowSize, CurrentTool);
				editorTasks.AddRange(Overworld.GetTile());
			}

			// Update selected tiles
			foreach (var picker in OptionPickers)
			{
				picker.ProcessTasks();
			}

			// Update Windows
			WindowsManager.ProcessTasks(editorTasks);

			// Update File management
			WarningWindows.OfType<SaveWarningWindow>().First().ProcessTasks(Overworld, windowSize, editorTasks);

			fileManager.ProcessTasks(Overworld, null, editorTasks);
			Overworld.ProcessTasks();
			InfoBox.ProcessTasks(editorTasks);

			// Process Middle Mouse Button
			if (keyboard.LCTRL)
			{
				CurrentTool.UpdateBrushScroll(Mouse);
			}
			else
			{
				Overworld.UpdateZoom(windowSize);
			}

			if (Mouse.MiddleDown)
			{
				Overworld.UpdateView(Mouse.GetHoldOffset(), windowSize);
			}
			else if (Mouse.MiddleClick)
			{
				Mouse.SetHoldOffset();
			}
		}

		public override void Draw(Point windowSize)
		{
			// Draw base canvas
			GraphicsDevice.Clear(Color.DarkSlateGray);

			// Draw map+overlays
			Overworld.Draw(WindowsManager, CurrentTool, windowSize);

			// Draw menus
			ToolsMenu.Draw();
			InfoWindow.Draw(SpriteBatch);

			foreach (var picker in OptionPickers)
			{
				picker.Draw();
			}

			InfoBox.Draw(SpriteBatch, windowSize);

			foreach (var warning in WarningWindows)
			{
				warning.Draw(SpriteBatch);
			}
		}
	}
}