using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light flashlight;
    [SerializeField] private Transform viewTransform;

    [Tooltip("Include wall and crate layers only.")]
    [SerializeField] private LayerMask occlusionMask;

    [Header("Flashlight Settings")]
    [SerializeField, Min(0.1f)]
    private float detectionRange = 8f;

    [SerializeField, Range(1f, 179f)]
    private float beamAngle = 45f;


    private GameObject[] enemies;
    private HashSet<EnemyController> detectedEnemies = new();

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
            enabled = false;
            return;
        }

        flashlight.type = LightType.Spot;
        flashlight.range = detectionRange;
        flashlight.spotAngle = beamAngle;
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            flashlight.enabled = !flashlight.enabled;
        }

        HashSet<EnemyController> currentlyDetected = new();

        if (flashlight.enabled)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in enemies)
            {
                if (IsEnemyInsideBeam(enemy.transform))
                {
                    // Debug.Log("Trying to freeze Enemy");
                    EnemyController controller =
                        enemy.GetComponent<EnemyController>();

                    if (controller != null)
                    {
                        currentlyDetected.Add(controller);
                        controller.SetFrozen(true);
                    }
                }
            }
        }

        // Unfreeze enemies that are no longer illuminated.
        foreach (EnemyController enemy in detectedEnemies)
        {
            if (!currentlyDetected.Contains(enemy))
                enemy.SetFrozen(false);
        }

        detectedEnemies.Clear();
        detectedEnemies.UnionWith(currentlyDetected);
    }

    private bool IsEnemyInsideBeam(Transform enemyTarget)
    {
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

        return HasLineOfSightToEnemy(enemyTarget);
    }

    public bool HasLineOfSightToEnemy(Transform enemyTarget)
    {
        Vector3 toEnemy = enemyTarget.position - viewTransform.position;
        float distance = toEnemy.magnitude;

        if (distance <= 0f)
            return true;

        return !Physics.Raycast(
            viewTransform.position,
            toEnemy.normalized,
            distance,
            occlusionMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
