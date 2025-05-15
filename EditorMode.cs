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
		protected GraphicsDevice GraphicsDevice;
		protected SpriteBatch SpriteBatch;

		protected FileManager FileManager;
		protected TaskManager TaskManager;
		protected MouseState Mouse;
		protected KeyboardState Keyboard;

		protected WindowsManager WindowsManager;
		protected OptionPicker ToolsMenu;
		protected InfoWindow InfoWindow;
		protected CurrentTool CurrentTool;
		protected List<OptionPicker> OptionPickers;
		protected List<WarningWindow> WarningWindows;
		protected InfoBox InfoBox;
		public virtual bool UnsavedChanges { get => false; }
		public EditorMode() { }
		public virtual void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MouseState mouse, KeyboardState keyboard, FileManager fileManager, TaskManager tasks)	{ }
		public virtual void Update(FileManager fileManager, TaskManager editorTasks, KeyboardState keyboard, Point windowSize, bool windowResized) { }
		public virtual void Draw(Point windowSize) { }
	}
}