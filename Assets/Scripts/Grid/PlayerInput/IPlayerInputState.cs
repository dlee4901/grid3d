public interface IPlayerInputState
{
    void OnEnter();
    void OnExit();
    void OnPositionSelected(QueryContext clicked);
    void OnHover(QueryContext? hovered);
    void OnCancel();
    void OnAbilityActivate(Ability ability, QueryContext ctx);
    void OnAbilityPreview(Ability ability, QueryContext ctx);
    void OnAbilityCancelPreview();
}

public abstract class PlayerInputStateBase : IPlayerInputState
{
    protected readonly PlayerInputContext Ctx;
    protected PlayerInputStateBase(PlayerInputContext ctx) { Ctx = ctx; }

    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public virtual void OnPositionSelected(QueryContext clicked) {}
    public virtual void OnHover(QueryContext? hovered) {}
    public virtual void OnCancel() {}
    public virtual void OnAbilityActivate(Ability ability, QueryContext ctx) {}
    public virtual void OnAbilityPreview(Ability ability, QueryContext ctx) {}
    public virtual void OnAbilityCancelPreview() {}
}
