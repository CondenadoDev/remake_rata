using UnityEngine;

public abstract class ConfigurationBase : ScriptableObject
{
    [Header("📝 Info")]
    [TextArea(3, 5)]
    public string description;
    
    [Header("🔧 Debug")]
    public bool enableDebugLogs = false;
    
    protected virtual void OnValidate()
    {
        ValidateValues();
    }

    public virtual void ValidateValues() { }
    
    protected void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[{GetType().Name}] {message}");
        }
    }
    
    public virtual void ResetToDefaults() { }
}