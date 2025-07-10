using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("🛑 Pause Settings")]
    public GameObject pausePanel;
    public KeyCode pauseKey = KeyCode.Escape;
    public bool pauseAudio = true;
    public string mainMenuSceneName = "MainMenu";
    
    private bool isPaused = false;
    private AudioSource[] audioSources;
    
    void Start()
    {
        // Inicializar sistema de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        // Encontrar todas las fuentes de audio para pausar/reanudar
        audioSources = FindObjectsOfType<AudioSource>();
        
        Debug.Log("🛑 Pause Manager initialized");
    }
    
    void Update()
    {
        // Input para pausar/reanudar
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        if (isPaused) return;
        
        Debug.Log("⏸️ Pausing game...");
        
        isPaused = true;
        
        // Pausar tiempo del juego
        Time.timeScale = 0f;
        
        // Pausar audio si está habilitado
        if (pauseAudio)
        {
            PauseAllAudio();
        }
        
        // Mostrar menú de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            
            // Seleccionar primer botón para navegación
            Button firstButton = pausePanel.GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                firstButton.Select();
            }
        }
        
        // Cambiar cursor si es necesario
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("✅ Game paused successfully");
    }
    
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        Debug.Log("▶️ Resuming game...");
        
        isPaused = false;
        
        // Restaurar tiempo del juego
        Time.timeScale = 1f;
        
        // Reanudar audio si estaba pausado
        if (pauseAudio)
        {
            ResumeAllAudio();
        }
        
        // Ocultar menú de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        // Restaurar cursor si es necesario (para juegos en primera persona)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("✅ Game resumed successfully");
    }
    
    public void GoToMainMenu()
    {
        Debug.Log("🏠 Going to main menu...");
        
        // Restaurar tiempo normal antes de cambiar escena
        Time.timeScale = 1f;
        
        // Cargar escena del menú principal
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ Main menu scene name not set!");
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("❌ Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void PauseAllAudio()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }
    
    private void ResumeAllAudio()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }
    }
    
    public bool IsPaused => isPaused;
    
    void OnDestroy()
    {
        // Asegurar que el tiempo vuelva a normal al destruir
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }
}