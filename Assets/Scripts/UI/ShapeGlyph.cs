using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GlyphCell { Empty, Caster, Direction, Area }

public sealed class GlyphShape
{
    private static readonly (int X, int Y)[] DirectionOffsets =
        { (0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1) };

    public int Size { get; }
    public GlyphCell[] Cells { get; }

    private GlyphShape(int size)
    {
        Size = size;
        Cells = new GlyphCell[size * size];
    }

    public static GlyphShape FromDirections(int mask)
    {
        var shape = new GlyphShape(3);
        shape.Set(0, 0, -1, 1, GlyphCell.Caster);
        for (var i = 0; i < DirectionOffsets.Length; i++)
            if ((mask & (1 << i)) != 0) shape.Set(DirectionOffsets[i].X, DirectionOffsets[i].Y, -1, 1, GlyphCell.Direction);
        return shape;
    }

    public static GlyphShape FromArea(IReadOnlyList<(int X, int Y)> cells, bool fromCaster)
    {
        int minX = fromCaster ? 0 : int.MaxValue, maxX = fromCaster ? 0 : int.MinValue;
        int minY = minX, maxY = maxX;
        foreach (var (x, y) in cells)
        {
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
        var size = Mathf.Max(maxX - minX, maxY - minY) + 1;
        var left = minX - (size - (maxX - minX + 1)) / 2;
        var top = maxY + (size - (maxY - minY + 1)) / 2;
        var shape = new GlyphShape(size);
        if (fromCaster) shape.Set(0, 0, left, top, GlyphCell.Caster);
        foreach (var (x, y) in cells) shape.Set(x, y, left, top, GlyphCell.Area);
        return shape;
    }

    private void Set(int x, int y, int left, int top, GlyphCell cell)
    {
        var column = x - left;
        var row = top - y;
        if (column < 0 || column >= Size || row < 0 || row >= Size) return;
        Cells[row * Size + column] = cell;
    }
}

public class ShapeGlyph : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup _grid;
    [SerializeField] private Image _cell;
    [SerializeField] private float _size = 48f;
    [SerializeField] private float _spacing = 2f;
    [SerializeField] private Color32 _emptyColor = new(255, 255, 255, 40);
    [SerializeField] private Color32 _casterColor = new(255, 192, 64, 255);
    [SerializeField] private Color32 _directionColor = new(255, 255, 255, 230);
    [SerializeField] private Color32 _areaColor = new(255, 128, 96, 230);

    public void Show(GlyphShape shape)
    {
        var cell = (_size - _spacing * (shape.Size - 1)) / shape.Size;
        _grid.cellSize = new Vector2(cell, cell);
        _grid.spacing = new Vector2(_spacing, _spacing);
        _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _grid.constraintCount = shape.Size;
        foreach (var kind in shape.Cells)
        {
            var image = Instantiate(_cell, _grid.transform);
            image.color = ColorOf(kind);
            image.gameObject.SetActive(true);
        }
    }

    private Color32 ColorOf(GlyphCell kind) => kind switch
    {
        GlyphCell.Caster => _casterColor,
        GlyphCell.Direction => _directionColor,
        GlyphCell.Area => _areaColor,
        _ => _emptyColor
    };
}
