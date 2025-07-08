#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UISystem.Configuration;

public static class StartMenuConfigCreator
{
    [MenuItem("Tools/UI/Create Start Menu Config")]
    public static void CreateStartMenuConfig()
    {
        const string path = "Assets/Resources/Configs/StartMenuUIConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<UIGenerationConfig>(path);
        if (config != null)
        {
            Debug.Log("StartMenuUIConfig already exists at " + path);
            Selection.activeObject = config;
            return;
        }

        config = ScriptableObject.CreateInstance<UIGenerationConfig>();
        config.theme = new UITheme();
        config.navigation = new NavigationConfig();
        config.panels = new List<PanelConfig>();

        // Main Menu Panel
        var mainMenu = new PanelConfig
        {
            panelId = "MainMenu",
            displayName = "Main Menu",
            panelType = PanelType.Menu,
            startHidden = false,
            elements = new List<ElementConfig>
            {
                new ElementConfig
                {
                    elementId = "Button_NewGame",
                    displayText = "New Game",
                    elementType = ElementType.Button,
                    actionTarget = "NewGame"
                },
                new ElementConfig
                {
                    elementId = "Button_LoadGame",
                    displayText = "Load Game",
                    elementType = ElementType.Button,
                    actionTarget = "LoadGame"
                },
                new ElementConfig
                {
                    elementId = "Button_Options",
                    displayText = "Options",
                    elementType = ElementType.Button,
                    actionTarget = "OptionsMain"
                },
                new ElementConfig
                {
                    elementId = "Button_Quit",
                    displayText = "Quit",
                    elementType = ElementType.Button,
                    actionTarget = "ExitConfirm"
                }
            }
        };
        config.panels.Add(mainMenu);

        // Options Main Panel
        var optionsMain = new PanelConfig
        {
            panelId = "OptionsMain",
            displayName = "Options",
            panelType = PanelType.Menu,
            elements = new List<ElementConfig>
            {
                new ElementConfig
                {
                    elementId = "Button_Audio",
                    displayText = "Audio",
                    elementType = ElementType.Button,
                    actionTarget = "AudioOptions"
                },
                new ElementConfig
                {
                    elementId = "Button_Graphics",
                    displayText = "Graphics",
                    elementType = ElementType.Button,
                    actionTarget = "GraphicsOptions"
                },
                new ElementConfig
                {
                    elementId = "Button_Controls",
                    displayText = "Controls",
                    elementType = ElementType.Button,
                    actionTarget = "ControlsOptions"
                },
                new ElementConfig
                {
                    elementId = "Button_Gameplay",
                    displayText = "Gameplay",
                    elementType = ElementType.Button,
                    actionTarget = "GameplayOptions"
                },
                new ElementConfig
                {
                    elementId = "Button_Back",
                    displayText = "Back",
                    elementType = ElementType.Button,
                    actionTarget = "MainMenu"
                }
            }
        };
        config.panels.Add(optionsMain);

        // Settings panels (empty, will use bindings)
        config.panels.Add(new PanelConfig
        {
            panelId = "AudioOptions",
            displayName = "Audio Options",
            panelType = PanelType.Settings
        });
        config.panels.Add(new PanelConfig
        {
            panelId = "GraphicsOptions",
            displayName = "Graphics Options",
            panelType = PanelType.Settings
        });
        config.panels.Add(new PanelConfig
        {
            panelId = "ControlsOptions",
            displayName = "Controls Options",
            panelType = PanelType.Settings
        });
        config.panels.Add(new PanelConfig
        {
            panelId = "GameplayOptions",
            displayName = "Gameplay Options",
            panelType = PanelType.Settings
        });

        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();
        Debug.Log("StartMenuUIConfig created at " + path);
        Selection.activeObject = config;
    }
}
#endif
