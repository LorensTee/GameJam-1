using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private Slider healthBarSlider;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 1f;

    [SerializeField] private GridMover gridMover;
    [SerializeField] private Vector2Int respawnCell;

    private int lives;
    private bool isRespawning;
    private CharacterController characterController;

    private void Awake()
    {
        lives = maxLives;
        characterController = GetComponent<CharacterController>();
        gridMover = GetComponent<GridMover>();
    }

    private void Start()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxLives;
            healthBarSlider.value = lives;
        }
    }

    public void Decrement()
    {
        if (isRespawning || lives <= 0)
            return;

        lives--;

        if (healthBarSlider != null)
            healthBarSlider.value = lives;

        if (lives <= 0)
        {
            print("Game Over! No lives remaining.");
            StartCoroutine(RespawnSequence());
            return;
        }
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;

        if (characterController != null)
            characterController.enabled = false;

        if (gridMover != null)
        {
            gridMover.RespawnAt(respawnPoint.position);
        }
        else if (respawnPoint != null)
        {
            transform.SetPositionAndRotation(
                respawnPoint.position,
                respawnPoint.rotation
            );
        }

        if (characterController != null)
            characterController.enabled = true;

        isRespawning = false;

        yield break;
    }
}