using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;     // slider
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject panelMain;        // VerticalLayout (Start / Options / Quit)
    [SerializeField] GameObject panelOptions;     // Panel de opciones (slider + back)

    [Header("Audio")]
    [SerializeField] AudioMixer masterMixer;
    [SerializeField] string exposedParam = "MasterVolume";
    [SerializeField] Slider sliderVolume;         // arrastra el Slider-Volume

    const string VOL_KEY = "MasterVol";

    /* ------------------ CICLO ------------------ */

    void Start()
    {
        Debug.Log($"[DEBUG] Mixer asignado = {masterMixer.name}");

        // Carga preferencia
        float saved = PlayerPrefs.GetFloat(VOL_KEY, 1f);
        sliderVolume.value = saved;
        ApplyVolume(saved);

        ShowMain();

        Debug.Log("<color=cyan>[MENU]</color> MainMenuUI iniciado");
    }

    /* -------------- BOTONES PRINCIPALES -------------- */

    public void OnStartPressed()
    {
        Debug.Log("<color=lime>[MENU]</color> Start ➜ Gameplay");
        SceneManager.LoadScene("Lobby");   // ajusta si tu escena tiene otro nombre
    }

    public void OnOptionsPressed()
    {
        Debug.Log("<color=yellow>[MENU]</color> Abre Opciones");
        ShowOptions();
    }

    public void OnQuitPressed()
    {
        Debug.Log("<color=red>[MENU]</color> Quit");
        Application.Quit();
    }

    /* ----------------- OPCIONES ----------------- */

    public void OnVolumeChanged(float value)        // Slider callback
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(VOL_KEY, value);
        Debug.Log($"<color=orange>[OPCIONES]</color> Volumen slider = {value:F2}");
    }

    public void OnBackPressed()
    {
        Debug.Log("<color=yellow>[MENU]</color> Volver al menú principal");
        ShowMain();
    }

    /* ---------------- HELPERS ---------------- */

    void ShowMain()
    {
        panelMain.SetActive(true);
        panelOptions.SetActive(false);
    }

    void ShowOptions()
    {
        panelMain.SetActive(false);
        panelOptions.SetActive(true);
    }

    void ApplyVolume(float normalized)
    {
        // 0-1  →  -80dB … 0dB
        float dB = Mathf.Lerp(-80f, 0f, normalized);
        masterMixer.SetFloat(exposedParam, dB);
    }
}
