using System;
using System.Collections.Generic;
using System.Reflection;

public static class ContentRegistry
{
    public static void RegisterAll()
    {
        var effectIds = new HashSet<string>();
        RegisterStaticFields<EntityConfig>(typeof(UnitConfigs), effectIds);
        RegisterStaticFields<AbilityConfig>(typeof(AbilityConfigs), effectIds);
        RegisterStaticFields<TraitConfig>(typeof(TraitConfigs), effectIds);
        RegisterStaticFields<StatusConfig>(typeof(StatusConfigs), effectIds);
        RegisterMaps();
    }

    private static void RegisterStaticFields<T>(Type container, HashSet<string> effectIds) where T : INameId
    {
        foreach (var field in container.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (!typeof(T).IsAssignableFrom(field.FieldType) || field.GetValue(null) is not T config) continue;
            if (config is EffectConfig && !effectIds.Add(config.Id))
                GridLog.Error($"Effect id '{config.Id}' is used by more than one ability, trait or status");
            IdRegistry<T>.Register(config);
        }
    }

    private static void RegisterMaps()
    {
        foreach (var field in typeof(MapConfigs).GetFields(BindingFlags.Public | BindingFlags.Static))
            if (field.GetValue(null) is GridDefinition map)
            {
                map.Init();
                IdRegistry<GridDefinition>.Register(map);
            }
    }
}
