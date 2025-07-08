using UnityEngine;

public class GameplaySettingsPanel : SettingsPanel<GameplayConfig>
{
    public override string PersistenceKey => "GameplaySettings";
        
    protected override void OnApplyConfiguration(GameplayConfig config)
    {
        // Apply gameplay settings
        // This would integrate with your game systems
        // Example: DifficultyManager.Instance.SetDifficulty(config.difficulty);
            
        // Apply camera settings
        if (Camera.main != null)
        {
            Camera.main.fieldOfView = config.fieldOfView;
        }
    }
        
    protected override void RefreshUI()
    {
        // UI will be automatically refreshed through bindings
    }
}