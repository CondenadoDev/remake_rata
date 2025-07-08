using System;
using System.Collections.Generic;
using UnityEngine;
using UISystem.Core;

namespace UISystem.Configuration
{
    public class ConfigurationManager : MonoBehaviour
    {
        private static ConfigurationManager _instance;

        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ConfigurationManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ConfigurationManager");
                        _instance = go.AddComponent<ConfigurationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private ConfigurationProviderType defaultProvider = ConfigurationProviderType.PlayerPrefs;

        [Header("🔊 Global Scriptable Configs")]
        [SerializeField] private AudioConfig audioConfig;
        [SerializeField] private InputConfig inputConfig;
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private GraphicsConfig graphicsConfig; // AGREGADO

        private Dictionary<ConfigurationProviderType, IConfigurationProvider> providers;
        private Dictionary<string, object> configCache;

        public event Action<string, object> OnConfigurationChanged;

        // === Propiedades de acceso rápido universal ===

        public AudioConfig Audio
        {
            get
            {
                if (audioConfig == null)
                    audioConfig = GetConfiguration<AudioConfig>("AudioConfig");
                return audioConfig;
            }
        }

        public InputConfig Input
        {
            get
            {
                if (inputConfig == null)
                    inputConfig = GetConfiguration<InputConfig>("InputConfig");
                return inputConfig;
            }
        }

        public PlayerConfig Player
        {
            get
            {
                if (playerConfig == null)
                    playerConfig = GetConfiguration<PlayerConfig>("PlayerConfig");
                return playerConfig;
            }
        }

        public GraphicsConfig Graphics // AGREGADO
        {
            get
            {
                if (graphicsConfig == null)
                    graphicsConfig = GetConfiguration<GraphicsConfig>("GraphicsConfig");
                return graphicsConfig;
            }
        }

        // === Unity Methods ===

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeProviders();
            configCache = new Dictionary<string, object>();
        }

        private void InitializeProviders()
        {
            providers = new Dictionary<ConfigurationProviderType, IConfigurationProvider>
            {
                { ConfigurationProviderType.PlayerPrefs, new PlayerPrefsProvider() },
                { ConfigurationProviderType.ScriptableObject, new ScriptableObjectProvider() },
                { ConfigurationProviderType.FileSystem, new FileSystemProvider() }
            };
        }

        // === Métodos generales ===

        public T GetConfiguration<T>(string key, ConfigurationProviderType? providerType = null) where T : ScriptableObject
        {
            var provider = providerType ?? defaultProvider;

            // Check cache first
            string cacheKey = $"{provider}_{key}";
            if (configCache.TryGetValue(cacheKey, out object cached))
            {
                return cached as T;
            }

            // Load from provider
            var config = providers[provider].LoadConfiguration<T>(key);
            if (config != null)
            {
                configCache[cacheKey] = config;
            }

            return config;
        }

        public void SaveConfiguration<T>(string key, T data, ConfigurationProviderType? providerType = null)
            where T : ScriptableObject
        {
            var provider = providerType ?? defaultProvider;
            providers[provider].SaveConfiguration(key, data);

            // Update cache
            string cacheKey = $"{provider}_{key}";
            configCache[cacheKey] = data;

            // Notify listeners
            OnConfigurationChanged?.Invoke(key, data);
        }

        public void ClearCache()
        {
            configCache.Clear();
        }
    }

    public enum ConfigurationProviderType
    {
        PlayerPrefs,
        ScriptableObject,
        FileSystem
    }
}
