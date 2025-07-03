using UnityEngine;

public class TeleportSystem : MonoBehaviour
{
    [Header("Portal Positions")]
    public Transform portalA;
    public Transform portalB;
    public Transform destinationA; // Donde aparece cuando vas hacia A
    public Transform destinationB; // Donde aparece cuando vas hacia B
    
    [Header("Settings")]
    public float cooldownTime = 1f;
    public string playerTag = "Player";
    
    private bool playerInPortalA = false;
    private bool playerInPortalB = false;
    private GameObject currentPlayer;
    
    // Control de qué portal está activo
    private bool portalAActive = true;
    private bool portalBActive = true;
    private float lastTeleportTime = -999f;

    void Update()
    {
        if (currentPlayer != null && Input.GetKeyDown(KeyCode.E))
        {
            // Verificar cooldown
            if (Time.time - lastTeleportTime < cooldownTime)
            {
                Debug.Log($"Esperando cooldown: {cooldownTime - (Time.time - lastTeleportTime):F1}s");
                return;
            }

            // Teleportar según dónde esté el jugador
            if (playerInPortalA && portalAActive)
            {
                TeleportTo(destinationB, "A", "B");
                portalAActive = false;  // Desactivar portal A
                portalBActive = true;   // Activar portal B
            }
            else if (playerInPortalB && portalBActive)
            {
                TeleportTo(destinationA, "B", "A");
                portalBActive = false;  // Desactivar portal B
                portalAActive = true;   // Activar portal A
            }
        }
    }

    private void TeleportTo(Transform destination, string from, string to)
    {
        if (destination == null || currentPlayer == null) return;

        Debug.Log($"Teleportando de Portal {from} a Portal {to}");

        // Desactivar CharacterController
        CharacterController controller = currentPlayer.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        // Teleportar
        currentPlayer.transform.position = destination.position;

        // Reactivar CharacterController
        if (controller != null) controller.enabled = true;

        // Limpiar estados
        playerInPortalA = false;
        playerInPortalB = false;
        currentPlayer = null;

        // Actualizar tiempo
        lastTeleportTime = Time.time;

        Debug.Log($"Portal {from} DESACTIVADO, Portal {to} ACTIVADO");
    }

    // Triggers para Portal A
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            currentPlayer = other.gameObject;
            
            // Determinar en qué portal está basado en la distancia
            float distanceToA = Vector3.Distance(other.transform.position, portalA.position);
            float distanceToB = Vector3.Distance(other.transform.position, portalB.position);
            
            if (distanceToA < distanceToB)
            {
                playerInPortalA = true;
                playerInPortalB = false;
                if (portalAActive)
                {
                    Debug.Log("Jugador en Portal A - Presiona E para ir a Portal B");
                }
                else
                {
                    Debug.Log("Portal A desactivado temporalmente");
                }
            }
            else
            {
                playerInPortalB = true;
                playerInPortalA = false;
                if (portalBActive)
                {
                    Debug.Log("Jugador en Portal B - Presiona E para ir a Portal A");
                }
                else
                {
                    Debug.Log("Portal B desactivado temporalmente");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && other.gameObject == currentPlayer)
        {
            playerInPortalA = false;
            playerInPortalB = false;
            currentPlayer = null;
            Debug.Log("Jugador salió de los portales");
        }
    }

    // Método para debug
    void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label($"Portal A: {(portalAActive ? "ACTIVO" : "INACTIVO")}");
            GUILayout.Label($"Portal B: {(portalBActive ? "ACTIVO" : "INACTIVO")}");
            GUILayout.Label($"Jugador en A: {playerInPortalA}");
            GUILayout.Label($"Jugador en B: {playerInPortalB}");
            GUILayout.Label($"Cooldown: {(Time.time - lastTeleportTime < cooldownTime ? "SÍ" : "NO")}");
            GUILayout.EndArea();
        }
    }
}