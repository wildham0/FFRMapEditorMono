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
	public class Selector
	{
		public Vector2 InitialPosition { get; set; }
		public Vector2 FinalPosition { get; set; }
		public bool ShowSelection => Enable && (GetRectangle().Height != 0 && GetRectangle().Width != 0);
		public bool Enable { get; set; }

		public Selector()
		{
			InitialPosition = new Vector2(0, 0);
			FinalPosition = new Vector2(0, 0);
			Enable = false;
		}
		public Rectangle GetRectangle()
		{
			int widthbonus = (InitialPosition.X < FinalPosition.X) ? 1 : 0;
			int heightbonus = (InitialPosition.Y < FinalPosition.Y) ? 1 : 0;

			int topx = (int)Math.Min(InitialPosition.X, FinalPosition.X);
			int bottomx = (int)Math.Max(InitialPosition.X, FinalPosition.X);
			int topy = (int)Math.Min(InitialPosition.Y, FinalPosition.Y); ;
			int bottomy = (int)Math.Max(InitialPosition.Y, FinalPosition.Y); ;
			int width = bottomx - topx + widthbonus;
			int height = bottomy - topy + heightbonus;

			return new Rectangle(topx, topy, width, height);
		}

	}
}