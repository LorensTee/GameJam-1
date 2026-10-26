using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 3;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 0.5f;

    [Header("Dependencies")]
    [SerializeField] private HUDController hudController;
    [SerializeField] private GridMover gridMover;

    private int lives;
    private bool isRespawning;
    private CharacterController characterController;

    private void Awake()
    {
        lives = maxLives;
        characterController = GetComponent<CharacterController>();
        
        if (gridMover == null)
            gridMover = GetComponent<GridMover>();
    }

    private void Start()
    {
        if (hudController != null)
            hudController.SetMaxHealth(maxLives);
    }

    public void Decrement()
    {
        if (isRespawning || lives <= 0)
            return;

        lives--;

        if (hudController != null)
            hudController.UpdateHealthUI(lives);

        if (lives <= 0)
        {
            if (hudController != null)
                hudController.ShowRespawnUI();
        }
    }

    public void OnRespawnButtonPressed()
    {
        if (hudController != null)
            hudController.HideRespawnUI();

        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;

        if (characterController != null)
            characterController.enabled = false;

        if (gridMover != null && respawnPoint != null)
        {
            gridMover.RespawnAt(respawnPoint.position);
        }
        else if (respawnPoint != null)
        {
            transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
        }

        if (characterController != null)
            characterController.enabled = true;

        lives = maxLives;
        if (hudController != null)
            hudController.UpdateHealthUI(lives);

        yield return new WaitForSecondsRealtime(respawnDelay);

        isRespawning = false;
    }
}