using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Game/Configs/Audio Config")]
public class AudioConfig : ConfigBase
{
    [Header("🔊 Volume Levels")]
    [UIOption("Volumen General", UIControlType.Slider, 0f, 1f, "Volume", 1)]
    [Range(0f, 1f)] public float masterVolume = 1f;
    
    [UIOption("Volumen Música", UIControlType.Slider, 0f, 1f, "Volume", 2)]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    
    [UIOption("Volumen Efectos", UIControlType.Slider, 0f, 1f, "Volume", 3)]
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    
    [UIOption("Volumen UI", UIControlType.Slider, 0f, 1f, "Volume", 4)]
    [Range(0f, 1f)] public float uiVolume = 0.6f;
    
    [UIOption("Volumen Ambiente", UIControlType.Slider, 0f, 1f, "Volume", 5)]
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;
    
    [UIOption("Volumen Voces", UIControlType.Slider, 0f, 1f, "Volume", 6)]
    [Range(0f, 1f)] public float voiceVolume = 0.8f;
    
    [Header("🎵 Music Settings")]
    [UIOption("Habilitar Música", UIControlType.Toggle, "Music", 10)]
    public bool enableMusic = true;
    
    [UIOption("Crossfade Musical", UIControlType.Toggle, "Music", 11)]
    public bool crossfadeMusic = true;
    
    [UIOption("Duración Crossfade", UIControlType.Slider, 0.5f, 5f, "Music", 12)]
    public float musicCrossfadeDuration = 2f;
    
    [UIOption("Playlist Aleatoria", UIControlType.Toggle, "Music", 13)]
    public bool randomizePlaylist = false;
    
    [Header("🔔 SFX Settings")]
    [UIOption("Habilitar Efectos", UIControlType.Toggle, "Effects", 20)]
    public bool enableSFX = true;
    
    [UIOption("Max SFX Concurrentes", UIControlType.Slider, 8f, 64f, "Effects", 21)]
    public int maxConcurrentSFX = 32;
    
    [UIOption("Duración Fade Out", UIControlType.Slider, 0.1f, 2f, "Effects", 22)]
    public float sfxFadeOutDuration = 0.5f;
    
    [UIOption("Audio 3D", UIControlType.Toggle, "Effects", 23)]
    public bool use3DAudio = true;
    
    [Header("🎧 Quality Settings")]
    [UIOption("Calidad Audio", new string[] {"Baja", "Media", "Alta", "Ultra"}, "Quality", 30)]
    public AudioQuality audioQuality = AudioQuality.Alta;
    
    [UIOption("Compresión Audio", UIControlType.Toggle, "Quality", 31)]
    public bool enableAudioCompression = false;
    
    [UIOption("Latencia Audio", UIControlType.Slider, 0.001f, 0.1f, "Quality", 32)]
    public float audioLatency = 0.02f;
    
    [Header("🌍 Spatial Audio")]
    [UIOption("Nivel Doppler", UIControlType.Slider, 0f, 1f, "Spatial", 40)]
    public float dopplerLevel = 0.5f;
    
    [UIOption("Volumen Listener", UIControlType.Slider, 0f, 2f, "Spatial", 41)]
    public float listenerVolume = 1f;
    
    // Curve no se puede generar automáticamente, se omite de UI
    public AnimationCurve spatialFalloffCurve = AnimationCurve.Linear(0, 1, 1, 0);

    public override void ValidateValues()
    {
        musicCrossfadeDuration = Mathf.Max(0.1f, musicCrossfadeDuration);
        maxConcurrentSFX = Mathf.Clamp(maxConcurrentSFX, 1, 128);
        sfxFadeOutDuration = Mathf.Max(0f, sfxFadeOutDuration);
        audioLatency = Mathf.Clamp(audioLatency, 0.001f, 0.1f);
        dopplerLevel = Mathf.Clamp01(dopplerLevel);
    }
}