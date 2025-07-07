using UnityEngine;
using System.Collections;

[System.Serializable]
public enum MouseMovementStyle
{
    Slow,       // Ratón que se mueve lentamente
    Normal,     // Movimiento estándar
    Fast,       // Ratón ágil y rápido
    Automatic   // Detecta automáticamente basado en la velocidad
}

public class AutoMouseTailSetup : MonoBehaviour
{
    [Header("🐭 Configuración Automática de Cola de Ratón")]
    [Tooltip("Estilo de movimiento del ratón")]
    public MouseMovementStyle movementStyle = MouseMovementStyle.Automatic;
    
    [Tooltip("Configurar automáticamente al inicio")]
    public bool autoSetupOnStart = true;
    
    [Tooltip("Tiempo de espera antes de la configuración automática")]
    [Range(0f, 2f)]
    public float setupDelay = 0.1f;

    [Header("🎯 Referencias (Auto-detectadas si están vacías)")]
    public Transform characterTransform;
    public Rigidbody characterRigidbody;
    public Transform tailRoot;

    [Header("📊 Monitoreo Automático")]
    [Tooltip("Reconfigurar automáticamente si cambia el estilo de movimiento")]
    public bool adaptiveReconfiguration = true;
    
    [Tooltip("Tiempo entre verificaciones automáticas")]
    [Range(1f, 10f)]
    public float checkInterval = 3f;

    // Componentes que se configurarán automáticamente
    private TailPhysicsSetup tailPhysics;
    private RealisticMouseTail mouseTail;
    private TailStretchPreventer stretchPreventer;
    private TailJointStabilizer jointStabilizer;

    // Variables para detección automática
    private Vector3 lastPosition;
    private float averageSpeed;
    private float speedSampleCount;
    private MouseMovementStyle detectedStyle;

    void Start()
    {
        if (autoSetupOnStart)
        {
            StartCoroutine(AutoSetupSequence());
        }
    }

    IEnumerator AutoSetupSequence()
    {
        Debug.Log("🐭 === CONFIGURACIÓN AUTOMÁTICA DE COLA DE RATÓN ===");
        
        // Esperar un frame para asegurar que otros componentes estén inicializados
        yield return new WaitForSeconds(setupDelay);
        
        // Paso 1: Auto-detectar componentes
        AutoDetectComponents();
        yield return null;
        
        // Paso 2: Configurar o crear componentes necesarios
        SetupRequiredComponents();
        yield return null;
        
        // Paso 3: Configurar según el estilo de movimiento
        ConfigureForMovementStyle();
        yield return null;
        
        // Paso 4: Optimización final
        FinalOptimization();
        yield return null;
        
        Debug.Log("✅ Cola de ratón configurada automáticamente");
        
        // Iniciar monitoreo adaptativo si está habilitado
        if (adaptiveReconfiguration && movementStyle == MouseMovementStyle.Automatic)
        {
            InvokeRepeating(nameof(MonitorAndAdapt), checkInterval, checkInterval);
        }
    }

    void AutoDetectComponents()
    {
        Debug.Log("🔍 Auto-detectando componentes...");
        
        // Detectar character transform
        if (characterTransform == null)
        {
            // Buscar en padres
            Transform current = transform.parent;
            while (current != null)
            {
                Rigidbody rb = current.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    characterTransform = current;
                    characterRigidbody = rb;
                    break;
                }
                current = current.parent;
            }
            
            // Si no encuentra, usar el transform actual
            if (characterTransform == null)
            {
                characterTransform = transform;
                characterRigidbody = GetComponent<Rigidbody>();
            }
        }

        // Detectar tail root
        if (tailRoot == null)
        {
            // Buscar objeto llamado "BASECOLA" o similar
            tailRoot = transform.Find("BASECOLA");
            if (tailRoot == null)
            {
                // Buscar en hijos
                tailRoot = GetComponentInChildren<Transform>();
                if (tailRoot == transform) tailRoot = null;
            }
        }

        // Detectar rigidbody si no está asignado
        if (characterRigidbody == null && characterTransform != null)
        {
            characterRigidbody = characterTransform.GetComponent<Rigidbody>();
        }

        Debug.Log($"✅ Detectados - Character: {characterTransform?.name}, TailRoot: {tailRoot?.name}");
    }

    void SetupRequiredComponents()
    {
        Debug.Log("⚙️ Configurando componentes necesarios...");
        
        // TailPhysicsSetup
        tailPhysics = GetComponent<TailPhysicsSetup>();
        if (tailPhysics == null)
        {
            tailPhysics = gameObject.AddComponent<TailPhysicsSetup>();
            Debug.Log("➕ TailPhysicsSetup añadido");
        }
        
        // Configurar TailPhysicsSetup
        if (tailRoot != null)
        {
            tailPhysics.tailRoot = tailRoot;
        }
        if (characterRigidbody != null)
        {
            // Asegurar que el character rigidbody sea kinematic si no lo es
            if (!characterRigidbody.isKinematic)
            {
                Debug.Log("⚠️ Character Rigidbody debe ser kinematic para la cola. Configurando...");
                characterRigidbody.isKinematic = true;
            }
            tailPhysics.connectedTo = characterRigidbody;
        }

        // RealisticMouseTail
        mouseTail = GetComponent<RealisticMouseTail>();
        if (mouseTail == null)
        {
            mouseTail = gameObject.AddComponent<RealisticMouseTail>();
            Debug.Log("➕ RealisticMouseTail añadido");
        }
        
        // Configurar RealisticMouseTail
        mouseTail.characterTransform = characterTransform;
        mouseTail.characterRigidbody = characterRigidbody;
        mouseTail.tailSetup = tailPhysics;
        mouseTail.autoOptimizeOnStart = false; // Lo haremos nosotros

        // TailStretchPreventer
        stretchPreventer = GetComponent<TailStretchPreventer>();
        if (stretchPreventer == null)
        {
            stretchPreventer = gameObject.AddComponent<TailStretchPreventer>();
            Debug.Log("➕ TailStretchPreventer añadido");
        }
        stretchPreventer.tailSetup = tailPhysics;

        // TailJointStabilizer
        jointStabilizer = GetComponent<TailJointStabilizer>();
        if (jointStabilizer == null)
        {
            jointStabilizer = gameObject.AddComponent<TailJointStabilizer>();
            Debug.Log("➕ TailJointStabilizer añadido");
        }
        jointStabilizer.tailSetup = tailPhysics;
    }

    void ConfigureForMovementStyle()
    {
        Debug.Log($"🎛️ Configurando para estilo: {movementStyle}");
        
        MouseMovementStyle styleToUse = movementStyle;
        
        // Si es automático, detectar basado en el rigidbody del personaje
        if (movementStyle == MouseMovementStyle.Automatic)
        {
            styleToUse = DetectMovementStyle();
        }

        switch (styleToUse)
        {
            case MouseMovementStyle.Slow:
                ConfigureSlowMouse();
                break;
            case MouseMovementStyle.Normal:
                ConfigureNormalMouse();
                break;
            case MouseMovementStyle.Fast:
                ConfigureFastMouse();
                break;
        }
        
        detectedStyle = styleToUse;
        Debug.Log($"✅ Configurado para: {styleToUse}");
    }

    MouseMovementStyle DetectMovementStyle()
    {
        if (characterRigidbody == null) return MouseMovementStyle.Normal;

        float mass = characterRigidbody.mass;
        float drag = characterRigidbody.linearDamping;
        
        // Heurística simple basada en mass y drag
        if (mass > 2f || drag > 5f)
            return MouseMovementStyle.Slow;
        else if (mass < 0.5f || drag < 1f)
            return MouseMovementStyle.Fast;
        else
            return MouseMovementStyle.Normal;
    }

    void ConfigureSlowMouse()
    {
        // Configuración para ratón lento/pesado
        if (tailPhysics != null)
        {
            tailPhysics.springForce = 600f;
            tailPhysics.dampingForce = 40f;
            tailPhysics.angularLimit = 50f;
            tailPhysics.segmentMass = 0.12f;
        }

        if (mouseTail != null)
        {
            mouseTail.backwardForce = 80f;
            mouseTail.airResistance = 1.5f;
            mouseTail.inertiaFactor = 1.2f;
            mouseTail.minVelocityThreshold = 0.2f;
        }

        if (stretchPreventer != null)
        {
            stretchPreventer.maxSegmentDistance = 0.08f;
            stretchPreventer.constraintForce = 1500f;
        }
    }

    void ConfigureNormalMouse()
    {
        // Configuración estándar para ratón normal
        if (tailPhysics != null)
        {
            tailPhysics.springForce = 1200f;
            tailPhysics.dampingForce = 80f;
            tailPhysics.angularLimit = 35f;
            tailPhysics.segmentMass = 0.08f;
        }

        if (mouseTail != null)
        {
            mouseTail.backwardForce = 150f;
            mouseTail.airResistance = 3f;
            mouseTail.inertiaFactor = 0.8f;
            mouseTail.minVelocityThreshold = 0.5f;
        }

        if (stretchPreventer != null)
        {
            stretchPreventer.maxSegmentDistance = 0.05f;
            stretchPreventer.constraintForce = 2500f;
        }
    }

    void ConfigureFastMouse()
    {
        // Configuración para ratón rápido/ágil
        if (tailPhysics != null)
        {
            tailPhysics.springForce = 2000f;
            tailPhysics.dampingForce = 120f;
            tailPhysics.angularLimit = 25f;
            tailPhysics.segmentMass = 0.06f;
        }

        if (mouseTail != null)
        {
            mouseTail.backwardForce = 250f;
            mouseTail.airResistance = 4f;
            mouseTail.inertiaFactor = 0.6f;
            mouseTail.minVelocityThreshold = 1f;
        }

        if (stretchPreventer != null)
        {
            stretchPreventer.maxSegmentDistance = 0.03f;
            stretchPreventer.constraintForce = 4000f;
        }

        if (jointStabilizer != null)
        {
            jointStabilizer.maxJointForce = 30000f;
            jointStabilizer.maxSegmentVelocity = 25f;
        }
    }

    void FinalOptimization()
    {
        Debug.Log("🔧 Aplicando optimización final...");
        
        // Optimizar configuraciones de física
        Physics.defaultSolverIterations = 12;
        Physics.defaultSolverVelocityIterations = 8;
        Physics.bounceThreshold = 1f;
        
        // Aplicar configuraciones
        if (mouseTail != null)
        {
            mouseTail.OptimizeForMouse();
        }
        
        Debug.Log("✅ Optimización final completada");
    }

    void MonitorAndAdapt()
    {
        if (characterTransform == null) return;

        // Calcular velocidad promedio
        Vector3 currentPos = characterTransform.position;
        float currentSpeed = Vector3.Distance(currentPos, lastPosition) / checkInterval;
        
        // Actualizar promedio
        averageSpeed = (averageSpeed * speedSampleCount + currentSpeed) / (speedSampleCount + 1);
        speedSampleCount = Mathf.Min(speedSampleCount + 1, 10); // Máximo 10 muestras
        
        lastPosition = currentPos;

        // Detectar si necesita reconfiguración
        MouseMovementStyle newStyle = DetectStyleFromSpeed(averageSpeed);
        
        if (newStyle != detectedStyle)
        {
            Debug.Log($"🔄 Cambio de estilo detectado: {detectedStyle} -> {newStyle}");
            movementStyle = newStyle;
            ConfigureForMovementStyle();
        }
    }

    MouseMovementStyle DetectStyleFromSpeed(float speed)
    {
        if (speed < 1f) return MouseMovementStyle.Slow;
        else if (speed > 5f) return MouseMovementStyle.Fast;
        else return MouseMovementStyle.Normal;
    }

    [ContextMenu("🔄 Reconfigurar Automáticamente")]
    public void ReconfigureAutomatically()
    {
        StartCoroutine(AutoSetupSequence());
    }

    [ContextMenu("📊 Mostrar Estado Completo")]
    public void ShowCompleteStatus()
    {
        Debug.Log("=== ESTADO COMPLETO DE LA COLA DEL RATÓN ===");
        Debug.Log($"Estilo de movimiento: {movementStyle} (Detectado: {detectedStyle})");
        Debug.Log($"Velocidad promedio: {averageSpeed:F2} m/s");
        Debug.Log($"Character: {characterTransform?.name}");
        Debug.Log($"TailRoot: {tailRoot?.name}");
        Debug.Log($"Componentes activos:");
        Debug.Log($"  - TailPhysicsSetup: {tailPhysics != null}");
        Debug.Log($"  - RealisticMouseTail: {mouseTail != null}");
        Debug.Log($"  - TailStretchPreventer: {stretchPreventer != null}");
        Debug.Log($"  - TailJointStabilizer: {jointStabilizer != null}");
    }
}