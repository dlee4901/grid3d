using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Coordination / composition root. Owns the authoritative executor + dispatcher, the input components,
// the match lifecycle (StartGame/GameStarted), and the command seam (Submit/StateChanged). Visuals live
// in GridRenderer; test/dev startup lives in GameBootstrap.
public class GridManager : LoggableBehaviour
{
    [SerializeField] private GridRenderer _renderer;

    private Camera _mainCamera;
    private InputAction _selectAction;

    private TurnExecutor _executor;
    private CommandDispatcher _dispatcher;

    public IReadOnlyGridState GridState => _executor.State;
    public GridInput Input { get; private set; }
    public PlayerInputController Player { get; private set; }

    // Fired after any command; GridRenderer re-renders and UI panels (StateView) refresh from state.
    public event Action StateChanged;
    // Fired once when the match starts (state built). Sticky via IsGameStarted for order-independence.
    public event Action GameStarted;
    public bool IsGameStarted { get; private set; }

    // Frontend entry point for UI-issued commands (end turn, timer timeouts). The seam networking intercepts.
    public bool Submit(ICommand command) => _dispatcher.Submit(command);

    private void Awake()
    {
        GridLog.Info    = Debug.Log;
        GridLog.Warning = Debug.LogWarning;
        GridLog.Error   = Debug.LogError;

        Input = gameObject.AddComponent<GridInput>();
        Player = gameObject.AddComponent<PlayerInputController>();
    }

    // Explicit "begin the match" entry point — called by GameBootstrap today, the lobby/relay flow later.
    // State does not exist until this runs. seedCommands are applied before the CommandApplied subscription.
    public void StartGame(GridDefinition definition, IReadOnlyList<ICommand> seedCommands)
    {
        if (IsGameStarted) return;

        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");

        _executor = TurnExecutor.ForDefinition(definition);
        _dispatcher = new CommandDispatcher(_executor);
        if (seedCommands != null)
            foreach (var command in seedCommands) _executor.Apply(command);

        _executor.CommandApplied += OnCommandApplied;

        Input.Init(_renderer.Grid, _mainCamera, GridState, _selectAction);
        _renderer.Build();
        Player.Init(Input, _dispatcher, _renderer);

        IsGameStarted = true;
        GameStarted?.Invoke();
    }

    // Every command re-renders the grid (GridRenderer) and refreshes UI panels — both via StateChanged.
    private void OnCommandApplied(ICommand command) => StateChanged?.Invoke();
}
