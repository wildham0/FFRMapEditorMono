using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;


namespace FFRMapEditorMono
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private Overworld overworld;
		private TilePicker tilePicker;
		private BrushPicker brushPicker;
		private DomainPicker domainsPicker;
		private DockPicker docksPicker;
		private MapObjectPicker mapObjectsPicker;
		private TemplatePicker templatesPicker;
		private ToolsMenu toolsMenu;
		private InfoWindow infoWindow;

		private List<WarningWindow> warningWindows;

		private Point windowSize = new(1200, 800);
		private SpriteFont font;
		private FileManager fileManager;
		private CurrentTool currentTool;
		private MouseState mouse;
		private List<EditorTask> editorTasks;
		private WindowsManager windowsManager;
		private bool LastActiveState = true;
		private List<string> unplacedTiles;
		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			_graphics.PreferredBackBufferWidth = windowSize.X;
			_graphics.PreferredBackBufferHeight = windowSize.Y;
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			Window.Title = "FFR Map Editor";
			Window.AllowUserResizing = true;

			// Init Mouse
			mouse = new();

			// Init Tasks list
			editorTasks = new();

			unplacedTiles = new();

			// Create File Manager
			fileManager = new FileManager();
			fileManager.LoadSettings();
			windowSize = fileManager.Settings.GetResolution();
			_graphics.PreferredBackBufferWidth = windowSize.X;
			_graphics.PreferredBackBufferHeight = windowSize.Y;
			_graphics.ApplyChanges();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			//Load Textures
			Texture2D tileSetTexture = Content.Load<Texture2D>("maptiles");
			Texture2D domainGroupsIcons = Content.Load<Texture2D>("domainsicons");
			Texture2D docksIcons = Content.Load<Texture2D>("docksicons");
			Texture2D mapobjectsIcons = Content.Load<Texture2D>("mapobjects");
			Texture2D selectors16 = Content.Load<Texture2D>("cursorsmerge16");
			Texture2D selectors32 = Content.Load<Texture2D>("cursorsmerge32");
			Texture2D infowindowtexture = Content.Load<Texture2D>("windowborder");
			Texture2D toolsTexture = Content.Load<Texture2D>("tools");
			Texture2D brushesTexture = Content.Load<Texture2D>("smarthbrushes");
			Texture2D placingIcons = Content.Load<Texture2D>("placingicons");
			Texture2D templatesIcons = Content.Load<Texture2D>("templatesicons");
			Texture2D buttonTexture = Content.Load<Texture2D>("button");

			font = Content.Load<SpriteFont>("File");

			// Create overworld
			overworld = new(tileSetTexture, font, domainGroupsIcons, docksIcons, mapobjectsIcons, GraphicsDevice, _spriteBatch);

			// Create menus
			toolsMenu = new(toolsTexture, selectors32, font);
			domainsPicker = new(domainGroupsIcons, selectors32, font);
			docksPicker = new(docksIcons, selectors32, placingIcons, font);
			brushPicker = new(brushesTexture, selectors16, font);
			templatesPicker = new(templatesIcons, selectors32, font);
			infoWindow = new(infowindowtexture, font, buttonTexture, windowSize);
			mapObjectsPicker = new(mapobjectsIcons, selectors16, placingIcons, font);
			currentTool = new();
			tilePicker = new(tileSetTexture, selectors16, placingIcons, font);

			// Warning Windows
			warningWindows = new()
			{
				new ExitWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new SaveWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new NewMapWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new LoadMapWarningWindow(infowindowtexture, font, buttonTexture, windowSize)
			};

			// Create Windows manager
			windowsManager = new(toolsMenu, tilePicker, brushPicker, domainsPicker, docksPicker, mapObjectsPicker, templatesPicker, infoWindow);
			windowsManager.RegisterWarningWindows(warningWindows);

		}
		protected override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Escape) || editorTasks.Contains(new EditorTask() { Type = EditorTasks.ExitProgram }))
			{
				if (overworld.UnsavedChanges)
				{
					editorTasks.Add(new EditorTask() { Type = EditorTasks.ExitWarningOpen });
					editorTasks.RemoveAll(t => t.Type == EditorTasks.ExitProgram);
				}
				else
				{
					Exit();
				}
			}

			if (editorTasks.Contains(new EditorTask() { Type = EditorTasks.ExitProgramHard }))
			{
				Exit();
			}

			// Check if window is active to reduce misclicks
			if (!IsActive)
			{
				return;
			}

			if (WindowResized())
			{
				infoWindow.UpdatePosition(windowSize);
			}

			// Remove None tasks
			editorTasks.RemoveAll(t => t.Type == EditorTasks.None);

			// Update Mouse Statuts
			mouse.Update();

			// Select Options
			editorTasks.AddRange(tilePicker.PickOption(mouse));
			editorTasks.AddRange(toolsMenu.PickOption(mouse));
			editorTasks.AddRange(domainsPicker.PickOption(mouse));
			editorTasks.AddRange(docksPicker.PickOption(mouse));
			editorTasks.AddRange(mapObjectsPicker.PickOption(mouse));
			editorTasks.AddRange(brushPicker.PickOption(mouse));
			editorTasks.AddRange(templatesPicker.PickOption(mouse));
			editorTasks.AddRange(infoWindow.ProcessInput(mouse));

			foreach (var warning in warningWindows)
			{
				editorTasks.AddRange(warning.ProcessInput(mouse));
			}

			// Update Selected Tool
			currentTool.Update(editorTasks);
			currentTool.GetTemplate(editorTasks, templatesPicker.CurrentTemplate);
			toolsMenu.UpdateBrushSize(currentTool.BrushSize + 1);

			// Interact with the map
			if (windowsManager.CanInteractWithMap(mouse.Position))
			{
				overworld.UpdateTile(GraphicsDevice, _spriteBatch, mouse, windowSize, currentTool);
				overworld.PlaceTemplate(GraphicsDevice, _spriteBatch, mouse, windowSize, currentTool);
				overworld.UpdateDomain(mouse, currentTool);
				overworld.UpdateDock(mouse, currentTool);
				overworld.UpdateMapObject(mouse, currentTool);
				mapObjectsPicker.UpdatePlaced(overworld);
				docksPicker.UpdatePlaced(overworld);
				unplacedTiles = tilePicker.UpdatePlaced(overworld);
				editorTasks.AddRange(overworld.GetTile(mouse));
			}

			// Update selected tiles
			tilePicker.ProcessTasks(editorTasks);
			brushPicker.ProcessTasks(editorTasks);

			// Update Windows
			windowsManager.ProcessTasks(editorTasks, overworld);

			// Update File management
			warningWindows.OfType<SaveWarningWindow>().First().ProcessTasks(overworld, unplacedTiles, windowSize, editorTasks);

			fileManager.ProcessTasks(overworld, unplacedTiles, editorTasks);
			overworld.ProcessTasks(fileManager.OverworldData, editorTasks);

			// Process Middle Mouse Button
			if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
			{
				currentTool.UpdateBrushScroll(mouse);
			}
			else
			{
				overworld.UpdateZoom(mouse, windowSize);
			}

			if (mouse.MiddleDown)
			{
				overworld.UpdateView(mouse.GetHoldOffset(), windowSize);
			}
			else if (mouse.MiddleClick)
			{
				mouse.SetHoldOffset();
			}

			// Update Window title
			if (fileManager.FilenameUpdated)
			{
				Window.Title = "FFR Map Editor" + fileManager.GetFileName();
				fileManager.FilenameUpdated = false;
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			// Draw base canvas
			GraphicsDevice.Clear(Color.DarkSlateGray);

			// Draw map+overlays
			overworld.Draw(_spriteBatch, windowsManager, currentTool, mouse);
			
			// Draw menus
			tilePicker.Draw(_spriteBatch, font, mouse.Position);
			brushPicker.Draw(_spriteBatch, font, mouse.Position);
			domainsPicker.Draw(_spriteBatch, font, mouse.Position);
			docksPicker.Draw(_spriteBatch, font, mouse.Position);
			mapObjectsPicker.Draw(_spriteBatch, font, mouse.Position);
			templatesPicker.Draw(_spriteBatch, font, mouse.Position);
			toolsMenu.Draw(_spriteBatch, font, mouse.Position);
			infoWindow.Draw(_spriteBatch);

			foreach (var warning in warningWindows)
			{
				warning.Draw(_spriteBatch);
			}

			base.Draw(gameTime);
		}

		private bool WindowResized()
		{
			if ((_graphics.PreferredBackBufferWidth != _graphics.GraphicsDevice.Viewport.Width) ||
				(_graphics.PreferredBackBufferHeight != _graphics.GraphicsDevice.Viewport.Height))
			{
				_graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.Viewport.Width;
				_graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.Viewport.Height;
				_graphics.ApplyChanges();
				windowSize = new Point(_graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override void OnExiting(Object sender, EventArgs args)
		{
			// Save Setting
			fileManager.Settings.SetResolution(windowSize);
			fileManager.SaveSettings();

			base.OnExiting(sender, args);
		}
	}
}