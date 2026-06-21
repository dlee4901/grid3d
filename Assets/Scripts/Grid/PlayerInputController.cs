using UnityEngine.InputSystem;

public class PlayerInputController : LoggableBehaviour
{
    private PlayerInputContext _ctx;
    private IPlayerInputState _current;

    public IPlayerInputState State => _current;

    public void Init(GridManager gridManager, GridInput input, TurnExecutor executor, CommandDispatcher dispatcher)
    {
        _ctx = new PlayerInputContext
        {
            Controller = this,
            Input = input,
            GridManager = gridManager,
            Executor = executor,
            Dispatcher = dispatcher
        };

        _current = new IdleState(_ctx);
        _current.OnEnter();

        input.OnPositionSelected += clicked => _current.OnPositionSelected(clicked);
        input.OnSelectionChanged += ctx => _current.OnSelectionChanged(ctx);
        input.OnCancelClicked += () => _current.OnCancel();
    }

    private void Update()
    {
        if (_current == null) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Log("Cancel (ESC)");
            _current.OnCancel();
        }
    }

    // ---- Ability icon intent handlers (called by UnitInfo from AbilityIcon events)
    public void OnAbilityPreview(Ability ability, QueryContext ctx)
    {
        if (_current is TargetingState) return;
        Log($"AbilityPreview: {ability?.Id}");
        _ctx.GridManager.ShowAbilityPreview(ability, ctx);
    }

    public void OnAbilityCancelPreview()
    {
        if (_current is TargetingState) return;
        Log("AbilityCancelPreview");
        _ctx.GridManager.ClearAbilityPreview();
    }

    public void OnAbilityActivate(Ability ability, QueryContext source)
    {
        Log($"AbilityActivate: {ability?.Id}");
        _current.OnAbilityActivate(ability, source);
    }

    // ---- State transitions
    public void TransitionTo(IPlayerInputState next)
    {
        if (ReferenceEquals(_current, next)) return;
        Log($"State: {_current?.GetType().Name} → {next.GetType().Name}");
        _current?.OnExit();
        _current = next;
        _current.OnEnter();
    }

    // ---- Logging passthrough for the plain state classes (gated by this component's _debug)
    public void LogState(string message) => Log(message);
}
