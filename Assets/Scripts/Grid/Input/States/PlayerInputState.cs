public abstract class PlayerInputState
{
    protected readonly PlayerInputContext Ctx;
    protected PlayerInputState(PlayerInputContext ctx) { Ctx = ctx; }

    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public virtual void OnPress(GridSource pressed) {}
    public virtual void OnDrag(GridSource dragged) {}
    public virtual void OnPositionSelected(GridSource clicked) {}
    public virtual void OnCancel() {}
    public virtual void OnAbilityActivate(Ability ability, GridSource source) {}

    protected PlayerInputState Select(Ability ability, GridSource source) => new AbilitySelectedState(Ctx, ability, source);
}
