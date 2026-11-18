public abstract class PlayerInputState
{
    protected readonly PlayerInputContext Ctx;
    protected PlayerInputState(PlayerInputContext ctx) { Ctx = ctx; }

    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public virtual void OnPress(GridSource pressed) {}
    public virtual void OnPositionSelected(GridSource clicked) {}
    public virtual void OnHover(GridSource? hovered) {}
    public virtual void OnCancel() {}
    public virtual void OnAbilityActivate(Ability ability, GridSource source) {}
    public virtual void OnAbilityPreview(Ability ability, GridSource source) {}
    public virtual void OnAbilityCancelPreview() {}
}
