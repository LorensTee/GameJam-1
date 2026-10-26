using UnityEngine;

public class VictoryController : MonoBehaviour
{
    [SerializeField] private HUDController hudController;

    private void OnTriggerEnter(Collider other)
    {
        // Direct victory UI invocation removed to prevent bypassing GameManager.
        // Victory is raised exclusively by PuzzleManager (via PlayerWonEvent) when reaching the active unlocked exit,
        // allowing GameManager to properly handle Game Over priority, enemy freezing, and input disabling.
    }
}