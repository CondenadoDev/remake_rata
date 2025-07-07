// InputManager.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using InputConfig = InputConfig;

public class InputManager : MonoBehaviour
{
    [Header("📋 Input Assets")]
    [SerializeField] private InputActionAsset inputActions;
    
    [Header("⚙️ Settings")]
    [SerializeField] private bool enableInputBuffering = true;
    [SerializeField] private float inputBufferTime = 0.2f;
    [SerializeField] private bool debugInputs = false;
    
    // Singleton
    public static InputManager Instance { get; private set; }
    
    // Input Maps
    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;
    private InputActionMap debugActionMap;
    
    // Input Buffer System
    private Dictionary<string, float> inputBuffer = new Dictionary<string, float>();
    private Dictionary<string, bool> inputStates = new Dictionary<string, bool>();
    
    // Input Context
    private InputContext currentContext = InputContext.Gameplay;
    private Stack<InputContext> contextStack = new Stack<InputContext>();
    
    // Events
    public static event System.Action<Vector2> OnMoveInput;
    public static event System.Action<bool> OnSprintInput;
    public static event System.Action OnAttackInput;
    public static event System.Action OnDodgeInput;
    public static event System.Action OnInteractInput;
    public static event System.Action OnPauseInput;
    public static event System.Action OnInventoryInput;
    public static event System.Action<Vector2> OnLookInput;
    public static event System.Action OnTargetLockInput;
    
    // UI Events
    public static event System.Action OnUIConfirm;
    public static event System.Action OnUICancel;
    public static event System.Action<Vector2> OnUINavigate;
    
    // Properties
    public InputContext CurrentContext => currentContext;
    public bool IsGameplayInputActive => currentContext == InputContext.Gameplay;
    public bool IsUIInputActive => currentContext == InputContext.UI;

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
        DontDestroyOnLoad(gameObject);
        
        InitializeInputSystem();
    }
    
    void InitializeInputSystem()
    {
        // Cargar Input Actions si no están asignadas
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("Input/PlayerInputActions");
        }
        
        if (inputActions == null)
        {
            Debug.LogError("❌ InputActionAsset not found! Please assign it in the InputManager.");
            return;
        }
        
        // Obtener Action Maps
        playerActionMap = inputActions.FindActionMap("Player");
        uiActionMap = inputActions.FindActionMap("UI");
        debugActionMap = inputActions.FindActionMap("Debug");
        
        SetupPlayerInputs();
        SetupUIInputs();
        SetupDebugInputs();
        
        // Habilitar contexto inicial
        SwitchContext(InputContext.Gameplay);
        
        Debug.Log("🎮 InputManager initialized successfully");
    }
    
    void SetupPlayerInputs()
    {
        if (playerActionMap == null) return;
        
        // Movement
        var moveAction = playerActionMap.FindAction("Move");
        if (moveAction != null)
        {
            moveAction.performed += ctx => HandleMoveInput(ctx.ReadValue<Vector2>());
            moveAction.canceled += ctx => HandleMoveInput(Vector2.zero);
        }
        
        // Sprint
        var sprintAction = playerActionMap.FindAction("Sprint");
        if (sprintAction != null)
        {
            sprintAction.performed += ctx => HandleSprintInput(true);
            sprintAction.canceled += ctx => HandleSprintInput(false);
        }
        
        // Attack
        var attackAction = playerActionMap.FindAction("Attack");
        if (attackAction != null)
        {
            attackAction.performed += ctx => HandleAttackInput();
        }
        
        // Dodge
        var dodgeAction = playerActionMap.FindAction("Dodge");
        if (dodgeAction != null)
        {
            dodgeAction.performed += ctx => HandleDodgeInput();
        }
        
        // Interact
        var interactAction = playerActionMap.FindAction("Interact");
        if (interactAction != null)
        {
            interactAction.performed += ctx => HandleInteractInput();
        }
        
        // Look
        var lookAction = playerActionMap.FindAction("Look");
        if (lookAction != null)
        {
            lookAction.performed += ctx => HandleLookInput(ctx.ReadValue<Vector2>());
            lookAction.canceled += ctx => HandleLookInput(Vector2.zero);
        }
        
        // Target Lock
        var targetLockAction = playerActionMap.FindAction("TargetLock");
        if (targetLockAction != null)
        {
            targetLockAction.performed += ctx => HandleTargetLockInput();
        }
        
        // Pause
        var pauseAction = playerActionMap.FindAction("Pause");
        if (pauseAction != null)
        {
            pauseAction.performed += ctx => HandlePauseInput();
        }
        
        // Inventory
        var inventoryAction = playerActionMap.FindAction("Inventory");
        if (inventoryAction != null)
        {
            inventoryAction.performed += ctx => HandleInventoryInput();
        }
    }
    
    void SetupUIInputs()
    {
        if (uiActionMap == null) return;
        
        // Navigate
        var navigateAction = uiActionMap.FindAction("Navigate");
        if (navigateAction != null)
        {
            navigateAction.performed += ctx => HandleUINavigate(ctx.ReadValue<Vector2>());
        }
        
        // Confirm
        var confirmAction = uiActionMap.FindAction("Submit");
        if (confirmAction != null)
        {
            confirmAction.performed += ctx => HandleUIConfirm();
        }
        
        // Cancel
        var cancelAction = uiActionMap.FindAction("Cancel");
        if (cancelAction != null)
        {
            cancelAction.performed += ctx => HandleUICancel();
        }
    }
    
    void SetupDebugInputs()
    {
        if (debugActionMap == null) return;
        
        // Debug inputs aquí...
        var debugToggleAction = debugActionMap.FindAction("ToggleDebug");
        if (debugToggleAction != null)
        {
            debugToggleAction.performed += ctx => ToggleDebugMode();
        }
    }
    
    #endregion

    #region Input Handlers
    
    void HandleMoveInput(Vector2 input)
    {
        if (currentContext != InputContext.Gameplay) return;
        
        // Aplicar dead zone
        input = ApplyDeadzone(input, ConfigurationManager.Input.gamepadDeadzone);
        
        OnMoveInput?.Invoke(input);
        LogInput("Move", input.ToString());
    }
    
    void HandleSprintInput(bool isPressed)
    {
        if (currentContext != InputContext.Gameplay) return;
        
        OnSprintInput?.Invoke(isPressed);
        LogInput("Sprint", isPressed.ToString());
    }
    
    void HandleAttackInput()
    {
        if (currentContext != InputContext.Gameplay) return;
        
        if (enableInputBuffering)
        {
            BufferInput("Attack");
        }
        
        OnAttackInput?.Invoke();
        LogInput("Attack", "Performed");
    }
    
    void HandleDodgeInput()
    {
        if (currentContext != InputContext.Gameplay) return;
        
        if (enableInputBuffering)
        {
            BufferInput("Dodge");
        }
        
        OnDodgeInput?.Invoke();
        LogInput("Dodge", "Performed");
    }
    
    void HandleInteractInput()
    {
        if (currentContext != InputContext.Gameplay) return;
        
        OnInteractInput?.Invoke();
        LogInput("Interact", "Performed");
    }
    
    void HandleLookInput(Vector2 input)
    {
        if (currentContext != InputContext.Gameplay) return;

        if (ConfigurationManager.Instance == null || ConfigurationManager.Input == null)
        {
            Debug.LogError("❌ ConfigurationManager o su InputConfig están null.");
            return;
        }

        float sensitivity = Gamepad.current != null ?
            ConfigurationManager.Input.gamepadSensitivity :
            ConfigurationManager.Input.mouseSensitivity;

        bool invertY = Gamepad.current != null ?
            ConfigurationManager.Input.invertGamepadY :
            ConfigurationManager.Input.invertMouseY;

        if (invertY)
            input.y = -input.y;

        OnLookInput?.Invoke(input);
    }

    
    void HandleTargetLockInput()
    {
        if (currentContext != InputContext.Gameplay) return;
        
        OnTargetLockInput?.Invoke();
        LogInput("TargetLock", "Performed");
    }
    
    void HandlePauseInput()
    {
        OnPauseInput?.Invoke();
        LogInput("Pause", "Performed");
    }
    
    void HandleInventoryInput()
    {
        if (currentContext != InputContext.Gameplay) return;
        
        OnInventoryInput?.Invoke();
        LogInput("Inventory", "Performed");
    }
    
    void HandleUINavigate(Vector2 input)
    {
        if (currentContext != InputContext.UI) return;
        
        OnUINavigate?.Invoke(input);
    }
    
    void HandleUIConfirm()
    {
        if (currentContext != InputContext.UI) return;
        
        OnUIConfirm?.Invoke();
        LogInput("UI Confirm", "Performed");
    }
    
    void HandleUICancel()
    {
        if (currentContext != InputContext.UI) return;
        
        OnUICancel?.Invoke();
        LogInput("UI Cancel", "Performed");
    }
    
    #endregion

    #region Context Management
    
    public void SwitchContext(InputContext newContext)
    {
        if (currentContext == newContext) return;
        
        // Deshabilitar contexto actual
        DisableCurrentContext();
        
        // Cambiar contexto
        currentContext = newContext;
        
        // Habilitar nuevo contexto
        EnableCurrentContext();
        
        Debug.Log($"🎮 Input context switched to: {newContext}");
    }
    
    public void PushContext(InputContext newContext)
    {
        contextStack.Push(currentContext);
        SwitchContext(newContext);
    }
    
    public void PopContext()
    {
        if (contextStack.Count > 0)
        {
            InputContext previousContext = contextStack.Pop();
            SwitchContext(previousContext);
        }
    }
    
    void DisableCurrentContext()
    {
        switch (currentContext)
        {
            case InputContext.Gameplay:
                playerActionMap?.Disable();
                break;
            case InputContext.UI:
                uiActionMap?.Disable();
                break;
            case InputContext.Debug:
                debugActionMap?.Disable();
                break;
        }
    }
    
    void EnableCurrentContext()
    {
        switch (currentContext)
        {
            case InputContext.Gameplay:
                playerActionMap?.Enable();
                break;
            case InputContext.UI:
                uiActionMap?.Enable();
                break;
            case InputContext.Debug:
                debugActionMap?.Enable();
                break;
        }
    }
    
    #endregion

    #region Input Buffering
    
    void BufferInput(string inputName)
    {
        if (!enableInputBuffering) return;
        
        inputBuffer[inputName] = Time.time;
    }
    
    public bool ConsumeBufferedInput(string inputName)
    {
        if (!enableInputBuffering) return false;
        
        if (inputBuffer.TryGetValue(inputName, out float timestamp))
        {
            if (Time.time - timestamp <= inputBufferTime)
            {
                inputBuffer.Remove(inputName);
                return true;
            }
        }
        
        return false;
    }
    
    public bool IsInputBuffered(string inputName)
    {
        if (!enableInputBuffering) return false;
        
        if (inputBuffer.TryGetValue(inputName, out float timestamp))
        {
            return Time.time - timestamp <= inputBufferTime;
        }
        
        return false;
    }
    
    void Update()
    {
        CleanupExpiredBufferedInputs();
    }
    
    void CleanupExpiredBufferedInputs()
    {
        if (!enableInputBuffering) return;
        
        var keysToRemove = new List<string>();
        
        foreach (var kvp in inputBuffer)
        {
            if (Time.time - kvp.Value > inputBufferTime)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            inputBuffer.Remove(key);
        }
    }
    
    #endregion

    #region Utility Methods
    
    Vector2 ApplyDeadzone(Vector2 input, float deadzone)
    {
        if (input.magnitude < deadzone)
        {
            return Vector2.zero;
        }
        
        // Normalizar fuera de la deadzone
        float magnitude = (input.magnitude - deadzone) / (1f - deadzone);
        return input.normalized * magnitude;
    }
    
    public bool IsUsingGamepad()
    {
        return Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
    }
    
    public bool IsUsingKeyboardMouse()
    {
        return (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame) ||
               (Mouse.current != null && Mouse.current.wasUpdatedThisFrame);
    }
    
    public void SetVibration(float leftMotor, float rightMotor, float duration = 0.2f)
    {
        if (Gamepad.current != null && ConfigurationManager.Input.enableGamepadVibration)
        {
            Gamepad.current.SetMotorSpeeds(leftMotor, rightMotor);
            StartCoroutine(StopVibrationAfterDelay(duration));
        }
    }
    
    IEnumerator StopVibrationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }
    
    void LogInput(string inputName, string value)
    {
        if (debugInputs)
        {
            Debug.Log($"🎮 Input [{inputName}]: {value}");
        }
    }
    
    void ToggleDebugMode()
    {
        debugInputs = !debugInputs;
        Debug.Log($"🔧 Input debug mode: {(debugInputs ? "ON" : "OFF")}");
    }
    
    #endregion

    #region Enable/Disable
    
    void OnEnable()
    {
        EnableCurrentContext();
    }
    
    void OnDisable()
    {
        DisableCurrentContext();
    }
    
    void OnDestroy()
    {
        // Cleanup
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }
    
    #endregion
}

// =====================================================================
// InputContext.cs - Enum para contextos de input
// =====================================================================

public enum InputContext
{
    Gameplay,
    UI,
    Pause,
    Inventory,
    Dialogue,
    Debug,
    Cutscene
}

// =====================================================================
// InputHelper.cs - Clase de utilidades para input
// =====================================================================

public static class InputHelper
{
    public static bool IsActionPressed(string actionName)
    {
        return InputManager.Instance != null && 
               InputManager.Instance.ConsumeBufferedInput(actionName);
    }
    
    public static bool IsActionBuffered(string actionName)
    {
        return InputManager.Instance != null && 
               InputManager.Instance.IsInputBuffered(actionName);
    }
    
    public static void TriggerHapticFeedback(float intensity = 0.5f, float duration = 0.1f)
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetVibration(intensity, intensity, duration);
        }
    }
    
    public static void TriggerHapticFeedback(float leftMotor, float rightMotor, float duration = 0.1f)
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetVibration(leftMotor, rightMotor, duration);
        }
    }
}