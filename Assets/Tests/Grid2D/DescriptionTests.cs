using NUnit.Framework;
using System.Reflection;
public class DescriptionTests
{
    [Test]
    public void AllAbilityAndTraitDescriptionTokensResolve()
    {
        var issues = new System.Collections.Generic.List<string>();
        foreach (var type in new[] { typeof(AbilityConfigs), typeof(TraitConfigs), typeof(TerrainConfigs), typeof(StatusConfigs) })
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            if (field.GetValue(null) is EffectConfig config) issues.AddRange(Descriptions.Validate(config));
        Assert.IsEmpty(issues, string.Join("\n", issues));
    }

    [Test]
    public void EveryTerrainType_HasATerrainConfigWithDescription()
    {
        foreach (TerrainType terrain in System.Enum.GetValues(typeof(TerrainType)))
        {
            var field = typeof(TerrainConfigs).GetField(terrain.ToString(), BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, $"TerrainConfigs.{terrain} missing");
            var config = (TerrainConfig)field.GetValue(null);
            Assert.AreEqual(terrain.ToString(), config.Id);
            Assert.IsFalse(string.IsNullOrEmpty(config.Description), $"{terrain} has no description");
        }
    }
}
