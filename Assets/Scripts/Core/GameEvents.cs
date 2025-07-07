// Assets/Scripts/Core/GameEvents.cs
using UnityEngine;

public static class GameEvents
{
    // 💚 Health Events
    public static event System.Action<float> OnPlayerHealthChanged;
    public static event System.Action OnPlayerDied;
    public static event System.Action<float> OnPlayerHealed;
    
    // ⚔️ Combat Events  
    public static event System.Action<GameObject> OnEnemyKilled;
    public static event System.Action<float> OnPlayerAttack;
    public static event System.Action<GameObject, float> OnDamageDealt;
    
    // 🏰 Level Events
    public static event System.Action OnDungeonGenerated;
    public static event System.Action<string> OnSceneLoaded;
    
    // 🎵 Audio Events
    public static event System.Action<AudioClip> OnPlaySFX;
    public static event System.Action<AudioClip> OnPlayMusic;
    
    // Methods to trigger events safely
    public static void TriggerPlayerHealthChanged(float newHealth)
    {
        OnPlayerHealthChanged?.Invoke(newHealth);
    }

    public static void TriggerPlayerDied()
    {
        OnPlayerDied?.Invoke();
    }

    public static void TriggerEnemyKilled(GameObject enemy)
    {
        OnEnemyKilled?.Invoke(enemy);
    }

    public static void TriggerDungeonGenerated()
    {
        OnDungeonGenerated?.Invoke();
    }
}