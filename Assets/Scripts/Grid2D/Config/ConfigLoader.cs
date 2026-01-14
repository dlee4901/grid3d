using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public static class ConfigLoader<T> where T : INameId
{
    public static List<T> LoadFolder(string folderPath) 
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException(folderPath);

        var objects = new List<T>();
        foreach (var filePath in Directory.GetFiles(folderPath, "*.json"))
        {
            objects.Add(LoadFile(filePath));
        }
        
        return objects;
    }
    
    public static T LoadFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var json = File.ReadAllText(filePath);
        var obj = JsonConvert.DeserializeObject<T>(json) ?? throw new Exception($"Failed to parse file {filePath}");
        if (obj.Id == "") throw new Exception($"Missing ID in file {filePath}");
        
        return obj;
    }
}