using UnityEngine.InputSystem;

public class PlayerInputController : LoggableBehaviour
{
    public IPlayerInputState State { get; private set; }
    public HighlightTracker Highlights { get; private set; }
    public string ActiveAbilityId { get; private set; }
    public event System.Action<string> OnActiveAbilityChanged;
    
    private PlayerInputContext _ctx;

    public void Init(GridInput input, CommandDispatcher dispatcher, IGridRenderer renderer, IReadOnlyGridState grid)
    {
        Highlights = new HighlightTracker(renderer);
        _ctx = new PlayerInputContext
        {
            Controller = this,
            Input = input,
            Dispatcher = dispatcher,
            Renderer = Highlights,
            Grid = grid
        };

        State = new IdleState(_ctx);
        State.OnEnter();

        input.OnPositionSelected += clicked => State.OnPositionSelected(clicked);
        input.OnCancelClicked += () => State.OnCancel();
        input.OnHoverChanged += hovered => State.OnHover(hovered);
    }

    private void Update()
    {
        if (State == null) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Log("Cancel (ESC)");
            State.OnCancel();
        }
    }

    public void OnAbilityPreview(Ability ability, GridSource source) => State.OnAbilityPreview(ability, source);
    public void OnAbilityCancelPreview() => State.OnAbilityCancelPreview();
    public void OnAbilityActivate(Ability ability, GridSource source) => State.OnAbilityActivate(ability, source);
    public void OnPositionSelected(GridSource source) => State.OnPositionSelected(source);

    private void UpdateActiveAbility()
    {
        var id = (State as TargetingState)?.AbilityId;
        if (ActiveAbilityId == id) return;
        ActiveAbilityId = id;
        OnActiveAbilityChanged?.Invoke(id);
    }

    private GridSource? _currentSelection;
    public GridSource? CurrentSelection => _currentSelection;
    public event System.Action<GridSource?> SelectionChanged;

    private void UpdateSelection()
    {
        GridSource? sel = State switch
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
        if (ReferenceEquals(State, next)) return;
        Log($"State: {State?.GetType().Name} → {next.GetType().Name}");
        State?.OnExit();
        State = next;
        State.OnEnter();
        UpdateActiveAbility();
        UpdateSelection();
    }
    
    public void LogState(string message) => Log(message);
}
