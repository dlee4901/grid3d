using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionController : SingletonBehaviour<SelectionController>
{
    private QueryContext? _hovered;
    private QueryContext? _selected;
    private readonly List<(int, int)> _skillTargets = new();
    private bool _isLocked;

    public QueryContext? Hovered => _hovered;
    public QueryContext? Selected => _selected;
    public IReadOnlyList<(int, int)> SkillTargets => _skillTargets;
    public bool IsLocked => _isLocked;
    public bool HasSelection => _selected.HasValue;

    public event Action<QueryContext?> OnHoverChanged;
    public event Action<QueryContext?> OnSelectionChanged;
    public event Action<IReadOnlyList<(int, int)>> OnSkillTargetsChanged;
    public event Action<bool> OnLockChanged;

    // ---- Hover (transient, never blocked by lock)
    public void SetHover(QueryContext? ctx)
    {
        if (NullableContextEquals(_hovered, ctx)) return;
        _hovered = ctx;
        OnHoverChanged?.Invoke(_hovered);
    }

    public void ClearHover() => SetHover(null);

    // ---- Selection (committed, blocked by lock)
    public void Select(QueryContext ctx)
    {
        if (_isLocked) return;
        if (_selected.HasValue && ContextEquals(_selected.Value, ctx)) return;
        _selected = ctx;
        ClearSkillTargets();
        OnSelectionChanged?.Invoke(_selected);
    }

    public void ClearSelection()
    {
        if (_isLocked) return;
        if (!_selected.HasValue) return;
        _selected = null;
        ClearSkillTargets();
        OnSelectionChanged?.Invoke(_selected);
    }

    // ---- Skill targets (multi-tile AOE flow, allowed while locked)
    public void AddSkillTarget((int, int) position)
    {
        _skillTargets.Add(position);
        OnSkillTargetsChanged?.Invoke(_skillTargets);
    }

    public bool RemoveSkillTarget((int, int) position)
    {
        if (!_skillTargets.Remove(position)) return false;
        OnSkillTargetsChanged?.Invoke(_skillTargets);
        return true;
    }

    public void ClearSkillTargets()
    {
        if (_skillTargets.Count == 0) return;
        _skillTargets.Clear();
        OnSkillTargetsChanged?.Invoke(_skillTargets);
    }

    // ---- Lock (cross-turn, idempotent)
    public void Lock()
    {
        if (_isLocked) return;
        _isLocked = true;
        OnLockChanged?.Invoke(true);
    }

    public void Unlock()
    {
        if (!_isLocked) return;
        _isLocked = false;
        OnLockChanged?.Invoke(false);
    }

    // ---- Convenience predicates for UI binding
    public bool IsHovered((int, int) position)
        => _hovered.HasValue && _hovered.Value.SourcePosition == position;

    public bool IsSelected((int, int) position)
        => _selected.HasValue && _selected.Value.SourcePosition == position;

    public bool IsSkillTarget((int, int) position)
        => _skillTargets.Contains(position);

    // ---- Equality helpers
    private static bool ContextEquals(QueryContext a, QueryContext b)
        => a.SourcePosition == b.SourcePosition
           && ReferenceEquals(a.SourceEntity, b.SourceEntity)
           && ReferenceEquals(a.Grid, b.Grid);

    private static bool NullableContextEquals(QueryContext? a, QueryContext? b)
    {
        if (!a.HasValue && !b.HasValue) return true;
        if (a.HasValue != b.HasValue) return false;
        return ContextEquals(a.Value, b.Value);
    }
}
