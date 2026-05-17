using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerFlowState { Idle, EntitySelected, AimingSkill }

public class PlayerInputController : MonoBehaviour
{
    private GridManager _gridManager;
    private GridInput _input;
    private TurnExecutor _executor;
    private FiniteStateMachine<PlayerFlowState> _fsm;

    private Skill _aimingSkill;
    private QueryContext _aimingSource;
    private readonly List<(int, int)> _targets = new();

    [SerializeField] private bool _debug;

    public PlayerFlowState State => _fsm.Current;

    public void Init(GridManager gridManager, GridInput input, TurnExecutor executor)
    {
        _gridManager = gridManager;
        _input = input;
        _executor = executor;

        _fsm = new FiniteStateMachine<PlayerFlowState>(PlayerFlowState.Idle)
            .OnEnter(PlayerFlowState.AimingSkill, EnterAiming)
            .OnExit(PlayerFlowState.AimingSkill, ExitAiming);
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
        if (_fsm.Is(PlayerFlowState.AimingSkill)) return; // don't overwrite real aim preview
        _gridManager.ShowSkillPreview(skill, ctx);
    }

    public void OnSkillCancelPreview()
    {
        if (_fsm.Is(PlayerFlowState.AimingSkill)) return; // keep real aim preview visible
        _gridManager.ClearSkillPreview();
    }

    public void OnSkillActivate(Skill skill, QueryContext ctx)
    {
        if (!_fsm.Is(PlayerFlowState.EntitySelected)) return;
        _aimingSkill = skill;
        _aimingSource = ctx;
        _targets.Clear();
        _fsm.TransitionTo(PlayerFlowState.AimingSkill);
    }

    // ---- Grid input handlers
    private void OnTileClicked(QueryContext clicked)
    {
        switch (_fsm.Current)
        {
            case PlayerFlowState.Idle:
            case PlayerFlowState.EntitySelected:
                _input.Select(clicked);
                break;

            case PlayerFlowState.AimingSkill:
                if (!IsValidTarget(clicked.SourcePosition)) return;
                _targets.Add(clicked.SourcePosition);
                _gridManager.HighlightTargets(_targets);
                if (_targets.Count >= _aimingSkill.Selection.SelectionAmount) Confirm();
                break;
        }
    }

    private void OnGridSelectionChanged(QueryContext? ctx)
    {
        if (_fsm.Is(PlayerFlowState.AimingSkill)) return; // selection is locked while aiming
        _fsm.TransitionTo(ctx.HasValue && ctx.Value.SourceEntity != null
            ? PlayerFlowState.EntitySelected
            : PlayerFlowState.Idle);
    }

    public void Cancel()
    {
        if (!_fsm.Is(PlayerFlowState.AimingSkill)) return;
        _fsm.TransitionTo(_input.HasSelection ? PlayerFlowState.EntitySelected : PlayerFlowState.Idle);
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
        _executor.Apply(new SkillCommand(_aimingSkill, _aimingSource, _targets.ToArray()));
        _fsm.TransitionTo(_input.HasSelection ? PlayerFlowState.EntitySelected : PlayerFlowState.Idle);
    }

    private bool IsValidTarget((int, int) pos)
    {
        var (areas, _) = _aimingSkill.Selection.GetSelectablePositions(_aimingSource);
        return areas.Contains(pos);
    }

    private void Log(string message)
    {
        if (!_debug) return;
        Debug.Log($"[PlayerInput] {message}");
    }
}
