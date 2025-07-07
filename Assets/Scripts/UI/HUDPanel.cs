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
    private bool isSubscribedToEvents = false;
    
    protected override void OnInitialize()
    {
        panelID = "HUD";
        startVisible = true;
        useScaleAnimation = false;
        
        SetupInitialState();
        LogDebug("HUD Panel initialized");
    }
    
    void OnEnable()
    {
        SubscribeToEvents();
    }
    
    void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    void SubscribeToEvents()
    {
        if (isSubscribedToEvents) return;
        
        try
        {
            UIEvents.OnHealthChanged += UpdateHealthBar;
            UIEvents.OnStaminaChanged += UpdateStaminaBar;
            UIEvents.OnComboChanged += ShowComboCount;
            UIEvents.OnShowInteractionPrompt += ShowInteractionPrompt;
            UIEvents.OnHideInteractionPrompt += HideInteractionPrompt;
            
            isSubscribedToEvents = true;
            LogDebug("Subscribed to UI events");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to subscribe to events: {e.Message}");
        }
    }
    
    void UnsubscribeFromEvents()
    {
        if (!isSubscribedToEvents) return;
        
        try
        {
            UIEvents.OnHealthChanged -= UpdateHealthBar;
            UIEvents.OnStaminaChanged -= UpdateStaminaBar;
            UIEvents.OnComboChanged -= ShowComboCount;
            UIEvents.OnShowInteractionPrompt -= ShowInteractionPrompt;
            UIEvents.OnHideInteractionPrompt -= HideInteractionPrompt;
            
            isSubscribedToEvents = false;
            LogDebug("Unsubscribed from UI events");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to unsubscribe from events: {e.Message}");
        }
    }
    
    void SetupInitialState()
    {
        try
        {
            if (comboPanel != null)
                comboPanel.SetActive(false);
                
            if (damageOverlay != null)
            {
                damageOverlay.color = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);
            }
            
            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
            
            // Inicializar barras con valores por defecto
            if (healthBar != null)
                healthBar.value = 1f;
            if (staminaBar != null)
                staminaBar.value = 1f;
                
            LogDebug("Initial state setup completed");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to setup initial state: {e.Message}");
        }
    }
    
    void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        try
        {
            if (healthBar != null && maxHealth > 0)
            {
                float normalizedHealth = currentHealth / maxHealth;
                healthBar.value = normalizedHealth;
                LogDebug($"Health bar updated: {normalizedHealth:F2}");
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
        catch (System.Exception e)
        {
            LogError($"Failed to update health bar: {e.Message}");
        }
    }
    
    void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        try
        {
            if (staminaBar != null && maxStamina > 0)
            {
                float normalizedStamina = currentStamina / maxStamina;
                staminaBar.value = normalizedStamina;
                LogDebug($"Stamina bar updated: {normalizedStamina:F2}");
            }
            
            if (staminaText != null)
            {
                staminaText.text = $"{Mathf.Ceil(currentStamina)}/{Mathf.Ceil(maxStamina)}";
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to update stamina bar: {e.Message}");
        }
    }
    
    public void ShowComboCount(int comboCount)
    {
        try
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
                
                LogDebug($"Combo panel shown: x{comboCount}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to show combo count: {e.Message}");
        }
    }
    
    IEnumerator HideComboPanelAfterDelay()
    {
        yield return new WaitForSeconds(comboPanelHideDelay);
        
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
            LogDebug("Combo panel hidden");
        }
    }
    
    void ShowDamageEffect()
    {
        try
        {
            if (damageOverlay != null)
            {
                if (damageFlashCoroutine != null)
                    StopCoroutine(damageFlashCoroutine);
                    
                damageFlashCoroutine = StartCoroutine(DamageFlashEffect());
                LogDebug("Damage effect triggered");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to show damage effect: {e.Message}");
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
        LogDebug("Damage flash effect completed");
    }
    
    public void ShowInteractionPrompt(string text)
    {
        try
        {
            if (interactionPrompt != null && !string.IsNullOrEmpty(text))
            {
                interactionPrompt.text = text;
                interactionPrompt.gameObject.SetActive(true);
                LogDebug($"Interaction prompt shown: {text}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to show interaction prompt: {e.Message}");
        }
    }
    
    public void HideInteractionPrompt()
    {
        try
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.gameObject.SetActive(false);
                LogDebug("Interaction prompt hidden");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to hide interaction prompt: {e.Message}");
        }
    }
    
    protected override void OnShow()
    {
        // Habilitar cursor del juego si existe
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(true);
            LogDebug("Game cursor enabled");
        }
    }
    
    protected override void OnHide()
    {
        // Deshabilitar cursor del juego
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(false);
            LogDebug("Game cursor disabled");
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
        LogDebug("HUD Panel destroyed");
    }
}