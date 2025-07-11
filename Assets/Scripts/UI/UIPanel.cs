
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIPanel : MonoBehaviour
{
    [Header("🎨 Panel Settings")]
    public string panelID;
    [SerializeField] public bool startVisible = false;
    [SerializeField] public bool useScaleAnimation = false;
    [SerializeField] public bool blockGameInput = true;
    [SerializeField] protected bool enableDebugLogs = true;
    
    [Header("🎯 Navigation")]
    [SerializeField]
    public string nextPanelID;
    [SerializeField] public string previousPanelID;
    
    // Componentes
    private CanvasGroup canvasGroup;
    public CanvasGroup CanvasGroup => canvasGroup;
    
    // Estado
    private bool isVisible;
    private bool isInitialized;
    public bool IsVisible => isVisible;
    public bool UseScaleAnimation => useScaleAnimation;
    public bool IsInitialized => isInitialized;
    
    // Eventos
    public System.Action OnPanelOpened;
    public System.Action OnPanelClosed;
    
    #region Initialization
    protected virtual void Awake()
    {
        if (!isInitialized)
            Initialize();
    }

    public virtual void Initialize()
    {
        try
        {
            LogDebug("Initializing...");

            if (string.IsNullOrEmpty(panelID))
            {
                Debug.LogWarning($"⚠️ [UIPanel] Panel ID is empty on {gameObject.name}, using GameObject name as fallback");
                panelID = gameObject.name;
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                LogDebug("Creating CanvasGroup component");
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Activar objeto ANTES de visibilidad
            gameObject.SetActive(true);

            // Inicializar campos internos y OnInitialize
            OnInitialize();
            isInitialized = true;

            // Aplicar visibilidad inicial correctamente
            SetVisible(startVisible);

            LogDebug($"Successfully initialized (ID: {panelID})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [UIPanel] Initialization failed for {gameObject.name}: {e.Message}");
            isInitialized = false;
        }
    }

    
    protected virtual void OnInitialize() { }
    
    #endregion

    #region Visibility Control
    
    public virtual void SetVisible(bool visible)
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"⚠️ [UIPanel] {panelID} not initialized, cannot set visibility");
            return;
        }
        
        if (isVisible == visible)
        {
            LogDebug($"Visibility already set to {visible}");
            return;
        }
        
        isVisible = visible;
        LogDebug($"Visibility changed to: {visible}");
        
        if (visible)
        {
            OnPanelOpened?.Invoke();
            OnShow();
        }
        else
        {
            OnPanelClosed?.Invoke();
            OnHide();
        }
    }
    
    protected virtual void OnShow() 
    {
        LogDebug("OnShow called");
    }
    
    protected virtual void OnHide() 
    {
        LogDebug("OnHide called");
    }
    
    public virtual void OnShowComplete() 
    {
        LogDebug("Show animation completed");
    }
    
    public virtual void OnHideComplete()
    {
        LogDebug("Hide animation completed");
    }

    #endregion

    #region Navigation

    /// <summary>
    /// Configura la navegación hacia el siguiente y anterior panel.
    /// </summary>
    /// <param name="nextID">ID del siguiente panel.</param>
    /// <param name="previousID">ID del panel previo.</param>
    public void SetNavigation(string nextID, string previousID)
    {
        nextPanelID = nextID;
        previousPanelID = previousID;
    }
    
    public virtual void GoToNextPanel()
    {
        if (!string.IsNullOrEmpty(nextPanelID))
        {
            LogDebug($"Navigating to next panel: {nextPanelID}");
            if (UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
            {
                UIManager.Instance.ShowPanel(nextPanelID);
            }
        }
        else
        {
            LogDebug("No next panel ID specified");
        }
    }
    
    public virtual void GoToPreviousPanel()
    {
        if (!string.IsNullOrEmpty(previousPanelID))
        {
            LogDebug($"Navigating to previous panel: {previousPanelID}");
            if (UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
            {
                UIManager.Instance.ShowPanel(previousPanelID);
            }
        }
        else
        {
            LogDebug("No previous panel ID specified, using GoBack");
            if (UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
            {
                UIManager.Instance.GoBack();
            }
        }
    }
    
    public virtual void ClosePanel()
    {
        LogDebug("Closing panel");
        if (UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
        {
            UIManager.Instance.HidePanel(panelID);
        }
    }
    
    #endregion
    
    #region Utility
    
    protected void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"🎨 [UIPanel:{panelID}] {message}");
    }
    
    protected void LogWarning(string message)
    {
        Debug.LogWarning($"⚠️ [UIPanel:{panelID}] {message}");
    }
    
    protected void LogError(string message)
    {
        Debug.LogError($"❌ [UIPanel:{panelID}] {message}");
    }
    
    #endregion
    
    public void ConfigureSettings(bool startVisible, bool useScaleAnim, bool blockInput)
    {
        this.startVisible = startVisible;
        this.useScaleAnimation = useScaleAnim;
        this.blockGameInput = blockInput;
    }

}
