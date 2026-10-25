using System;
using System.Collections;
using UnityEngine;

public class GridMover : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private CellType occupantType;

    private GridManager gridManager;
    private Coroutine moveCoroutine;

    public Vector2Int CurrentCell { get; private set; }
    public Vector2Int TargetCell { get; private set; }
    public bool IsMoving { get; private set; }

    public event Action<Vector2Int> OnMoveStarted;
    public event Action<Vector2Int> OnMoveCompleted;

    public void Initialize(GridManager manager, Vector2Int startCell)
    {
        gridManager = manager;
        CurrentCell = startCell;
        TargetCell = startCell;
        
        Vector3 snappedPos = gridManager.CellToWorld(startCell, transform.position.y);
        transform.position = snappedPos;

        gridManager.RegisterOccupant(startCell, this, occupantType);
    }

    public void StartMove(Vector2Int destination)
    {
        if (IsMoving) return;

        TargetCell = destination;
        IsMoving = true;
        OnMoveStarted?.Invoke(destination);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateMove(destination));
    }

    private IEnumerator AnimateMove(Vector2Int destination)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = gridManager.CellToWorld(destination, transform.position.y);
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;

        Vector2Int previousCell = CurrentCell;
        CurrentCell = destination;
        IsMoving = false;

        gridManager.CompleteMove(previousCell, CurrentCell, this, occupantType);
        OnMoveCompleted?.Invoke(CurrentCell);
    }

    public void CancelAndSnapTo(Vector2Int cell)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        IsMoving = false;
        CurrentCell = cell;
        TargetCell = cell;
        transform.position = gridManager.CellToWorld(cell, transform.position.y);
    }

    public void RespawnAt(Vector3 worldPosition)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        IsMoving = false;

        Vector2Int oldCell = CurrentCell;
        Vector2Int newCell = gridManager.WorldToCell(worldPosition);

        gridManager.CompleteMove(oldCell, newCell, this, occupantType);

        CurrentCell = newCell;
        TargetCell = newCell;

        transform.position = gridManager.CellToWorld(
            newCell,
            transform.position.y
        );
    }
}