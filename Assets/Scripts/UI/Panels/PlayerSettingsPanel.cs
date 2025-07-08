using UnityEngine;

public class PlayerSettingsPanel : SettingsPanel<PlayerConfig>
{
    public override string PersistenceKey => "PlayerSettings";

    protected override void OnApplyConfiguration(PlayerConfig config)
    {
        // Apply player configuration if needed
        config.ValidateValues();
    }

    protected override void RefreshUI()
    {
        // UI elements are updated via bindings
    }
}
