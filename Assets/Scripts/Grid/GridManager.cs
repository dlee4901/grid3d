using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public event Action StateChanged;
    public event Action GameStarted;
    public bool IsGameStarted { get; private set; }

    public bool Submit(ICommand command) => _dispatcher.Submit(command);

    private void Awake()
    {
        GridLog.Info    = Debug.Log;
        GridLog.Warning = Debug.LogWarning;
        GridLog.Error   = Debug.LogError;

        Input = gameObject.AddComponent<GridInput>();
        Player = gameObject.AddComponent<PlayerInputController>();
    }

    public void StartGame(GridDefinition definition, IReadOnlyList<ICommand> seedCommands)
    {
        if (IsGameStarted) return;

        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");

        _executor = TurnExecutor.ForDefinition(definition);
        _dispatcher = new CommandDispatcher(_executor);
        if (seedCommands != null)
            foreach (var command in seedCommands) _executor.Apply(command);

        _lastActivePlayer = GridState.ActivePlayer;
        _executor.CommandApplied += OnCommandApplied;

        Input.Init(_renderer.Grid, _mainCamera, GridState, _selectAction);
        _renderer.Build();
        Player.Init(Input, _dispatcher, _renderer);

        IsGameStarted = true;
        GameStarted?.Invoke();
    }

    private int _lastActivePlayer;

    private void OnCommandApplied(ICommand command)
    {
        if (GridState.ActivePlayer != _lastActivePlayer)
        {
            _lastActivePlayer = GridState.ActivePlayer;
            Player.ResetToIdle();
        }
        StateChanged?.Invoke();
    }
}
