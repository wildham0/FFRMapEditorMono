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
		private ToolsMenu toolsMenu;
		private InfoWindow infoWindow;

		private List<WarningWindow> warningWindows;
		private List<OptionPicker> optionPickers;

		private Point windowSize = new(1200, 800);
		private SpriteFont font;
		private FileManager fileManager;
		private CurrentTool currentTool;
		private MouseState mouse;
		private List<EditorTask> editorTasks;
		private WindowsManager windowsManager;
		
		private bool LastActiveState = true;
		private List<string> unplacedTiles;
		private int suspendKeyboard = 0;
		private int timeToBackup;
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
			timeToBackup = fileManager.Settings.GetBackupDelay();
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
			overworld = new(tileSetTexture, font, domainGroupsIcons, docksIcons, mapobjectsIcons, GraphicsDevice, _spriteBatch, fileManager.Settings.GetUndoDepth());

			// Create menus
			toolsMenu = new(toolsTexture, selectors32, font);
			infoWindow = new(infowindowtexture, font, buttonTexture, windowSize);
			currentTool = new();

			optionPickers = new()
			{
				new DomainPicker(domainGroupsIcons, selectors32, font),
				new TemplatePicker(templatesIcons, selectors32, font),
				new DockPicker(docksIcons, selectors32, placingIcons, font, overworld),
				new MapObjectPicker(mapobjectsIcons, selectors16, placingIcons, font, overworld),
				new BrushPicker(brushesTexture, selectors16, font),
				new TilePicker(tileSetTexture, selectors16, placingIcons, font, overworld)
			};

			// Warning Windows
			warningWindows = new()
			{
				new ExitWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new SaveWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new NewMapWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new LoadMapWarningWindow(infowindowtexture, font, buttonTexture, windowSize)
			};

			// Create Windows manager
			windowsManager = new(toolsMenu, infoWindow);
			windowsManager.RegisterWarningWindows(warningWindows);
			windowsManager.RegisterOptionPickers(optionPickers);

		}
		protected override void Update(GameTime gameTime)
		{
			if (editorTasks.Contains(new EditorTask() { Type = EditorTasks.ResetBackupCounter }))
			{
				editorTasks.RemoveAll(t => t.Type == EditorTasks.ResetBackupCounter);
				timeToBackup = fileManager.Settings.GetBackupDelay();
			}
			else if (timeToBackup > 0)
			{
				timeToBackup--;
			}
			else
			{
				editorTasks.Add(new EditorTask() { Type = EditorTasks.SaveBackupMap });
				timeToBackup = fileManager.Settings.GetBackupDelay();
			}
			
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

			if (suspendKeyboard > 0)
			{
				suspendKeyboard--;
			}
			else if(Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.Z))
			{
				if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
				{
					overworld.Redo();
					suspendKeyboard = 20;
				}
				else
				{
					overworld.Undo();
					suspendKeyboard = 20;
				}
			}

			// Remove None tasks
			editorTasks.RemoveAll(t => t.Type == EditorTasks.None);

			// Update Mouse Statuts
			mouse.Update();

			// Select Options
			editorTasks.AddRange(toolsMenu.PickOption(mouse));
			editorTasks.AddRange(infoWindow.ProcessInput(mouse));

			foreach (var picker in optionPickers)
			{
				editorTasks.AddRange(picker.PickOption(mouse));
			}

			foreach (var warning in warningWindows)
			{
				editorTasks.AddRange(warning.ProcessInput(mouse));
			}

			// Update Selected Tool
			currentTool.Update(editorTasks);
			currentTool.GetTemplate(editorTasks, optionPickers.OfType<TemplatePicker>().First().CurrentTemplate);
			toolsMenu.UpdateBrushSize(currentTool.BrushSize + 1);

			// Interact with the map
			if (windowsManager.CanInteractWithMap(mouse.Position))
			{
				overworld.UpdateTile(GraphicsDevice, _spriteBatch, mouse, windowSize, currentTool);
				overworld.PlaceTemplate(GraphicsDevice, _spriteBatch, mouse, windowSize, currentTool);
				overworld.UpdateDomain(mouse, currentTool);
				overworld.UpdateDock(mouse, currentTool);
				overworld.UpdateMapObject(mouse, currentTool);
				editorTasks.AddRange(overworld.GetTile(mouse));
			}

			// Update selected tiles
			foreach (var picker in optionPickers)
			{
				picker.ProcessTasks(editorTasks);
			}

			// Update Windows
			windowsManager.ProcessTasks(editorTasks, overworld);

			// Update File management
			warningWindows.OfType<SaveWarningWindow>().First().ProcessTasks(overworld, optionPickers.OfType<TilePicker>().First().GetUnplacedTiles(), windowSize, editorTasks);

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
			toolsMenu.Draw(_spriteBatch, font, mouse.Position);
			infoWindow.Draw(_spriteBatch);

			foreach (var picker in optionPickers)
			{
				picker.Draw(_spriteBatch, font, mouse.Position);
			}

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