using UnityEngine;

public class TailStretchPreventer : MonoBehaviour
{
    [Header("🔗 Prevención de Estiramiento")]
    [Tooltip("Distancia máxima permitida entre segmentos")]
    [Range(0.01f, 0.2f)]
    public float maxSegmentDistance = 0.05f;
    
    [Tooltip("Fuerza para mantener los segmentos juntos")]
    [Range(500f, 5000f)]
    public float constraintForce = 2000f;
    
    [Tooltip("Referencia al script de configuración de cola")]
    public TailPhysicsSetup tailSetup;
    
    [Header("🎯 Monitoreo")]
    public bool showDistanceWarnings = true;
    public bool autoCorrectStretching = true;

    private Transform[] tailBones;
    private float[] originalDistances;

    void Start()
    {
        if (tailSetup == null)
        {
            tailSetup = GetComponent<TailPhysicsSetup>();
        }

        if (tailSetup != null)
        {
            InitializeTailBones();
            CalculateOriginalDistances();
        }
        else
        {
            Debug.LogError("❌ TailStretchPreventer: No se encontró TailPhysicsSetup");
        }
    }

    void InitializeTailBones()
    {
        tailBones = new Transform[tailSetup.boneNames.Length];
        
        for (int i = 0; i < tailSetup.boneNames.Length; i++)
        {
            tailBones[i] = FindBoneRecursive(tailSetup.tailRoot, tailSetup.boneNames[i]);
        }
    }

    void CalculateOriginalDistances()
    {
        originalDistances = new float[tailBones.Length - 1];
        
        for (int i = 0; i < tailBones.Length - 1; i++)
        {
            if (tailBones[i] != null && tailBones[i + 1] != null)
            {
                originalDistances[i] = Vector3.Distance(tailBones[i].position, tailBones[i + 1].position);
                Debug.Log($"📏 Distancia original {tailSetup.boneNames[i]} -> {tailSetup.boneNames[i + 1]}: {originalDistances[i]:F3}");
            }
        }
    }

    void FixedUpdate()
    {
        if (autoCorrectStretching && tailBones != null)
        {
            PreventStretching();
        }
    }

    void PreventStretching()
    {
        for (int i = 0; i < tailBones.Length - 1; i++)
        {
            if (tailBones[i] == null || tailBones[i + 1] == null) continue;

            float currentDistance = Vector3.Distance(tailBones[i].position, tailBones[i + 1].position);
            float allowedDistance = originalDistances[i] + maxSegmentDistance;

            if (currentDistance > allowedDistance)
            {
                if (showDistanceWarnings)
                {
                    Debug.LogWarning($"⚠️ Estiramiento detectado: {tailSetup.boneNames[i]} -> {tailSetup.boneNames[i + 1]} | Distancia: {currentDistance:F3} > Permitida: {allowedDistance:F3}");
                }

                // Aplicar corrección
                ApplyConstraintForce(i);
            }
        }
    }

    void ApplyConstraintForce(int segmentIndex)
    {
        Transform bone1 = tailBones[segmentIndex];
        Transform bone2 = tailBones[segmentIndex + 1];

        Rigidbody rb1 = bone1.GetComponent<Rigidbody>();
        Rigidbody rb2 = bone2.GetComponent<Rigidbody>();

        if (rb1 != null && rb2 != null && !rb1.isKinematic && !rb2.isKinematic)
        {
            Vector3 direction = (bone1.position - bone2.position).normalized;
            float targetDistance = originalDistances[segmentIndex];
            float currentDistance = Vector3.Distance(bone1.position, bone2.position);
            float correction = (currentDistance - targetDistance) * 0.5f;

            Vector3 correctionForce = direction * correction * constraintForce * Time.fixedDeltaTime;

            // Aplicar fuerzas opuestas para mantener la distancia
            rb1.AddForce(-correctionForce, ForceMode.Force);
            rb2.AddForce(correctionForce, ForceMode.Force);
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

    [ContextMenu("🔄 Recalcular Distancias Originales")]
    public void RecalculateOriginalDistances()
    {
        if (tailBones != null)
        {
            CalculateOriginalDistances();
            Debug.Log("🔄 Distancias originales recalculadas");
        }
    }

    [ContextMenu("📊 Mostrar Estado Actual")]
    public void ShowCurrentState()
    {
        if (tailBones == null || originalDistances == null) return;

        Debug.Log("=== ESTADO ACTUAL DE LA COLA ===");
        
        for (int i = 0; i < tailBones.Length - 1; i++)
        {
            if (tailBones[i] != null && tailBones[i + 1] != null)
            {
                float currentDistance = Vector3.Distance(tailBones[i].position, tailBones[i + 1].position);
                float stretchPercent = ((currentDistance - originalDistances[i]) / originalDistances[i]) * 100f;
                
                string status = stretchPercent > 20f ? "❌ ESTIRADO" : stretchPercent > 10f ? "⚠️ ALERTA" : "✅ OK";
                
                Debug.Log($"{status} {tailSetup.boneNames[i]} -> {tailSetup.boneNames[i + 1]}: {currentDistance:F3} ({stretchPercent:+F1}%)");
            }
        }
    }

    void OnDrawGizmos()
    {
        if (tailBones == null || !Application.isPlaying) return;

        for (int i = 0; i < tailBones.Length - 1; i++)
        {
            if (tailBones[i] != null && tailBones[i + 1] != null)
            {
                float currentDistance = Vector3.Distance(tailBones[i].position, tailBones[i + 1].position);
                float allowedDistance = originalDistances[i] + maxSegmentDistance;

                // Color basado en el estado del estiramiento
                Gizmos.color = currentDistance > allowedDistance ? Color.red : Color.green;
                Gizmos.DrawLine(tailBones[i].position, tailBones[i + 1].position);
                
                // Mostrar distancia máxima permitida
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(tailBones[i].position, allowedDistance);
            }
        }
    }
}