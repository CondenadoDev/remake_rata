// Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("🎮 Core References")]
    public PlayerStateMachine  player;
    public Transform playerSpawnPoint;
    
    [Header("📊 Game State")]
    public GameState currentState = GameState.MainMenu;
    
    [Header("⚙️ Settings")]
    public GameSettings gameSettings;
    
    // Singleton
    public static GameManager Instance { get; private set; }
    
    // Events
    public static event System.Action<GameState> OnGameStateChanged;
    public static event System.Action OnPlayerDied;
    public static event System.Action OnGamePaused;
    
    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeGame();
    }
    
    void InitializeGame()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;

        // 🧠 Suscripción al evento global de muerte del jugador
        HealthSystem.OnAnyPlayerDeath += HandlePlayerDeath;
    }
    
    public void ChangeGameState(GameState newState)
    {
        GameState oldState = currentState;
        currentState = newState;
        
        Debug.Log($"🎮 Game State: {oldState} → {newState}");
        OnGameStateChanged?.Invoke(newState);
        
        HandleStateChange(newState);
    }
    
    void HandleStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Gameplay:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                OnGamePaused?.Invoke();
                break;
                
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }
    
    void HandlePlayerDeath()
    {
        ChangeGameState(GameState.GameOver);
        OnPlayerDied?.Invoke();

        // 🔁 Reinicia la escena luego de 2 segundos
        Invoke(nameof(RestartLevel), 2f);
    }
    
    void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Gameplay)
                ChangeGameState(GameState.Paused);
            else if (currentState == GameState.Paused)
                ChangeGameState(GameState.Gameplay);
        }
    }
}

public enum GameState
{
    MainMenu,
    Gameplay, 
    Paused,
    GameOver,
    Loading
}
