using UISystem.Configuration;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PanelConfig))]
public class PanelConfigDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
            
        var panelId = property.FindPropertyRelative("panelId");
        var displayName = property.FindPropertyRelative("displayName");
            
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
            
        var idRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
        var nameRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height);
            
        EditorGUI.PropertyField(idRect, panelId, GUIContent.none);
        EditorGUI.PropertyField(nameRect, displayName, GUIContent.none);
            
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}