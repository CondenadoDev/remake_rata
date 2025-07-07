using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [Header("🎵 Audio")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private string exposedParam = "MasterVolume";
    [SerializeField] private Slider sliderVolume;

    [Header("🎮 Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    
    [Header("🏞️ Scene Management")]
    [SerializeField] private string gameplaySceneName = "Lobby";
    
    [Header("🔧 Settings")]
    [SerializeField] private bool enableDebugLogs = true;

    const string VOL_KEY = "MasterVol";
    private bool isInitialized = false;

    void Start()
    {
        StartCoroutine(InitializeAsync());
    }
    
    IEnumerator InitializeAsync()
    {
        LogDebug("Initializing Main Menu...");
        
        yield return new WaitForEndOfFrame();
        
        try
        {
            SetupAudio();
            SetupButtons();
            CheckSaveFileStatus();
            
            isInitialized = true;
            LogDebug("Main Menu initialization completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Initialization failed: {e.Message}");
        }
    }
    
    void SetupAudio()
    {
        try
        {
            if (masterMixer == null)
            {
                Debug.LogWarning("⚠️ [MainMenuUI] Master mixer not assigned");
                return;
            }
            
            LogDebug($"Mixer assigned: {masterMixer.name}");

            // Cargar volumen guardado
            float saved = PlayerPrefs.GetFloat(VOL_KEY, 1f);
            
            if (sliderVolume != null)
            {
                sliderVolume.value = saved;
                sliderVolume.onValueChanged.AddListener(OnVolumeChanged);
                LogDebug($"Volume slider configured with saved value: {saved:F2}");
            }
            
            ApplyVolume(saved);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to setup audio: {e.Message}");
        }
    }
    
    void SetupButtons()
    {
        try
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnStartPressed);
                LogDebug("New Game button configured");
            }
            
            if (loadButton != null)
            {
                loadButton.onClick.AddListener(OnLoadPressed);
                LogDebug("Load Game button configured");
            }
            
            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(OnOptionsPressed);
                LogDebug("Options button configured");
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitPressed);
                LogDebug("Quit button configured");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to setup buttons: {e.Message}");
        }
    }
    
    void CheckSaveFileStatus()
    {
        try
        {
            bool hasSaveFile = false;
            
            if (UIValidation.ValidateManager(SaveSystem.Instance, "SaveSystem"))
            {
                var saveInfos = SaveSystem.Instance.GetSaveFileInfos();
                if (saveInfos != null && saveInfos.Length > 0)
                {
                    hasSaveFile = saveInfos[0].exists;
                }
            }
            
            if (loadButton != null)
            {
                loadButton.interactable = hasSaveFile;
                LogDebug($"Load button status: {(hasSaveFile ? "Enabled" : "Disabled")}");
            }
            
            if (!hasSaveFile)
            {
                LogDebug("No save file found for loading");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to check save file status: {e.Message}");
            
            // En caso de error, deshabilitar botón de carga
            if (loadButton != null)
                loadButton.interactable = false;
        }
    }

    #region Button Handlers

    public void OnStartPressed()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ [MainMenuUI] Not initialized yet, ignoring new game request");
            return;
        }
        
        try
        {
            LogDebug("Starting new game");

            if (UIValidation.ValidateManager(SaveSystem.Instance, "SaveSystem"))
            {
                SaveSystem.Instance.DeleteSave(0); // Reinicia slot 0
                LogDebug("Previous save deleted");
            }

            LoadGameplayScene();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to start new game: {e.Message}");
        }
    }

    public void OnLoadPressed()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ [MainMenuUI] Not initialized yet, ignoring load request");
            return;
        }
        
        try
        {
            LogDebug("Loading saved game");

            if (!UIValidation.ValidateManager(SaveSystem.Instance, "SaveSystem"))
                return;

            bool result = SaveSystem.Instance.LoadGame(0);

            if (result)
            {
                LogDebug("Save loaded successfully");
                // El SaveSystem se encargará de cargar la escena correcta
            }
            else
            {
                Debug.LogWarning("⚠️ [MainMenuUI] Failed to load save file");
                // Fallback: ir a escena de gameplay
                LoadGameplayScene();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to load game: {e.Message}");
        }
    }

    public void OnOptionsPressed()
    {
        try
        {
            LogDebug("Opening options menu");
            
            if (UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
            {
                UIManager.Instance.ShowPanel("OptionsMenu");
            }
            else
            {
                Debug.LogWarning("⚠️ [MainMenuUI] UIManager not available, cannot open options");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to open options: {e.Message}");
        }
    }

    public void OnQuitPressed()
    {
        try
        {
            LogDebug("Quitting application");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to quit: {e.Message}");
        }
    }

    public void OnVolumeChanged(float value)
    {
        try
        {
            ApplyVolume(value);
            PlayerPrefs.SetFloat(VOL_KEY, value);
            PlayerPrefs.Save();
            LogDebug($"Volume changed to: {value:F2}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to change volume: {e.Message}");
        }
    }

    #endregion

    #region Utility Methods

    void ApplyVolume(float normalized)
    {
        try
        {
            if (masterMixer == null) return;
            
            float dB = normalized > 0.001f ? Mathf.Log10(normalized) * 20f : -80f;
            bool success = masterMixer.SetFloat(exposedParam, dB);
            
            if (!success)
            {
                Debug.LogWarning($"⚠️ [MainMenuUI] Failed to set mixer parameter: {exposedParam}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to apply volume: {e.Message}");
        }
    }
    
    void LoadGameplayScene()
    {
        try
        {
            if (string.IsNullOrEmpty(gameplaySceneName))
            {
                Debug.LogError("❌ [MainMenuUI] Gameplay scene name is not set");
                return;
            }
            
            LogDebug($"Loading gameplay scene: {gameplaySceneName}");
            SceneManager.LoadScene(gameplaySceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MainMenuUI] Failed to load gameplay scene: {e.Message}");
        }
    }
    
    void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"🎭 [MainMenuUI] {message}");
    }

    #endregion
    
    void OnDestroy()
    {
        // Cleanup listeners
        if (sliderVolume != null)
        {
            sliderVolume.onValueChanged.RemoveAllListeners();
        }
        
        LogDebug("Main Menu UI destroyed");
    }
}