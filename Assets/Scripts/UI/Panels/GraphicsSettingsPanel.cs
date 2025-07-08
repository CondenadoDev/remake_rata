using UnityEngine;

public class GraphicsSettingsPanel : SettingsPanel<GraphicsConfig>
{
    public override string PersistenceKey => "GraphicsSettings";
        
    protected override void OnApplyConfiguration(GraphicsConfig config)
    {
        // Apply resolution
        if (config.resolutionIndex >= 0 && config.resolutionIndex < Screen.resolutions.Length)
        {
            Resolution res = Screen.resolutions[config.resolutionIndex];
            Screen.SetResolution(res.width, res.height, config.fullScreenMode);
        }
            
        // Apply quality settings
        QualitySettings.SetQualityLevel(config.qualityLevel);
        QualitySettings.vSyncCount = config.vSync ? 1 : 0;
        Application.targetFrameRate = config.targetFrameRate;
            
        // Apply shadows
        QualitySettings.shadows = config.shadows ? config.shadowQuality : ShadowQuality.Disable;
        QualitySettings.shadowResolution = config.shadowResolution;
        QualitySettings.shadowDistance = config.shadowDistance;
            
        // Apply texture quality
        QualitySettings.masterTextureLimit = config.textureQuality;
        QualitySettings.lodBias = config.lodBias;
        QualitySettings.anisotropicFiltering = config.anisotropicFiltering;
        QualitySettings.antiAliasing = config.antiAliasing;
    }
        
    protected override void RefreshUI()
    {
        // UI will be automatically refreshed through bindings
    }
}