using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace AccountModelsLib
{
    public class FileJson
{
	public FileJson()
	{
	}

	public static List<T> ConvertEmbaddedFileToList<T>(string path)
	{
		return FileJson.ReadJsonToList<T>(FileJson.ReadFileEmbeded(path));
	}

	public static string ReadFileEmbeded(string Path)
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		string resourceName = Path;
		string script = "";
		using (Stream stream = assembly.GetManifestResourceStream(resourceName))
		{
			using (StreamReader reader = new StreamReader(stream))
			{
				script = string.Concat(script, reader.ReadToEnd());
			}
		}
		return script;
	}

	public static T ReadJsonToClass<T>(string json)
	where T : class
	{
		T t;
		try
		{
			t = JsonConvert.DeserializeObject<T>(json);
		}
		catch (Exception exception)
		{
			t = default(T);
		}
		return t;
	}

	public static List<T> ReadJsonToList<T>(string json)
	{
		List<T> ts;
		try
		{
			if (!string.IsNullOrEmpty(json))
			{
				ts = JsonConvert.DeserializeObject<List<T>>(json);
			}
			else
			{
				ts = null;
			}
		}
		catch (Exception exception)
		{
			ts = null;
		}
		return ts;
	}

	public static void WriteFileJson(object file, string Path)
	{
		string RootInvoices = JsonConvert.SerializeObject(file);
		string jsonFormatted = JToken.Parse(RootInvoices).ToString(Formatting.Indented, Array.Empty<JsonConverter>());
		File.WriteAllText(string.Concat(Path, ".json"), jsonFormatted);
	}
}
}