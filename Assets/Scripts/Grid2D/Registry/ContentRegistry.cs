using System;
using System.Reflection;

public static class ContentRegistry
{
    public static void RegisterAll()
    {
        RegisterStaticFields<EntityConfig>(typeof(UnitConfigs));
        RegisterStaticFields<AbilityConfig>(typeof(AbilityConfigs));
        RegisterMaps();
    }

    private static void RegisterStaticFields<T>(Type container) where T : INameId
    {
        foreach (var field in container.GetFields(BindingFlags.Public | BindingFlags.Static))
            if (typeof(T).IsAssignableFrom(field.FieldType) && field.GetValue(null) is T config)
                IdRegistry<T>.Register(config);
    }

    private static void RegisterMaps()
    {
        foreach (var field in typeof(MapConfigs).GetFields(BindingFlags.Public | BindingFlags.Static))
            if (field.GetValue(null) is GridDefinition map)
            {
                map.Bake();
                IdRegistry<GridDefinition>.Register(map);
            }
    }
}
