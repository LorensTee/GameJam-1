
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool inputEnabled = true;

    [Tooltip("Print movement requests to the Console while testing.")]
    [SerializeField] private bool debugInput = true;

    private Vector2Int pendingDirection;
    private bool hasPendingMove;

    public bool HasPendingMove => hasPendingMove;
    public bool InputEnabled => inputEnabled;

    private void Update()
    {
        if (!inputEnabled || hasPendingMove)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        Vector2Int direction = ReadMovementInput(keyboard);

        if (direction == Vector2Int.zero)
            return;

        // Hold this request until the grid system consumes it.
        pendingDirection = direction;
        hasPendingMove = true;

        if (debugInput)
        {
            Debug.Log(
                $"Movement requested: {GetDirectionName(direction)}",
                this
            );
        }
    }

    private Vector2Int ReadMovementInput(Keyboard keyboard)
    {
        // North: positive world Z.
        if (keyboard.wKey.wasPressedThisFrame ||
            keyboard.upArrowKey.wasPressedThisFrame)
        {
            return new Vector2Int(0, 1);
        }

        // South: negative world Z.
        if (keyboard.sKey.wasPressedThisFrame ||
            keyboard.downArrowKey.wasPressedThisFrame)
        {
            return new Vector2Int(0, -1);
        }

        // West: negative world X.
        if (keyboard.aKey.wasPressedThisFrame ||
            keyboard.leftArrowKey.wasPressedThisFrame)
        {
            return new Vector2Int(-1, 0);
        }

        // East: positive world X.
        if (keyboard.dKey.wasPressedThisFrame ||
            keyboard.rightArrowKey.wasPressedThisFrame)
        {
            return new Vector2Int(1, 0);
        }

        return Vector2Int.zero;
    }

    /// <summary>
    /// Lets the grid system retrieve one pending movement request.
    /// Returns false when there is no request waiting.
    /// </summary>
    public bool TryConsumeMoveRequest(out Vector2Int direction)
    {
        if (!hasPendingMove)
        {
            direction = Vector2Int.zero;
            return false;
        }

        direction = pendingDirection;

        pendingDirection = Vector2Int.zero;
        hasPendingMove = false;

        return true;
    }

    public void SetInputEnabled(bool value)
    {
        inputEnabled = value;

        if (!value)
            ClearPendingMove();
    }

    public void ClearPendingMove()
    {
        pendingDirection = Vector2Int.zero;
        hasPendingMove = false;
    }

    private string GetDirectionName(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return "North";
        if (direction == Vector2Int.down) return "South";
        if (direction == Vector2Int.left) return "West";
        if (direction == Vector2Int.right) return "East";

        return "Unknown";
    }
}
