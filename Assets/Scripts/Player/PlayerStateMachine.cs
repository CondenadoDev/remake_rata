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
    
    // 🔥 NUEVO: Control de input anti-spam
    private float lastAttackInputTime = -1f;
    private const float ATTACK_INPUT_COOLDOWN = 0.1f;
    
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
    
    // 🔥 NUEVO: Estados que bloquean input/movimiento
    public bool IsInActionState => currentState is DodgingPlayerState || currentState is AttackingPlayerState;
    
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
        
        PlayerState previousState = currentState;
        
        // Salir del estado actual
        currentState?.Exit();
        
        // Cambiar al nuevo estado
        currentState = newState;
        
        // Entrar al nuevo estado
        currentState?.Enter();
        
        // Disparar evento
        OnStateChanged?.Invoke(previousState, currentState);
        
        Debug.Log($"🔄 State: {previousState?.GetType().Name} → {currentState?.GetType().Name}");
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
        movement.SetMoveInput(input);
        currentState?.OnMoveInput(input);
    }
    
    void HandleSprintInput(bool isPressed)
    {
        movement.SetSprinting(isPressed);
        currentState?.OnSprintInput(isPressed);
    }
    
    void HandleAttackInput()
    {
        // 🔥 CORREGIDO: Anti-spam de ataques
        if (Time.time - lastAttackInputTime < ATTACK_INPUT_COOLDOWN)
        {
            return; // Ignorar clicks muy rápidos
        }
        
        lastAttackInputTime = Time.time;
        currentState?.OnAttackInput();
    }
    
    void HandleDodgeInput()
    {
        currentState?.OnDodgeInput();
    }
    
    void HandleInteractInput()
    {
        currentState?.OnInteractInput();
    }
    
    #endregion

    #region Update Loop
    
    void Update()
    {
        currentState?.Update();
        
        // 🔥 CORREGIDO: Solo actualizar movimiento si NO estamos en estados de acción
        if (!IsInActionState)
        {
            movement?.UpdateMovement();
        }
        
        stats?.UpdateStats();
    }
    
    void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }
    
    #endregion

    #region Unity Events
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        InputManager.OnMoveInput -= HandleMoveInput;
        InputManager.OnSprintInput -= HandleSprintInput;
        InputManager.OnAttackInput -= HandleAttackInput;
        InputManager.OnDodgeInput -= HandleDodgeInput;
        InputManager.OnInteractInput -= HandleInteractInput;
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
}
