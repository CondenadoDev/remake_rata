using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Genera automáticamente un sistema completo de pausa con menú funcional
/// </summary>
public class PauseMenuGenerator : MonoBehaviour
{
    [Header("🎮 Pause Menu Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool destroyAfterGeneration = true;
    
    [Header("🖼️ Canvas Configuration")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private bool createNewCanvas = false;
    [SerializeField] private string canvasName = "Pause Canvas";
    [SerializeField] private int canvasSortOrder = 200;
    
    [Header("🎨 Menu Style")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Color buttonHoverColor = new Color(0.3f, 0.7f, 1f, 1f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Vector2 buttonSize = new Vector2(250, 60);
    [SerializeField] private float buttonSpacing = 80f;
    
    [Header("🔧 Pause Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool pauseAudio = true;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GeneratePauseSystemAfterFrame());
        }
    }
    
    System.Collections.IEnumerator GeneratePauseSystemAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        GenerateCompletePauseSystem();
        
        if (destroyAfterGeneration)
        {
            Destroy(gameObject);
        }
    }
    
    [ContextMenu("🛑 Generate Complete Pause System")]
    public void GenerateCompletePauseSystem()
    {
        Debug.Log("🛑 Generating complete pause system...");
        
        // Buscar o crear Canvas
        Canvas canvas = FindOrCreateCanvas();
        
        if (canvas == null)
        {
            Debug.LogError("❌ No Canvas available for pause menu generation!");
            return;
        }
        
        // Asegurar EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            CreateEventSystem();
        }
        
        Debug.Log($"🛑 Generating pause menu in Canvas: {canvas.name}");
        
        // Crear Panel de Pausa principal
        GameObject pausePanel = CreatePausePanel(canvas.transform);
        
        // Crear elementos del menú
        CreateMenuTitle(pausePanel.transform);
        CreateMenuButtons(pausePanel.transform);
        
        // Crear sistema de gestión de pausa
        CreatePauseManager(pausePanel);
        
        Debug.Log("✅ Complete pause system generated successfully!");
        
        #if UNITY_EDITOR
        EditorUtility.DisplayDialog("Pause System Generated", 
            $"✅ Complete pause system generated!\n\n" +
            $"Canvas: {canvas.name}\n" +
            $"Pause Key: {pauseKey}\n\n" +
            "Buttons created:\n" +
            "• Resume Game\n" +
            "• Main Menu\n" +
            "• Quit Game\n\n" +
            "Press ESC in game to test!", "Perfect!");
        #endif
    }
    
    Canvas FindOrCreateCanvas()
    {
        if (targetCanvas != null)
        {
            return targetCanvas;
        }
        
        if (createNewCanvas)
        {
            return CreateNewCanvas();
        }
        
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null)
        {
            return existingCanvas;
        }
        
        return CreateNewCanvas();
    }
    
    Canvas CreateNewCanvas()
    {
        GameObject canvasObj = new GameObject(canvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortOrder;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        return canvas;
    }
    
    void CreateEventSystem()
    {
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        Debug.Log("✅ EventSystem created");
    }
    
    GameObject CreatePausePanel(Transform canvasTransform)
    {
        GameObject pausePanel = new GameObject("🛑 Pause Menu");
        pausePanel.transform.SetParent(canvasTransform, false);
        pausePanel.SetActive(false); // Inicialmente oculto
        
        RectTransform rect = pausePanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Fondo semi-transparente
        Image background = pausePanel.AddComponent<Image>();
        background.color = backgroundColor;
        background.raycastTarget = true; // Bloquear clicks
        
        return pausePanel;
    }
    
    void CreateMenuTitle(Transform parent)
    {
        GameObject titleObj = new GameObject("⏸️ Title");
        titleObj.transform.SetParent(parent, false);
        
        RectTransform rect = titleObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 150);
        rect.sizeDelta = new Vector2(400, 80);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "JUEGO PAUSADO";
        titleText.fontSize = 42;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        // Outline para mejor legibilidad
        Outline outline = titleObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
    }
    
    void CreateMenuButtons(Transform parent)
    {
        // Contenedor de botones
        GameObject buttonContainer = new GameObject("📋 Button Container");
        buttonContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, -20);
        containerRect.sizeDelta = new Vector2(buttonSize.x, buttonSize.y * 3 + buttonSpacing * 2);
        
        // Crear botones
        CreateButton(buttonContainer.transform, "▶️ REANUDAR", 0, () => {
            PauseMenuManager pauseManager = FindObjectOfType<PauseMenuManager>();
            if (pauseManager != null) pauseManager.ResumeGame();
        });
        
        CreateButton(buttonContainer.transform, "🏠 MENÚ PRINCIPAL", 1, () => {
            PauseMenuManager pauseManager = FindObjectOfType<PauseMenuManager>();
            if (pauseManager != null) pauseManager.GoToMainMenu();
        });
        
        CreateButton(buttonContainer.transform, "❌ SALIR DEL JUEGO", 2, () => {
            PauseMenuManager pauseManager = FindObjectOfType<PauseMenuManager>();
            if (pauseManager != null) pauseManager.QuitGame();
        });
    }
    
    void CreateButton(Transform parent, string text, int index, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -(index * buttonSpacing));
        rect.sizeDelta = buttonSize;
        
        // Imagen del botón
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;
        
        // Componente Button
        Button button = buttonObj.AddComponent<Button>();
        
        // Colores del botón
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = buttonColor * 0.8f;
        colors.selectedColor = buttonHoverColor;
        button.colors = colors;
        
        // Texto del botón
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 20;
        buttonText.color = textColor;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        
        // Outline para el texto
        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(1, -1);
        
        // Eventos del botón
        if (onClick != null)
        {
            button.onClick.AddListener(() => {
                Debug.Log($"🛑 Button pressed: {text}");
                onClick.Invoke();
            });
        }
        
        // Efectos hover simples
        AddButtonHoverEffect(buttonObj, button, buttonImage);
    }
    
    void AddButtonHoverEffect(GameObject buttonObj, Button button, Image buttonImage)
    {
        ButtonHoverEffect hoverEffect = buttonObj.AddComponent<ButtonHoverEffect>();
        hoverEffect.SetupButton(button, buttonImage, buttonColor, buttonHoverColor);
    }
    
    void CreatePauseManager(GameObject pausePanel)
    {
        // Buscar si ya existe un PauseMenuManager
        PauseMenuManager existingManager = FindObjectOfType<PauseMenuManager>();
        
        if (existingManager != null)
        {
            // Asignar el panel al manager existente
            existingManager.pausePanel = pausePanel;
            existingManager.pauseKey = pauseKey;
            existingManager.pauseAudio = pauseAudio;
            existingManager.mainMenuSceneName = mainMenuSceneName;
            Debug.Log("✅ Existing pause manager updated with new panel");
        }
        else
        {
            // Crear nuevo manager
            GameObject managerObj = new GameObject("🛑 Pause Menu Manager");
            PauseMenuManager pauseManager = managerObj.AddComponent<PauseMenuManager>();
            pauseManager.pausePanel = pausePanel;
            pauseManager.pauseKey = pauseKey;
            pauseManager.pauseAudio = pauseAudio;
            pauseManager.mainMenuSceneName = mainMenuSceneName;
            Debug.Log("✅ New pause manager created and configured");
        }
    }
}