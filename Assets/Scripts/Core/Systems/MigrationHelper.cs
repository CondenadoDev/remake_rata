using UnityEngine;

public static class MigrationHelper
{
    public static void LogMigrationStep(string step)
    {
        Debug.Log($"🔄 [MIGRATION] {step}");
    }
    
    public static void LogMigrationComplete(string component)
    {
        Debug.Log($"✅ [MIGRATION] {component} migration completed");
    }
    
    public static void LogMigrationWarning(string warning)
    {
        Debug.LogWarning($"⚠️ [MIGRATION] {warning}");
    }
    
    public static void LogMigrationError(string error)
    {
        Debug.LogError($"❌ [MIGRATION] {error}");
    }
    
    // Utilidad para transferir componentes preservando referencias
    public static T MigrateComponent<T>(GameObject target, Component oldComponent) where T : Component
    {
        if (target.GetComponent<T>() != null)
        {
            LogMigrationWarning($"{typeof(T).Name} already exists on {target.name}");
            return target.GetComponent<T>();
        }
        
        T newComponent = target.AddComponent<T>();
        
        // Deshabilitar el viejo componente en lugar de eliminarlo
        if (oldComponent != null)
        {
            MonoBehaviour oldMono = oldComponent as MonoBehaviour;
            if (oldMono != null)
                oldMono.enabled = false;
        }
        
        LogMigrationComplete($"{typeof(T).Name} on {target.name}");
        return newComponent;
    }
    
    // Verificar si un objeto necesita migración
    public static bool NeedsMigration<TOld, TNew>(GameObject target) 
        where TOld : Component 
        where TNew : Component
    {
        return target.GetComponent<TOld>() != null && target.GetComponent<TNew>() == null;
    }
}