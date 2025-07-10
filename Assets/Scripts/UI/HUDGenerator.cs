using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Genera automáticamente un HUD completo y funcional
/// </summary>
public class HUDGenerator : MonoBehaviour
{
    [Header("🎨 HUD Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool destroyAfterGeneration = true;
    
    [Header("🖼️ Canvas Configuration")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private bool createNewCanvas = false;
    [SerializeField] private string canvasName = "HUD Canvas";
    [SerializeField] private int canvasSortOrder = 100;
    
    [Header("🎯 HUD Configuration")]
    [SerializeField] private Color healthColor = Color.red;
    [SerializeField] private Color staminaColor = Color.yellow;
    [SerializeField] private Color backgroundBarColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Vector2 healthBarSize = new Vector2(200, 20);
    [SerializeField] private Vector2 staminaBarSize = new Vector2(200, 15);
    [SerializeField] private Vector2 barSpacing = new Vector2(0, 10);
    
    [Header("📱 UI Style")]
    [SerializeField] private float cornerRadius = 5f;
    [SerializeField] private bool addGlow = true;
    [SerializeField] private bool addOutline = true;
    
    void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateHUDAfterFrame());
        }
    }
    
    System.Collections.IEnumerator GenerateHUDAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        GenerateCompleteHUD();
        
        if (destroyAfterGeneration)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Valida y optimiza la configuración del Canvas para HUD
    /// </summary>
    void ValidateCanvasForHUD(Canvas canvas)
    {
        if (canvas == null) return;
        
        // Asegurar configuración óptima para HUD
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning($"⚠️ Canvas {canvas.name} is not ScreenSpaceOverlay, HUD may not display correctly");
        }
        
        if (canvas.sortingOrder < 50)
        {
            Debug.LogWarning($"⚠️ Canvas {canvas.name} has low sorting order ({canvas.sortingOrder}), HUD may be hidden behind other UI");
        }
        
        // Verificar CanvasScaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            Debug.LogWarning($"⚠️ Canvas {canvas.name} doesn't have CanvasScaler, HUD may not scale properly");
        }
        else if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            Debug.LogWarning($"⚠️ Canvas {canvas.name} is not set to ScaleWithScreenSize, HUD may not be responsive");
        }
        
        Debug.Log($"✅ Canvas {canvas.name} validated for HUD generation");
    }
    
    [ContextMenu("🚀 Generate Complete HUD")]
    public void GenerateCompleteHUD()
    {
        Debug.Log("🎨 Generating complete HUD system...");
        
        // Buscar o crear Canvas
        Canvas canvas = FindOrCreateCanvas();
        
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas available for HUD generation!");
            return;
        }
        
        Debug.Log($"🎨 Generating HUD in Canvas: {canvas.name}");
        
        // Validar Canvas para HUD
        ValidateCanvasForHUD(canvas);
        
        // Crear HUD Panel principal
        GameObject hudPanel = CreateHUDPanel(canvas.transform);
        
        // Crear barras de vida y stamina
        CreateHealthBar(hudPanel.transform);
        CreateStaminaBar(hudPanel.transform);
        
        // Crear elementos adicionales del HUD
        CreateCrosshair(hudPanel.transform);
        CreateComboDisplay(hudPanel.transform);
        CreateInteractionPrompt(hudPanel.transform);
        
        // Conectar con PlayerStats
        ConnectToPlayerStats(hudPanel);
        
        // Registrar en UIManager si existe
        RegisterWithUIManager(hudPanel);
        
        Debug.Log("✅ Complete HUD generated successfully!");
        
        #if UNITY_EDITOR
        EditorUtility.DisplayDialog("HUD Generated", 
            $"✅ Complete HUD system generated!\n\n" +
            $"Canvas: {canvas.name}\n" +
            $"Sort Order: {canvas.sortingOrder}\n\n" +
            "• Health Bar\n" +
            "• Stamina Bar\n" +
            "• Crosshair\n" +
            "• Combo Display\n" +
            "• Interaction Prompt\n\n" +
            "All connected to PlayerStats automatically!", "Perfect!");
        #endif
    }
    
    [ContextMenu("🖼️ Create HUD Canvas Only")]
    public void CreateHUDCanvasOnly()
    {
        Canvas canvas = CreateNewCanvas();
        Debug.Log($"✅ HUD Canvas created: {canvas.name}");
        
        #if UNITY_EDITOR
        // Seleccionar el Canvas creado
        UnityEditor.Selection.activeGameObject = canvas.gameObject;
        #endif
    }
    
    [ContextMenu("🔍 Find and Assign Existing Canvas")]
    public void FindAndAssignCanvas()
    {
        Canvas foundCanvas = FindFirstObjectByType<Canvas>();
        if (foundCanvas != null)
        {
            targetCanvas = foundCanvas;
            Debug.Log($"✅ Canvas assigned: {foundCanvas.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ No Canvas found in scene");
        }
    }
    
    Canvas FindOrCreateCanvas()
    {
        // Si hay un Canvas asignado específicamente, usarlo
        if (targetCanvas != null)
        {
            Debug.Log($"🎨 Using assigned Canvas: {targetCanvas.name}");
            return targetCanvas;
        }
        
        // Si se especifica crear nuevo Canvas, crearlo
        if (createNewCanvas)
        {
            return CreateNewCanvas();
        }
        
        // Buscar Canvas existente
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        
        if (existingCanvas != null)
        {
            Debug.Log($"🎨 Using existing Canvas: {existingCanvas.name}");
            return existingCanvas;
        }
        
        // No hay Canvas, crear uno nuevo
        Debug.Log("🎨 No Canvas found, creating new one");
        return CreateNewCanvas();
    }
    
    Canvas CreateNewCanvas()
    {
        Debug.Log($"🎨 Creating new Canvas: {canvasName}");
        
        GameObject canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortOrder; // Asegurar que esté encima
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // GraphicRaycaster solo si es necesario
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        
        return canvas;
    }
    
    GameObject CreateHUDPanel(Transform canvasTransform)
    {
        GameObject hudPanel = new GameObject("🎮 Player HUD");
        hudPanel.transform.SetParent(canvasTransform, false);
        
        RectTransform rect = hudPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // CanvasGroup para animaciones
        CanvasGroup canvasGroup = hudPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false; // HUD no debe bloquear clicks
        
        // ConcreteUIPanel para integración con UIManager
        ConcreteUIPanel hudComponent = hudPanel.AddComponent<ConcreteUIPanel>();
        hudComponent.panelID = "HUD";
        hudComponent.startVisible = true;
        hudComponent.useScaleAnimation = false;
        hudComponent.blockGameInput = false;
        
        return hudPanel;
    }
    
    void CreateHealthBar(Transform parent)
    {
        // Contenedor de la barra de vida
        GameObject healthContainer = new GameObject("💚 Health Bar");
        healthContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = healthContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20);
        containerRect.sizeDelta = healthBarSize;
        
        // Background de la barra
        Image backgroundImage = healthContainer.AddComponent<Image>();
        backgroundImage.color = backgroundBarColor;
        
        if (addOutline)
        {
            Outline outline = healthContainer.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);
        }
        
        // Slider para la vida
        Slider healthSlider = healthContainer.AddComponent<Slider>();
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = 1f;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(healthContainer.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2, 2);
        fillAreaRect.offsetMax = new Vector2(-2, -2);
        
        // Fill (barra de vida)
        GameObject fill = new GameObject("Health Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = healthColor;
        
        if (addGlow)
        {
            Shadow glow = fill.AddComponent<Shadow>();
            glow.effectColor = new Color(healthColor.r, healthColor.g, healthColor.b, 0.5f);
            glow.effectDistance = new Vector2(0, 0);
        }
        
        // Configurar slider
        healthSlider.fillRect = fillRect;
        healthSlider.targetGraphic = fillImage;
        
        // Label de vida
        GameObject healthLabel = new GameObject("Health Label");
        healthLabel.transform.SetParent(healthContainer.transform, false);
        RectTransform labelRect = healthLabel.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI healthText = healthLabel.AddComponent<TextMeshProUGUI>();
        healthText.text = "100 / 100";
        healthText.fontSize = 12;
        healthText.color = Color.white;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.fontStyle = FontStyles.Bold;
        
        Outline textOutline = healthLabel.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(1, -1);
        
        // Componente para manejar la barra
        HealthBarUI healthBarUI = healthContainer.AddComponent<HealthBarUI>();
        healthBarUI.healthSlider = healthSlider;
        healthBarUI.healthText = healthText;
    }
    
    void CreateStaminaBar(Transform parent)
    {
        // Contenedor de la barra de stamina
        GameObject staminaContainer = new GameObject("⚡ Stamina Bar");
        staminaContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = staminaContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20 - healthBarSize.y - barSpacing.y);
        containerRect.sizeDelta = staminaBarSize;
        
        // Background de la barra
        Image backgroundImage = staminaContainer.AddComponent<Image>();
        backgroundImage.color = backgroundBarColor;
        
        if (addOutline)
        {
            Outline outline = staminaContainer.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);
        }
        
        // Slider para la stamina
        Slider staminaSlider = staminaContainer.AddComponent<Slider>();
        staminaSlider.minValue = 0f;
        staminaSlider.maxValue = 1f;
        staminaSlider.value = 1f;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(staminaContainer.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2, 2);
        fillAreaRect.offsetMax = new Vector2(-2, -2);
        
        // Fill (barra de stamina)
        GameObject fill = new GameObject("Stamina Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = staminaColor;
        
        if (addGlow)
        {
            Shadow glow = fill.AddComponent<Shadow>();
            glow.effectColor = new Color(staminaColor.r, staminaColor.g, staminaColor.b, 0.5f);
            glow.effectDistance = new Vector2(0, 0);
        }
        
        // Configurar slider
        staminaSlider.fillRect = fillRect;
        staminaSlider.targetGraphic = fillImage;
        
        // Label de stamina
        GameObject staminaLabel = new GameObject("Stamina Label");
        staminaLabel.transform.SetParent(staminaContainer.transform, false);
        RectTransform labelRect = staminaLabel.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI staminaText = staminaLabel.AddComponent<TextMeshProUGUI>();
        staminaText.text = "100 / 100";
        staminaText.fontSize = 10;
        staminaText.color = Color.white;
        staminaText.alignment = TextAlignmentOptions.Center;
        staminaText.fontStyle = FontStyles.Bold;
        
        Outline textOutline = staminaLabel.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(1, -1);
        
        // Componente para manejar la barra
        StaminaBarUI staminaBarUI = staminaContainer.AddComponent<StaminaBarUI>();
        staminaBarUI.staminaSlider = staminaSlider;
        staminaBarUI.staminaText = staminaText;
    }
    
    void CreateCrosshair(Transform parent)
    {
        GameObject crosshair = new GameObject("🎯 Crosshair");
        crosshair.transform.SetParent(parent, false);
        
        RectTransform rect = crosshair.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(20, 20);
        
        // Cruz vertical
        GameObject vertical = new GameObject("Vertical");
        vertical.transform.SetParent(crosshair.transform, false);
        RectTransform vRect = vertical.AddComponent<RectTransform>();
        vRect.anchorMin = new Vector2(0.5f, 0.5f);
        vRect.anchorMax = new Vector2(0.5f, 0.5f);
        vRect.pivot = new Vector2(0.5f, 0.5f);
        vRect.sizeDelta = new Vector2(2, 16);
        vRect.anchoredPosition = Vector2.zero;
        
        Image vImage = vertical.AddComponent<Image>();
        vImage.color = new Color(1, 1, 1, 0.8f);
        
        // Cruz horizontal
        GameObject horizontal = new GameObject("Horizontal");
        horizontal.transform.SetParent(crosshair.transform, false);
        RectTransform hRect = horizontal.AddComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0.5f, 0.5f);
        hRect.anchorMax = new Vector2(0.5f, 0.5f);
        hRect.pivot = new Vector2(0.5f, 0.5f);
        hRect.sizeDelta = new Vector2(16, 2);
        hRect.anchoredPosition = Vector2.zero;
        
        Image hImage = horizontal.AddComponent<Image>();
        hImage.color = new Color(1, 1, 1, 0.8f);
        
        // Outline para mejor visibilidad
        Outline vOutline = vertical.AddComponent<Outline>();
        vOutline.effectColor = Color.black;
        vOutline.effectDistance = new Vector2(1, -1);
        
        Outline hOutline = horizontal.AddComponent<Outline>();
        hOutline.effectColor = Color.black;
        hOutline.effectDistance = new Vector2(1, -1);
    }
    
    void CreateComboDisplay(Transform parent)
    {
        GameObject comboDisplay = new GameObject("💥 Combo Display");
        comboDisplay.transform.SetParent(parent, false);
        comboDisplay.SetActive(false); // Inicialmente oculto
        
        RectTransform rect = comboDisplay.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-20, -20);
        rect.sizeDelta = new Vector2(150, 50);
        
        // Background
        Image background = comboDisplay.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);
        
        // Texto del combo
        TextMeshProUGUI comboText = comboDisplay.AddComponent<TextMeshProUGUI>();
        comboText.text = "COMBO x3";
        comboText.fontSize = 18;
        comboText.color = Color.yellow;
        comboText.alignment = TextAlignmentOptions.Center;
        comboText.fontStyle = FontStyles.Bold;
        
        // Componente para manejar el combo
        ComboDisplayUI comboUI = comboDisplay.AddComponent<ComboDisplayUI>();
        comboUI.comboText = comboText;
        comboUI.comboPanel = comboDisplay;
    }
    
    void CreateInteractionPrompt(Transform parent)
    {
        GameObject interactionPrompt = new GameObject("💬 Interaction Prompt");
        interactionPrompt.transform.SetParent(parent, false);
        interactionPrompt.SetActive(false); // Inicialmente oculto
        
        RectTransform rect = interactionPrompt.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, -100);
        rect.sizeDelta = new Vector2(200, 40);
        
        // Background
        Image background = interactionPrompt.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.8f);
        
        // Texto de interacción
        TextMeshProUGUI promptText = interactionPrompt.AddComponent<TextMeshProUGUI>();
        promptText.text = "Press E to interact";
        promptText.fontSize = 14;
        promptText.color = Color.white;
        promptText.alignment = TextAlignmentOptions.Center;
        
        // Componente para manejar las prompts
        InteractionPromptUI promptUI = interactionPrompt.AddComponent<InteractionPromptUI>();
        promptUI.promptText = promptText;
        promptUI.promptPanel = interactionPrompt;
    }
    
    void ConnectToPlayerStats(GameObject hudPanel)
    {
        Debug.Log("🔗 Connecting HUD to PlayerStats...");
        
        // Buscar player
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerStateMachine>()?.gameObject;
        }
        
        if (player != null)
        {
            // Agregar connector si no existe
            PlayerStatsUIConnector connector = player.GetComponent<PlayerStatsUIConnector>();
            if (connector == null)
            {
                connector = player.AddComponent<PlayerStatsUIConnector>();
                Debug.Log("✅ PlayerStatsUIConnector added to player");
            }
            
            Debug.Log("✅ HUD connected to PlayerStats");
        }
        else
        {
            Debug.LogWarning("⚠️ Player not found! HUD created but not connected to stats.");
        }
    }
    
    void RegisterWithUIManager(GameObject hudPanel)
    {
        if (UIManager.Instance != null)
        {
            ConcreteUIPanel hudComponent = hudPanel.GetComponent<ConcreteUIPanel>();
            if (hudComponent != null)
            {
                UIManager.Instance.RegisterPanel(hudComponent);
                Debug.Log("✅ HUD registered with UIManager");
            }
        }
    }
}