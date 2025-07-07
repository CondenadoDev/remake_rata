using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private string exposedParam = "MasterVolume";
    [SerializeField] private Slider sliderVolume;

    [Header("Botones")]
    [SerializeField] private Button loadButton;

    const string VOL_KEY = "MasterVol";

    void Start()
    {
        Debug.Log($"[DEBUG] Mixer asignado = {masterMixer.name}");

        // Cargar volumen guardado
        float saved = PlayerPrefs.GetFloat(VOL_KEY, 1f);
        sliderVolume.value = saved;
        ApplyVolume(saved);

        // Verifica si hay partida guardada
        if (SaveSystem.Instance != null && SaveSystem.Instance.GetSaveFileInfos()[0].exists)
        {
            loadButton.interactable = true;
        }
        else
        {
            loadButton.interactable = false;
            Debug.Log("<color=orange>[MENU]</color> No hay partida para cargar");
        }

        Debug.Log("<color=cyan>[MENU]</color> MainMenuUI iniciado");
    }

    /* ---------- BOTONES ---------- */

    public void OnStartPressed()
    {
        Debug.Log("<color=lime>[MENU]</color> Nueva partida ➜ Gameplay");

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSave(0); // Reinicia slot 0
        }

        SceneManager.LoadScene("Lobby"); // Ajusta si tu escena tiene otro nombre
    }

    public void OnLoadPressed()
    {
        Debug.Log("<color=cyan>[MENU]</color> Cargar Partida");

        if (SaveSystem.Instance != null)
        {
            bool result = SaveSystem.Instance.LoadGame(0);

            if (!result)
                Debug.LogWarning("<color=red>[MENU]</color> No se pudo cargar la partida.");
        }
        else
        {
            Debug.LogWarning("SaveSystem no inicializado");
        }
    }

    public void OnOptionsPressed()
    {
        Debug.Log("<color=yellow>[MENU]</color> Abre Opciones");
        UIManager.Instance.ShowPanel("OptionsMenu"); // Usar sistema UIManager
    }

    public void OnQuitPressed()
    {
        Debug.Log("<color=red>[MENU]</color> Quit");
        Application.Quit();
    }

    public void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(VOL_KEY, value);
        Debug.Log($"<color=orange>[OPCIONES]</color> Volumen slider = {value:F2}");
    }

    /* ---------- MÉTODOS AUXILIARES ---------- */

    void ApplyVolume(float normalized)
    {
        float dB = Mathf.Lerp(-80f, 0f, normalized);
        masterMixer.SetFloat(exposedParam, dB);
    }
}
