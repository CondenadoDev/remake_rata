using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerStateMachine : MonoBehaviour
{
    [Header("🔧 References")]
    [SerializeField] private Transform cameraTransform;
    
    [Header("⚙️ Configuration")]
    [SerializeField] private PlayerConfig config;
    
    [Header("🎨 Visual")]
    [SerializeField] private GameObject visualModel;
    
    // Componentes
    private CharacterController controller;
    private Animator animator;
    private PlayerMovement movement;
    private PlayerCombat combat;
    private PlayerStats stats;
    
    // State Machine
    private Dictionary<System.Type, PlayerState> states;
    private PlayerState currentState;
    
    // 🔥 CORREGIDO: Control de input anti-spam mejorado
    private float lastAttackInputTime = -1f;
    private float lastDodgeInputTime = -1f;
    private const float ATTACK_INPUT_COOLDOWN = 0.2f; // Aumentado para evitar spam
    private const float DODGE_INPUT_COOLDOWN = 0.1f;
    
    // Propiedades públicas
    public CharacterController Controller => controller;
    public Animator Animator => animator;
    public PlayerMovement Movement => movement;
    public PlayerCombat Combat => combat;
    public PlayerStats Stats => stats;
    public PlayerConfig Config => config;
    public Transform CameraTransform => cameraTransform;
    public bool IsGrounded => controller.isGrounded;
    
    // Estado actual
    public PlayerState CurrentState => currentState;
    public System.Type CurrentStateType => currentState?.GetType();
    
    // 🔥 CORREGIDO: Estados que bloquean input/movimiento
    public bool IsInActionState => currentState is DodgingPlayerState || currentState is AttackingPlayerState || currentState is StunnedPlayerState;
    public bool CanReceiveInput => !IsInActionState && currentState is not DeadPlayerState;
    
    // Eventos
    public static event System.Action<PlayerState, PlayerState> OnStateChanged;

    #region Initialization
    
    void Awake()
    {
        InitializeComponents();
        InitializeStates();
        
        // Cargar configuración si no está asignada
        if (config == null)
        {
            config = ConfigurationManager.Player;
        }
    }
    
    void Start()
    {
        // Estado inicial
        ChangeState<IdlePlayerState>();
        
        // Suscribirse a eventos de input
        SubscribeToInputEvents();
        
        Debug.Log("🎮 PlayerStateMachine initialized");
    }
    
    void InitializeComponents()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        // Inicializar sistemas del jugador
        movement = GetComponent<PlayerMovement>() ?? gameObject.AddComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>() ?? gameObject.AddComponent<PlayerCombat>();
        stats = GetComponent<PlayerStats>() ?? gameObject.AddComponent<PlayerStats>();
        
        // Configurar cámara si no está asignada
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraTransform = mainCam.transform;
        }
    }
    
    void InitializeStates()
    {
        states = new Dictionary<System.Type, PlayerState>
        {
            { typeof(IdlePlayerState), new IdlePlayerState() },
            { typeof(MovingPlayerState), new MovingPlayerState() },
            { typeof(RunningPlayerState), new RunningPlayerState() },
            { typeof(DodgingPlayerState), new DodgingPlayerState() },
            { typeof(AttackingPlayerState), new AttackingPlayerState() },
            { typeof(StunnedPlayerState), new StunnedPlayerState() },
            { typeof(DeadPlayerState), new DeadPlayerState() }
        };
        
        // Inicializar cada estado
        foreach (var state in states.Values)
        {
            state.Initialize(this);
        }
    }
    
    void SubscribeToInputEvents()
    {
        InputManager.OnMoveInput += HandleMoveInput;
        InputManager.OnSprintInput += HandleSprintInput;
        InputManager.OnAttackInput += HandleAttackInput;
        InputManager.OnDodgeInput += HandleDodgeInput;
        InputManager.OnInteractInput += HandleInteractInput;
    }
    
    #endregion

    #region State Management
    
    public void ChangeState<T>() where T : PlayerState
    {
        if (states.TryGetValue(typeof(T), out PlayerState newState))
        {
            ChangeState(newState);
        }
        else
        {
            Debug.LogError($"❌ State {typeof(T)} not found!");
        }
    }
    
    public void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;
        
        // 🔥 NUEVO: Verificar si puede entrar al estado
        if (!newState.CanEnter())
        {
            Debug.LogWarning($"⚠️ Cannot enter state {newState.GetType().Name}");
            return;
        }
        
        PlayerState previousState = currentState;
        
        // Salir del estado actual
        currentState?.Exit();
        
        // Cambiar al nuevo estado
        currentState = newState;
        
        // Entrar al nuevo estado
        currentState?.Enter();
        
        // Disparar evento
        OnStateChanged?.Invoke(previousState, currentState);
        
        Debug.Log($"🔄 State: {previousState?.GetType().Name ?? "NULL"} → {currentState?.GetType().Name}");
    }
    
    public bool CanChangeToState<T>() where T : PlayerState
    {
        if (states.TryGetValue(typeof(T), out PlayerState state))
        {
            return state.CanEnter();
        }
        return false;
    }
    
    public bool IsInState<T>() where T : PlayerState
    {
        return currentState is T;
    }
    
    #endregion

    #region Input Handlers
    
    void HandleMoveInput(UnityEngine.Vector2 input)
    {
        // 🔥 CORREGIDO: Solo procesar input de movimiento si puede recibirlo
        if (!CanReceiveInput && IsInActionState)
        {
            return;
        }
        
        movement.SetMoveInput(input);
        currentState?.OnMoveInput(input);
    }
    
    void HandleSprintInput(bool isPressed)
    {
        // 🔥 CORREGIDO: Solo procesar sprint si puede recibirlo
        if (!CanReceiveInput)
        {
            return;
        }
        
        movement.SetSprinting(isPressed);
        currentState?.OnSprintInput(isPressed);
    }
    
    void HandleAttackInput()
    {
        // 🔥 CORREGIDO: Mejor anti-spam de ataques
        if (!CanReceiveInput || Time.time - lastAttackInputTime < ATTACK_INPUT_COOLDOWN)
        {
            Debug.Log("⚠️ Attack input blocked (cooldown or invalid state)");
            return;
        }
        
        lastAttackInputTime = Time.time;
        currentState?.OnAttackInput();
    }
    
    void HandleDodgeInput()
    {
        // 🔥 CORREGIDO: Anti-spam para dodge también
        if (!CanReceiveInput || Time.time - lastDodgeInputTime < DODGE_INPUT_COOLDOWN)
        {
            Debug.Log("⚠️ Dodge input blocked (cooldown or invalid state)");
            return;
        }
        
        lastDodgeInputTime = Time.time;
        currentState?.OnDodgeInput();
    }
    
    void HandleInteractInput()
    {
        if (!CanReceiveInput)
        {
            return;
        }
        
        currentState?.OnInteractInput();
    }
    
    #endregion

    #region Update Loop
    
    void Update()
    {
        // 🔥 CORREGIDO: Verificar si el jugador está muerto
        if (stats.CurrentHealth <= 0 && !IsInState<DeadPlayerState>())
        {
            ChangeState<DeadPlayerState>();
            return;
        }
        
        currentState?.Update();
        
        // 🔥 CORREGIDO: Control de movimiento más granular
        if (CanReceiveInput && !IsInActionState)
        {
            movement?.UpdateMovement();
        }
        else if (IsInActionState)
        {
            // Durante acciones, asegurar que el movimiento esté parado
            movement?.StopMovement();
        }
        
        stats?.UpdateStats();
    }
    
    void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }
    
    #endregion

    #region Utility Methods
    
    // 🔥 NUEVO: Método para forzar parada inmediata
    public void ForceStop()
    {
        movement?.StopMovement();
        
        // Resetear parámetros de animación
        if (animator != null)
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
            animator.SetFloat("MoveSpeed", 0f);
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);
        }
    }
    
    // 🔥 NUEVO: Método para obtener información de estado
    public string GetStateInfo()
    {
        return $"Current State: {currentState?.GetType().Name ?? "NULL"}\n" +
               $"Can Receive Input: {CanReceiveInput}\n" +
               $"Is In Action: {IsInActionState}\n" +
               $"Health: {stats.CurrentHealth:F1}/{stats.MaxHealth}\n" +
               $"Stamina: {stats.CurrentStamina:F1}/{stats.MaxStamina}";
    }
    
    // 🔥 NUEVO: Método para debug
    [ContextMenu("Debug State Info")]
    void DebugStateInfo()
    {
        Debug.Log(GetStateInfo());
    }
    
    #endregion

    #region Unity Events
    
    void OnDestroy()
    {
        // 🔥 CORREGIDO: Verificar que InputManager existe antes de desuscribirse
        if (InputManager.Instance != null)
        {
            InputManager.OnMoveInput -= HandleMoveInput;
            InputManager.OnSprintInput -= HandleSprintInput;
            InputManager.OnAttackInput -= HandleAttackInput;
            InputManager.OnDodgeInput -= HandleDodgeInput;
            InputManager.OnInteractInput -= HandleInteractInput;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        currentState?.OnTriggerEnter(other);
    }
    
    void OnTriggerExit(Collider other)
    {
        currentState?.OnTriggerExit(other);
    }
    
    #endregion

    #region Debug Visualization
    
    void OnDrawGizmosSelected()
    {
        // Mostrar información del estado actual
        if (currentState != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 textPos = transform.position + Vector3.up * 3f;
            
            // Nota: En una implementación real, usarías un sistema de UI para mostrar esto
            // Aquí solo mostramos en la consola cuando se selecciona
        }
    }
    
    #endregion
}