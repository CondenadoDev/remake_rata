using UISystem.Core;
using UnityEngine;

/// <summary>
/// Implementación concreta de UIPanel para uso con el generador
/// </summary>
public class ConcreteUIPanel BaseUIPanel
{
    [Header("🎯 Panel Configuration")]
    
    [Header("🔧 Custom Actions")]
    public UnityEngine.Events.UnityEvent onPanelShown;
    public UnityEngine.Events.UnityEvent onPanelHidden;
    
    protected override void OnInitialize()
    {
        // Configurar propiedades usando reflection para compatibilidad
        var startVisibleField = typeof(UIPanel).GetField("startVisible", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (startVisibleField != null)
        {
            startVisibleField.SetValue(this, startVisible);
        }
        
        LogDebug($"ConcreteUIPanel '{panelID}' initialized");
    }
    
    protected override void OnShow()
    {
        base.OnShow();
        onPanelShown?.Invoke();
        
        if (useScaleAnimation)
        {
            transform.localScale = Vector3.zero;
            StartCoroutine(AnimateScale(Vector3.one, 0.3f));
        }
    }
    
    protected override void OnHide()
    {
        base.OnHide();
        onPanelHidden?.Invoke();
        
        if (useScaleAnimation)
        {
            StartCoroutine(AnimateScale(Vector3.zero, 0.2f, () => {
                gameObject.SetActive(false);
            }));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    System.Collections.IEnumerator AnimateScale(Vector3 targetScale, float duration, System.Action onComplete = null)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0, 1, t); // Curva suave
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
        onComplete?.Invoke();
    }
    
    // Métodos públicos para fácil configuración desde el editor
    public void ShowPanel(string panelID)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanel(panelID);
        }
    }
    
    public void HideThisPanel()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HidePanel(panelID);
        }
    }
    
    public void GoBack()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.GoBack();
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayClickSound();
        }
    }
}
