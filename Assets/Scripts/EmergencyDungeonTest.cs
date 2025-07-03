using UnityEngine;
using DungeonSystem;

/// <summary>
/// Versión de emergencia sin corrutinas para evitar congelamiento
/// </summary>
public class EmergencyDungeonTest : MonoBehaviour
{
    [Header("Setup")]
    public DungeonManager dungeonManager;
    public float interactionRange = 3f;
    
    [Header("Status")]
    public string currentStatus = "Ready";
    public bool canGenerate = true;
    
    private Transform player;
    private float lastGenerationTime;
    
    void Awake()
    {
        // Log inmediato
        Debug.Log($"[EMERGENCY] Awake at {Time.time}");
    }
    
    void Start()
    {
        Debug.Log($"[EMERGENCY] Start at {Time.time}");
        
        // Buscar componentes
        if (dungeonManager == null)
            dungeonManager = FindObjectOfType<DungeonManager>();
            
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
        
        // Verificar
        Debug.Log($"[EMERGENCY] DungeonManager: {dungeonManager != null}");
        Debug.Log($"[EMERGENCY] Player: {player != null}");
        
        // Desactivar auto generación
        if (dungeonManager != null)
        {
            dungeonManager.autoGenerateOnStart = false;
            currentStatus = "Ready - Press E near portal";
        }
        else
        {
            currentStatus = "ERROR: No DungeonManager";
        }
    }
    
    void Update()
    {
        // Verificar distancia
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            bool inRange = dist <= interactionRange;
            
            // Cambiar color para feedback visual
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = inRange ? Color.green : Color.red;
            }
            
            // Detectar tecla E
            if (inRange && Input.GetKeyDown(KeyCode.E))
            {
                OnPlayerPressE();
            }
        }
        
        // Tecla de emergencia F9
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("[EMERGENCY] F9 - Force generation");
            ForceGenerate();
        }
        
        // Tecla de limpieza F10
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("[EMERGENCY] F10 - Clear dungeon");
            if (dungeonManager != null)
                dungeonManager.ClearDungeon();
        }
    }
    
    void OnPlayerPressE()
    {
        Debug.Log($"[EMERGENCY] E pressed at {Time.time}");
        currentStatus = "E pressed - Attempting generation";
        
        if (!canGenerate)
        {
            Debug.Log("[EMERGENCY] Cannot generate - cooldown active");
            return;
        }
        
        if (dungeonManager == null)
        {
            Debug.LogError("[EMERGENCY] No DungeonManager!");
            currentStatus = "ERROR: No DungeonManager";
            return;
        }
        
        ForceGenerate();
    }
    
    void ForceGenerate()
    {
        if (Time.time - lastGenerationTime < 2f)
        {
            Debug.Log("[EMERGENCY] Too soon since last generation");
            return;
        }
        
        canGenerate = false;
        lastGenerationTime = Time.time;
        currentStatus = "Generating...";
        
        try
        {
            Debug.Log("[EMERGENCY] === STARTING GENERATION ===");
            
            // Nueva semilla
            int seed = Random.Range(0, 999999);
            dungeonManager.generationSettings.seed = seed;
            Debug.Log($"[EMERGENCY] Seed: {seed}");
            
            // Generar TODO de una vez (sin corrutinas)
            Debug.Log("[EMERGENCY] Calling GenerateCompleteDungeon...");
            dungeonManager.GenerateCompleteDungeon();
            
            Debug.Log("[EMERGENCY] === GENERATION COMPLETE ===");
            currentStatus = "Generation complete!";
            
            // Teleport player
            TeleportPlayer();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EMERGENCY] EXCEPTION: {e.Message}");
            Debug.LogError($"[EMERGENCY] STACK:\n{e.StackTrace}");
            currentStatus = $"ERROR: {e.Message}";
        }
        
        canGenerate = true;
    }
    
    void TeleportPlayer()
    {
        if (player == null || dungeonManager.DungeonData == null)
        {
            Debug.Log("[EMERGENCY] Cannot teleport - missing player or data");
            return;
        }
        
        // Teleport simple al centro
        Vector3 centerPos = new Vector3(50, 1, 50);
        player.position = dungeonManager.transform.position + centerPos;
        Debug.Log($"[EMERGENCY] Player teleported to {player.position}");
    }
    
    void OnGUI()
    {
        // Mostrar estado en pantalla
        GUI.Label(new Rect(10, 10, 400, 30), $"Status: {currentStatus}");
        GUI.Label(new Rect(10, 40, 400, 30), $"Can Generate: {canGenerate}");
        GUI.Label(new Rect(10, 70, 400, 30), "Press E near portal or F9 anywhere");
    }
    
    void OnDrawGizmos()
    {
        // Visual
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}