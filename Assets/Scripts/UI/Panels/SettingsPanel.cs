using System;
using System.Collections;
using System.Collections.Generic;
using UISystem.Configuration;
using UISystem.Core;
using UISystem.Panels;
using UnityEngine;

public abstract class SettingsPanel<T> : BaseUIPanel, IConfigurable<T>, IPersistable 
        where T : ScriptableObject
    {
        [Header("Settings")]
        [SerializeField] protected T defaultConfiguration;
        [SerializeField] protected bool autoSaveOnChange = true;
        [SerializeField] protected float saveDelay = 0.5f;
        
        protected T currentConfiguration;
        protected SettingsData<T> settingsData;
        private Coroutine saveCoroutine;
        
        public T Configuration 
        { 
            get => currentConfiguration;
            set
            {
                currentConfiguration = value;
                ApplyConfiguration(value);
            }
        }
        
        public abstract string PersistenceKey { get; }

        protected override void OnInitialize()
        {
            // Load saved settings or use defaults
            Load();
            
            // Apply to UI
            RefreshUI();
        }

        public virtual void ApplyConfiguration(T config)
        {
            if (config == null) return;
            
            currentConfiguration = config;
            settingsData = ScriptableObject.CreateInstance<SettingsData<T>>();
            settingsData.configuration = config;
            
            // Bind UI to settings data
            BindToData(settingsData);
            
            // Apply to game systems
            OnApplyConfiguration(config);
            
            // Auto save if enabled
            if (autoSaveOnChange)
            {
                ScheduleSave();
            }
        }

        public virtual void ResetToDefault()
        {
            if (defaultConfiguration != null)
            {
                ApplyConfiguration(Instantiate(defaultConfiguration));
                Save();
            }
        }

        public virtual void Save()
        {
            if (settingsData == null) return;
            
            ConfigurationManager.Instance.SaveConfiguration(PersistenceKey, settingsData);
        }

        public virtual void Load()
        {
            // Try to load saved data
            settingsData = ConfigurationManager.Instance.GetConfiguration<SettingsData<T>>(PersistenceKey);
            
            if (settingsData?.configuration != null)
            {
                currentConfiguration = settingsData.configuration;
            }
            else if (defaultConfiguration != null)
            {
                currentConfiguration = Instantiate(defaultConfiguration);
                settingsData = ScriptableObject.CreateInstance<SettingsData<T>>();
                settingsData.configuration = currentConfiguration;
            }
        }

        public bool HasSavedData()
        {
            return ConfigurationManager.Instance.GetConfiguration<SettingsData<T>>(PersistenceKey) != null;
        }

        protected abstract void OnApplyConfiguration(T config);
        protected abstract void RefreshUI();

        protected void ScheduleSave()
        {
            if (saveCoroutine != null)
            {
                StopCoroutine(saveCoroutine);
            }
            
            saveCoroutine = StartCoroutine(SaveAfterDelay());
        }

        private IEnumerator SaveAfterDelay()
        {
            yield return new WaitForSeconds(saveDelay);
            Save();
        }
    }

    [Serializable]
    public class SettingsData<T> : ScriptableObject where T : ScriptableObject
    {
        public T configuration;
        public long lastModified;
        public int version;
        
        public SettingsData()
        {
            lastModified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            version = 1;
        }
    }