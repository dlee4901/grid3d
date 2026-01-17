using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonHandler
{
    public static void SaveData<T>(T data) where T : INameId
    {
        var filePath = Path.Combine(Application.persistentDataPath, data.Id + ".json");
        var jsonData = JsonConvert.SerializeObject(data); //JsonUtility.ToJson(data);
        Debug.Log(filePath);
        File.WriteAllText(filePath, jsonData);
    }

    public static T LoadData<T>(string id) where T : INameId
    {
        string filePath = Path.Combine(Application.persistentDataPath, id + ".json");
        if (!File.Exists(filePath)) throw new FileNotFoundException("Save file not found", filePath);
        Debug.Log(filePath);
        var jsonData = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(jsonData);
    }
}