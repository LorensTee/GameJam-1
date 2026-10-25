using UnityEngine;

[CreateAssetMenu(
    fileName = "GameSettings",
    menuName = "Game/Sokoban/Game Settings"
)]
public sealed class GameSettingsSO : ScriptableObject
{
    [Header("Player")]
    [SerializeField, Min(1)] private int startingHealth = 3;
    [SerializeField, Min(0f)] private float damageInvulnerabilityDuration = 1f;

    [Header("Enemy")]
    [SerializeField, Min(0f)] private float enemyRecoveryDuration = 2f;
    [SerializeField, Min(0f)] private float enemyDetectionRadius = 6f;
    [SerializeField, Min(0f)] private float enemyMoveSpeed = 1.5f;
    [SerializeField, Min(0f)] private float enemyRoamWaitDuration = 0.5f;

    [Header("Flashlight")]
    [SerializeField, Min(0.1f)] private float flashlightRange = 8f;
    [SerializeField, Range(1f, 179f)] private float flashlightBeamAngle = 45f;

    [Header("Grid Movement")]
    [SerializeField, Min(0.01f)] private float moveDuration = 0.2f;

    public int StartingHealth => startingHealth;
    public float DamageInvulnerabilityDuration => damageInvulnerabilityDuration;
    public float EnemyRecoveryDuration => enemyRecoveryDuration;
    public float EnemyDetectionRadius => enemyDetectionRadius;
    public float EnemyMoveSpeed => enemyMoveSpeed;
    public float EnemyRoamWaitDuration => enemyRoamWaitDuration;
    public float FlashlightRange => flashlightRange;
    public float FlashlightBeamAngle => flashlightBeamAngle;
    public float MoveDuration => moveDuration;
}
