using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Current State")]
    [SerializeField] private GameState currentState = GameState.Playing;
    public GameState CurrentState => currentState;

    [Header("Event Channels")]
    [SerializeField] private GameEventChannelSO playerDiedEvent;
    [SerializeField] private GameEventChannelSO playerWonEvent;
    [SerializeField] private GameEventChannelSO levelRestartedEvent;

    [Header("Dependencies")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private HUDController hudController;

    private bool deathReported;
    private bool victoryReported;

    private static bool isRestarting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (playerController == null)
            playerController = FindAnyObjectByType<PlayerController>();

        if (hudController == null)
            hudController = FindAnyObjectByType<HUDController>();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (isRestarting)
        {
            isRestarting = false;
            levelRestartedEvent?.RaiseEvent();
        }
    }

    private void OnEnable()
    {
        if (playerDiedEvent != null)
            playerDiedEvent.RegisterListener(HandlePlayerDied);

        if (playerWonEvent != null)
            playerWonEvent.RegisterListener(HandlePlayerWon);
    }

    private void OnDisable()
    {
        if (playerDiedEvent != null)
            playerDiedEvent.UnregisterListener(HandlePlayerDied);

        if (playerWonEvent != null)
            playerWonEvent.UnregisterListener(HandlePlayerWon);
    }

    private void HandlePlayerDied()
    {
        deathReported = true;
    }

    private void HandlePlayerWon()
    {
        victoryReported = true;
    }

    private void LateUpdate()
    {
        if (currentState != GameState.Playing)
            return;

        // Resolve reported outcomes together; death takes priority over victory
        if (deathReported)
        {
            EndGame(GameState.GameOver);
        }
        else if (victoryReported)
        {
            EndGame(GameState.Victory);
        }

        deathReported = false;
        victoryReported = false;
    }

    private void EndGame(GameState outcome)
    {
        currentState = outcome;

        // 1. Disable player input
        if (playerController != null)
        {
            playerController.SetInputEnabled(false);
        }

        // 2. Freeze all enemies and disable their controllers
        EnemyController[] enemies = FindObjectsByType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SetFrozen(true);
                enemy.enabled = false;
            }
        }

        // 3. Unlock and display mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 4. Pause the game
        Time.timeScale = 0f;

        // 5. Trigger appropriate UI
        if (outcome == GameState.GameOver)
        {
            if (hudController != null)
            {
                hudController.ShowRespawnUI();
            }
        }
        else if (outcome == GameState.Victory)
        {
            if (hudController != null)
            {
                hudController.ShowVictoryUI();
            }
        }
    }

    public void RestartLevel()
    {
        isRestarting = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
