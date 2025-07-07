using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel principal de opciones
/// </summary>
public class OptionsMainUIPanel : UIPanel
{
    protected override void OnInitialize()
    {
        panelID = "OptionsMain";
        LogDebug("Options Main UI Panel initialized");
        
        SetupCategoryButtons();
    }
    
    void SetupCategoryButtons()
    {
        // Auto-buscar y configurar botones
        Button audioBtn = transform.Find("Button_AUDIO")?.GetComponent<Button>();
        Button graphicsBtn = transform.Find("Button_GRAFICOS")?.GetComponent<Button>();
        Button controlsBtn = transform.Find("Button_CONTROLES")?.GetComponent<Button>();
        Button gameplayBtn = transform.Find("Button_GAMEPLAY")?.GetComponent<Button>();
        Button backBtn = transform.Find("Button_VOLVER")?.GetComponent<Button>();
        
        if (audioBtn != null)
            audioBtn.onClick.AddListener(() => UIManager.Instance?.ShowPanel("AudioOptions"));
        if (graphicsBtn != null)
            graphicsBtn.onClick.AddListener(() => UIManager.Instance?.ShowPanel("GraphicsOptions"));
        if (controlsBtn != null)
            controlsBtn.onClick.AddListener(() => UIManager.Instance?.ShowPanel("ControlsOptions"));
        if (gameplayBtn != null)
            gameplayBtn.onClick.AddListener(() => UIManager.Instance?.ShowPanel("GameplayOptions"));
        if (backBtn != null)
            backBtn.onClick.AddListener(() => UIManager.Instance?.ShowPanel("MainMenu"));
    }
}