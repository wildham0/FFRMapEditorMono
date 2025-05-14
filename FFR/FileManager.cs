using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFRMapEditorMono.FFR
{
    class FileManagerFFR : FileManager
    {
		public OwMapExchangeData MapData { get; set; }

		public FileManagerFFR(GameMode mode) : base(mode)
		{
			MapData = new();
			FileFilter = "FFR Json Overworld Map (*.json)|*.json|FFHackster Overworld Map (*.ffm)|*.ffm|All files (*.*)|*.*";
		}
		public override void ResetMapData()
		{
			MapData = new();
		}
		public override void LoadMapData(string json)
		{
			MapData = JsonSerializer.Deserialize<OwMapExchangeData>(json);
		}

		public override string GetJsonString()
		{
			return JsonSerializer.Serialize<OwMapExchangeData>(MapData, new JsonSerializerOptions { WriteIndented = true }); ;
		}
		public override WriteFormat GetFileFormat(int index)
		{
			var filename = LoadedMapName.Split('.');

			if (index == 0)
			{
				if (filename.Last() == "ffm")
				{
					return WriteFormat.Binary;
				}
				else
				{
					return WriteFormat.Json;
				}
			}
			else if (index == 2)
			{
				return WriteFormat.Binary;
			}
			else
			{
				return WriteFormat.Json;
			}
		}
		public override void WriteFile(Stream file, WriteFormat format)
		{
			if (format == WriteFormat.Binary)
			{
				using var stream = new BinaryWriter(file);
				stream.Write(MapData.DecodeMap());
			}
			else
			{
				string serializedOwData = JsonSerializer.Serialize<OwMapExchangeData>(MapData, new JsonSerializerOptions { WriteIndented = true });
				using var stream = new StreamWriter(file);
				stream.Write(serializedOwData);
			}
		}
		public override void ReadFile(Stream file, WriteFormat format)
		{
			if (format == WriteFormat.Binary)
			{
				using var stream = new BinaryReader(file);
				var dataarray = stream.ReadBytes(0x10000);

				MapData = new();
				MapData.EncodeMapFromBytes(dataarray);
			}
			else
			{
				using var stream = new StreamReader(file);
				var jsonstring = stream.ReadToEnd();

				MapData = JsonSerializer.Deserialize<OwMapExchangeData>(jsonstring);
			}
		}
	}
}
