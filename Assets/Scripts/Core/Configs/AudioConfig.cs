using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Game/Configs/Audio Config")]
public class AudioConfig : ConfigurationBase
{
    [Header("🔊 Volume Levels")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float uiVolume = 0.6f;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;
    [Range(0f, 1f)] public float voiceVolume = 0.8f;
    
    [Header("🎵 Music Settings")]
    public bool enableMusic = true;
    public bool crossfadeMusic = true;
    public float musicCrossfadeDuration = 2f;
    public bool randomizePlaylist = false;
    
    [Header("🔔 SFX Settings")]
    public bool enableSFX = true;
    public int maxConcurrentSFX = 32;
    public float sfxFadeOutDuration = 0.5f;
    public bool use3DAudio = true;
    
    [Header("🎧 Quality Settings")]
    public AudioQuality audioQuality = AudioQuality.High;
    public bool enableAudioCompression = false;
    public float audioLatency = 0.02f;
    
    [Header("🌍 Spatial Audio")]
    public float dopplerLevel = 0.5f;
    public float listenerVolume = 1f;
    public AnimationCurve spatialFalloffCurve = AnimationCurve.Linear(0, 1, 1, 0);
    
    protected override void ValidateValues()
    {
        musicCrossfadeDuration = Mathf.Max(0.1f, musicCrossfadeDuration);
        maxConcurrentSFX = Mathf.Clamp(maxConcurrentSFX, 1, 128);
        sfxFadeOutDuration = Mathf.Max(0f, sfxFadeOutDuration);
        audioLatency = Mathf.Clamp(audioLatency, 0.001f, 0.1f);
        dopplerLevel = Mathf.Clamp01(dopplerLevel);
    }
}

public enum AudioQuality
{
    Low,
    Medium, 
    High,
    Ultra
}