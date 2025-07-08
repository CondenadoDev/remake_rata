// AudioManager.cs
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("🎵 Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;
    
    [Header("🔊 Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private int sfxPoolSize = 10;
    
    [Header("⚙️ Settings")]
    [SerializeField] private float defaultFadeDuration = 1f;
    [SerializeField] private bool persistAcrossScenes = true;
    
    // Singleton
    public static AudioManager Instance { get; private set; }
    
    // Object Pooling para SFX
    private Queue<AudioSource> sfxSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    
    // Estado actual
    private AudioClip currentMusic;
    private AudioClip currentAmbience;
    private Dictionary<AudioClip, float> clipVolumes = new Dictionary<AudioClip, float>();
    
    // Configuración de volumen
    [System.Serializable]
    public class VolumeSettings
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float uiVolume = 0.6f;
        [Range(0f, 1f)] public float ambienceVolume = 0.5f;
    }
    
    [Header("🎚️ Volume")]
    [SerializeField] private VolumeSettings volumeSettings = new VolumeSettings();
    
    // Eventos
    public static event System.Action<AudioClip> OnMusicChanged;
    public static event System.Action<AudioClip> OnAmbienceChanged;
    public static event System.Action<VolumeSettings> OnVolumeChanged;

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
        
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
        
        InitializeAudioManager();
    }
    
    void InitializeAudioManager()
    {
        // Crear AudioSources si no existen
        if (musicSource == null)
        {
            musicSource = CreateAudioSource("Music Source", musicGroup);
            musicSource.loop = true;
        }
        
        if (ambienceSource == null)
        {
            ambienceSource = CreateAudioSource("Ambience Source", ambienceGroup);
            ambienceSource.loop = true;
        }
        
        // Crear pool de SFX
        CreateSfxPool();
        
        // Cargar configuración guardada
        LoadVolumeSettings();
        ApplyConfigurationValues();

        Debug.Log($"🔊 AudioManager initialized with {sfxPoolSize} SFX sources");
    }
    
    AudioSource CreateAudioSource(string name, AudioMixerGroup group)
    {
        GameObject sourceObj = new GameObject(name);
        sourceObj.transform.SetParent(transform);
        
        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = group;
        source.playOnAwake = false;
        
        return source;
    }
    
    void CreateSfxPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource source = CreateAudioSource($"SFX Source {i}", sfxGroup);
            source.gameObject.SetActive(false);
            sfxSourcePool.Enqueue(source);
        }
    }
    
    #endregion

    #region Music Management
    
    public void PlayMusic(AudioClip musicClip, bool fade = true, float customFadeDuration = -1f)
    {
        if (musicClip == null) return;
        
        float fadeDuration = customFadeDuration > 0 ? customFadeDuration : defaultFadeDuration;
        
        if (currentMusic == musicClip && musicSource.isPlaying) return;
        
        StartCoroutine(PlayMusicCoroutine(musicClip, fade, fadeDuration));
    }
    
    IEnumerator PlayMusicCoroutine(AudioClip musicClip, bool fade, float fadeDuration)
    {
        // Fade out música actual
        if (musicSource.isPlaying && fade)
        {
            yield return StartCoroutine(FadeAudioSource(musicSource, musicSource.volume, 0f, fadeDuration * 0.5f));
        }
        
        // Cambiar clip
        musicSource.clip = musicClip;
        currentMusic = musicClip;
        
        // Configurar volumen inicial
        float targetVolume = GetClipVolume(musicClip, volumeSettings.musicVolume);
        
        if (fade)
        {
            musicSource.volume = 0f;
            musicSource.Play();
            yield return StartCoroutine(FadeAudioSource(musicSource, 0f, targetVolume, fadeDuration * 0.5f));
        }
        else
        {
            musicSource.volume = targetVolume;
            musicSource.Play();
        }
        
        OnMusicChanged?.Invoke(musicClip);
        Debug.Log($"🎵 Now playing: {musicClip.name}");
    }
    
    public void StopMusic(bool fade = true, float customFadeDuration = -1f)
    {
        if (!musicSource.isPlaying) return;
        
        float fadeDuration = customFadeDuration > 0 ? customFadeDuration : defaultFadeDuration;
        
        if (fade)
        {
            StartCoroutine(StopMusicCoroutine(fadeDuration));
        }
        else
        {
            musicSource.Stop();
            currentMusic = null;
        }
    }
    
    IEnumerator StopMusicCoroutine(float fadeDuration)
    {
        yield return StartCoroutine(FadeAudioSource(musicSource, musicSource.volume, 0f, fadeDuration));
        musicSource.Stop();
        currentMusic = null;
        OnMusicChanged?.Invoke(null);
    }
    
    public void PauseMusic()
    {
        musicSource.Pause();
    }
    
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }
    
    #endregion

    #region SFX Management
    
    public void PlaySFX(AudioClip sfxClip, float volume = 1f, float pitch = 1f, Vector3? position = null)
    {
        if (sfxClip == null) return;
        
        AudioSource source = GetSfxSource();
        if (source == null) return;
        
        // Configurar source
        source.clip = sfxClip;
        source.volume = volume * volumeSettings.sfxVolume;
        source.pitch = pitch;
        source.loop = false;
        
        // Posición 3D opcional
        if (position.HasValue)
        {
            source.transform.position = position.Value;
            source.spatialBlend = 1f; // 3D
        }
        else
        {
            source.spatialBlend = 0f; // 2D
        }
        
        source.gameObject.SetActive(true);
        source.Play();
        
        // Programar retorno al pool
        StartCoroutine(ReturnSfxSourceToPool(source, sfxClip.length / pitch));
    }
    
    public void PlaySFX3D(AudioClip sfxClip, Vector3 position, float volume = 1f, float pitch = 1f, 
                          float minDistance = 1f, float maxDistance = 50f)
    {
        if (sfxClip == null) return;
        
        AudioSource source = GetSfxSource();
        if (source == null) return;
        
        // Configurar source 3D
        source.clip = sfxClip;
        source.volume = volume * volumeSettings.sfxVolume;
        source.pitch = pitch;
        source.loop = false;
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.transform.position = position;
        
        source.gameObject.SetActive(true);
        source.Play();
        
        StartCoroutine(ReturnSfxSourceToPool(source, sfxClip.length / pitch));
    }
    
    public void PlayUISFX(AudioClip uiClip, float volume = 1f)
    {
        if (uiClip == null) return;
        
        AudioSource source = GetSfxSource();
        if (source == null) return;
        
        source.outputAudioMixerGroup = uiGroup;
        source.clip = uiClip;
        source.volume = volume * volumeSettings.uiVolume;
        source.pitch = 1f;
        source.loop = false;
        source.spatialBlend = 0f;
        
        source.gameObject.SetActive(true);
        source.Play();
        
        StartCoroutine(ReturnSfxSourceToPool(source, uiClip.length));
    }
    
    AudioSource GetSfxSource()
    {
        if (sfxSourcePool.Count > 0)
        {
            AudioSource source = sfxSourcePool.Dequeue();
            activeSfxSources.Add(source);
            return source;
        }
        
        // Si no hay fuentes disponibles, crear una nueva
        AudioSource newSource = CreateAudioSource($"SFX Source {sfxPoolSize}", sfxGroup);
        activeSfxSources.Add(newSource);
        sfxPoolSize++;
        
        Debug.LogWarning("🔊 SFX Pool exhausted! Created new AudioSource.");
        return newSource;
    }
    
    IEnumerator ReturnSfxSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        
        if (source != null)
        {
            source.gameObject.SetActive(false);
            source.outputAudioMixerGroup = sfxGroup; // Reset mixer group
            activeSfxSources.Remove(source);
            sfxSourcePool.Enqueue(source);
        }
    }
    
    #endregion

    #region Ambience Management
    
    public void PlayAmbience(AudioClip ambienceClip, bool fade = true, float customFadeDuration = -1f)
    {
        if (ambienceClip == null) return;
        
        float fadeDuration = customFadeDuration > 0 ? customFadeDuration : defaultFadeDuration;
        
        if (currentAmbience == ambienceClip && ambienceSource.isPlaying) return;
        
        StartCoroutine(PlayAmbienceCoroutine(ambienceClip, fade, fadeDuration));
    }
    
    IEnumerator PlayAmbienceCoroutine(AudioClip ambienceClip, bool fade, float fadeDuration)
    {
        // Fade out ambience actual
        if (ambienceSource.isPlaying && fade)
        {
            yield return StartCoroutine(FadeAudioSource(ambienceSource, ambienceSource.volume, 0f, fadeDuration * 0.5f));
        }
        
        // Cambiar clip
        ambienceSource.clip = ambienceClip;
        currentAmbience = ambienceClip;
        
        // Configurar volumen
        float targetVolume = GetClipVolume(ambienceClip, volumeSettings.ambienceVolume);
        
        if (fade)
        {
            ambienceSource.volume = 0f;
            ambienceSource.Play();
            yield return StartCoroutine(FadeAudioSource(ambienceSource, 0f, targetVolume, fadeDuration * 0.5f));
        }
        else
        {
            ambienceSource.volume = targetVolume;
            ambienceSource.Play();
        }
        
        OnAmbienceChanged?.Invoke(ambienceClip);
    }
    
    public void StopAmbience(bool fade = true)
    {
        if (!ambienceSource.isPlaying) return;
        
        if (fade)
        {
            StartCoroutine(StopAmbienceCoroutine());
        }
        else
        {
            ambienceSource.Stop();
            currentAmbience = null;
        }
    }
    
    IEnumerator StopAmbienceCoroutine()
    {
        yield return StartCoroutine(FadeAudioSource(ambienceSource, ambienceSource.volume, 0f, defaultFadeDuration));
        ambienceSource.Stop();
        currentAmbience = null;
        OnAmbienceChanged?.Invoke(null);
    }
    
    #endregion

    #region Volume Control
    
    public void SetMasterVolume(float volume)
    {
        volumeSettings.masterVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("MasterVolume", volumeSettings.masterVolume);
        SaveVolumeSettings();
        OnVolumeChanged?.Invoke(volumeSettings);
    }
    
    public void SetMusicVolume(float volume)
    {
        volumeSettings.musicVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("MusicVolume", volumeSettings.musicVolume);
        if (musicSource.isPlaying)
        {
            musicSource.volume = GetClipVolume(currentMusic, volumeSettings.musicVolume);
        }
        SaveVolumeSettings();
        OnVolumeChanged?.Invoke(volumeSettings);
    }
    
    public void SetSFXVolume(float volume)
    {
        volumeSettings.sfxVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("SFXVolume", volumeSettings.sfxVolume);
        SaveVolumeSettings();
        OnVolumeChanged?.Invoke(volumeSettings);
    }
    
    public void SetUIVolume(float volume)
    {
        volumeSettings.uiVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("UIVolume", volumeSettings.uiVolume);
        SaveVolumeSettings();
        OnVolumeChanged?.Invoke(volumeSettings);
    }
    
    public void SetAmbienceVolume(float volume)
    {
        volumeSettings.ambienceVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("AmbienceVolume", volumeSettings.ambienceVolume);
        if (ambienceSource.isPlaying)
        {
            ambienceSource.volume = GetClipVolume(currentAmbience, volumeSettings.ambienceVolume);
        }
        SaveVolumeSettings();
        OnVolumeChanged?.Invoke(volumeSettings);
    }
    
    void UpdateMixerVolume(string parameterName, float volume)
    {
        if (audioMixer != null)
        {
            float dbValue = volume > 0.001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat(parameterName, dbValue);
        }
    }
    
    public VolumeSettings GetVolumeSettings()
    {
        return volumeSettings;
    }

    /// <summary>
    /// Apply configuration values from the global AudioConfig to this manager.
    /// </summary>
    public void ApplyConfigurationValues()
    {
        if (ConfigurationManager.Instance == null || ConfigurationManager.Audio == null)
            return;

        var config = ConfigurationManager.Audio;

        SetMasterVolume(config.masterVolume);
        SetMusicVolume(config.musicVolume);
        SetSFXVolume(config.sfxVolume);
        SetUIVolume(config.uiVolume);
        SetAmbienceVolume(config.ambienceVolume);
    }
    
    #endregion

    #region Utility Methods
    
    IEnumerator FadeAudioSource(AudioSource source, float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        
        source.volume = targetVolume;
    }
    
    float GetClipVolume(AudioClip clip, float baseVolume)
    {
        if (clip != null && clipVolumes.TryGetValue(clip, out float clipVolume))
        {
            return baseVolume * clipVolume;
        }
        return baseVolume;
    }
    
    public void SetClipVolume(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            clipVolumes[clip] = Mathf.Clamp01(volume);
        }
    }
    
    public void StopAllSFX()
    {
        foreach (AudioSource source in activeSfxSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }
    
    public bool IsPlayingMusic()
    {
        return musicSource.isPlaying;
    }
    
    public bool IsPlayingAmbience()
    {
        return ambienceSource.isPlaying;
    }
    
    public AudioClip GetCurrentMusic()
    {
        return currentMusic;
    }
    
    public AudioClip GetCurrentAmbience()
    {
        return currentAmbience;
    }
    
    #endregion

    #region Save/Load Settings
    
    void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("Audio_MasterVolume", volumeSettings.masterVolume);
        PlayerPrefs.SetFloat("Audio_MusicVolume", volumeSettings.musicVolume);
        PlayerPrefs.SetFloat("Audio_SFXVolume", volumeSettings.sfxVolume);
        PlayerPrefs.SetFloat("Audio_UIVolume", volumeSettings.uiVolume);
        PlayerPrefs.SetFloat("Audio_AmbienceVolume", volumeSettings.ambienceVolume);
        PlayerPrefs.Save();
    }
    
    void LoadVolumeSettings()
    {
        volumeSettings.masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", volumeSettings.masterVolume);
        volumeSettings.musicVolume = PlayerPrefs.GetFloat("Audio_MusicVolume", volumeSettings.musicVolume);
        volumeSettings.sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", volumeSettings.sfxVolume);
        volumeSettings.uiVolume = PlayerPrefs.GetFloat("Audio_UIVolume", volumeSettings.uiVolume);
        volumeSettings.ambienceVolume = PlayerPrefs.GetFloat("Audio_AmbienceVolume", volumeSettings.ambienceVolume);
        
        // Aplicar configuración cargada
        UpdateMixerVolume("MasterVolume", volumeSettings.masterVolume);
        UpdateMixerVolume("MusicVolume", volumeSettings.musicVolume);
        UpdateMixerVolume("SFXVolume", volumeSettings.sfxVolume);
        UpdateMixerVolume("UIVolume", volumeSettings.uiVolume);
        UpdateMixerVolume("AmbienceVolume", volumeSettings.ambienceVolume);
    }
    
    #endregion
}