using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FFRMapEditorMono.MysticQuest
{
    public class FileManagerMQ : FileManager
    {
		public JsonMap MapData { get; set; }

		public FileManagerMQ(GameMode mode) : base(mode)
		{
			MapData = new();
		}
		public override void ResetMapData()
		{
			MapData = new();
		}
		public override void LoadMapData(string json)
		{
			MapData = new JsonMap(json);
		}
		public override string GetJsonString()
		{
			return MapData.ToJson();
		}
		public override WriteFormat GetFileFormat(int index)
		{
			return WriteFormat.Json;
		}
		public override void WriteFile(Stream file, WriteFormat format)
		{
			string serializedOwData = GetJsonString();
			using var stream = new StreamWriter(file);
			stream.Write(serializedOwData);
		}
		public override void ReadFile(Stream file, WriteFormat format)
		{
			using var stream = new StreamReader(file);
			var jsonstring = stream.ReadToEnd();
			LoadMapData(jsonstring);
		}
	}
}
