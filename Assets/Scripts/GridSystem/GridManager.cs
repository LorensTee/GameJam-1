using System.Collections.Generic;
using UnityEngine;

public struct GridMoveResult
{
    public bool CanMove;
    public bool IsPushingCrate;
    public Vector2Int PlayerTarget;
    public Vector2Int CrateTarget;
    public GridMover CrateMover;
}

public class GridManager : MonoBehaviour
{
    [SerializeField] Vector2Int gridSize;
    [SerializeField] int unitGridSize;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;
    
    public int UnitGridSize => unitGridSize;
    public Vector2Int GridSize => gridSize;

    Dictionary<Vector2Int, Node> grid = new Dictionary<Vector2Int, Node>();

    private void Awake()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int coords = new Vector2Int(x, y);
                grid.Add(coords, new Node(coords));
            }
        }
    }

    public bool IsInBounds(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < gridSize.x && coords.y >= 0 && coords.y < gridSize.y;
    }

    public Node GetNode(Vector2Int coords)
    {
        if (grid.TryGetValue(coords, out Node node))
        {
            return node;
        }

        return null;
    }
    
    // validate move request
    public bool TryExecuteMove(Vector2Int currentCell, Vector2Int direction, GridMover playerMover, out GridMoveResult result)
    {
        result = new GridMoveResult();
        Vector2Int targetCell = currentCell + direction;

        // check if the targetCell is in bounds
        if (!IsInBounds(targetCell)) return false;

        // check if the node actually exist and walkable
        Node targetNode = GetNode(targetCell);
        if (targetNode == null || !targetNode.IsWalkable) return false;

        // if the target cell is empty
        if (targetNode.IsAvailable)
        {
            targetNode.IsReserved = true;

            result.CanMove = true;
            result.IsPushingCrate = false;
            result.PlayerTarget = targetCell;

            playerMover.StartMove(targetCell);
            return true;
        }

        // if the target cell has a movable crate
        if (targetNode.OccupantType == CellType.Crate && !targetNode.IsLocked)
        {
            Vector2Int crateTargetCell = targetCell + direction;

            // check if the target cell of the crate is in bounds
            if (!IsInBounds(crateTargetCell)) return false;

            // if the node of the target cell exists
            Node crateTargetNode = GetNode(crateTargetCell);
            if (crateTargetNode != null && crateTargetNode.IsAvailable)
            {
                // check if the crate is already reserved to move
                GridMover crateMover = targetNode.Occupant != null ? targetNode.Occupant.GetComponent<GridMover>() : null;
                if (crateMover == null || crateMover.IsMoving) return false;

                targetNode.IsReserved = true;
                crateTargetNode.IsReserved = true;

                result.CanMove = true;
                result.IsPushingCrate = true;
                result.PlayerTarget = targetCell;
                result.CrateTarget = crateTargetCell;
                result.CrateMover = crateMover;

                // move them both
                playerMover.StartMove(targetCell);
                crateMover.StartMove(crateTargetCell);
            }
        }

        return false;
    }

    public void CompleteMove(Vector2Int fromCell, Vector2Int toCell, Component occupant, CellType type)
    {
        Node fromNode = GetNode(fromCell);
        Node toNode = GetNode(toCell);

        // clear the state of the previous node
        if (fromNode != null && fromNode.Occupant == occupant)
        {
            fromNode.OccupantType = CellType.Empty;
            fromNode.Occupant = null;
            fromNode.IsReserved = false;
        }

        // assign states to the new node
        if (toNode != null)
        {
            toNode.OccupantType = type;
            toNode.Occupant = occupant;
            toNode.IsReserved = false;
        }
    }

    public void RegisterOccupant(Vector2Int cell, Component occupant, CellType type)
    {
        Node node = GetNode(cell);
        if (node != null)
        {
            node.OccupantType = type;
            node.Occupant = occupant;
            node.IsReserved = false;
        }
    }

    public void ResetGridState()
    {
        foreach (var kvp in grid)
        {
            kvp.Value.IsReserved = false;
            kvp.Value.OccupantType = CellType.Empty;
            kvp.Value.Occupant = null;
        }
    }

    public Vector3 CellToWorld(Vector2Int cell, float heightY = 0f)
    {
        return new Vector3(
            gridOrigin.x + (cell.x * unitGridSize),
            heightY,
            gridOrigin.z + (cell.y * unitGridSize)
        );
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / (float)unitGridSize);
        int y = Mathf.RoundToInt(worldPos.z / (float)unitGridSize);
        return new Vector2Int(x, y);
    }
}
