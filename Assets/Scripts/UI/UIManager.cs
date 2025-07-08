using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("🎮 UI Panels")]
    [SerializeField] private UIPanel[] uiPanels;
    
    [Header("⚙️ Configuration")]
    [SerializeField] private float defaultTransitionDuration = 0.3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("🔊 Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip clickSound;
    
    // Singleton
    public static UIManager Instance { get; private set; }
    
    // Estado actual
    private Dictionary<string, UIPanel> panelDictionary = new Dictionary<string, UIPanel>();
    private UIPanel currentActivePanel;
    private Stack<UIPanel> panelHistory = new Stack<UIPanel>();
    private bool isInitialized = false;
    
    // Eventos
    public static event System.Action<string> OnPanelOpened;
    public static event System.Action<string> OnPanelClosed;
    public static event System.Action<UIPanel, UIPanel> OnPanelSwitched;

    #region Initialization
    
    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("🔄 [UIManager] Duplicate UIManager found, destroying...");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("🎮 [UIManager] Initializing...");
        StartCoroutine(InitializeAsync());
    }
    
    IEnumerator InitializeAsync()
    {
        yield return new WaitForEndOfFrame();
        
        bool success = false;
        
        try
        {
            InitializePanels();
            SubscribeToEvents();
            success = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [UIManager] Initialization failed: {e.Message}");
        }
        
        isInitialized = success;
        
        if (success)
        {
            Debug.Log($"✅ [UIManager] Successfully initialized with {panelDictionary.Count} panels");
        }
    }
    
    void InitializePanels()
    {
        int successCount = 0;
        int failCount = 0;
        
        foreach (var panel in uiPanels)
        {
            if (panel == null)
            {
                failCount++;
                Debug.LogWarning("⚠️ [UIManager] Found null panel in uiPanels array");
                continue;
            }
            
            if (!UIValidation.ValidateString(panel.panelID, "Panel ID"))
            {
                failCount++;
                Debug.LogError($"❌ [UIManager] Panel {panel.name} has invalid ID");
                continue;
            }
            
            if (panelDictionary.ContainsKey(panel.panelID))
            {
                failCount++;
                Debug.LogError($"❌ [UIManager] Duplicate panel ID: {panel.panelID}");
                continue;
            }
            
            try
            {
                panelDictionary[panel.panelID] = panel;
                panel.Initialize();
                
                // Suscribirse a eventos del panel
                panel.OnPanelOpened += () => {
                    OnPanelOpened?.Invoke(panel.panelID);
                    LogDebug($"Panel opened: {panel.panelID}");
                };
                panel.OnPanelClosed += () => {
                    OnPanelClosed?.Invoke(panel.panelID);
                    LogDebug($"Panel closed: {panel.panelID}");
                };
                
                successCount++;
                LogDebug($"Panel registered: {panel.panelID}");
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError($"❌ [UIManager] Failed to initialize panel {panel.panelID}: {e.Message}");
            }
        }
        
        Debug.Log($"🎮 [UIManager] Panel initialization complete. Success: {successCount}, Failed: {failCount}");
    }
    
    void SubscribeToEvents()
    {
        UIEvents.OnPlayUISound += PlayUISound;
        LogDebug("Subscribed to UI events");
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
        Debug.Log("🗑️ [UIManager] Destroyed");
    }
    
    void UnsubscribeFromEvents()
    {
        UIEvents.OnPlayUISound -= PlayUISound;
        LogDebug("Unsubscribed from UI events");
    }
    
    #endregion

    #region Panel Management
    
public void ShowPanel(string panelID, bool addToHistory = true)
{
    if (!isInitialized)
    {
        Debug.LogError("❌ [UIManager] Not initialized yet, cannot show panel");
        return;
    }

    if (!panelDictionary.TryGetValue(panelID, out UIPanel panel))
    {
        Debug.LogError($"❌ [UIManager] Panel '{panelID}' not found!");
        return;
    }

    LogDebug($"Showing panel: {panelID}");

    // Guardar panel anterior en historial (evitar duplicados)
    if (addToHistory && currentActivePanel != null && currentActivePanel != panel)
    {
        panelHistory.Push(currentActivePanel);
        LogDebug($"Added to history: {currentActivePanel.panelID}");
    }

    // Ocultar panel actual
    if (currentActivePanel != null && currentActivePanel != panel)
    {
        HidePanel(currentActivePanel.panelID, false);
    }

    // Mostrar nuevo panel (sin animación)
    panel.gameObject.SetActive(true);
    panel.SetVisible(true);
    if (panel.CanvasGroup != null)
    {
        panel.CanvasGroup.alpha = 1f;
        panel.CanvasGroup.blocksRaycasts = true;
    }
    if (panel.UseScaleAnimation)
    {
        panel.transform.localScale = Vector3.one;
    }

    try
    {
        panel.OnShowComplete();
    }
    catch { }

    var previousPanel = currentActivePanel;
    currentActivePanel = panel;
    OnPanelSwitched?.Invoke(previousPanel, panel);

    PlaySound(openSound);
}

public void HidePanel(string panelID, bool returnToPrevious = true)
{
    if (!panelDictionary.TryGetValue(panelID, out UIPanel panel))
    {
        Debug.LogError($"❌ [UIManager] Panel '{panelID}' not found!");
        return;
    }

    LogDebug($"Hiding panel: {panelID}");

    // Ocultar panel altiro (sin animación)
    panel.SetVisible(false);
    if (panel.CanvasGroup != null)
    {
        panel.CanvasGroup.alpha = 0f;
        panel.CanvasGroup.blocksRaycasts = false;
    }
    if (panel.UseScaleAnimation)
    {
        panel.transform.localScale = Vector3.one;
    }
    panel.gameObject.SetActive(false);

    try
    {
        panel.OnHideComplete();
    }
    catch { }

    // Volver al panel anterior si existe
    if (returnToPrevious && panelHistory.Count > 0)
    {
        var previousPanel = panelHistory.Pop();
        LogDebug($"Returning to previous panel: {previousPanel.panelID}");
        ShowPanel(previousPanel.panelID, false);
    }
    else if (panel == currentActivePanel)
    {
        currentActivePanel = null;
    }

    PlaySound(closeSound);
}
    
    public void TogglePanel(string panelID)
    {
        if (!panelDictionary.TryGetValue(panelID, out UIPanel panel))
        {
            Debug.LogError($"❌ [UIManager] Panel '{panelID}' not found!");
            return;
        }
        
        if (panel.IsVisible)
        {
            LogDebug($"Toggling OFF: {panelID}");
            HidePanel(panelID);
        }
        else
        {
            LogDebug($"Toggling ON: {panelID}");
            ShowPanel(panelID);
        }
    }
    
    public bool IsPanelVisible(string panelID)
    {
        if (panelDictionary.TryGetValue(panelID, out UIPanel panel))
            return panel.IsVisible;
        return false;
    }
    
    public void HideAllPanels()
    {
        LogDebug("Hiding all panels");
        
        foreach (var panel in panelDictionary.Values)
        {
            if (panel.IsVisible)
            {
                StartCoroutine(HidePanelCoroutine(panel));
            }
        }
        
        currentActivePanel = null;
        panelHistory.Clear();
    }
    
    public void GoBack()
    {
        if (currentActivePanel != null)
        {
            LogDebug($"Going back from: {currentActivePanel.panelID}");
            HidePanel(currentActivePanel.panelID, true);
        }
        else
        {
            LogDebug("No active panel to go back from");
        }
    }
    
    #endregion

    #region Animation Coroutines
    
    private IEnumerator ShowPanelCoroutine(UIPanel panel)
    {
        bool hasError = false;
        
        // Validar panel antes de comenzar animación
        if (panel == null)
        {
            Debug.LogError("❌ [UIManager] Panel is null in ShowPanelCoroutine");
            yield break;
        }
        
        panel.gameObject.SetActive(true);
        panel.SetVisible(true);
        
        // Animación de entrada
        if (panel.CanvasGroup != null)
        {
            panel.CanvasGroup.alpha = 0f;
            panel.CanvasGroup.blocksRaycasts = false;
            
            yield return StartCoroutine(AnimateCanvasGroup(panel.CanvasGroup, 0f, 1f, defaultTransitionDuration));
            
            panel.CanvasGroup.blocksRaycasts = true;
        }
        
        // Escala de entrada (opcional)
        if (panel.UseScaleAnimation)
        {
            panel.transform.localScale = Vector3.zero;
            yield return StartCoroutine(AnimateScale(panel.transform, Vector3.zero, Vector3.one, defaultTransitionDuration));
        }
        
        // Llamar OnShowComplete solo si no hubo errores
        if (!hasError)
        {
            try
            {
                panel.OnShowComplete();
                LogDebug($"Show animation completed: {panel.panelID}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ [UIManager] Error in OnShowComplete for {panel.panelID}: {e.Message}");
            }
        }
    }
    
    private IEnumerator HidePanelCoroutine(UIPanel panel)
    {
        // Validar panel antes de comenzar animación
        if (panel == null)
        {
            Debug.LogError("❌ [UIManager] Panel is null in HidePanelCoroutine");
            yield break;
        }
        
        if (panel.CanvasGroup != null)
        {
            panel.CanvasGroup.blocksRaycasts = false;
            yield return StartCoroutine(AnimateCanvasGroup(panel.CanvasGroup, 1f, 0f, defaultTransitionDuration));
        }
        
        // Escala de salida (opcional)
        if (panel.UseScaleAnimation)
        {
            yield return StartCoroutine(AnimateScale(panel.transform, Vector3.one, Vector3.zero, defaultTransitionDuration));
        }
        
        panel.SetVisible(false);
        panel.gameObject.SetActive(false);
        
        try
        {
            panel.OnHideComplete();
            LogDebug($"Hide animation completed: {panel.panelID}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [UIManager] Error in OnHideComplete for {panel.panelID}: {e.Message}");
        }
    }
    
    private IEnumerator AnimateCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("❌ [UIManager] CanvasGroup is null in animation");
            yield break;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float curveValue = transitionCurve.Evaluate(t);
            
            canvasGroup.alpha = Mathf.Lerp(from, to, curveValue);
            yield return null;
        }
        
        canvasGroup.alpha = to;
    }
    
    private IEnumerator AnimateScale(Transform target, Vector3 from, Vector3 to, float duration)
    {
        if (target == null)
        {
            Debug.LogError("❌ [UIManager] Transform is null in scale animation");
            yield break;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float curveValue = transitionCurve.Evaluate(t);
            
            target.localScale = Vector3.Lerp(from, to, curveValue);
            yield return null;
        }
        
        target.localScale = to;
    }
    
    #endregion

    #region Audio & Effects
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && UIValidation.ValidateManager(AudioManager.Instance, "AudioManager"))
        {
            AudioManager.Instance.PlayUISFX(clip);
        }
    }
    
    private void PlayUISound(AudioClip clip)
    {
        PlaySound(clip);
    }
    
    public void PlayClickSound()
    {
        PlaySound(clickSound);
        LogDebug("Click sound played");
    }
    
    #endregion

    #region Utility Methods
    
    public UIPanel GetPanel(string panelID)
    {
        panelDictionary.TryGetValue(panelID, out UIPanel panel);
        return panel;
    }
    
    public T GetPanel<T>(string panelID) where T : UIPanel
    {
        if (panelDictionary.TryGetValue(panelID, out UIPanel panel))
        {
            return panel as T;
        }
        return null;
    }
    
    public void RegisterPanel(UIPanel panel)
    {
        if (panel != null && UIValidation.ValidateString(panel.panelID, "Panel ID"))
        {
            panelDictionary[panel.panelID] = panel;
            panel.Initialize();
            LogDebug($"Panel registered dynamically: {panel.panelID}");
        }
    }
    
    public void UnregisterPanel(string panelID)
    {
        if (panelDictionary.ContainsKey(panelID))
        {
            panelDictionary.Remove(panelID);
            LogDebug($"Panel unregistered: {panelID}");
        }
    }
    
    public int GetPanelCount() => panelDictionary.Count;
    public bool IsInitialized => isInitialized;
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"🎮 [UIManager] {message}");
    }
    
    #endregion

    #region Input Handling
    
    void Update()
    {
        if (isInitialized)
            HandleInput();
    }
    
    private void HandleInput()
    {
        // ESC para volver atrás
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBack();
        }
    }
    
    #endregion
}
