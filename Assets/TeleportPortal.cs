using UnityEngine;

public class TeleportPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    public Transform destination;
    public string playerTag = "Player";
    public float cooldown = 1f;
    
    [Header("Positioning")]
    public Vector3 teleportOffset = new Vector3(0, 0, 3f); // Offset desde el portal destino
    public bool useDestinationRotation = true;
    
    [Header("Detection")]
    public float detectionRadius = 2f;
    
    [Header("Visual")]
    public bool showPrompt = true;
    public string promptText = "Press E to teleport";
    
    // Estados
    private bool canTeleport = true;
    private bool playerInRange = false;
    private Transform currentPlayer;
    private float lastTeleportTime = -10f; // Tiempo del último teleport
    
    // Sistema simple anti-spam
    private static float globalLastTeleport = -10f;

    void Update()
    {
        CheckPlayerInRange();
        HandleInput();
    }

    void CheckPlayerInRange()
    {
        // ✅ DETECCIÓN SIMPLE Y CONFIABLE
        Collider[] players = Physics.OverlapSphere(transform.position, detectionRadius);
        
        bool foundPlayer = false;
        Transform playerTransform = null;
        
        foreach (var col in players)
        {
            if (col.CompareTag(playerTag))
            {
                foundPlayer = true;
                playerTransform = col.transform;
                break;
            }
        }
        
        // Actualizar estado
        if (foundPlayer && !playerInRange)
        {
            playerInRange = true;
            currentPlayer = playerTransform;
            Debug.Log($"🌀 Player entered {gameObject.name}");
        }
        else if (!foundPlayer && playerInRange)
        {
            playerInRange = false;
            currentPlayer = null;
            Debug.Log($"🌀 Player left {gameObject.name}");
        }
    }

    void HandleInput()
    {
        if (playerInRange && currentPlayer != null && Input.GetKeyDown(KeyCode.E))
        {
            if (CanTeleport())
            {
                TeleportPlayer();
            }
            else
            {
                float timeLeft = cooldown - (Time.time - lastTeleportTime);
                Debug.Log($"🌀 Cooldown: {timeLeft:F1}s remaining");
            }
        }
    }

    bool CanTeleport()
    {
        // ✅ VALIDACIONES SIMPLES
        if (destination == null)
        {
            Debug.LogError($"No destination set for {gameObject.name}");
            return false;
        }
        
        // Cooldown personal
        if (Time.time - lastTeleportTime < cooldown)
        {
            return false;
        }
        
        // Anti-spam global (muy corto)
        if (Time.time - globalLastTeleport < 0.3f)
        {
            return false;
        }
        
        return canTeleport;
    }

    void TeleportPlayer()
    {
        if (currentPlayer == null) return;

        // ✅ CALCULAR POSICIÓN CON OFFSET CORRECTO
        Vector3 targetPosition = CalculateTargetPosition();
        
        Debug.Log($"🌀 Teleporting from {transform.position} to {targetPosition}");

        // ✅ TELEPORTACIÓN ROBUSTA
        CharacterController cc = currentPlayer.GetComponent<CharacterController>();
        
        if (cc)
        {
            cc.enabled = false;
            currentPlayer.position = targetPosition;
            
            // Rotar si es necesario
            if (useDestinationRotation)
            {
                currentPlayer.rotation = destination.rotation;
            }
            
            cc.enabled = true;
        }
        else
        {
            // Para objetos sin CharacterController
            currentPlayer.position = targetPosition;
            if (useDestinationRotation)
            {
                currentPlayer.rotation = destination.rotation;
            }
        }

        // ✅ ACTUALIZAR TIEMPOS
        lastTeleportTime = Time.time;
        globalLastTeleport = Time.time;
        
        // ✅ COOLDOWN TEMPORAL DEL PORTAL DESTINO
        TeleportPortal destinationPortal = destination.GetComponent<TeleportPortal>();
        if (destinationPortal != null)
        {
            destinationPortal.SetTemporaryCooldown(0.5f); // Muy corto
        }

        Debug.Log($"🌀 ✅ Teleported to {destination.name}");
        
        // Limpiar estado local
        playerInRange = false;
        currentPlayer = null;
    }

    Vector3 CalculateTargetPosition()
    {
        // ✅ POSICIÓN BASE
        Vector3 basePosition = destination.position;
        
        // ✅ APLICAR OFFSET RESPECTO AL PORTAL DESTINO
        Vector3 worldOffset;
        
        if (useDestinationRotation)
        {
            // Offset en el espacio local del portal destino
            worldOffset = destination.TransformDirection(teleportOffset);
        }
        else
        {
            // Offset en coordenadas mundiales
            worldOffset = teleportOffset;
        }
        
        Vector3 finalPosition = basePosition + worldOffset;
        
        // ✅ VERIFICAR QUE LA POSICIÓN SEA VÁLIDA
        // Asegurar que no esté bajo el suelo
        if (Physics.Raycast(finalPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
        {
            finalPosition.y = hit.point.y + 0.1f; // Pequeño offset sobre el suelo
        }
        
        return finalPosition;
    }

    public void SetTemporaryCooldown(float duration)
    {
        StartCoroutine(TemporaryCooldown(duration));
    }

    System.Collections.IEnumerator TemporaryCooldown(float duration)
    {
        canTeleport = false;
        yield return new WaitForSeconds(duration);
        canTeleport = true;
    }

    // ✅ UI SIMPLE
    void OnGUI()
    {
        if (!showPrompt || !playerInRange || currentPlayer == null) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.z <= 0) return;
        
        // Estilo según disponibilidad
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.textColor = CanTeleport() ? Color.green : Color.yellow;
        style.fontSize = 16;
        style.alignment = TextAnchor.MiddleCenter;
        
        string text = promptText;
        if (!CanTeleport())
        {
            float timeLeft = cooldown - (Time.time - lastTeleportTime);
            text = $"Wait {timeLeft:F1}s";
        }
        
        Rect rect = new Rect(screenPos.x - 75, Screen.height - screenPos.y - 40, 150, 30);
        GUI.Label(rect, text, style);
    }

    // ✅ GIZMOS INFORMATIVOS
    void OnDrawGizmos()
    {
        // Portal actual
        Gizmos.color = playerInRange ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Label del portal
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, gameObject.name);
        #endif
        
        if (destination != null)
        {
            // Línea hacia destino
            Gizmos.color = CanTeleport() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, destination.position);
            
            // Posición de teleport
            Vector3 teleportPos = CalculateTargetPosition();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(teleportPos, 0.5f);
            Gizmos.DrawLine(destination.position, teleportPos);
            
            // Flecha indicando dirección
            Vector3 direction = (teleportPos - destination.position).normalized;
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(destination.position, direction * 1.5f);
        }
        
        // Player connection
        if (currentPlayer != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, currentPlayer.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Mostrar área de detección más claramente
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, detectionRadius);
        
        if (destination != null)
        {
            // Mostrar offset visualmente
            Gizmos.color = Color.red;
            Vector3 targetPos = CalculateTargetPosition();
            Gizmos.DrawSphere(targetPos, 0.3f);
        }
    }

    // ✅ MÉTODOS DE UTILIDAD
    [ContextMenu("Test Teleport Position")]
    public void TestTeleportPosition()
    {
        if (destination == null) return;
        
        Vector3 pos = CalculateTargetPosition();
        Debug.Log($"Teleport position would be: {pos}");
        
        // Crear un objeto temporal para visualizar
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "Teleport Position Preview";
        marker.transform.position = pos;
        marker.GetComponent<Renderer>().material.color = Color.yellow;
        
        // Destruir después de 3 segundos
        DestroyImmediate(marker, false);
    }

    [ContextMenu("Setup Bidirectional")]
    public void SetupBidirectional()
    {
        if (destination == null)
        {
            Debug.LogError("Assign destination first");
            return;
        }
        
        TeleportPortal otherPortal = destination.GetComponent<TeleportPortal>();
        if (otherPortal != null)
        {
            otherPortal.destination = transform;
            Debug.Log($"✅ Bidirectional setup: {gameObject.name} ↔ {destination.name}");
        }
    }
}