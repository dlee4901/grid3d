using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputController : LoggableBehaviour
{
    public PlayerInputState State { get; private set; }
    public string ActiveAbilityId { get; private set; }
    public event System.Action<string> OnActiveAbilityChanged;
    public event System.Action TargetingChanged;
    public bool CanConfirm => State is PreviewState { Castable: true };
    
    private PlayerInputContext _ctx;

    public void Init(GridManager manager, GridInput input, CommandDispatcher dispatcher, HighlightRenderer highlights, GridState grid)
    {
        _ctx = new PlayerInputContext
        {
            Controller = this,
            Input = input,
            Dispatcher = dispatcher,
            Display = new TargetingDisplay(manager, highlights),
            Grid = grid
        };

        State = new IdleState(_ctx);
        State.OnEnter();

        input.OnPositionPressed += pressed => State.OnPress(pressed);
        input.OnPositionSelected += clicked => State.OnPositionSelected(clicked);
        input.OnPositionDragged += dragged => State.OnDrag(dragged);
        input.OnCancelClicked += () => State.OnCancel();
    }

    public void Rebind(CommandDispatcher dispatcher, GridState grid)
    {
        _ctx.Dispatcher = dispatcher;
        _ctx.Grid = grid;
        ResetToIdle();
    }

    private void Update()
    {
        if (State == null) return;
        var keyboard = Keyboard.current;
        if (keyboard == null || IsTyping()) return;
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            Log("Cancel (ESC)");
            State.OnCancel();
        }
        for (var i = 0; i < AbilityKeys.Length; i++)
            if (keyboard[AbilityKeys[i]].wasPressedThisFrame) ActivateAbilityAt(i);
    }

    private static bool IsTyping() =>
        EventSystem.current != null
        && EventSystem.current.currentSelectedGameObject is { } selected
        && selected.TryGetComponent<TMP_InputField>(out var field) && field.isFocused;

    private static readonly Key[] AbilityKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };

    private void ActivateAbilityAt(int index)
    {
        if (CurrentSelection is not { Entity: { } entity } source) return;
        if (!entity.TryGetComponent<AbilityComponent>(out var abilities) || index >= abilities.List.Count) return;
        Log($"Ability key {index + 1}: {abilities.List[index].Id}");
        State.OnAbilityActivate(abilities.List[index], source);
    }

    public void OnAbilityActivate(Ability ability, GridSource source) => State.OnAbilityActivate(ability, source);
    public void OnPositionSelected(GridSource source) => State.OnPositionSelected(source);
    public void Confirm() => (State as PreviewState)?.Confirm();
    public void Cancel() => State.OnCancel();

    public void DeselectAbility()
    {
        if (State is AbilityInputState ability) TransitionTo(new PositionSelectedState(_ctx, ability.Source));
    }

    private void UpdateActiveAbility()
    {
        var id = (State as AbilityInputState)?.AbilityId;
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
            PositionSelectedState s => s.Selected,
            AbilityInputState a => a.Source,
            _ => null
        };
        if (_currentSelection == sel) return;
        _currentSelection = sel;
        SelectionChanged?.Invoke(sel);
    }
    
    public void ResetToIdle() => TransitionTo(new IdleState(_ctx));

    public void TransitionTo(PlayerInputState next)
    {
        if (ReferenceEquals(State, next)) return;
        Log($"State: {State?.GetType().Name} → {next.GetType().Name}");
        State?.OnExit();
        State = next;
        State.OnEnter();
        UpdateActiveAbility();
        UpdateSelection();
        TargetingChanged?.Invoke();
    }
    
    public void LogState(string message) => Log(message);
}
