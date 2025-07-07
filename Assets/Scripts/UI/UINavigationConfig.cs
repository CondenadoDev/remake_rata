using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Sistema de configuración de navegación UI para facilitar la personalización desde el editor
/// </summary>
[System.Serializable]
public class UINavigationLink
{
    public string linkName = "New Link";
    public string fromPanel = "";
    public string toPanel = "";
    public Button triggerButton;
    public KeyCode shortcutKey = KeyCode.None;
    public bool addToHistory = true;
    public UnityEngine.Events.UnityEvent onNavigate;
}

[System.Serializable]
public class UIPanelConfig
{
    public string panelID = "";
    public GameObject panelObject;
    public bool isMainPanel = false;
    public float transitionDuration = 0.3f;
    public bool useAnimation = true;
    public List<UINavigationLink> navigationLinks = new List<UINavigationLink>();
    public UnityEngine.Events.UnityEvent onPanelOpen;
    public UnityEngine.Events.UnityEvent onPanelClose;
}

public class UINavigationConfig : MonoBehaviour
{
    [Header("🎮 UI Navigation Configuration")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool autoFindUIManager = true;
    [SerializeField] public bool enableKeyboardShortcuts = true;
    [SerializeField] private bool enableGamepadSupport = true;
    
    [Header("📋 Panel Configurations")]
    [SerializeField] private List<UIPanelConfig> panelConfigs = new List<UIPanelConfig>();
    
    [Header("⌨️ Global Shortcuts")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode inventoryKey = KeyCode.Tab;
    [SerializeField] private KeyCode quickSaveKey = KeyCode.F5;
    [SerializeField] private KeyCode quickLoadKey = KeyCode.F9;
    
    [Header("🎯 Quick Actions")]
    [SerializeField] private bool showDebugUI = false;
    
    // Estado interno
    private Dictionary<string, UIPanelConfig> configDictionary = new Dictionary<string, UIPanelConfig>();
    private bool isInitialized = false;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        if (autoFindUIManager && uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
        
        if (uiManager == null)
        {
            Debug.LogError("❌ [UINavigationConfig] UIManager not found!");
            return;
        }
        
        // Construir diccionario de configuraciones
        BuildConfigDictionary();
        
        // Configurar todos los enlaces de navegación
        SetupNavigationLinks();
        
        isInitialized = true;
        Debug.Log($"✅ [UINavigationConfig] Initialized with {panelConfigs.Count} panel configurations");
    }
    
    void BuildConfigDictionary()
    {
        configDictionary.Clear();
        
        foreach (var config in panelConfigs)
        {
            if (!string.IsNullOrEmpty(config.panelID))
            {
                configDictionary[config.panelID] = config;
            }
        }
    }
    
    void SetupNavigationLinks()
    {
        foreach (var config in panelConfigs)
        {
            foreach (var link in config.navigationLinks)
            {
                SetupSingleLink(link);
            }
        }
    }
    
    void SetupSingleLink(UINavigationLink link)
    {
        if (link.triggerButton != null)
        {
            link.triggerButton.onClick.RemoveAllListeners();
            link.triggerButton.onClick.AddListener(() => {
                NavigateToPanel(link.toPanel, link.addToHistory);
                link.onNavigate?.Invoke();
            });
        }
    }
    
    void Update()
    {
        if (!isInitialized || !enableKeyboardShortcuts) return;
        
        HandleKeyboardInput();
        
        if (enableGamepadSupport)
        {
            HandleGamepadInput();
        }
    }
    
    void HandleKeyboardInput()
    {
        // Shortcuts globales
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
        
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }
        
        if (Input.GetKeyDown(quickSaveKey))
        {
            QuickSave();
        }
        
        if (Input.GetKeyDown(quickLoadKey))
        {
            QuickLoad();
        }
        
        // Shortcuts específicos de paneles
        foreach (var config in panelConfigs)
        {
            foreach (var link in config.navigationLinks)
            {
                if (link.shortcutKey != KeyCode.None && Input.GetKeyDown(link.shortcutKey))
                {
                    // Solo activar si el panel origen está activo
                    if (IsPanelActive(config.panelID))
                    {
                        NavigateToPanel(link.toPanel, link.addToHistory);
                        link.onNavigate?.Invoke();
                    }
                }
            }
        }
    }
    
    void HandleGamepadInput()
    {
        // Implementación básica de soporte para gamepad
        if (Input.GetButtonDown("Cancel"))
        {
            uiManager.GoBack();
        }
        
        if (Input.GetButtonDown("Submit"))
        {
            // Activar el botón seleccionado
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                Button button = selected.GetComponent<Button>();
                button?.onClick.Invoke();
            }
        }
    }
    
    // Métodos públicos para navegación
    public void NavigateToPanel(string panelID, bool addToHistory = true)
    {
        if (uiManager != null)
        {
            uiManager.ShowPanel(panelID, addToHistory);
            
            // Invocar eventos de configuración
            if (configDictionary.TryGetValue(panelID, out UIPanelConfig config))
            {
                config.onPanelOpen?.Invoke();
            }
        }
    }
    
    public void NavigateBack()
    {
        uiManager?.GoBack();
    }
    
    public void TogglePause()
    {
        if (IsPanelActive("HUD"))
        {
            NavigateToPanel("PauseMenu");
        }
        else if (IsPanelActive("PauseMenu"))
        {
            NavigateToPanel("HUD");
        }
    }
    
    public void ToggleInventory()
    {
        if (IsPanelActive("HUD"))
        {
            NavigateToPanel("Inventory");
        }
        else if (IsPanelActive("Inventory"))
        {
            NavigateBack();
        }
    }
    
    public void QuickSave()
    {
        Debug.Log("💾 Quick Save triggered");
        // Aquí iría la lógica de guardado rápido
    }
    
    public void QuickLoad()
    {
        Debug.Log("📂 Quick Load triggered");
        // Aquí iría la lógica de carga rápida
    }
    
    // Utilidades
    public bool IsPanelActive(string panelID)
    {
        var panel = uiManager?.GetPanel(panelID);
        return panel != null && panel.gameObject.activeInHierarchy;
    }
    
    public void ShowPanel(string panelID)
    {
        NavigateToPanel(panelID);
    }
    
    public void HidePanel(string panelID)
    {
        uiManager?.HidePanel(panelID);
    }
    
    // Métodos de configuración
    public void AddNavigationLink(string fromPanel, string toPanel, Button button = null, KeyCode shortcut = KeyCode.None)
    {
        var config = configDictionary.GetValueOrDefault(fromPanel);
        if (config == null)
        {
            config = new UIPanelConfig { panelID = fromPanel };
            panelConfigs.Add(config);
            configDictionary[fromPanel] = config;
        }
        
        var link = new UINavigationLink
        {
            linkName = $"{fromPanel} → {toPanel}",
            fromPanel = fromPanel,
            toPanel = toPanel,
            triggerButton = button,
            shortcutKey = shortcut,
            addToHistory = true
        };
        
        config.navigationLinks.Add(link);
        SetupSingleLink(link);
    }
    
    public void RemoveNavigationLink(string fromPanel, string toPanel)
    {
        if (configDictionary.TryGetValue(fromPanel, out UIPanelConfig config))
        {
            config.navigationLinks.RemoveAll(link => link.toPanel == toPanel);
        }
    }
    
    // Debug UI
    void OnGUI()
    {
        if (!showDebugUI || !isInitialized) return;
        
        GUI.BeginGroup(new Rect(10, 10, 300, 500));
        GUI.Box(new Rect(0, 0, 300, 500), "");
        
        GUI.Label(new Rect(10, 10, 280, 30), "🎮 UI Navigation Debug");
        
        int yPos = 50;
        GUI.Label(new Rect(10, yPos, 280, 20), "Active Panels:");
        yPos += 25;
        
        foreach (var config in panelConfigs)
        {
            if (IsPanelActive(config.panelID))
            {
                GUI.Label(new Rect(10, yPos, 280, 20), $"✓ {config.panelID}");
                yPos += 20;
            }
        }
        
        yPos += 10;
        GUI.Label(new Rect(10, yPos, 280, 20), "Quick Navigation:");
        yPos += 25;
        
        foreach (var config in panelConfigs)
        {
            if (GUI.Button(new Rect(10, yPos, 280, 25), $"Go to {config.panelID}"))
            {
                NavigateToPanel(config.panelID);
            }
            yPos += 30;
        }
        
        GUI.EndGroup();
    }
    
    // Métodos de utilidad para el editor
    [ContextMenu("Auto-Detect Panels")]
    public void AutoDetectPanels()
    {
        panelConfigs.Clear();
        
        UIPanel[] allPanels = FindObjectsOfType<UIPanel>(true);
        foreach (var panel in allPanels)
        {
            var config = new UIPanelConfig
            {
                panelID = panel.panelID,
                panelObject = panel.gameObject,
                isMainPanel = panel.panelID == "MainMenu"
            };
            
            // Auto-detectar botones y crear enlaces
            Button[] buttons = panel.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                // Intentar inferir el destino por el nombre del botón
                string buttonName = button.name.ToLower();
                string targetPanel = InferTargetPanel(buttonName, panel.panelID);
                
                if (!string.IsNullOrEmpty(targetPanel))
                {
                    config.navigationLinks.Add(new UINavigationLink
                    {
                        linkName = $"{button.name} Link",
                        fromPanel = panel.panelID,
                        toPanel = targetPanel,
                        triggerButton = button
                    });
                }
            }
            
            panelConfigs.Add(config);
        }
        
        Debug.Log($"✅ Auto-detected {panelConfigs.Count} panels");
    }
    
    string InferTargetPanel(string buttonName, string currentPanel)
    {
        // Lógica simple para inferir destinos basada en nombres comunes
        if (buttonName.Contains("new") || buttonName.Contains("nueva"))
            return "NewGame";
        if (buttonName.Contains("load") || buttonName.Contains("cargar"))
            return "LoadGame";
        if (buttonName.Contains("option") || buttonName.Contains("opcion"))
            return "OptionsMain";
        if (buttonName.Contains("credit") || buttonName.Contains("credito"))
            return "Credits";
        if (buttonName.Contains("exit") || buttonName.Contains("salir"))
            return "ExitConfirm";
        if (buttonName.Contains("back") || buttonName.Contains("volver"))
            return ""; // Usar GoBack
        if (buttonName.Contains("continue") || buttonName.Contains("continuar"))
            return currentPanel == "PauseMenu" ? "HUD" : "";
        if (buttonName.Contains("video"))
            return "VideoOptions";
        if (buttonName.Contains("audio"))
            return "AudioOptions";
        if (buttonName.Contains("control"))
            return "ControlsOptions";
        if (buttonName.Contains("gameplay") || buttonName.Contains("jugabilidad"))
            return "GameplayOptions";
        
        return "";
    }
    
    [ContextMenu("Validate Configuration")]
    public void ValidateConfiguration()
    {
        int errors = 0;
        int warnings = 0;
        
        foreach (var config in panelConfigs)
        {
            if (string.IsNullOrEmpty(config.panelID))
            {
                Debug.LogError($"❌ Panel configuration has empty ID");
                errors++;
            }
            
            if (config.panelObject == null)
            {
                Debug.LogWarning($"⚠️ Panel '{config.panelID}' has no GameObject assigned");
                warnings++;
            }
            
            foreach (var link in config.navigationLinks)
            {
                if (string.IsNullOrEmpty(link.toPanel))
                {
                    Debug.LogWarning($"⚠️ Navigation link in '{config.panelID}' has no target panel");
                    warnings++;
                }
                
                if (link.triggerButton == null && link.shortcutKey == KeyCode.None)
                {
                    Debug.LogWarning($"⚠️ Navigation link '{link.linkName}' has no trigger");
                    warnings++;
                }
            }
        }
        
        Debug.Log($"🔍 Validation complete: {errors} errors, {warnings} warnings");
    }
}