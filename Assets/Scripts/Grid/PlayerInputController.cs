using UnityEngine.InputSystem;

public class PlayerInputController : LoggableBehaviour
{
    private PlayerInputContext _ctx;
    private IPlayerInputState _current;

    public IPlayerInputState State => _current;

    public void Init(GridInput input, CommandDispatcher dispatcher, IGridRenderer renderer, IReadOnlyGridState grid)
    {
        _ctx = new PlayerInputContext
        {
            Controller = this,
            Input = input,
            Dispatcher = dispatcher,
            Renderer = renderer,
            Grid = grid
        };

        _current = new IdleState(_ctx);
        _current.OnEnter();

        input.OnPositionSelected += clicked => _current.OnPositionSelected(clicked);
        input.OnCancelClicked += () => _current.OnCancel();
        input.OnHoverChanged += hovered => _current.OnHover(hovered);
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

    public void OnAbilityPreview(Ability ability, QueryContext ctx)
        => _current.OnAbilityPreview(ability, ctx);

    public void OnAbilityCancelPreview()
        => _current.OnAbilityCancelPreview();

    public void OnAbilityActivate(Ability ability, QueryContext source)
        => _current.OnAbilityActivate(ability, source);

    private string _activeAbilityId;
    public string ActiveAbilityId => _activeAbilityId;
    public event System.Action<string> OnActiveAbilityChanged;

    private void UpdateActiveAbility()
    {
        var id = (_current as TargetingState)?.AbilityId;
        if (_activeAbilityId == id) return;
        _activeAbilityId = id;
        OnActiveAbilityChanged?.Invoke(id);
    }

    private QueryContext? _currentSelection;
    public QueryContext? CurrentSelection => _currentSelection;
    public event System.Action<QueryContext?> SelectionChanged;

    private void UpdateSelection()
    {
        QueryContext? sel = _current switch
        {
            SelectedState s  => s.Selected,
            TargetingState t => t.Source,
            _ => null
        };
        if (_currentSelection == sel) return;
        _currentSelection = sel;
        SelectionChanged?.Invoke(sel);
    }
    
    public void ResetToIdle() => TransitionTo(new IdleState(_ctx));

    public void TransitionTo(IPlayerInputState next)
    {
        if (ReferenceEquals(_current, next)) return;
        Log($"State: {_current?.GetType().Name} → {next.GetType().Name}");
        _current?.OnExit();
        _current = next;
        _current.OnEnter();
        UpdateActiveAbility();
        UpdateSelection();
    }
    
    public void LogState(string message) => Log(message);
}
