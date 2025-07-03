using UnityEngine;

public class GroundSetupHelper : MonoBehaviour
{
    [Header("🌍 Configuración del Suelo")]
    [Tooltip("Transform que representa el suelo")]
    public Transform groundObject;
    
    [Tooltip("Tamaño del collider del suelo")]
    public Vector3 groundSize = new Vector3(100f, 1f, 100f);

    [ContextMenu("🏗️ Crear Suelo Automáticamente")]
    public void CreateGround()
    {
        // Crear objeto de suelo si no existe
        if (groundObject == null)
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            groundObject = ground.transform;
        }

        // Añadir BoxCollider
        BoxCollider groundCollider = groundObject.GetComponent<BoxCollider>();
        if (groundCollider == null)
        {
            groundCollider = groundObject.gameObject.AddComponent<BoxCollider>();
        }
        
        groundCollider.size = groundSize;
        groundCollider.isTrigger = false;

        // Crear material de física para el suelo
        PhysicsMaterial groundMaterial = new PhysicsMaterial("GroundMaterial");
        groundMaterial.dynamicFriction = 0.8f;
        groundMaterial.staticFriction = 0.9f;
        groundMaterial.bounciness = 0.1f;
        groundMaterial.frictionCombine = PhysicsMaterialCombine.Maximum;
        groundMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        
        groundCollider.material = groundMaterial;

        // Configurar capa del suelo
        groundObject.gameObject.layer = 0; // Default layer

        Debug.Log("🌍 Suelo creado y configurado correctamente");
    }

    [ContextMenu("⚙️ Configurar Capas de Colisión")]
    public void SetupCollisionLayers()
    {
        Debug.Log("💡 INSTRUCCIONES PARA CONFIGURAR CAPAS:");
        Debug.Log("1. Ve a Edit > Project Settings > Tags and Layers");
        Debug.Log("2. En 'Layers', crea una nueva capa llamada 'TailPhysics'");
        Debug.Log("3. Ve a Edit > Project Settings > Physics");
        Debug.Log("4. En 'Layer Collision Matrix', asegúrate de que 'TailPhysics' colisiona con 'Default' (suelo)");
        Debug.Log("5. Luego ejecuta el comando '🌍 Configurar Colisión con Suelo' en tu TailPhysicsSetup");
    }

    void Start()
    {
        if (groundObject == null)
        {
            Debug.LogWarning("⚠️ GroundSetupHelper: No hay objeto de suelo asignado. Usa '🏗️ Crear Suelo Automáticamente'");
        }
    }

    void OnDrawGizmos()
    {
        if (groundObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundObject.position, groundSize);
        }
    }
}