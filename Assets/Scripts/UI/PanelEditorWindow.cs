using System.Collections.Generic;
using UISystem.Configuration;
using UnityEditor;
using UnityEngine;

public class PanelEditorWindow : EditorWindow
    {
        private PanelConfig currentPanel;
        private Vector2 scrollPosition;
        private ElementConfig selectedElement;
        
        public static void ShowWindow(PanelConfig panel)
        {
            var window = GetWindow<PanelEditorWindow>("Panel Editor");
            window.currentPanel = panel;
            window.minSize = new Vector2(400, 500);
        }
        
        private void OnGUI()
        {
            if (currentPanel == null)
            {
                EditorGUILayout.HelpBox("No panel selected", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"Editing: {currentPanel.displayName}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Panel Properties
            DrawPanelProperties();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Elements", EditorStyles.boldLabel);
            
            // Element List
            DrawElementList();
            
            // Add Element Button
            if (GUILayout.Button("Add Element"))
            {
                AddElement();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawPanelProperties()
        {
            currentPanel.panelId = EditorGUILayout.TextField("Panel ID", currentPanel.panelId);
            currentPanel.displayName = EditorGUILayout.TextField("Display Name", currentPanel.displayName);
            currentPanel.panelType = (PanelType)EditorGUILayout.EnumPopup("Type", currentPanel.panelType);
            currentPanel.startHidden = EditorGUILayout.Toggle("Start Hidden", currentPanel.startHidden);
            currentPanel.showAnimation = (PanelAnimation)EditorGUILayout.EnumPopup("Show Animation", currentPanel.showAnimation);
            currentPanel.hideAnimation = (PanelAnimation)EditorGUILayout.EnumPopup("Hide Animation", currentPanel.hideAnimation);
        }
        
        private void DrawElementList()
        {
            for (int i = 0; i < currentPanel.elements.Count; i++)
            {
                var element = currentPanel.elements[i];
                
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.BeginHorizontal();
                
                bool isSelected = selectedElement == element;
                if (GUILayout.Toggle(isSelected, element.elementId, "Button"))
                {
                    selectedElement = element;
                }
                
                if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0)
                {
                    currentPanel.elements.RemoveAt(i);
                    currentPanel.elements.Insert(i - 1, element);
                    GUI.changed = true;
                }
                
                if (GUILayout.Button("↓", GUILayout.Width(25)) && i < currentPanel.elements.Count - 1)
                {
                    currentPanel.elements.RemoveAt(i);
                    currentPanel.elements.Insert(i + 1, element);
                    GUI.changed = true;
                }
                
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    currentPanel.elements.RemoveAt(i);
                    if (selectedElement == element) selectedElement = null;
                    GUI.changed = true;
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (isSelected)
                {
                    DrawElementProperties(element);
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawElementProperties(ElementConfig element)
        {
            EditorGUI.indentLevel++;
            
            element.elementId = EditorGUILayout.TextField("ID", element.elementId);
            element.displayText = EditorGUILayout.TextField("Display Text", element.displayText);
            element.elementType = (ElementType)EditorGUILayout.EnumPopup("Type", element.elementType);
            
            // Binding
            EditorGUILayout.LabelField("Data Binding", EditorStyles.miniBoldLabel);
            element.bindingPath = EditorGUILayout.TextField("Binding Path", element.bindingPath);
            element.bindingMode = (BindingMode)EditorGUILayout.EnumPopup("Binding Mode", element.bindingMode);
            
            // Type-specific properties
            switch (element.elementType)
            {
                case ElementType.Button:
                    element.actionTarget = EditorGUILayout.TextField("Target Panel", element.actionTarget);
                    break;
                    
                case ElementType.Slider:
                    element.minValue = EditorGUILayout.FloatField("Min Value", element.minValue);
                    element.maxValue = EditorGUILayout.FloatField("Max Value", element.maxValue);
                    element.wholeNumbers = EditorGUILayout.Toggle("Whole Numbers", element.wholeNumbers);
                    break;
                    
                case ElementType.Dropdown:
                    EditorGUILayout.LabelField("Options:");
                    if (element.options == null) element.options = new List<string>();
                    
                    for (int i = 0; i < element.options.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        element.options[i] = EditorGUILayout.TextField(element.options[i]);
                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            element.options.RemoveAt(i);
                            break;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    if (GUILayout.Button("Add Option"))
                    {
                        element.options.Add($"Option {element.options.Count}");
                    }
                    break;
                    
                case ElementType.InputField:
                    element.placeholder = EditorGUILayout.TextField("Placeholder", element.placeholder);
                    break;
            }
            
            EditorGUI.indentLevel--;
        }
        
        private void AddElement()
        {
            var newElement = new ElementConfig
            {
                elementId = $"Element_{currentPanel.elements.Count}",
                displayText = "New Element",
                elementType = ElementType.Button
            };
            
            currentPanel.elements.Add(newElement);
            selectedElement = newElement;
        }
    }
    