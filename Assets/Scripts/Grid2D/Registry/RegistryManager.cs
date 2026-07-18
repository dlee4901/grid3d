using System.Collections.Generic;

public static class RegistryManager
{
    public static void Register<T>(List<T> list) where T : INameId
    {
        IdRegistry<T>.Register(list);
    }
}
