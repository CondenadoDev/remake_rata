// IConfigurationProvider.cs

using UnityEngine;

public interface IConfigurationProvider
{
    T LoadConfiguration<T>(string key) where T : ScriptableObject;
    void SaveConfiguration<T>(string key, T data) where T : ScriptableObject;
    bool HasConfiguration(string key);
    void DeleteConfiguration(string key);
}