using UnityEngine;

public class TailPhysicsSetup : MonoBehaviour
{
    [Tooltip("Raíz de la cola (el primer hueso)")]
    public Transform tailRoot;

    [Tooltip("Lista de nombres exactos de los huesos de la cola")]
    public string[] boneNames = {
        "Cola_1", "Cola_2", "Cola_3", "Cola_4", "Cola_5", 
        "Cola_6", "Cola_7", "Cola_8"  // ✅ Corregidos a mayúsculas
    };

    [Tooltip("Cuerpo al que se conecta la cola (ej: Hips)")]
    public Rigidbody connectedTo;

    [Header("🎛️ Configuración de Física")]
    [Tooltip("Límite angular en grados (recomendado: 15-45)")]
    [Range(5f, 90f)]
    public float angularLimit = 30f;
    
    [Tooltip("Rigidez de la cola (mayor = más rígida)")]
    [Range(100f, 5000f)]
    public float springForce = 800f;
    
    [Tooltip("Amortiguación (mayor = menos rebote)")]
    [Range(10f, 200f)]
    public float dampingForce = 80f;
    
    [Header("⚙️ Configuración Avanzada")]
    [Tooltip("Masa de cada segmento de cola")]
    [Range(0.05f, 1f)]
    public float segmentMass = 0.15f;
    
    [Tooltip("Radio del collider")]
    [Range(0.005f, 0.05f)]
    public float colliderRadius = 0.015f;
    
    [Tooltip("Altura del collider")]
    [Range(0.02f, 0.1f)]
    public float colliderHeight = 0.04f;
    
    [Header("🎨 Comportamiento Visual")]
    [Tooltip("Hacer la cola más flexible hacia la punta")]
    public bool progressiveFlexibility = true;
    
    [Tooltip("Factor de reducción de rigidez hacia la punta")]
    [Range(0.1f, 1f)]
    public float flexibilityFactor = 0.6f;

    void Start()
    {
        if (tailRoot == null || connectedTo == null)
        {
            Debug.LogError("TailPhysicsSetup: tailRoot o connectedTo no asignados.");
            return;
        }

        SetupTailPhysics();
        ConfigurePhysicsSettings();
    }

    void SetupTailPhysics()
    {
        Debug.Log("=== CONFIGURANDO FÍSICA DE COLA ===");
        
        Rigidbody previous = connectedTo;

        for (int i = 0; i < boneNames.Length; i++)
        {
            string boneName = boneNames[i];
            Transform bone = FindBoneRecursive(tailRoot, boneName);
            
            if (bone == null)
            {
                Debug.LogWarning($"❌ No se encontró: '{boneName}'");
                continue;
            }

            GameObject boneObj = bone.gameObject;
            
            // Calcular valores progresivos para flexibilidad natural
            float progressFactor = progressiveFlexibility ? 
                Mathf.Lerp(1f, flexibilityFactor, (float)i / (boneNames.Length - 1)) : 1f;
            
            float currentSpring = springForce * progressFactor;
            float currentDamping = dampingForce * progressFactor;
            float currentMass = segmentMass * (1f + (float)i * 0.1f); // Masa ligeramente creciente
            
            Debug.Log($"✅ Configurando '{boneName}' - Spring: {currentSpring:F0}, Damping: {currentDamping:F0}");

            // Configurar Rigidbody
            Rigidbody rb = SetupRigidbody(boneObj, currentMass);
            
            // Configurar Collider
            SetupCollider(boneObj);
            
            // Configurar Joint
            SetupJoint(boneObj, previous, currentSpring, currentDamping, i);

            previous = rb;
        }
        
        Debug.Log("🎉 Configuración de cola completada!");
    }

    Rigidbody SetupRigidbody(GameObject obj, float mass)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) rb = obj.AddComponent<Rigidbody>();
        
        rb.mass = mass;
        rb.linearDamping = 1.2f;              // Mayor drag para movimiento más suave
        rb.angularDamping = 0.8f;       // Reducir spinning excesivo
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // Más eficiente
        rb.isKinematic = false;
        
        return rb;
    }

    void SetupCollider(GameObject obj)
    {
        CapsuleCollider col = obj.GetComponent<CapsuleCollider>();
        if (col == null) col = obj.AddComponent<CapsuleCollider>();
        
        col.radius = colliderRadius;
        col.height = colliderHeight;
        col.direction = 2; // Z-axis (ajustar según orientación)
        col.isTrigger = false;
    }

    void SetupJoint(GameObject obj, Rigidbody connectedBody, float spring, float damping, int segmentIndex)
    {
        ConfigurableJoint joint = obj.GetComponent<ConfigurableJoint>();
        if (joint == null) joint = obj.AddComponent<ConfigurableJoint>();
        
        // Conexión
        joint.connectedBody = connectedBody;
        
        // Bloquear movimiento lineal
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        
        // Permitir rotación limitada
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        // Límites angulares progresivos (más flexibilidad hacia la punta)
        float currentLimit = angularLimit + (segmentIndex * 5f); // Aumenta 5° por segmento
        
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = currentLimit;
        
        joint.lowAngularXLimit = limit;
        joint.highAngularXLimit = limit;
        joint.angularYLimit = limit;
        joint.angularZLimit = limit;

        // Configurar drives para movimiento suave
        JointDrive angularDrive = new JointDrive();
        angularDrive.positionSpring = spring;
        angularDrive.positionDamper = damping;
        angularDrive.maximumForce = 5000f; // Limitar fuerza máxima para evitar explosiones

        joint.angularXDrive = angularDrive;
        joint.angularYZDrive = angularDrive;

        // Configuraciones de estabilidad
        joint.configuredInWorldSpace = false;
        joint.swapBodies = false;
        joint.enableCollision = false;
        joint.enablePreprocessing = true;
        joint.autoConfigureConnectedAnchor = true;
        
        // Configuración específica para Unity 6
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.1f;
        joint.projectionAngle = 180f;
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

    void ConfigurePhysicsSettings()
    {
        // Configuraciones optimizadas para Unity 6
        Physics.defaultSolverIterations = 15;           // Aumentado para mejor estabilidad
        Physics.defaultSolverVelocityIterations = 12;   
        Physics.bounceThreshold = 2f;
        Physics.sleepThreshold = 0.005f;
        
        Debug.Log($"⚙️ Physics configurado: Solver={Physics.defaultSolverIterations}, VelocitySolver={Physics.defaultSolverVelocityIterations}");
    }

    // ============ MÉTODOS PÚBLICOS PARA AJUSTE EN RUNTIME ============
    
    [ContextMenu("🧪 Test Cola Physics")]
    public void TestColaPush()
    {
        Debug.Log("=== PROBANDO FÍSICA DE LA COLA ===");
        
        for (int i = 0; i < boneNames.Length; i++)
        {
            Transform bone = FindBoneRecursive(tailRoot, boneNames[i]);
            if (bone != null)
            {
                Rigidbody rb = bone.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    Vector3 force = new Vector3(Random.Range(-3f, 3f), Random.Range(0f, 3f), Random.Range(-3f, 3f));
                    rb.AddForce(force, ForceMode.Impulse);
                    Debug.Log($"💥 Fuerza aplicada a {bone.name}: {force}");
                }
            }
        }
    }

    [ContextMenu("🔧 Ajustar para Estética")]
    public void OptimizeForAesthetics()
    {
        springForce = 1200f;      // Más rígida
        dampingForce = 100f;      // Más amortiguada
        angularLimit = 25f;       // Menos flexible
        segmentMass = 0.1f;       // Más liviana
        
        Debug.Log("🎨 Parámetros ajustados para mejor estética");
        
        if (Application.isPlaying)
        {
            RefreshTailSettings();
        }
    }

    [ContextMenu("🏃 Ajustar para Movimiento Rápido")]
    public void OptimizeForFastMovement()
    {
        springForce = 2000f;      // Muy rígida
        dampingForce = 150f;      // Muy amortiguada
        angularLimit = 20f;       // Poco flexible
        
        Debug.Log("⚡ Parámetros ajustados para movimiento rápido");
        
        if (Application.isPlaying)
        {
            RefreshTailSettings();
        }
    }

    void RefreshTailSettings()
    {
        for (int i = 0; i < boneNames.Length; i++)
        {
            Transform bone = FindBoneRecursive(tailRoot, boneNames[i]);
            if (bone != null)
            {
                ConfigurableJoint joint = bone.GetComponent<ConfigurableJoint>();
                if (joint != null)
                {
                    float progressFactor = progressiveFlexibility ? 
                        Mathf.Lerp(1f, flexibilityFactor, (float)i / (boneNames.Length - 1)) : 1f;
                    
                    JointDrive drive = joint.angularXDrive;
                    drive.positionSpring = springForce * progressFactor;
                    drive.positionDamper = dampingForce * progressFactor;
                    joint.angularXDrive = drive;
                    joint.angularYZDrive = drive;
                }
            }
        }
    }

    // ============ MÉTODOS PARA CONFIGURAR COLISIONES CON SUELO ============
    
    [ContextMenu("🌍 Configurar Colisión con Suelo")]
    public void SetupGroundCollision()
    {
        // Crear capa para la cola si no existe
        SetupTailLayer();
        
        // Configurar todos los segmentos de cola
        for (int i = 0; i < boneNames.Length; i++)
        {
            Transform bone = FindBoneRecursive(tailRoot, boneNames[i]);
            if (bone != null)
            {
                // Asignar a capa de cola
                bone.gameObject.layer = LayerMask.NameToLayer("TailPhysics");
                
                // Configurar material de física para bouncing realista
                SetupPhysicsMaterial(bone.gameObject);
            }
        }
        
        Debug.Log("🌍 Colisiones con suelo configuradas. Asegúrate de tener un collider en el suelo.");
    }
    
    void SetupTailLayer()
    {
        // Nota: En un proyecto real, deberías crear la capa "TailPhysics" manualmente
        // en Project Settings > Tags and Layers
        Debug.Log("💡 Crea una capa llamada 'TailPhysics' en Project Settings > Tags and Layers");
    }
    
    void SetupPhysicsMaterial(GameObject obj)
    {
        CapsuleCollider col = obj.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            // Crear material de física si no existe (Unity 6 compatible)
            PhysicsMaterial tailMaterial = new PhysicsMaterial("TailMaterial");
            tailMaterial.dynamicFriction = 0.6f;    // Fricción media
            tailMaterial.staticFriction = 0.7f;     // Fricción estática
            tailMaterial.bounciness = 0.2f;         // Poco rebote
            tailMaterial.frictionCombine = PhysicsMaterialCombine.Average;
            tailMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
            
            col.material = tailMaterial;
        }
    }
}