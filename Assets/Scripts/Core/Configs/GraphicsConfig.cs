using UnityEngine;

[CreateAssetMenu(fileName = "GraphicsConfig", menuName = "Game/Configs/Graphics Config")]
public class GraphicsConfig : ConfigBase
{
    [Header("🖥️ Display")]
    [UIOption("FPS Objetivo", UIControlType.Slider, 30f, 240f, "Display", 1)]
    public int targetFrameRate = 60;
    
    [UIOption("VSync", UIControlType.Toggle, "Display", 2)]
    public bool vSyncEnabled = true;
    
    [UIOption("Modo Pantalla", new string[] {"Ventana", "Pantalla Completa", "Ventana Sin Bordes"}, "Display", 3)]
    public CustomFullScreenMode fullScreenMode = CustomFullScreenMode.PantallaCompletaExclusiva;
    
    // Resolution se omite por ser Vector2Int complejo
    public Vector2Int resolution = new Vector2Int(1920, 1080);
    
    [Header("📊 Quality")]
    [UIOption("Nivel de Calidad", new string[] {"Muy Bajo", "Bajo", "Medio", "Alto", "Muy Alto", "Ultra"}, "Quality", 10)]
    public QualityLevel qualityLevel = QualityLevel.Alto;
    
    [UIOption("Calidad Texturas", new string[] {"Full Res", "Half Res", "Quarter Res", "Eighth Res"}, "Quality", 11)]
    public int textureQuality = 0;
    
    [UIOption("Calidad Sombras", UIControlType.Slider, 0f, 4f, "Quality", 12)]
    public int shadowQuality = 2;
    
    [UIOption("Anti-Aliasing", new string[] {"Desactivado", "2x", "4x", "8x"}, "Quality", 13)]
    public int antiAliasing = 4;
    
    [UIOption("Post-Procesado", UIControlType.Toggle, "Quality", 14)]
    public bool enablePostProcessing = true;
    
    [Header("🌅 Lighting")]
    [UIOption("Iluminación Tiempo Real", UIControlType.Toggle, "Lighting", 20)]
    public bool enableRealTimeLighting = true;
    
    [UIOption("Sombras", UIControlType.Toggle, "Lighting", 21)]
    public bool enableShadows = true;
    
    [UIOption("Resolución Sombras", new string[] {"Baja", "Media", "Alta", "Muy Alta"}, "Lighting", 22)]
    public CustomShadowResolution shadowResolution = CustomShadowResolution.Alta;
    
    [UIOption("Distancia Sombras", UIControlType.Slider, 10f, 200f, "Lighting", 23)]
    public float shadowDistance = 50f;
    
    [Header("🎭 Effects")]
    [UIOption("Partículas", UIControlType.Toggle, "Effects", 30)]
    public bool enableParticles = true;
    
    [UIOption("Max Partículas", UIControlType.Slider, 100f, 5000f, "Effects", 31)]
    public int maxParticles = 1000;
    
    [UIOption("Bloom", UIControlType.Toggle, "Effects", 32)]
    public bool enableBloom = true;
    
    [UIOption("Motion Blur", UIControlType.Toggle, "Effects", 33)]
    public bool enableMotionBlur = false;
    
    [UIOption("Ambient Occlusion", UIControlType.Toggle, "Effects", 34)]
    public bool enableAmbientOcclusion = true;
    
    [Header("🔧 Performance")]
    [UIOption("Occlusion Culling", UIControlType.Toggle, "Performance", 40)]
    public bool enableOcclusion = true;
    
    [UIOption("LOD Bias", UIControlType.Slider, 0.1f, 2f, "Performance", 41)]
    public float lodBias = 1f;
    
    [UIOption("Luces Pixel", UIControlType.Slider, 0f, 8f, "Performance", 42)]
    public int pixelLightCount = 4;
    
    [UIOption("GPU Instancing", UIControlType.Toggle, "Performance", 43)]
    public bool enableGPUInstancing = true;
    
    public override void ValidateValues()
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
    public override void ApplySettings()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
        
        // Convertir nuestro enum al enum de Unity
        UnityEngine.FullScreenMode unityFullScreenMode = (UnityEngine.FullScreenMode)fullScreenMode;
        Screen.SetResolution(resolution.x, resolution.y, unityFullScreenMode);
        
        QualitySettings.SetQualityLevel((int)qualityLevel);
        QualitySettings.globalTextureMipmapLimit = textureQuality;
        QualitySettings.shadows = enableShadows ? ShadowQuality.All : ShadowQuality.Disable;
        
        // Convertir nuestro enum al enum de Unity
        UnityEngine.ShadowResolution unityShadowResolution = (UnityEngine.ShadowResolution)shadowResolution;
        QualitySettings.shadowResolution = unityShadowResolution;
        QualitySettings.shadowDistance = shadowDistance;
        
        QualitySettings.antiAliasing = antiAliasing;
        QualitySettings.lodBias = lodBias;
        QualitySettings.pixelLightCount = pixelLightCount;
        
        DebugLog("Graphics settings applied");
        
    }
}