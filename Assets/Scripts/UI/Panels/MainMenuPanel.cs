using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel específico para el menú principal
/// </summary>
public class MainMenuUIPanel : UIPanel
{
    [Header("🎮 Main Menu Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button optionsButton;
    public Button exitButton;

    protected override void OnInitialize()
    {
        panelID = "MainMenu";
        LogDebug("Main Menu UI Panel initialized");
        
        // Auto-buscar botones si no están asignados
        AutoFindButtons();
        SetupButtonEvents();
    }
    
    void AutoFindButtons()
    {
        if (newGameButton == null)
            newGameButton = transform.Find("Button_NUEVO JUEGO")?.GetComponent<Button>();
        if (optionsButton == null)
            optionsButton = transform.Find("Button_OPCIONES")?.GetComponent<Button>();
        if (exitButton == null)
            exitButton = transform.Find("Button_SALIR")?.GetComponent<Button>();
    }
    
    void SetupButtonEvents()
    {
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(() => {
                if (UIValidation.ValidateManager(UIManager.Instance, "UIManager"))
                    UIManager.Instance.ShowPanel("OptionsMain");
            });
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() => {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }
    }
}