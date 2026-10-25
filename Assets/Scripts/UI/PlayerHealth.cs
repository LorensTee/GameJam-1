using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] Slider healthBarSlider;
    
    // [SerializeField] Player player;

    void Start()
    {
        // healthBarSlider.maxValue = player.base_hp;
    }

    // Update is called once per frame
    void Update()
    {
        // healthBarSlider.value = player.hp;
    }
}
