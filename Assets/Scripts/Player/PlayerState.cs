using UnityEngine;

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
    public virtual void OnMoveInput(UnityEngine.Vector2 input) { }
    public virtual void OnSprintInput(bool isPressed) { }
    public virtual void OnAttackInput() { }
    public virtual void OnDodgeInput() { }
    public virtual void OnInteractInput() { }
    
    // Collision handlers
    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnTriggerExit(Collider other) { }
}

// =====================================================================
// Estados específicos del jugador - CORREGIDOS
// =====================================================================

public class IdlePlayerState : PlayerState
{
    public override void Enter()
    {
        animator.SetFloat("MoveX", 0f, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", 0f, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveSpeed", 0f, 0.1f, Time.deltaTime);
        movement.SetVelocity(UnityEngine.Vector3.zero);
    }
    
    public override void OnMoveInput(UnityEngine.Vector2 input)
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
        // 🔥 CORREGIDO: Verificar stamina específicamente para dodge
        if (combat.CanDodge() && stats.CanConsummeStamina(config.dodgeStaminaCost))
        {
            player.ChangeState<DodgingPlayerState>();
        }
        else
        {
            Debug.Log("⚠️ No hay suficiente stamina para dodge!");
        }
    }
}

public class MovingPlayerState : PlayerState
{
    public override void Enter()
    {
        UpdateAnimationParameters();
    }
    
    public override void Update()
    {
        UnityEngine.Vector2 moveInput = movement.GetMoveInput();
        
        if (moveInput.magnitude < 0.1f)
        {
            player.ChangeState<IdlePlayerState>();
        }
        // 🔥 CORREGIDO: Usar CanStartSprinting() para empezar a correr
        else if (movement.IsSprinting && stats.CanStartSprinting())
        {
            player.ChangeState<RunningPlayerState>();
        }
        
        UpdateAnimationParameters();
    }
    
    void UpdateAnimationParameters()
    {
        UnityEngine.Vector2 moveInput = movement.GetMoveInput();
        animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveInput.y, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveSpeed", moveInput.magnitude * 0.5f, 0.1f, Time.deltaTime);
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
        // 🔥 CORREGIDO: Verificar stamina específicamente para dodge
        if (combat.CanDodge() && stats.CanConsummeStamina(config.dodgeStaminaCost))
        {
            player.ChangeState<DodgingPlayerState>();
        }
        else
        {
            Debug.Log("⚠️ No hay suficiente stamina para dodge!");
        }
    }
}

public class RunningPlayerState : PlayerState
{
    public override void Enter()
    {
        UpdateAnimationParameters();
    }
    
    public override void Update()
    {
        UnityEngine.Vector2 moveInput = movement.GetMoveInput();
        
        if (moveInput.magnitude < 0.1f)
        {
            player.ChangeState<IdlePlayerState>();
        }
        // 🔥 CORREGIDO: Parar inmediatamente si no tiene stamina O si se queda sin stamina
        else if (!movement.IsSprinting || stats.IsStaminaEmpty() || !stats.CanSprint())
        {
            player.ChangeState<MovingPlayerState>();
        }
        else
        {
            // Solo consumir stamina si realmente está corriendo
            stats.ConsumeStamina(config.runStaminaCost * Time.deltaTime);
        }
        
        UpdateAnimationParameters();
    }
    
    void UpdateAnimationParameters()
    {
        UnityEngine.Vector2 moveInput = movement.GetMoveInput();
        animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveInput.y, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveSpeed", 1f, 0.1f, Time.deltaTime);
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
        // 🔥 CORREGIDO: Verificar stamina específicamente para dodge
        if (combat.CanDodge() && stats.CanConsummeStamina(config.dodgeStaminaCost))
        {
            player.ChangeState<DodgingPlayerState>();
        }
        else
        {
            Debug.Log("⚠️ No hay suficiente stamina para dodge!");
        }
    }
}

public class DodgingPlayerState : PlayerState
{
    private float dodgeTimer;
    private UnityEngine.Vector3 dodgeDirection;
    
    public override bool CanEnter()
    {
        // 🔥 CORREGIDO: Verificación más estricta de stamina para dodge
        bool hasStamina = stats.CanConsummeStamina(config.dodgeStaminaCost);
        bool canDodgeCooldown = combat.CanDodge();
        
        if (!hasStamina)
        {
            Debug.Log($"⚠️ Dash bloqueado: Stamina actual {stats.CurrentStamina:F1}, necesaria {config.dodgeStaminaCost}");
        }
        
        return canDodgeCooldown && hasStamina;
    }
    
    public override void Enter()
    {
        dodgeTimer = 0f;
        dodgeDirection = movement.GetLastMoveDirection();
        
        if (dodgeDirection == UnityEngine.Vector3.zero)
            dodgeDirection = player.transform.forward;
        
        // Consumir stamina
        stats.ConsumeStamina(config.dodgeStaminaCost);
        
        // Activar invulnerabilidad
        combat.SetInvulnerable(true, config.invulnerabilityDuration);
        
        // Animación
        animator.SetBool("IsDashing", true);
        
        // Bloquear rotación del personaje
        movement.SetVelocity(UnityEngine.Vector3.zero);
        
        // Efectos
        InputHelper.TriggerHapticFeedback(0.3f, 0.1f);
        
        Debug.Log($"🚶‍♂️ Dodge started! Stamina restante: {stats.CurrentStamina:F1}");
    }
    
    public override void Update()
    {
        dodgeTimer += Time.deltaTime;
        
        // Velocidad CONSTANTE durante todo el dash
        UnityEngine.Vector3 dodgeVelocity = dodgeDirection * config.dodgeSpeed;
        controller.Move(dodgeVelocity * Time.deltaTime);
        
        // Terminar dodge
        if (dodgeTimer >= config.dodgeDuration)
        {
            // Volver al estado apropiado
            if (movement.GetMoveInput().magnitude > 0.1f)
            {
                // 🔥 CORREGIDO: Usar CanStartSprinting() después del dodge
                if (movement.IsSprinting && stats.CanStartSprinting())
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
        animator.SetBool("IsDashing", false);
        movement.SetVelocity(UnityEngine.Vector3.zero);
        Debug.Log("🚶‍♂️ Dodge ended!");
    }
    
    // Durante el dash, ignorar TODOS los inputs
    public override void OnMoveInput(UnityEngine.Vector2 input) { }
    public override void OnAttackInput() { }
    public override void OnDodgeInput() { }
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
        
        // 🔥 CORREGIDO: Parar completamente el movimiento y bloquear rotación
        movement.SetVelocity(UnityEngine.Vector3.zero);
        
        // Animación
        animator.SetTrigger("Attack");
        
        // Efectos
        InputHelper.TriggerHapticFeedback(0.5f, 0.2f);
        
        Debug.Log("⚔️ Attack started!");
    }
    
    public override void Update()
    {
        attackTimer += Time.deltaTime;
        
        // 🔥 CORREGIDO: Mantener al personaje quieto durante todo el ataque
        movement.SetVelocity(UnityEngine.Vector3.zero);
        
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
    
    public override void Exit()
    {
        Debug.Log("⚔️ Attack ended!");
    }
    
    // 🔥 CORREGIDO: NO permitir combos ni spam de ataques
    public override void OnAttackInput() 
    { 
        // Ignorar completamente - un ataque a la vez
    }
    
    // 🔥 CORREGIDO: Bloquear movimiento durante ataque
    public override void OnMoveInput(UnityEngine.Vector2 input) { }
    public override void OnDodgeInput() { }
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
        movement.SetVelocity(UnityEngine.Vector3.zero);
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
    public override void OnMoveInput(UnityEngine.Vector2 input) { }
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
        movement.SetVelocity(UnityEngine.Vector3.zero);
        animator.SetBool("IsDead", true);
        
        // Notificar muerte
        GameEvents.TriggerPlayerDied();
    }
    
    // Durante la muerte, no procesar inputs
    public override void OnMoveInput(UnityEngine.Vector2 input) { }
    public override void OnSprintInput(bool isPressed) { }
    public override void OnAttackInput() { }
    public override void OnDodgeInput() { }
    public override void OnInteractInput() { }
}