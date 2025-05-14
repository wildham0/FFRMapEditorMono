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
		private WindowsManager WindowsManager;
		private ToolsMenu ToolsMenu;
		private InfoWindow InfoWindow;
		private CurrentTool CurrentTool;
		private MouseState mouse;
		private InfoBox InfoBox;
		private List<OptionPicker> OptionPickers;
		private List<WarningWindow> WarningWindows;
		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;
		private List<string> unplacedTiles;
		private SpriteFont font;
		//private TaskManager taskManager;
		public override bool UnsavedChanges { get => Canvas.UnsavedChanges; }
		public override void LoadContent(SpriteBatch _spriteBatch, ContentManager content, MouseState _mouse, GraphicsDevice _graphicsDevice, FileManager fileManager, SpriteFont _font, TaskManager tasks)
		{
			graphicsDevice = _graphicsDevice;
			spriteBatch = _spriteBatch;
			unplacedTiles = new();
			font = _font;
			//taskManager = new();
			mouse = _mouse;

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
			Canvas = new(textures, font, graphicsDevice, spriteBatch, fileManager, mouse);

			// Create menus
			ToolsMenu = new(textures["tools"], textures["selectors32"], font);
			InfoWindow = new(textures["infowindow"], font, textures["button"], windowSize);
			CurrentTool = new();
			InfoBox = new(font);

			OptionPickers = new()
			{
				//new DomainPicker(domainGroupsIcons, selectors32, font),
				//new TemplatePicker(templatesIcons, selectors32, font),
				//new DockPicker(docksIcons, selectors32, placingIcons, font, Overworld),
				//new MapObjectPicker(mapobjectsIcons, selectors16, placingIcons, font, Overworld),
				//new BrushPicker(brushesTexture, selectors16, font),
				new TilePickerMQ(textures["selectors16"], textures["placingicons"], font, Canvas),
				new MapResize(textures["resizeicons"], textures["selectors32"], font)
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
			ToolsMenu.UpdateBrushSize(CurrentTool.BrushSize + 1);
			ToolsMenu.UpdateGridSize(Canvas.GridSize);

			// Interact with the map
			
			if (WindowsManager.CanInteractWithMap(mouse.Position))
			{
				Canvas.UpdateTile(graphicsDevice, spriteBatch, mouse, windowSize, CurrentTool);
				Canvas.Selector(graphicsDevice, spriteBatch, mouse, windowSize, CurrentTool);
				Canvas.Copy(CurrentTool, editorTasks);
				Canvas.Paste(graphicsDevice, spriteBatch, mouse, windowSize, CurrentTool, editorTasks);
				editorTasks.AddRange(Canvas.GetTile(mouse));
			}

			// Update selected tiles
			foreach (var picker in OptionPickers)
			{
				picker.ProcessTasks(editorTasks);
			}

			// Update Windows
			WindowsManager.ProcessTasks(editorTasks);

			// Update File management
			//WarningWindows.OfType<SaveWarningWindow>().First().ProcessTasks(Overworld, OptionPickers.OfType<TilePicker>().First().GetUnplacedTiles(), windowSize, editorTasks);

			fileManager.ProcessTasks(null, Canvas, editorTasks);
			Canvas.ProcessTasks(editorTasks);
			InfoBox.ProcessTasks(editorTasks);

			// Process Middle Mouse Button
			if (keyboard.LCTRL)
			{
				CurrentTool.UpdateBrushScroll(mouse);
			}
			else
			{
				Canvas.UpdateZoom(mouse, windowSize);
			}

			if (mouse.MiddleDown)
			{
				Canvas.UpdateView(mouse.GetHoldOffset(), windowSize);
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
			Canvas.Draw(spriteBatch, WindowsManager, CurrentTool, mouse, windowSize);

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