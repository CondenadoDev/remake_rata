using UnityEngine;
[CreateAssetMenu(fileName = "GraphicsConfig", menuName = "Game/Configs/Graphics Config")]
public class GraphicsConfig : ConfigurationBase
{
    [Header("🖥️ Display")]
    public int targetFrameRate = 60;
    public bool vSyncEnabled = true;
    public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;
    public Vector2Int resolution = new Vector2Int(1920, 1080);
    
    [Header("📊 Quality")]
    public QualityLevel qualityLevel = QualityLevel.High;
    public int textureQuality = 0; // 0 = Full Res
    public int shadowQuality = 2; // 0-4
    public int antiAliasing = 4; // 0, 2, 4, 8
    public bool enablePostProcessing = true;
    
    [Header("🌅 Lighting")]
    public bool enableRealTimeLighting = true;
    public bool enableShadows = true;
    public ShadowResolution shadowResolution = ShadowResolution.High;
    public float shadowDistance = 50f;
    
    [Header("🎭 Effects")]
    public bool enableParticles = true;
    public int maxParticles = 1000;
    public bool enableBloom = true;
    public bool enableMotionBlur = false;
    public bool enableAmbientOcclusion = true;
    
    [Header("🔧 Performance")]
    public bool enableOcclusion = true;
    public float lodBias = 1f;
    public int pixelLightCount = 4;
    public bool enableGPUInstancing = true;
    
    protected override void ValidateValues()
    {
        targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 240);
        resolution.x = Mathf.Max(640, resolution.x);
        resolution.y = Mathf.Max(480, resolution.y);
        
        textureQuality = Mathf.Clamp(textureQuality, 0, 3);
        shadowQuality = Mathf.Clamp(shadowQuality, 0, 4);
        
        shadowDistance = Mathf.Max(10f, shadowDistance);
        maxParticles = Mathf.Max(100, maxParticles);
        lodBias = Mathf.Clamp(lodBias, 0.1f, 2f);
        pixelLightCount = Mathf.Max(0, pixelLightCount);
    }
    
    public void ApplySettings()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
        Screen.SetResolution(resolution.x, resolution.y, fullScreenMode);
        
        QualitySettings.SetQualityLevel((int)qualityLevel);
        QualitySettings.globalTextureMipmapLimit = textureQuality;
        QualitySettings.shadows = enableShadows ? ShadowQuality.All : ShadowQuality.Disable;
        QualitySettings.shadowResolution = shadowResolution;
        QualitySettings.shadowDistance = shadowDistance;
        
        QualitySettings.antiAliasing = antiAliasing;
        QualitySettings.lodBias = lodBias;
        QualitySettings.pixelLightCount = pixelLightCount;
        
        DebugLog("Graphics settings applied");
    }
}

public enum QualityLevel
{
    VeryLow = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    VeryHigh = 4,
    Ultra = 5
}