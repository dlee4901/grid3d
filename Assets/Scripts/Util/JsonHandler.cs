using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonHandler
{
    private static string PathFor(string id) => Path.Combine(Application.persistentDataPath, id + ".json");

    public static void SaveData<T>(T data) where T : INameId
    {
        var filePath = PathFor(data.Id);
        var jsonData = JsonConvert.SerializeObject(data);
        File.WriteAllText(filePath, jsonData);
    }

    public static bool Exists(string id) => File.Exists(PathFor(id));

    public static void Delete(string id) => File.Delete(PathFor(id));

    public static IEnumerable<string> Ids()
    {
        foreach (var file in Directory.GetFiles(Application.persistentDataPath, "*.json"))
            yield return Path.GetFileNameWithoutExtension(file);
    }

    public static T LoadData<T>(string id) where T : INameId
    {
        var filePath = PathFor(id);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Save file not found", filePath);
        var jsonData = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(jsonData);
    }
}
