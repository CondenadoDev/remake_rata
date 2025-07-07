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
/// Genera un sistema UI completo desde cero con toda la funcionalidad
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
    [SerializeField] private Color backgroundDark = new Color(0.1f, 0.1f, 0.15f, 0.9f);
    [SerializeField] private Color backgroundLight = new Color(0.2f, 0.2f, 0.25f, 0.8f);
    [SerializeField] private Font customFont;
    
    [Header("📊 Generation Results")]
    [SerializeField] private int panelsCreated = 0;
    [SerializeField] private int buttonsCreated = 0;
    [SerializeField] private int slidersCreated = 0;
    [SerializeField] private bool generationComplete = false;
    
    private Canvas mainCanvas;
    private UIManager uiManager;
    private EventSystem eventSystem;
    private Dictionary<string, GameObject> createdPanels = new Dictionary<string, GameObject>();
    private List<ConcreteUIPanel> allPanels = new List<ConcreteUIPanel>();
    
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
        LogDebug("🚀 Starting Complete UI Generation...");
        
        try
        {
            ResetCounters();
            PrepareScene();
            CreateCanvas();
            CreateEventSystem();
            CreateUIManager();
            GenerateAllPanels();
            SetupNavigation();
            ConfigureUIManager();
            FinalizeGeneration();
            
            LogDebug($"✅ UI Generation Complete! Created {panelsCreated} panels, {buttonsCreated} buttons, {slidersCreated} sliders");
            ShowGenerationSummary();
            
            generationComplete = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ UI Generation failed: {e.Message}\n{e.StackTrace}");
        }
    }
    
    void ResetCounters()
    {
        panelsCreated = 0;
        buttonsCreated = 0;
        slidersCreated = 0;
        generationComplete = false;
        createdPanels.Clear();
        allPanels.Clear();
    }
    
    void PrepareScene()
    {
        LogDebug("🧹 Preparing scene...");
        
        if (destroyExistingUI)
        {
            // Destruir Canvas existente
            Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in existingCanvases)
            {
                if (Application.isPlaying)
                    Destroy(canvas.gameObject);
                else
                    DestroyImmediate(canvas.gameObject);
            }
            
            // Destruir UIManager existente
            UIManager[] existingManagers = FindObjectsOfType<UIManager>();
            foreach (UIManager manager in existingManagers)
            {
                if (Application.isPlaying)
                    Destroy(manager.gameObject);
                else
                    DestroyImmediate(manager.gameObject);
            }
        }
        
        LogDebug("✅ Scene prepared");
    }
    
    void CreateCanvas()
    {
        LogDebug("🎨 Creating main canvas...");
        
        GameObject canvasGO = new GameObject("Main Canvas");
        mainCanvas = canvasGO.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 0;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        LogDebug("✅ Canvas created");
    }
    
    void CreateEventSystem()
    {
        LogDebug("🎯 Creating Event System...");
        
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            
            LogDebug("✅ Event System created");
        }
        else
        {
            LogDebug("✅ Event System already exists");
        }
    }
    
    void CreateUIManager()
    {
        LogDebug("🎮 Creating UI Manager...");
        
        GameObject uiManagerGO = new GameObject("UI Manager");
        uiManager = uiManagerGO.AddComponent<UIManager>();
        
        LogDebug("✅ UI Manager created");
    }
    
    void GenerateAllPanels()
    {
        LogDebug("🏗️ Generating all UI panels...");
        
        // Generar paneles en orden
        GenerateMainMenuPanel();
        GenerateOptionsMainPanel();
        GenerateAudioOptionsPanel();
        GenerateGraphicsOptionsPanel();
        GenerateControlsOptionsPanel();
        GenerateGameplayOptionsPanel();
        GenerateHUDPanel();
        GeneratePauseMenuPanel();
        GenerateLoadingPanel();
        
        LogDebug($"🏗️ Generated {panelsCreated} panels total");
    }
    
    GameObject CreateBasePanel(string panelName, string panelID, bool startVisible = false)
    {
        LogDebug($"Creating panel: {panelName} (ID: {panelID})");
        
        GameObject panelGO = new GameObject(panelName);
        panelGO.transform.SetParent(mainCanvas.transform, false);
        
        // RectTransform full screen
        RectTransform rect = panelGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // CanvasGroup
        CanvasGroup canvasGroup = panelGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = startVisible ? 1f : 0f;
        canvasGroup.blocksRaycasts = startVisible;
        canvasGroup.interactable = true;
        
        // Background
        Image background = panelGO.AddComponent<Image>();
        background.color = backgroundDark;
        
        // Agregar componente UIPanel concreto
        ConcreteUIPanel uiPanel = panelGO.AddComponent<ConcreteUIPanel>();
        uiPanel.panelID = panelID;
        uiPanel.startVisible = startVisible;
        uiPanel.useScaleAnimation = panelID != "HUD";
        uiPanel.blockGameInput = panelID != "HUD";
        
        panelGO.SetActive(startVisible);
        
        createdPanels[panelID] = panelGO;
        allPanels.Add(uiPanel);
        panelsCreated++;
        
        LogDebug($"✅ Created panel: {panelName} (ID: {panelID})");
        return panelGO;
    }
    
    Button CreateButton(Transform parent, string text, Vector2 position, Vector2 size, System.Action onClick = null)
    {
        GameObject buttonGO = new GameObject($"Button_{text}");
        buttonGO.transform.SetParent(parent, false);
        
        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = buttonGO.AddComponent<Image>();
        image.color = primaryColor;
        
        Button button = buttonGO.AddComponent<Button>();
        
        // Create text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.fontSize = 24;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        if (customFont != null)
        {
            textComponent.font = TMP_FontAsset.CreateFontAsset(customFont);
        }
        
        // Setup colors
        ColorBlock colors = button.colors;
        colors.normalColor = primaryColor;
        colors.highlightedColor = primaryColor * 1.2f;
        colors.pressedColor = primaryColor * 0.8f;
        colors.disabledColor = Color.gray;
        button.colors = colors;
        
        if (onClick != null)
            button.onClick.AddListener(() => onClick());
        
        buttonsCreated++;
        return button;
    }
    
    Slider CreateSlider(Transform parent, string labelText, Vector2 position, Vector2 size, float defaultValue = 0.5f)
    {
        GameObject sliderGO = new GameObject($"Slider_{labelText}");
        sliderGO.transform.SetParent(parent, false);
        
        RectTransform rect = sliderGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.value = defaultValue;
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderGO.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.gray;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
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
        
        // Configure slider
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(sliderGO.transform, false);
        
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(0, 40);
        labelRect.sizeDelta = new Vector2(size.x, 30);
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        
        TextMeshProUGUI labelComponent = labelGO.AddComponent<TextMeshProUGUI>();
        labelComponent.text = labelText;
        labelComponent.color = Color.white;
        labelComponent.fontSize = 18;
        labelComponent.alignment = TextAlignmentOptions.Center;
        
        slidersCreated++;
        return slider;
    }
    
    void GenerateMainMenuPanel()
    {
        GameObject panel = CreateBasePanel("Main Menu Panel", "MainMenu", true);
        
        // Title
        CreateTitle(panel.transform, "GAME TITLE", new Vector2(0, 200));
        
        // Buttons
        Button newGameBtn = CreateButton(panel.transform, "NUEVO JUEGO", new Vector2(0, 50), new Vector2(300, 60), 
            () => Debug.Log("Nueva partida"));
        
        CreateButton(panel.transform, "CARGAR JUEGO", new Vector2(0, -20), new Vector2(300, 60), 
            () => Debug.Log("Cargar partida"));
        
        CreateButton(panel.transform, "OPCIONES", new Vector2(0, -90), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("OptionsMain"));
        
        CreateButton(panel.transform, "SALIR", new Vector2(0, -160), new Vector2(300, 60), 
            () => {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });
        
        // Set first button as selected for gamepad navigation
        StartCoroutine(SelectButtonDelayed(newGameBtn));
    }
    
    void GenerateOptionsMainPanel()
    {
        GameObject panel = CreateBasePanel("Options Main Panel", "OptionsMain");
        
        CreateTitle(panel.transform, "OPCIONES", new Vector2(0, 250));
        
        Button audioBtn = CreateButton(panel.transform, "AUDIO", new Vector2(0, 100), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("AudioOptions"));
        
        CreateButton(panel.transform, "GRAFICOS", new Vector2(0, 30), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("GraphicsOptions"));
        
        CreateButton(panel.transform, "CONTROLES", new Vector2(0, -40), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("ControlsOptions"));
        
        CreateButton(panel.transform, "GAMEPLAY", new Vector2(0, -110), new Vector2(300, 60), 
            () => uiManager?.ShowPanel("GameplayOptions"));
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -200), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("MainMenu"));
    }
    
    void GenerateAudioOptionsPanel()
    {
        GameObject panel = CreateBasePanel("Audio Options Panel", "AudioOptions");
        
        CreateTitle(panel.transform, "OPCIONES DE AUDIO", new Vector2(0, 300));
        
        // Volume sliders
        CreateSlider(panel.transform, "Volumen General", new Vector2(0, 200), new Vector2(400, 30), 1f);
        CreateSlider(panel.transform, "Música", new Vector2(0, 130), new Vector2(400, 30), 0.7f);
        CreateSlider(panel.transform, "Efectos", new Vector2(0, 60), new Vector2(400, 30), 0.8f);
        CreateSlider(panel.transform, "UI", new Vector2(0, -10), new Vector2(400, 30), 0.6f);
        CreateSlider(panel.transform, "Ambiente", new Vector2(0, -80), new Vector2(400, 30), 0.5f);
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -200), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("OptionsMain"));
    }
    
    void GenerateGraphicsOptionsPanel()
    {
        GameObject panel = CreateBasePanel("Graphics Options Panel", "GraphicsOptions");
        
        CreateTitle(panel.transform, "OPCIONES GRAFICAS", new Vector2(0, 300));
        
        // Quality dropdown simulation with buttons
        CreateButton(panel.transform, "Calidad: Alta", new Vector2(0, 200), new Vector2(300, 50));
        CreateButton(panel.transform, "Resolución: 1920x1080", new Vector2(0, 130), new Vector2(300, 50));
        CreateButton(panel.transform, "Pantalla Completa: SI", new Vector2(0, 60), new Vector2(300, 50));
        CreateButton(panel.transform, "VSync: SI", new Vector2(0, -10), new Vector2(300, 50));
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -200), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("OptionsMain"));
    }
    
    void GenerateControlsOptionsPanel()
    {
        GameObject panel = CreateBasePanel("Controls Options Panel", "ControlsOptions");
        
        CreateTitle(panel.transform, "CONTROLES", new Vector2(0, 300));
        
        CreateSlider(panel.transform, "Sensibilidad Mouse", new Vector2(0, 200), new Vector2(400, 30), 0.5f);
        CreateButton(panel.transform, "Invertir Mouse Y: NO", new Vector2(0, 130), new Vector2(300, 50));
        CreateButton(panel.transform, "Vibración Gamepad: SI", new Vector2(0, 60), new Vector2(300, 50));
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -200), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("OptionsMain"));
    }
    
    void GenerateGameplayOptionsPanel()
    {
        GameObject panel = CreateBasePanel("Gameplay Options Panel", "GameplayOptions");
        
        CreateTitle(panel.transform, "GAMEPLAY", new Vector2(0, 300));
        
        CreateButton(panel.transform, "Dificultad: Normal", new Vector2(0, 200), new Vector2(300, 50));
        CreateButton(panel.transform, "Auto-Guardado: SI", new Vector2(0, 130), new Vector2(300, 50));
        CreateButton(panel.transform, "Subtítulos: SI", new Vector2(0, 60), new Vector2(300, 50));
        
        CreateButton(panel.transform, "VOLVER", new Vector2(0, -200), new Vector2(200, 50), 
            () => uiManager?.ShowPanel("OptionsMain"));
    }
    
    void GenerateHUDPanel()
    {
        GameObject panel = CreateBasePanel("HUD Panel", "HUD");
        
        // Health bar (top left)
        CreateProgressBar(panel.transform, "Salud", new Vector2(-700, 400), new Vector2(200, 20), Color.red);
        
        // Stamina bar (below health)
        CreateProgressBar(panel.transform, "Stamina", new Vector2(-700, 360), new Vector2(200, 20), Color.yellow);
        
        // Crosshair (center)
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(panel.transform, false);
        
        RectTransform crossRect = crosshair.AddComponent<RectTransform>();
        crossRect.anchoredPosition = Vector2.zero;
        crossRect.sizeDelta = new Vector2(20, 20);
        crossRect.anchorMin = new Vector2(0.5f, 0.5f);
        crossRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Image crossImage = crosshair.AddComponent<Image>();
        crossImage.color = Color.white;
    }
    
    void GeneratePauseMenuPanel()
    {
        GameObject panel = CreateBasePanel("Pause Menu Panel", "PauseMenu");
        
        CreateTitle(panel.transform, "PAUSA", new Vector2(0, 200));
        
        Button continueBtn = CreateButton(panel.transform, "CONTINUAR", new Vector2(0, 80), new Vector2(250, 60), 
            () => uiManager?.ShowPanel("HUD"));
        
        CreateButton(panel.transform, "OPCIONES", new Vector2(0, 10), new Vector2(250, 60), 
            () => uiManager?.ShowPanel("OptionsMain"));
        
        CreateButton(panel.transform, "MENU PRINCIPAL", new Vector2(0, -60), new Vector2(250, 60), 
            () => uiManager?.ShowPanel("MainMenu"));
        
        CreateButton(panel.transform, "SALIR", new Vector2(0, -130), new Vector2(250, 60), 
            () => {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });
    }
    
    void GenerateLoadingPanel()
    {
        GameObject panel = CreateBasePanel("Loading Panel", "Loading");
        
        CreateTitle(panel.transform, "CARGANDO...", new Vector2(0, 50));
        
        // Loading bar
        CreateProgressBar(panel.transform, "", new Vector2(0, -50), new Vector2(400, 30), primaryColor);
    }
    
    void CreateTitle(Transform parent, string text, Vector2 position)
    {
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(parent, false);
        
        RectTransform rect = titleGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(800, 80);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        TextMeshProUGUI textComponent = titleGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.fontSize = 48;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        if (customFont != null)
        {
            textComponent.font = TMP_FontAsset.CreateFontAsset(customFont);
        }
    }
    
    void CreateProgressBar(Transform parent, string labelText, Vector2 position, Vector2 size, Color barColor)
    {
        GameObject barGO = new GameObject($"ProgressBar_{labelText}");
        barGO.transform.SetParent(parent, false);
        
        RectTransform rect = barGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        // Background
        Image background = barGO.AddComponent<Image>();
        background.color = Color.gray;
        
        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(barGO.transform, false);
        
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = barColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        
        // Label
        if (!string.IsNullOrEmpty(labelText))
        {
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(barGO.transform, false);
            
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(-size.x/2 - 10, 0);
            labelRect.sizeDelta = new Vector2(100, size.y);
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(1f, 0.5f);
            
            TextMeshProUGUI labelComponent = labelGO.AddComponent<TextMeshProUGUI>();
            labelComponent.text = labelText;
            labelComponent.color = Color.white;
            labelComponent.fontSize = 16;
            labelComponent.alignment = TextAlignmentOptions.Center;
        }
    }
    
    System.Collections.IEnumerator SelectButtonDelayed(Button button)
    {
        yield return new WaitForEndOfFrame();
        
        if (button != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
    
    void SetupNavigation()
    {
        LogDebug("🧭 Setting up panel navigation...");
        
        SetPanelNavigation("OptionsMain", "", "MainMenu");
        SetPanelNavigation("AudioOptions", "", "OptionsMain");
        SetPanelNavigation("GraphicsOptions", "", "OptionsMain");
        SetPanelNavigation("ControlsOptions", "", "OptionsMain");
        SetPanelNavigation("GameplayOptions", "", "OptionsMain");
        SetPanelNavigation("PauseMenu", "", "HUD");
        
        LogDebug("✅ Navigation configured");
    }
    
    void SetPanelNavigation(string panelID, string nextID, string previousID)
    {
        if (createdPanels.TryGetValue(panelID, out GameObject panelGO))
        {
            ConcreteUIPanel panel = panelGO.GetComponent<ConcreteUIPanel>();
            if (panel != null)
            {
                panel.nextPanelID = nextID ?? "";
                panel.previousPanelID = previousID ?? "";
            }
        }
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
                LogDebug("⚠️ Could not find uiPanels field in UIManager");
            }
        }
    }
    
    void FinalizeGeneration()
    {
        LogDebug("🏁 Finalizing generation...");
        
        // Ensure only main menu is visible
        foreach (var kvp in createdPanels)
        {
            bool shouldBeVisible = kvp.Key == "MainMenu";
            kvp.Value.SetActive(shouldBeVisible);
        }
        
        LogDebug("✅ Generation finalized");
    }
    
    void ShowGenerationSummary()
    {
        string summary = $@"
🎯 COMPLETE UI GENERATION SUMMARY
==================================
📄 Panels Created: {panelsCreated}
🔘 Buttons Created: {buttonsCreated}  
🎚️ Sliders Created: {slidersCreated}

📋 Generated Panels:
{string.Join("\n", createdPanels.Keys.Select(id => $"  • {id}"))}

✅ Complete UI system generated!
🎮 Ready to play and customize!
";
        
        Debug.Log(summary);
        
        #if UNITY_EDITOR
        EditorUtility.DisplayDialog("UI Generation Complete", 
            $"Successfully generated complete UI system!\n\n" +
            $"Panels: {panelsCreated}\nButtons: {buttonsCreated}\nSliders: {slidersCreated}\n\n" +
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
            Button[] buttons = kvp.Value.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = primaryColor;
                colors.highlightedColor = primaryColor * 1.2f;
                colors.pressedColor = primaryColor * 0.8f;
                button.colors = colors;
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
        
        ResetCounters();
        LogDebug("🧹 Generated UI destroyed");
    }
}

// =====================================================================
// 📄 CLASE CONCRETA PARA UIPanel
// =====================================================================

/// <summary>
/// Implementación concreta de UIPanel para uso con el generador
/// </summary>
public class ConcreteUIPanel : UIPanel
{
    [Header("🎯 Panel Configuration")]
    public bool startVisible = false;
    public bool useScaleAnimation = true;
    public bool blockGameInput = true;
    public string nextPanelID = "";
    public string previousPanelID = "";
    
    protected override void OnInitialize()
    {
        // Configurar propiedades usando reflection para compatibilidad
        var startVisibleField = typeof(UIPanel).GetField("startVisible", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var useScaleField = typeof(UIPanel).GetField("useScaleAnimation", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var blockInputField = typeof(UIPanel).GetField("blockGameInput", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nextField = typeof(UIPanel).GetField("nextPanelID", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var prevField = typeof(UIPanel).GetField("previousPanelID", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        startVisibleField?.SetValue(this, startVisible);
        useScaleField?.SetValue(this, useScaleAnimation);
        blockInputField?.SetValue(this, blockGameInput);
        nextField?.SetValue(this, nextPanelID);
        prevField?.SetValue(this, previousPanelID);
        
        Debug.Log($"🎨 [ConcreteUIPanel] Initialized panel: {panelID}");
    }
}