using UnityEngine;

public class TailJointStabilizer : MonoBehaviour
{
    [Header("🔗 Estabilización de Joints")]
    [Tooltip("Referencia al script de configuración de cola")]
    public TailPhysicsSetup tailSetup;
    
    [Tooltip("Fuerza máxima que pueden soportar los joints antes de 'romperse'")]
    [Range(1000f, 50000f)]
    public float maxJointForce = 20000f;
    
    [Tooltip("Distancia de proyección para evitar separación")]
    [Range(0.01f, 0.5f)]
    public float projectionDistance = 0.05f;
    
    [Tooltip("Ángulo de proyección para evitar separación")]
    [Range(1f, 180f)]
    public float projectionAngle = 10f;

    [Header("🎛️ Configuración Avanzada")]
    [Tooltip("Multiplicador de rigidez basado en la velocidad")]
    [Range(0.1f, 3f)]
    public float velocityStiffnessMultiplier = 1.5f;
    
    [Tooltip("Velocidad máxima permitida para cada segmento")]
    [Range(5f, 50f)]
    public float maxSegmentVelocity = 15f;

    private Transform[] tailBones;
    private ConfigurableJoint[] tailJoints;
    private JointDrive[] originalDrives;

    void Start()
    {
        if (tailSetup == null)
            tailSetup = GetComponent<TailPhysicsSetup>();

        if (tailSetup != null)
        {
            InitializeJoints();
            StabilizeAllJoints();
        }
    }

    void InitializeJoints()
    {
        tailBones = new Transform[tailSetup.boneNames.Length];
        tailJoints = new ConfigurableJoint[tailSetup.boneNames.Length];
        originalDrives = new JointDrive[tailSetup.boneNames.Length];
        
        for (int i = 0; i < tailSetup.boneNames.Length; i++)
        {
            tailBones[i] = FindBoneRecursive(tailSetup.tailRoot, tailSetup.boneNames[i]);
            
            if (tailBones[i] != null)
            {
                tailJoints[i] = tailBones[i].GetComponent<ConfigurableJoint>();
                if (tailJoints[i] != null)
                {
                    originalDrives[i] = tailJoints[i].angularXDrive;
                }
            }
        }
    }

    void StabilizeAllJoints()
    {
        Debug.Log("🔗 ESTABILIZANDO JOINTS DE LA COLA...");
        
        for (int i = 0; i < tailJoints.Length; i++)
        {
            if (tailJoints[i] != null)
            {
                StabilizeJoint(tailJoints[i], i);
            }
        }
        
        Debug.Log("✅ Estabilización completada");
    }

    void StabilizeJoint(ConfigurableJoint joint, int index)
    {
        // Configurar proyección para evitar separación
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = projectionDistance;
        joint.projectionAngle = projectionAngle;
        
        // Configurar límites de fuerza
        JointDrive currentDrive = joint.angularXDrive;
        currentDrive.maximumForce = maxJointForce;
        
        joint.angularXDrive = currentDrive;
        joint.angularYZDrive = currentDrive;
        
        // Configuraciones adicionales para estabilidad
        joint.enablePreprocessing = true;
        joint.configuredInWorldSpace = false;
        
        Debug.Log($"🔗 Joint estabilizado: {tailSetup.boneNames[index]} | MaxForce: {maxJointForce}");
    }

    void FixedUpdate()
    {
        if (tailBones != null)
        {
            AdaptiveStiffness();
            ControlVelocities();
        }
    }

    void AdaptiveStiffness()
    {
        for (int i = 0; i < tailJoints.Length; i++)
        {
            if (tailJoints[i] != null && tailBones[i] != null)
            {
                Rigidbody rb = tailBones[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float velocityMagnitude = rb.linearVelocity.magnitude;
                    float stiffnessMultiplier = Mathf.Lerp(1f, velocityStiffnessMultiplier, velocityMagnitude / maxSegmentVelocity);
                    
                    JointDrive drive = originalDrives[i];
                    drive.positionSpring = originalDrives[i].positionSpring * stiffnessMultiplier;
                    drive.maximumForce = maxJointForce;
                    
                    tailJoints[i].angularXDrive = drive;
                    tailJoints[i].angularYZDrive = drive;
                }
            }
        }
    }

    void ControlVelocities()
    {
        for (int i = 0; i < tailBones.Length; i++)
        {
            if (tailBones[i] != null)
            {
                Rigidbody rb = tailBones[i].GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    // Limitar velocidad lineal
                    if (rb.linearVelocity.magnitude > maxSegmentVelocity)
                    {
                        rb.linearVelocity = rb.linearVelocity.normalized * maxSegmentVelocity;
                    }
                    
                    // Limitar velocidad angular
                    if (rb.angularVelocity.magnitude > 20f)
                    {
                        rb.angularVelocity = rb.angularVelocity.normalized * 20f;
                    }
                }
            }
        }
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

    [ContextMenu("🔧 Re-estabilizar Joints")]
    public void RestabilizeJoints()
    {
        if (tailJoints != null)
        {
            StabilizeAllJoints();
            Debug.Log("🔧 Joints re-estabilizados");
        }
    }

    [ContextMenu("⚡ Modo Velocidad Alta")]
    public void HighSpeedMode()
    {
        maxJointForce = 30000f;
        projectionDistance = 0.03f;
        projectionAngle = 5f;
        maxSegmentVelocity = 25f;
        velocityStiffnessMultiplier = 2f;
        
        RestabilizeJoints();
        Debug.Log("⚡ Modo de velocidad alta activado");
    }

    [ContextMenu("🧘 Modo Suave")]
    public void SoftMode()
    {
        maxJointForce = 15000f;
        projectionDistance = 0.08f;
        projectionAngle = 15f;
        maxSegmentVelocity = 10f;
        velocityStiffnessMultiplier = 1.2f;
        
        RestabilizeJoints();
        Debug.Log("🧘 Modo suave activado");
    }

    [ContextMenu("📊 Diagnosticar Joints")]
    public void DiagnoseJoints()
    {
        Debug.Log("=== DIAGNÓSTICO DE JOINTS ===");
        
        for (int i = 0; i < tailJoints.Length; i++)
        {
            if (tailJoints[i] != null && tailBones[i] != null)
            {
                Rigidbody rb = tailBones[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float velocity = rb.linearVelocity.magnitude;
                    float angularVel = rb.angularVelocity.magnitude;
                    
                    string status = velocity > maxSegmentVelocity ? "⚡ RÁPIDO" : 
                                  velocity > maxSegmentVelocity * 0.5f ? "⚠️ ACTIVO" : "✅ OK";
                    
                    Debug.Log($"{status} {tailSetup.boneNames[i]}: Vel={velocity:F1}, AngVel={angularVel:F1}");
                }
            }
        }
    }
}