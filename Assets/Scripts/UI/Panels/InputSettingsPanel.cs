using UnityEngine;

public class InputSettingsPanel : SettingsPanel<InputConfig>
{
    public override string PersistenceKey => "InputSettings";

    protected override void OnApplyConfiguration(InputConfig config)
    {
        // Apply input configuration if needed
        // For now, InputManager reads values directly from ConfigurationManager
        // so we just validate values
        config.ValidateValues();
    }

    protected override void RefreshUI()
    {
        // UI elements are updated via bindings
    }
}
