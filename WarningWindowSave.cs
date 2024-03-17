using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System;

namespace FFRMapEditorMono
{
	public class SaveWarningWindow : WarningWindow
	{
		public SaveWarningWindow(Texture2D _windowtexture, SpriteFont _font, Texture2D _buttonTexture, Point resolution)
		{
			Type = WarningType.SaveValidation;

			windowTexture = _windowtexture;
			font = _font;

			Show = false;
			zoom = 3.0f;
			windowWidth = 28 * 8;
			windowDimensions = new(windowWidth, 100);
			UpdatePosition(resolution);
			buttons = new()
			{
				new(font, "Save map file anyway", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.SaveNoWarning, Value = (int)saveMode }, new EditorTask() { Type = EditorTasks.SaveWarningClose, Value = 10 } } ),
				new(font, "Return to editor", _buttonTexture, new() { new EditorTask() { Type = EditorTasks.SaveWarningClose, Value = 10 } })
			};
		}
		public void ProcessTasks(Overworld overworld, List<string> missingTiles, Point resolution, List<EditorTask> tasks)
		{
			var validtasks = tasks.ToList();
			foreach (var task in validtasks)
			{
				if (task.Type == EditorTasks.SetSavingMode)
				{
					saveMode = (SavingMode)task.Value;
					tasks.Remove(task);
				}
				else if (task.Type == EditorTasks.SaveWarningUpdate)
				{

					saveMode = (SavingMode)task.Value;

					var validationresult = overworld.ValidateObjects();

					warningText = "*** Warning *** \n\n";

					if (!validationresult.defaultdock)
					{
						warningText += "Default dock hasn't been placed.\n\n";
					}

					var missingobjects = validationresult.missingmapobjects;
					if (missingobjects.Any())
					{
						string currentline = "";
						//int

						currentline += "Missing Map Objects: ";
						for (int i = 0; i < missingobjects.Count; i++)
						{
							string objectname = Enum.GetName(missingobjects[i]);

							if (currentline.Length + objectname.Length + 2 > 60)
							{
								warningText += currentline + "\n";
								currentline = "  " + objectname;
							}
							else
							{
								currentline += objectname;
							}

							if (i < (missingobjects.Count - 1))
							{
								currentline += ", ";
							}
							else
							{
								currentline += ".";
							}
						}

						warningText += currentline + "\n\n";
					}
					//var missingtiles = validationresult.missingmapobjects;
					if (missingTiles.Any())
					{
						string currentline = "";

						currentline += "Missing Required Tiles: ";
						for (int i = 0; i < missingTiles.Count; i++)
						{
							string tilename = missingTiles[i];

							if (currentline.Length + tilename.Length + 2 > 60)
							{
								warningText += currentline + "\n";
								currentline = "  " + tilename;
							}
							else
							{
								currentline += tilename;
							}

							if (i < (missingTiles.Count - 1))
							{
								currentline += ", ";
							}
							else
							{
								currentline += ".";
							}
						}

						warningText += currentline + "\n\n";
					}

					var windowHeight = warningText.Count(c => c == '\n') * 8 + 32;
					windowDimensions = new Vector2(windowWidth, windowHeight);
					UpdatePosition(resolution);
					tasks.Remove(task);
				}
			}
		}
	}
}