 using System;
 using UISystem.Configuration;
 using UnityEngine;

 public class GameDataManager : MonoBehaviour
    {
        private static GameDataManager _instance;
        public static GameDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameDataManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameDataManager");
                        _instance = go.AddComponent<GameDataManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        private PlayerData currentPlayerData;
        private const string SAVE_KEY = "PlayerSave";
        
        public PlayerData CurrentPlayerData => currentPlayerData;
        public event Action<PlayerData> OnPlayerDataChanged;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadPlayerData();
        }
        
        public void SavePlayerData()
        {
            if (currentPlayerData == null) return;
            
            ConfigurationManager.Instance.SaveConfiguration(SAVE_KEY, currentPlayerData);
        }
        
        public void LoadPlayerData()
        {
            currentPlayerData = ConfigurationManager.Instance.GetConfiguration<PlayerData>(SAVE_KEY);
            
            if (currentPlayerData == null)
            {
                currentPlayerData = new PlayerData();
            }
            
            OnPlayerDataChanged?.Invoke(currentPlayerData);
        }
        
        public void UpdatePlayerData(Action<PlayerData> updateAction)
        {
            if (currentPlayerData == null) return;
            
            updateAction(currentPlayerData);
            OnPlayerDataChanged?.Invoke(currentPlayerData);
        }
    }