using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField, Min(0.1f)]
    private float sensitivity = 2f;

    [SerializeField]
    private float minPitch = -75f;

    [SerializeField]
    private float maxPitch = 75f;

    private float yaw;
    private float pitch;

    private void OnEnable()
    {
        Vector3 angles = transform.localEulerAngles;

        yaw = angles.y;
        pitch = Mathf.DeltaAngle(0f, angles.x);

        LockCursor();
    }

    private void OnDisable()
    {
        UnlockCursor();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
            return;
        }

        // Allow the player to click back into the game.
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                LockCursor();
            }

            return;
        }

        if (mouse == null)
        {
            return;
        }

        Vector2 mouseDelta = mouse.delta.ReadValue();

        yaw += mouseDelta.x * sensitivity;
        pitch -= mouseDelta.y * sensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Rotate the camera only, not the player root.
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}