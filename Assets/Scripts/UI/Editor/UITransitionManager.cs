#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UITransitionManager))]
public class UITransitionManagerEditor : Editor
{
    private bool showDefaultSettings = true;
    private bool showOverrides = true;
    private bool showEffects = false;
    private bool showAudio = false;
    
    public override void OnInspectorGUI()
    {
        UITransitionManager manager = (UITransitionManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🎨 UI TRANSITION MANAGER", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Quick Actions
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Test Transitions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Test All Transition Types", GUILayout.Height(30)))
            {
                manager.TestAllTransitions();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        // Enable Transitions
        SerializedProperty enableProp = serializedObject.FindProperty("enableTransitions");
        EditorGUILayout.PropertyField(enableProp);
        
        if (!manager.enableTransitions)
        {
            EditorGUILayout.HelpBox("Transitions are disabled. Panels will show/hide instantly.", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Default Settings
        showDefaultSettings = EditorGUILayout.Foldout(showDefaultSettings, "Default Transition Settings", true);
        if (showDefaultSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SerializedProperty defaultProp = serializedObject.FindProperty("defaultTransition");
            DrawTransitionSettings(defaultProp);
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space();
        
        // Panel Overrides
        showOverrides = EditorGUILayout.Foldout(showOverrides, "Panel-Specific Transitions", true);
        if (showOverrides)
        {
            SerializedProperty overridesProp = serializedObject.FindProperty("transitionOverrides");
            
            for (int i = 0; i < overridesProp.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                SerializedProperty overrideProp = overridesProp.GetArrayElementAtIndex(i);
                
                EditorGUILayout.BeginHorizontal();
                SerializedProperty panelIDProp = overrideProp.FindPropertyRelative("panelID");
                EditorGUILayout.PropertyField(panelIDProp, GUIContent.none);
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    overridesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Open Transition", EditorStyles.boldLabel);
                SerializedProperty openProp = overrideProp.FindPropertyRelative("openTransition");
                DrawTransitionSettings(openProp);
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Close Transition", EditorStyles.boldLabel);
                SerializedProperty closeProp = overrideProp.FindPropertyRelative("closeTransition");
                DrawTransitionSettings(closeProp);
                
                EditorGUILayout.Space();
                
                SerializedProperty openSoundProp = overrideProp.FindPropertyRelative("customOpenSound");
                EditorGUILayout.PropertyField(openSoundProp);
                
                SerializedProperty closeSoundProp = overrideProp.FindPropertyRelative("customCloseSound");
                EditorGUILayout.PropertyField(closeSoundProp);
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            
            if (GUILayout.Button("Add Panel Override"))
            {
                overridesProp.InsertArrayElementAtIndex(overridesProp.arraySize);
            }
        }
        
        EditorGUILayout.Space();
        
        // Effects
        showEffects = EditorGUILayout.Foldout(showEffects, "Visual Effects", true);
        if (showEffects)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            SerializedProperty blurProp = serializedObject.FindProperty("enableBlurEffect");
            EditorGUILayout.PropertyField(blurProp);
            
            if (manager.enableBlurEffect)
            {
                SerializedProperty blurAmountProp = serializedObject.FindProperty("maxBlurAmount");
                EditorGUILayout.PropertyField(blurAmountProp);
            }
            
            SerializedProperty particlesProp = serializedObject.FindProperty("enableParticleEffects");
            EditorGUILayout.PropertyField(particlesProp);
            
            if (manager.enableParticleEffects)
            {
                SerializedProperty particlePrefabProp = serializedObject.FindProperty("transitionParticlePrefab");
                EditorGUILayout.PropertyField(particlePrefabProp);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space();
        
        // Audio
        showAudio = EditorGUILayout.Foldout(showAudio, "Audio Settings", true);
        if (showAudio)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            SerializedProperty openSoundProp = serializedObject.FindProperty("defaultOpenSound");
            EditorGUILayout.PropertyField(openSoundProp);
            
            SerializedProperty closeSoundProp = serializedObject.FindProperty("defaultCloseSound");
            EditorGUILayout.PropertyField(closeSoundProp);
            
            SerializedProperty whooshProp = serializedObject.FindProperty("whooshSound");
            EditorGUILayout.PropertyField(whooshProp);
            
            EditorGUILayout.EndVertical();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawTransitionSettings(SerializedProperty settingsProp)
    {
        SerializedProperty typeProp = settingsProp.FindPropertyRelative("transitionType");
        EditorGUILayout.PropertyField(typeProp);
        
        SerializedProperty durationProp = settingsProp.FindPropertyRelative("duration");
        EditorGUILayout.PropertyField(durationProp);
        
        SerializedProperty curveProp = settingsProp.FindPropertyRelative("animationCurve");
        EditorGUILayout.PropertyField(curveProp);
        
        SerializedProperty unscaledProp = settingsProp.FindPropertyRelative("useUnscaledTime");
        EditorGUILayout.PropertyField(unscaledProp);
        
        UITransitionType type = (UITransitionType)typeProp.enumValueIndex;
        
        // Mostrar propiedades específicas según el tipo
        switch (type)
        {
            case UITransitionType.SlideLeft:
            case UITransitionType.SlideRight:
            case UITransitionType.SlideUp:
            case UITransitionType.SlideDown:
                SerializedProperty slideProp = settingsProp.FindPropertyRelative("slideDistance");
                EditorGUILayout.PropertyField(slideProp);
                break;
                
            case UITransitionType.Scale:
            case UITransitionType.ScaleAndFade:
                SerializedProperty scaleFromProp = settingsProp.FindPropertyRelative("scaleFrom");
                EditorGUILayout.PropertyField(scaleFromProp);
                SerializedProperty scaleToProp = settingsProp.FindPropertyRelative("scaleTo");
                EditorGUILayout.PropertyField(scaleToProp);
                break;
                
            case UITransitionType.Rotate:
                SerializedProperty rotateProp = settingsProp.FindPropertyRelative("rotationAmount");
                EditorGUILayout.PropertyField(rotateProp);
                break;
        }
        
        if (type != UITransitionType.None)
        {
            SerializedProperty fadeBgProp = settingsProp.FindPropertyRelative("fadeBackground");
            EditorGUILayout.PropertyField(fadeBgProp);
            
            if (settingsProp.FindPropertyRelative("fadeBackground").boolValue)
            {
                SerializedProperty tintProp = settingsProp.FindPropertyRelative("backgroundTint");
                EditorGUILayout.PropertyField(tintProp);
            }
        }
    }
}
#endif