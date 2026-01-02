public class UnitFireWizard : Entity
{
    public UnitFireWizard() : base("FireWizard")
    {
        AddComponent(new Health(10));
        AddComponent(new Move(new List<TileSele));
    }
}