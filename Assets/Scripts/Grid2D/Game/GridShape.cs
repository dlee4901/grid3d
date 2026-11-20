using System;
using System.Collections.Generic;

public sealed class GridShape
{
    private readonly bool[] _exists;

    public int X { get; }
    public int Y { get; }

    public GridShape(int x, int y, IReadOnlyList<string> rows = null)
    {
        X = x;
        Y = y;
        _exists = new bool[x * y];
        for (var i = 0; i < _exists.Length; i++) _exists[i] = rows == null;
        if (rows == null) return;
        if (rows.Count != y) throw new ArgumentException($"Layout has {rows.Count} rows, expected {y}");
        for (var row = 0; row < y; row++)
        {
            if (rows[row].Length != x) throw new ArgumentException($"Layout row {row} has {rows[row].Length} columns, expected {x}");
            for (var col = 0; col < x; col++) _exists[row * x + col] = rows[row][col] == '#';
        }
    }

    public bool Contains(int dim1) => dim1 >= 0 && dim1 < _exists.Length && _exists[dim1];

    public bool Contains((int x, int y) dim2)
        => dim2.x >= 0 && dim2.x < X && dim2.y >= 0 && dim2.y < Y && _exists[dim2.y * X + dim2.x];
}
