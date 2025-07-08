 using UISystem.Core;
 using UnityEngine;

 public class PlayerPrefsProvider : IConfigurationProvider
    {
        public T LoadConfiguration<T>(string key) where T : ScriptableObject
        {
            if (!PlayerPrefs.HasKey(key)) return null;
            
            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<T>(json);
        }

        public void SaveConfiguration<T>(string key, T data) where T : ScriptableObject
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public bool HasConfiguration(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteConfiguration(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }