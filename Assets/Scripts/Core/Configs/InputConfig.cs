using UnityEngine;

[CreateAssetMenu(fileName = "InputConfig", menuName = "Game/Configs/Input Config")]
public class InputConfig : ConfigBase
{
    [Header("🖱️ Mouse")] [UIOption("Sensibilidad Ratón", UIControlType.Slider, 0.1f, 10f, "Mouse", 1)]
    public float mouseSensitivity = 2f;

    [UIOption("Invertir Eje Y", UIControlType.Toggle, "Mouse", 2)]
    public bool invertMouseY = false;

    [UIOption("Suavizado Ratón", UIControlType.Toggle, "Mouse", 3)]
    public bool enableMouseSmoothing = true;

    [UIOption("Intensidad Suavizado", UIControlType.Slider, 0.1f, 10f, "Mouse", 4)]
    public float mouseSmoothing = 5f;

    [Header("🎮 Gamepad")] [UIOption("Sensibilidad Gamepad", UIControlType.Slider, 0.1f, 5f, "Gamepad", 10)]
    public float gamepadSensitivity = 1.5f;

    [UIOption("Invertir Y Gamepad", UIControlType.Toggle, "Gamepad", 11)]
    public bool invertGamepadY = false;

    [UIOption("Zona Muerta", UIControlType.Slider, 0f, 0.5f, "Gamepad", 12)]
    public float gamepadDeadzone = 0.1f;

    [UIOption("Vibración", UIControlType.Toggle, "Gamepad", 13)]
    public bool enableGamepadVibration = true;

    [Header("⌨️ Keyboard")] [UIOption("Repetición Teclas", UIControlType.Toggle, "Keyboard", 20)]
    public bool enableKeyRepeat = true;

    [UIOption("Delay Repetición", UIControlType.Slider, 0.1f, 1f, "Keyboard", 21)]
    public float keyRepeatDelay = 0.5f;

    [UIOption("Velocidad Repetición", UIControlType.Slider, 0.05f, 0.5f, "Keyboard", 22)]
    public float keyRepeatRate = 0.1f;

    [Header("🎯 Targeting")] [UIOption("Auto-Apuntado", UIControlType.Toggle, "Targeting", 30)]
    public bool enableAutoAim = false;

    [UIOption("Fuerza Auto-Apuntado", UIControlType.Slider, 0f, 1f, "Targeting", 31)]
    public float autoAimStrength = 0.3f;

    [UIOption("Rango Auto-Apuntado", UIControlType.Slider, 1f, 50f, "Targeting", 32)]
    public float autoAimRange = 10f;

    [Header("⏱️ Timing")] [UIOption("Tiempo Doble Click", UIControlType.Slider, 0.1f, 1f, "Timing", 40)]
    public float doubleClickTime = 0.3f;

    [UIOption("Tiempo Mantener", UIControlType.Slider, 0.1f, 2f, "Timing", 41)]
    public float holdTime = 0.5f;

    [UIOption("Buffer Inputs", UIControlType.Toggle, "Timing", 42)]
    public bool bufferInputs = true;

    [UIOption("Tiempo Buffer", UIControlType.Slider, 0f, 1f, "Timing", 43)]
    public float inputBufferTime = 0.2f;

    public override void ValidateValues()
    {
        mouseSensitivity = Mathf.Clamp(mouseSensitivity, 0.1f, 10f);
        mouseSmoothing = Mathf.Max(0.1f, mouseSmoothing);

        gamepadSensitivity = Mathf.Clamp(gamepadSensitivity, 0.1f, 5f);
        gamepadDeadzone = Mathf.Clamp01(gamepadDeadzone);

        autoAimStrength = Mathf.Clamp01(autoAimStrength);
        autoAimRange = Mathf.Max(1f, autoAimRange);

        doubleClickTime = Mathf.Clamp(doubleClickTime, 0.1f, 1f);
        holdTime = Mathf.Clamp(holdTime, 0.1f, 2f);
        inputBufferTime = Mathf.Clamp(inputBufferTime, 0f, 1f);
    }
}