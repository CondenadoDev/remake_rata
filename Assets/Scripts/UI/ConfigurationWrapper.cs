// ConfigurationWrapper.cs - Para envolver datos no-ScriptableObject

using UnityEngine;

[CreateAssetMenu(fileName = "ConfigWrapper", menuName = "UI System/Configuration Wrapper")]
public class ConfigurationWrapper : ScriptableObject
{
    [SerializeField] private string jsonData;

    public void SetData<T>(T data) where T : class
    {
        jsonData = JsonUtility.ToJson(data);
    }

    public T GetData<T>() where T : class
    {
        return JsonUtility.FromJson<T>(jsonData);
    }
}