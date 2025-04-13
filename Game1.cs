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

		private EditorMode editorMode;

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

			// Init Editor Mode
			editorMode = new FFREditorMode();

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
			font = Content.Load<SpriteFont>("File");

			editorMode.LoadContent(_spriteBatch, Content, GraphicsDevice, fileManager, font);
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
				if (editorMode.UnsavedChanges)
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


			if (suspendKeyboard > 0)
			{
				suspendKeyboard--;
			}
	
			// Remove None tasks
			editorTasks.RemoveAll(t => t.Type == EditorTasks.None);

			// Update Mouse Statuts
			mouse.Update();

			// Check if window was resized
			bool windowResized = WindowResized();

			// Process Editor
			editorMode.Update(fileManager, mouse, editorTasks, ref suspendKeyboard, windowSize, windowResized);

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