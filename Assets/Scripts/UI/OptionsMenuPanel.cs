using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UISystem.Configuration;
using UISystem.Core;
using UISystem.Panels;
using UnityEngine.Events;

public class OptionsMenuPanel : BaseUIPanel
{
    [Header("\uD83D\uDCCD Panel Configuration")]
    [SerializeField] private string panelID = "";
    [SerializeField] private bool startVisible = true;
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private bool blockGameInput = false;
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
    [SerializeField] private Button applyButton;
    
    [Header("📂 Categories")]
    [SerializeField] private GameObject categorySelectorPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject graphicsPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject controlsPanel;
    
    // State tracking
    private bool isLoadingSettings = false;
    private Resolution[] availableResolutions;

    protected override void OnInitialize()
    {
        panelID = "OptionsMenu";
        startVisible = false;
        useScaleAnimation = true;
        blockGameInput = true;
        
        SetupControls();
        PopulateDropdowns();
        LoadCurrentSettings();
        
        LogDebug("Options Menu Panel initialized");
    }
    
    void SetupControls()
    {
        try
        {
            // Audio sliders - usando métodos directos en lugar de diccionario
            SetupSlider(masterVolumeSlider, OnMasterVolumeChanged);
            SetupSlider(musicVolumeSlider, OnMusicVolumeChanged);
            SetupSlider(sfxVolumeSlider, OnSFXVolumeChanged);
            SetupSlider(mouseSensitivitySlider, OnMouseSensitivityChanged);
            
            // Graphics controls
            SetupDropdown(qualityDropdown, OnQualityChanged);
            SetupDropdown(resolutionDropdown, OnResolutionChanged);
            SetupToggle(fullscreenToggle, OnFullscreenChanged);
            SetupToggle(vSyncToggle, OnVSyncChanged);
            
            // Input controls
            SetupToggle(invertMouseYToggle, OnInvertMouseYChanged);
            
            // Buttons
            SetupButton(backButton, GoBack);
            SetupButton(resetToDefaultsButton, ResetToDefaults);
            SetupButton(applyButton, ApplySettings);
            
            LogDebug("Controls setup completed");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to setup controls: {e.Message}");
        }
    }
    
    void SetupSlider(Slider slider, UnityAction<float> callback)
    {
        if (slider != null && callback != null)
        {
            slider.onValueChanged.AddListener(callback);
            LogDebug($"Slider configured: {slider.name}");
        }
    }
    
    void SetupDropdown(TMP_Dropdown dropdown, UnityAction<int> callback)
    {
        if (dropdown != null && callback != null)
        {
            dropdown.onValueChanged.AddListener(callback);
            LogDebug($"Dropdown configured: {dropdown.name}");
        }
    }
    
    void SetupToggle(Toggle toggle, UnityAction<bool> callback)
    {
        if (toggle != null && callback != null)
        {
            toggle.onValueChanged.AddListener(callback);
            LogDebug($"Toggle configured: {toggle.name}");
        }
    }
    
    void SetupButton(Button button, UnityAction callback)
    {
        if (button != null && callback != null)
        {
            button.onClick.AddListener(callback);
            LogDebug($"Button configured: {button.name}");
        }
    }
    
    void PopulateDropdowns()
    {
        try
        {
            PopulateQualityDropdown();
            PopulateResolutionDropdown();
            LogDebug("Dropdowns populated");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to populate dropdowns: {e.Message}");
        }
    }
    
    void LoadCurrentSettings()
    {
        isLoadingSettings = true;
        
        try
        {
            LoadAudioSettings();
            LoadGraphicsSettings();
            LoadInputSettings();
            
            LogDebug("Current settings loaded");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to load current settings: {e.Message}");
        }
        finally
        {
            isLoadingSettings = false;
        }
    }
    
    void LoadAudioSettings()
    {
        if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
        {
            var volumeSettings = AudioManager.Instance.GetVolumeSettings();
            
            SetSliderValue(masterVolumeSlider, volumeSettings.masterVolume, masterVolumeText);
            SetSliderValue(musicVolumeSlider, volumeSettings.musicVolume, musicVolumeText);
            SetSliderValue(sfxVolumeSlider, volumeSettings.sfxVolume, sfxVolumeText);
        }
    }
    
    void LoadGraphicsSettings()
    {
        if (UIValidation.ValidateManager(ConfigurationManager.Graphics, "GraphicsConfig"))
        {
            var graphicsConfig = ConfigurationManager.Graphics;
            
            SetDropdownValue(qualityDropdown, (int)graphicsConfig.qualityLevel);
            SetToggleValue(fullscreenToggle, graphicsConfig.fullScreenMode == FullScreenMode.FullScreenWindow);
            SetToggleValue(vSyncToggle, graphicsConfig.vSyncEnabled);
            
            // Set resolution dropdown to current resolution
            SetCurrentResolution();
        }
    }
    
    void LoadInputSettings()
    {
        if (UIValidation.ValidateManager(ConfigurationManager.Input, "InputConfig"))
        {
            var inputConfig = ConfigurationManager.Input;
            
            SetSliderValue(mouseSensitivitySlider, inputConfig.mouseSensitivity, mouseSensitivityText);
            SetToggleValue(invertMouseYToggle, inputConfig.invertMouseY);
        }
    }
    
    // Helper methods for setting UI values
    void SetSliderValue(Slider slider, float value, TextMeshProUGUI text)
    {
        if (slider != null)
        {
            slider.value = value;
            UpdateVolumeText(text, value);
        }
    }
    
    void SetDropdownValue(TMP_Dropdown dropdown, int value)
    {
        if (dropdown != null && value >= 0 && value < dropdown.options.Count)
        {
            dropdown.value = value;
        }
    }
    
    void SetToggleValue(Toggle toggle, bool value)
    {
        if (toggle != null)
        {
            toggle.isOn = value;
        }
    }

    #region Audio Callbacks
    
    void OnMasterVolumeChanged(float value)
    {
        if (isLoadingSettings) return;
        
        try
        {
            if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
            {
                AudioManager.Instance.SetMasterVolume(value);
                UpdateVolumeText(masterVolumeText, value);
                LogDebug($"Master volume changed: {value:F2}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change master volume: {e.Message}");
        }
    }
    
    void OnMusicVolumeChanged(float value)
    {
        if (isLoadingSettings) return;
        
        try
        {
            if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
            {
                AudioManager.Instance.SetMusicVolume(value);
                UpdateVolumeText(musicVolumeText, value);
                LogDebug($"Music volume changed: {value:F2}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change music volume: {e.Message}");
        }
    }
    
    void OnSFXVolumeChanged(float value)
    {
        if (isLoadingSettings) return;
        
        try
        {
            if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
            {
                AudioManager.Instance.SetSFXVolume(value);
                UpdateVolumeText(sfxVolumeText, value);
                LogDebug($"SFX volume changed: {value:F2}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change SFX volume: {e.Message}");
        }
    }
    
    #endregion

    #region Graphics Callbacks
    
    void OnQualityChanged(int qualityIndex)
    {
        if (isLoadingSettings) return;
        
        try
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            
            if (UIValidation.ValidateManager(ConfigurationManager.Graphics, "GraphicsConfig"))
            {
                ConfigurationManager.Graphics.qualityLevel = (QualityLevel)qualityIndex;
            }
            
            LogDebug($"Quality level changed: {qualityIndex}");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change quality level: {e.Message}");
        }
    }
    
    void OnResolutionChanged(int resolutionIndex)
    {
        if (isLoadingSettings || availableResolutions == null) return;
        
        try
        {
            if (resolutionIndex >= 0 && resolutionIndex < availableResolutions.Length)
            {
                Resolution resolution = availableResolutions[resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
                LogDebug($"Resolution changed: {resolution.width}x{resolution.height}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change resolution: {e.Message}");
        }
    }
    
    void OnFullscreenChanged(bool isFullscreen)
    {
        if (isLoadingSettings) return;
        
        try
        {
            Screen.fullScreen = isFullscreen;
            
            if (UIValidation.ValidateManager(ConfigurationManager.Graphics, "GraphicsConfig"))
            {
                ConfigurationManager.Graphics.fullScreenMode = isFullscreen ? 
                    FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            }
            
            LogDebug($"Fullscreen changed: {isFullscreen}");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change fullscreen mode: {e.Message}");
        }
    }
    
    void OnVSyncChanged(bool enabled)
    {
        if (isLoadingSettings) return;
        
        try
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            
            if (UIValidation.ValidateManager(ConfigurationManager.Graphics, "GraphicsConfig"))
            {
                ConfigurationManager.Graphics.vSyncEnabled = enabled;
            }
            
            LogDebug($"VSync changed: {enabled}");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change VSync: {e.Message}");
        }
    }
    
    #endregion

    #region Input Callbacks
    
    void OnMouseSensitivityChanged(float value)
    {
        if (isLoadingSettings) return;
        
        try
        {
            if (UIValidation.ValidateManager(ConfigurationManager.Input, "InputConfig"))
            {
                ConfigurationManager.Input.mouseSensitivity = value;
                UpdateSensitivityText(value);
                LogDebug($"Mouse sensitivity changed: {value:F1}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change mouse sensitivity: {e.Message}");
        }
    }
    
    void OnInvertMouseYChanged(bool inverted)
    {
        if (isLoadingSettings) return;
        
        try
        {
            if (UIValidation.ValidateManager(ConfigurationManager.Input, "InputConfig"))
            {
                ConfigurationManager.Input.invertMouseY = inverted;
                LogDebug($"Invert mouse Y changed: {inverted}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to change invert mouse Y: {e.Message}");
        }
    }
    
    #endregion

    #region Utility Methods
    
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
            var options = new List<string>
            {
                "Very Low", "Low", "Medium", "High", "Very High", "Ultra"
            };
            qualityDropdown.AddOptions(options);
        }
    }
    
    void PopulateResolutionDropdown()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            availableResolutions = Screen.resolutions;
            var options = new List<string>();
            
            foreach (Resolution res in availableResolutions)
            {
                options.Add($"{res.width}x{res.height}");
            }
            
            resolutionDropdown.AddOptions(options);
        }
    }
    
    void SetCurrentResolution()
    {
        if (resolutionDropdown != null && availableResolutions != null)
        {
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                if (availableResolutions[i].width == Screen.currentResolution.width &&
                    availableResolutions[i].height == Screen.currentResolution.height)
                {
                    resolutionDropdown.value = i;
                    break;
                }
            }
        }
    }
    
    void ResetToDefaults()
    {
        try
        {
            LogDebug("Resetting to default settings");
            
            // Reset audio
            if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
            {
                AudioManager.Instance.SetMasterVolume(1f);
                AudioManager.Instance.SetMusicVolume(0.7f);
                AudioManager.Instance.SetSFXVolume(0.8f);
            }
            
            // Reset graphics
            if (UIValidation.ValidateManager(ConfigurationManager.Graphics, "GraphicsConfig"))
            {
                var config = ConfigurationManager.Graphics;
                config.qualityLevel = QualityLevel.High;
                config.vSyncEnabled = true;
                config.fullScreenMode = FullScreenMode.FullScreenWindow;
                config.ValidateValues();
            }
            
            // Reset input
            if (UIValidation.ValidateManager(ConfigurationManager.Input, "InputConfig"))
            {
                var config = ConfigurationManager.Input;
                config.mouseSensitivity = 2f;
                config.invertMouseY = false;
                config.ValidateValues();
            }
            
            // Apply changes
            if (UIValidation.ValidateManager(InputManager.Instance, "InputManager"))
            {
                InputManager.Instance.ApplyConfigurationValues();
            }

            if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
            {
                AudioManager.Instance.ApplyConfigurationValues();
            }

            LoadCurrentSettings();

            LogDebug("Settings reset to defaults");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to reset to defaults: {e.Message}");
        }
    }
    
    void ApplySettings()
    {
        try
        {
            LogDebug("Applying settings");

            // Graphics
            if (UIValidation.ValidateManager(ConfigurationManager.Graphics, "GraphicsConfig"))
            {
                ConfigurationManager.Graphics.ValidateValues();
                ConfigurationManager.Graphics.ApplySettings();
            }

            // Audio
            if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
            {
                AudioManager.Instance.ApplyConfigurationValues();
            }

            // Input
            if (UIValidation.ValidateManager(InputManager.Instance, "InputManager"))
            {
                InputManager.Instance.ApplyConfigurationValues();
            }

            LogDebug("Settings applied successfully");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to apply settings: {e.Message}");
        }
    }
    
    #endregion

    #region Category Management
    
    public void ShowCategorySelector()
    {
        try
        {
            SetCategoryPanelActive("selector");
            LogDebug("Category selector shown");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to show category selector: {e.Message}");
        }
    }
    
    public void OnCategorySelected(string category)
    {
        try
        {
            SetCategoryPanelActive(category);
            LogDebug($"Category selected: {category}");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to select category: {e.Message}");
        }
    }
    
    void SetCategoryPanelActive(string activeCategory)
    {
        if (categorySelectorPanel != null)
            categorySelectorPanel.SetActive(activeCategory == "selector");
        if (audioPanel != null)
            audioPanel.SetActive(activeCategory == "audio");
        if (graphicsPanel != null)
            graphicsPanel.SetActive(activeCategory == "graphics");
        if (gameplayPanel != null)
            gameplayPanel.SetActive(activeCategory == "gameplay");
        if (controlsPanel != null)
            controlsPanel.SetActive(activeCategory == "controls");
    }
    
    #endregion
    
    void GoBack()
    {
        try
        {
            LogDebug("Going back");
            if (UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
            {
                UIManager.Instance.GoBack();
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to go back: {e.Message}");
        }
    }
    
    protected override void OnShow()
    {
        // Seleccionar primer botón para navegación con gamepad
        if (backButton != null)
        {
            backButton.Select();
            LogDebug("Back button selected for navigation");
        }
        
        // Show category selector by default
        ShowCategorySelector();
    }
    
    void OnDestroy()
    {
        // Cleanup listeners
        CleanupSliders();
        CleanupDropdowns();
        CleanupToggles();
        CleanupButtons();
        
        LogDebug("Options Menu Panel destroyed");
    }
    
    void CleanupSliders()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveAllListeners();
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveAllListeners();
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.RemoveAllListeners();
    }
    
    void CleanupDropdowns()
    {
        if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveAllListeners();
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveAllListeners();
    }
    
    void CleanupToggles()
    {
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveAllListeners();
        if (vSyncToggle != null) vSyncToggle.onValueChanged.RemoveAllListeners();
        if (invertMouseYToggle != null) invertMouseYToggle.onValueChanged.RemoveAllListeners();
    }
    
    void CleanupButtons()
    {
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (resetToDefaultsButton != null) resetToDefaultsButton.onClick.RemoveAllListeners();
        if (applyButton != null) applyButton.onClick.RemoveAllListeners();
    }

    void LogDebug(string message)
    {
        Debug.Log($"[OptionsMenuPanel] {message}");
    }

    void LogError(string message)
    {
        Debug.LogError($"[OptionsMenuPanel] {message}");
    }
}