using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Maneja las transiciones visuales entre paneles UI
/// </summary>
public enum UITransitionType
{
    None,
    Fade,
    SlideLeft,
    SlideRight,
    SlideUp,
    SlideDown,
    Scale,
    ScaleAndFade,
    Rotate,
    Custom
}

[System.Serializable]
public class UITransitionSettings
{
    public UITransitionType transitionType = UITransitionType.Fade;
    public float duration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useUnscaledTime = true;
    
    [Header("Slide Settings")]
    public float slideDistance = 1000f;
    
    [Header("Scale Settings")]
    public Vector3 scaleFrom = Vector3.zero;
    public Vector3 scaleTo = Vector3.one;
    
    [Header("Rotate Settings")]
    public Vector3 rotationAmount = new Vector3(0, 0, 360);
    
    [Header("Advanced")]
    public bool fadeBackground = true;
    public Color backgroundTint = new Color(0, 0, 0, 0.5f);
}

public class UITransitionManager : MonoBehaviour
{
    [Header("🎨 Transition Manager")]
    [SerializeField]
    public bool enableTransitions = true;
    [SerializeField] private UITransitionSettings defaultTransition = new UITransitionSettings();
    
    [Header("📋 Panel-Specific Transitions")]
    [SerializeField] private List<PanelTransitionOverride> transitionOverrides = new List<PanelTransitionOverride>();
    
    [Header("🎯 Effects")]
    [SerializeField]
    public bool enableBlurEffect = false;
    [SerializeField] private float maxBlurAmount = 5f;
    [SerializeField] public bool enableParticleEffects = false;
    [SerializeField] private GameObject transitionParticlePrefab;
    
    [Header("🔊 Audio")]
    [SerializeField] private AudioClip defaultOpenSound;
    [SerializeField] private AudioClip defaultCloseSound;
    [SerializeField] private AudioClip whooshSound;
    
    // Referencias
    private UIManager uiManager;
    private Dictionary<string, UITransitionSettings> transitionDict = new Dictionary<string, UITransitionSettings>();
    private List<Coroutine> activeTransitions = new List<Coroutine>();
    private GameObject backgroundOverlay;
    
    [System.Serializable]
    public class PanelTransitionOverride
    {
        public string panelID;
        public UITransitionSettings openTransition;
        public UITransitionSettings closeTransition;
        public AudioClip customOpenSound;
        public AudioClip customCloseSound;
    }
    
    void Awake()
    {
        uiManager = GetComponent<UIManager>();
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
        
        BuildTransitionDictionary();
        CreateBackgroundOverlay();
        
        // Suscribirse a eventos
        if (uiManager != null)
        {
            UIManager.OnPanelOpened += OnPanelOpened;
            UIManager.OnPanelClosed += OnPanelClosed;
            UIManager.OnPanelSwitched += OnPanelSwitched;
        }
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        UIManager.OnPanelOpened -= OnPanelOpened;
        UIManager.OnPanelClosed -= OnPanelClosed;
        UIManager.OnPanelSwitched -= OnPanelSwitched;
        
        // Limpiar transiciones activas
        foreach (var transition in activeTransitions)
        {
            if (transition != null)
                StopCoroutine(transition);
        }
    }
    
    void BuildTransitionDictionary()
    {
        transitionDict.Clear();
        foreach (var overrideSettings in transitionOverrides)
        {
            if (!string.IsNullOrEmpty(overrideSettings.panelID))
            {
                transitionDict[overrideSettings.panelID + "_open"] = overrideSettings.openTransition;
                transitionDict[overrideSettings.panelID + "_close"] = overrideSettings.closeTransition;
            }
        }
    }
    
    void CreateBackgroundOverlay()
    {
        backgroundOverlay = new GameObject("Background Overlay");
        backgroundOverlay.transform.SetParent(transform, false);
        
        RectTransform rect = backgroundOverlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image image = backgroundOverlay.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);
        image.raycastTarget = false;
        
        Canvas.ForceUpdateCanvases();
        backgroundOverlay.transform.SetAsFirstSibling();
        backgroundOverlay.SetActive(false);
    }
    
    // Event Handlers
    void OnPanelOpened(string panelID)
    {
        if (!enableTransitions) return;
        
        UIPanel panel = uiManager.GetPanel(panelID);
        if (panel != null)
        {
            UITransitionSettings settings = GetTransitionSettings(panelID, true);
            AudioClip sound = GetPanelSound(panelID, true);
            
            var transition = StartCoroutine(AnimatePanel(panel, settings, true, sound));
            activeTransitions.Add(transition);
        }
    }
    
    void OnPanelClosed(string panelID)
    {
        if (!enableTransitions) return;
        
        UIPanel panel = uiManager.GetPanel(panelID);
        if (panel != null)
        {
            UITransitionSettings settings = GetTransitionSettings(panelID, false);
            AudioClip sound = GetPanelSound(panelID, false);
            
            var transition = StartCoroutine(AnimatePanel(panel, settings, false, sound));
            activeTransitions.Add(transition);
        }
    }
    
    void OnPanelSwitched(UIPanel fromPanel, UIPanel toPanel)
    {
        if (!enableTransitions) return;
        
        // Efectos especiales para transiciones específicas
        if (fromPanel != null && toPanel != null)
        {
            // Por ejemplo, efecto especial al ir del menú principal al juego
            if (fromPanel.panelID == "MainMenu" && toPanel.panelID == "HUD")
            {
                StartCoroutine(SpecialGameStartTransition());
            }
        }
    }
    
    UITransitionSettings GetTransitionSettings(string panelID, bool isOpening)
    {
        string key = panelID + (isOpening ? "_open" : "_close");
        if (transitionDict.TryGetValue(key, out UITransitionSettings settings))
        {
            return settings;
        }
        return defaultTransition;
    }
    
    AudioClip GetPanelSound(string panelID, bool isOpening)
    {
        foreach (var overrideSettings in transitionOverrides)
        {
            if (overrideSettings.panelID == panelID)
            {
                AudioClip customSound = isOpening ? overrideSettings.customOpenSound : overrideSettings.customCloseSound;
                if (customSound != null)
                    return customSound;
            }
        }
        return isOpening ? defaultOpenSound : defaultCloseSound;
    }
    
    // Animaciones principales
    IEnumerator AnimatePanel(UIPanel panel, UITransitionSettings settings, bool isOpening, AudioClip sound)
    {
        if (panel == null) yield break;
        
        GameObject panelGO = panel.gameObject;
        RectTransform rectTransform = panelGO.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = panelGO.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = panelGO.AddComponent<CanvasGroup>();
        }
        
        // Reproducir sonido
        if (sound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISFX(sound);
        }
        
        // Estado inicial
        if (isOpening)
        {
            panelGO.SetActive(true);
            SetupInitialState(rectTransform, canvasGroup, settings, true);
        }
        
        // Background fade
        if (settings.fadeBackground && backgroundOverlay != null)
        {
            StartCoroutine(AnimateBackgroundOverlay(isOpening, settings.duration));
        }
        
        // Partículas
        if (enableParticleEffects && transitionParticlePrefab != null)
        {
            SpawnTransitionParticles(rectTransform.position);
        }
        
        // Animación principal
        float elapsed = 0;
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;
        Vector3 startRotation = rectTransform.localEulerAngles;
        float startAlpha = canvasGroup.alpha;
        
        Vector3 targetPos = GetTargetPosition(rectTransform, settings, isOpening);
        Vector3 targetScale = GetTargetScale(settings, isOpening);
        Vector3 targetRotation = GetTargetRotation(startRotation, settings, isOpening);
        float targetAlpha = isOpening ? 1f : 0f;
        
        while (elapsed < settings.duration)
        {
            elapsed += settings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = settings.animationCurve.Evaluate(elapsed / settings.duration);
            
            // Aplicar animaciones según el tipo
            switch (settings.transitionType)
            {
                case UITransitionType.Fade:
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                    break;
                    
                case UITransitionType.SlideLeft:
                case UITransitionType.SlideRight:
                case UITransitionType.SlideUp:
                case UITransitionType.SlideDown:
                    rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                    break;
                    
                case UITransitionType.Scale:
                    rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    break;
                    
                case UITransitionType.ScaleAndFade:
                    rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                    break;
                    
                case UITransitionType.Rotate:
                    rectTransform.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, t);
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                    break;
                    
                case UITransitionType.Custom:
                    // Combinación de todas las animaciones
                    rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
                    rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    rectTransform.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, t);
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                    break;
            }
            
            // Efecto de blur (si está habilitado)
            if (enableBlurEffect)
            {
                ApplyBlurEffect(t * maxBlurAmount);
            }
            
            yield return null;
        }
        
        // Estado final
        if (!isOpening)
        {
            panelGO.SetActive(false);
        }
        else
        {
            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localEulerAngles = Vector3.zero;
        }
        
        // Limpiar la lista de transiciones activas
        activeTransitions.RemoveAll(c => c == null);
    }
    
    void SetupInitialState(RectTransform rect, CanvasGroup canvasGroup, UITransitionSettings settings, bool isOpening)
    {
        switch (settings.transitionType)
        {
            case UITransitionType.Fade:
                canvasGroup.alpha = 0f;
                break;
                
            case UITransitionType.SlideLeft:
                rect.anchoredPosition = new Vector2(settings.slideDistance, 0);
                canvasGroup.alpha = 0f;
                break;
                
            case UITransitionType.SlideRight:
                rect.anchoredPosition = new Vector2(-settings.slideDistance, 0);
                canvasGroup.alpha = 0f;
                break;
                
            case UITransitionType.SlideUp:
                rect.anchoredPosition = new Vector2(0, -settings.slideDistance);
                canvasGroup.alpha = 0f;
                break;
                
            case UITransitionType.SlideDown:
                rect.anchoredPosition = new Vector2(0, settings.slideDistance);
                canvasGroup.alpha = 0f;
                break;
                
            case UITransitionType.Scale:
            case UITransitionType.ScaleAndFade:
                rect.localScale = settings.scaleFrom;
                if (settings.transitionType == UITransitionType.ScaleAndFade)
                    canvasGroup.alpha = 0f;
                break;
                
            case UITransitionType.Rotate:
                rect.localEulerAngles = settings.rotationAmount;
                canvasGroup.alpha = 0f;
                break;
        }
    }
    
    Vector3 GetTargetPosition(RectTransform rect, UITransitionSettings settings, bool isOpening)
    {
        if (isOpening)
        {
            return Vector2.zero;
        }
        
        switch (settings.transitionType)
        {
            case UITransitionType.SlideLeft:
                return new Vector2(-settings.slideDistance, 0);
            case UITransitionType.SlideRight:
                return new Vector2(settings.slideDistance, 0);
            case UITransitionType.SlideUp:
                return new Vector2(0, settings.slideDistance);
            case UITransitionType.SlideDown:
                return new Vector2(0, -settings.slideDistance);
            default:
                return rect.anchoredPosition;
        }
    }
    
    Vector3 GetTargetScale(UITransitionSettings settings, bool isOpening)
    {
        if (settings.transitionType == UITransitionType.Scale || 
            settings.transitionType == UITransitionType.ScaleAndFade ||
            settings.transitionType == UITransitionType.Custom)
        {
            return isOpening ? settings.scaleTo : settings.scaleFrom;
        }
        return Vector3.one;
    }
    
    Vector3 GetTargetRotation(Vector3 currentRotation, UITransitionSettings settings, bool isOpening)
    {
        if (settings.transitionType == UITransitionType.Rotate || 
            settings.transitionType == UITransitionType.Custom)
        {
            return isOpening ? Vector3.zero : currentRotation + settings.rotationAmount;
        }
        return currentRotation;
    }
    
    IEnumerator AnimateBackgroundOverlay(bool fadeIn, float duration)
    {
        if (backgroundOverlay == null) yield break;
        
        backgroundOverlay.SetActive(true);
        Image bgImage = backgroundOverlay.GetComponent<Image>();
        
        float elapsed = 0;
        Color startColor = bgImage.color;
        Color targetColor = fadeIn ? defaultTransition.backgroundTint : new Color(0, 0, 0, 0);
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            bgImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        bgImage.color = targetColor;
        
        if (!fadeIn)
        {
            backgroundOverlay.SetActive(false);
        }
    }
    
    void SpawnTransitionParticles(Vector3 position)
    {
        if (transitionParticlePrefab != null)
        {
            GameObject particles = Instantiate(transitionParticlePrefab, position, Quaternion.identity);
            Destroy(particles, 3f);
        }
    }
    
    void ApplyBlurEffect(float blurAmount)
    {
        // Aquí iría la lógica para aplicar blur
        // Esto requeriría un shader de post-procesamiento
        // Por ahora es un placeholder
    }
    
    // Transiciones especiales
    IEnumerator SpecialGameStartTransition()
    {
        Debug.Log("🎮 Starting special game transition!");
        
        // Efecto de "zoom in" dramático
        if (whooshSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISFX(whooshSound);
        }
        
        // Aquí podrías agregar efectos más elaborados
        yield return new WaitForSeconds(0.5f);
    }
    
    // Métodos públicos para control manual
    public void PlayTransition(string panelID, UITransitionType transitionType, float duration = 0.3f)
    {
        UIPanel panel = uiManager?.GetPanel(panelID);
        if (panel != null)
        {
            UITransitionSettings customSettings = new UITransitionSettings
            {
                transitionType = transitionType,
                duration = duration
            };
            
            StartCoroutine(AnimatePanel(panel, customSettings, true, null));
        }
    }
    
    public void StopAllTransitions()
    {
        foreach (var transition in activeTransitions)
        {
            if (transition != null)
                StopCoroutine(transition);
        }
        activeTransitions.Clear();
    }
    
    // Configuración en runtime
    public void SetDefaultTransition(UITransitionType type, float duration)
    {
        defaultTransition.transitionType = type;
        defaultTransition.duration = duration;
    }
    
    public void AddPanelTransition(string panelID, UITransitionSettings openSettings, UITransitionSettings closeSettings)
    {
        var existing = transitionOverrides.Find(x => x.panelID == panelID);
        if (existing != null)
        {
            existing.openTransition = openSettings;
            existing.closeTransition = closeSettings;
        }
        else
        {
            transitionOverrides.Add(new PanelTransitionOverride
            {
                panelID = panelID,
                openTransition = openSettings,
                closeTransition = closeSettings
            });
        }
        
        BuildTransitionDictionary();
    }
    
    // Debug
    [ContextMenu("Test All Transitions")]
    public void TestAllTransitions()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Transitions can only be tested in Play mode");
            return;
        }
        
        StartCoroutine(TestTransitionSequence());
    }
    
    IEnumerator TestTransitionSequence()
    {
        UITransitionType[] types = (UITransitionType[])System.Enum.GetValues(typeof(UITransitionType));
        
        foreach (var type in types)
        {
            if (type == UITransitionType.None || type == UITransitionType.Custom) continue;
            
            Debug.Log($"Testing transition: {type}");
            
            // Crear un panel de prueba
            GameObject testPanel = new GameObject($"Test Panel {type}");
            testPanel.transform.SetParent(transform, false);
            
            RectTransform rect = testPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.25f, 0.25f);
            rect.anchorMax = new Vector2(0.75f, 0.75f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image image = testPanel.AddComponent<Image>();
            image.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
            
            TextMeshProUGUI text = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(testPanel.transform, false);
            text.text = type.ToString();
            text.fontSize = 48;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Crear un panel temporal
            ConcreteUIPanel panel = testPanel.AddComponent<ConcreteUIPanel>();
            panel.panelID = $"Test_{type}";
            
            // Configurar transición
            UITransitionSettings settings = new UITransitionSettings
            {
                transitionType = type,
                duration = 0.5f
            };
            
            // Animar entrada
            yield return StartCoroutine(AnimatePanel(panel, settings, true, null));
            yield return new WaitForSeconds(1f);
            
            // Animar salida
            yield return StartCoroutine(AnimatePanel(panel, settings, false, null));
            
            // Limpiar
            Destroy(testPanel);
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("✅ Transition test complete!");
    }
}
