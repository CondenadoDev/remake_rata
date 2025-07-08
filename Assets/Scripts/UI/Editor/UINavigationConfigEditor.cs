using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(UINavigationConfig))]
public class UINavigationConfigEditor : Editor
{
    private bool showPanelConfigs = true;
    private Dictionary<int, bool> panelFoldouts = new Dictionary<int, bool>();
    private Dictionary<string, bool> linkFoldouts = new Dictionary<string, bool>();
    
    public override void OnInspectorGUI()
    {
        UINavigationConfig config = (UINavigationConfig)target;
        
        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🎮 UI NAVIGATION CONFIGURATOR", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Quick Actions
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("🔍 Auto-Detect Panels", GUILayout.Height(30)))
        {
            config.AutoDetectPanels();
            EditorUtility.SetDirty(config);
        }
        
        if (GUILayout.Button("✓ Validate Config", GUILayout.Height(30)))
        {
            config.ValidateConfiguration();
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Basic Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        
        SerializedProperty uiManagerProp = serializedObject.FindProperty("uiManager");
        EditorGUILayout.PropertyField(uiManagerProp);
        
        SerializedProperty autoFindProp = serializedObject.FindProperty("autoFindUIManager");
        EditorGUILayout.PropertyField(autoFindProp);
        
        SerializedProperty keyboardProp = serializedObject.FindProperty("enableKeyboardShortcuts");
        EditorGUILayout.PropertyField(keyboardProp);
        
        SerializedProperty gamepadProp = serializedObject.FindProperty("enableGamepadSupport");
        EditorGUILayout.PropertyField(gamepadProp);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Global Shortcuts
        if (config.enableKeyboardShortcuts)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⌨️ Global Shortcuts", EditorStyles.boldLabel);
            
            SerializedProperty pauseKeyProp = serializedObject.FindProperty("pauseKey");
            EditorGUILayout.PropertyField(pauseKeyProp);
            
            SerializedProperty inventoryKeyProp = serializedObject.FindProperty("inventoryKey");
            EditorGUILayout.PropertyField(inventoryKeyProp);
            
            SerializedProperty quickSaveProp = serializedObject.FindProperty("quickSaveKey");
            EditorGUILayout.PropertyField(quickSaveProp);
            
            SerializedProperty quickLoadProp = serializedObject.FindProperty("quickLoadKey");
            EditorGUILayout.PropertyField(quickLoadProp);
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space();
        
        // Panel Configurations
        showPanelConfigs = EditorGUILayout.Foldout(showPanelConfigs, "📋 Panel Configurations", true);
        if (showPanelConfigs)
        {
            SerializedProperty panelConfigsProp = serializedObject.FindProperty("panelConfigs");
            
            for (int i = 0; i < panelConfigsProp.arraySize; i++)
            {
                SerializedProperty configProp = panelConfigsProp.GetArrayElementAtIndex(i);
                DrawPanelConfig(configProp, i);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("➕ Add Panel Config"))
            {
                panelConfigsProp.InsertArrayElementAtIndex(panelConfigsProp.arraySize);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space();
        
        // Debug Options
        SerializedProperty debugProp = serializedObject.FindProperty("showDebugUI");
        EditorGUILayout.PropertyField(debugProp);
        
        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawPanelConfig(SerializedProperty configProp, int index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        SerializedProperty panelIDProp = configProp.FindPropertyRelative("panelID");
        SerializedProperty panelObjectProp = configProp.FindPropertyRelative("panelObject");
        
        // Panel header
        EditorGUILayout.BeginHorizontal();
        
        if (!panelFoldouts.ContainsKey(index))
            panelFoldouts[index] = false;
            
        string panelName = string.IsNullOrEmpty(panelIDProp.stringValue) ? 
            $"Panel {index}" : panelIDProp.stringValue;
            
        panelFoldouts[index] = EditorGUILayout.Foldout(panelFoldouts[index], panelName, true);
        
        if (GUILayout.Button("❌", GUILayout.Width(25)))
        {
            SerializedProperty panelConfigsProp = serializedObject.FindProperty("panelConfigs");
            panelConfigsProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (panelFoldouts[index])
        {
            EditorGUI.indentLevel++;
            
            // Basic properties
            EditorGUILayout.PropertyField(panelIDProp);
            EditorGUILayout.PropertyField(panelObjectProp);
            
            SerializedProperty isMainProp = configProp.FindPropertyRelative("isMainPanel");
            EditorGUILayout.PropertyField(isMainProp);
            
            SerializedProperty transitionProp = configProp.FindPropertyRelative("transitionDuration");
            EditorGUILayout.PropertyField(transitionProp);
            
            SerializedProperty useAnimProp = configProp.FindPropertyRelative("useAnimation");
            EditorGUILayout.PropertyField(useAnimProp);
            
            // Events
            EditorGUILayout.Space();
            SerializedProperty onOpenProp = configProp.FindPropertyRelative("onPanelOpen");
            EditorGUILayout.PropertyField(onOpenProp);
            
            SerializedProperty onCloseProp = configProp.FindPropertyRelative("onPanelClose");
            EditorGUILayout.PropertyField(onCloseProp);
            
            // Navigation Links
            EditorGUILayout.Space();
            SerializedProperty linksProp = configProp.FindPropertyRelative("navigationLinks");
            
            string linkKey = $"links_{index}";
            if (!linkFoldouts.ContainsKey(linkKey))
                linkFoldouts[linkKey] = true;
                
            linkFoldouts[linkKey] = EditorGUILayout.Foldout(linkFoldouts[linkKey], 
                $"Navigation Links ({linksProp.arraySize})", true);
                
            if (linkFoldouts[linkKey])
            {
                EditorGUI.indentLevel++;
                
                for (int j = 0; j < linksProp.arraySize; j++)
                {
                    DrawNavigationLink(linksProp.GetArrayElementAtIndex(j), j, linksProp);
                }
                
                if (GUILayout.Button("➕ Add Link"))
                {
                    linksProp.InsertArrayElementAtIndex(linksProp.arraySize);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawNavigationLink(SerializedProperty linkProp, int index, SerializedProperty parentArray)
    {
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        SerializedProperty nameProp = linkProp.FindPropertyRelative("linkName");
        EditorGUILayout.PropertyField(nameProp, GUIContent.none);
        
        if (GUILayout.Button("❌", GUILayout.Width(25)))
        {
            parentArray.DeleteArrayElementAtIndex(index);
            return;
        }
        EditorGUILayout.EndHorizontal();
        
        SerializedProperty toPanelProp = linkProp.FindPropertyRelative("toPanel");
        EditorGUILayout.PropertyField(toPanelProp);
        
        SerializedProperty buttonProp = linkProp.FindPropertyRelative("triggerButton");
        EditorGUILayout.PropertyField(buttonProp);
        
        SerializedProperty shortcutProp = linkProp.FindPropertyRelative("shortcutKey");
        EditorGUILayout.PropertyField(shortcutProp);
        
        SerializedProperty historyProp = linkProp.FindPropertyRelative("addToHistory");
        EditorGUILayout.PropertyField(historyProp);
        
        SerializedProperty onNavigateProp = linkProp.FindPropertyRelative("onNavigate");
        EditorGUILayout.PropertyField(onNavigateProp);
        
        EditorGUILayout.EndVertical();
    }
}
#endif