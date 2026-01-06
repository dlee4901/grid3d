public class DamageEffect : Effect
{
    public int Target;
    public int Amount;
    
    // public override void Apply(Grid2D grid)
    // {
    //     var entity = grid.GetEntity(Target);
    //     if (entity != null)
    //     {
    //         entity.Health -= Amount;
    //     }
    // }
}