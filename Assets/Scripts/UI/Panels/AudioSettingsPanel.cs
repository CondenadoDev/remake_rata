using UnityEngine;

public class AudioSettingsPanel : SettingsPanel<AudioConfig>
{
    public override string PersistenceKey => "AudioSettings";
        
    protected override void OnApplyConfiguration(AudioConfig config)
    {
        // Apply volume settings
        AudioListener.volume = config.masterVolume;
            
        // Apply audio configuration
        AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
        audioConfig.speakerMode = config.speakerMode;
        audioConfig.sampleRate = config.sampleRate;
        AudioSettings.Reset(audioConfig);
            
        // Note: Music, SFX, Voice volumes would be applied through your audio system
        // Example: AudioManager.Instance.SetMusicVolume(config.musicVolume);
    }
        
    protected override void RefreshUI()
    {
        // UI will be automatically refreshed through bindings
    }
}