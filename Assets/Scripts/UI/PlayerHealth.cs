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
    [SerializeField] private float respawnDelay = 0.5f;

    [Header("UI References")]
    [SerializeField] private GameObject respawnUIPanel; // Drag your UI Canvas/Panel here

    [Header("Grid Movement")]
    [SerializeField] private GridMover gridMover;
    [SerializeField] private Vector2Int respawnCell;

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
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxLives;
            healthBarSlider.value = lives;
        }

        if (respawnUIPanel != null)
            respawnUIPanel.SetActive(false);
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
            ShowRespawnUI();
        }
    }

    private void ShowRespawnUI()
    {
        isRespawning = true;
        
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (respawnUIPanel != null)
            respawnUIPanel.SetActive(true);
    }

    public void OnRespawnButtonPressed()
    {
        if (respawnUIPanel != null)
            respawnUIPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {

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
        if (healthBarSlider != null)
            healthBarSlider.value = lives;

        yield return new WaitForSecondsRealtime(respawnDelay);
        
        isRespawning = false;
    }
}