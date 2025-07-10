using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SetupManager : MonoBehaviour
{
    [Header("🎯 Auto Setup Options")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createUICanvasIfMissing = true;
    [SerializeField] private bool setupEventSystemIfMissing = true;
    [SerializeField] private bool createCoreManagers = true;
    [SerializeField] private bool setupPlayerSystemIfMissing = true; // ✅ NUEVO
    
    [Header("🎮 Manager Prefabs (Optional)")]
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject uiManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject inputManagerPrefab;
    [SerializeField] private GameObject saveSystemPrefab;
    [SerializeField] private GameObject playerPrefab; // ✅ NUEVO
    
    [Header("📋 Status")]
    [SerializeField] private bool isSetupComplete = false;
    
    void Start()
    {
        if (autoSetupOnStart && !isSetupComplete)
        {
            StartCoroutine(AutoSetupCoroutine());
        }
    }
    
    System.Collections.IEnumerator AutoSetupCoroutine()
    {
        Debug.Log("🚀 Starting automated setup...");
        
        yield return new WaitForSeconds(0.1f);
        
        if (createCoreManagers)
            SetupCoreManagers();
            
        yield return new WaitForSeconds(0.1f);
        
        if (createUICanvasIfMissing)
            SetupUICanvas();
            
        yield return new WaitForSeconds(0.1f);
        
        if (setupEventSystemIfMissing)
            SetupEventSystem();
            
        yield return new WaitForSeconds(0.1f);
        
        if (setupPlayerSystemIfMissing) // ✅ NUEVO
            SetupPlayerSystem();
            
        yield return new WaitForSeconds(0.1f);
        
        CreateDefaultConfigurations();
        
        isSetupComplete = true;
        Debug.Log("✅ Automated setup completed!");
    }
    
    void SetupCoreManagers()
    {
        Debug.Log("🔧 Setting up core managers...");
        
        // GameManager
        if (GameManager.Instance == null)
        {
            if (gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
            else
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
                Debug.Log("✅ GameManager created");
            }
        }
        
        // UIManager
        if (UIManager.Instance == null)
        {
            if (uiManagerPrefab != null)
            {
                Instantiate(uiManagerPrefab);
            }
            else
            {
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManagerObj.AddComponent<UIManager>();
                Debug.Log("✅ UIManager created");
            }
        }
        
        // AudioManager
        if (AudioManager.Instance == null)
        {
            if (audioManagerPrefab != null)
            {
                Instantiate(audioManagerPrefab);
            }
            else
            {
                GameObject audioManagerObj = new GameObject("AudioManager");
                audioManagerObj.AddComponent<AudioManager>();
                Debug.Log("✅ AudioManager created");
            }
        }
        
        // InputManager
        if (InputManager.Instance == null)
        {
            if (inputManagerPrefab != null)
            {
                Instantiate(inputManagerPrefab);
            }
            else
            {
                GameObject inputManagerObj = new GameObject("InputManager");
                inputManagerObj.AddComponent<InputManager>();
                Debug.Log("✅ InputManager created");
            }
        }
        
        // SaveSystem
        if (SaveSystem.Instance == null)
        {
            if (saveSystemPrefab != null)
            {
                Instantiate(saveSystemPrefab);
            }
            else
            {
                GameObject saveSystemObj = new GameObject("SaveSystem");
                saveSystemObj.AddComponent<SaveSystem>();
                Debug.Log("✅ SaveSystem created");
            }
        }
        
        // ConfigurationManager
        if (ConfigurationManager.Instance == null)
        {
            GameObject configManagerObj = new GameObject("ConfigurationManager");
            configManagerObj.AddComponent<ConfigurationManager>();
            Debug.Log("✅ ConfigurationManager created");
        }
    }
    
    // ✅ NUEVO MÉTODO - Reemplaza MigrateExistingComponents
    void SetupPlayerSystem()
    {
        Debug.Log("🎮 Setting up player system...");
        
        // Buscar PlayerStateMachine existente
        PlayerStateMachine existingPlayer = FindFirstObjectByType<PlayerStateMachine>();
        
        if (existingPlayer == null)
        {
            // No hay player, crear uno nuevo
            if (playerPrefab != null)
            {
                GameObject newPlayer = Instantiate(playerPrefab);
                Debug.Log("✅ Player created from prefab");
            }
            else
            {
                // Crear player básico
                CreateBasicPlayer();
            }
        }
        else
        {
            // Player existe, verificar que tenga todos los componentes
            ValidatePlayerComponents(existingPlayer.gameObject);
        }
    }
    
    void CreateBasicPlayer()
    {
        Debug.Log("🎮 Creating basic player setup...");
        
        GameObject playerObj = new GameObject("Player");
        
        // Componentes esenciales
        CharacterController controller = playerObj.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0, 1, 0);
        
        // Animator (opcional)
        Animator animator = playerObj.AddComponent<Animator>();
        
        // Sistema de salud
        HealthSystem healthSystem = playerObj.AddComponent<HealthSystem>();
        healthSystem.maxHealth = 100f;
        
        // Sistema principal del jugador
        PlayerStateMachine playerStateMachine = playerObj.AddComponent<PlayerStateMachine>();
        
        // Sistemas complementarios se agregan automáticamente en PlayerStateMachine
        
        // Configurar posición inicial
        playerObj.transform.position = new Vector3(0, 1, 0);
        
        // Tag y layer
        playerObj.tag = "Player";
        playerObj.layer = LayerMask.NameToLayer("Default");
        
        Debug.Log("✅ Basic player created with full system");
    }
    
    void ValidatePlayerComponents(GameObject playerObj)
    {
        Debug.Log("🔍 Validating player components...");
        
        // Verificar componentes esenciales
        if (playerObj.GetComponent<CharacterController>() == null)
        {
            CharacterController controller = playerObj.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = new Vector3(0, 1, 0);
            Debug.Log("✅ CharacterController added to player");
        }
        
        if (playerObj.GetComponent<Animator>() == null)
        {
            playerObj.AddComponent<Animator>();
            Debug.Log("✅ Animator added to player");
        }
        
        if (playerObj.GetComponent<HealthSystem>() == null)
        {
            HealthSystem healthSystem = playerObj.AddComponent<HealthSystem>();
            healthSystem.maxHealth = 100f;
            Debug.Log("✅ HealthSystem added to player");
        }
        
        // PlayerMovement, PlayerCombat, PlayerStats se agregan automáticamente
        // en PlayerStateMachine.Awake() si no existen
        
        Debug.Log("✅ Player components validated");
    }
    
    void SetupUICanvas()
    {
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        
        if (existingCanvas == null)
        {
            Debug.Log("🎨 Creating UI Canvas...");
            
            // Crear Canvas principal
            GameObject canvasObj = new GameObject("UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Crear paneles básicos
            CreateHUDPanel(canvasObj.transform);
            CreatePauseMenuPanel(canvasObj.transform);
            CreateOptionsMenuPanel(canvasObj.transform);
            
            Debug.Log("✅ UI Canvas created with basic panels");
        }
        else
        {
            Debug.Log("🎨 UI Canvas already exists, skipping creation");
        }
    }
    
    void SetupEventSystem()
    {
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();
        
        if (existingEventSystem == null)
        {
            Debug.Log("🎯 Creating EventSystem...");
            
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            
            Debug.Log("✅ EventSystem created");
        }
        else
        {
            Debug.Log("🎯 EventSystem already exists, skipping creation");
        }
    }
    
    void CreateDefaultConfigurations()
    {
        Debug.Log("⚙️ Creating default configurations...");
        
        #if UNITY_EDITOR
        // Crear directorio de configuraciones
        string configPath = "Assets/Resources/Configs";
        if (!AssetDatabase.IsValidFolder(configPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateFolder("Assets/Resources", "Configs");
        }
        
        // Crear PlayerConfig si no existe
        if (Resources.Load<PlayerConfig>("Configs/PlayerConfig") == null)
        {
            PlayerConfig playerConfig = ScriptableObject.CreateInstance<PlayerConfig>();
            AssetDatabase.CreateAsset(playerConfig, $"{configPath}/PlayerConfig.asset");
            Debug.Log("✅ PlayerConfig created");
        }
        
        // Crear GameplayConfig si no existe
        if (Resources.Load<GameplayConfig>("Configs/GameplayConfig") == null)
        {
            GameplayConfig gameplayConfig = ScriptableObject.CreateInstance<GameplayConfig>();
            AssetDatabase.CreateAsset(gameplayConfig, $"{configPath}/GameplayConfig.asset");
            Debug.Log("✅ GameplayConfig created");
        }
        
        // Crear AudioConfig si no existe
        if (Resources.Load<AudioConfig>("Configs/AudioConfig") == null)
        {
            AudioConfig audioConfig = ScriptableObject.CreateInstance<AudioConfig>();
            AssetDatabase.CreateAsset(audioConfig, $"{configPath}/AudioConfig.asset");
            Debug.Log("✅ AudioConfig created");
        }
        
        // Crear GraphicsConfig si no existe
        if (Resources.Load<GraphicsConfig>("Configs/GraphicsConfig") == null)
        {
            GraphicsConfig graphicsConfig = ScriptableObject.CreateInstance<GraphicsConfig>();
            AssetDatabase.CreateAsset(graphicsConfig, $"{configPath}/GraphicsConfig.asset");
            Debug.Log("✅ GraphicsConfig created");
        }
        
        // Crear InputConfig si no existe
        if (Resources.Load<InputConfig>("Configs/InputConfig") == null)
        {
            InputConfig inputConfig = ScriptableObject.CreateInstance<InputConfig>();
            AssetDatabase.CreateAsset(inputConfig, $"{configPath}/InputConfig.asset");
            Debug.Log("✅ InputConfig created");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        #endif
    }
    
    void CreateHUDPanel(Transform parent)
    {
        GameObject hudPanel = new GameObject("HUD Panel");
        hudPanel.transform.SetParent(parent, false);
        
        RectTransform rect = hudPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        CanvasGroup canvasGroup = hudPanel.AddComponent<CanvasGroup>();
        
        // ✅ Usar ConcreteUIPanel en lugar de HUDPanel específico
        ConcreteUIPanel hudComponent = hudPanel.AddComponent<ConcreteUIPanel>();
        hudComponent.panelID = "HUD";
        hudComponent.startVisible = true;
        hudComponent.useScaleAnimation = false;
        hudComponent.blockGameInput = false;
        
        // Crear elementos básicos del HUD
        CreateHealthBar(hudPanel.transform);
        CreateStaminaBar(hudPanel.transform);
        
        // Agregar conector para PlayerStats
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerStatsUIConnector connector = player.GetComponent<PlayerStatsUIConnector>();
            if (connector == null)
            {
                player.AddComponent<PlayerStatsUIConnector>();
                Debug.Log("✅ PlayerStatsUIConnector added to player");
            }
        }
    }
    
    void CreateHealthBar(Transform parent)
    {
        GameObject healthBarObj = new GameObject("Health Bar");
        healthBarObj.transform.SetParent(parent, false);
        
        RectTransform rect = healthBarObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(200, 20);
        
        Slider slider = healthBarObj.AddComponent<Slider>();
        slider.value = 1f;
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarObj.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.red;
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        slider.targetGraphic = fillImage;
        slider.fillRect = fillRect;
    }
    
    void CreateStaminaBar(Transform parent)
    {
        GameObject staminaBarObj = new GameObject("Stamina Bar");
        staminaBarObj.transform.SetParent(parent, false);
        
        RectTransform rect = staminaBarObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -50);
        rect.sizeDelta = new Vector2(200, 20);
        
        Slider slider = staminaBarObj.AddComponent<Slider>();
        slider.value = 1f;
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(staminaBarObj.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.gray;
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.yellow;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        slider.targetGraphic = fillImage;
        slider.fillRect = fillRect;
    }
    
    void CreatePauseMenuPanel(Transform parent)
    {
        GameObject pausePanel = new GameObject("Pause Menu Panel");
        pausePanel.transform.SetParent(parent, false);
        pausePanel.SetActive(false);
        
        RectTransform rect = pausePanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        CanvasGroup canvasGroup = pausePanel.AddComponent<CanvasGroup>();
        
        // ✅ Usar ConcreteUIPanel en lugar de PauseMenuPanel específico
        ConcreteUIPanel pauseComponent = pausePanel.AddComponent<ConcreteUIPanel>();
        pauseComponent.panelID = "PauseMenu";
        pauseComponent.startVisible = false;
        pauseComponent.useScaleAnimation = true;
        pauseComponent.blockGameInput = true;
        
        // Fondo semi-transparente
        Image backgroundImage = pausePanel.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.7f);
    }
    
    void CreateOptionsMenuPanel(Transform parent)
    {
        GameObject optionsPanel = new GameObject("Options Menu Panel");
        optionsPanel.transform.SetParent(parent, false);
        optionsPanel.SetActive(false);
        
        RectTransform rect = optionsPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        CanvasGroup canvasGroup = optionsPanel.AddComponent<CanvasGroup>();
        
        // ✅ Usar ConcreteUIPanel en lugar de OptionsMenuPanel específico
        ConcreteUIPanel optionsComponent = optionsPanel.AddComponent<ConcreteUIPanel>();
        optionsComponent.panelID = "OptionsMenu";
        optionsComponent.startVisible = false;
        optionsComponent.useScaleAnimation = true;
        optionsComponent.blockGameInput = true;
        
        // Fondo semi-transparente
        Image backgroundImage = optionsPanel.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.8f);
    }
    
    // Métodos públicos para llamar desde el inspector
    [ContextMenu("🚀 Run Complete Setup")]
    public void RunCompleteSetup()
    {
        StartCoroutine(AutoSetupCoroutine());
    }
    
    [ContextMenu("🔧 Setup Core Managers Only")]
    public void SetupCoreManagersOnly()
    {
        SetupCoreManagers();
    }
    
    [ContextMenu("🎨 Setup UI Only")]
    public void SetupUIOnly()
    {
        SetupUICanvas();
        SetupEventSystem();
    }
    
    [ContextMenu("🎮 Setup Player System Only")]
    public void SetupPlayerSystemOnly()
    {
        SetupPlayerSystem();
    }
    
    [ContextMenu("⚙️ Create Configurations Only")]
    public void CreateConfigurationsOnly()
    {
        CreateDefaultConfigurations();
    }
}