public static class RegistryManager
{
    public static void Init()
    {
        TypeRegistry.Register(typeof(SkillSelection));
        TypeRegistry.Register(typeof(SingleSkillSelection));
        TypeRegistry.Register(typeof(AreaSkillSelection));
        TypeRegistry.Register(typeof(FillSkillSelection));
    }
}