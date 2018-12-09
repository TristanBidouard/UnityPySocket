using Matt.Json;
using System.IO;
using UnityEngine;

namespace Jake
{
	using RL;

	public class Settings : MonoBehaviour
	{
		public string jsonFile;
		public Data[] data;

		void Start()
		{
			LoadSettings();
			WatchSettings();
		}

		void OnSettingsChanged(object source, FileSystemEventArgs e)
		{
			LoadSettings();
		}

		private void LoadSettings()
		{
			var json = Project.LoadJson(jsonFile);
			var n = Mathf.Min(data.Length, json.Count);
			for (int i = 0; i < n; ++i)
			{
				switch (json[i].type)
				{
					case JSONObject.Type.NUMBER:
						data[i].SetValue(json[i].n);
						break;
					case JSONObject.Type.STRING:
						data[i].SetValue(json[i].str);
						break;
					case JSONObject.Type.ARRAY:
						if (json[i].Count == 3 && json[i][0].type == JSONObject.Type.NUMBER)
						{
							var value = new Vector3(json[i][0].n, json[i][1].n, json[i][2].n);
							data[i].SetValue(value);
						}
						else if (json[i].Count == 4 && json[i][0].type == JSONObject.Type.NUMBER)
						{
							var value = new Quaternion(json[i][0].n, json[i][1].n, json[i][2].n, json[i][3].n);
							data[i].SetValue(value);
						}
						break;
				}
			}
		}

		private void WatchSettings()
		{
			var path		= Project.GetProjectPath() + jsonFile;
			var fileIndex	= path.LastIndexOf(@"\") + 1;
			var file		= path.Substring(fileIndex);
				path		= path.Remove(fileIndex);

			var watcher = new FileSystemWatcher(path)
			{
				NotifyFilter = NotifyFilters.LastWrite,
				Filter = file
			};
			watcher.Changed += OnSettingsChanged;
			watcher.EnableRaisingEvents = true;
		}
	}
}
