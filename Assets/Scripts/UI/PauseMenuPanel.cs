using UnityEngine;
using UnityEngine.UI;

public class PauseMenuPanel : UIPanel
{
    [Header("🎛️ Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    protected override void OnInitialize()
    {
        panelID = "PauseMenu";
        startVisible = false;
        useScaleAnimation = true;
        blockGameInput = true;
        
        SetupButtons();
        
        // Suscribirse a eventos
        InputManager.OnPauseInput += TogglePause;
    }
    
    void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
            
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    void TogglePause()
    {
        if (IsVisible)
            Resume();
        else
            Pause();
    }
    
    public void Pause()
    {
        UIManager.Instance.ShowPanel(panelID);
        Time.timeScale = 0f;
        InputManager.Instance.SwitchContext(InputContext.UI);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PauseMusic();
    }
    
    public void Resume()
    {
        UIManager.Instance.HidePanel(panelID);
        Time.timeScale = 1f;
        InputManager.Instance.SwitchContext(InputContext.Gameplay);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeMusic();
    }
    
    void OpenOptions()
    {
        UIManager.Instance.ShowPanel("OptionsMenu");
    }
    
    void GoToMainMenu()
    {
        Time.timeScale = 1f; // Asegurar que el tiempo vuelva a normal
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    protected override void OnShow()
    {
        // Seleccionar primer botón para navegación con gamepad
        if (resumeButton != null)
            resumeButton.Select();
    }
    
    void OnDestroy()
    {
        InputManager.OnPauseInput -= TogglePause;
    }
}
