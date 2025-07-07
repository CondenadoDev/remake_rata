using UnityEngine;

/// <summary>
/// Conecta PlayerStats con el sistema de eventos UI
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerStatsUIConnector : MonoBehaviour
{
    private PlayerStats playerStats;
    private bool isSubscribed = false;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        
        if (playerStats == null)
        {
            Debug.LogError("❌ [PlayerStatsUIConnector] PlayerStats component not found!");
            enabled = false;
            return;
        }
        
        Debug.Log("🔗 [PlayerStatsUIConnector] Initialized");
    }

    void OnEnable()
    {
        SubscribeToPlayerStats();
    }

    void OnDisable()
    {
        UnsubscribeFromPlayerStats();
    }

    void SubscribeToPlayerStats()
    {
        if (isSubscribed || playerStats == null) return;

        try
        {
            PlayerStats.OnHealthChanged += OnHealthChanged;
            PlayerStats.OnStaminaChanged += OnStaminaChanged;
            
            isSubscribed = true;
            Debug.Log("🔗 [PlayerStatsUIConnector] Subscribed to PlayerStats events");
            
            // Enviar valores iniciales
            SendInitialValues();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [PlayerStatsUIConnector] Failed to subscribe: {e.Message}");
        }
    }

    void UnsubscribeFromPlayerStats()
    {
        if (!isSubscribed) return;

        try
        {
            PlayerStats.OnHealthChanged -= OnHealthChanged;
            PlayerStats.OnStaminaChanged -= OnStaminaChanged;
            
            isSubscribed = false;
            Debug.Log("🔗 [PlayerStatsUIConnector] Unsubscribed from PlayerStats events");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [PlayerStatsUIConnector] Failed to unsubscribe: {e.Message}");
        }
    }

    void SendInitialValues()
    {
        if (playerStats != null)
        {
            // Enviar valores actuales al UI
            UIEvents.TriggerHealthChanged(playerStats.CurrentHealth, playerStats.MaxHealth);
            UIEvents.TriggerStaminaChanged(playerStats.CurrentStamina, playerStats.MaxStamina);
        }
    }

    void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UIEvents.TriggerHealthChanged(currentHealth, maxHealth);
    }

    void OnStaminaChanged(float currentStamina, float maxStamina)
    {
        UIEvents.TriggerStaminaChanged(currentStamina, maxStamina);
    }

    void OnDestroy()
    {
        UnsubscribeFromPlayerStats();
        Debug.Log("🗑️ [PlayerStatsUIConnector] Destroyed");
    }
}