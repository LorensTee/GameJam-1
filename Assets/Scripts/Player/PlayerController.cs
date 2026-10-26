
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool inputEnabled = true;

    [Header("View Settings")]
    [SerializeField] private Transform viewTransform;

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
        Vector2Int relativeInput = Vector2Int.zero;

        // North
        if (keyboard.wKey.wasPressedThisFrame ||
            keyboard.upArrowKey.wasPressedThisFrame)
        {
            relativeInput = Vector2Int.up;
        }

        // South
        if (keyboard.sKey.wasPressedThisFrame ||
            keyboard.downArrowKey.wasPressedThisFrame)
        {
            relativeInput = Vector2Int.down;
        }

        // West
        if (keyboard.aKey.wasPressedThisFrame ||
            keyboard.leftArrowKey.wasPressedThisFrame)
        {
            relativeInput = Vector2Int.left;
        }

        // East
        if (keyboard.dKey.wasPressedThisFrame ||
            keyboard.rightArrowKey.wasPressedThisFrame)
        {
            relativeInput = Vector2Int.right;
        }

        if (relativeInput == Vector2Int.zero)
            return Vector2Int.zero;

        return TransformToGridDirection(relativeInput);
    }

    private Vector2Int TransformToGridDirection(Vector2Int localDir)
    {
        // use assigned view transform, or fallback to main camera / local transform
        Transform referenceTransform = viewTransform != null ? viewTransform : (Camera.main != null ? Camera.main.transform : transform);

        Vector3 forward = referenceTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.01f)
        {
            forward = referenceTransform.up;
            forward.y = 0f;
        }
        forward.Normalize();

        Vector2Int gridForward;
        if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
        {
            gridForward = new Vector2Int(forward.x > 0 ? 1 : -1, 0);
        }
        else
        {
            gridForward = new Vector2Int(0, forward.z > 0 ? 1 : -1);
        }

        Vector2Int gridRight = new Vector2Int(gridForward.y, -gridForward.x);

        // Map local input (Forward/Backward/Left/Right) onto grid vectors
        return (gridForward * localDir.y) + (gridRight * localDir.x);
    }

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
