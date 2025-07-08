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

    public void ResetToDefaults()
    {
        resolutionIndex = -1;
        fullScreenMode = FullScreenMode.FullScreenWindow;
        targetFrameRate = 60;
        vSync = true;
        qualityLevel = 3;
        antiAliasing = 2;
        anisotropicFiltering = AnisotropicFiltering.Enable;
        shadows = true;
        shadowQuality = ShadowQuality.All;
        shadowResolution = ShadowResolution.High;
        shadowDistance = 100f;
        postProcessing = true;
        bloom = true;
        ambientOcclusion = true;
        motionBlur = false;
        depthOfField = true;
        renderScale = 1f;
        textureQuality = 0;
        lodBias = 1f;
    }
    public void ValidateValues()
    {
        qualityLevel = Mathf.Clamp(qualityLevel, 0, 5);
        antiAliasing = Mathf.Clamp(antiAliasing, 0, 8);
        shadowDistance = Mathf.Max(0f, shadowDistance);
        renderScale = Mathf.Clamp(renderScale, 0.5f, 2f);
        lodBias = Mathf.Max(0.1f, lodBias);
        textureQuality = Mathf.Clamp(textureQuality, 0, 2);
    }

    public void ApplySettings()
    {
        // Resolución y pantalla
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            var res = resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, fullScreenMode);
        }
        else
        {
            // Fallback a la resolución actual si el índice no es válido
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullScreenMode);
        }

        // Calidad gráfica general
        QualitySettings.SetQualityLevel(qualityLevel, true);

        // VSync
        QualitySettings.vSyncCount = vSync ? 1 : 0;

        // Antialiasing
        QualitySettings.antiAliasing = antiAliasing;

        // Anisotropic Filtering
        QualitySettings.anisotropicFiltering = anisotropicFiltering;

        // Sombras
        QualitySettings.shadows = shadows ? ShadowQuality.All : ShadowQuality.Disable;
        QualitySettings.shadowResolution = shadowResolution;
        QualitySettings.shadowDistance = shadowDistance;

        // LOD
        QualitySettings.lodBias = lodBias;

        // Texture Quality
        QualitySettings.globalTextureMipmapLimit = textureQuality;

        // FPS
        Application.targetFrameRate = targetFrameRate;

        // Aquí puedes agregar más settings si tu proyecto lo requiere.
    }
}
