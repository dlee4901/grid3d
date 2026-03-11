using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public static class ConfigLoader
{
    public static List<T> LoadFolder<T>(string folderPath, bool searchSubfolders=false, bool polymorphic=false) where T : INameId
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException(folderPath);

        // var objects = new List<T>();
        // var filePaths = searchSubfolders ? Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories) : Directory.GetFiles(folderPath, "*.json");
        // foreach (var filePath in filePaths)
        //     objects.Add(LoadFile<T>(filePath, polymorphic));
        // return objects;
        var filePaths = searchSubfolders ? Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories) : Directory.GetFiles(folderPath, "*.json");
        return filePaths.Select(filePath => LoadFile<T>(filePath, polymorphic)).ToList();
    }
    
    private static T LoadFile<T>(string filePath, bool polymorphic=false) where T : INameId
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
        
        var settings = !polymorphic ? null : new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new NewtonsoftJsonConverter<T>() },
            Formatting = Formatting.Indented
        };

        var json = File.ReadAllText(filePath);
        var obj = JsonConvert.DeserializeObject<T>(json, settings) ?? throw new Exception($"Failed to parse file {filePath}");
        if (obj.Id == "") throw new Exception($"Missing ID in file {filePath}");
        
        return obj;
    }
}