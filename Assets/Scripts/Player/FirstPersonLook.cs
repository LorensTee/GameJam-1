using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField, Min(0.1f)]
    private float sensitivity = 15f;

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
        if (Time.timeScale == 0f)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                UnlockCursor();
            }
            return;
        }

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
            return;
        }

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

        // Scale by Time.deltaTime to decouple mouse look speed from frame rate fluctuations
        float deltaFactor = Time.deltaTime * sensitivity;

        yaw += mouseDelta.x * deltaFactor;
        pitch -= mouseDelta.y * deltaFactor;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply rotation
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}