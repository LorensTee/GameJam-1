using UnityEngine;

public class VictoryController : MonoBehaviour
{
    [SerializeField] private HUDController hudController;

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger victory when the local player enters
        if (other.CompareTag("Player"))
        {
            if (hudController != null)
                hudController.ShowVictoryUI();
        }
    }
}