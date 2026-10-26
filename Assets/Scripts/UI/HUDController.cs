using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject respawnUIPanel;
    [SerializeField] private Slider healthBarSlider;
    
    private void Start()
    {
        if (respawnUIPanel != null)
            respawnUIPanel.SetActive(false);
    }

    // Health bar
    public void SetMaxHealth(int maxLives)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxLives;
            healthBarSlider.value = maxLives;
        }
    }

    // Update health bar
    public void UpdateHealthUI(int currentLives)
    {
        if (healthBarSlider != null)
            healthBarSlider.value = currentLives;
    }

    // Shows death screen
    public void ShowRespawnUI()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (respawnUIPanel != null)
            respawnUIPanel.SetActive(true);
    }

    // Hides death screen
    public void HideRespawnUI()
    {
        if (respawnUIPanel != null)
            respawnUIPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}