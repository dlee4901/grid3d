using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerInputState { Idle, EntitySelected, AimingSkill }

public class PlayerInputController : LoggableBehaviour
{
    private GridManager _gridManager;
    private GridInput _input;
    private TurnExecutor _executor;
    private FiniteStateMachine<PlayerInputState> _fsm;

    private Skill _aimingSkill;
    private QueryContext _aimingSource;
    private readonly List<(int, int)> _targets = new();

    public PlayerInputState State => _fsm.Current;

    public void Init(GridManager gridManager, GridInput input, TurnExecutor executor)
    {
        _gridManager = gridManager;
        _input = input;
        _executor = executor;

        _fsm = new FiniteStateMachine<PlayerInputState>(PlayerInputState.Idle)
            .OnEnter(PlayerInputState.AimingSkill, EnterAiming)
            .OnExit(PlayerInputState.AimingSkill, ExitAiming);
        _fsm.OnTransition += (prev, next) => Log($"State: {prev} → {next}");

        _input.OnTileClicked += OnTileClicked;
        _input.OnSelectionChanged += OnGridSelectionChanged;
    }

    private void Update()
    {
        if (_fsm == null) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Cancel();
    }

    // ---- Skill icon intent handlers (called by UnitInfo from SkillIcon events)
    public void OnSkillPreview(Skill skill, QueryContext ctx)
    {
        if (_fsm.Is(PlayerInputState.AimingSkill)) return; // don't overwrite real aim preview
        _gridManager.ShowSkillPreview(skill, ctx);
    }

    public void OnSkillCancelPreview()
    {
        if (_fsm.Is(PlayerInputState.AimingSkill)) return; // keep real aim preview visible
        _gridManager.ClearSkillPreview();
    }

    public void OnSkillActivate(Skill skill, QueryContext ctx)
    {
        if (!_fsm.Is(PlayerInputState.EntitySelected)) return;
        _aimingSkill = skill;
        _aimingSource = ctx;
        _targets.Clear();
        _fsm.TransitionTo(PlayerInputState.AimingSkill);
    }

    // ---- Grid input handlers
    private void OnTileClicked(QueryContext clicked)
    {
        switch (_fsm.Current)
        {
            case PlayerInputState.Idle:
            case PlayerInputState.EntitySelected:
                _input.Select(clicked);
                break;
            case PlayerInputState.AimingSkill:
                if (!IsValidTarget(clicked.SourcePosition))
                {
                    _fsm.TransitionTo(PlayerInputState.Idle);
                    break;
                }
                _targets.Add(clicked.SourcePosition);
                _gridManager.HighlightTargets(_targets);
                if (_targets.Count >= _aimingSkill.Selection.SelectionAmount) Confirm();
                break;
        }
    }

    private void OnGridSelectionChanged(QueryContext? ctx)
    {
        if (_fsm.Is(PlayerInputState.AimingSkill)) return; // selection is locked while aiming
        _fsm.TransitionTo(ctx.HasValue && ctx.Value.SourceEntity != null
            ? PlayerInputState.EntitySelected
            : PlayerInputState.Idle);
    }

    public void Cancel()
    {
        if (!_fsm.Is(PlayerInputState.AimingSkill)) return;
        _fsm.TransitionTo(_input.HasSelection ? PlayerInputState.EntitySelected : PlayerInputState.Idle);
    }

    // ---- FSM lifecycle
    private void EnterAiming()
    {
        _input.Lock();
        _gridManager.ShowSkillPreview(_aimingSkill, _aimingSource);
    }

    private void ExitAiming()
    {
        _input.Unlock();
        _gridManager.ClearSkillPreview();
        _gridManager.ClearTargetHighlight();
        _aimingSkill = null;
        _targets.Clear();
    }

    // ---- Helpers
    private void Confirm()
    {
        var sourcePos1D = _aimingSource.Grid.ToPosition1D(_aimingSource.SourcePosition);
        var targets1D = _targets.Select(t => _aimingSource.Grid.ToPosition1D(t)).ToArray();
        var ok = _executor.Apply(new SkillCommand(_aimingSkill.Id, sourcePos1D, targets1D));
        if (ok) _gridManager.RefreshEntityModelPositions();
        _fsm.TransitionTo(_input.HasSelection ? PlayerInputState.EntitySelected : PlayerInputState.Idle);
    }

    private bool IsValidTarget((int, int) pos)
    {
        var (areas, _) = _aimingSkill.Selection.GetSelectablePositions(_aimingSource);
        return areas.Contains(pos);
    }
}
