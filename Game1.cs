using FFRMapEditorMono.FFR;
using Microsoft.VisualBasic.Devices;
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
		// Set mode to switch between FFR or FFMQ editing mode
		private GameMode gameMode = GameMode.FFR;
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private Canvas overworld;
		private ToolsMenu toolsMenu;
		private InfoWindow infoWindow;

		private EditorMode editorMode;

		private List<WarningWindow> warningWindows;
		private List<OptionPicker> optionPickers;

		private Point windowSize = new(1200, 800);
		private SpriteFont font;
		private FileManager fileManager;
		private CurrentTool currentTool;
		private MouseState mouse;
		private KeyboardState keyboard;
		private TaskManager editorTasks;
		private WindowsManager windowsManager;
		
		private bool LastActiveState = true;
		private List<string> unplacedTiles;
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
			
			// Init Tasks list
			editorTasks = new();

			// Init Inputs
			mouse = new();
			keyboard = new(editorTasks);

			unplacedTiles = new();

			// Init Editor Mode
			editorMode = gameMode == GameMode.FFR ? new FFR.FFREditorMode() : new MysticQuest.MQEditorMode();

			// Create File Manager
			fileManager = new FileManager(gameMode);
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
			font = Content.Load<SpriteFont>("File");

			editorMode.LoadContent(_spriteBatch, Content, mouse, GraphicsDevice, fileManager, font, editorTasks);
		}
		protected override void Update(GameTime gameTime)
		{
			if (editorTasks.Pop(EditorTasks.ResetBackupCounter))
			{
				timeToBackup = fileManager.Settings.GetBackupDelay();
			}
			else if (timeToBackup > 0)
			{
				timeToBackup--;
			}
			else
			{
				editorTasks.Add(new EditorTask(EditorTasks.SaveBackupMap));
				timeToBackup = fileManager.Settings.GetBackupDelay();
			}
			
			if (editorTasks.Pop(EditorTasks.ExitProgram))
			{
				if (editorMode.UnsavedChanges)
				{
					editorTasks.Add(EditorTasks.ExitWarningOpen);
					editorTasks.Prune(EditorTasks.ExitProgram);
				}
				else
				{
					Exit();
				}
			}

			if (editorTasks.Pop(EditorTasks.ExitProgramHard))
			{
				Exit();
			}

			// Check if window is active to reduce misclicks
			if (!IsActive)
			{
				return;
			}
	
			// Remove None tasks
			editorTasks.Prune(EditorTasks.None);

			// Update Input Statuts
			mouse.Update();
			keyboard.Update();

			// Check if window was resized
			bool windowResized = WindowResized();

			// Process Editor
			editorMode.Update(fileManager, editorTasks, keyboard, windowSize, windowResized);

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
			editorMode.Draw(mouse, windowSize);

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