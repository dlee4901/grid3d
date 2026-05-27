public interface IPlayerInputState
{
    void OnEnter();
    void OnExit();
    void OnTileClicked(QueryContext clicked);
    void OnSkillActivate(Skill skill, QueryContext ctx);
    void OnGridSelectionChanged(QueryContext? ctx);
    void OnCancel();
}

public abstract class PlayerInputStateBase : IPlayerInputState
{
    protected readonly PlayerInputContext Ctx;
    protected PlayerInputStateBase(PlayerInputContext ctx) { Ctx = ctx; }

    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public virtual void OnTileClicked(QueryContext clicked) {}
    public virtual void OnSkillActivate(Skill skill, QueryContext ctx) {}
    public virtual void OnGridSelectionChanged(QueryContext? ctx) {}
    public virtual void OnCancel() {}
}
