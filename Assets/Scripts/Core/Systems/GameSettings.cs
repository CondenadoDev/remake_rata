// Assets/Scripts/Core/GameSettings.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Core Settings")]
public class GameSettings : ScriptableObject
{
    [Header("🎮 Gameplay")]
    public float masterTimeScale = 1f;
    public bool debugMode = false;
    
    [Header("⚡ Performance")]
    public int targetFrameRate = 60;
    public bool vSyncEnabled = true;
    
    [Header("🔊 Audio")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    
    [Header("🎯 Combat")]
    public float globalDamageMultiplier = 1f;
    public float invincibilityTime = 0.5f;
}