using System;

[Serializable]
public struct GridCell : IEquatable<GridCell>
{
    public int X;
    public int Z;

    public GridCell(int x, int z)
    {
        X = x;
        Z = z;
    }

    public bool Equals(GridCell other) => X == other.X && Z == other.Z;

    public override bool Equals(object obj) => obj is GridCell other && Equals(other);

    public override int GetHashCode() => (X * 397) ^ Z;

    public static bool operator ==(GridCell left, GridCell right) => left.Equals(right);
    public static bool operator !=(GridCell left, GridCell right) => !left.Equals(right);

    public override string ToString() => $"({X}, {Z})";
}
