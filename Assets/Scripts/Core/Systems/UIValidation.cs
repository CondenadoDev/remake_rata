using UnityEngine;

/// <summary>
/// Utilidades para validación en el sistema UI
/// </summary>
public static class UIValidation
{
    public static bool ValidateManager<T>(T manager, string managerName) where T : class
    {
        if (manager == null)
        {
            Debug.LogWarning($"⚠️ [UIValidation] {managerName} is not available");
            return false;
        }
        return true;
    }
    
    public static bool ValidateComponent<T>(T component, string componentName, GameObject owner) where T : Component
    {
        if (component == null)
        {
            Debug.LogError($"❌ [UIValidation] {componentName} is missing on {owner.name}");
            return false;
        }
        return true;
    }
    
    public static bool ValidateString(string value, string fieldName)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError($"❌ [UIValidation] {fieldName} is null or empty");
            return false;
        }
        return true;
    }
}