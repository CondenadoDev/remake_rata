using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("🎯 References")]
    private CharacterController controller;
    private PlayerConfig config;
    private Transform cameraTransform;
    
    // Input
    private Vector2 moveInput;
    private bool isSprinting;
    
    // Movement state
    private Vector3 velocity;
    private Vector3 lastMoveDirection;
    private float currentSpeed;
    
    // 🔥 NUEVO: Control de desaceleración
    private bool forceStop = false;
    
    // Properties
    public Vector2 GetMoveInput() => moveInput;
    public bool IsSprinting => isSprinting;
    public Vector3 GetLastMoveDirection() => lastMoveDirection;
    public float CurrentSpeed => currentSpeed;
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        config = ConfigurationManager.Player;
        
        // Encontrar cámara
        Camera mainCam = Camera.main;
        if (mainCam != null)
            cameraTransform = mainCam.transform;
    }
    
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
        forceStop = false; // Permitir movimiento nuevamente
    }
    
    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
    }
    
    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }
    
    // 🔥 NUEVO: Método para parar completamente el movimiento
    public void StopMovement()
    {
        velocity = Vector3.zero;
        currentSpeed = 0f;
        forceStop = true;
        
        // Aplicar inmediatamente
        if (controller != null)
        {
            // Solo aplicar gravedad
            Vector3 gravityVelocity = new Vector3(0, velocity.y, 0);
            if (!controller.isGrounded)
            {
                gravityVelocity.y += Physics.gravity.y * Time.deltaTime;
            }
            else
            {
                gravityVelocity.y = -2f;
            }
            
            controller.Move(gravityVelocity * Time.deltaTime);
        }
    }
    
    public void UpdateMovement()
    {
        // 🔥 CORREGIDO: Si está forzado a parar, no procesar movimiento
        if (forceStop)
        {
            // Solo aplicar gravedad
            ApplyGravity();
            controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
            return;
        }
        
        // Calcular dirección de movimiento basada en cámara
        Vector3 moveDirection = CalculateMoveDirection();
        
        // Calcular velocidad objetivo
        float targetSpeed = isSprinting ? config.runSpeed : config.walkSpeed;
        currentSpeed = moveInput.magnitude * targetSpeed;
        
        // 🔥 CORREGIDO: Aplicar aceleración/desaceleración mejorada
        if (moveDirection.magnitude > 0.1f)
        {
            // Acelerar hacia la velocidad objetivo
            Vector3 targetVelocity = moveDirection * currentSpeed;
            velocity = Vector3.MoveTowards(velocity, targetVelocity, 
                config.acceleration * Time.deltaTime);
            
            lastMoveDirection = moveDirection.normalized;
            
            // Rotación hacia la dirección de movimiento
            RotateTowardsMovement(moveDirection);
        }
        else
        {
            // 🔥 CORREGIDO: Desaceleración más agresiva
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, 
                config.deceleration * 2f * Time.deltaTime); // Doble velocidad de desaceleración
            
            velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
            currentSpeed = horizontalVelocity.magnitude;
        }
        
        // Aplicar gravedad
        ApplyGravity();
        
        // Mover el personaje
        controller.Move(velocity * Time.deltaTime);
    }
    
    void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f; // Pequeña fuerza para mantener en el suelo
        }
    }
    
    Vector3 CalculateMoveDirection()
    {
        if (cameraTransform == null) return Vector3.zero;
        
        // Obtener direcciones de cámara (sin componente Y)
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        
        camForward.y = 0;
        camRight.y = 0;
        
        camForward.Normalize();
        camRight.Normalize();
        
        // Calcular dirección final
        return camForward * moveInput.y + camRight * moveInput.x;
    }
    
    void RotateTowardsMovement(Vector3 direction)
    {
        if (direction.magnitude > 0.1f && !forceStop)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                config.rotationSpeed * Time.deltaTime);
        }
    }
}