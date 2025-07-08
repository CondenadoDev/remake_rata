// PlayerStateMachine.cs
using UnityEngine;
using System.Collections.Generic;
using UISystem.Configuration;

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
            config = ConfigurationManager.Instance.Player;
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
    
    void HandleMoveInput(Vector2 input)
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
        
        // Actualizar sistemas
        movement?.UpdateMovement();
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

// =====================================================================
// PlayerState.cs - Clase base para estados del jugador
// =====================================================================

public abstract class PlayerState
{
    protected PlayerStateMachine player;
    protected PlayerConfig config;
    protected CharacterController controller;
    protected Animator animator;
    protected PlayerMovement movement;
    protected PlayerCombat combat;
    protected PlayerStats stats;
    
    public virtual void Initialize(PlayerStateMachine playerStateMachine)
    {
        player = playerStateMachine;
        config = playerStateMachine.Config;
        controller = playerStateMachine.Controller;
        animator = playerStateMachine.Animator;
        movement = playerStateMachine.Movement;
        combat = playerStateMachine.Combat;
        stats = playerStateMachine.Stats;
    }
    
    public virtual bool CanEnter() { return true; }
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
    
    // Input handlers
    public virtual void OnMoveInput(Vector2 input) { }
    public virtual void OnSprintInput(bool isPressed) { }
    public virtual void OnAttackInput() { }
    public virtual void OnDodgeInput() { }
    public virtual void OnInteractInput() { }
    
    // Collision handlers
    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnTriggerExit(Collider other) { }
}

// =====================================================================
// Estados específicos del jugador
// =====================================================================

public class IdlePlayerState : PlayerState
{
    public override void Enter()
    {
        animator.SetFloat("MoveSpeed", 0f);
        movement.SetVelocity(Vector3.zero);
    }
    
    public override void OnMoveInput(Vector2 input)
    {
        if (input.magnitude > 0.1f)
        {
            player.ChangeState<MovingPlayerState>();
        }
    }
    
    public override void OnAttackInput()
    {
        if (combat.CanAttack())
        {
            player.ChangeState<AttackingPlayerState>();
        }
    }
    
    public override void OnDodgeInput()
    {
        if (combat.CanDodge())
        {
            player.ChangeState<DodgingPlayerState>();
        }
    }
}

public class MovingPlayerState : PlayerState
{
    public override void Enter()
    {
        animator.SetFloat("MoveSpeed", 0.5f);
    }
    
    public override void Update()
    {
        if (movement.GetMoveInput().magnitude < 0.1f)
        {
            player.ChangeState<IdlePlayerState>();
        }
        else if (movement.IsSprinting && stats.CanSprint())
        {
            player.ChangeState<RunningPlayerState>();
        }
    }
    
    public override void OnAttackInput()
    {
        if (combat.CanAttack())
        {
            player.ChangeState<AttackingPlayerState>();
        }
    }
    
    public override void OnDodgeInput()
    {
        if (combat.CanDodge())
        {
            player.ChangeState<DodgingPlayerState>();
        }
    }
}

public class RunningPlayerState : PlayerState
{
    public override void Enter()
    {
        animator.SetFloat("MoveSpeed", 1f);
    }
    
    public override void Update()
    {
        if (movement.GetMoveInput().magnitude < 0.1f)
        {
            player.ChangeState<IdlePlayerState>();
        }
        else if (!movement.IsSprinting || !stats.CanSprint())
        {
            player.ChangeState<MovingPlayerState>();
        }
        
        // Consumir stamina
        stats.ConsumeStamina(config.runStaminaCost * Time.deltaTime);
    }
    
    public override void OnAttackInput()
    {
        if (combat.CanAttack())
        {
            player.ChangeState<AttackingPlayerState>();
        }
    }
    
    public override void OnDodgeInput()
    {
        if (combat.CanDodge())
        {
            player.ChangeState<DodgingPlayerState>();
        }
    }
}

public class DodgingPlayerState : PlayerState
{
    private float dodgeTimer;
    private Vector3 dodgeDirection;
    
    public override bool CanEnter()
    {
        return combat.CanDodge() && stats.CanConsummeStamina(config.dodgeStaminaCost);
    }
    
    public override void Enter()
    {
        dodgeTimer = 0f;
        dodgeDirection = movement.GetLastMoveDirection();
        
        if (dodgeDirection == Vector3.zero)
            dodgeDirection = player.transform.forward;
        
        // Consumir stamina
        stats.ConsumeStamina(config.dodgeStaminaCost);
        
        // Activar invulnerabilidad
        combat.SetInvulnerable(true, config.invulnerabilityDuration);
        
        // Animación
        animator.SetTrigger("Dodge");
        
        // Efectos
        InputHelper.TriggerHapticFeedback(0.3f, 0.1f);
    }
    
    public override void Update()
    {
        dodgeTimer += Time.deltaTime;
        
        // Movimiento de dodge
        float dodgeProgress = dodgeTimer / config.dodgeDuration;
        float speedMultiplier = Mathf.Lerp(1f, 0f, dodgeProgress);
        
        Vector3 dodgeVelocity = dodgeDirection * config.dodgeSpeed * speedMultiplier;
        movement.SetVelocity(dodgeVelocity);
        
        // Terminar dodge
        if (dodgeTimer >= config.dodgeDuration)
        {
            // Volver al estado apropiado
            if (movement.GetMoveInput().magnitude > 0.1f)
            {
                if (movement.IsSprinting && stats.CanSprint())
                    player.ChangeState<RunningPlayerState>();
                else
                    player.ChangeState<MovingPlayerState>();
            }
            else
            {
                player.ChangeState<IdlePlayerState>();
            }
        }
    }
    
    public override void Exit()
    {
        movement.SetVelocity(Vector3.zero);
    }
}

public class AttackingPlayerState : PlayerState
{
    private float attackTimer;
    
    public override bool CanEnter()
    {
        return combat.CanAttack() && stats.CanConsummeStamina(config.attackStaminaCost);
    }
    
    public override void Enter()
    {
        attackTimer = 0f;
        
        // Consumir stamina
        stats.ConsumeStamina(config.attackStaminaCost);
        
        // Ejecutar ataque
        combat.PerformAttack();
        
        // Parar movimiento
        movement.SetVelocity(Vector3.zero);
        
        // Animación
        animator.SetTrigger("Attack");
        
        // Efectos
        InputHelper.TriggerHapticFeedback(0.5f, 0.2f);
    }
    
    public override void Update()
    {
        attackTimer += Time.deltaTime;
        
        if (attackTimer >= config.attackCooldown)
        {
            // Volver al estado apropiado
            if (movement.GetMoveInput().magnitude > 0.1f)
            {
                if (movement.IsSprinting && stats.CanSprint())
                    player.ChangeState<RunningPlayerState>();
                else
                    player.ChangeState<MovingPlayerState>();
            }
            else
            {
                player.ChangeState<IdlePlayerState>();
            }
        }
    }
    
    public override void OnAttackInput()
    {
        // Combo system - podrías expandir esto
        if (attackTimer > config.attackCooldown * 0.5f && combat.CanCombo())
        {
            attackTimer = 0f;
            combat.PerformCombo();
            animator.SetTrigger("Attack");
        }
    }
}

public class StunnedPlayerState : PlayerState
{
    private float stunTimer;
    private float stunDuration;
    
    public void StartStun(float duration)
    {
        stunDuration = duration;
    }
    
    public override void Enter()
    {
        stunTimer = 0f;
        movement.SetVelocity(Vector3.zero);
        animator.SetBool("IsStunned", true);
    }
    
    public override void Update()
    {
        stunTimer += Time.deltaTime;
        
        if (stunTimer >= stunDuration)
        {
            player.ChangeState<IdlePlayerState>();
        }
    }
    
    public override void Exit()
    {
        animator.SetBool("IsStunned", false);
    }
    
    // Durante el stun, ignorar la mayoría de inputs
    public override void OnMoveInput(Vector2 input) { }
    public override void OnAttackInput() { }
    public override void OnDodgeInput() { }
}

public class DeadPlayerState : PlayerState
{
    public override bool CanEnter()
    {
        return stats.CurrentHealth <= 0;
    }
    
    public override void Enter()
    {
        movement.SetVelocity(Vector3.zero);
        animator.SetBool("IsDead", true);
        
        // Notificar muerte
        GameEvents.TriggerPlayerDied();
    }
    
    // Durante la muerte, no procesar inputs
    public override void OnMoveInput(Vector2 input) { }
    public override void OnSprintInput(bool isPressed) { }
    public override void OnAttackInput() { }
    public override void OnDodgeInput() { }
    public override void OnInteractInput() { }
}