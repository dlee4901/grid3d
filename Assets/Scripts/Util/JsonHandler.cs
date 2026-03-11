using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonHandler
{
    public static void SaveData<T>(T data, bool polymorphic=false) where T : INameId
    {
        var settings = !polymorphic ? null : new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new NewtonsoftJsonConverter<T>() },
            Formatting = Formatting.Indented
        };
        var filePath = Path.Combine(Application.persistentDataPath, data.Id + ".json");
        var jsonData = JsonConvert.SerializeObject(data, settings); //JsonUtility.ToJson(data);
        File.WriteAllText(filePath, jsonData);
    }

    public static T LoadData<T>(string id, bool polymorphic=false) where T : INameId
    {
        var settings = !polymorphic ? null : new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new NewtonsoftJsonConverter<T>() },
            Formatting = Formatting.Indented
        };
        var filePath = Path.Combine(Application.persistentDataPath, id + ".json");
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Save file not found", filePath);
        var jsonData = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(jsonData, settings);
    }
}