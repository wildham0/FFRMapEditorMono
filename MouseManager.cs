using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FFRMapEditorMono
{
	public class MouseState
	{
		public bool LeftClick;
		public bool RightClick;
		public bool MiddleClick;
		public bool LeftDown;
		public bool RightDown;
		public bool MiddleDown;
		public bool ScrollUp;
		public bool ScrollDown;
		public Vector2 Position;
		private ButtonState lastLeftState;
		private ButtonState lastRightState;
		private ButtonState lastMiddleState;
		private int wheelRoll;
		private Vector2 holdOffset;
		public MouseState()
		{
			Update();
		}
		public void Update()
		{
			ButtonState currentLeftState = Mouse.GetState().LeftButton;
			ButtonState currentRightState = Mouse.GetState().RightButton;
			ButtonState currentMiddleState = Mouse.GetState().MiddleButton;

			Position = new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);

			LeftClick = (lastLeftState == ButtonState.Released && currentLeftState == ButtonState.Pressed);
			RightClick = (lastRightState == ButtonState.Released && currentRightState == ButtonState.Pressed);
			MiddleClick = (lastMiddleState == ButtonState.Released && currentMiddleState == ButtonState.Pressed);

			LeftDown = (lastLeftState == ButtonState.Pressed && currentLeftState == ButtonState.Pressed);
			RightDown = (lastRightState == ButtonState.Pressed && currentRightState == ButtonState.Pressed);
			MiddleDown = (lastMiddleState == ButtonState.Pressed && currentMiddleState == ButtonState.Pressed);

			lastLeftState = currentLeftState;
			lastRightState = currentRightState;
			lastMiddleState = currentMiddleState;

			if (Mouse.GetState().ScrollWheelValue > wheelRoll)
			{
				wheelRoll = Mouse.GetState().ScrollWheelValue;
				ScrollUp = true;
				ScrollDown = false;
			}
			else if (Mouse.GetState().ScrollWheelValue < wheelRoll)
			{
				wheelRoll = Mouse.GetState().ScrollWheelValue;
				ScrollUp = false;
				ScrollDown = true;
			}
			else
			{
				ScrollUp = false;
				ScrollDown = false;
			}
		}
		public Vector2 GetHoldOffset()
		{
			Vector2 offset = new Vector2(holdOffset.X - Position.X, holdOffset.Y - Position.Y);
			holdOffset = Position;

			return offset;
		}
		public void SetHoldOffset()
		{
			holdOffset = Position;
		}
	}
}