using UnityEngine;

public abstract class ConfigBase : ScriptableObject
{
    [Header("🔧 Configuration Base")]
    [SerializeField] protected bool debugLogging = false;
    
    /// <summary>
    /// Valida y corrige valores fuera de rango
    /// </summary>
    public virtual void ValidateValues()
    {
        // Implementar en clases derivadas
    }
    
    /// <summary>
    /// Aplica la configuración al sistema correspondiente
    /// </summary>
    public virtual void ApplySettings()
    {
        // Implementar en clases derivadas si es necesario
        DebugLog($"Settings applied for {GetType().Name}");
    }
    
    /// <summary>
    /// Resetea todos los valores a sus defaults
    /// </summary>
    public virtual void ResetToDefaults()
    {
        // Implementar en clases derivadas
        DebugLog($"Reset to defaults: {GetType().Name}");
    }
    
    /// <summary>
    /// Carga configuración desde PlayerPrefs
    /// </summary>
    public virtual void LoadFromPlayerPrefs()
    {
        var fields = GetType().GetFields();
        foreach (var field in fields)
        {
            string key = $"{GetType().Name}_{field.Name}";
            
            if (field.FieldType == typeof(float))
            {
                if (PlayerPrefs.HasKey(key))
                    field.SetValue(this, PlayerPrefs.GetFloat(key));
            }
            else if (field.FieldType == typeof(int))
            {
                if (PlayerPrefs.HasKey(key))
                    field.SetValue(this, PlayerPrefs.GetInt(key));
            }
            else if (field.FieldType == typeof(bool))
            {
                if (PlayerPrefs.HasKey(key))
                    field.SetValue(this, PlayerPrefs.GetInt(key) == 1);
            }
            else if (field.FieldType == typeof(string))
            {
                if (PlayerPrefs.HasKey(key))
                    field.SetValue(this, PlayerPrefs.GetString(key));
            }
        }
        
        ValidateValues();
        DebugLog($"Loaded from PlayerPrefs: {GetType().Name}");
    }
    
    /// <summary>
    /// Guarda configuración en PlayerPrefs
    /// </summary>
    public virtual void SaveToPlayerPrefs()
    {
        var fields = GetType().GetFields();
        foreach (var field in fields)
        {
            string key = $"{GetType().Name}_{field.Name}";
            object value = field.GetValue(this);
            
            if (field.FieldType == typeof(float))
            {
                PlayerPrefs.SetFloat(key, (float)value);
            }
            else if (field.FieldType == typeof(int))
            {
                PlayerPrefs.SetInt(key, (int)value);
            }
            else if (field.FieldType == typeof(bool))
            {
                PlayerPrefs.SetInt(key, (bool)value ? 1 : 0);
            }
            else if (field.FieldType == typeof(string))
            {
                PlayerPrefs.SetString(key, (string)value);
            }
        }
        
        PlayerPrefs.Save();
        DebugLog($"Saved to PlayerPrefs: {GetType().Name}");
    }
    
    protected void DebugLog(string message)
    {
        if (debugLogging)
        {
            Debug.Log($"[{GetType().Name}] {message}");
        }
    }
    
    protected void OnValidate()
    {
        ValidateValues();
    }
}