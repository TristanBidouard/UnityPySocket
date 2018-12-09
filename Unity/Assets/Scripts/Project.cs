using Matt.Json;
using System;
using System.IO;
using UnityEngine;

namespace Jake
{
	using StringExtensions;
	using Threading;

	public static class Project
	{
		public static string GetProjectPath()
		{
			var path = Directory.GetCurrentDirectory();
			if (path.EndsWith("Python"))
			{
				path = path.RemoveFromEnd(@"\Python\Python".Length);
			}
			else if (path.EndsWith("Build"))
			{
				path = path.RemoveFromEnd(@"\Unity\Build".Length);
			}
			else if (path.EndsWith("Unity"))
			{
				path = path.RemoveFromEnd(@"\Unity".Length);
			}

			return path;
		}

		public static JSONObject LoadJson(string path)
		{
			if (path[0] != '\\')
			{
				path = "\\" + path;
			}

			path = GetProjectPath() + path;

			try
			{
				var text = File.ReadAllText(path);
				var json = new JSONObject(text);

				return json;
			}
			catch (Exception e)
			{
				if (!Dispatcher.IsMainThread)
				{
					Debug.LogError(e);
				}

				throw e;
			}
		}
	}
}
