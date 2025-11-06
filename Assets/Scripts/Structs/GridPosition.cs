using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public struct GridPosition : IEquatable<GridPosition>
{
    public int x;
    public int y;

    public GridPosition (int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public override string ToString ()
    {
        return ToString (null, null);
    }

    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public string ToString (string format)
    {
        return ToString (format, null);
    }

    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public string ToString (string format, IFormatProvider formatProvider)
    {
        if (string.IsNullOrEmpty (format))
        {
            format = "F2";
        }

        if (formatProvider == null)
        {
            formatProvider = CultureInfo.InvariantCulture.NumberFormat;
        }

        return string.Format ("({0}, {1})", x.ToString (format, formatProvider), y.ToString (format, formatProvider));
    }

    public static GridPosition operator +(GridPosition l, GridPosition r)
    {
        return new GridPosition(l.x + r.x, l.y + r.y);
    }

    public static GridPosition operator -(GridPosition l, GridPosition r)
    {
        return new GridPosition(l.x - r.x, l.y - r.y);
    }

    public static GridPosition operator *(GridPosition grid, int num)
    {
        return new GridPosition(grid.x * num, grid.y * num);
    }

    public static bool operator ==(GridPosition l, GridPosition r)
    {
        return l.x == r.x && l.y == r.y;
    }

    public static bool operator !=(GridPosition l, GridPosition r)
    {
        return l.x != r.x || l.y != r.y;
    }

    public override bool Equals(object obj)
    {
        return obj is GridPosition other && this == other;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + x;
            hash = hash * 31 + y;
            return hash;
        }
    }

    public bool Equals(GridPosition other)
    {
        return this == other;   
    }
}
