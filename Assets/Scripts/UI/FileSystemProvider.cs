using UISystem.Core;
using UnityEngine;

public class FileSystemProvider : IConfigurationProvider
{
    private string GetPath(string key)
    {
        return System.IO.Path.Combine(Application.persistentDataPath, $"{key}.json");
    }

    public T LoadConfiguration<T>(string key) where T : ScriptableObject
    {
        string path = GetPath(key);
        if (!System.IO.File.Exists(path)) return null;
            
        string json = System.IO.File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }

    public void SaveConfiguration<T>(string key, T data) where T : ScriptableObject
    {
        string path = GetPath(key);
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(path, json);
    }

    public bool HasConfiguration(string key)
    {
        return System.IO.File.Exists(GetPath(key));
    }

    public void DeleteConfiguration(string key)
    {
        string path = GetPath(key);
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }
}