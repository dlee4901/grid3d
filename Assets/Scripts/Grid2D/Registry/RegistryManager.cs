using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class RegistryManager
{
    static RegistryManager()
    {
        // RegisterDerivedTypes<SkillSelection>();
        // RegisterDerivedTypes<IEntityComponent>();
    }
    
    public static void Register<T>(List<T> list) where T : INameId
    {
        IdRegistry<T>.Register(list);
    }
    
    public static void LoadAndRegister<TConfig>(string loadPath, bool searchSubfolders=false, bool polymorphic=false) where TConfig : INameId
    {
        var configs = ConfigLoader.LoadFolder<TConfig>(loadPath, searchSubfolders, polymorphic);
        IdRegistry<TConfig>.Register(configs);
    }
    
    // public static void LoadInstancesAndRegister<TConfig, TInstance>(string loadPath, Func<TConfig, TInstance> instanceCreation, bool searchSubfolders=false) where TConfig : INameId where TInstance : INameId
    // {
    //     var configs = ConfigLoader.LoadFolder<TConfig>(loadPath, searchSubfolders);
    //     var instances = configs.Select(instanceCreation).ToList();
    //     IdRegistry<TInstance>.Register(instances);
    // }
    
    // For Newtonsoft.JSON polymorphic serialization
    public static void RegisterDerivedTypes<T>()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
        foreach (var type in types)
        {
            TypeRegistry.Register(type);
            Debug.Log(type);
        }
            
    }
}