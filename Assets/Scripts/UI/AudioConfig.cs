using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Game/Audio Config")]
public class AudioConfig : ScriptableObject
{
    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1f;
    [Range(0, 1)]
    public float musicVolume = 0.8f;
    [Range(0, 1)]
    public float sfxVolume = 1f;
    [Range(0, 1)]
    public float voiceVolume = 1f;
    [Range(0, 1)]
    public float ambientVolume = 0.6f;
        
    [Header("Audio Quality")]
    public AudioSpeakerMode speakerMode = AudioSpeakerMode.Stereo;
    public int sampleRate = 48000;
    public AudioReverbPreset reverbPreset = AudioReverbPreset.Off;
        
    [Header("Features")]
    public bool enableSpatialAudio = true;
    public bool enableDynamicRange = true;
    public bool muteOnFocusLoss = true;
}