using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light flashlight;
    [SerializeField] private Transform viewTransform;
    [SerializeField] private Transform enemyTarget;

    [Tooltip("Include wall and crate layers only.")]
    [SerializeField] private LayerMask occlusionMask;

    [Header("Flashlight Settings")]
    [SerializeField, Min(0.1f)]
    private float detectionRange = 8f;

    [SerializeField, Range(1f, 179f)]
    private float beamAngle = 45f;

    [Header("Events")]
    [SerializeField] private GameEventChannelSO enemyDetectedEvent;

    public bool IsEnemyIlluminated { get; private set; }

    private bool wasDetected;

    private void Awake()
    {
        if (flashlight == null)
            flashlight = GetComponentInChildren<Light>();

        if (viewTransform == null)
            viewTransform = transform;

        if (flashlight == null)
        {
            Debug.LogError(
                "FlashlightController requires a Light component.",
                this
            );

            enabled = false;
            return;
        }

        flashlight.type = LightType.Spot;
        flashlight.range = detectionRange;
        flashlight.spotAngle = beamAngle;
    }

    private void Update()
    {
        // Explicitly detect F using the new Input System.
        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            flashlight.enabled = !flashlight.enabled;

            Debug.Log(
                "Flashlight toggled: " +
                (flashlight.enabled ? "ON" : "OFF")
            );
        }

        IsEnemyIlluminated =
            flashlight.enabled && IsEnemyInsideBeam();

        if (IsEnemyIlluminated && !wasDetected)
        {
            Debug.Log("TEST ENEMY DETECTED!", this);
            enemyDetectedEvent?.RaiseEvent();
        }

        wasDetected = IsEnemyIlluminated;
    }

    private bool IsEnemyInsideBeam()
    {
        if (enemyTarget == null || viewTransform == null)
            return false;

        Vector3 toEnemy = enemyTarget.position - viewTransform.position;
        float distance = toEnemy.magnitude;

        if (distance <= 0f || distance > detectionRange)
            return false;

        float angle = Vector3.Angle(
            viewTransform.forward,
            toEnemy / distance
        );

        if (angle > beamAngle * 0.5f)
            return false;

        return HasLineOfSightToEnemy();
    }

    public bool HasLineOfSightToEnemy()
    {
        if (enemyTarget == null || viewTransform == null)
            return false;

        Vector3 toEnemy = enemyTarget.position - viewTransform.position;
        float distance = toEnemy.magnitude;

        if (distance <= 0f)
            return true;

        return !Physics.Raycast(
            viewTransform.position,
            toEnemy / distance,
            distance,
            occlusionMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
