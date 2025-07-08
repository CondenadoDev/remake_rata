using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Genera un sistema UI completo e interactivo con todas las funcionalidades
/// </summary>
public class CompleteUIGenerator : MonoBehaviour
{
    [Header("🚀 Generation Settings")]
    [SerializeField] private bool generateOnStart = false;
    [SerializeField] private bool destroyExistingUI = true;
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("🎨 Visual Settings")]
    [SerializeField] private Color primaryColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color secondaryColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color accentColor = new Color(1f, 0.5f, 0.2f);
    [SerializeField] private Color backgroundDark = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    [SerializeField] private Color backgroundLight = new Color(0.2f, 0.2f, 0.25f, 0.9f);
    [SerializeField] private Font customFont;
    
    [Header("🎮 Game Settings")]
    [SerializeField] private int maxSaveSlots = 3;
    [SerializeField] private bool supportControllerInput = true;
    
    [Header("📊 Generation Results")]
    [SerializeField] private int panelsCreated = 0;
    [SerializeField] private int buttonsCreated = 0;
    [SerializeField] private int slidersCreated = 0;
    [SerializeField] private int togglesCreated = 0;
    [SerializeField] private int dropdownsCreated = 0;
    [SerializeField] private bool generationComplete = false;
    
    // Referencias internas
    private Canvas mainCanvas;
    private UIManager uiManager;
    private EventSystem eventSystem;
    private Dictionary<string, GameObject> createdPanels = new Dictionary<string, GameObject>();
    private List<ConcreteUIPanel> allPanels = new List<ConcreteUIPanel>();
    
    // Sistema de guardado simulado
    private class SaveData
    {
        public string saveName = "Empty Slot";
        public string playTime = "--:--:--";
        public int level = 0;
        public string lastSaveDate = "---";
        public bool isEmpty = true;
    }
    private SaveData[] saveSlots;
    
    // Configuraciones del juego
    private float masterVolume = 1f;
    private float musicVolume = 0.8f;
    private float sfxVolume = 1f;
    private float voiceVolume = 1f;
    private int graphicsQuality = 2; // 0=Low, 1=Medium, 2=High, 3=Ultra
    private bool fullscreen = true;
    private int resolutionIndex = 0;
    private float brightness = 0.5f;
    private float gamma = 1f;
    private bool vsync = true;
    private bool shadows = true;
    private bool antialiasing = true;
    private bool subtitles = true;
    private string language = "Español";
    private float mouseSensitivity = 0.5f;
    private bool invertY = false;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateCompleteUI();
        }
    }
    
    [ContextMenu("🚀 Generate Complete UI System")]
    public void GenerateCompleteUI()
    {
        uiManager = uiManager ?? FindObjectOfType<UIManager>();
        Debug.Log($"✅ UIManager asignado: {(uiManager != null ? "Sí" : "NO")}");
        LogDebug("🚀 Starting Complete UI Generation...");
        
        try
        {
            InitializeSaveSystem();
            ResetCounters();
            PrepareScene();
            CreateCanvas();
            CreateEventSystem();
            CreateUIManager();
            GenerateAllPanels();
            SetupNavigation();
            ConfigureUIManager();
            FinalizeGeneration();
            
            generationComplete = true;
            ShowGenerationSummary();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ UI Generation failed: {e.Message}\n{e.StackTrace}");
            generationComplete = false;
        }
    }
    
    void InitializeSaveSystem()
    {
        saveSlots = new SaveData[maxSaveSlots];
        for (int i = 0; i < maxSaveSlots; i++)
        {
            saveSlots[i] = new SaveData();
            
            // Simular algunos saves existentes
            if (i == 0)
            {
                saveSlots[i] = new SaveData
                {
                    saveName = "Aventura Principal",
                    playTime = "12:34:56",
                    level = 15,
                    lastSaveDate = "2025-01-15",
                    isEmpty = false
                };
            }
            else if (i == 1)
            {
                saveSlots[i] = new SaveData
                {
                    saveName = "Nueva Partida+",
                    playTime = "45:12:00",
                    level = 32,
                    lastSaveDate = "2025-01-10",
                    isEmpty = false
                };
            }
        }
    }
    
    void ResetCounters()
    {
        panelsCreated = 0;
        buttonsCreated = 0;
        slidersCreated = 0;
        togglesCreated = 0;
        dropdownsCreated = 0;
        createdPanels.Clear();
        allPanels.Clear();
    }
    
    void PrepareScene()
    {
        if (destroyExistingUI)
        {
            // Destruir Canvas existente
            Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in existingCanvases)
            {
                if (canvas.name.Contains("UI") || canvas.name.Contains("Canvas"))
                {
                    DestroyImmediate(canvas.gameObject);
                }
            }
            
            // Destruir UIManager existente
            UIManager existingManager = FindObjectOfType<UIManager>();
            if (existingManager != null)
            {
                DestroyImmediate(existingManager.gameObject);
            }
            
            // Destruir EventSystem existente
            EventSystem existingEventSystem = FindObjectOfType<EventSystem>();
            if (existingEventSystem != null)
            {
                DestroyImmediate(existingEventSystem.gameObject);
            }
        }
    }
    
    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Main UI Canvas");
        mainCanvas = canvasGO.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 0;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        LogDebug("✅ Canvas created");
    }
    
    void CreateEventSystem()
    {
        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystem = eventSystemGO.AddComponent<EventSystem>();
        
        StandaloneInputModule inputModule = eventSystemGO.AddComponent<StandaloneInputModule>();
        inputModule.horizontalAxis = "Horizontal";
        inputModule.verticalAxis = "Vertical";
        inputModule.submitButton = "Submit";
        inputModule.cancelButton = "Cancel";
        
        LogDebug("✅ EventSystem created");
    }
    
    void CreateUIManager()
    {
        GameObject uiManagerGO = new GameObject("UIManager");
        uiManager = uiManagerGO.AddComponent<UIManager>();
        
        LogDebug("✅ UIManager created");
    }
    
    void GenerateAllPanels()
    {
        LogDebug("🎨 Generating all UI panels...");
        
        // Menú Principal
        GenerateMainMenuPanel();
        
        // Nueva Partida
        GenerateNewGamePanel();
        
        // Cargar Partida
        GenerateLoadGamePanel();
        
        // Opciones (Menú principal de opciones)
        GenerateOptionsMainPanel();
        
        // Opciones - Video
        GenerateVideoOptionsPanel();
        
        // Opciones - Audio
        GenerateAudioOptionsPanel();
        
        // Opciones - Controles
        GenerateControlsOptionsPanel();
        
        // Opciones - Jugabilidad
        GenerateGameplayOptionsPanel();
        
        // Confirmación de Salida
        GenerateExitConfirmPanel();
        
        // HUD del Juego
        GenerateHUDPanel();
        
        // Menú de Pausa
        GeneratePauseMenuPanel();
        
        // Inventario
        GenerateInventoryPanel();
        
        // Diálogo
        GenerateDialoguePanel();
        
        // Confirmación Genérica
        GenerateConfirmationPanel();
        
        // Créditos
        GenerateCreditsPanel();
        
        LogDebug($"✅ All panels generated. Total: {panelsCreated}");
    }
    
    GameObject CreateBasePanel(string panelName, string panelID, bool startVisible = false)
    {
        GameObject panelGO = new GameObject(panelName);
        panelGO.transform.SetParent(mainCanvas.transform, false);
        
        RectTransform rectTransform = panelGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Fondo
        Image background = panelGO.AddComponent<Image>();
        background.color = backgroundDark;
        
        // Canvas Group para animaciones
        CanvasGroup canvasGroup = panelGO.AddComponent<CanvasGroup>();
        
        // Agregar UIPanel concreto
        ConcreteUIPanel panel = panelGO.AddComponent<ConcreteUIPanel>();
        panel.panelID = panelID;
        panel.startVisible = startVisible;
        panel.useScaleAnimation = true;
        panel.blockGameInput = true;
        
        allPanels.Add(panel);
        createdPanels[panelID] = panelGO;
        panelsCreated++;
        
        panelGO.SetActive(false);
        
        uiManager?.RegisterPanel(panel);
        
        return panelGO;
    }
    
    TextMeshProUGUI CreateTitle(Transform parent, string text, Vector2 position, float fontSize = 48)
    {
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(parent, false);
        
        RectTransform rectTransform = titleGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(800, 100);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = text;
        titleText.fontSize = fontSize;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        // Sombra
        Shadow shadow = titleGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);
        
        return titleText;
    }
    
    Button CreateButton(Transform parent, string text, Vector2 position, Vector2 size, System.Action onClick = null)
    {
        GameObject buttonGO = new GameObject($"Button_{text}");
        buttonGO.transform.SetParent(parent, false);
        
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = primaryColor;
        
        Button button = buttonGO.AddComponent<Button>();
        
        // Colores del botón
        ColorBlock colors = button.colors;
        colors.normalColor = primaryColor;
        colors.highlightedColor = primaryColor * 1.2f;
        colors.pressedColor = primaryColor * 0.8f;
        colors.selectedColor = primaryColor * 1.1f;
        colors.disabledColor = primaryColor * 0.5f;
        button.colors = colors;
        
        // Texto del botón
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.color = Color.white;
        buttonText.fontSize = 20;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        // Outline para mejor legibilidad
        Outline outline = buttonGO.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.5f);
        outline.effectDistance = new Vector2(1, -1);
        
        // Agregar onClick
        if (onClick != null)
        {
            button.onClick.AddListener(() =>
            {
                Debug.Log($"🖱 Botón '{text}' presionado");
                onClick?.Invoke();
            });
        }
        
        buttonsCreated++;
        return button;
    }
    
    Slider CreateSlider(Transform parent, string labelText, Vector2 position, Vector2 size, 
        float minValue = 0, float maxValue = 1, float currentValue = 0.5f, System.Action<float> onValueChanged = null)
    {
        GameObject sliderContainer = new GameObject($"Slider_{labelText}");
        sliderContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = sliderContainer.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(size.x + 150, size.y + 40);
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(sliderContainer.transform, false);
        
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(-75, 0);
        labelRect.sizeDelta = new Vector2(150, 30);
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.pivot = new Vector2(1, 0.5f);
        
        TextMeshProUGUI labelComponent = labelGO.AddComponent<TextMeshProUGUI>();
        labelComponent.text = labelText;
        labelComponent.color = Color.white;
        labelComponent.fontSize = 18;
        labelComponent.alignment = TextAlignmentOptions.Right;
        
        // Slider
        GameObject sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(sliderContainer.transform, false);
        
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(50, 0);
        sliderRect.sizeDelta = size;
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(0, 0.5f);
        sliderRect.pivot = new Vector2(0, 0.5f);
        
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = currentValue;
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderGO.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundLight;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = new Vector2(1, 1);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-15, 0);
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = primaryColor;
        
        // Handle Slide Area
        GameObject handleSlideArea = new GameObject("Handle Slide Area");
        handleSlideArea.transform.SetParent(sliderGO.transform, false);
        
        RectTransform handleAreaRect = handleSlideArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);
        
        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleSlideArea.transform, false);
        
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        
        // Configurar slider
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        
        // Value Text
        GameObject valueTextGO = new GameObject("Value");
        valueTextGO.transform.SetParent(sliderContainer.transform, false);
        
        RectTransform valueRect = valueTextGO.AddComponent<RectTransform>();
        valueRect.anchoredPosition = new Vector2(size.x + 100, 0);
        valueRect.sizeDelta = new Vector2(60, 30);
        valueRect.anchorMin = new Vector2(0, 0.5f);
        valueRect.anchorMax = new Vector2(0, 0.5f);
        
        TextMeshProUGUI valueText = valueTextGO.AddComponent<TextMeshProUGUI>();
        valueText.text = $"{(int)(currentValue * 100)}%";
        valueText.color = secondaryColor;
        valueText.fontSize = 16;
        valueText.alignment = TextAlignmentOptions.Center;
        
        // Actualizar texto cuando cambie el valor
        slider.onValueChanged.AddListener((float value) => {
            if (maxValue <= 1)
                valueText.text = $"{(int)(value * 100)}%";
            else
                valueText.text = $"{(int)value}";
                
            onValueChanged?.Invoke(value);
        });
        
        slidersCreated++;
        return slider;
    }
    
    Toggle CreateToggle(Transform parent, string labelText, Vector2 position, bool isOn = false, System.Action<bool> onValueChanged = null)
    {
        GameObject toggleGO = new GameObject($"Toggle_{labelText}");
        toggleGO.transform.SetParent(parent, false);
        
        RectTransform rectTransform = toggleGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(300, 30);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        
        Toggle toggle = toggleGO.AddComponent<Toggle>();
        toggle.isOn = isOn;
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleGO.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchoredPosition = new Vector2(-135, 0);
        bgRect.sizeDelta = new Vector2(30, 30);
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundLight;
        
        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(background.transform, false);
        
        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.offsetMin = new Vector2(5, 5);
        checkRect.offsetMax = new Vector2(-5, -5);
        
        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = primaryColor;
        
        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(toggleGO.transform, false);
        
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(20, 0);
        labelRect.sizeDelta = new Vector2(250, 30);
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        
        TextMeshProUGUI labelTMP = label.AddComponent<TextMeshProUGUI>();
        labelTMP.text = labelText;
        labelTMP.color = Color.white;
        labelTMP.fontSize = 18;
        labelTMP.alignment = TextAlignmentOptions.Left;
        
        // Configurar toggle
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        
        if (onValueChanged != null)
        {
            toggle.onValueChanged.AddListener((bool val) => onValueChanged?.Invoke(val));
        }
        
        togglesCreated++;
        return toggle;
    }
    
    TMP_Dropdown CreateDropdown(Transform parent, string labelText, Vector2 position, string[] options, int selectedIndex = 0, System.Action<int> onValueChanged = null)
    {
        GameObject dropdownContainer = new GameObject($"Dropdown_{labelText}");
        dropdownContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = dropdownContainer.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(400, 30);
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(dropdownContainer.transform, false);
        
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(-100, 0);
        labelRect.sizeDelta = new Vector2(150, 30);
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.pivot = new Vector2(1, 0.5f);
        
        TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text = labelText;
        labelTMP.color = Color.white;
        labelTMP.fontSize = 18;
        labelTMP.alignment = TextAlignmentOptions.Right;
        
        // Dropdown
        GameObject dropdownGO = new GameObject("Dropdown");
        dropdownGO.transform.SetParent(dropdownContainer.transform, false);
        
        RectTransform dropRect = dropdownGO.AddComponent<RectTransform>();
        dropRect.anchoredPosition = new Vector2(50, 0);
        dropRect.sizeDelta = new Vector2(200, 30);
        dropRect.anchorMin = new Vector2(0, 0.5f);
        dropRect.anchorMax = new Vector2(0, 0.5f);
        
        Image dropImage = dropdownGO.AddComponent<Image>();
        dropImage.color = backgroundLight;
        
        TMP_Dropdown dropdown = dropdownGO.AddComponent<TMP_Dropdown>();
        
        // Template (panel desplegable)
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownGO.transform, false);
        template.SetActive(false);
        
        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);
        
        Image templateImage = template.AddComponent<Image>();
        templateImage.color = backgroundDark;
        
        ScrollRect scrollRect = template.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(0, 0);
        viewportRect.offsetMax = new Vector2(-18, 0);
        
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.white;
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 28);
        
        // Item
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        
        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 20);
        
        Toggle itemToggle = item.AddComponent<Toggle>();
        
        // Item Background
        GameObject itemBg = new GameObject("Item Background");
        itemBg.transform.SetParent(item.transform, false);
        
        RectTransform itemBgRect = itemBg.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.offsetMin = Vector2.zero;
        itemBgRect.offsetMax = Vector2.zero;
        
        Image itemBgImage = itemBg.AddComponent<Image>();
        itemBgImage.color = Color.white;
        
        // Item Checkmark
        GameObject itemCheckmark = new GameObject("Item Checkmark");
        itemCheckmark.transform.SetParent(item.transform, false);
        
        RectTransform checkRect = itemCheckmark.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0, 0.5f);
        checkRect.anchorMax = new Vector2(0, 0.5f);
        checkRect.sizeDelta = new Vector2(20, 20);
        checkRect.anchoredPosition = new Vector2(10, 0);
        
        Image checkImage = itemCheckmark.AddComponent<Image>();
        checkImage.color = primaryColor;
        
        // Item Label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        
        RectTransform itemLabelRect = itemLabel.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(20, 1);
        itemLabelRect.offsetMax = new Vector2(-10, -2);
        
        TextMeshProUGUI itemText = itemLabel.AddComponent<TextMeshProUGUI>();
        itemText.color = Color.black;
        itemText.fontSize = 14;
        itemText.alignment = TextAlignmentOptions.Left;
        
        // Scrollbar
        GameObject scrollbar = new GameObject("Scrollbar");
        scrollbar.transform.SetParent(template.transform, false);
        
        RectTransform scrollbarRect = scrollbar.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = Vector2.one;
        scrollbarRect.sizeDelta = new Vector2(20, 0);
        scrollbarRect.anchoredPosition = Vector2.zero;
        
        Image scrollbarImage = scrollbar.AddComponent<Image>();
        scrollbarImage.color = backgroundLight;
        
        Scrollbar scrollbarComponent = scrollbar.AddComponent<Scrollbar>();
        scrollbarComponent.direction = Scrollbar.Direction.TopToBottom;
        
        // Scrollbar Handle
        GameObject scrollHandle = new GameObject("Sliding Area");
        scrollHandle.transform.SetParent(scrollbar.transform, false);
        
        RectTransform handleAreaRect = scrollHandle.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 10);
        handleAreaRect.offsetMax = new Vector2(-10, -10);
        
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(scrollHandle.transform, false);
        
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(-10, -10);
        handleRect.offsetMin = new Vector2(5, 5);
        handleRect.offsetMax = new Vector2(-5, -5);
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = primaryColor;
        
        scrollbarComponent.handleRect = handleRect;
        scrollbarComponent.targetGraphic = handleImage;
        
        // Arrow
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.SetParent(dropdownGO.transform, false);
        
        RectTransform arrowRect = arrow.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.sizeDelta = new Vector2(20, 20);
        arrowRect.anchoredPosition = new Vector2(-15, 0);
        
        TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
        arrowText.text = "▼";
        arrowText.color = Color.white;
        arrowText.fontSize = 12;
        arrowText.alignment = TextAlignmentOptions.Center;
        
        // Label del dropdown
        GameObject dropLabel = new GameObject("Label");
        dropLabel.transform.SetParent(dropdownGO.transform, false);
        
        RectTransform dropLabelRect = dropLabel.AddComponent<RectTransform>();
        dropLabelRect.anchorMin = Vector2.zero;
        dropLabelRect.anchorMax = Vector2.one;
        dropLabelRect.offsetMin = new Vector2(10, 2);
        dropLabelRect.offsetMax = new Vector2(-35, -2);
        
        TextMeshProUGUI dropText = dropLabel.AddComponent<TextMeshProUGUI>();
        dropText.color = Color.white;
        dropText.fontSize = 14;
        dropText.alignment = TextAlignmentOptions.Left;
        
        // Configurar referencias
        dropdown.template = templateRect;
        dropdown.captionText = dropText;
        dropdown.itemText = itemText;
        
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontalScrollbar = null;
        scrollRect.verticalScrollbar = scrollbarComponent;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 1;
        
        // Configurar opciones
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        dropdown.value = selectedIndex;
        
        // Configurar toggle del item
        itemToggle.targetGraphic = itemBgImage;
        itemToggle.graphic = checkImage;
        itemToggle.isOn = true;
        
        // Colores del item
        ColorBlock itemColors = itemToggle.colors;
        itemColors.normalColor = Color.white;
        itemColors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        itemColors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        itemToggle.colors = itemColors;
        
        if (onValueChanged != null)
        {
            dropdown.onValueChanged.AddListener((int val) => onValueChanged?.Invoke(val));
        }
        
        dropdownsCreated++;
        return dropdown;
    }
    
    void CreateProgressBar(Transform parent, string label, Vector2 position, Vector2 size, Color fillColor)
    {
        GameObject container = new GameObject($"ProgressBar_{label}");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = size;
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        
        // Background
        Image bgImage = container.AddComponent<Image>();
        bgImage.color = backgroundLight;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(container.transform, false);
        
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0.8f, 1); // 80% lleno
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(container.transform, false);
        
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.color = Color.white;
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Center;
        
        Outline outline = labelGO.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
    }
    
    // GENERACIÓN DE PANELES ESPECÍFICOS
    
    void GenerateMainMenuPanel()
    {
        GameObject panel = CreateBasePanel("Main Menu Panel", "MainMenu", true);
        
        // Título del juego
        CreateTitle(panel.transform, "EPIC GAME TITLE", new Vector2(0, 250), 72);
        
        // Versión
        GameObject versionGO = new GameObject("Version");
        versionGO.transform.SetParent(panel.transform, false);
        
        RectTransform versionRect = versionGO.AddComponent<RectTransform>();
        versionRect.anchoredPosition = new Vector2(-50, -50);
        versionRect.sizeDelta = new Vector2(200, 30);
        versionRect.anchorMin = new Vector2(1, 0);
        versionRect.anchorMax = new Vector2(1, 0);
        versionRect.pivot = new Vector2(1, 0);
        
        TextMeshProUGUI versionText = versionGO.AddComponent<TextMeshProUGUI>();
        versionText.text = "v1.0.0";
        versionText.color = new Color(1, 1, 1, 0.5f);
        versionText.fontSize = 16;
        versionText.alignment = TextAlignmentOptions.Right;
        
        // Botones del menú principal
        CreateButton(panel.transform, "NUEVA PARTIDA", new Vector2(0, 80), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("NewGame"));
            
        CreateButton(panel.transform, "CONTINUAR", new Vector2(0, 10), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("LoadGame"));
            
        CreateButton(panel.transform, "OPCIONES", new Vector2(0, -60), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("OptionsMain"));
            
        CreateButton(panel.transform, "CRÉDITOS", new Vector2(0, -130), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("Credits"));
            
        CreateButton(panel.transform, "SALIR", new Vector2(0, -200), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("ExitConfirm"));
    }
    
    void GenerateNewGamePanel()
    {
        GameObject panel = CreateBasePanel("New Game Panel", "NewGame");
        
        CreateTitle(panel.transform, "NUEVA PARTIDA", new Vector2(0, 300));
        
        // Dificultad
        CreateTitle(panel.transform, "Selecciona Dificultad", new Vector2(0, 200), 24);
        
        string[] difficulties = { "Fácil", "Normal", "Difícil", "Pesadilla" };
        int selectedDiff = 1;
        
        for (int i = 0; i < difficulties.Length; i++)
        {
            int index = i;
            float yPos = 120 - (i * 50);
            bool isSelected = i == selectedDiff;
            
            Button diffButton = CreateButton(panel.transform, difficulties[i], new Vector2(0, yPos), new Vector2(250, 45), 
                () => {
                    // Aquí iría la lógica para cambiar dificultad
                    LogDebug($"Dificultad seleccionada: {difficulties[index]}");
                });
                
            if (isSelected)
            {
                diffButton.image.color = accentColor;
            }
        }
        
        // Descripción de dificultad
        GameObject descContainer = new GameObject("DifficultyDescription");
        descContainer.transform.SetParent(panel.transform, false);
        
        RectTransform descRect = descContainer.AddComponent<RectTransform>();
        descRect.anchoredPosition = new Vector2(0, -100);
        descRect.sizeDelta = new Vector2(600, 100);
        descRect.anchorMin = new Vector2(0.5f, 0.5f);
        descRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Image descBg = descContainer.AddComponent<Image>();
        descBg.color = backgroundLight;
        
        TextMeshProUGUI descText = CreateTitle(descContainer.transform, 
            "Dificultad Normal: La experiencia estándar del juego.\nEnemigos balanceados y recursos moderados.", 
            Vector2.zero, 16);
        descText.alignment = TextAlignmentOptions.Center;
        
        // Botones de acción
        CreateButton(panel.transform, "COMENZAR", new Vector2(-100, -250), new Vector2(180, 50), 
            () => {
                LogDebug("Iniciando nueva partida...");
                uiManager?.ShowPanel("HUD");
            });
            
        CreateButton(panel.transform, "VOLVER", new Vector2(100, -250), new Vector2(180, 50), 
            () => uiManager?.ShowPanel("MainMenu"));
    }
    
    void GenerateLoadGamePanel()
    {
        GameObject panel = CreateBasePanel("Load Game Panel", "LoadGame");
        
        CreateTitle(panel.transform, "CARGAR PARTIDA", new Vector2(0, 300));
        
        // Lista de saves
        for (int i = 0; i < saveSlots.Length; i++)
        {
            SaveData save = saveSlots[i];
            float yPos = 150 - (i * 120);
            
            // Contenedor del save
            GameObject saveContainer = new GameObject($"SaveSlot_{i}");
            saveContainer.transform.SetParent(panel.transform, false);
            
            RectTransform saveRect = saveContainer.AddComponent<RectTransform>();
            saveRect.anchoredPosition = new Vector2(0, yPos);
            saveRect.sizeDelta = new Vector2(700, 100);
            saveRect.anchorMin = new Vector2(0.5f, 0.5f);
            saveRect.anchorMax = new Vector2(0.5f, 0.5f);
            
            Image saveBg = saveContainer.AddComponent<Image>();
            saveBg.color = save.isEmpty ? backgroundLight : backgroundLight * 1.2f;
            
            // Información del save
            if (!save.isEmpty)
            {
                // Nombre
                TextMeshProUGUI nameText = CreateTitle(saveContainer.transform, save.saveName, new Vector2(-200, 20), 20);
                nameText.alignment = TextAlignmentOptions.Left;
                
                // Nivel
                TextMeshProUGUI levelText = CreateTitle(saveContainer.transform, $"Nivel {save.level}", new Vector2(-200, -10), 16);
                levelText.alignment = TextAlignmentOptions.Left;
                levelText.color = secondaryColor;
                
                // Tiempo
                TextMeshProUGUI timeText = CreateTitle(saveContainer.transform, $"Tiempo: {save.playTime}", new Vector2(0, 20), 16);
                timeText.alignment = TextAlignmentOptions.Center;
                
                // Fecha
                TextMeshProUGUI dateText = CreateTitle(saveContainer.transform, save.lastSaveDate, new Vector2(0, -10), 14);
                dateText.alignment = TextAlignmentOptions.Center;
                dateText.color = secondaryColor;
                
                // Botones
                int slotIndex = i;
                CreateButton(saveContainer.transform, "CARGAR", new Vector2(200, 0), new Vector2(100, 40), 
                    () => {
                        LogDebug($"Cargando partida {slotIndex}...");
                        uiManager?.ShowPanel("HUD");
                    });
                    
                CreateButton(saveContainer.transform, "BORRAR", new Vector2(320, 0), new Vector2(80, 30), 
                    () => {
                        LogDebug($"Borrando partida {slotIndex}...");
                        // Aquí iría la confirmación
                    });
            }
            else
            {
                TextMeshProUGUI emptyText = CreateTitle(saveContainer.transform, "- Vacío -", Vector2.zero, 24);
                emptyText.color = new Color(1, 1, 1, 0.3f);
            }
        }
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -250), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("MainMenu"));
    }
    
    void GenerateOptionsMainPanel()
    {
        GameObject panel = CreateBasePanel("Options Main Panel", "OptionsMain");
        
        CreateTitle(panel.transform, "OPCIONES", new Vector2(0, 250));
        
        // Categorías de opciones
        CreateButton(panel.transform, "VIDEO", new Vector2(0, 120), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("VideoOptions"));
            
        CreateButton(panel.transform, "AUDIO", new Vector2(0, 50), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("AudioOptions"));
            
        CreateButton(panel.transform, "CONTROLES", new Vector2(0, -20), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("ControlsOptions"));
            
        CreateButton(panel.transform, "JUGABILIDAD", new Vector2(0, -90), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("GameplayOptions"));
            
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -200), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("MainMenu"));
    }
    
void GenerateVideoOptionsPanel()
{
    GameObject panel = CreateBasePanel("Video Options Panel", "VideoOptions");

    CreateTitle(panel.transform, "OPCIONES DE VIDEO", new Vector2(0, 320));

    int y = 250;

    // ----- DISPLAY -----
    CreateTitle(panel.transform, "Pantalla", new Vector2(0, y), 20);
    y -= 40;

    // Resoluciones populares
    string[] resolutions = { "1920x1080", "1600x900", "1280x720", "1024x768" };
    int currentResIndex = System.Array.FindIndex(resolutions, r =>
        r == $"{ConfigurationManager.Graphics.resolution.x}x{ConfigurationManager.Graphics.resolution.y}");
    if (currentResIndex < 0) currentResIndex = 0;
    CreateDropdown(panel.transform, "Resolución:", new Vector2(0, y), resolutions, currentResIndex,
        (int index) =>
        {
            string[] parts = resolutions[index].Split('x');
            ConfigurationManager.Graphics.resolution = new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
            LogDebug($"Resolución cambiada a: {resolutions[index]}");
        });
    y -= 40;

    // Fullscreen
    string[] fsModes = System.Enum.GetNames(typeof(FullScreenMode));
    CreateDropdown(panel.transform, "Modo de Pantalla:", new Vector2(0, y), fsModes, (int)ConfigurationManager.Graphics.fullScreenMode,
        (int index) =>
        {
            ConfigurationManager.Graphics.fullScreenMode = (FullScreenMode)index;
            LogDebug($"FullScreen Mode: {fsModes[index]}");
        });
    y -= 40;

    // Target framerate
    CreateSlider(panel.transform, "Framerate objetivo:", new Vector2(0, y), new Vector2(200, 20), 30, 240,
        ConfigurationManager.Graphics.targetFrameRate, (float value) =>
        {
            ConfigurationManager.Graphics.targetFrameRate = Mathf.RoundToInt(value);
            LogDebug($"Framerate objetivo: {value}");
        });
    y -= 40;

    // VSync
    CreateToggle(panel.transform, "VSync", new Vector2(0, y), ConfigurationManager.Graphics.vSyncEnabled,
        (bool value) =>
        {
            ConfigurationManager.Graphics.vSyncEnabled = value;
            LogDebug($"VSync: {value}");
        });
    y -= 50;

    // ----- QUALITY -----
    CreateTitle(panel.transform, "Calidad", new Vector2(0, y), 20);
    y -= 40;

    // Quality Level
    string[] qualities = System.Enum.GetNames(typeof(QualityLevel));
    CreateDropdown(panel.transform, "Calidad General:", new Vector2(0, y), qualities, (int)ConfigurationManager.Graphics.qualityLevel,
        (int index) =>
        {
            ConfigurationManager.Graphics.qualityLevel = (QualityLevel)index;
            LogDebug($"Calidad general: {qualities[index]}");
        });
    y -= 40;

    // Texture Quality
    CreateDropdown(panel.transform, "Calidad Texturas:", new Vector2(0, y),
        new string[] { "Alta", "Media", "Baja", "Muy Baja" },
        ConfigurationManager.Graphics.textureQuality, (int value) =>
        {
            ConfigurationManager.Graphics.textureQuality = value;
            LogDebug($"Calidad de texturas: {value}");
        });
    y -= 40;

    // Shadow Quality
    CreateDropdown(panel.transform, "Calidad Sombras:", new Vector2(0, y),
        new string[] { "Desactivado", "Baja", "Media", "Alta", "Ultra" },
        ConfigurationManager.Graphics.shadowQuality, (int value) =>
        {
            ConfigurationManager.Graphics.shadowQuality = value;
            LogDebug($"Calidad de sombras: {value}");
        });
    y -= 40;

    // AntiAliasing
    CreateDropdown(panel.transform, "AntiAliasing:", new Vector2(0, y),
        new string[] { "Ninguno", "2x", "4x", "8x" },
        (ConfigurationManager.Graphics.antiAliasing == 0) ? 0 :
        (ConfigurationManager.Graphics.antiAliasing == 2) ? 1 :
        (ConfigurationManager.Graphics.antiAliasing == 4) ? 2 : 3,
        (int index) =>
        {
            int[] aaValues = { 0, 2, 4, 8 };
            ConfigurationManager.Graphics.antiAliasing = aaValues[index];
            LogDebug($"AntiAliasing: {aaValues[index]}");
        });
    y -= 40;

    // Post processing
    CreateToggle(panel.transform, "Post Procesado", new Vector2(0, y), ConfigurationManager.Graphics.enablePostProcessing,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enablePostProcessing = value;
            LogDebug($"Post procesado: {value}");
        });
    y -= 50;

    // ----- LIGHTING -----
    CreateTitle(panel.transform, "Iluminación", new Vector2(0, y), 20);
    y -= 40;

    CreateToggle(panel.transform, "Iluminación Tiempo Real", new Vector2(0, y), ConfigurationManager.Graphics.enableRealTimeLighting,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enableRealTimeLighting = value;
            LogDebug($"Iluminación tiempo real: {value}");
        });
    y -= 40;

    CreateToggle(panel.transform, "Sombras", new Vector2(0, y), ConfigurationManager.Graphics.enableShadows,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enableShadows = value;
            LogDebug($"Sombras: {value}");
        });
    y -= 40;

    string[] shadowRes = System.Enum.GetNames(typeof(ShadowResolution));
    CreateDropdown(panel.transform, "Resolución Sombras:", new Vector2(0, y), shadowRes, (int)ConfigurationManager.Graphics.shadowResolution,
        (int value) =>
        {
            ConfigurationManager.Graphics.shadowResolution = (ShadowResolution)value;
            LogDebug($"Resolución sombras: {shadowRes[value]}");
        });
    y -= 40;

    CreateSlider(panel.transform, "Distancia Sombras:", new Vector2(0, y), new Vector2(200, 20), 10, 200,
        ConfigurationManager.Graphics.shadowDistance, (float value) =>
        {
            ConfigurationManager.Graphics.shadowDistance = value;
            LogDebug($"Distancia sombras: {value}");
        });
    y -= 50;

    // ----- EFFECTS -----
    CreateTitle(panel.transform, "Efectos", new Vector2(0, y), 20);
    y -= 40;

    CreateToggle(panel.transform, "Partículas", new Vector2(0, y), ConfigurationManager.Graphics.enableParticles,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enableParticles = value;
            LogDebug($"Partículas: {value}");
        });
    y -= 40;

    CreateSlider(panel.transform, "Máx Partículas:", new Vector2(0, y), new Vector2(200, 20), 100, 5000,
        ConfigurationManager.Graphics.maxParticles, (float value) =>
        {
            ConfigurationManager.Graphics.maxParticles = Mathf.RoundToInt(value);
            LogDebug($"Máx partículas: {value}");
        });
    y -= 40;

    CreateToggle(panel.transform, "Bloom", new Vector2(0, y), ConfigurationManager.Graphics.enableBloom,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enableBloom = value;
            LogDebug($"Bloom: {value}");
        });
    y -= 40;

    CreateToggle(panel.transform, "Motion Blur", new Vector2(0, y), ConfigurationManager.Graphics.enableMotionBlur,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enableMotionBlur = value;
            LogDebug($"Motion Blur: {value}");
        });
    y -= 40;

    CreateToggle(panel.transform, "Ambient Occlusion", new Vector2(0, y), ConfigurationManager.Graphics.enableAmbientOcclusion,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enableAmbientOcclusion = value;
            LogDebug($"Ambient Occlusion: {value}");
        });
    y -= 50;

    // ----- PERFORMANCE -----
    CreateTitle(panel.transform, "Rendimiento", new Vector2(0, y), 20);
    y -= 40;

    CreateToggle(panel.transform, "Occlusion Culling", new Vector2(0, y), ConfigurationManager.Graphics.enableOcclusion,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enableOcclusion = value;
            LogDebug($"Occlusion: {value}");
        });
    y -= 40;

    CreateSlider(panel.transform, "LOD Bias:", new Vector2(0, y), new Vector2(200, 20), 0.1f, 2f,
        ConfigurationManager.Graphics.lodBias, (float value) =>
        {
            ConfigurationManager.Graphics.lodBias = value;
            LogDebug($"LOD Bias: {value}");
        });
    y -= 40;

    CreateSlider(panel.transform, "Luces por Pixel:", new Vector2(0, y), new Vector2(200, 20), 0, 10,
        ConfigurationManager.Graphics.pixelLightCount, (float value) =>
        {
            ConfigurationManager.Graphics.pixelLightCount = Mathf.RoundToInt(value);
            LogDebug($"Luces por pixel: {value}");
        });
    y -= 40;

    CreateToggle(panel.transform, "Instancing GPU", new Vector2(0, y), ConfigurationManager.Graphics.enableGPUInstancing,
        (bool value) =>
        {
            ConfigurationManager.Graphics.enableGPUInstancing = value;
            LogDebug($"Instancing GPU: {value}");
        });
    y -= 50;

    // ----- BOTONES -----
    CreateButton(panel.transform, "APLICAR", new Vector2(-100, y), new Vector2(150, 45),
        () =>
        {
            ConfigurationManager.Graphics.ValidateValues();
            ConfigurationManager.Graphics.ApplySettings();
            LogDebug("Configuración gráfica aplicada.");
        });

    CreateButton(panel.transform, "VOLVER", new Vector2(100, y), new Vector2(150, 45),
        () => uiManager?.ShowPanel("OptionsMain"));
}


    
    void GenerateAudioOptionsPanel()
    {
        GameObject panel = CreateBasePanel("Audio Options Panel", "AudioOptions");
        
        CreateTitle(panel.transform, "OPCIONES DE AUDIO", new Vector2(0, 300));
        
        // Volumen maestro
        CreateSlider(panel.transform, "Volumen General:", new Vector2(0, 200), new Vector2(300, 20), 0, 1, masterVolume,
            (float value) => {
                masterVolume = value;
                LogDebug($"Volumen maestro: {value}");
            });
        
        // Volumen música
        CreateSlider(panel.transform, "Música:", new Vector2(0, 140), new Vector2(300, 20), 0, 1, musicVolume,
            (float value) => {
                musicVolume = value;
                LogDebug($"Volumen música: {value}");
            });
        
        // Volumen SFX
        CreateSlider(panel.transform, "Efectos:", new Vector2(0, 80), new Vector2(300, 20), 0, 1, sfxVolume,
            (float value) => {
                sfxVolume = value;
                LogDebug($"Volumen SFX: {value}");
            });
        
        // Volumen voces
        CreateSlider(panel.transform, "Voces:", new Vector2(0, 20), new Vector2(300, 20), 0, 1, voiceVolume,
            (float value) => {
                voiceVolume = value;
                LogDebug($"Volumen voces: {value}");
            });
        
        // Subtítulos
        CreateToggle(panel.transform, "Subtítulos", new Vector2(0, -50), subtitles,
            (bool value) => {
                subtitles = value;
                LogDebug($"Subtítulos: {value}");
            });
        
        // Idioma
        string[] languages = { "Español", "English", "Français", "Deutsch", "日本語" };
        int langIndex = System.Array.IndexOf(languages, language);
        CreateDropdown(panel.transform, "Idioma:", new Vector2(0, -100), languages, langIndex,
            (int index) => {
                language = languages[index];
                LogDebug($"Idioma: {language}");
            });
        
        // Test de audio
        CreateButton(panel.transform, "PROBAR SONIDO", new Vector2(0, -180), new Vector2(200, 40), 
            () => LogDebug("Reproduciendo sonido de prueba..."));
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -250), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("OptionsMain"));
    }
    
    void GenerateControlsOptionsPanel()
    {
        GameObject panel = CreateBasePanel("Controls Options Panel", "ControlsOptions");
        
        CreateTitle(panel.transform, "OPCIONES DE CONTROLES", new Vector2(0, 300));
        
        // Sensibilidad del ratón
        CreateSlider(panel.transform, "Sensibilidad Ratón:", new Vector2(0, 200), new Vector2(300, 20), 0.1f, 2f, mouseSensitivity,
            (float value) => {
                mouseSensitivity = value;
                LogDebug($"Sensibilidad ratón: {value}");
            });
        
        // Invertir Y
        CreateToggle(panel.transform, "Invertir Eje Y", new Vector2(0, 140), invertY,
            (bool value) => {
                invertY = value;
                LogDebug($"Invertir Y: {value}");
            });
        
        // Esquema de control
        string[] schemes = { "WASD + Ratón", "Flechas + Ratón", "Gamepad", "Personalizado" };
        CreateDropdown(panel.transform, "Esquema:", new Vector2(0, 80), schemes, 0,
            (int index) => LogDebug($"Esquema de control: {schemes[index]}"));
        
        // Título controles
        CreateTitle(panel.transform, "Controles Principales", new Vector2(0, 0), 20);
        
        // Lista de controles (simplificada)
        string[] controls = {
            "Mover: WASD",
            "Saltar: Espacio",
            "Correr: Shift",
            "Interactuar: E",
            "Inventario: Tab"
        };
        
        for (int i = 0; i < controls.Length; i++)
        {
            float yPos = -60 - (i * 30);
            TextMeshProUGUI controlText = CreateTitle(panel.transform, controls[i], new Vector2(0, yPos), 16);
            controlText.color = secondaryColor;
        }
        
        CreateButton(panel.transform, "RESTABLECER", new Vector2(-100, -250), new Vector2(150, 45), 
            () => LogDebug("Restableciendo controles..."));
            
        CreateButton(panel.transform, "VOLVER", new Vector2(100, -250), new Vector2(150, 45), 
            () => uiManager?.ShowPanel("OptionsMain"));
    }
    
    void GenerateGameplayOptionsPanel()
    {
        GameObject panel = CreateBasePanel("Gameplay Options Panel", "GameplayOptions");
        
        CreateTitle(panel.transform, "OPCIONES DE JUGABILIDAD", new Vector2(0, 300));
        
        // Dificultad (durante el juego)
        string[] difficulties = { "Fácil", "Normal", "Difícil", "Pesadilla" };
        CreateDropdown(panel.transform, "Dificultad:", new Vector2(0, 200), difficulties, 1,
            (int index) => LogDebug($"Dificultad cambiada a: {difficulties[index]}"));
        
        // Auto-guardado
        CreateToggle(panel.transform, "Auto-Guardado", new Vector2(0, 140), true,
            (bool value) => LogDebug($"Auto-guardado: {value}"));
        
        // Frecuencia auto-guardado
        string[] saveFrequencies = { "5 minutos", "10 minutos", "15 minutos", "30 minutos" };
        CreateDropdown(panel.transform, "Guardar cada:", new Vector2(0, 90), saveFrequencies, 1,
            (int index) => LogDebug($"Frecuencia auto-guardado: {saveFrequencies[index]}"));
        
        // Mostrar tutoriales
        CreateToggle(panel.transform, "Mostrar Tutoriales", new Vector2(0, 30), true,
            (bool value) => LogDebug($"Tutoriales: {value}"));
        
        // Mostrar daño numérico
        CreateToggle(panel.transform, "Números de Daño", new Vector2(0, -10), true,
            (bool value) => LogDebug($"Números de daño: {value}"));
        
        // Mini-mapa
        CreateToggle(panel.transform, "Mostrar Mini-mapa", new Vector2(0, -50), true,
            (bool value) => LogDebug($"Mini-mapa: {value}"));
        
        // Modo daltónico
        CreateToggle(panel.transform, "Modo Daltónico", new Vector2(0, -90), false,
            (bool value) => LogDebug($"Modo daltónico: {value}"));
        
        // Campo de visión
        CreateSlider(panel.transform, "Campo de Visión:", new Vector2(0, -150), new Vector2(300, 20), 60, 120, 90,
            (float value) => LogDebug($"FOV: {value}"));
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -250), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("OptionsMain"));
    }
    
    void GenerateExitConfirmPanel()
    {
        GameObject panel = CreateBasePanel("Exit Confirm Panel", "ExitConfirm");
        
        CreateTitle(panel.transform, "¿SALIR DEL JUEGO?", new Vector2(0, 100));
        
        TextMeshProUGUI confirmText = CreateTitle(panel.transform, 
            "¿Estás seguro de que quieres salir?\nTodo progreso no guardado se perderá.", 
            new Vector2(0, 0), 20);
        confirmText.alignment = TextAlignmentOptions.Center;
        confirmText.color = secondaryColor;
        
        CreateButton(panel.transform, "SÍ, SALIR", new Vector2(-120, -100), new Vector2(200, 50), 
            () => {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });
            
        CreateButton(panel.transform, "CANCELAR", new Vector2(120, -100), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("MainMenu"));
    }
    
    void GenerateHUDPanel()
    {
        GameObject panel = CreateBasePanel("HUD Panel", "HUD");
        
        // Barra de salud (arriba izquierda)
        CreateProgressBar(panel.transform, "HP: 80/100", new Vector2(20, -20), new Vector2(250, 25), Color.red);
        
        // Barra de stamina
        CreateProgressBar(panel.transform, "Stamina", new Vector2(20, -50), new Vector2(200, 20), Color.yellow);
        
        // Barra de maná/energía
        CreateProgressBar(panel.transform, "MP: 50/50", new Vector2(20, -75), new Vector2(200, 20), new Color(0.2f, 0.5f, 1f));
        
        // Mini-mapa (arriba derecha)
        GameObject minimapContainer = new GameObject("Minimap");
        minimapContainer.transform.SetParent(panel.transform, false);
        
        RectTransform minimapRect = minimapContainer.AddComponent<RectTransform>();
        minimapRect.anchoredPosition = new Vector2(-20, -20);
        minimapRect.sizeDelta = new Vector2(200, 200);
        minimapRect.anchorMin = new Vector2(1, 1);
        minimapRect.anchorMax = new Vector2(1, 1);
        minimapRect.pivot = new Vector2(1, 1);
        
        Image minimapBg = minimapContainer.AddComponent<Image>();
        minimapBg.color = new Color(0, 0, 0, 0.7f);
        
        // Marco del minimapa
        Outline minimapOutline = minimapContainer.AddComponent<Outline>();
        minimapOutline.effectColor = primaryColor;
        minimapOutline.effectDistance = new Vector2(2, 2);
        
        // Experiencia (abajo)
        GameObject xpBar = new GameObject("XP Bar");
        xpBar.transform.SetParent(panel.transform, false);
        
        RectTransform xpRect = xpBar.AddComponent<RectTransform>();
        xpRect.anchoredPosition = new Vector2(0, 20);
        xpRect.sizeDelta = new Vector2(500, 10);
        xpRect.anchorMin = new Vector2(0.5f, 0);
        xpRect.anchorMax = new Vector2(0.5f, 0);
        
        Image xpBg = xpBar.AddComponent<Image>();
        xpBg.color = backgroundLight;
        
        GameObject xpFill = new GameObject("Fill");
        xpFill.transform.SetParent(xpBar.transform, false);
        
        RectTransform xpFillRect = xpFill.AddComponent<RectTransform>();
        xpFillRect.anchorMin = Vector2.zero;
        xpFillRect.anchorMax = new Vector2(0.65f, 1);
        xpFillRect.offsetMin = Vector2.zero;
        xpFillRect.offsetMax = Vector2.zero;
        
        Image xpFillImage = xpFill.AddComponent<Image>();
        xpFillImage.color = accentColor;
        
        // Nivel
        TextMeshProUGUI levelText = CreateTitle(xpBar.transform, "Nivel 15", new Vector2(0, 20), 16);
        levelText.color = Color.white;
        
        // Hotbar (abajo centro)
        GameObject hotbar = new GameObject("Hotbar");
        hotbar.transform.SetParent(panel.transform, false);
        
        RectTransform hotbarRect = hotbar.AddComponent<RectTransform>();
        hotbarRect.anchoredPosition = new Vector2(0, 60);
        hotbarRect.sizeDelta = new Vector2(500, 60);
        hotbarRect.anchorMin = new Vector2(0.5f, 0);
        hotbarRect.anchorMax = new Vector2(0.5f, 0);
        
        // Slots del hotbar
        for (int i = 0; i < 10; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(hotbar.transform, false);
            
            RectTransform slotRect = slot.AddComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(-225 + (i * 50), 0);
            slotRect.sizeDelta = new Vector2(45, 45);
            slotRect.anchorMin = new Vector2(0, 0.5f);
            slotRect.anchorMax = new Vector2(0, 0.5f);
            
            Image slotImage = slot.AddComponent<Image>();
            slotImage.color = backgroundLight;
            
            // Número del slot
            TextMeshProUGUI slotNumber = CreateTitle(slot.transform, (i + 1).ToString(), new Vector2(0, 15), 12);
            slotNumber.color = secondaryColor;
        }
        
        // Crosshair (centro)
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(panel.transform, false);
        
        RectTransform crossRect = crosshair.AddComponent<RectTransform>();
        crossRect.anchoredPosition = Vector2.zero;
        crossRect.sizeDelta = new Vector2(2, 20);
        crossRect.anchorMin = new Vector2(0.5f, 0.5f);
        crossRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Image crossV = crosshair.AddComponent<Image>();
        crossV.color = new Color(1, 1, 1, 0.8f);
        
        GameObject crossH = new GameObject("Horizontal");
        crossH.transform.SetParent(crosshair.transform, false);
        
        RectTransform crossHRect = crossH.AddComponent<RectTransform>();
        crossHRect.anchoredPosition = Vector2.zero;
        crossHRect.sizeDelta = new Vector2(20, 2);
        
        Image crossHImage = crossH.AddComponent<Image>();
        crossHImage.color = new Color(1, 1, 1, 0.8f);
        
        // Objetivo actual (arriba centro)
        GameObject objective = new GameObject("Current Objective");
        objective.transform.SetParent(panel.transform, false);
        
        RectTransform objRect = objective.AddComponent<RectTransform>();
        objRect.anchoredPosition = new Vector2(0, -50);
        objRect.sizeDelta = new Vector2(400, 60);
        objRect.anchorMin = new Vector2(0.5f, 1);
        objRect.anchorMax = new Vector2(0.5f, 1);
        
        Image objBg = objective.AddComponent<Image>();
        objBg.color = new Color(0, 0, 0, 0.5f);
        
        TextMeshProUGUI objText = CreateTitle(objective.transform, "Objetivo: Encuentra la llave antigua", Vector2.zero, 16);
        objText.color = accentColor;
    }
    
    void GeneratePauseMenuPanel()
    {
        GameObject panel = CreateBasePanel("Pause Menu Panel", "PauseMenu");
        
        // Fondo oscuro semi-transparente
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
        
        CreateTitle(panel.transform, "PAUSA", new Vector2(0, 200));
        
        CreateButton(panel.transform, "CONTINUAR", new Vector2(0, 80), new Vector2(250, 60), 
            () => uiManager?.ShowPanel("HUD"));
            
        CreateButton(panel.transform, "OPCIONES", new Vector2(0, 10), new Vector2(250, 60), 
            () => uiManager?.ShowPanel("OptionsMain"));
            
        CreateButton(panel.transform, "GUARDAR PARTIDA", new Vector2(0, -60), new Vector2(250, 60), 
            () => LogDebug("Guardando partida..."));
            
        CreateButton(panel.transform, "CARGAR PARTIDA", new Vector2(0, -130), new Vector2(250, 60), 
            () => uiManager?.ShowPanel("LoadGame"));
            
        CreateButton(panel.transform, "MENÚ PRINCIPAL", new Vector2(0, -200), new Vector2(250, 60), 
            () => uiManager?.ShowPanel("MainMenu"));
    }
    
    void GenerateInventoryPanel()
    {
        GameObject panel = CreateBasePanel("Inventory Panel", "Inventory");
        
        CreateTitle(panel.transform, "INVENTARIO", new Vector2(0, 300));
        
        // Grid de inventario
        GameObject gridContainer = new GameObject("Inventory Grid");
        gridContainer.transform.SetParent(panel.transform, false);
        
        RectTransform gridRect = gridContainer.AddComponent<RectTransform>();
        gridRect.anchoredPosition = new Vector2(-200, 0);
        gridRect.sizeDelta = new Vector2(400, 400);
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        GridLayoutGroup grid = gridContainer.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(60, 60);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 6;
        
        // Crear slots de inventario
        for (int i = 0; i < 36; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(gridContainer.transform, false);
            
            Image slotImage = slot.AddComponent<Image>();
            slotImage.color = backgroundLight;
            
            Button slotButton = slot.AddComponent<Button>();
            
            // Simular algunos items
            if (i == 0 || i == 3 || i == 7 || i == 15)
            {
                GameObject item = new GameObject("Item");
                item.transform.SetParent(slot.transform, false);
                
                RectTransform itemRect = item.AddComponent<RectTransform>();
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.one;
                itemRect.offsetMin = new Vector2(5, 5);
                itemRect.offsetMax = new Vector2(-5, -5);
                
                Image itemImage = item.AddComponent<Image>();
                itemImage.color = primaryColor;
            }
        }
        
        // Panel de información del personaje
        GameObject charPanel = new GameObject("Character Panel");
        charPanel.transform.SetParent(panel.transform, false);
        
        RectTransform charRect = charPanel.AddComponent<RectTransform>();
        charRect.anchoredPosition = new Vector2(250, 0);
        charRect.sizeDelta = new Vector2(300, 400);
        charRect.anchorMin = new Vector2(0.5f, 0.5f);
        charRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Image charBg = charPanel.AddComponent<Image>();
        charBg.color = backgroundLight;
        
        // Stats del personaje
        CreateTitle(charPanel.transform, "ESTADÍSTICAS", new Vector2(0, 180), 20);
        
        string[] stats = {
            "Fuerza: 15",
            "Agilidad: 12",
            "Inteligencia: 8",
            "Resistencia: 10",
            "Suerte: 5"
        };
        
        for (int i = 0; i < stats.Length; i++)
        {
            float yPos = 120 - (i * 30);
            TextMeshProUGUI statText = CreateTitle(charPanel.transform, stats[i], new Vector2(0, yPos), 16);
            statText.alignment = TextAlignmentOptions.Left;
        }
        
        // Equipo
        CreateTitle(charPanel.transform, "EQUIPO", new Vector2(0, -50), 20);
        
        string[] equipment = {
            "Arma: Espada de Hierro",
            "Armadura: Cota de Malla",
            "Accesorio: Anillo de Poder"
        };
        
        for (int i = 0; i < equipment.Length; i++)
        {
            float yPos = -90 - (i * 25);
            TextMeshProUGUI equipText = CreateTitle(charPanel.transform, equipment[i], new Vector2(0, yPos), 14);
            equipText.alignment = TextAlignmentOptions.Left;
            equipText.color = secondaryColor;
        }
        
        CreateButton(panel.transform, "CERRAR", new Vector2(0, -250), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("HUD"));
    }
    
    void GenerateDialoguePanel()
    {
        GameObject panel = CreateBasePanel("Dialogue Panel", "Dialogue");
        
        // Panel de diálogo en la parte inferior
        GameObject dialogueBox = new GameObject("Dialogue Box");
        dialogueBox.transform.SetParent(panel.transform, false);
        
        RectTransform dialogueRect = dialogueBox.AddComponent<RectTransform>();
        dialogueRect.anchoredPosition = new Vector2(0, -200);
        dialogueRect.sizeDelta = new Vector2(800, 200);
        dialogueRect.anchorMin = new Vector2(0.5f, 0);
        dialogueRect.anchorMax = new Vector2(0.5f, 0);
        
        Image dialogueBg = dialogueBox.AddComponent<Image>();
        dialogueBg.color = backgroundDark;
        
        // Nombre del personaje
        GameObject nameBox = new GameObject("Name Box");
        nameBox.transform.SetParent(dialogueBox.transform, false);
        
        RectTransform nameRect = nameBox.AddComponent<RectTransform>();
        nameRect.anchoredPosition = new Vector2(-300, 80);
        nameRect.sizeDelta = new Vector2(200, 40);
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(0, 1);
        
        Image nameBg = nameBox.AddComponent<Image>();
        nameBg.color = primaryColor;
        
        TextMeshProUGUI nameText = CreateTitle(nameBox.transform, "NPC Misterioso", Vector2.zero, 18);
        
        // Texto del diálogo
        GameObject dialogueTextGO = new GameObject("Dialogue Text");
        dialogueTextGO.transform.SetParent(dialogueBox.transform, false);
        
        RectTransform textRect = dialogueTextGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -60);
        
        TextMeshProUGUI dialogueText = dialogueTextGO.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "¡Hola aventurero! He oído hablar mucho de ti. ¿Podrías ayudarme con una misión muy importante?";
        dialogueText.color = Color.white;
        dialogueText.fontSize = 18;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        
        // Opciones de respuesta
        GameObject optionsContainer = new GameObject("Options Container");
        optionsContainer.transform.SetParent(panel.transform, false);
        
        RectTransform optionsRect = optionsContainer.AddComponent<RectTransform>();
        optionsRect.anchoredPosition = new Vector2(0, 50);
        optionsRect.sizeDelta = new Vector2(600, 200);
        optionsRect.anchorMin = new Vector2(0.5f, 0);
        optionsRect.anchorMax = new Vector2(0.5f, 0);
        
        string[] responses = {
            "1. Claro, ¿en qué puedo ayudarte?",
            "2. Depende de la recompensa...",
            "3. No tengo tiempo ahora.",
            "4. [Persuasión] Primero dime qué gano yo."
        };
        
        for (int i = 0; i < responses.Length; i++)
        {
            int index = i;
            float yPos = 150 - (i * 40);
            Button responseBtn = CreateButton(optionsContainer.transform, responses[i], new Vector2(0, yPos), new Vector2(550, 35), 
                () => LogDebug($"Respuesta seleccionada: {index + 1}"));
            
            // Resaltar opciones especiales
            if (responses[i].Contains("["))
            {
                responseBtn.image.color = accentColor * 0.8f;
            }
        }
        
        // Indicador de continuar
        GameObject continueIndicator = new GameObject("Continue Indicator");
        continueIndicator.transform.SetParent(dialogueBox.transform, false);
        
        RectTransform continueRect = continueIndicator.AddComponent<RectTransform>();
        continueRect.anchoredPosition = new Vector2(370, -80);
        continueRect.sizeDelta = new Vector2(30, 30);
        
        TextMeshProUGUI continueText = continueIndicator.AddComponent<TextMeshProUGUI>();
        continueText.text = "▼";
        continueText.color = Color.white;
        continueText.fontSize = 20;
        continueText.alignment = TextAlignmentOptions.Center;
    }
    
    void GenerateConfirmationPanel()
    {
        GameObject panel = CreateBasePanel("Confirmation Panel", "Confirmation");
        
        // Hacer el panel más pequeño y centrado
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(500, 300);
        
        CreateTitle(panel.transform, "CONFIRMACIÓN", new Vector2(0, 80));
        
        GameObject messageGO = new GameObject("Message");
        messageGO.transform.SetParent(panel.transform, false);
        
        RectTransform messageRect = messageGO.AddComponent<RectTransform>();
        messageRect.anchoredPosition = Vector2.zero;
        messageRect.sizeDelta = new Vector2(400, 100);
        messageRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        TextMeshProUGUI messageText = messageGO.AddComponent<TextMeshProUGUI>();
        messageText.text = "¿Estás seguro de que quieres realizar esta acción?";
        messageText.color = Color.white;
        messageText.fontSize = 18;
        messageText.alignment = TextAlignmentOptions.Center;
        
        CreateButton(panel.transform, "ACEPTAR", new Vector2(-100, -80), new Vector2(150, 45), 
            () => LogDebug("Confirmación aceptada"));
            
        CreateButton(panel.transform, "CANCELAR", new Vector2(100, -80), new Vector2(150, 45), 
            () => LogDebug("Confirmación cancelada"));
    }
    
    void GenerateCreditsPanel()
    {
        GameObject panel = CreateBasePanel("Credits Panel", "Credits");
        
        CreateTitle(panel.transform, "CRÉDITOS", new Vector2(0, 300));
        
        // Contenedor scrolleable
        GameObject scrollView = new GameObject("Scroll View");
        scrollView.transform.SetParent(panel.transform, false);
        
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchoredPosition = new Vector2(0, -50);
        scrollRect.sizeDelta = new Vector2(600, 400);
        scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.movementType = ScrollRect.MovementType.Elastic;
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);
        
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 800); // Altura para scroll
        
        // Agregar créditos
        string[] credits = {
            "EPIC GAME TITLE",
            "",
            "Desarrollado por",
            "Tu Estudio Increíble",
            "",
            "Programación",
            "Lead Programmer - Juan Pérez",
            "Gameplay Programmer - María García",
            "",
            "Arte",
            "Art Director - Carlos López",
            "3D Artist - Ana Martínez",
            "",
            "Diseño",
            "Game Designer - Luis Rodríguez",
            "Level Designer - Sofia Hernández",
            "",
            "Audio",
            "Sound Designer - Miguel Ángel",
            "Composer - Elena Ruiz",
            "",
            "QA",
            "QA Lead - Roberto Sánchez",
            "",
            "Agradecimientos Especiales",
            "A todos los que hicieron posible este juego",
            "",
            "© 2025 Tu Estudio"
        };
        
        float yOffset = -20;
        foreach (string credit in credits)
        {
            GameObject creditGO = new GameObject("Credit");
            creditGO.transform.SetParent(content.transform, false);
            
            RectTransform creditRect = creditGO.AddComponent<RectTransform>();
            creditRect.anchoredPosition = new Vector2(0, yOffset);
            creditRect.sizeDelta = new Vector2(500, 30);
            creditRect.anchorMin = new Vector2(0.5f, 1);
            creditRect.anchorMax = new Vector2(0.5f, 1);
            
            TextMeshProUGUI creditText = creditGO.AddComponent<TextMeshProUGUI>();
            creditText.text = credit;
            creditText.fontSize = credit == "" ? 10 : (credit.Contains(" - ") ? 16 : 20);
            creditText.color = credit.Contains(" - ") ? secondaryColor : Color.white;
            creditText.alignment = TextAlignmentOptions.Center;
            creditText.fontStyle = credit.Contains("©") ? FontStyles.Italic : FontStyles.Normal;
            
            yOffset -= 35;
        }
        
        // Configurar scroll
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -300), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("MainMenu"));
    }
    
    void SetupNavigation()
    {
        LogDebug("🔗 Setting up panel navigation...");
        
        // Configurar navegación entre paneles
        foreach (var panel in allPanels)
        {
            string panelID = panel.panelID;
            string nextID = "";
            string previousID = "";
            
            switch (panelID)
            {
                case "MainMenu":
                    previousID = "";
                    break;
                    
                case "NewGame":
                case "LoadGame":
                case "OptionsMain":
                case "Credits":
                    previousID = "MainMenu";
                    break;
                    
                case "VideoOptions":
                case "AudioOptions":
                case "ControlsOptions":
                case "GameplayOptions":
                    previousID = "OptionsMain";
                    break;
                    
                case "ExitConfirm":
                    previousID = "MainMenu";
                    break;
                    
                case "HUD":
                    nextID = "PauseMenu";
                    break;
                    
                case "PauseMenu":
                    previousID = "HUD";
                    break;
                    
                case "Inventory":
                    previousID = "HUD";
                    break;
            }
            
            // Usar reflection para establecer las propiedades
            var nextField = typeof(UIPanel).GetField("nextPanelID", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var prevField = typeof(UIPanel).GetField("previousPanelID", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (nextField == null || prevField == null)
            {
                // Si los campos son públicos en ConcreteUIPanel
                panel.nextPanelID = nextID;
                panel.previousPanelID = previousID;
            }
            else
            {
                // Si los campos son privados en UIPanel base
                nextField?.SetValue(panel, nextID);
                prevField?.SetValue(panel, previousID);
            }
        }
        
        LogDebug("✅ Navigation setup complete");
    }
    
    void ConfigureUIManager()
    {
        LogDebug("📋 Configuring UIManager...");
        
        if (uiManager != null)
        {
            // Crear array de UIPanel para UIManager
            UIPanel[] uiPanelArray = new UIPanel[allPanels.Count];
            for (int i = 0; i < allPanels.Count; i++)
            {
                uiPanelArray[i] = allPanels[i];
            }
            
            // Configurar UIManager usando reflection
            var field = typeof(UIManager).GetField("uiPanels", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(uiManager, uiPanelArray);
                LogDebug($"✅ UIManager configured with {allPanels.Count} panels");
            }
            else
            {
                // Intentar con campo público
                var publicField = typeof(UIManager).GetField("uiPanels", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                if (publicField != null)
                {
                    publicField.SetValue(uiManager, uiPanelArray);
                    LogDebug($"✅ UIManager configured with {allPanels.Count} panels (public field)");
                }
                else
                {
                    LogDebug("⚠️ Could not find uiPanels field in UIManager");
                }
            }
        }
    }
    
    void FinalizeGeneration()
    {
        LogDebug("🏁 Finalizing generation...");
        
        // Asegurar que solo el menú principal sea visible
        foreach (var kvp in createdPanels)
        {
            bool shouldBeVisible = kvp.Key == "MainMenu";
            kvp.Value.SetActive(shouldBeVisible);
        }
        
        // Configurar la navegación con teclado/gamepad si está habilitado
        if (supportControllerInput && eventSystem != null)
        {
            // Encontrar el primer botón seleccionable en el menú principal
            GameObject mainMenu = createdPanels["MainMenu"];
            Button firstButton = mainMenu.GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                eventSystem.SetSelectedGameObject(firstButton.gameObject);
            }
        }
        
        LogDebug("✅ Generation finalized");
    }
    
    void ShowGenerationSummary()
    {
        string summary = $@"
🎯 COMPLETE UI GENERATION SUMMARY
=====================================
📄 Panels Created: {panelsCreated}
🔘 Buttons Created: {buttonsCreated}  
🎚️ Sliders Created: {slidersCreated}
☑️ Toggles Created: {togglesCreated}
📋 Dropdowns Created: {dropdownsCreated}

📋 Generated Panels:
{string.Join("\n", createdPanels.Keys.Select(id => $"  • {id}"))}

✅ Complete UI system generated!
🎮 Ready to play and fully interactive!
";
        
        Debug.Log(summary);
        
        #if UNITY_EDITOR
        EditorUtility.DisplayDialog("UI Generation Complete", 
            $"Successfully generated complete interactive UI system!\n\n" +
            $"Panels: {panelsCreated}\n" +
            $"Buttons: {buttonsCreated}\n" +
            $"Sliders: {slidersCreated}\n" +
            $"Toggles: {togglesCreated}\n" +
            $"Dropdowns: {dropdownsCreated}\n\n" +
            "Your UI is ready to use!", "Awesome!");
        #endif
    }
    
    void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"🚀 [UIGenerator] {message}");
    }
    
    [ContextMenu("🎨 Customize Colors")]
    public void CustomizeColors()
    {
        foreach (var kvp in createdPanels)
        {
            // Actualizar botones
            Button[] buttons = kvp.Value.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = primaryColor;
                colors.highlightedColor = primaryColor * 1.2f;
                colors.pressedColor = primaryColor * 0.8f;
                button.colors = colors;
            }
            
            // Actualizar sliders
            Slider[] sliders = kvp.Value.GetComponentsInChildren<Slider>();
            foreach (Slider slider in sliders)
            {
                Image fillImage = slider.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                    fillImage.color = primaryColor;
            }
            
            // Actualizar toggles
            Toggle[] toggles = kvp.Value.GetComponentsInChildren<Toggle>();
            foreach (Toggle toggle in toggles)
            {
                if (toggle.graphic != null)
                    toggle.graphic.color = primaryColor;
            }
        }
        
        LogDebug("🎨 Colors updated");
    }
    
    [ContextMenu("🧹 Destroy Generated UI")]
    public void DestroyGeneratedUI()
    {
        if (mainCanvas != null)
        {
            if (Application.isPlaying)
                Destroy(mainCanvas.gameObject);
            else
                DestroyImmediate(mainCanvas.gameObject);
        }
        
        if (uiManager != null)
        {
            if (Application.isPlaying)
                Destroy(uiManager.gameObject);
            else
                DestroyImmediate(uiManager.gameObject);
        }
        
        if (eventSystem != null)
        {
            if (Application.isPlaying)
                Destroy(eventSystem.gameObject);
            else
                DestroyImmediate(eventSystem.gameObject);
        }
        
        ResetCounters();
        generationComplete = false;
        LogDebug("🧹 Generated UI destroyed");
    }
    
    [ContextMenu("📊 Show Panel Hierarchy")]
    public void ShowPanelHierarchy()
    {
        string hierarchy = "📊 PANEL NAVIGATION HIERARCHY\n";
        hierarchy += "================================\n";
        
        foreach (var panel in allPanels)
        {
            hierarchy += $"\n{panel.panelID}:\n";
            hierarchy += $"  → Next: {(string.IsNullOrEmpty(panel.nextPanelID) ? "None" : panel.nextPanelID)}\n";
            hierarchy += $"  ← Previous: {(string.IsNullOrEmpty(panel.previousPanelID) ? "None" : panel.previousPanelID)}\n";
        }
        
        Debug.Log(hierarchy);
    }
    
    // Método para testear navegación
    [ContextMenu("🧪 Test Navigation")]
    public void TestNavigation()
    {
        if (Application.isPlaying && uiManager != null)
        {
            StartCoroutine(TestNavigationSequence());
        }
        else
        {
            Debug.LogWarning("⚠️ Navigation test can only run in Play mode with UIManager present");
        }
    }
    
    System.Collections.IEnumerator TestNavigationSequence()
    {
        LogDebug("🧪 Starting navigation test...");
        
        // Secuencia de prueba
        string[] testSequence = {
            "MainMenu", "NewGame", "MainMenu",
            "OptionsMain", "VideoOptions", "OptionsMain",
            "AudioOptions", "OptionsMain", "MainMenu",
            "LoadGame", "MainMenu", "Credits"
        };
        
        foreach (string panelID in testSequence)
        {
            LogDebug($"Testing: {panelID}");
            uiManager.ShowPanel(panelID);
            yield return new WaitForSeconds(0.5f);
        }
        
        LogDebug("🧪 Navigation test complete!");
        uiManager.ShowPanel("MainMenu");
    }
}