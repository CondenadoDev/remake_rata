using UnityEngine;

/// <summary>
/// Sistema centralizado de eventos de UI para evitar dependencias directas
/// </summary>
public static class UIEvents
{
    // 💚 Health & Stats Events
    public static event System.Action<float, float> OnHealthChanged;
    public static event System.Action<float, float> OnStaminaChanged;
    public static event System.Action<int> OnComboChanged;
    
    // 🎮 Game State Events
    public static event System.Action OnGamePaused;
    public static event System.Action OnGameResumed;
    public static event System.Action OnPlayerDied;
    
    // 🎵 Audio Events
    public static event System.Action<AudioClip> OnPlayUISound;
    
    // 💬 Interaction Events
    public static event System.Action<string> OnShowInteractionPrompt;
    public static event System.Action OnHideInteractionPrompt;
    
    // Methods to trigger events safely
    public static void TriggerHealthChanged(float current, float max)
    {
        OnHealthChanged?.Invoke(current, max);
        Debug.Log($"🎮 [UIEvents] Health changed: {current:F1}/{max:F1}");
    }
    
    public static void TriggerStaminaChanged(float current, float max)
    {
        OnStaminaChanged?.Invoke(current, max);
        Debug.Log($"🎮 [UIEvents] Stamina changed: {current:F1}/{max:F1}");
    }
    
    public static void TriggerComboChanged(int comboCount)
    {
        OnComboChanged?.Invoke(comboCount);
        Debug.Log($"🎮 [UIEvents] Combo changed: {comboCount}");
    }
    
    public static void TriggerGamePaused()
    {
        OnGamePaused?.Invoke();
        Debug.Log("⏸️ [UIEvents] Game paused");
    }
    
    public static void TriggerGameResumed()
    {
        OnGameResumed?.Invoke();
        Debug.Log("▶️ [UIEvents] Game resumed");
    }
    
    public static void TriggerShowInteractionPrompt(string text)
    {
        OnShowInteractionPrompt?.Invoke(text);
        Debug.Log($"💬 [UIEvents] Show interaction: {text}");
    }
    
    public static void TriggerHideInteractionPrompt()
    {
        OnHideInteractionPrompt?.Invoke();
        Debug.Log("💬 [UIEvents] Hide interaction prompt");
    }
    
    public static void TriggerPlayUISound(AudioClip clip)
    {
        OnPlayUISound?.Invoke(clip);
        if (clip != null)
            Debug.Log($"🔊 [UIEvents] Play UI sound: {clip.name}");
    }
}