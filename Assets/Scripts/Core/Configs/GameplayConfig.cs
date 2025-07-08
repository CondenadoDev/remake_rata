using UnityEngine;

[CreateAssetMenu(fileName = "GameplayConfig", menuName = "Game/Configs/Gameplay Config")]

public class GameplayConfig : ConfigurationBase
{
    [Header("⏱️ Time")]
    public float globalTimeScale = 1f;
    public bool allowPausing = true;
    public bool pauseOnFocusLoss = true;
    
    [Header("💀 Death & Respawn")]
    public float respawnDelay = 2f;
    public bool loseProgressOnDeath = false;
    public float deathPenaltyMultiplier = 0.1f;
    
    [Header("💰 Economy")]
    public float globalDamageMultiplier = 1f;
    public float experienceMultiplier = 1f;
    public float lootDropMultiplier = 1f;
    public float goldMultiplier = 1f;
    
    [Header("🎯 Difficulty")]
    public DifficultyLevel defaultDifficulty = DifficultyLevel.Normal;
    public bool allowDifficultyChange = true;
    public float difficultyScaling = 1f;
    
    [Header("💾 Save System")]
    public bool autoSaveEnabled = true;
    public float autoSaveInterval = 300f; // 5 minutos
    public int maxSaveSlots = 3;
    
    [Header("🔧 Debug")]
    public bool showFPS = false;
    public bool showDebugInfo = false;
    public bool godModeAvailable = false;
    public bool unlockAllAreas = false;

    public override void ValidateValues()
    {
        globalTimeScale = Mathf.Clamp(globalTimeScale, 0.1f, 3f);
        respawnDelay = Mathf.Max(0f, respawnDelay);
        
        globalDamageMultiplier = Mathf.Max(0.1f, globalDamageMultiplier);
        experienceMultiplier = Mathf.Max(0.1f, experienceMultiplier);
        lootDropMultiplier = Mathf.Max(0.1f, lootDropMultiplier);
        
        autoSaveInterval = Mathf.Max(60f, autoSaveInterval);
        maxSaveSlots = Mathf.Clamp(maxSaveSlots, 1, 10);
    }
}

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
    Nightmare
}