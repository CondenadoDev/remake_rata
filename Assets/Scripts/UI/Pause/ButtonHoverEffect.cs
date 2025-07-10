using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("🎨 Button References")]
    public Button button;
    public Image buttonImage;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.cyan;
    
    [Header("🎯 Animation Settings")]
    public float scaleMultiplier = 1.05f;
    public float animationDuration = 0.1f;
    public bool useScaleAnimation = true;
    public bool useColorAnimation = true;
    
    private Vector3 originalScale;
    private bool isHovering = false;
    private Coroutine currentAnimation;
    
    void Start()
    {
        originalScale = transform.localScale;
        
        // Auto-detectar componentes si no están asignados
        if (button == null)
            button = GetComponent<Button>();
        
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        
        // Configurar colores iniciales
        if (buttonImage != null && useColorAnimation)
        {
            buttonImage.color = normalColor;
        }
    }
    
    public void SetupButton(Button btn, Image img, Color normal, Color hover)
    {
        button = btn;
        buttonImage = img;
        normalColor = normal;
        hoverColor = hover;
        
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enabled || isHovering) return;
        
        OnHoverEnter();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enabled || !isHovering) return;
        
        OnHoverExit();
    }
    
    void OnHoverEnter()
    {
        isHovering = true;
        
        Debug.Log($"🎯 Button hover enter: {gameObject.name}");
        
        // Detener animación actual
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        // Animar hacia estado hover
        currentAnimation = StartCoroutine(AnimateToHover());
    }
    
    void OnHoverExit()
    {
        isHovering = false;
        
        Debug.Log($"🎯 Button hover exit: {gameObject.name}");
        
        // Detener animación actual
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        // Animar hacia estado normal
        currentAnimation = StartCoroutine(AnimateToNormal());
    }
    
    IEnumerator AnimateToHover()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = useScaleAnimation ? originalScale * scaleMultiplier : originalScale;
        Color startColor = buttonImage != null ? buttonImage.color : Color.white;
        Color targetColor = useColorAnimation ? hoverColor : startColor;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            
            // Animación suave con easing
            float easedT = EaseOutQuad(t);
            
            // Escala
            if (useScaleAnimation)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
            }
            
            // Color
            if (useColorAnimation && buttonImage != null)
            {
                buttonImage.color = Color.Lerp(startColor, targetColor, easedT);
            }
            
            yield return null;
        }
        
        // Asegurar valores finales
        if (useScaleAnimation)
            transform.localScale = targetScale;
        
        if (useColorAnimation && buttonImage != null)
            buttonImage.color = targetColor;
    }
    
    IEnumerator AnimateToNormal()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale;
        Color startColor = buttonImage != null ? buttonImage.color : Color.white;
        Color targetColor = useColorAnimation ? normalColor : startColor;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            
            // Animación suave con easing
            float easedT = EaseOutQuad(t);
            
            // Escala
            if (useScaleAnimation)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
            }
            
            // Color
            if (useColorAnimation && buttonImage != null)
            {
                buttonImage.color = Color.Lerp(startColor, targetColor, easedT);
            }
            
            yield return null;
        }
        
        // Asegurar valores finales
        if (useScaleAnimation)
            transform.localScale = targetScale;
        
        if (useColorAnimation && buttonImage != null)
            buttonImage.color = targetColor;
    }
    
    // Función de easing para animaciones más suaves
    float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
    
    void OnDisable()
    {
        // Restaurar estado normal al desactivar
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        transform.localScale = originalScale;
        
        if (buttonImage != null && useColorAnimation)
        {
            buttonImage.color = normalColor;
        }
        
        isHovering = false;
    }
    
    // Método público para cambiar colores dinámicamente
    public void SetColors(Color normal, Color hover)
    {
        normalColor = normal;
        hoverColor = hover;
        
        if (!isHovering && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }
}