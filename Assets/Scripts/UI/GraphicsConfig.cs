using UnityEngine;

[CreateAssetMenu(fileName = "GraphicsConfig", menuName = "Game/Graphics Config")]
public class GraphicsConfig : ScriptableObject
{
    [Header("Display")]
    public int resolutionIndex = -1;
    public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;
    public int targetFrameRate = 60;
    public bool vSync = true;
        
    [Header("Quality")]
    [Range(0, 5)]
    public int qualityLevel = 3;
    public int antiAliasing = 2;
    public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Enable;
        
    [Header("Effects")]
    public bool shadows = true;
    public ShadowQuality shadowQuality = ShadowQuality.All;
    public ShadowResolution shadowResolution = ShadowResolution.High;
    public float shadowDistance = 100f;
        
    [Header("Post Processing")]
    public bool postProcessing = true;
    public bool bloom = true;
    public bool ambientOcclusion = true;
    public bool motionBlur = false;
    public bool depthOfField = true;
        
    [Header("Performance")]
    public float renderScale = 1f;
    public int textureQuality = 0; // 0 = Full, 1 = Half, 2 = Quarter
    public float lodBias = 1f;
}