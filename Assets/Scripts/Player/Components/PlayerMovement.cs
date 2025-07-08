using UISystem.Configuration;
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
    }
    
    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
    }
    
    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }
    
    public void UpdateMovement()
    {
        // Calcular dirección de movimiento basada en cámara
        Vector3 moveDirection = CalculateMoveDirection();
        
        // Calcular velocidad objetivo
        float targetSpeed = isSprinting ? config.runSpeed : config.walkSpeed;
        currentSpeed = moveInput.magnitude * targetSpeed;
        
        // Aplicar aceleración/desaceleración
        if (moveDirection.magnitude > 0.1f)
        {
            velocity = Vector3.MoveTowards(velocity, moveDirection * currentSpeed, 
                config.acceleration * Time.deltaTime);
            lastMoveDirection = moveDirection.normalized;
            
            // Rotación hacia la dirección de movimiento
            RotateTowardsMovement(moveDirection);
        }
        else
        {
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, 
                config.deceleration * Time.deltaTime);
        }
        
        // Aplicar gravedad
        if (!controller.isGrounded)
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f; // Pequeña fuerza para mantener en el suelo
        }
        
        // Mover el personaje
        controller.Move(velocity * Time.deltaTime);
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
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                config.rotationSpeed * Time.deltaTime);
        }
    }
}