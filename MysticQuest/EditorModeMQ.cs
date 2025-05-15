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
using FFRMapEditorMono.FFR;

namespace FFRMapEditorMono.MysticQuest
{
	public class MQEditorMode: EditorMode
	{
		private CanvasMQ Canvas;
		public override bool UnsavedChanges { get => Canvas.UnsavedChanges; }
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
				{ "tilesets", content.Load<Texture2D>("maptiles") },
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
			Canvas = new(textures, font, GraphicsDevice, SpriteBatch, FileManager, TaskManager, Mouse, Keyboard);

			// Create menus
			ToolsMenu = new MysticQuest.ToolsMenu(textures["tools"], textures["selectors32"], font, SpriteBatch, TaskManager, Mouse);
			InfoWindow = new(textures["infowindow"], font, textures["button"], windowSize);
			CurrentTool = new();
			InfoBox = new(font);

			OptionPickers = new()
			{
				new TilePickerMQ(textures["selectors16"], textures["placingicons"], Canvas, font, SpriteBatch, TaskManager, Mouse),
				new MapResize(textures["resizeicons"], textures["selectors32"], font, SpriteBatch, TaskManager, Mouse)
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

			tasks.Add(new EditorTask(EditorTasks.InfoBoxUpdateLayer, 0));
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
			ToolsMenu.Update(Canvas, CurrentTool);

			// Interact with the map
			if (WindowsManager.CanInteractWithMap(Mouse.Position))
			{
				Canvas.UpdateTile(windowSize, CurrentTool);
				Canvas.Selector(windowSize, CurrentTool);
				Canvas.Copy(CurrentTool);
				Canvas.Paste(windowSize, CurrentTool);
				editorTasks.AddRange(Canvas.GetTile());
			}

			// Update selected tiles
			foreach (var picker in OptionPickers)
			{
				picker.ProcessTasks();
			}

			// Update Windows
			WindowsManager.ProcessTasks(editorTasks);

			// Update File management
			//WarningWindows.OfType<SaveWarningWindow>().First().ProcessTasks(Overworld, OptionPickers.OfType<TilePicker>().First().GetUnplacedTiles(), windowSize, editorTasks);

			fileManager.ProcessTasks(null, Canvas, editorTasks);
			Canvas.ProcessTasks();
			InfoBox.ProcessTasks(editorTasks);

			// Process Middle Mouse Button
			if (keyboard.LCTRL)
			{
				CurrentTool.UpdateBrushScroll(Mouse);
			}
			else
			{
				Canvas.UpdateZoom(windowSize);
			}

			if (Mouse.MiddleDown)
			{
				Canvas.UpdateView(Mouse.GetHoldOffset(), windowSize);
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
			Canvas.Draw(WindowsManager, CurrentTool, windowSize);

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