
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuPanel : UIPanel
{
    [Header("🎛️ Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    private bool isSubscribedToEvents = false;
    
    protected override void OnInitialize()
    {
        panelID = "PauseMenu";
        startVisible = false;
        useScaleAnimation = true;
        blockGameInput = true;
        
        SetupButtons();
        LogDebug("Pause Menu Panel initialized");
    }
    
    void OnEnable()
    {
        SubscribeToEvents();
    }
    
    void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    void SubscribeToEvents()
    {
        if (isSubscribedToEvents) return;
        
        try
        {
            if (UIValidation.ValidateManager(InputManager.Instance, "InputManager"))
            {
                InputManager.OnPauseInput += TogglePause;
                isSubscribedToEvents = true;
                LogDebug("Subscribed to input events");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to subscribe to events: {e.Message}");
        }
    }
    
    void UnsubscribeFromEvents()
    {
        if (!isSubscribedToEvents) return;
        
        try
        {
            if (InputManager.Instance != null)
            {
                InputManager.OnPauseInput -= TogglePause;
                isSubscribedToEvents = false;
                LogDebug("Unsubscribed from input events");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to unsubscribe from events: {e.Message}");
        }
    }
    
    void SetupButtons()
    {
        try
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(Resume);
                LogDebug("Resume button configured");
            }
                
            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(OpenOptions);
                LogDebug("Options button configured");
            }
                
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(GoToMainMenu);
                LogDebug("Main menu button configured");
            }
                
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
                LogDebug("Quit button configured");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to setup buttons: {e.Message}");
        }
    }
    
    void TogglePause()
    {
        try
        {
            if (IsVisible)
            {
                LogDebug("Toggle pause: Resuming");
                Resume();
            }
            else
            {
                LogDebug("Toggle pause: Pausing");
                Pause();
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to toggle pause: {e.Message}");
        }
    }
    
    public void Pause()
    {
        try
        {
            if (!UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
                return;
                
            LogDebug("Pausing game");
            
            UIManager.Instance.ShowPanel(panelID);
            
            // Usar GameManager para manejar el estado del juego
            if (UIValidation.ValidateManager(GameManager.Instance, "GameManager"))
            {
                GameManager.Instance.ChangeGameState(GameState.Paused);
            }
            else
            {
                // Fallback si GameManager no está disponible
                Time.timeScale = 0f;
                LogWarning("GameManager not available, using direct Time.timeScale manipulation");
            }
            
            // Cambiar contexto de input
            if (UIValidation.ValidateManager(InputManager.Instance, "InputManager"))
            {
                InputManager.Instance.SwitchContext(InputContext.UI);
            }
            
            // Pausar música
            if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
            {
                AudioManager.Instance.PauseMusic();
            }
            
            // Disparar evento
            UIEvents.TriggerGamePaused();
        }
        catch (System.Exception e)
        {
            LogError($"Failed to pause game: {e.Message}");
        }
    }
    
    public void Resume()
    {
        try
        {
            if (!UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
                return;
                
            LogDebug("Resuming game");
            
            UIManager.Instance.HidePanel(panelID);
            
            // Usar GameManager para manejar el estado del juego
            if (UIValidation.ValidateManager(GameManager.Instance, "GameManager"))
            {
                GameManager.Instance.ChangeGameState(GameState.Gameplay);
            }
            else
            {
                // Fallback si GameManager no está disponible
                Time.timeScale = 1f;
                LogWarning("GameManager not available, using direct Time.timeScale manipulation");
            }
            
            // Cambiar contexto de input
            if (UIValidation.ValidateManager(InputManager.Instance, "InputManager"))
            {
                InputManager.Instance.SwitchContext(InputContext.Gameplay);
            }
            
            // Resumir música
            if (UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
            {
                AudioManager.Instance.ResumeMusic();
            }
            
            // Disparar evento
            UIEvents.TriggerGameResumed();
        }
        catch (System.Exception e)
        {
            LogError($"Failed to resume game: {e.Message}");
        }
    }
    
    void OpenOptions()
    {
        try
        {
            LogDebug("Opening options menu");
            if (UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
            {
                UIManager.Instance.ShowPanel("OptionsMenu");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to open options: {e.Message}");
        }
    }
    
    void GoToMainMenu()
    {
        try
        {
            LogDebug("Going to main menu");
            
            // Asegurar que el tiempo vuelva a normal
            Time.timeScale = 1f;
            
            // Usar GameManager si está disponible
            if (UIValidation.ValidateManager(GameManager.Instance, "GameManager"))
            {
                GameManager.Instance.ChangeGameState(GameState.MainMenu);
            }
            
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        catch (System.Exception e)
        {
            LogError($"Failed to go to main menu: {e.Message}");
        }
    }
    
    void QuitGame()
    {
        try
        {
            LogDebug("Quitting game");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        catch (System.Exception e)
        {
            LogError($"Failed to quit game: {e.Message}");
        }
    }
    
    protected override void OnShow()
    {
        // Seleccionar primer botón para navegación con gamepad
        if (resumeButton != null)
        {
            resumeButton.Select();
            LogDebug("Resume button selected for navigation");
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
        LogDebug("Pause Menu Panel destroyed");
    }
}