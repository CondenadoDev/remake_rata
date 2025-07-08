using UnityEngine;
[CreateAssetMenu(fileName = "InputConfig", menuName = "Game/Configs/Input Config")]
public class InputConfig : ConfigurationBase
{
    [Header("🖱️ Mouse")]
    public float mouseSensitivity = 2f;
    public bool invertMouseY = false;
    public bool enableMouseSmoothing = true;
    public float mouseSmoothing = 5f;
    
    [Header("🎮 Gamepad")]
    public float gamepadSensitivity = 1.5f;
    public bool invertGamepadY = false;
    public float gamepadDeadzone = 0.1f;
    public bool enableGamepadVibration = true;
    
    [Header("⌨️ Keyboard")]
    public bool enableKeyRepeat = true;
    public float keyRepeatDelay = 0.5f;
    public float keyRepeatRate = 0.1f;
    
    [Header("🎯 Targeting")]
    public bool enableAutoAim = false;
    public float autoAimStrength = 0.3f;
    public float autoAimRange = 10f;
    
    [Header("⏱️ Timing")]
    public float doubleClickTime = 0.3f;
    public float holdTime = 0.5f;
    public bool bufferInputs = true;
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