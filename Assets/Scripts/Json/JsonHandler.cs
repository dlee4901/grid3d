using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JsonHandler
{
    public void SaveData<T>(T data)
    {
        string jsonData = JsonConvert.SerializeObject(data); //JsonUtility.ToJson(data);
        string filePath = Application.persistentDataPath + "/" + typeof(T).Name + ".json";
        Debug.Log(filePath);
        File.WriteAllText(filePath, jsonData);
    }

    public T LoadData<T>()
    {
        string filePath = Application.persistentDataPath + "/" + typeof(T).Name + ".json";
        Debug.Log(filePath);
        string jsonData = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(jsonData); //JsonUtility.FromJson<T>(jsonData);
    }
}