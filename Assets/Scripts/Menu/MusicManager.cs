using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Header("Audio")]
    public  AudioClip       menuTheme;             // arrastra tu pista
    public  AudioMixerGroup outputGroup;           // Master o “Music”
    [Range(0f, 1f)] public float startVolume = 1f; // volumen inicial (0-1)

    AudioSource src;
    static MusicManager instance;                  // para evitar duplicados

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);             // persiste entre escenas

        src = GetComponent<AudioSource>();
        src.clip  = menuTheme;
        src.loop  = true;
        src.playOnAwake = false;
        src.outputAudioMixerGroup = outputGroup;
        src.volume = startVolume;
        src.Play();

        Debug.Log("<color=cyan>[MUSIC]</color> Reproduciendo theme de menú");
    }

    /* Opcional: detener música manualmente */
    public static void StopMusic()
    {
        if (instance != null) instance.src.Stop();
    }
}