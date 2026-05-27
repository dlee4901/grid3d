using UnityEngine.InputSystem;

public class PlayerInputController : LoggableBehaviour
{
    private PlayerInputContext _ctx;
    private IPlayerInputState _current;

    public IPlayerInputState State => _current;

    public void Init(GridManager gridManager, GridInput input, TurnExecutor executor)
    {
        _ctx = new PlayerInputContext
        {
            Controller = this,
            Input = input,
            GridManager = gridManager,
            Executor = executor
        };

        _current = new IdleState(_ctx);
        _current.OnEnter();

        input.OnTileClicked       += clicked => _current.OnTileClicked(clicked);
        input.OnSelectionChanged  += ctx     => _current.OnGridSelectionChanged(ctx);
    }

    private void Update()
    {
        if (_current == null) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            _current.OnCancel();
    }

    // ---- Skill icon intent handlers (called by UnitInfo from SkillIcon events)
    public void OnSkillPreview(Skill skill, QueryContext ctx)
    {
        if (_current is AimingSkillState) return;
        _ctx.GridManager.ShowSkillPreview(skill, ctx);
    }

    public void OnSkillCancelPreview()
    {
        if (_current is AimingSkillState) return;
        _ctx.GridManager.ClearSkillPreview();
    }

    public void OnSkillActivate(Skill skill, QueryContext source)
        => _current.OnSkillActivate(skill, source);

    // ---- State transitions
    public void TransitionTo(IPlayerInputState next)
    {
        if (ReferenceEquals(_current, next)) return;
        Log($"State: {_current?.GetType().Name} → {next.GetType().Name}");
        _current?.OnExit();
        _current = next;
        _current.OnEnter();
    }
}
