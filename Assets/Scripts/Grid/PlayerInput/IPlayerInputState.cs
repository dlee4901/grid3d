public interface IPlayerInputState
{
    void OnEnter();
    void OnExit();
    void OnPositionSelected(GridSource clicked);
    void OnHover(GridSource? hovered);
    void OnCancel();
    void OnAbilityActivate(Ability ability, GridSource source);
    void OnAbilityPreview(Ability ability, GridSource source);
    void OnAbilityCancelPreview();
}

public abstract class PlayerInputStateBase : IPlayerInputState
{
    protected readonly PlayerInputContext Ctx;
    protected PlayerInputStateBase(PlayerInputContext ctx) { Ctx = ctx; }

    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public virtual void OnPositionSelected(GridSource clicked) {}
    public virtual void OnHover(GridSource? hovered) {}
    public virtual void OnCancel() {}
    public virtual void OnAbilityActivate(Ability ability, GridSource source) {}
    public virtual void OnAbilityPreview(Ability ability, GridSource source) {}
    public virtual void OnAbilityCancelPreview() {}
}
