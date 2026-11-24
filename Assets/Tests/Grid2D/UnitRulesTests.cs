using System.Reflection;
using NUnit.Framework;

public class UnitRulesTests
{
    [Test]
    public void EveryUnit_HasAtMostMaxAbilities()
    {
        foreach (var field in typeof(UnitConfigs).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not EntityConfig config || config.Abilities == null) continue;
            Assert.LessOrEqual(config.Abilities.Count, EntityConfig.MaxAbilities, config.Id);
        }
    }
}
