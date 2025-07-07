using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIPanel : MonoBehaviour
{
    [Header("🎨 Panel Settings")]
    public string panelID;
    [SerializeField] protected bool startVisible = false;
    [SerializeField] protected bool useScaleAnimation = true;
    [SerializeField] protected bool blockGameInput = true;
    
    [Header("🎯 Navigation")]
    [SerializeField] protected string nextPanelID;
    [SerializeField] protected string previousPanelID;
    
    // Componentes
    private CanvasGroup canvasGroup;
    public CanvasGroup CanvasGroup => canvasGroup;
    
    // Estado
    private bool isVisible;
    public bool IsVisible => isVisible;
    public bool UseScaleAnimation => useScaleAnimation;
    
    // Eventos
    public System.Action OnPanelOpened;
    public System.Action OnPanelClosed;
    
    #region Initialization
    
    public virtual void Initialize()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Estado inicial
        SetVisible(startVisible);
        gameObject.SetActive(startVisible);
        
        // Configuración inicial
        if (!startVisible)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        
        OnInitialize();
    }
    
    protected virtual void OnInitialize() { }
    
    #endregion

    #region Visibility Control
    
    public virtual void SetVisible(bool visible)
    {
        isVisible = visible;
        
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
    
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
    
    public virtual void OnShowComplete() { }
    public virtual void OnHideComplete() { }
    
    #endregion

    #region Navigation
    
    public virtual void GoToNextPanel()
    {
        if (!string.IsNullOrEmpty(nextPanelID))
        {
            UIManager.Instance.ShowPanel(nextPanelID);
        }
    }
    
    public virtual void GoToPreviousPanel()
    {
        if (!string.IsNullOrEmpty(previousPanelID))
        {
            UIManager.Instance.ShowPanel(previousPanelID);
        }
        else
        {
            UIManager.Instance.GoBack();
        }
    }
    
    public virtual void ClosePanel()
    {
        UIManager.Instance.HidePanel(panelID);
    }
    
    #endregion
}