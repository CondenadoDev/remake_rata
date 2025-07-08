using UnityEngine;

[CreateAssetMenu(fileName = "GameplayConfig", menuName = "Game/Gameplay Config")]
public class GameplayConfig : ScriptableObject
{
    [Header("Difficulty")]
    public DifficultyLevel difficulty = DifficultyLevel.Normal;
    public float damageMultiplier = 1f;
    public float enemyHealthMultiplier = 1f;
    public float experienceMultiplier = 1f;
        
    [Header("Game Options")]
    public bool showTutorials = true;
    public bool autoSave = true;
    public float autoSaveInterval = 300f; // 5 minutes
    public bool showDamageNumbers = true;
    public bool showHealthBars = true;
        
    [Header("Camera")]
    public float cameraSensitivity = 1f;
    public bool invertYAxis = false;
    public bool cameraShake = true;
    public float fieldOfView = 60f;
        
    [Header("Controls")]
    public bool holdToSprint = true;
    public bool toggleCrouch = false;
    public bool autoAim = false;
}