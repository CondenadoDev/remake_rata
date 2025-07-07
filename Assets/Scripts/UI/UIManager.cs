// UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Collections; // Para corrutinas

public class UIManager : MonoBehaviour
{
    [Header("🎮 UI Panels")]
    [SerializeField] private UIPanel[] uiPanels;
    
    [Header("⚙️ Configuration")]
    [SerializeField] private float defaultTransitionDuration = 0.3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
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
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializePanels();
    }
    
    void InitializePanels()
    {
        foreach (var panel in uiPanels)
        {
            if (panel != null && !string.IsNullOrEmpty(panel.panelID))
            {
                panelDictionary[panel.panelID] = panel;
                panel.Initialize();
                
                // Suscribirse a eventos del panel
                panel.OnPanelOpened += () => OnPanelOpened?.Invoke(panel.panelID);
                panel.OnPanelClosed += () => OnPanelClosed?.Invoke(panel.panelID);
            }
        }
        
        Debug.Log($"🎮 UIManager initialized with {panelDictionary.Count} panels");
    }
    
    #endregion

    #region Panel Management
    
    public void ShowPanel(string panelID, bool addToHistory = true)
    {
        if (!panelDictionary.TryGetValue(panelID, out UIPanel panel))
        {
            Debug.LogError($"❌ Panel '{panelID}' not found!");
            return;
        }
        
        // Guardar panel anterior en historial
        if (addToHistory && currentActivePanel != null)
        {
            panelHistory.Push(currentActivePanel);
        }
        
        // Ocultar panel actual
        if (currentActivePanel != null && currentActivePanel != panel)
        {
            HidePanel(currentActivePanel.panelID, false);
        }
        
        // Mostrar nuevo panel
        StartCoroutine(ShowPanelCoroutine(panel));
        
        currentActivePanel = panel;
        OnPanelSwitched?.Invoke(currentActivePanel, panel);
        
        // Audio feedback
        PlaySound(openSound);
    }
    
    public void HidePanel(string panelID, bool returnToPrevious = true)
    {
        if (!panelDictionary.TryGetValue(panelID, out UIPanel panel))
        {
            Debug.LogError($"❌ Panel '{panelID}' not found!");
            return;
        }
        
        StartCoroutine(HidePanelCoroutine(panel));
        
        // Volver al panel anterior si existe
        if (returnToPrevious && panelHistory.Count > 0)
        {
            var previousPanel = panelHistory.Pop();
            ShowPanel(previousPanel.panelID, false);
        }
        else if (panel == currentActivePanel)
        {
            currentActivePanel = null;
        }
        
        // Audio feedback
        PlaySound(closeSound);
    }
    
    public void TogglePanel(string panelID)
    {
        if (!panelDictionary.TryGetValue(panelID, out UIPanel panel))
        {
            Debug.LogError($"❌ Panel '{panelID}' not found!");
            return;
        }
        
        if (panel.IsVisible)
            HidePanel(panelID);
        else
            ShowPanel(panelID);
    }
    
    public bool IsPanelVisible(string panelID)
    {
        if (panelDictionary.TryGetValue(panelID, out UIPanel panel))
            return panel.IsVisible;
        return false;
    }
    
    public void HideAllPanels()
    {
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
            HidePanel(currentActivePanel.panelID, true);
        }
    }
    
    #endregion

    #region Animation Coroutines
    
    private IEnumerator ShowPanelCoroutine(UIPanel panel)
    {
        panel.gameObject.SetActive(true);
        panel.SetVisible(true);
        
        // Animación de entrada
        if (panel.CanvasGroup != null)
        {
            panel.CanvasGroup.alpha = 0f;
            panel.CanvasGroup.blocksRaycasts = false;
            
            yield return AnimateCanvasGroup(panel.CanvasGroup, 0f, 1f, defaultTransitionDuration);
            
            panel.CanvasGroup.blocksRaycasts = true;
        }
        
        // Escala de entrada (opcional)
        if (panel.UseScaleAnimation)
        {
            panel.transform.localScale = Vector3.zero;
            yield return AnimateScale(panel.transform, Vector3.zero, Vector3.one, defaultTransitionDuration);
        }
        
        panel.OnShowComplete();
    }
    
    private IEnumerator HidePanelCoroutine(UIPanel panel)
    {
        if (panel.CanvasGroup != null)
        {
            panel.CanvasGroup.blocksRaycasts = false;
            yield return AnimateCanvasGroup(panel.CanvasGroup, 1f, 0f, defaultTransitionDuration);
        }
        
        // Escala de salida (opcional)
        if (panel.UseScaleAnimation)
        {
            yield return AnimateScale(panel.transform, Vector3.one, Vector3.zero, defaultTransitionDuration);
        }
        
        panel.SetVisible(false);
        panel.gameObject.SetActive(false);
        panel.OnHideComplete();
    }
    
    private IEnumerator AnimateCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
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
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
    }
    
    public void PlayClickSound()
    {
        PlaySound(clickSound);
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
        if (panel != null && !string.IsNullOrEmpty(panel.panelID))
        {
            panelDictionary[panel.panelID] = panel;
            panel.Initialize();
        }
    }
    
    public void UnregisterPanel(string panelID)
    {
        if (panelDictionary.ContainsKey(panelID))
        {
            panelDictionary.Remove(panelID);
        }
    }
    
    #endregion

    #region Input Handling
    
    void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // ESC para volver atrás
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBack();
        }
        
        // Otros inputs globales de UI aquí...
    }
    
    #endregion
}