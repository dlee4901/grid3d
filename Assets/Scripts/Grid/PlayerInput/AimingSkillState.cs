using System.Collections.Generic;
using System.Linq;

public class AimingSkillState : PlayerInputStateBase
{
    private readonly Skill _skill;
    private readonly QueryContext _source;
    private readonly List<(int, int)> _targets = new();

    public AimingSkillState(PlayerInputContext ctx, Skill skill, QueryContext source) : base(ctx)
    {
        _skill = skill;
        _source = source;
    }

    public override void OnEnter()
    {
        Ctx.Input.Lock();
        Ctx.GridManager.ShowSkillPreview(_skill, _source);
    }

    public override void OnExit()
    {
        Ctx.Input.Unlock();
        Ctx.GridManager.ClearSkillPreview();
        Ctx.GridManager.ClearTargetHighlight();
    }

    public override void OnTileClicked(QueryContext clicked)
    {
        if (!IsValidTarget(clicked.SourcePosition))
        {
            Ctx.Controller.TransitionTo(new IdleState(Ctx));
            return;
        }
        _targets.Add(clicked.SourcePosition);
        Ctx.GridManager.HighlightTargets(_targets);
        if (_targets.Count >= _skill.Selection.SelectionAmount) Confirm();
    }

    // selection is locked while aiming — no-op
    public override void OnGridSelectionChanged(QueryContext? ctx) { }

    public override void OnCancel()
    {
        var next = Ctx.Input.HasSelection
            ? (IPlayerInputState)new EntitySelectedState(Ctx)
            : new IdleState(Ctx);
        Ctx.Controller.TransitionTo(next);
    }

    private bool IsValidTarget((int, int) pos)
    {
        var (areas, _) = _skill.Selection.GetSelectablePositions(_source);
        return areas.Contains(pos);
    }

    private void Confirm()
    {
        var sourcePos1D = _source.Grid.ToPosition1D(_source.SourcePosition);
        var targets1D = _targets.Select(t => _source.Grid.ToPosition1D(t)).ToArray();
        var ok = Ctx.Executor.Apply(new SkillCommand(_skill.Id, sourcePos1D, targets1D));
        if (ok) Ctx.GridManager.RefreshEntityModelPositions();
        var next = Ctx.Input.HasSelection
            ? (IPlayerInputState)new EntitySelectedState(Ctx)
            : new IdleState(Ctx);
        Ctx.Controller.TransitionTo(next);
    }
}
