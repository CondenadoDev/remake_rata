// HUDPanel.cs - Panel principal del HUD
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDPanel : UIPanel
{
    [Header("🎮 HUD Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI interactionPrompt;
    [SerializeField] private Image crosshair;
    
    [Header("📊 Stats Display")]
    [SerializeField] private TextMeshProUGUI comboCountText;
    [SerializeField] private GameObject comboPanel;
    [SerializeField] private float comboPanelHideDelay = 2f;
    
    [Header("⚡ Effects")]
    [SerializeField] private Image damageOverlay;
    [SerializeField] private float damageFlashDuration = 0.3f;
    [SerializeField] private Color damageColor = Color.red;
    
    private Coroutine comboPanelCoroutine;
    private Coroutine damageFlashCoroutine;
    
    protected override void OnInitialize()
    {
        panelID = "HUD";
        startVisible = true;
        useScaleAnimation = false;
        
        // Suscribirse a eventos del jugador
        PlayerStats.OnHealthChanged += UpdateHealthBar;
        PlayerStats.OnStaminaChanged += UpdateStaminaBar;
        
        // Configurar elementos iniciales
        SetupInitialState();
    }
    
    void SetupInitialState()
    {
        if (comboPanel != null)
            comboPanel.SetActive(false);
            
        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);
        }
        
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }
    
    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)}/{Mathf.Ceil(maxHealth)}";
        }
        
        // Efecto de daño
        if (currentHealth < maxHealth)
        {
            ShowDamageEffect();
        }
    }
    
    void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
        
        if (staminaText != null)
        {
            staminaText.text = $"{Mathf.Ceil(currentStamina)}/{Mathf.Ceil(maxStamina)}";
        }
    }
    
    public void ShowComboCount(int comboCount)
    {
        if (comboPanel != null && comboCountText != null)
        {
            comboPanel.SetActive(true);
            comboCountText.text = $"COMBO x{comboCount}";
            
            // Cancelar el hide anterior
            if (comboPanelCoroutine != null)
                StopCoroutine(comboPanelCoroutine);
                
            // Programar hide
            comboPanelCoroutine = StartCoroutine(HideComboPanelAfterDelay());
        }
    }
    
    IEnumerator HideComboPanelAfterDelay()
    {
        yield return new WaitForSeconds(comboPanelHideDelay);
        
        if (comboPanel != null)
            comboPanel.SetActive(false);
    }
    
    void ShowDamageEffect()
    {
        if (damageOverlay != null)
        {
            if (damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);
                
            damageFlashCoroutine = StartCoroutine(DamageFlashEffect());
        }
    }
    
    IEnumerator DamageFlashEffect()
    {
        float elapsed = 0f;
        Color startColor = new Color(damageColor.r, damageColor.g, damageColor.b, 0.3f);
        Color endColor = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);
        
        while (elapsed < damageFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / damageFlashDuration;
            
            damageOverlay.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        damageOverlay.color = endColor;
    }
    
    public void ShowInteractionPrompt(string text)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.text = text;
            interactionPrompt.gameObject.SetActive(true);
        }
    }
    
    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }
    
    protected override void OnShow()
    {
        // Habilitar cursor del juego si existe
        if (crosshair != null)
            crosshair.gameObject.SetActive(true);
    }
    
    protected override void OnHide()
    {
        // Deshabilitar cursor del juego
        if (crosshair != null)
            crosshair.gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        PlayerStats.OnHealthChanged -= UpdateHealthBar;
        PlayerStats.OnStaminaChanged -= UpdateStaminaBar;
    }
}
