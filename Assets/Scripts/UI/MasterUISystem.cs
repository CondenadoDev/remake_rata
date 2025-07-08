using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// SISTEMA UI MAESTRO CORREGIDO - Genera automáticamente toda la interfaz funcional del juego
/// Version 2.0 - Corrige problemas de inicialización y navegación
/// </summary>
public class MasterUISystem : MonoBehaviour
{
    [Header("MASTER UI SYSTEM")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool destroyExistingUI = true;
    
    [Header("Visual Settings")]
    [SerializeField] private Color primaryColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Color secondaryColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color accentColor = new Color(1f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    
    [Header("Game Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("Generation Status")]
    [SerializeField] private bool isGenerated = false;
    [SerializeField] private int totalPanelsCreated = 0;
    [SerializeField] private int totalControlsCreated = 0;
    
    // Componentes principales
    private Canvas mainCanvas;
    private UIManager uiManager;
    private EventSystem eventSystem;
    private AudioMixer masterMixer;
    
    // Diccionarios para organización
    private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
    private Dictionary<string, ConcreteUIPanel> panelComponents = new Dictionary<string, ConcreteUIPanel>();
    
    // Configuraciones del juego (valores por defecto)
    private GameSettings gameSettings = new GameSettings();
    
    [System.Serializable]
    public class GameSettings
    {
        [Header("Audio")]
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public float uiVolume = 0.6f;
        public float voiceVolume = 0.9f;
        
        [Header("Graphics")]
        public int qualityLevel = 2; // High
        public bool fullscreen = true;
        public bool vsync = true;
        public int targetFPS = 60;
        public int resolutionIndex = 0;
        public int antiAliasing = 4;
        public bool shadows = true;
        public bool postProcessing = true;
        public float renderScale = 1f;
        
        [Header("Input")]
        public float mouseSensitivity = 1f;
        public bool invertMouseY = false;
        public float gamepadSensitivity = 1f;
        public bool enableVibration = true;
        
        [Header("Gameplay")]
        public string difficulty = "Normal";
        public bool autoSave = true;
        public float autoSaveInterval = 300f;
        public bool showTutorials = true;
        public bool showDamageNumbers = true;
        public string language = "Español";
        public bool subtitles = true;
    }
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateCompleteUISystem();
        }
    }
    
    [ContextMenu("Generate Complete UI System")]
    public void GenerateCompleteUISystem()
    {
        Debug.Log("[MasterUISystem] Iniciando generación completa...");
    
        try
        {
            LoadExistingSettings();
            CleanupExistingUI();
            CreateCoreComponents();
        
            // VERIFICACIÓN AGREGADA
            VerifyComponents();
        
            CreateAllPanels();
            SetupCompleteNavigation();
            ConfigureUIManager();
            FinalizeSystem();
        
            isGenerated = true;
            Debug.Log($"[MasterUISystem] Sistema generado exitosamente! Paneles: {totalPanelsCreated}, Controles: {totalControlsCreated}");
        
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("UI System Generated", 
                $"Sistema UI completo generado exitosamente!\n\nPaneles: {totalPanelsCreated}\nControles: {totalControlsCreated}\n\n¡Todo listo para usar!", 
                "¡Perfecto!");
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MasterUISystem] Error durante la generación: {e.Message}\n{e.StackTrace}");
            isGenerated = false;
        
            // Limpiar en caso de error
            panels.Clear();
            panelComponents.Clear();
            totalPanelsCreated = 0;
            totalControlsCreated = 0;
        }
    }
    
    void LoadExistingSettings()
    {
        // Cargar configuraciones guardadas
        gameSettings.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        gameSettings.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        gameSettings.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        gameSettings.qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
        gameSettings.fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        gameSettings.vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
        gameSettings.mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        gameSettings.difficulty = PlayerPrefs.GetString("Difficulty", "Normal");
        
        Debug.Log("[MasterUISystem] Configuraciones cargadas");
    }
    
    void CleanupExistingUI()
    {
        if (!destroyExistingUI) return;
        
        // Limpiar UI existente
        Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in existingCanvases)
        {
            if (canvas.name.Contains("UI") || canvas.name.Contains("Canvas") || canvas.name.Contains("MASTER"))
            {
                DestroyImmediate(canvas.gameObject);
            }
        }
        
        UIManager existingManager = FindObjectOfType<UIManager>();
        if (existingManager != null && existingManager != uiManager)
            DestroyImmediate(existingManager.gameObject);
            
        EventSystem existingEventSystem = FindObjectOfType<EventSystem>();
        if (existingEventSystem != null && existingEventSystem != eventSystem)
            DestroyImmediate(existingEventSystem.gameObject);
            
        Debug.Log("[MasterUISystem] UI existente limpiada");
    }
    
    void CreateCoreComponents()
    {
        // Canvas principal
        GameObject canvasGO = new GameObject("MASTER UI CANVAS");
        mainCanvas = canvasGO.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 0;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // EventSystem
        GameObject eventSystemGO = new GameObject("EVENT SYSTEM");
        eventSystem = eventSystemGO.AddComponent<EventSystem>();
        StandaloneInputModule inputModule = eventSystemGO.AddComponent<StandaloneInputModule>();
        
        // UIManager
        GameObject uiManagerGO = new GameObject("UI MANAGER");
        uiManager = uiManagerGO.AddComponent<UIManager>();
        
        // AudioMixer (crear uno básico si no existe)
        CreateAudioMixer();
        
        Debug.Log("[MasterUISystem] Componentes core creados");
    }
    
    void CreateAudioMixer()
    {
        // Buscar AudioMixer existente
        masterMixer = Resources.Load<AudioMixer>("MasterMixer");
        
        if (masterMixer == null)
        {
            Debug.Log("[MasterUISystem] AudioMixer no encontrado, usando configuración directa");
        }
        else
        {
            Debug.Log("[MasterUISystem] AudioMixer encontrado y configurado");
        }
    }
    
    // Agrega este método de verificación antes de CreateAllPanels:
    void VerifyComponents()
    {
        if (mainCanvas == null)
        {
            Debug.LogError("[MasterUISystem] mainCanvas es null");
            return;
        }
    
        if (uiManager == null)
        {
            Debug.LogError("[MasterUISystem] uiManager es null");
            return;
        }
    
        Debug.Log("[MasterUISystem] Componentes verificados correctamente");
    }
    void CreateAllPanels()
    {
        Debug.Log("[MasterUISystem] Generando todos los paneles...");
        
        // 1. Menú Principal
        CreateMainMenuPanel();
        
        // 2. Opciones Principal
        CreateOptionsMainPanel();
        
        // 3. Opciones específicas
        CreateAudioOptionsPanel();
        CreateGraphicsOptionsPanel();
        CreateControlsOptionsPanel();
        CreateGameplayOptionsPanel();
        
        // 4. Juego
        CreateNewGamePanel();
        CreateLoadGamePanel();
        CreateHUDPanel();
        CreatePauseMenuPanel();
        CreateInventoryPanel();
        
        // 5. Misceláneos
        CreateCreditsPanel();
        CreateConfirmExitPanel();
        
        Debug.Log($"[MasterUISystem] {totalPanelsCreated} paneles generados");
    }
    
    GameObject CreateBasePanel(string panelName, string panelID, bool startVisible = false)
    {
        try
        {
            GameObject panelGO = new GameObject($"{panelName}");
            panelGO.transform.SetParent(mainCanvas.transform, false);
        
            // RectTransform full-screen
            RectTransform rect = panelGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        
            // Background
            Image bg = panelGO.AddComponent<Image>();
            bg.color = backgroundColor;
        
            // CanvasGroup para animaciones
            CanvasGroup canvasGroup = panelGO.AddComponent<CanvasGroup>();
        
            // ConcreteUIPanel - SIMPLIFICADO
            ConcreteUIPanel panel = panelGO.AddComponent<ConcreteUIPanel>();
        
            if (panel != null)
            {
                // Configurar ANTES de Initialize - NO llamar Initialize aquí para evitar duplicados
                SetPanelProperties(panel, panelID, startVisible);
            
                // NO llamar panel.Initialize() aquí - lo hará el UIManager después
            
                panels[panelID] = panelGO;
                panelComponents[panelID] = panel;
                totalPanelsCreated++;
            
                panelGO.SetActive(startVisible);
            
                return panelGO;
            }
            else
            {
                Debug.LogError($"[MasterUISystem] No se pudo crear ConcreteUIPanel para {panelID}");
                DestroyImmediate(panelGO);
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MasterUISystem] Error creando panel base {panelID}: {e.Message}");
            return null;
        }
    }
    
    // Reemplaza el método SetPanelProperties completo:
    void SetPanelProperties(ConcreteUIPanel panel, string panelID, bool startVisible)
    {
        // Configurar panelID directamente - SIMPLIFICADO
        if (panel != null && !string.IsNullOrEmpty(panelID))
        {
            panel.panelID = panelID;
        
            // Forzar configuración del nombre del GameObject para que coincida
            panel.gameObject.name = panelID;
        
            // Solo configurar propiedades básicas sin reflection problemática
            // El panel manejará su propia inicialización cuando el UIManager lo llame
            Debug.Log($"[MasterUISystem] Panel configurado: {panelID}");
        }
        else
        {
            Debug.LogError($"[MasterUISystem] Panel es null o panelID vacío: {panelID}");
        }
    }
    
    Button CreateStyledButton(Transform parent, string text, Vector2 position, Vector2 size, System.Action onClick = null)
    {
        GameObject btnGO = new GameObject($"Button_{text}");
        btnGO.transform.SetParent(parent, false);
        
        RectTransform rect = btnGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Image img = btnGO.AddComponent<Image>();
        img.color = primaryColor;
        
        Button btn = btnGO.AddComponent<Button>();
        
        // Colores del botón
        ColorBlock colors = btn.colors;
        colors.normalColor = primaryColor;
        colors.highlightedColor = primaryColor * 1.3f;
        colors.pressedColor = primaryColor * 0.7f;
        colors.selectedColor = primaryColor * 1.1f;
        btn.colors = colors;
        
        // Texto
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        TextMeshProUGUI txtComponent = textGO.AddComponent<TextMeshProUGUI>();
        txtComponent.text = text;
        txtComponent.color = Color.white;
        txtComponent.fontSize = 18;
        txtComponent.alignment = TextAlignmentOptions.Center;
        txtComponent.fontStyle = FontStyles.Bold;
        
        // Eventos
        if (onClick != null)
        {
            btn.onClick.AddListener(() => {
                Debug.Log($"[MasterUISystem] Botón presionado: {text}");
                onClick.Invoke();
            });
        }
        
        totalControlsCreated++;
        return btn;
    }
    
    TextMeshProUGUI CreateTitle(Transform parent, string text, Vector2 position, float fontSize = 36)
    {
        GameObject titleGO = new GameObject($"Title_{text}");
        titleGO.transform.SetParent(parent, false);
        
        RectTransform rect = titleGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(800, fontSize * 2);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        
        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = text;
        title.fontSize = fontSize;
        title.color = Color.white;
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        
        Shadow shadow = titleGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);
        
        return title;
    }
    
    Slider CreateSlider(Transform parent, string label, Vector2 position, float minVal, float maxVal, float currentVal, System.Action<float> onChanged = null)
    {
        GameObject container = new GameObject($"Slider_{label}");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(500, 40);
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(container.transform, false);
        
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(-150, 0);
        labelRect.sizeDelta = new Vector2(200, 30);
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.pivot = new Vector2(1, 0.5f);
        
        TextMeshProUGUI labelTxt = labelGO.AddComponent<TextMeshProUGUI>();
        labelTxt.text = label + ":";
        labelTxt.fontSize = 16;
        labelTxt.color = Color.white;
        labelTxt.alignment = TextAlignmentOptions.Right;
        
        // Slider
        GameObject sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(container.transform, false);
        
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(50, 0);
        sliderRect.sizeDelta = new Vector2(200, 20);
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(0, 0.5f);
        
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = minVal;
        slider.maxValue = maxVal;
        slider.value = currentVal;
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(10, 0);
        fillAreaRect.offsetMax = new Vector2(-10, 0);
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = primaryColor;
        
        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);
        
        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        
        // Configurar referencias
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
        
        // Value display
        GameObject valueGO = new GameObject("Value");
        valueGO.transform.SetParent(container.transform, false);
        RectTransform valueRect = valueGO.AddComponent<RectTransform>();
        valueRect.anchoredPosition = new Vector2(200, 0);
        valueRect.sizeDelta = new Vector2(80, 30);
        valueRect.anchorMin = new Vector2(0, 0.5f);
        valueRect.anchorMax = new Vector2(0, 0.5f);
        
        TextMeshProUGUI valueTxt = valueGO.AddComponent<TextMeshProUGUI>();
        valueTxt.fontSize = 14;
        valueTxt.color = secondaryColor;
        valueTxt.alignment = TextAlignmentOptions.Center;
        
        // Actualizar valor
        System.Action updateValue = () => {
            if (maxVal <= 1f)
                valueTxt.text = $"{(int)(slider.value * 100)}%";
            else
                valueTxt.text = $"{(int)slider.value}";
        };
        
        updateValue();
        
        slider.onValueChanged.AddListener((float value) => {
            updateValue();
            onChanged?.Invoke(value);
        });
        
        totalControlsCreated++;
        return slider;
    }
    
    Toggle CreateToggle(Transform parent, string label, Vector2 position, bool isOn, System.Action<bool> onChanged = null)
    {
        GameObject toggleGO = new GameObject($"Toggle_{label}");
        toggleGO.transform.SetParent(parent, false);
        
        RectTransform rect = toggleGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 30);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Toggle toggle = toggleGO.AddComponent<Toggle>();
        toggle.isOn = isOn;
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(toggleGO.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchoredPosition = new Vector2(-150, 0);
        bgRect.sizeDelta = new Vector2(25, 25);
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Checkmark
        GameObject check = new GameObject("Checkmark");
        check.transform.SetParent(bg.transform, false);
        RectTransform checkRect = check.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.offsetMin = new Vector2(3, 3);
        checkRect.offsetMax = new Vector2(-3, -3);
        Image checkImg = check.AddComponent<Image>();
        checkImg.color = primaryColor;
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(toggleGO.transform, false);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(-100, 0);
        labelRect.sizeDelta = new Vector2(300, 30);
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        
        TextMeshProUGUI labelTxt = labelGO.AddComponent<TextMeshProUGUI>();
        labelTxt.text = label;
        labelTxt.fontSize = 16;
        labelTxt.color = Color.white;
        labelTxt.alignment = TextAlignmentOptions.Left;
        
        // Configurar
        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;
        
        if (onChanged != null)
            toggle.onValueChanged.AddListener(onChanged.Invoke);
        
        totalControlsCreated++;
        return toggle;
    }
    
    TMP_Dropdown CreateDropdown(Transform parent, string label, Vector2 position, string[] options, int selectedIndex, System.Action<int> onChanged = null)
    {
        GameObject container = new GameObject($"Dropdown_{label}");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(500, 30);
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(container.transform, false);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(-150, 0);
        labelRect.sizeDelta = new Vector2(200, 30);
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.pivot = new Vector2(1, 0.5f);
        
        TextMeshProUGUI labelTxt = labelGO.AddComponent<TextMeshProUGUI>();
        labelTxt.text = label + ":";
        labelTxt.fontSize = 16;
        labelTxt.color = Color.white;
        labelTxt.alignment = TextAlignmentOptions.Right;
        
        // Dropdown simplificado
        GameObject dropdownGO = new GameObject("Dropdown");
        dropdownGO.transform.SetParent(container.transform, false);
        RectTransform dropRect = dropdownGO.AddComponent<RectTransform>();
        dropRect.anchoredPosition = new Vector2(50, 0);
        dropRect.sizeDelta = new Vector2(200, 25);
        dropRect.anchorMin = new Vector2(0, 0.5f);
        dropRect.anchorMax = new Vector2(0, 0.5f);
        
        Image dropImg = dropdownGO.AddComponent<Image>();
        dropImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        TMP_Dropdown dropdown = dropdownGO.AddComponent<TMP_Dropdown>();
        
        // Crear template simplificado
        CreateDropdownTemplate(dropdown, dropdownGO);
        
        // Caption text
        GameObject captionGO = new GameObject("Label");
        captionGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform captionRect = captionGO.AddComponent<RectTransform>();
        captionRect.anchorMin = Vector2.zero;
        captionRect.anchorMax = Vector2.one;
        captionRect.offsetMin = new Vector2(10, 2);
        captionRect.offsetMax = new Vector2(-25, -2);
        TextMeshProUGUI captionTxt = captionGO.AddComponent<TextMeshProUGUI>();
        captionTxt.color = Color.white;
        captionTxt.fontSize = 14;
        captionTxt.alignment = TextAlignmentOptions.Left;
        
        dropdown.captionText = captionTxt;
        
        // Configurar opciones
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        dropdown.value = selectedIndex;
        
        if (onChanged != null)
            dropdown.onValueChanged.AddListener(onChanged.Invoke);
        
        totalControlsCreated++;
        return dropdown;
    }
    
    void CreateDropdownTemplate(TMP_Dropdown dropdown, GameObject dropdownGO)
    {
        // Template básico funcional
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownGO.transform, false);
        template.SetActive(false);
        
        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 120);
        
        Image templateImg = template.AddComponent<Image>();
        templateImg.color = backgroundColor;
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        Mask mask = viewport.AddComponent<Mask>();
        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = Color.clear;
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 28);
        
        // Item template
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 20);
        
        Toggle itemToggle = item.AddComponent<Toggle>();
        
        // Item background
        GameObject itemBg = new GameObject("Item Background");
        itemBg.transform.SetParent(item.transform, false);
        RectTransform itemBgRect = itemBg.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.offsetMin = Vector2.zero;
        itemBgRect.offsetMax = Vector2.zero;
        Image itemBgImg = itemBg.AddComponent<Image>();
        itemBgImg.color = Color.white;
        
        // Item checkmark
        GameObject itemCheck = new GameObject("Item Checkmark");
        itemCheck.transform.SetParent(item.transform, false);
        RectTransform checkRect = itemCheck.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0, 0.5f);
        checkRect.anchorMax = new Vector2(0, 0.5f);
        checkRect.sizeDelta = new Vector2(20, 20);
        checkRect.anchoredPosition = new Vector2(10, 0);
        Image checkImg = itemCheck.AddComponent<Image>();
        checkImg.color = primaryColor;
        
        // Item label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        RectTransform itemLabelRect = itemLabel.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(20, 1);
        itemLabelRect.offsetMax = new Vector2(-10, -2);
        TextMeshProUGUI itemTxt = itemLabel.AddComponent<TextMeshProUGUI>();
        itemTxt.color = Color.black;
        itemTxt.fontSize = 14;
        itemTxt.alignment = TextAlignmentOptions.Left;
        
        // Configurar referencias
        dropdown.template = templateRect;
        dropdown.itemText = itemTxt;
        
        ScrollRect scrollRect = template.AddComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        
        itemToggle.targetGraphic = itemBgImg;
        itemToggle.graphic = checkImg;
    }
    
    // ==================== CREACIÓN DE PANELES ====================
    
    void CreateMainMenuPanel()
    {
        GameObject panel = CreateBasePanel("MAIN MENU", "MainMenu", true);
        
        CreateTitle(panel.transform, "EPIC GAME", new Vector2(0, 250), 48);
        CreateTitle(panel.transform, "Aventura Procedural", new Vector2(0, 200), 24);
        
        CreateStyledButton(panel.transform, "NUEVA PARTIDA", new Vector2(0, 100), new Vector2(300, 50), 
            () => ShowPanel("NewGame"));
        
        CreateStyledButton(panel.transform, "CARGAR PARTIDA", new Vector2(0, 40), new Vector2(300, 50), 
            () => ShowPanel("LoadGame"));
        
        CreateStyledButton(panel.transform, "OPCIONES", new Vector2(0, -20), new Vector2(300, 50), 
            () => ShowPanel("OptionsMain"));
        
        CreateStyledButton(panel.transform, "CREDITOS", new Vector2(0, -80), new Vector2(300, 50), 
            () => ShowPanel("Credits"));
        
        CreateStyledButton(panel.transform, "SALIR", new Vector2(0, -140), new Vector2(300, 50), 
            () => ShowPanel("ConfirmExit"));
        
        // Versión
        CreateTitle(panel.transform, "v1.0.0", new Vector2(400, -400), 16);
    }
    
    // Reemplaza el método CreateNewGamePanel completo:
    void CreateNewGamePanel()
    {
        GameObject panel = CreateBasePanel("NEW GAME", "NewGame");
    
        if (panel == null)
        {
            Debug.LogError("[MasterUISystem] Error creando panel NewGame");
            return;
        }
    
        CreateTitle(panel.transform, "NUEVA PARTIDA", new Vector2(0, 300));
    
        CreateTitle(panel.transform, "Selecciona Dificultad:", new Vector2(0, 200), 24);
    
        // Dificultades - CORREGIDO
        string[] difficulties = { "Facil", "Normal", "Dificil", "Pesadilla" };
        for (int i = 0; i < difficulties.Length; i++)
        {
            int index = i; // Captura local
            bool isSelected = difficulties[i] == gameSettings.difficulty;
        
            try
            {
                Button diffBtn = CreateStyledButton(panel.transform, difficulties[i], 
                    new Vector2(0, 120 - (i * 60)), new Vector2(250, 50), 
                    () => {
                        gameSettings.difficulty = difficulties[index];
                        Debug.Log($"Dificultad seleccionada: {gameSettings.difficulty}");
                    });
            
                // VERIFICACIÓN DE NULL AGREGADA
                if (diffBtn != null && diffBtn.image != null && isSelected)
                {
                    diffBtn.image.color = accentColor;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MasterUISystem] Error creando botón dificultad {difficulties[i]}: {e.Message}");
            }
        }
    
        CreateStyledButton(panel.transform, "COMENZAR AVENTURA", new Vector2(-120, -280), new Vector2(200, 50), 
            StartNewGame);
    
        CreateStyledButton(panel.transform, "VOLVER", new Vector2(120, -280), new Vector2(200, 50), 
            () => ShowPanel("MainMenu"));
    }
    void StartNewGame()
    {
        Debug.Log("[MasterUISystem] Iniciando nueva partida...");
        
        // Guardar configuraciones actuales
        SaveAllSettings();
        
        // Aplicar configuraciones al juego
        ApplyGameSettings();
        
        // Cambiar a escena de juego
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log($"Cargando escena de juego: {gameSceneName}");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.Log("Simulando inicio de partida (escena no especificada)");
            ShowPanel("HUD");
        }
    }
    
    void CreateLoadGamePanel()
    {
        GameObject panel = CreateBasePanel("LOAD GAME", "LoadGame");
        
        CreateTitle(panel.transform, "CARGAR PARTIDA", new Vector2(0, 300));
        
        // Slots de guardado (simulados)
        for (int i = 0; i < 3; i++)
        {
            GameObject slotContainer = new GameObject($"Save Slot {i + 1}");
            slotContainer.transform.SetParent(panel.transform, false);
            
            RectTransform slotRect = slotContainer.AddComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(0, 150 - (i * 100));
            slotRect.sizeDelta = new Vector2(700, 80);
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            
            Image slotBg = slotContainer.AddComponent<Image>();
            slotBg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
            
            if (i == 0) // Simular save existente
            {
                CreateTitle(slotContainer.transform, $"Slot {i + 1}: Aventura Principal", new Vector2(-200, 15), 18);
                CreateTitle(slotContainer.transform, "Nivel 15 • 12:34:56 • 2025-01-15", new Vector2(-200, -15), 14);
                
                CreateStyledButton(slotContainer.transform, "CARGAR", new Vector2(200, 0), new Vector2(120, 40), 
                    () => {
                        Debug.Log($"Cargando partida slot {i + 1}");
                        StartNewGame(); // Por simplicidad, usar misma lógica
                    });
            }
            else
            {
                CreateTitle(slotContainer.transform, $"Slot {i + 1}: Vacio", new Vector2(0, 0), 20);
            }
        }
        
        CreateStyledButton(panel.transform, "VOLVER", new Vector2(0, -250), new Vector2(200, 50), 
            () => ShowPanel("MainMenu"));
    }
    
    void CreateOptionsMainPanel()
    {
        GameObject panel = CreateBasePanel("OPTIONS MAIN", "OptionsMain");
        
        CreateTitle(panel.transform, "CONFIGURACIONES", new Vector2(0, 300));
        
        CreateStyledButton(panel.transform, "AUDIO", new Vector2(0, 150), new Vector2(300, 60), 
            () => ShowPanel("AudioOptions"));
        
        CreateStyledButton(panel.transform, "GRAFICOS", new Vector2(0, 80), new Vector2(300, 60), 
            () => ShowPanel("GraphicsOptions"));
        
        CreateStyledButton(panel.transform, "CONTROLES", new Vector2(0, 10), new Vector2(300, 60), 
            () => ShowPanel("ControlsOptions"));
        
        CreateStyledButton(panel.transform, "JUGABILIDAD", new Vector2(0, -60), new Vector2(300, 60), 
            () => ShowPanel("GameplayOptions"));
        
        CreateStyledButton(panel.transform, "APLICAR TODO", new Vector2(-120, -180), new Vector2(200, 50), 
            ApplyAllSettings);
        
        CreateStyledButton(panel.transform, "VOLVER", new Vector2(120, -180), new Vector2(200, 50), 
            () => ShowPanel("MainMenu"));
    }
    
    void CreateAudioOptionsPanel()
    {
        GameObject panel = CreateBasePanel("AUDIO OPTIONS", "AudioOptions");
        
        CreateTitle(panel.transform, "CONFIGURACION DE AUDIO", new Vector2(0, 350));
        
        CreateSlider(panel.transform, "Volumen General", new Vector2(0, 250), 0, 1, gameSettings.masterVolume, 
            (value) => {
                gameSettings.masterVolume = value;
                ApplyAudioSettings();
            });
        
        CreateSlider(panel.transform, "Musica", new Vector2(0, 200), 0, 1, gameSettings.musicVolume, 
            (value) => {
                gameSettings.musicVolume = value;
                ApplyAudioSettings();
            });
        
        CreateSlider(panel.transform, "Efectos de Sonido", new Vector2(0, 150), 0, 1, gameSettings.sfxVolume, 
            (value) => {
                gameSettings.sfxVolume = value;
                ApplyAudioSettings();
            });
        
        CreateSlider(panel.transform, "Voces", new Vector2(0, 100), 0, 1, gameSettings.voiceVolume, 
            (value) => {
                gameSettings.voiceVolume = value;
                ApplyAudioSettings();
            });
        
        CreateSlider(panel.transform, "Interfaz", new Vector2(0, 50), 0, 1, gameSettings.uiVolume, 
            (value) => {
                gameSettings.uiVolume = value;
                ApplyAudioSettings();
            });
        
        string[] languages = { "Español", "English", "Français", "Deutsch", "日本語" };
        int langIndex = System.Array.IndexOf(languages, gameSettings.language);
        CreateDropdown(panel.transform, "Idioma", new Vector2(0, -20), languages, langIndex, 
            (index) => gameSettings.language = languages[index]);
        
        CreateToggle(panel.transform, "Subtitulos", new Vector2(0, -80), gameSettings.subtitles, 
            (value) => gameSettings.subtitles = value);
        
        CreateStyledButton(panel.transform, "PROBAR SONIDO", new Vector2(-120, -180), new Vector2(200, 50), 
            () => Debug.Log("Reproduciendo sonido de prueba..."));
        
        CreateStyledButton(panel.transform, "VOLVER", new Vector2(120, -180), new Vector2(200, 50), 
            () => ShowPanel("OptionsMain"));
    }
    
    void CreateGraphicsOptionsPanel()
    {
        GameObject panel = CreateBasePanel("GRAPHICS OPTIONS", "GraphicsOptions");
        
        CreateTitle(panel.transform, "CONFIGURACION GRAFICA", new Vector2(0, 400));
        
        // Crear todas las opciones gráficas con scroll
        int yOffset = 300;
        
        // Calidad general
        string[] qualities = { "Muy Bajo", "Bajo", "Medio", "Alto", "Muy Alto", "Ultra" };
        CreateDropdown(panel.transform, "Calidad General", new Vector2(0, yOffset), qualities, gameSettings.qualityLevel, 
            (index) => {
                gameSettings.qualityLevel = index;
                QualitySettings.SetQualityLevel(index);
            });
        yOffset -= 60;
        
        // Resolución
        string[] resolutions = { "1280x720", "1366x768", "1920x1080", "2560x1440", "3840x2160" };
        CreateDropdown(panel.transform, "Resolucion", new Vector2(0, yOffset), resolutions, gameSettings.resolutionIndex, 
            (index) => {
                gameSettings.resolutionIndex = index;
                ApplyGraphicsSettings();
            });
        yOffset -= 60;
        
        CreateToggle(panel.transform, "Pantalla Completa", new Vector2(0, yOffset), gameSettings.fullscreen, 
            (value) => {
                gameSettings.fullscreen = value;
                ApplyGraphicsSettings();
            });
        yOffset -= 50;
        
        CreateToggle(panel.transform, "VSync", new Vector2(0, yOffset), gameSettings.vsync, 
            (value) => {
                gameSettings.vsync = value;
                QualitySettings.vSyncCount = value ? 1 : 0;
            });
        yOffset -= 50;
        
        CreateSlider(panel.transform, "FPS Objetivo", new Vector2(0, yOffset), 30, 144, gameSettings.targetFPS, 
            (value) => {
                gameSettings.targetFPS = (int)value;
                Application.targetFrameRate = gameSettings.targetFPS;
            });
        yOffset -= 60;
        
        string[] aaOptions = { "Desactivado", "2x", "4x", "8x" };
        CreateDropdown(panel.transform, "Anti-Aliasing", new Vector2(0, yOffset), aaOptions, gameSettings.antiAliasing / 2, 
            (index) => {
                gameSettings.antiAliasing = index * 2;
                QualitySettings.antiAliasing = gameSettings.antiAliasing;
            });
        yOffset -= 60;
        
        CreateToggle(panel.transform, "Sombras", new Vector2(0, yOffset), gameSettings.shadows, 
            (value) => {
                gameSettings.shadows = value;
                QualitySettings.shadows = value ? ShadowQuality.All : ShadowQuality.Disable;
            });
        yOffset -= 50;
        
        CreateToggle(panel.transform, "Post-Procesado", new Vector2(0, yOffset), gameSettings.postProcessing, 
            (value) => gameSettings.postProcessing = value);
        yOffset -= 50;
        
        CreateSlider(panel.transform, "Escala de Renderizado", new Vector2(0, yOffset), 0.5f, 2f, gameSettings.renderScale, 
            (value) => gameSettings.renderScale = value);
        yOffset -= 80;
        
        CreateStyledButton(panel.transform, "APLICAR", new Vector2(-120, -380), new Vector2(200, 50), 
            ApplyGraphicsSettings);
        
        CreateStyledButton(panel.transform, "VOLVER", new Vector2(120, -380), new Vector2(200, 50), 
            () => ShowPanel("OptionsMain"));
    }
    
    void CreateControlsOptionsPanel()
    {
        GameObject panel = CreateBasePanel("CONTROLS OPTIONS", "ControlsOptions");
        
        CreateTitle(panel.transform, "CONFIGURACION DE CONTROLES", new Vector2(0, 350));
        
        CreateSlider(panel.transform, "Sensibilidad del Raton", new Vector2(0, 250), 0.1f, 3f, gameSettings.mouseSensitivity, 
            (value) => gameSettings.mouseSensitivity = value);
        
        CreateToggle(panel.transform, "Invertir Eje Y", new Vector2(0, 200), gameSettings.invertMouseY, 
            (value) => gameSettings.invertMouseY = value);
        
        CreateSlider(panel.transform, "Sensibilidad Gamepad", new Vector2(0, 150), 0.1f, 3f, gameSettings.gamepadSensitivity, 
            (value) => gameSettings.gamepadSensitivity = value);
        
        CreateToggle(panel.transform, "Vibracion Gamepad", new Vector2(0, 100), gameSettings.enableVibration, 
            (value) => gameSettings.enableVibration = value);
        
        // Controles principales (solo información)
        CreateTitle(panel.transform, "Controles Principales:", new Vector2(0, 40), 20);
        
        string[] controls = {
            "WASD - Movimiento",
            "Raton - Camara",
            "Espacio - Saltar",
            "Shift - Correr", 
            "E - Interactuar",
            "Tab - Inventario",
            "Esc - Pausa"
        };
        
        for (int i = 0; i < controls.Length; i++)
        {
            CreateTitle(panel.transform, controls[i], new Vector2(0, 0 - (i * 25)), 14);
        }
        
        CreateStyledButton(panel.transform, "RESTABLECER", new Vector2(-120, -280), new Vector2(200, 50), 
            () => {
                gameSettings.mouseSensitivity = 1f;
                gameSettings.invertMouseY = false;
                gameSettings.gamepadSensitivity = 1f;
                Debug.Log("Controles restablecidos a valores por defecto");
            });
        
        CreateStyledButton(panel.transform, "VOLVER", new Vector2(120, -280), new Vector2(200, 50), 
            () => ShowPanel("OptionsMain"));
    }
    
    void CreateGameplayOptionsPanel()
    {
        GameObject panel = CreateBasePanel("GAMEPLAY OPTIONS", "GameplayOptions");
        
        CreateTitle(panel.transform, "CONFIGURACION DE JUGABILIDAD", new Vector2(0, 350));
        
        string[] difficulties = { "Facil", "Normal", "Dificil", "Pesadilla" };
        int diffIndex = System.Array.IndexOf(difficulties, gameSettings.difficulty);
        CreateDropdown(panel.transform, "Dificultad", new Vector2(0, 250), difficulties, diffIndex, 
            (index) => gameSettings.difficulty = difficulties[index]);
        
        CreateToggle(panel.transform, "Auto-Guardado", new Vector2(0, 200), gameSettings.autoSave, 
            (value) => gameSettings.autoSave = value);
        
        CreateSlider(panel.transform, "Intervalo Auto-Guardado (min)", new Vector2(0, 150), 1, 10, gameSettings.autoSaveInterval / 60f, 
            (value) => gameSettings.autoSaveInterval = value * 60f);
        
        CreateToggle(panel.transform, "Mostrar Tutoriales", new Vector2(0, 100), gameSettings.showTutorials, 
            (value) => gameSettings.showTutorials = value);
        
        CreateToggle(panel.transform, "Numeros de Dano", new Vector2(0, 50), gameSettings.showDamageNumbers, 
            (value) => gameSettings.showDamageNumbers = value);
        
        CreateStyledButton(panel.transform, "RESTABLECER", new Vector2(-120, -220), new Vector2(200, 50), 
            () => {
                gameSettings.difficulty = "Normal";
                gameSettings.autoSave = true;
                gameSettings.showTutorials = true;
                Debug.Log("Opciones de gameplay restablecidas");
            });
        
        CreateStyledButton(panel.transform, "VOLVER", new Vector2(120, -220), new Vector2(200, 50), 
            () => ShowPanel("OptionsMain"));
    }
    
    void CreateHUDPanel()
    {
        GameObject panel = CreateBasePanel("HUD", "HUD");
        panel.GetComponent<Image>().color = Color.clear; // HUD transparente
        
        // Barras de estado (arriba izquierda)
        CreateHealthBar(panel.transform);
        CreateManaBar(panel.transform);
        CreateStaminaBar(panel.transform);
        
        // Minimapa (arriba derecha)
        CreateMinimap(panel.transform);
        
        // Hotbar (abajo centro)
        CreateHotbar(panel.transform);
        
        // Crosshair (centro)
        CreateCrosshair(panel.transform);
        
        // Info del objetivo (arriba centro)
        CreateObjectiveDisplay(panel.transform);
    }
    
    void CreateHealthBar(Transform parent)
    {
        GameObject healthContainer = new GameObject("Health Bar");
        healthContainer.transform.SetParent(parent, false);
        
        RectTransform rect = healthContainer.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(200, 25);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        
        Image bg = healthContainer.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthContainer.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0.8f, 1); // 80% de vida
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.red;
        
        CreateTitle(healthContainer.transform, "VIDA: 80/100", Vector2.zero, 14);
    }
    
    void CreateManaBar(Transform parent)
    {
        GameObject manaContainer = new GameObject("Mana Bar");
        manaContainer.transform.SetParent(parent, false);
        
        RectTransform rect = manaContainer.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(20, -50);
        rect.sizeDelta = new Vector2(200, 20);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        
        Image bg = manaContainer.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(manaContainer.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0.6f, 1); // 60% de maná
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.blue;
        
        CreateTitle(manaContainer.transform, "MANA: 60/100", Vector2.zero, 12);
    }
    
    void CreateStaminaBar(Transform parent)
    {
        GameObject staminaContainer = new GameObject("Stamina Bar");
        staminaContainer.transform.SetParent(parent, false);
        
        RectTransform rect = staminaContainer.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(20, -75);
        rect.sizeDelta = new Vector2(200, 15);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        
        Image bg = staminaContainer.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(staminaContainer.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0.9f, 1); // 90% de stamina
        fillRect.offsetMin = new Vector2(1, 1);
        fillRect.offsetMax = new Vector2(-1, -1);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.yellow;
    }
    
    void CreateMinimap(Transform parent)
    {
        GameObject minimapContainer = new GameObject("Minimap");
        minimapContainer.transform.SetParent(parent, false);
        
        RectTransform rect = minimapContainer.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-20, -20);
        rect.sizeDelta = new Vector2(150, 150);
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        
        Image bg = minimapContainer.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        CreateTitle(minimapContainer.transform, "MINIMAPA", Vector2.zero, 16);
    }
    
    void CreateHotbar(Transform parent)
    {
        GameObject hotbarContainer = new GameObject("Hotbar");
        hotbarContainer.transform.SetParent(parent, false);
        
        RectTransform rect = hotbarContainer.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 60);
        rect.sizeDelta = new Vector2(500, 60);
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        
        for (int i = 0; i < 8; i++)
        {
            GameObject slot = new GameObject($"Slot {i + 1}");
            slot.transform.SetParent(hotbarContainer.transform, false);
            
            RectTransform slotRect = slot.AddComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(-175 + (i * 50), 0);
            slotRect.sizeDelta = new Vector2(45, 45);
            slotRect.anchorMin = new Vector2(0, 0.5f);
            slotRect.anchorMax = new Vector2(0, 0.5f);
            
            Image slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            CreateTitle(slot.transform, (i + 1).ToString(), new Vector2(0, 20), 10);
            
            // Simular algunos items
            if (i == 0 || i == 2 || i == 5)
            {
                GameObject item = new GameObject("Item");
                item.transform.SetParent(slot.transform, false);
                RectTransform itemRect = item.AddComponent<RectTransform>();
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.one;
                itemRect.offsetMin = new Vector2(5, 5);
                itemRect.offsetMax = new Vector2(-5, -5);
                Image itemImg = item.AddComponent<Image>();
                itemImg.color = primaryColor;
            }
        }
    }
    
    void CreateCrosshair(Transform parent)
    {
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(parent, false);
        
        RectTransform rect = crosshair.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(20, 20);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Cruz vertical
        GameObject vertical = new GameObject("Vertical");
        vertical.transform.SetParent(crosshair.transform, false);
        RectTransform vRect = vertical.AddComponent<RectTransform>();
        vRect.sizeDelta = new Vector2(2, 16);
        Image vImg = vertical.AddComponent<Image>();
        vImg.color = new Color(1, 1, 1, 0.8f);
        
        // Cruz horizontal
        GameObject horizontal = new GameObject("Horizontal");
        horizontal.transform.SetParent(crosshair.transform, false);
        RectTransform hRect = horizontal.AddComponent<RectTransform>();
        hRect.sizeDelta = new Vector2(16, 2);
        Image hImg = horizontal.AddComponent<Image>();
        hImg.color = new Color(1, 1, 1, 0.8f);
    }
    
    void CreateObjectiveDisplay(Transform parent)
    {
        GameObject objContainer = new GameObject("Current Objective");
        objContainer.transform.SetParent(parent, false);
        
        RectTransform rect = objContainer.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -50);
        rect.sizeDelta = new Vector2(400, 50);
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        
        Image bg = objContainer.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        
        CreateTitle(objContainer.transform, "Objetivo: Explora el primer dungeon", Vector2.zero, 16);
    }
    
    void CreatePauseMenuPanel()
    {
        GameObject panel = CreateBasePanel("PAUSE MENU", "PauseMenu");
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
        
        CreateTitle(panel.transform, "JUEGO PAUSADO", new Vector2(0, 200));
        
        CreateStyledButton(panel.transform, "CONTINUAR", new Vector2(0, 100), new Vector2(250, 50), 
            () => ShowPanel("HUD"));
        
        CreateStyledButton(panel.transform, "INVENTARIO", new Vector2(0, 40), new Vector2(250, 50), 
            () => ShowPanel("Inventory"));
        
        CreateStyledButton(panel.transform, "OPCIONES", new Vector2(0, -20), new Vector2(250, 50), 
            () => ShowPanel("OptionsMain"));
        
        CreateStyledButton(panel.transform, "GUARDAR", new Vector2(0, -80), new Vector2(250, 50), 
            () => {
                SaveAllSettings();
                Debug.Log("Partida guardada");
            });
        
        CreateStyledButton(panel.transform, "MENU PRINCIPAL", new Vector2(0, -140), new Vector2(250, 50), 
            () => ShowPanel("MainMenu"));
    }
    
    void CreateInventoryPanel()
    {
        GameObject panel = CreateBasePanel("INVENTORY", "Inventory");
        
        CreateTitle(panel.transform, "INVENTARIO", new Vector2(0, 350));
        
        // Grid de inventario
        GameObject gridContainer = new GameObject("Inventory Grid");
        gridContainer.transform.SetParent(panel.transform, false);
        
        RectTransform gridRect = gridContainer.AddComponent<RectTransform>();
        gridRect.anchoredPosition = new Vector2(-150, 0);
        gridRect.sizeDelta = new Vector2(300, 400);
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        GridLayoutGroup grid = gridContainer.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(50, 50);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        
        // Crear slots
        for (int i = 0; i < 30; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(gridContainer.transform, false);
            
            Image slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Simular algunos items
            if (i % 4 == 0 || i % 7 == 0)
            {
                GameObject item = new GameObject("Item");
                item.transform.SetParent(slot.transform, false);
                RectTransform itemRect = item.AddComponent<RectTransform>();
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.one;
                itemRect.offsetMin = new Vector2(5, 5);
                itemRect.offsetMax = new Vector2(-5, -5);
                Image itemImg = item.AddComponent<Image>();
                itemImg.color = accentColor;
            }
        }
        
        // Panel de stats
        GameObject statsPanel = new GameObject("Stats Panel");
        statsPanel.transform.SetParent(panel.transform, false);
        
        RectTransform statsRect = statsPanel.AddComponent<RectTransform>();
        statsRect.anchoredPosition = new Vector2(200, 50);
        statsRect.sizeDelta = new Vector2(250, 300);
        statsRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Image statsBg = statsPanel.AddComponent<Image>();
        statsBg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        
        CreateTitle(statsPanel.transform, "ESTADISTICAS", new Vector2(0, 120), 18);
        
        string[] stats = {
            "Fuerza: 15",
            "Agilidad: 12", 
            "Inteligencia: 8",
            "Defensa: 10",
            "Suerte: 5"
        };
        
        for (int i = 0; i < stats.Length; i++)
        {
            CreateTitle(statsPanel.transform, stats[i], new Vector2(0, 70 - (i * 25)), 14);
        }
        
        CreateStyledButton(panel.transform, "CERRAR", new Vector2(0, -300), new Vector2(200, 50), 
            () => ShowPanel("HUD"));
    }
    
    void CreateCreditsPanel()
    {
        GameObject panel = CreateBasePanel("CREDITS", "Credits");
        
        CreateTitle(panel.transform, "CREDITOS", new Vector2(0, 350));
        
        string[] credits = {
            "",
            "EPIC GAME TITLE",
            "",
            "Desarrollado por:",
            "Tu Estudio Increible",
            "",
            "Programacion:",
            "- Master Developer",
            "- Junior Coder",
            "",
            "Arte y Diseno:",
            "- Art Director",
            "- 3D Artist",
            "",
            "Audio:",
            "- Sound Designer",
            "- Music Composer",
            "",
            "Herramientas Utilizadas:",
            "- Unity Engine",
            "- Visual Studio",
            "- Blender",
            "",
            "Agradecimientos Especiales:",
            "A todos los que hicieron posible este juego",
            "",
            "© 2025 Tu Estudio"
        };
        
        for (int i = 0; i < credits.Length; i++)
        {
            float yPos = 250 - (i * 20);
            if (yPos > -400) // Solo mostrar los que caben
            {
                TextMeshProUGUI creditText = CreateTitle(panel.transform, credits[i], new Vector2(0, yPos), 
                    credits[i].Contains("©") ? 12 : 
                    credits[i].Contains("-") ? 14 : 
                    credits[i] == "" ? 8 : 16);
                
                if (credits[i].Contains(":"))
                    creditText.color = accentColor;
                else if (credits[i].Contains("-"))
                    creditText.color = secondaryColor;
            }
        }
        
        CreateStyledButton(panel.transform, "VOLVER", new Vector2(0, -350), new Vector2(200, 50), 
            () => ShowPanel("MainMenu"));
    }
    
    void CreateConfirmExitPanel()
    {
        GameObject panel = CreateBasePanel("CONFIRM EXIT", "ConfirmExit");
        
        CreateTitle(panel.transform, "CONFIRMAR SALIDA", new Vector2(0, 100));
        
        CreateTitle(panel.transform, "¿Estas seguro de que quieres salir del juego?", new Vector2(0, 50), 18);
        CreateTitle(panel.transform, "Todo progreso no guardado se perdera.", new Vector2(0, 20), 16);
        
        CreateStyledButton(panel.transform, "SI, SALIR", new Vector2(-120, -50), new Vector2(200, 50), 
            () => {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });
        
        CreateStyledButton(panel.transform, "CANCELAR", new Vector2(120, -50), new Vector2(200, 50), 
            () => ShowPanel("MainMenu"));
    }
    
    // ==================== CONFIGURACIÓN Y NAVEGACIÓN ====================
    
    void SetupCompleteNavigation()
    {
        Debug.Log("[MasterUISystem] Configurando navegación completa...");
        
        // Configurar navegación entre paneles
        foreach (var panelKvp in panelComponents)
        {
            string panelID = panelKvp.Key;
            ConcreteUIPanel panel = panelKvp.Value;
            
            // Configurar navegación
            switch (panelID)
            {
                case "MainMenu":
                    break; // Es el panel raíz
                case "NewGame":
                case "LoadGame":
                case "OptionsMain":
                case "Credits":
                case "ConfirmExit":
                    panel.previousPanelID = "MainMenu";
                    break;
                case "AudioOptions":
                case "GraphicsOptions":
                case "ControlsOptions":
                case "GameplayOptions":
                    panel.previousPanelID = "OptionsMain";
                    break;
                case "PauseMenu":
                case "Inventory":
                    panel.previousPanelID = "HUD";
                    break;
            }
        }
        
        Debug.Log("[MasterUISystem] Navegación configurada");
    }
    
    void ConfigureUIManager()
    {
        Debug.Log("[MasterUISystem] Configurando UIManager...");
        
        if (uiManager != null)
        {
            // Convertir a array para UIManager
            UIPanel[] panelArray = panelComponents.Values.Cast<UIPanel>().ToArray();
            
            // Configurar UIManager usando reflection
            var field = typeof(UIManager).GetField("uiPanels", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            
            if (field != null)
            {
                field.SetValue(uiManager, panelArray);
                Debug.Log($"[MasterUISystem] UIManager configurado con {panelArray.Length} paneles");
                
                // Forzar inicialización del UIManager
                var initMethod = typeof(UIManager).GetMethod("InitializePanels", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                initMethod?.Invoke(uiManager, null);
            }
            else
            {
                Debug.LogWarning("[MasterUISystem] No se pudo configurar UIManager - campo uiPanels no encontrado");
            }
        }
    }
    
    void FinalizeSystem()
    {
        Debug.Log("[MasterUISystem] Finalizando sistema...");
        
        // Asegurar que solo MainMenu esté visible
        foreach (var panelKvp in panels)
        {
            bool shouldBeVisible = panelKvp.Key == "MainMenu";
            panelKvp.Value.SetActive(shouldBeVisible);
        }
        
        // Configurar primer botón seleccionado para gamepad
        if (eventSystem != null && panels.ContainsKey("MainMenu"))
        {
            Button firstButton = panels["MainMenu"].GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                eventSystem.SetSelectedGameObject(firstButton.gameObject);
            }
        }
        
        Debug.Log("[MasterUISystem] Sistema finalizado y listo");
    }
    
    // ==================== APLICACIÓN DE CONFIGURACIONES ====================
    
    void ApplyAllSettings()
    {
        Debug.Log("[MasterUISystem] Aplicando todas las configuraciones...");
        
        ApplyAudioSettings();
        ApplyGraphicsSettings();
        ApplyGameSettings();
        SaveAllSettings();
        
        Debug.Log("[MasterUISystem] Todas las configuraciones aplicadas");
    }
    
    void ApplyAudioSettings()
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("MasterVolume", gameSettings.masterVolume > 0.001f ? 
                Mathf.Log10(gameSettings.masterVolume) * 20f : -80f);
            masterMixer.SetFloat("MusicVolume", gameSettings.musicVolume > 0.001f ? 
                Mathf.Log10(gameSettings.musicVolume) * 20f : -80f);
            masterMixer.SetFloat("SFXVolume", gameSettings.sfxVolume > 0.001f ? 
                Mathf.Log10(gameSettings.sfxVolume) * 20f : -80f);
        }
        
        AudioListener.volume = gameSettings.masterVolume;
        
        Debug.Log($"[MasterUISystem] Audio aplicado - Master: {gameSettings.masterVolume:F2}");
    }
    
    void ApplyGraphicsSettings()
    {
        QualitySettings.SetQualityLevel(gameSettings.qualityLevel);
        QualitySettings.vSyncCount = gameSettings.vsync ? 1 : 0;
        Application.targetFrameRate = gameSettings.targetFPS;
        QualitySettings.antiAliasing = gameSettings.antiAliasing;
        QualitySettings.shadows = gameSettings.shadows ? ShadowQuality.All : ShadowQuality.Disable;
        
        // Aplicar resolución y fullscreen
        string[] resolutions = { "1280x720", "1366x768", "1920x1080", "2560x1440", "3840x2160" };
        if (gameSettings.resolutionIndex < resolutions.Length)
        {
            string[] parts = resolutions[gameSettings.resolutionIndex].Split('x');
            int width = int.Parse(parts[0]);
            int height = int.Parse(parts[1]);
            Screen.SetResolution(width, height, gameSettings.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
        
        Debug.Log($"[MasterUISystem] Gráficos aplicados - Calidad: {gameSettings.qualityLevel}, FPS: {gameSettings.targetFPS}");
    }
    
    void ApplyGameSettings()
    {
        Time.timeScale = 1f; // Asegurar tiempo normal
        
        // Aplicar configuraciones a sistemas específicos si existen
        if (ConfigurationManager.Instance != null)
        {
            if (ConfigurationManager.Audio != null)
            {
                ConfigurationManager.Audio.masterVolume = gameSettings.masterVolume;
                ConfigurationManager.Audio.musicVolume = gameSettings.musicVolume;
                ConfigurationManager.Audio.sfxVolume = gameSettings.sfxVolume;
            }
            
            if (ConfigurationManager.Input != null)
            {
                ConfigurationManager.Input.mouseSensitivity = gameSettings.mouseSensitivity;
                ConfigurationManager.Input.invertMouseY = gameSettings.invertMouseY;
            }
        }
        
        Debug.Log($"[MasterUISystem] Configuraciones de juego aplicadas - Dificultad: {gameSettings.difficulty}");
    }
    
    void SaveAllSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", gameSettings.masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", gameSettings.musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", gameSettings.sfxVolume);
        PlayerPrefs.SetFloat("UIVolume", gameSettings.uiVolume);
        PlayerPrefs.SetFloat("VoiceVolume", gameSettings.voiceVolume);
        
        PlayerPrefs.SetInt("QualityLevel", gameSettings.qualityLevel);
        PlayerPrefs.SetInt("Fullscreen", gameSettings.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", gameSettings.vsync ? 1 : 0);
        PlayerPrefs.SetInt("TargetFPS", gameSettings.targetFPS);
        PlayerPrefs.SetInt("ResolutionIndex", gameSettings.resolutionIndex);
        PlayerPrefs.SetInt("AntiAliasing", gameSettings.antiAliasing);
        PlayerPrefs.SetInt("Shadows", gameSettings.shadows ? 1 : 0);
        
        PlayerPrefs.SetFloat("MouseSensitivity", gameSettings.mouseSensitivity);
        PlayerPrefs.SetInt("InvertMouseY", gameSettings.invertMouseY ? 1 : 0);
        PlayerPrefs.SetFloat("GamepadSensitivity", gameSettings.gamepadSensitivity);
        
        PlayerPrefs.SetString("Difficulty", gameSettings.difficulty);
        PlayerPrefs.SetInt("AutoSave", gameSettings.autoSave ? 1 : 0);
        PlayerPrefs.SetFloat("AutoSaveInterval", gameSettings.autoSaveInterval);
        PlayerPrefs.SetInt("ShowTutorials", gameSettings.showTutorials ? 1 : 0);
        PlayerPrefs.SetInt("ShowDamageNumbers", gameSettings.showDamageNumbers ? 1 : 0);
        PlayerPrefs.SetString("Language", gameSettings.language);
        PlayerPrefs.SetInt("Subtitles", gameSettings.subtitles ? 1 : 0);
        
        PlayerPrefs.Save();
        
        Debug.Log("[MasterUISystem] Todas las configuraciones guardadas");
    }
    
    // ==================== MÉTODOS DE AYUDA ====================
    
    void ShowPanel(string panelID)
    {
        if (uiManager != null)
        {
            uiManager.ShowPanel(panelID);
        }
        else
        {
            Debug.LogError("[MasterUISystem] UIManager no está disponible");
        }
    }
    
    // ==================== MÉTODOS PÚBLICOS ====================
    
    [ContextMenu("Test All Panels")]
    public void TestAllPanels()
    {
        if (Application.isPlaying && uiManager != null)
        {
            StartCoroutine(TestNavigationSequence());
        }
    }
    
    System.Collections.IEnumerator TestNavigationSequence()
    {
        Debug.Log("[MasterUISystem] Iniciando prueba de navegación...");
        
        string[] testSequence = {
            "MainMenu", "OptionsMain", "AudioOptions", "OptionsMain",
            "GraphicsOptions", "OptionsMain", "MainMenu", "NewGame", "MainMenu"
        };
        
        foreach (string panelID in testSequence)
        {
            if (panels.ContainsKey(panelID))
            {
                ShowPanel(panelID);
                Debug.Log($"Mostrando: {panelID}");
                yield return new WaitForSeconds(1f);
            }
        }
        
        ShowPanel("MainMenu");
        Debug.Log("[MasterUISystem] Prueba de navegación completada");
    }
    
    [ContextMenu("Force Save Settings")]
    public void ForceSaveSettings()
    {
        SaveAllSettings();
    }
    
    [ContextMenu("Reset All Settings")]
    public void ResetAllSettings()
    {
        gameSettings = new GameSettings();
        ApplyAllSettings();
        Debug.Log("[MasterUISystem] Todas las configuraciones restablecidas");
    }
    
    [ContextMenu("Destroy UI System")]
    public void DestroyUISystem()
    {
        if (mainCanvas != null)
            DestroyImmediate(mainCanvas.gameObject);
        if (uiManager != null)
            DestroyImmediate(uiManager.gameObject);
        if (eventSystem != null)
            DestroyImmediate(eventSystem.gameObject);
            
        panels.Clear();
        panelComponents.Clear();
        isGenerated = false;
        totalPanelsCreated = 0;
        totalControlsCreated = 0;
        
        Debug.Log("[MasterUISystem] Sistema UI destruido");
    }
}