// SaveSystem.cs - Sistema de guardado/cargado
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class SaveSystem : MonoBehaviour
{
    [Header("⚙️ Configuration")]
    [SerializeField] private string saveFileName = "gamesave";
    [SerializeField] private string saveFileExtension = ".save";
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 minutos
    [SerializeField] private int maxSaveSlots = 3;
    [SerializeField] private bool encryptSaves = true;
    
    // Singleton
    public static SaveSystem Instance { get; private set; }
    
    // Save data
    private GameSaveData currentSaveData;
    private string saveDirectory;
    private float autoSaveTimer;
    
    // Eventos
    public static event System.Action<GameSaveData> OnGameSaved;
    public static event System.Action<GameSaveData> OnGameLoaded;
    public static event System.Action<string> OnSaveError;
    
    // Properties
    public GameSaveData CurrentSaveData => currentSaveData;
    public bool HasSaveData => currentSaveData != null;

    #region Initialization
    
    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeSaveSystem();
    }
    
    void InitializeSaveSystem()
    {
        // Configurar directorio de guardado
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        
        // Crear directorio si no existe
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        
        // Crear nuevo save data
        currentSaveData = new GameSaveData();
        
        Debug.Log($"💾 SaveSystem initialized. Save directory: {saveDirectory}");
    }
    
    #endregion

    #region Auto Save
    
    void Update()
    {
        if (enableAutoSave)
        {
            autoSaveTimer += Time.deltaTime;
            
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }
    }
    
    public void AutoSave()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Gameplay)
        {
            SaveGame(0, true); // Slot 0 para auto-save
            Debug.Log("💾 Auto-save completed");
        }
    }
    
    #endregion

    #region Save/Load Methods
    
    public bool SaveGame(int slot = 0, bool isAutoSave = false)
    {
        try
        {
            // Validar slot
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"❌ Invalid save slot: {slot}");
                return false;
            }
            
            // Actualizar datos de guardado
            UpdateSaveData();
            
            // Generar nombre de archivo
            string fileName = GetSaveFileName(slot, isAutoSave);
            string filePath = Path.Combine(saveDirectory, fileName);
            
            // Serializar datos
            string jsonData = JsonUtility.ToJson(currentSaveData, true);
            
            // Encriptar si está habilitado
            if (encryptSaves)
            {
                jsonData = EncryptString(jsonData);
            }
            
            // Escribir archivo
            File.WriteAllText(filePath, jsonData);
            
            // Actualizar metadatos del save
            currentSaveData.saveSlot = slot;
            currentSaveData.saveDate = DateTime.Now.ToString();
            currentSaveData.isAutoSave = isAutoSave;
            
            OnGameSaved?.Invoke(currentSaveData);
            
            Debug.Log($"💾 Game saved to slot {slot} ({(isAutoSave ? "Auto" : "Manual")})");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error saving game: {e.Message}");
            OnSaveError?.Invoke($"Error saving game: {e.Message}");
            return false;
        }
    }
    
    public bool LoadGame(int slot = 0)
    {
        try
        {
            // Validar slot
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"❌ Invalid save slot: {slot}");
                return false;
            }
            
            // Buscar archivo de guardado
            string fileName = GetSaveFileName(slot, false);
            string autoSaveFileName = GetSaveFileName(slot, true);
            string filePath = Path.Combine(saveDirectory, fileName);
            string autoSaveFilePath = Path.Combine(saveDirectory, autoSaveFileName);
            
            // Priorizar auto-save si existe y es más reciente
            if (File.Exists(autoSaveFilePath))
            {
                if (!File.Exists(filePath) || File.GetLastWriteTime(autoSaveFilePath) > File.GetLastWriteTime(filePath))
                {
                    filePath = autoSaveFilePath;
                }
            }
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"⚠️ No save file found for slot {slot}");
                return false;
            }
            
            // Leer archivo
            string jsonData = File.ReadAllText(filePath);
            
            // Desencriptar si es necesario
            if (encryptSaves)
            {
                jsonData = DecryptString(jsonData);
            }
            
            // Deserializar datos
            currentSaveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            if (currentSaveData == null)
            {
                Debug.LogError("❌ Failed to deserialize save data");
                return false;
            }
            
            // Aplicar datos cargados
            ApplySaveData();
            
            OnGameLoaded?.Invoke(currentSaveData);
            
            Debug.Log($"💾 Game loaded from slot {slot}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error loading game: {e.Message}");
            OnSaveError?.Invoke($"Error loading game: {e.Message}");
            return false;
        }
    }
    
    public bool DeleteSave(int slot)
    {
        try
        {
            string fileName = GetSaveFileName(slot, false);
            string autoSaveFileName = GetSaveFileName(slot, true);
            string filePath = Path.Combine(saveDirectory, fileName);
            string autoSaveFilePath = Path.Combine(saveDirectory, autoSaveFileName);
            
            bool deleted = false;
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                deleted = true;
            }
            
            if (File.Exists(autoSaveFilePath))
            {
                File.Delete(autoSaveFilePath);
                deleted = true;
            }
            
            if (deleted)
            {
                Debug.Log($"💾 Save slot {slot} deleted");
                return true;
            }
            else
            {
                Debug.LogWarning($"⚠️ No save file found to delete for slot {slot}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error deleting save: {e.Message}");
            return false;
        }
    }
    
    #endregion

    #region Save Data Management
    
    void UpdateSaveData()
    {
        if (currentSaveData == null)
            currentSaveData = new GameSaveData();
        
        // Player data
        var player = FindFirstObjectByType<PlayerStateMachine>();
        if (player != null)
        {
            currentSaveData.playerData.position = player.transform.position;
            currentSaveData.playerData.rotation = player.transform.rotation;
            
            if (player.Stats != null)
            {
                currentSaveData.playerData.currentHealth = player.Stats.CurrentHealth;
                currentSaveData.playerData.currentStamina = player.Stats.CurrentStamina;
            }
        }
        
        // Game state
        if (GameManager.Instance != null)
        {
            currentSaveData.gameData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            currentSaveData.gameData.gameState = GameManager.Instance.currentState.ToString();
        }
        
        // Settings
        if (AudioManager.Instance != null)
        {
            var volumeSettings = AudioManager.Instance.GetVolumeSettings();
            currentSaveData.settingsData.masterVolume = volumeSettings.masterVolume;
            currentSaveData.settingsData.musicVolume = volumeSettings.musicVolume;
            currentSaveData.settingsData.sfxVolume = volumeSettings.sfxVolume;
        }
        
        // Timestamp
        currentSaveData.saveDate = DateTime.Now.ToString();
        currentSaveData.playTime += Time.time - currentSaveData.sessionStartTime;
        currentSaveData.sessionStartTime = Time.time;
    }
    
    void ApplySaveData()
    {
        if (currentSaveData == null) return;
        
        // Player data
        var player = FindFirstObjectByType<PlayerStateMachine>();
        if (player != null)
        {
            player.transform.position = currentSaveData.playerData.position;
            player.transform.rotation = currentSaveData.playerData.rotation;
            
            if (player.Stats != null)
            {
                // Aplicar salud y stamina después de un frame para asegurar inicialización
                StartCoroutine(ApplyPlayerStatsDelayed(player.Stats));
            }
        }
        
        // Audio settings
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(currentSaveData.settingsData.masterVolume);
            AudioManager.Instance.SetMusicVolume(currentSaveData.settingsData.musicVolume);
            AudioManager.Instance.SetSFXVolume(currentSaveData.settingsData.sfxVolume);
        }
        
        // Scene loading
        string targetScene = currentSaveData.gameData.currentScene;
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (!string.IsNullOrEmpty(targetScene) && targetScene != currentScene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }
    
    System.Collections.IEnumerator ApplyPlayerStatsDelayed(PlayerStats stats)
    {
        yield return new WaitForEndOfFrame();
        
        if (stats != null)
        {
            stats.RestoreHealth(currentSaveData.playerData.currentHealth - stats.CurrentHealth);
            stats.RestoreStamina(currentSaveData.playerData.currentStamina - stats.CurrentStamina);
        }
    }
    
    #endregion

    #region Utility Methods
    
    string GetSaveFileName(int slot, bool isAutoSave)
    {
        string prefix = isAutoSave ? "auto_" : "";
        return $"{prefix}{saveFileName}_{slot:D2}{saveFileExtension}";
    }
    
    string EncryptString(string text)
    {
        // Encriptación simple XOR (para producción usar algo más robusto)
        byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
        byte key = 123; // Clave simple
        
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(data[i] ^ key);
        }
        
        return System.Convert.ToBase64String(data);
    }
    
    string DecryptString(string encryptedText)
    {
        try
        {
            byte[] data = System.Convert.FromBase64String(encryptedText);
            byte key = 123;
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ key);
            }
            
            return System.Text.Encoding.UTF8.GetString(data);
        }
        catch
        {
            // Si falla la desencriptación, asumir que no está encriptado
            return encryptedText;
        }
    }
    
    public SaveFileInfo[] GetSaveFileInfos()
    {
        List<SaveFileInfo> saveInfos = new List<SaveFileInfo>();
        
        for (int i = 0; i < maxSaveSlots; i++)
        {
            SaveFileInfo info = new SaveFileInfo
            {
                slot = i,
                exists = false
            };
            
            string fileName = GetSaveFileName(i, false);
            string autoSaveFileName = GetSaveFileName(i, true);
            string filePath = Path.Combine(saveDirectory, fileName);
            string autoSaveFilePath = Path.Combine(saveDirectory, autoSaveFileName);
            
            if (File.Exists(filePath) || File.Exists(autoSaveFilePath))
            {
                // Usar el archivo más reciente
                string mostRecentFile = filePath;
                if (File.Exists(autoSaveFilePath))
                {
                    if (!File.Exists(filePath) || File.GetLastWriteTime(autoSaveFilePath) > File.GetLastWriteTime(filePath))
                    {
                        mostRecentFile = autoSaveFilePath;
                        info.isAutoSave = true;
                    }
                }
                
                info.exists = true;
                info.lastModified = File.GetLastWriteTime(mostRecentFile);
                
                // Intentar leer metadatos básicos
                try
                {
                    string jsonData = File.ReadAllText(mostRecentFile);
                    if (encryptSaves)
                        jsonData = DecryptString(jsonData);
                    
                    GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                    if (saveData != null)
                    {
                        info.sceneName = saveData.gameData.currentScene;
                        info.playTime = saveData.playTime;
                        info.playerLevel = saveData.playerData.level;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Could not read save metadata for slot {i}: {e.Message}");
                }
            }
            
            saveInfos.Add(info);
        }
        
        return saveInfos.ToArray();
    }
    
    #endregion
}

// =====================================================================
// GameSaveData.cs - Estructura de datos de guardado
// =====================================================================

[System.Serializable]
public class GameSaveData
{
    [Header("📄 Save Metadata")]
    public string saveDate;
    public int saveSlot;
    public bool isAutoSave;
    public float playTime;
    public float sessionStartTime;
    public string gameVersion = "1.0.0";
    
    [Header("🎮 Game Data")]
    public GameData gameData = new GameData();
    
    [Header("👤 Player Data")]
    public PlayerSaveData playerData = new PlayerSaveData();
    
    [Header("⚙️ Settings Data")]
    public SettingsSaveData settingsData = new SettingsSaveData();
    
    [Header("🎯 Progress Data")]
    public ProgressSaveData progressData = new ProgressSaveData();
}

[System.Serializable]
public class GameData
{
    public string currentScene;
    public string gameState;
    public float gameTime;
    public int currentSeed;
}

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public float currentHealth = 100f;
    public float currentStamina = 100f;
    public int level = 1;
    public float experience = 0f;
    public string[] unlockedAbilities = new string[0];
    public ItemData[] inventory = new ItemData[0];
}

[System.Serializable]
public class SettingsSaveData
{
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    public float mouseSensitivity = 2f;
    public bool invertMouseY = false;
    public int qualityLevel = 3;
    public bool fullscreen = true;
}

[System.Serializable]
public class ProgressSaveData
{
    public string[] completedQuests = new string[0];
    public string[] unlockedAreas = new string[0];
    public int enemiesDefeated = 0;
    public float totalPlayTime = 0f;
    public System.Collections.Generic.Dictionary<string, float> statistics = 
        new System.Collections.Generic.Dictionary<string, float>();
}

[System.Serializable]
public class ItemData
{
    public string itemID;
    public int quantity = 1;
    public string[] modifiers = new string[0];
}

[System.Serializable]
public class SaveFileInfo
{
    public int slot;
    public bool exists;
    public bool isAutoSave;
    public System.DateTime lastModified;
    public string sceneName;
    public float playTime;
    public int playerLevel;
}