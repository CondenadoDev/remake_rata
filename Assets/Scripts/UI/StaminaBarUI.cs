using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [HideInInspector] public Slider staminaSlider;
    [HideInInspector] public TextMeshProUGUI staminaText;
    
    void OnEnable()
    {
        PlayerStats.OnStaminaChanged += UpdateStaminaBar;
    }
    
    void OnDisable()
    {
        PlayerStats.OnStaminaChanged -= UpdateStaminaBar;
    }
    
    void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaSlider != null && maxStamina > 0)
        {
            staminaSlider.value = currentStamina / maxStamina;
        }
        
        if (staminaText != null)
        {
            staminaText.text = $"{Mathf.Ceil(currentStamina)} / {Mathf.Ceil(maxStamina)}";
        }
    }
}