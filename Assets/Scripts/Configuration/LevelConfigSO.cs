using System;
using UnityEngine;

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

[CreateAssetMenu(
    fileName = "LevelConfig",
    menuName = "Game/Sokoban/Level Config"
)]
public sealed class LevelConfigSO : ScriptableObject
{
    [Header("Grid Settings")]
    [Tooltip("Number of playable grid cells along the world X and Z axes.")]
    [SerializeField] private GridCell gridDimensionsXZ = new GridCell(23, 23);
    [SerializeField, Min(0.01f)] private float cellSize = 1f;
    [Tooltip("World-space origin, so this correctly uses X, Y, and Z. Y is the vertical height.")]
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    [Header("Fixed Level Cells")]
    [SerializeField] private GridCell playerSpawnCellXZ = new GridCell(10, -12);
    [SerializeField] private GridCell keyholeCellXZ = new GridCell(-10, 8);
    [SerializeField] private GridCell northEastExitCellXZ = new GridCell(11, 8);
    [SerializeField] private GridCell southWestExitCellXZ = new GridCell(-10, -13);

    [Tooltip("The enemy spawn cells on the horizontal X/Z grid.")]
    [SerializeField] private GridCell[] enemySpawnCellsXZ =
    {
        new GridCell(5, -7),
        new GridCell(-5, -7),
        new GridCell(-5, 3),
        new GridCell(5, 3)
    };

    [Header("Eligible Key Crate Starting Cells")]
    [Tooltip("The 14 current crate positions under Level/Keys.")]
    [SerializeField] private GridCell[] eligibleKeyCrateStartCellsXZ =
    {
        new GridCell(9, 4),
        new GridCell(2, 1),
        new GridCell(-9, -10),
        new GridCell(9, 6),
        new GridCell(-6, -7),
        new GridCell(7, 1),
        new GridCell(-2, -7),
        new GridCell(0, -1),
        new GridCell(7, 5),
        new GridCell(-3, -5),
        new GridCell(4, -2),
        new GridCell(4, 4),
        new GridCell(-6, -11),
        new GridCell(-1, -4)
    };
    
    [SerializeField, HideInInspector] private Vector2Int gridSize = new Vector2Int(30, 30);
    [SerializeField, HideInInspector] private Vector2Int playerSpawnCell = new Vector2Int(10, -12);
    [SerializeField, HideInInspector] private Vector2Int keyholeCell = new Vector2Int(-10, 8);
    [SerializeField, HideInInspector] private Vector2Int northEastExitCell = new Vector2Int(11, 8);
    [SerializeField, HideInInspector] private Vector2Int southWestExitCell = new Vector2Int(-10, 0);
    [SerializeField, HideInInspector] private Vector2Int[] enemySpawnCells =
    {
        new Vector2Int(5, -7),
        new Vector2Int(-5, -7),
        new Vector2Int(-5, 3),
        new Vector2Int(5, 3)
    };
    [SerializeField, HideInInspector] private Vector2Int[] eligibleKeyCrateStartCells =
    {
        new Vector2Int(9, 4),
        new Vector2Int(2, 1),
        new Vector2Int(-9, -10),
        new Vector2Int(9, 6),
        new Vector2Int(-6, -7),
        new Vector2Int(7, 1),
        new Vector2Int(-2, -7),
        new Vector2Int(0, -1),
        new Vector2Int(7, 5),
        new Vector2Int(-3, -5),
        new Vector2Int(4, -2),
        new Vector2Int(4, 4),
        new Vector2Int(-6, -11),
        new Vector2Int(-1, -4)
    };
    [SerializeField, HideInInspector] private int coordinateSchemaVersion;

    public GridCell GridDimensionsXZ => gridDimensionsXZ;
    public float CellSize => cellSize;
    public Vector3 GridOrigin => gridOrigin;
    public GridCell PlayerSpawnCellXZ => playerSpawnCellXZ;
    public GridCell KeyholeCellXZ => keyholeCellXZ;
    public GridCell NorthEastExitCellXZ => northEastExitCellXZ;
    public GridCell SouthWestExitCellXZ => southWestExitCellXZ;
    public GridCell[] EnemySpawnCellsXZ => enemySpawnCellsXZ;
    public GridCell[] EligibleKeyCrateStartCellsXZ => eligibleKeyCrateStartCellsXZ;

    public bool IsEligibleKeyCrateCell(GridCell cell)
    {
        if (eligibleKeyCrateStartCellsXZ == null)
            return false;

        for (int i = 0; i < eligibleKeyCrateStartCellsXZ.Length; i++)
        {
            if (eligibleKeyCrateStartCellsXZ[i] == cell)
                return true;
        }

        return false;
    }

    private void OnValidate()
    {
        if (coordinateSchemaVersion >= 1)
            return;

        gridDimensionsXZ = new GridCell(gridSize.x, gridSize.y);
        playerSpawnCellXZ = new GridCell(playerSpawnCell.x, playerSpawnCell.y);
        keyholeCellXZ = new GridCell(keyholeCell.x, keyholeCell.y);
        northEastExitCellXZ = new GridCell(northEastExitCell.x, northEastExitCell.y);
        southWestExitCellXZ = new GridCell(southWestExitCell.x, southWestExitCell.y);
        enemySpawnCellsXZ = ConvertCells(enemySpawnCells);
        eligibleKeyCrateStartCellsXZ = ConvertCells(eligibleKeyCrateStartCells);

        coordinateSchemaVersion = 1;
    }

    private static GridCell[] ConvertCells(Vector2Int[] oldCells)
    {
        if (oldCells == null)
            return new GridCell[0];

        GridCell[] converted = new GridCell[oldCells.Length];
        for (int i = 0; i < oldCells.Length; i++)
            converted[i] = new GridCell(oldCells[i].x, oldCells[i].y);

        return converted;
    }
}
