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

namespace FFRMapEditorMono
{

	public class EditorMode
	{
		public virtual bool UnsavedChanges { get => false; }
		public EditorMode() { }
		public virtual void LoadContent(SpriteBatch spriteBatch, ContentManager content, GraphicsDevice graphicsDevice, FileManager fileManager, SpriteFont font)
		{ 
		
		}

		public virtual void Update(FileManager fileManager, MouseState mouse, List<EditorTask> editorTasks, ref int suspendKeyboard, Point windowSize, bool windowResized)
		{

		}

		public virtual void Draw(MouseState mouse, Point windowSize)
		{

		}
	}

	public class FFREditorMode: EditorMode
	{
		private Overworld Overworld;
		private WindowsManager WindowsManager;
		private ToolsMenu ToolsMenu;
		private InfoWindow InfoWindow;
		private CurrentTool CurrentTool;
		private List<OptionPicker> OptionPickers;
		private List<WarningWindow> WarningWindows;
		private GraphicsDevice graphicsDevice;
		private SpriteBatch spriteBatch;
		private List<string> unplacedTiles;
		private SpriteFont font;
		public override bool UnsavedChanges { get => Overworld.UnsavedChanges; }
		public override void LoadContent(SpriteBatch _spriteBatch, ContentManager content, GraphicsDevice _graphicsDevice, FileManager fileManager, SpriteFont _font)
		{
			graphicsDevice = _graphicsDevice;
			spriteBatch = _spriteBatch;
			unplacedTiles = new();
			font = _font;

			//Load Textures
			Texture2D tileSetTexture = content.Load<Texture2D>("maptiles");
			Texture2D domainGroupsIcons = content.Load<Texture2D>("domainsicons");
			Texture2D docksIcons = content.Load<Texture2D>("docksicons");
			Texture2D mapobjectsIcons = content.Load<Texture2D>("mapobjects");
			Texture2D selectors16 = content.Load<Texture2D>("cursorsmerge16");
			Texture2D selectors32 = content.Load<Texture2D>("cursorsmerge32");
			Texture2D infowindowtexture = content.Load<Texture2D>("windowborder");
			Texture2D toolsTexture = content.Load<Texture2D>("tools");
			Texture2D brushesTexture = content.Load<Texture2D>("smarthbrushes");
			Texture2D placingIcons = content.Load<Texture2D>("placingicons");
			Texture2D templatesIcons = content.Load<Texture2D>("templatesicons");
			Texture2D buttonTexture = content.Load<Texture2D>("button");

			var windowSize = fileManager.Settings.GetResolution();

			// Create overworld
			Overworld = new(tileSetTexture, font, domainGroupsIcons, docksIcons, mapobjectsIcons, graphicsDevice, spriteBatch, fileManager.Settings.GetUndoDepth());

			// Create menus
			ToolsMenu = new(toolsTexture, selectors32, font);
			InfoWindow = new(infowindowtexture, font, buttonTexture, windowSize);
			CurrentTool = new();

			OptionPickers = new()
			{
				new DomainPicker(domainGroupsIcons, selectors32, font),
				new TemplatePicker(templatesIcons, selectors32, font),
				new DockPicker(docksIcons, selectors32, placingIcons, font, Overworld),
				new MapObjectPicker(mapobjectsIcons, selectors16, placingIcons, font, Overworld),
				new BrushPicker(brushesTexture, selectors16, font),
				new TilePicker(tileSetTexture, selectors16, placingIcons, font, Overworld)
			};

			// Warning Windows
			WarningWindows = new()
			{
				new ExitWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new SaveWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new NewMapWarningWindow(infowindowtexture, font, buttonTexture, windowSize),
				new LoadMapWarningWindow(infowindowtexture, font, buttonTexture, windowSize)
			};

			// Create Windows manager
			WindowsManager = new(ToolsMenu, InfoWindow);
			WindowsManager.RegisterWarningWindows(WarningWindows);
			WindowsManager.RegisterOptionPickers(OptionPickers);
		}

		public override void Update(FileManager fileManager, MouseState mouse, List<EditorTask> editorTasks, ref int suspendKeyboard, Point windowSize, bool windowResized)
		{
			if (windowResized)
			{
				InfoWindow.UpdatePosition(windowSize);
			}


			if (suspendKeyboard == 0)
			{
				if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.Z))
				{
					if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
					{
						Overworld.Redo();
						suspendKeyboard = 20;
					}
					else
					{
						Overworld.Undo();
						suspendKeyboard = 20;
					}
				}
				else if (Keyboard.GetState().IsKeyDown(Keys.G))
				{
					editorTasks.Add(new EditorTask() { Type = EditorTasks.ToggleGridlines });
					suspendKeyboard = 20;
				}
				else if (Keyboard.GetState().IsKeyDown(Keys.C))
				{
					editorTasks.Add(new EditorTask() { Type = EditorTasks.TogglePositionIndicator });
					suspendKeyboard = 20;
				}
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
				editorTasks.AddRange(Overworld.GetTile(mouse));
			}

			// Update selected tiles
			foreach (var picker in OptionPickers)
			{
				picker.ProcessTasks(editorTasks);
			}

			// Update Windows
			WindowsManager.ProcessTasks(editorTasks, Overworld);

			// Update File management
			WarningWindows.OfType<SaveWarningWindow>().First().ProcessTasks(Overworld, OptionPickers.OfType<TilePicker>().First().GetUnplacedTiles(), windowSize, editorTasks);

			fileManager.ProcessTasks(Overworld, unplacedTiles, editorTasks);
			Overworld.ProcessTasks(fileManager.OverworldData, editorTasks);

			// Process Middle Mouse Button
			if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
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

			foreach (var warning in WarningWindows)
			{
				warning.Draw(spriteBatch);
			}
		}
	}
}