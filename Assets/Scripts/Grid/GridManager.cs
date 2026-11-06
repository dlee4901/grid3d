using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : LoggableBehaviour
{
    private enum SeatMode { Hotseat, Fixed }

    [SerializeField] private GridRenderer _renderer;
    [SerializeField] private SeatMode _seatMode = SeatMode.Hotseat;
    [SerializeField] private int _fixedSeat = 1;
    [SerializeField] private bool _timersEnabled = true;

    private Camera _mainCamera;
    private InputAction _selectAction;

    private TurnExecutor _executor;
    private CommandDispatcher _dispatcher;
    private GridState _preview;

    public GridState GridState => _executor.State;
    public GridState DisplayedState => _preview ?? GridState;
    public bool IsPreviewing => _preview != null;
    public GridInput Input { get; private set; }
    public PlayerInputController Player { get; private set; }
    public TurnController Turns { get; private set; }
    public Camera MainCamera => _mainCamera;
    public GridRenderer Renderer => _renderer;
    public int LocalSeat => _seatMode == SeatMode.Hotseat ? GridState.ActivePlayer : _fixedSeat;
    public bool IsNetworked => _seatMode == SeatMode.Fixed;
    public string StopReason { get; private set; }

    public event Action StateChanged;
    public event Action DisplayChanged;
    public event Action GameStarted;
    public bool IsGameStarted { get; private set; }

    public bool Submit(Func<int, Command> build) => _dispatcher.Submit(build);

    public bool Preview(Func<int, Command> build)
    {
        var preview = _dispatcher.Preview(build);
        if (preview == null) return false;
        _preview = preview;
        DisplayChanged?.Invoke();
        return true;
    }

    public void ClearPreview()
    {
        if (_preview == null) return;
        _preview = null;
        DisplayChanged?.Invoke();
    }

    private void Awake()
    {
        GridLog.Info    = Debug.Log;
        GridLog.Warning = Debug.LogWarning;
        GridLog.Error   = Debug.LogError;

        Input = gameObject.AddComponent<GridInput>();
        Player = gameObject.AddComponent<PlayerInputController>();
        Turns = gameObject.AddComponent<TurnController>();
    }

    public void StartGame(GridDefinition definition, IReadOnlyList<Command> seedCommands, int fixedSeat = 0)
    {
        if (IsGameStarted) return;
        if (fixedSeat > 0)
        {
            _seatMode = SeatMode.Fixed;
            _fixedSeat = fixedSeat;
        }

        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");

        _executor = TurnExecutor.ForDefinition(definition);
        _dispatcher = new CommandDispatcher(_executor, () => LocalSeat);
        if (seedCommands != null) foreach (var command in seedCommands) _executor.Apply(command);

        _lastActivePlayer = GridState.ActivePlayer;
        _executor.CommandApplied += OnCommandApplied;

        Input.Init(_renderer.Grid, _mainCamera, GridState, _selectAction);
        _renderer.Build();
        Player.Init(this, Input, _dispatcher, _renderer, GridState);
        Turns.Init(this, _timersEnabled);

        IsGameStarted = true;
        GameStarted?.Invoke();
    }

    public void AttachSession(LockstepSession session)
    {
        session.Failed += OnSessionFailed;
        session.Attach(_executor);
    }

    private void OnSessionFailed(string reason)
    {
        StopReason = reason;
        _dispatcher.Enabled = false;
        StateChanged?.Invoke();
    }

    private int _lastActivePlayer;

    private void OnCommandApplied(Command command)
    {
        _preview = null;
        if (GridState.ActivePlayer != _lastActivePlayer)
        {
            _lastActivePlayer = GridState.ActivePlayer;
            Player.ResetToIdle();
        }
        StateChanged?.Invoke();
        DisplayChanged?.Invoke();
    }
}
