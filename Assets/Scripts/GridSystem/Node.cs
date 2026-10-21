using UnityEngine;

public enum CellType
{
    Empty,
    Wall,
    Crate,
    Player,
    Enemy
}

public class Node
{
    public Vector2Int Coordinates { get; private set; }
    public bool IsWalkable { get; set; } = true;
    public CellType OccupantType { get; set; } = CellType.Empty;
    public Component Occupant { get; set; }
    public bool IsReserved { get; set; } = false;
    public bool IsLocked { get; set; } = false;

    public Node(Vector2Int coordinates)
    {
        Coordinates = coordinates;
    }

    public bool IsAvailable => IsWalkable && OccupantType == CellType.Empty && !IsReserved && !IsLocked;
}