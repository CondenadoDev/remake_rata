using UnityEngine;

[CreateAssetMenu(fileName = "GameplayConfig", menuName = "Game/Configs/Gameplay Config")]
public class GameplayConfig : ConfigBase
{
    [Header("⏱️ Time")]
    [UIOption("Escala Tiempo Global", UIControlType.Slider, 0.1f, 3f, "Time", 1)]
    public float globalTimeScale = 1f;
    
    [UIOption("Permitir Pausa", UIControlType.Toggle, "Time", 2)]
    public bool allowPausing = true;
    
    [UIOption("Pausar al Perder Foco", UIControlType.Toggle, "Time", 3)]
    public bool pauseOnFocusLoss = true;
    
    [Header("💀 Death & Respawn")]
    [UIOption("Delay Respawn", UIControlType.Slider, 0f, 10f, "Death", 10)]
    public float respawnDelay = 2f;
    
    [UIOption("Perder Progreso al Morir", UIControlType.Toggle, "Death", 11)]
    public bool loseProgressOnDeath = false;
    
    [UIOption("Penalización Muerte", UIControlType.Slider, 0f, 1f, "Death", 12)]
    public float deathPenaltyMultiplier = 0.1f;
    
    [Header("💰 Economy")]
    [UIOption("Multiplicador Daño", UIControlType.Slider, 0.1f, 5f, "Economy", 20)]
    public float globalDamageMultiplier = 1f;
    
    [UIOption("Multiplicador EXP", UIControlType.Slider, 0.1f, 5f, "Economy", 21)]
    public float experienceMultiplier = 1f;
    
    [UIOption("Multiplicador Loot", UIControlType.Slider, 0.1f, 5f, "Economy", 22)]
    public float lootDropMultiplier = 1f;
    
    [UIOption("Multiplicador Oro", UIControlType.Slider, 0.1f, 5f, "Economy", 23)]
    public float goldMultiplier = 1f;
    
    [Header("🎯 Difficulty")]
    [UIOption("Dificultad Por Defecto", new string[] {"Fácil", "Normal", "Difícil", "Pesadilla"}, "Difficulty", 30)]
    public DifficultyLevel defaultDifficulty = DifficultyLevel.Normal;
    
    [UIOption("Cambio Dificultad", UIControlType.Toggle, "Difficulty", 31)]
    public bool allowDifficultyChange = true;
    
    [UIOption("Escalado Dificultad", UIControlType.Slider, 0.5f, 2f, "Difficulty", 32)]
    public float difficultyScaling = 1f;
    
    [Header("💾 Save System")]
    [UIOption("Auto-Guardado", UIControlType.Toggle, "Save", 40)]
    public bool autoSaveEnabled = true;
    
    [UIOption("Intervalo Auto-Guardado", UIControlType.Slider, 60f, 1800f, "Save", 41)]
    public float autoSaveInterval = 300f;
    
    [UIOption("Max Slots Guardado", UIControlType.Slider, 1f, 10f, "Save", 42)]
    public int maxSaveSlots = 3;
    
    [Header("🔧 Debug")]
    [UIOption("Mostrar FPS", UIControlType.Toggle, "Debug", 50)]
    public bool showFPS = false;
    
    [UIOption("Info Debug", UIControlType.Toggle, "Debug", 51)]
    public bool showDebugInfo = false;
    
    [UIOption("Modo Dios", UIControlType.Toggle, "Debug", 52)]
    public bool godModeAvailable = false;
    
    [UIOption("Desbloquear Áreas", UIControlType.Toggle, "Debug", 53)]
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