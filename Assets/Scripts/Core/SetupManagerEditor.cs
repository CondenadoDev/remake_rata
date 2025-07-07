using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SetupManager))]
public class SetupManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🛠️ Setup Tools", EditorStyles.boldLabel);
        
        SetupManager setupManager = (SetupManager)target;
        
        if (GUILayout.Button("🚀 Run Complete Setup"))
        {
            setupManager.RunCompleteSetup();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🔧 Individual Setup Steps", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔧 Setup Core Managers"))
        {
            setupManager.SetupCoreManagersOnly();
        }
        
        if (GUILayout.Button("🎨 Setup UI"))
        {
            setupManager.SetupUIOnly();
        }
        
        if (GUILayout.Button("🔄 Migrate Components"))
        {
            setupManager.MigrateComponentsOnly();
        }
        
        if (GUILayout.Button("⚙️ Create Configurations"))
        {
            setupManager.CreateConfigurationsOnly();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This tool will help you migrate from your current system to the new architecture. " +
                                "It will preserve existing functionality while adding new features.", MessageType.Info);
    }
}