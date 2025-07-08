// ScriptableObjectProvider.cs

using UISystem.Core;
using UnityEngine;

public class ScriptableObjectProvider : IConfigurationProvider
{
    private const string RESOURCES_PATH = "Configurations/";

    // Solo para ScriptableObject
    public T LoadConfiguration<T>(string key) where T : ScriptableObject
    {
        // Resources.Load<T> SOLO acepta T : UnityEngine.Object (ScriptableObject incluido)
        return Resources.Load<T>($"{RESOURCES_PATH}{key}");
    }

#if UNITY_EDITOR
    public void SaveConfiguration<T>(string key, T data) where T : ScriptableObject
    {
        // Solo funciona en Editor
        var path = $"Assets/Resources/{RESOURCES_PATH}{key}.asset";
        UnityEditor.AssetDatabase.CreateAsset(data, path);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#else
    public void SaveConfiguration<T>(string key, T data) where T : ScriptableObject
    {
        Debug.LogWarning("ScriptableObjectProvider.SaveConfiguration solo funciona en Unity Editor.");
    }
#endif

    public bool HasConfiguration(string key)
    {
        // Resources.Load devuelve null si no existe
        return Resources.Load<ScriptableObject>($"{RESOURCES_PATH}{key}") != null;
    }

#if UNITY_EDITOR
    public void DeleteConfiguration(string key)
    {
        var path = $"Assets/Resources/{RESOURCES_PATH}{key}.asset";
        UnityEditor.AssetDatabase.DeleteAsset(path);
    }
#else
    public void DeleteConfiguration(string key)
    {
        Debug.LogWarning("ScriptableObjectProvider.DeleteConfiguration solo funciona en Unity Editor.");
    }
#endif
}