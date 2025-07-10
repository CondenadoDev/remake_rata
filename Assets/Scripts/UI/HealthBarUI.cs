using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [HideInInspector] public Slider healthSlider;
    [HideInInspector] public TextMeshProUGUI healthText;
    
    void OnEnable()
    {
        PlayerStats.OnHealthChanged += UpdateHealthBar;
    }
    
    void OnDisable()
    {
        PlayerStats.OnHealthChanged -= UpdateHealthBar;
    }
    
    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null && maxHealth > 0)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)} / {Mathf.Ceil(maxHealth)}";
        }
    }
}