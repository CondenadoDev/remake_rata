
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script que configura automáticamente todo el sistema UI basándose en la jerarquía existente
/// </summary>
public class AutoUISetup : MonoBehaviour
{
    [Header("🚀 Auto Setup Configuration")] [SerializeField]
    private bool runOnStart = false;

    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool createMissingComponents = true;
    [SerializeField] private bool configureNavigation = true;

    [Header("📋 Panel Detection Rules")] [SerializeField]
    private string[] menuKeywords = { "Menu", "Inicio", "Main", "Principal" };

    [SerializeField] private string[] optionsKeywords = { "Options", "Opciones", "Settings", "Config" };
    [SerializeField] private string[] audioKeywords = { "Audio", "Sound", "Sonido" };
    [SerializeField] private string[] graphicsKeywords = { "Graphics", "Video", "Graficos", "Visual" };
    [SerializeField] private string[] controlsKeywords = { "Controls", "Input", "Controles" };
    [SerializeField] private string[] gameplayKeywords = { "Gameplay", "Game", "Juego" };
    [SerializeField] private string[] hudKeywords = { "HUD", "Interfaz", "Interface" };
    [SerializeField] private string[] pauseKeywords = { "Pause", "Pausa" };

    [Header("📊 Setup Results")] [SerializeField]
    private int panelsFound = 0;

    [SerializeField] private int panelsConfigured = 0;
    [SerializeField] private int panelsCreated = 0;
    [SerializeField] private string[] configuredPanelIDs;

    private UIManager uiManager;
    private Dictionary<string, GameObject> foundPanels = new Dictionary<string, GameObject>();

    void Start()
    {
        if (runOnStart)
        {
            RunAutoSetup();
        }
    }

    [ContextMenu("🚀 Run Complete Auto Setup")]
    public void RunAutoSetup()
    {
        LogDebug("🚀 Starting Auto UI Setup...");

        try
        {
            ResetCounters();
            FindOrCreateUIManager();
            ScanExistingPanels();
            ConfigureFoundPanels();
            CreateMissingPanels();
            SetupUIManager();
            ConfigurePanelNavigation();
            FinalizeSetup();

            LogDebug($"✅ Auto Setup Complete! Configured {panelsConfigured} panels, Created {panelsCreated} panels");
            ShowSetupSummary();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Auto Setup failed: {e.Message}\n{e.StackTrace}");
        }
    }

    void ResetCounters()
    {
        panelsFound = 0;
        panelsConfigured = 0;
        panelsCreated = 0;
        foundPanels.Clear();
    }

    void FindOrCreateUIManager()
    {
        LogDebug("🔍 Finding UIManager...");

        uiManager = FindFirstObjectByType<UIManager>();

        if (uiManager == null)
        {
            LogDebug("📦 Creating UIManager...");
            GameObject uiManagerGO = new GameObject("UIManager");
            uiManager = uiManagerGO.AddComponent<UIManager>();
            LogDebug("✅ UIManager created");
        }
        else
        {
            LogDebug("✅ UIManager found");
        }
    }

    void ScanExistingPanels()
    {
        LogDebug("🔍 Scanning existing panels...");

        // Buscar todos los GameObjects en la escena que podrían ser paneles
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>(true); // Incluir inactivos

        foreach (GameObject go in allGameObjects)
        {
            string panelID = DeterminePanelID(go.name);
            if (!string.IsNullOrEmpty(panelID))
            {
                foundPanels[panelID] = go;
                panelsFound++;
                LogDebug($"📋 Found panel: {go.name} → ID: {panelID}");
            }
        }

        LogDebug($"📊 Total panels found: {panelsFound}");
    }

    string DeterminePanelID(string gameObjectName)
    {
        string lowerName = gameObjectName.ToLower();

        // Menu principal
        if (ContainsAnyKeyword(lowerName, menuKeywords) && !ContainsAnyKeyword(lowerName, optionsKeywords))
            return "MainMenu";

        // Opciones principal
        if (ContainsAnyKeyword(lowerName, optionsKeywords) && !ContainsAnyKeyword(lowerName, audioKeywords)
                                                           && !ContainsAnyKeyword(lowerName, graphicsKeywords) &&
                                                           !ContainsAnyKeyword(lowerName, controlsKeywords))
            return "OptionsMenu";

        // Categorías de opciones
        if (ContainsAnyKeyword(lowerName, audioKeywords))
            return "AudioOptions";
        if (ContainsAnyKeyword(lowerName, graphicsKeywords))
            return "GraphicsOptions";
        if (ContainsAnyKeyword(lowerName, controlsKeywords))
            return "ControlsOptions";
        if (ContainsAnyKeyword(lowerName, gameplayKeywords))
            return "GameplayOptions";

        // Paneles de juego
        if (ContainsAnyKeyword(lowerName, hudKeywords))
            return "HUD";
        if (ContainsAnyKeyword(lowerName, pauseKeywords))
            return "PauseMenu";

        // Paneles generales
        if (lowerName.Contains("panel") || lowerName.Contains("menu"))
        {
            // Intentar extraer un ID del nombre
            string cleanName = gameObjectName.Replace("Panel", "").Replace("Menu", "").Replace(" ", "");
            if (!string.IsNullOrEmpty(cleanName))
                return cleanName;
        }

        return null; // No es un panel reconocible
    }

    bool ContainsAnyKeyword(string text, string[] keywords)
    {
        return keywords.Any(keyword => text.Contains(keyword.ToLower()));
    }

    void ConfigureFoundPanels()
    {
        LogDebug("⚙️ Configuring found panels...");

        foreach (var kvp in foundPanels)
        {
            string panelID = kvp.Key;
            GameObject panelGO = kvp.Value;

            try
            {
                ConfigurePanel(panelGO, panelID);
                panelsConfigured++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to configure panel {panelID}: {e.Message}");
            }
        }
    }

    void ConfigurePanel(GameObject panelGO, string panelID)
    {
        LogDebug($"⚙️ Configuring panel: {panelID}");

        // Asegurar que tenga CanvasGroup
        CanvasGroup canvasGroup = panelGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panelGO.AddComponent<CanvasGroup>();
            LogDebug($"➕ Added CanvasGroup to {panelID}");
        }

        // Determinar qué script necesita
        UIPanel uiPanel = GetOrAddUIPanel(panelGO, panelID);

        // Configurar el panel ID
        uiPanel.panelID = panelID;

        // Configurar propiedades usando reflection
        ConfigurePanelProperties(uiPanel, panelID);

        LogDebug($"✅ Panel {panelID} configured");
    }

    UIPanel GetOrAddUIPanel(GameObject panelGO, string panelID)
    {
        // Buscar script específico primero
        UIPanel existingPanel = panelGO.GetComponent<UIPanel>();

        if (existingPanel != null)
        {
            LogDebug($"📋 Using existing UIPanel on {panelID}");
            return existingPanel;
        }

        // Determinar qué tipo de panel agregar
        System.Type panelType = DeterminePanelType(panelID);

        UIPanel newPanel = panelGO.AddComponent(panelType) as UIPanel;
        LogDebug($"➕ Added {panelType.Name} to {panelID}");

        return newPanel;
    }

    System.Type DeterminePanelType(string panelID)
    {
        switch (panelID)
        {
            case "OptionsMenu":
                return typeof(OptionsMenuPanel);
            case "HUD":
                return typeof(HUDPanel);
            case "PauseMenu":
                return typeof(PauseMenuPanel);
            default:
                return typeof(UIPanel);
        }
    }

    void ConfigurePanelProperties(UIPanel panel, string panelID)
    {
        // Usar reflection para acceder a campos protegidos
        Type panelType = typeof(UIPanel);
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

        FieldInfo startVisibleField = panelType.GetField("startVisible", flags);
        FieldInfo useScaleAnimationField = panelType.GetField("useScaleAnimation", flags);
        FieldInfo blockGameInputField = panelType.GetField("blockGameInput", flags);

        // Configuración según el tipo de panel
        switch (panelID)
        {
            case "MainMenu":
                SetFieldValue(startVisibleField, panel, true);
                SetFieldValue(useScaleAnimationField, panel, true);
                SetFieldValue(blockGameInputField, panel, false);
                break;

            case "HUD":
                SetFieldValue(startVisibleField, panel, false);
                SetFieldValue(useScaleAnimationField, panel, false);
                SetFieldValue(blockGameInputField, panel, false);
                break;

            case "PauseMenu":
            case "OptionsMenu":
            case "AudioOptions":
            case "GraphicsOptions":
            case "ControlsOptions":
            case "GameplayOptions":
                SetFieldValue(startVisibleField, panel, false);
                SetFieldValue(useScaleAnimationField, panel, true);
                SetFieldValue(blockGameInputField, panel, true);
                break;

            default:
                SetFieldValue(startVisibleField, panel, false);
                SetFieldValue(useScaleAnimationField, panel, true);
                SetFieldValue(blockGameInputField, panel, true);
                break;
        }

        LogDebug($"📐 Properties configured for {panelID}");
    }

    void SetFieldValue(FieldInfo field, object target, object value)
    {
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    void CreateMissingPanels()
    {
        LogDebug("🏗️ Creating missing essential panels...");

        string[] essentialPanels = { "MainMenu", "OptionsMenu", "HUD", "PauseMenu" };

        foreach (string panelID in essentialPanels)
        {
            if (!foundPanels.ContainsKey(panelID))
            {
                LogDebug($"🏗️ Creating missing panel: {panelID}");
                CreatePanel(panelID);
                panelsCreated++;
            }
        }
    }

    void CreatePanel(string panelID)
    {
        GameObject panelGO = new GameObject($"{panelID} Panel");

        // Configurar en la jerarquía
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            panelGO.transform.SetParent(canvas.transform, false);
        }

        // Configurar RectTransform para panel full-screen
        RectTransform rectTransform = panelGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Agregar componentes
        CanvasGroup canvasGroup = panelGO.AddComponent<CanvasGroup>();

        // Agregar script apropiado
        System.Type panelType = DeterminePanelType(panelID);
        UIPanel panel = panelGO.AddComponent(panelType) as UIPanel;

        // Configurar
        panel.panelID = panelID;
        ConfigurePanelProperties(panel, panelID);

        // Agregar a la lista
        foundPanels[panelID] = panelGO;

        LogDebug($"✅ Created panel: {panelID}");
    }

    void SetupUIManager()
    {
        LogDebug("📋 Setting up UIManager with found panels...");

        List<UIPanel> panelList = new List<UIPanel>();

        foreach (var kvp in foundPanels)
        {
            UIPanel panel = kvp.Value.GetComponent<UIPanel>();
            if (panel != null)
            {
                panelList.Add(panel);
                LogDebug($"📋 Added {kvp.Key} to UIManager");
            }
        }

        // Configurar el array en UIManager usando reflection
        if (uiManager != null)
        {
            var field = typeof(UIManager).GetField("uiPanels",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(uiManager, panelList.ToArray());
                LogDebug($"✅ UIManager configured with {panelList.Count} panels");
            }
            else
            {
                // Intentar con campo público si existe
                var publicField = typeof(UIManager).GetField("uiPanels",
                    BindingFlags.Public | BindingFlags.Instance);
                if (publicField != null)
                {
                    publicField.SetValue(uiManager, panelList.ToArray());
                    LogDebug($"✅ UIManager configured with {panelList.Count} panels (public field)");
                }
                else
                {
                    Debug.LogWarning("⚠️ Could not find uiPanels field in UIManager. Please assign panels manually.");
                }
            }
        }
    }

    void ConfigurePanelNavigation()
    {
        if (!configureNavigation) return;

        LogDebug("🧭 Configuring panel navigation...");

        // Configurar navegación básica
        ConfigureNavigationForPanel("OptionsMenu", null, "MainMenu");
        ConfigureNavigationForPanel("AudioOptions", null, "OptionsMenu");
        ConfigureNavigationForPanel("GraphicsOptions", null, "OptionsMenu");
        ConfigureNavigationForPanel("ControlsOptions", null, "OptionsMenu");
        ConfigureNavigationForPanel("GameplayOptions", null, "OptionsMenu");
        ConfigureNavigationForPanel("PauseMenu", null, "HUD");

        LogDebug("✅ Navigation configured");
    }

    void ConfigureNavigationForPanel(string panelID, string nextID, string previousID)
    {
        if (foundPanels.TryGetValue(panelID, out GameObject panelGO))
        {
            UIPanel panel = panelGO.GetComponent<UIPanel>();
            if (panel != null)
            {
                var nextField = typeof(UIPanel).GetField("nextPanelID",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var prevField = typeof(UIPanel).GetField("previousPanelID",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (nextField != null) nextField.SetValue(panel, nextID ?? "");
                if (prevField != null) prevField.SetValue(panel, previousID ?? "");

                LogDebug($"🧭 Navigation set for {panelID}: Previous={previousID}");
            }
        }
    }

    void FinalizeSetup()
    {
        LogDebug("🏁 Finalizing setup...");

        // Crear lista de IDs configurados
        configuredPanelIDs = foundPanels.Keys.ToArray();

        // Asegurar que paneles estén en el estado correcto
        foreach (var kvp in foundPanels)
        {
            UIPanel panel = kvp.Value.GetComponent<UIPanel>();
            if (panel != null)
            {
                // Usar reflection para leer startVisible
                var startVisibleField = typeof(UIPanel).GetField("startVisible",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (startVisibleField != null)
                {
                    bool shouldStartVisible = (bool)startVisibleField.GetValue(panel);
                    if (!shouldStartVisible)
                    {
                        kvp.Value.SetActive(false);
                    }
                }
            }
        }

        LogDebug("✅ Setup finalized");
    }

    void ShowSetupSummary()
    {
        string summary = $@"
🎯 AUTO UI SETUP SUMMARY
========================
📊 Panels Found: {panelsFound}
⚙️ Panels Configured: {panelsConfigured}
🏗️ Panels Created: {panelsCreated}

📋 Configured Panels:
{string.Join("\n", configuredPanelIDs.Select(id => $"  • {id}"))}

✅ Setup completed successfully!
💡 You can now focus on styling and content.
";

        Debug.Log(summary);

#if UNITY_EDITOR
        EditorUtility.DisplayDialog("Auto UI Setup Complete",
            $"Successfully configured {panelsConfigured} panels and created {panelsCreated} new panels.\n\n" +
            "Check the console for detailed summary.", "OK");
#endif
    }

    // Métodos públicos para configuración manual
    [ContextMenu("🔍 Scan Panels Only")]
    public void ScanPanelsOnly()
    {
        ResetCounters();
        ScanExistingPanels();
        LogDebug($"📊 Scan complete. Found {panelsFound} panels.");
    }

    [ContextMenu("⚙️ Configure Existing Panels Only")]
    public void ConfigureExistingOnly()
    {
        ScanExistingPanels();
        ConfigureFoundPanels();
        LogDebug($"⚙️ Configuration complete. Configured {panelsConfigured} panels.");
    }

    [ContextMenu("🏗️ Create Missing Panels Only")]
    public void CreateMissingOnly()
    {
        ScanExistingPanels();
        CreateMissingPanels();
        LogDebug($"🏗️ Creation complete. Created {panelsCreated} panels.");
    }

    void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"🚀 [AutoUISetup] {message}");
    }
}