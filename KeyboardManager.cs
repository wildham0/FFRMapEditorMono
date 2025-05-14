using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using FFRMapEditorMono.FFR;

namespace FFRMapEditorMono
{
	public class KeyboardState
	{
		protected int suspendKeyboard;
		protected TaskManager taskManager;
		public bool LCTRL => Keyboard.GetState().IsKeyDown(Keys.LeftControl);
		public KeyboardState(TaskManager manager)
		{
			suspendKeyboard = 0;
			taskManager = manager;
		}
		public void Update()
		{
			if (suspendKeyboard > 0)
			{
				suspendKeyboard--;
				return;
			}
			
			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				taskManager.Add(EditorTasks.ExitProgram);
				SuspendKeyboard();
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.Z))
			{
				if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
				{
					taskManager.Add(EditorTasks.Redo);
					SuspendKeyboard();
				}
				else
				{
					taskManager.Add(EditorTasks.Undo);
					SuspendKeyboard();
				}
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.C))
			{
				taskManager.Add(EditorTasks.Copy);
				SuspendKeyboard();
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.V))
			{
				taskManager.Add(EditorTasks.PasterSetTool);
				SuspendKeyboard();
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.G))
			{
				taskManager.Add(EditorTasks.ToggleGridlines);
				SuspendKeyboard();
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.I))
			{
				taskManager.Add(EditorTasks.ToggleInfoBox);
				SuspendKeyboard();
			}

		}
		protected void SuspendKeyboard()
		{
			suspendKeyboard = 20;
		}
	}
}