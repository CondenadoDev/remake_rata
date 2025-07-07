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
    [SerializeField] private bool migrateExistingComponents = true;
    
    [Header("🎮 Manager Prefabs (Optional)")]
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject uiManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject inputManagerPrefab;
    [SerializeField] private GameObject saveSystemPrefab;
    
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
        
        if (migrateExistingComponents)
            MigrateExistingComponents();
            
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
    
    void MigrateExistingComponents()
    {
        Debug.Log("🔄 Migrating existing components...");
        
        // Migrar PlayerController existente
        PlayerController oldPlayerController = FindFirstObjectByType<PlayerController>();
        if (oldPlayerController != null)
        {
            GameObject playerObj = oldPlayerController.gameObject;
            
            // Verificar si ya tiene PlayerStateMachine
            if (playerObj.GetComponent<PlayerStateMachine>() == null)
            {
                Debug.Log("🔄 Migrating PlayerController to PlayerStateMachine...");
                
                // Copiar configuración básica
                PlayerStateMachine newPlayerController = playerObj.AddComponent<PlayerStateMachine>();
                
                // Copiar referencias de cámara si existen
                if (oldPlayerController.cameraTransform != null)
                {
                    // newPlayerController.cameraTransform = oldPlayerController.cameraTransform;
                }
                
                // Deshabilitar el viejo (no eliminar para preservar referencias)
                oldPlayerController.enabled = false;
                
                Debug.Log("✅ PlayerController migrated to PlayerStateMachine");
            }
        }
        
        // Migrar HealthSystem existente (ya es compatible)
        HealthSystem[] healthSystems = FindObjectsByType<HealthSystem>(FindObjectsSortMode.None);
        foreach (HealthSystem healthSystem in healthSystems)
        {
            // HealthSystem ya es compatible, solo verificar configuración
            Debug.Log($"✅ HealthSystem on {healthSystem.name} is compatible");
        }
        
        // Buscar y configurar sistemas de UI existentes
        PlayerHealthUI existingHealthUI = FindFirstObjectByType<PlayerHealthUI>();
        if (existingHealthUI != null)
        {
            Debug.Log("✅ Existing PlayerHealthUI found and is compatible");
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
        HUDPanel hudComponent = hudPanel.AddComponent<HUDPanel>();
        
        // Crear elementos básicos del HUD
        CreateHealthBar(hudPanel.transform);
        CreateStaminaBar(hudPanel.transform);
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
        PauseMenuPanel pauseComponent = pausePanel.AddComponent<PauseMenuPanel>();
        
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
        OptionsMenuPanel optionsComponent = optionsPanel.AddComponent<OptionsMenuPanel>();
        
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
    
    [ContextMenu("🔄 Migrate Components Only")]
    public void MigrateComponentsOnly()
    {
        MigrateExistingComponents();
    }
    
    [ContextMenu("⚙️ Create Configurations Only")]
    public void CreateConfigurationsOnly()
    {
        CreateDefaultConfigurations();
    }
}