using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] Vector2Int gridSize;
    [SerializeField] int unitGridSize;
    
    public int UnitGridSize{get { return unitGridSize; }}

    Dictionary<Vector2Int, Node> grid = new Dictionary<Vector2Int, Node>();
    Dictionary<Vector2Int, Node> Grid {get { return grid; }}

    private void Awake()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int cords = new Vector2Int(x, y);
                grid.Add(cords, new Node(cords));
            }
        }
    }
}
