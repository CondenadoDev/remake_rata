using UnityEngine;

public class RealisticMouseTail : MonoBehaviour
{
    [Header("🐭 Configuración de Cola de Ratón")]
    [Tooltip("Transform del personaje/ratón")]
    public Transform characterTransform;
    
    [Tooltip("Rigidbody del personaje")]
    public Rigidbody characterRigidbody;
    
    [Tooltip("Referencia al script de configuración de cola")]
    public TailPhysicsSetup tailSetup;

    [Header("🎯 Comportamiento Natural")]
    [Tooltip("Fuerza que empuja la cola hacia atrás")]
    [Range(50f, 500f)]
    public float backwardForce = 150f;
    
    [Tooltip("Velocidad mínima para aplicar fuerzas direccionales")]
    [Range(0.1f, 5f)]
    public float minVelocityThreshold = 0.5f;
    
    [Tooltip("Resistencia al aire (simula el drag natural de una cola)")]
    [Range(1f, 10f)]
    public float airResistance = 3f;
    
    [Tooltip("Factor de inercia (qué tan rápido reacciona la cola)")]
    [Range(0.1f, 2f)]
    public float inertiaFactor = 0.8f;

    [Header("🎛️ Parámetros Automáticos")]
    [Tooltip("Auto-optimizar al inicio")]
    public bool autoOptimizeOnStart = true;
    
    [Tooltip("Detectar automáticamente el personaje")]
    public bool autoDetectCharacter = true;

    private Transform[] tailBones;
    private Rigidbody[] tailRigidbodies;
    private Vector3 lastCharacterPosition;
    private Vector3 characterVelocity;
    private Vector3 characterDirection;
    private float currentSpeed;

    void Start()
    {
        InitializeComponents();
        
        if (autoOptimizeOnStart)
        {
            OptimizeForMouseBehavior();
        }
        
        SetupRealisticPhysics();
        
        Debug.Log("🐭 Cola de ratón realista configurada y optimizada automáticamente");
    }

    void InitializeComponents()
    {
        // Auto-detectar componentes si no están asignados
        if (autoDetectCharacter)
        {
            if (characterTransform == null)
                characterTransform = transform.parent != null ? transform.parent : transform;
            
            if (characterRigidbody == null)
                characterRigidbody = characterTransform.GetComponent<Rigidbody>();
        }

        if (tailSetup == null)
            tailSetup = GetComponent<TailPhysicsSetup>();

        // Inicializar arrays de huesos
        if (tailSetup != null)
        {
            tailBones = new Transform[tailSetup.boneNames.Length];
            tailRigidbodies = new Rigidbody[tailSetup.boneNames.Length];
            
            for (int i = 0; i < tailSetup.boneNames.Length; i++)
            {
                tailBones[i] = FindBoneRecursive(tailSetup.tailRoot, tailSetup.boneNames[i]);
                if (tailBones[i] != null)
                {
                    tailRigidbodies[i] = tailBones[i].GetComponent<Rigidbody>();
                }
            }
        }

        lastCharacterPosition = characterTransform.position;
    }

    void OptimizeForMouseBehavior()
    {
        Debug.Log("🔧 Optimizando para comportamiento de ratón...");
        
        // Configurar parámetros ideales para cola de ratón
        if (tailSetup != null)
        {
            tailSetup.springForce = 1200f;          // Rigidez moderada
            tailSetup.dampingForce = 80f;           // Amortiguación suave
            tailSetup.angularLimit = 35f;           // Flexibilidad natural
            tailSetup.segmentMass = 0.08f;          // Peso ligero y realista
            tailSetup.colliderRadius = 0.012f;      // Tamaño de cola de ratón
            tailSetup.colliderHeight = 0.035f;
            tailSetup.progressiveFlexibility = true; // Más flexible hacia la punta
            tailSetup.flexibilityFactor = 0.5f;     // Gradiente suave
        }

        // Optimizar configuraciones de physics
        OptimizePhysicsSettings();
    }

    void SetupRealisticPhysics()
    {
        for (int i = 0; i < tailRigidbodies.Length; i++)
        {
            if (tailRigidbodies[i] != null)
            {
                // Configurar masa progresiva (más liviana hacia la punta)
                float massMultiplier = Mathf.Lerp(1f, 0.3f, (float)i / (tailRigidbodies.Length - 1));
                tailRigidbodies[i].mass = tailSetup.segmentMass * massMultiplier;
                
                // Configurar drag realista
                float dragMultiplier = Mathf.Lerp(1f, 1.8f, (float)i / (tailRigidbodies.Length - 1));
                tailRigidbodies[i].linearDamping = 2f * dragMultiplier;
                tailRigidbodies[i].angularDamping = 1.5f * dragMultiplier;
                
                // Configurar interpolación suave
                tailRigidbodies[i].interpolation = RigidbodyInterpolation.Interpolate;
                tailRigidbodies[i].collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
        }
        
        Debug.Log("✅ Física realista configurada");
    }

    void OptimizePhysicsSettings()
    {
        // Configuraciones específicas para cola de animal
        Physics.defaultSolverIterations = 12;
        Physics.defaultSolverVelocityIterations = 8;
        Physics.bounceThreshold = 1f;
        Physics.sleepThreshold = 0.005f;
        Physics.defaultContactOffset = 0.01f;
        
        Debug.Log("⚙️ Configuraciones de física optimizadas para cola de ratón");
    }

    void FixedUpdate()
    {
        UpdateCharacterMovement();
        ApplyNaturalTailBehavior();
    }

    void UpdateCharacterMovement()
    {
        if (characterTransform == null) return;

        // Calcular velocidad y dirección del personaje
        Vector3 currentPosition = characterTransform.position;
        characterVelocity = (currentPosition - lastCharacterPosition) / Time.fixedDeltaTime;
        currentSpeed = characterVelocity.magnitude;
        
        // Filtrar ruido en velocidades muy bajas
        if (currentSpeed > minVelocityThreshold)
        {
            characterDirection = characterVelocity.normalized;
        }
        
        lastCharacterPosition = currentPosition;
    }

    void ApplyNaturalTailBehavior()
    {
        if (tailRigidbodies == null || characterTransform == null) return;

        for (int i = 0; i < tailRigidbodies.Length; i++)
        {
            if (tailRigidbodies[i] == null || tailRigidbodies[i].isKinematic) continue;

            // Calcular fuerzas naturales para cada segmento
            ApplyBackwardDrag(tailRigidbodies[i], i);
            ApplyInertialForces(tailRigidbodies[i], i);
            ApplyAirResistance(tailRigidbodies[i], i);
        }
    }

    void ApplyBackwardDrag(Rigidbody rb, int segmentIndex)
    {
        // Fuerza que empuja la cola hacia atrás del personaje
        if (currentSpeed > minVelocityThreshold)
        {
            Vector3 backwardDirection = -characterDirection;
            float forceMultiplier = Mathf.Lerp(1f, 0.4f, (float)segmentIndex / (tailRigidbodies.Length - 1));
            Vector3 dragForce = backwardDirection * backwardForce * forceMultiplier * currentSpeed;
            
            rb.AddForce(dragForce, ForceMode.Force);
        }
    }

    void ApplyInertialForces(Rigidbody rb, int segmentIndex)
    {
        // Simular inercia natural - la cola "sigue" al cuerpo con retraso
        Vector3 targetVelocity = characterVelocity * inertiaFactor;
        Vector3 velocityDifference = targetVelocity - rb.linearVelocity;
        
        float inertiaStrength = Mathf.Lerp(0.3f, 0.1f, (float)segmentIndex / (tailRigidbodies.Length - 1));
        Vector3 inertiaForce = velocityDifference * inertiaStrength * rb.mass;
        
        rb.AddForce(inertiaForce, ForceMode.Force);
    }

    void ApplyAirResistance(Rigidbody rb, int segmentIndex)
    {
        // Resistencia al aire progresiva (más resistencia hacia la punta)
        float resistanceMultiplier = Mathf.Lerp(1f, 2f, (float)segmentIndex / (tailRigidbodies.Length - 1));
        Vector3 airDrag = -rb.linearVelocity * airResistance * resistanceMultiplier;
        
        rb.AddForce(airDrag, ForceMode.Force);
        
        // Resistencia angular para evitar spinning excesivo
        Vector3 angularDrag = -rb.angularVelocity * airResistance * 0.5f;
        rb.AddTorque(angularDrag, ForceMode.Force);
    }

    Transform FindBoneRecursive(Transform parent, string boneName)
    {
        if (parent.name == boneName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindBoneRecursive(parent.GetChild(i), boneName);
            if (result != null)
                return result;
        }

        return null;
    }

    [ContextMenu("🐭 Optimizar para Ratón")]
    public void OptimizeForMouse()
    {
        OptimizeForMouseBehavior();
        SetupRealisticPhysics();
        Debug.Log("🐭 Cola optimizada para comportamiento de ratón");
    }

    [ContextMenu("🏃 Configurar para Movimiento Rápido")]
    public void SetupForFastMovement()
    {
        backwardForce = 250f;
        airResistance = 4f;
        inertiaFactor = 0.6f;
        minVelocityThreshold = 1f;
        
        // Reconfigurar tailSetup para movimiento rápido
        if (tailSetup != null)
        {
            tailSetup.springForce = 1800f;
            tailSetup.dampingForce = 120f;
            tailSetup.angularLimit = 25f;
        }
        
        SetupRealisticPhysics();
        Debug.Log("🏃 Cola configurada para movimiento rápido");
    }

    [ContextMenu("🧘 Configurar para Movimiento Lento")]
    public void SetupForSlowMovement()
    {
        backwardForce = 100f;
        airResistance = 2f;
        inertiaFactor = 1f;
        minVelocityThreshold = 0.2f;
        
        // Reconfigurar tailSetup para movimiento lento
        if (tailSetup != null)
        {
            tailSetup.springForce = 800f;
            tailSetup.dampingForce = 60f;
            tailSetup.angularLimit = 45f;
        }
        
        SetupRealisticPhysics();
        Debug.Log("🧘 Cola configurada para movimiento lento");
    }

    [ContextMenu("📊 Mostrar Estado del Ratón")]
    public void ShowMouseState()
    {
        Debug.Log("=== ESTADO DE LA COLA DEL RATÓN ===");
        Debug.Log($"Velocidad del personaje: {currentSpeed:F2} m/s");
        Debug.Log($"Dirección: {characterDirection}");
        Debug.Log($"Fuerza hacia atrás: {backwardForce}");
        Debug.Log($"Resistencia al aire: {airResistance}");
        Debug.Log($"Factor de inercia: {inertiaFactor}");
        
        if (tailRigidbodies != null)
        {
            Debug.Log($"Segmentos de cola: {tailRigidbodies.Length}");
            
            for (int i = 0; i < tailRigidbodies.Length; i++)
            {
                if (tailRigidbodies[i] != null)
                {
                    float segmentSpeed = tailRigidbodies[i].linearVelocity.magnitude;
                    Debug.Log($"  {tailSetup.boneNames[i]}: Velocidad={segmentSpeed:F2}, Masa={tailRigidbodies[i].mass:F3}");
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (characterTransform != null && Application.isPlaying)
        {
            // Mostrar dirección del personaje
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(characterTransform.position, characterDirection * 2f);
            
            // Mostrar fuerza hacia atrás
            Gizmos.color = Color.red;
            Gizmos.DrawRay(characterTransform.position, -characterDirection * 1.5f);
            
            // Mostrar velocidad
            Gizmos.color = Color.green;
            Gizmos.DrawRay(characterTransform.position, characterVelocity);
        }
    }
}