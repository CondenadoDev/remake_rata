using UnityEngine;
using System.Collections.Generic;

public class ConfigurationManager : MonoBehaviour
{
    [Header("📋 Configuration Assets")]
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private GameplayConfig gameplayConfig;
    [SerializeField] private AudioConfig audioConfig;
    [SerializeField] private GraphicsConfig graphicsConfig;
    [SerializeField] private InputConfig inputConfig;
    
    // Singleton
    public static ConfigurationManager Instance { get; private set; }
    
    // Properties para acceso fácil
    public static PlayerConfig Player => Instance.playerConfig;
    public static GameplayConfig Gameplay => Instance.gameplayConfig;
    public static AudioConfig Audio => Instance.audioConfig;
    public static GraphicsConfig Graphics => Instance.graphicsConfig;
    public static InputConfig Input => Instance.inputConfig;
    
    // Eventos
    public static event System.Action<ConfigurationBase> OnConfigurationChanged;
    
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
        
        LoadAllConfigurations();
    }
    
    void LoadAllConfigurations()
    {
        // Cargar configuraciones desde Resources si no están asignadas
        if (playerConfig == null)
            playerConfig = Resources.Load<PlayerConfig>("Configs/PlayerConfig");
        if (gameplayConfig == null)
            gameplayConfig = Resources.Load<GameplayConfig>("Configs/GameplayConfig");
        if (audioConfig == null)
            audioConfig = Resources.Load<AudioConfig>("Configs/AudioConfig");
        if (graphicsConfig == null)
            graphicsConfig = Resources.Load<GraphicsConfig>("Configs/GraphicsConfig");
        if (inputConfig == null)
            inputConfig = Resources.Load<InputConfig>("Configs/InputConfig");
        
        Debug.Log("⚙️ ConfigurationManager loaded all configurations");
    }
    
    public static void NotifyConfigurationChanged(ConfigurationBase config)
    {
        OnConfigurationChanged?.Invoke(config);
    }
    
    public static T GetConfig<T>() where T : ConfigurationBase
    {
        if (typeof(T) == typeof(PlayerConfig)) return Instance.playerConfig as T;
        if (typeof(T) == typeof(GameplayConfig)) return Instance.gameplayConfig as T;
        if (typeof(T) == typeof(AudioConfig)) return Instance.audioConfig as T;
        if (typeof(T) == typeof(GraphicsConfig)) return Instance.graphicsConfig as T;
        if (typeof(T) == typeof(InputConfig)) return Instance.inputConfig as T;
        
        Debug.LogError($"❌ Configuration type {typeof(T)} not found!");
        return null;
    }
}