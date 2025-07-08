#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UISystem.Core;
using UISystem.Panels;
using UISystem.Configuration;

namespace UISystem.Editor
{
    // UISystemEditorWindow.cs
    public class UISystemEditorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private UIGenerationConfig selectedConfig;
        private SerializedObject serializedConfig;
        private bool showGenerationSettings = true;
        private bool showPanelList = true;
        private bool showNavigationGraph = false;
        private bool showThemeEditor = false;

        private GUIStyle headerStyle;
        private GUIStyle boxStyle;

        [MenuItem("Window/UI System/Control Panel")]
        public static void ShowWindow()
        {
            var window = GetWindow<UISystemEditorWindow>("UI System");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            LoadStyles();
        }

        private void LoadStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 10, 10)
            };
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // Toolbar
            DrawToolbar();

            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Configuration Selection
            DrawConfigSelection();

            if (selectedConfig != null)
            {
                // Generation Settings
                if (showGenerationSettings)
                {
                    DrawGenerationSettings();
                }

                // Panel List
                if (showPanelList)
                {
                    DrawPanelList();
                }

                // Navigation Graph
                if (showNavigationGraph)
                {
                    DrawNavigationGraph();
                }

                // Theme Editor
                if (showThemeEditor)
                {
                    DrawThemeEditor();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Generate UI", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                GenerateUI();
            }

            if (GUILayout.Button("Clear UI", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                ClearUI();
            }

            GUILayout.FlexibleSpace();

            showGenerationSettings = GUILayout.Toggle(showGenerationSettings, "Settings", EditorStyles.toolbarButton);
            showPanelList = GUILayout.Toggle(showPanelList, "Panels", EditorStyles.toolbarButton);
            showNavigationGraph = GUILayout.Toggle(showNavigationGraph, "Navigation", EditorStyles.toolbarButton);
            showThemeEditor = GUILayout.Toggle(showThemeEditor, "Theme", EditorStyles.toolbarButton);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawConfigSelection()
        {
            EditorGUILayout.LabelField("UI Configuration", headerStyle);

            EditorGUI.BeginChangeCheck();
            selectedConfig = (UIGenerationConfig)EditorGUILayout.ObjectField(
                "Config Asset",
                selectedConfig,
                typeof(UIGenerationConfig),
                false
            );

            if (EditorGUI.EndChangeCheck() && selectedConfig != null)
            {
                serializedConfig = new SerializedObject(selectedConfig);
            }

            if (GUILayout.Button("Create New Config"))
            {
                CreateNewConfig();
            }

            EditorGUILayout.Space();
        }

        private void DrawGenerationSettings()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Generation Settings", headerStyle);

            if (serializedConfig != null)
            {
                serializedConfig.Update();

                // Draw navigation settings
                var navProp = serializedConfig.FindProperty("navigation");
                EditorGUILayout.PropertyField(navProp, true);

                serializedConfig.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawPanelList()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Panels", headerStyle);

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                AddNewPanel();
            }

            EditorGUILayout.EndHorizontal();

            if (selectedConfig != null && selectedConfig.panels != null)
            {
                for (int i = 0; i < selectedConfig.panels.Count; i++)
                {
                    DrawPanelItem(selectedConfig.panels[i], i);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawPanelItem(PanelConfig panel, int index)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            panel.panelId = EditorGUILayout.TextField("ID", panel.panelId);

            if (GUILayout.Button("Edit", GUILayout.Width(50)))
            {
                PanelEditorWindow.ShowWindow(panel);
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                selectedConfig.panels.RemoveAt(index);
                EditorUtility.SetDirty(selectedConfig);
            }

            EditorGUILayout.EndHorizontal();

            panel.displayName = EditorGUILayout.TextField("Name", panel.displayName);
            panel.panelType = (PanelType)EditorGUILayout.EnumPopup("Type", panel.panelType);
            panel.startHidden = EditorGUILayout.Toggle("Start Hidden", panel.startHidden);

            EditorGUILayout.EndVertical();
        }

        private void DrawNavigationGraph()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Navigation Graph", headerStyle);

            // Visual representation of panel connections
            Rect graphRect = GUILayoutUtility.GetRect(position.width - 40, 300);
            GUI.Box(graphRect, GUIContent.none);

            if (selectedConfig != null && selectedConfig.panels != null)
            {
                DrawNavigationNodes(graphRect);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawNavigationNodes(Rect container)
        {
            int panelCount = selectedConfig.panels.Count;
            if (panelCount == 0) return;

            float nodeWidth = 100;
            float nodeHeight = 50;
            float padding = 20;

            // Simple grid layout
            int cols = Mathf.CeilToInt(Mathf.Sqrt(panelCount));
            int rows = Mathf.CeilToInt((float)panelCount / cols);

            for (int i = 0; i < panelCount; i++)
            {
                var panel = selectedConfig.panels[i];
                int col = i % cols;
                int row = i / cols;

                float x = container.x + padding + col * (nodeWidth + padding);
                float y = container.y + padding + row * (nodeHeight + padding);

                Rect nodeRect = new Rect(x, y, nodeWidth, nodeHeight);

                // Draw node
                GUI.Box(nodeRect, panel.displayName);

                // Draw connections (simplified)
                foreach (var element in panel.elements)
                {
                    if (element.elementType == ElementType.Button && !string.IsNullOrEmpty(element.actionTarget))
                    {
                        var targetPanel = selectedConfig.panels.FirstOrDefault(p => p.panelId == element.actionTarget);
                        if (targetPanel != null)
                        {
                            int targetIndex = selectedConfig.panels.IndexOf(targetPanel);
                            DrawConnection(nodeRect, i, targetIndex, container, nodeWidth, nodeHeight, padding, cols);
                        }
                    }
                }
            }
        }

        private void DrawConnection(Rect fromRect, int fromIndex, int toIndex, Rect container,
            float nodeWidth, float nodeHeight, float padding, int cols)
        {
            int toCol = toIndex % cols;
            int toRow = toIndex / cols;

            float toX = container.x + padding + toCol * (nodeWidth + padding) + nodeWidth / 2;
            float toY = container.y + padding + toRow * (nodeHeight + padding) + nodeHeight / 2;

            Vector2 from = new Vector2(fromRect.center.x, fromRect.center.y);
            Vector2 to = new Vector2(toX, toY);

            Handles.DrawBezier(from, to, from, to, Color.cyan, null, 2f);
        }

        private void DrawThemeEditor()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Theme Editor", headerStyle);

            if (selectedConfig.theme == null)
            {
                selectedConfig.theme = new UITheme();
            }

            selectedConfig.theme.primaryColor =
                EditorGUILayout.ColorField("Primary Color", selectedConfig.theme.primaryColor);
            selectedConfig.theme.secondaryColor =
                EditorGUILayout.ColorField("Secondary Color", selectedConfig.theme.secondaryColor);
            selectedConfig.theme.backgroundColor =
                EditorGUILayout.ColorField("Background Color", selectedConfig.theme.backgroundColor);
            selectedConfig.theme.textColor = EditorGUILayout.ColorField("Text Color", selectedConfig.theme.textColor);

            selectedConfig.theme.defaultFont = (Font)EditorGUILayout.ObjectField("Default Font",
                selectedConfig.theme.defaultFont, typeof(Font), false);
            selectedConfig.theme.defaultFontSize =
                EditorGUILayout.FloatField("Default Font Size", selectedConfig.theme.defaultFontSize);

            if (GUILayout.Button("Apply Theme"))
            {
                ApplyTheme();
            }

            EditorGUILayout.EndVertical();
        }

        private void GenerateUI()
        {
            if (selectedConfig == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a UI Configuration asset", "OK");
                return;
            }

            // Find or create UI Generator
            UIGenerator generator = FindObjectOfType<UIGenerator>();
            if (generator == null)
            {
                GameObject go = new GameObject("UI Generator");
                generator = go.AddComponent<UIGenerator>();
            }

            // Set config and generate
            SerializedObject genSO = new SerializedObject(generator);
            genSO.FindProperty("generationConfig").objectReferenceValue = selectedConfig;
            genSO.ApplyModifiedProperties();

            generator.GenerateUI();

            EditorUtility.DisplayDialog("Success", "UI generated successfully!", "OK");
        }

        private void ClearUI()
        {
            if (EditorUtility.DisplayDialog("Clear UI", "This will remove all generated UI. Continue?", "Yes", "No"))
            {
                UIGenerator generator = FindObjectOfType<UIGenerator>();
                if (generator != null)
                {
                    generator.ClearGeneratedUI();
                }
            }
        }

        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save UI Configuration",
                "UIConfig",
                "asset",
                "Save UI configuration asset"
            );

            if (!string.IsNullOrEmpty(path))
            {
                UIGenerationConfig newConfig = CreateInstance<UIGenerationConfig>();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();

                selectedConfig = newConfig;
                serializedConfig = new SerializedObject(selectedConfig);
            }
        }

        private void AddNewPanel()
        {
            var newPanel = new PanelConfig
            {
                panelId = $"Panel_{selectedConfig.panels.Count}",
                displayName = "New Panel",
                panelType = PanelType.Menu,
                elements = new List<ElementConfig>()
            };

            selectedConfig.panels.Add(newPanel);
            EditorUtility.SetDirty(selectedConfig);
        }

        private void ApplyTheme()
        {
            // Apply theme to existing UI elements
            var panels = FindObjectsOfType<BaseUIPanel>();
            foreach (var panel in panels)
            {
                ApplyThemeToPanel(panel.gameObject);
            }
        }

        private void ApplyThemeToPanel(GameObject panelObject)
        {
            // Apply to images
            var images = panelObject.GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (var img in images)
            {
                if (img.name.Contains("Background"))
                {
                    img.color = selectedConfig.theme.backgroundColor;
                }
                else
                {
                    img.color = selectedConfig.theme.primaryColor;
                }
            }

            // Apply to text
            var texts = panelObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (var text in texts)
            {
                text.color = selectedConfig.theme.textColor;
                if (selectedConfig.theme.defaultFont != null)
                {
                    // Note: Would need TMP font asset conversion
                }

                text.fontSize = selectedConfig.theme.defaultFontSize;
            }
        }
    }
}
#endif