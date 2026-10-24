using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GridMover))]
public class PlayerGridAdapter : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private PlayerController playerController;
    private GridMover gridMover;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        gridMover = GetComponent<GridMover>();
    }

    private void Start()
    {
        Vector2Int startCell = gridManager.WorldToCell(transform.position);
        gridMover.Initialize(gridManager, startCell);
    }

    private void Update()
    {
        // dont pull input if moving
        if (gridMover.IsMoving) return;

        // consume pending input when available
        if (playerController.TryConsumeMoveRequest(out Vector2Int direction))
        {
            // validate move
            gridManager.TryExecuteMove(gridMover.CurrentCell, direction, gridMover, out _);
        }
    }
}