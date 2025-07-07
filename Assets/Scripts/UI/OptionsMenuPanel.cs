using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenuPanel : UIPanel
{
    [Header("🔊 Audio Options")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    
    [Header("🖥️ Graphics Options")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;
    
    [Header("🎮 Input Options")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertMouseYToggle;
    [SerializeField] private TextMeshProUGUI mouseSensitivityText;
    
    [Header("🔙 Navigation")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button resetToDefaultsButton;
    
    [Header("🔙 Category")]
    [SerializeField] GameObject categorySelectorPanel;
    [SerializeField] GameObject audioPanel;
    [SerializeField] GameObject graphicsPanel;
    [SerializeField] GameObject gameplayPanel;
    [SerializeField] GameObject controlsPanel;

    protected override void OnInitialize()
    {
        panelID = "OptionsMenu";
        startVisible = false;
        useScaleAnimation = true;
        blockGameInput = true;
        
        SetupControls();
        LoadCurrentSettings();
    }
    
    void SetupControls()
    {
        // Audio sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        // Graphics controls
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        
        // Input controls
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        if (invertMouseYToggle != null)
            invertMouseYToggle.onValueChanged.AddListener(OnInvertMouseYChanged);
        
        // Buttons
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
        if (resetToDefaultsButton != null)
            resetToDefaultsButton.onClick.AddListener(ResetToDefaults);
        
        // Populate dropdowns
        PopulateQualityDropdown();
        PopulateResolutionDropdown();
    }
    
    void LoadCurrentSettings()
    {
        if (AudioManager.Instance != null)
        {
            var volumeSettings = AudioManager.Instance.GetVolumeSettings();
            
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = volumeSettings.masterVolume;
                UpdateVolumeText(masterVolumeText, volumeSettings.masterVolume);
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = volumeSettings.musicVolume;
                UpdateVolumeText(musicVolumeText, volumeSettings.musicVolume);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = volumeSettings.sfxVolume;
                UpdateVolumeText(sfxVolumeText, volumeSettings.sfxVolume);
            }
        }
        
        if (ConfigurationManager.Graphics != null)
        {
            var graphicsConfig = ConfigurationManager.Graphics;
            
            if (qualityDropdown != null)
                qualityDropdown.value = (int)graphicsConfig.qualityLevel;
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = graphicsConfig.fullScreenMode == FullScreenMode.FullScreenWindow;
            if (vSyncToggle != null)
                vSyncToggle.isOn = graphicsConfig.vSyncEnabled;
        }
        
        if (ConfigurationManager.Input != null)
        {
            var inputConfig = ConfigurationManager.Input;
            
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.value = inputConfig.mouseSensitivity;
                UpdateSensitivityText(inputConfig.mouseSensitivity);
            }
            if (invertMouseYToggle != null)
                invertMouseYToggle.isOn = inputConfig.invertMouseY;
        }
    }
    
    // Audio callbacks
    void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
        UpdateVolumeText(masterVolumeText, value);
    }
    
    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
        UpdateVolumeText(musicVolumeText, value);
    }
    
    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
        UpdateVolumeText(sfxVolumeText, value);
    }
    
    // Graphics callbacks
    void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        if (ConfigurationManager.Graphics != null)
        {
            ConfigurationManager.Graphics.qualityLevel = (QualityLevel)qualityIndex;
        }
    }
    
    void OnResolutionChanged(int resolutionIndex)
    {
        // Implementar cambio de resolución
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
    
    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        if (ConfigurationManager.Graphics != null)
        {
            ConfigurationManager.Graphics.fullScreenMode = isFullscreen ? 
                FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }
    }
    
    void OnVSyncChanged(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        if (ConfigurationManager.Graphics != null)
        {
            ConfigurationManager.Graphics.vSyncEnabled = enabled;
        }
    }
    
    // Input callbacks
    void OnMouseSensitivityChanged(float value)
    {
        if (ConfigurationManager.Input != null)
        {
            ConfigurationManager.Input.mouseSensitivity = value;
        }
        UpdateSensitivityText(value);
    }
    
    void OnInvertMouseYChanged(bool inverted)
    {
        if (ConfigurationManager.Input != null)
        {
            ConfigurationManager.Input.invertMouseY = inverted;
        }
    }
    
    // Utility methods
    void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
            text.text = $"{Mathf.RoundToInt(value * 100)}%";
    }
    
    void UpdateSensitivityText(float value)
    {
        if (mouseSensitivityText != null)
            mouseSensitivityText.text = $"{value:F1}";
    }
    
    void PopulateQualityDropdown()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Very Low", "Low", "Medium", "High", "Very High", "Ultra"
            });
        }
    }
    
    void PopulateResolutionDropdown()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            Resolution[] resolutions = Screen.resolutions;
            var options = new System.Collections.Generic.List<string>();
            
            foreach (Resolution res in resolutions)
            {
                options.Add($"{res.width}x{res.height}");
            }
            
            resolutionDropdown.AddOptions(options);
            
            // Seleccionar resolución actual
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    resolutionDropdown.value = i;
                    break;
                }
            }
        }
    }
    
    void ResetToDefaults()
    {
        // Reset audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(1f);
            AudioManager.Instance.SetMusicVolume(0.7f);
            AudioManager.Instance.SetSFXVolume(0.8f);
        }
        
        // Reset graphics
        if (ConfigurationManager.Graphics != null)
        {
            ConfigurationManager.Graphics.qualityLevel = QualityLevel.High;
            ConfigurationManager.Graphics.vSyncEnabled = true;
            ConfigurationManager.Graphics.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        
        // Reset input
        if (ConfigurationManager.Input != null)
        {
            ConfigurationManager.Input.mouseSensitivity = 2f;
            ConfigurationManager.Input.invertMouseY = false;
        }
        
        // Reload UI
        LoadCurrentSettings();
    }
    
    public void ShowCategorySelector()
    {
        categorySelectorPanel.SetActive(true);
        audioPanel.SetActive(false);
        graphicsPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }
    
    public void OnCategorySelected(string category)
    {
        categorySelectorPanel.SetActive(false);
        audioPanel.SetActive(category == "Audio");
        graphicsPanel.SetActive(category == "Graphics");
        gameplayPanel.SetActive(category == "Gameplay");
        controlsPanel.SetActive(category == "Controls");
    }
    
    void GoBack()
    {
        UIManager.Instance.GoBack();
    }
    
    protected override void OnShow()
    {
        if (backButton != null)
            backButton.Select();
    }
    
}