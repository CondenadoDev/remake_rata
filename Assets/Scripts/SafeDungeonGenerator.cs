using UnityEngine;
using DungeonSystem;

/// <summary>
/// Generador seguro que no se congela
/// </summary>
public class SafeDungeonGenerator : MonoBehaviour
{
    public DungeonManager dungeonManager;
    public KeyCode generateKey = KeyCode.G;
    public bool limitRoomConnections = true;
    
    void Start()
    {
        if (dungeonManager == null)
            dungeonManager = FindObjectOfType<DungeonManager>();
            
        if (dungeonManager != null)
        {
            dungeonManager.autoGenerateOnStart = false;
            
            // Forzar configuración segura
            if (dungeonManager.generationSettings != null)
            {
                var settings = dungeonManager.generationSettings;
                
                // Limitar tamaño
                settings.dungeonWidth = Mathf.Min(settings.dungeonWidth, 80);
                settings.dungeonHeight = Mathf.Min(settings.dungeonHeight, 80);
                
                // Asegurar tamaños de habitación razonables
                settings.minRoomSize = Mathf.Max(5, settings.minRoomSize);
                settings.maxRoomSize = Mathf.Min(20, settings.maxRoomSize);
                
                Debug.Log($"[SafeGen] Settings adjusted: {settings.dungeonWidth}x{settings.dungeonHeight}");
            }
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(generateKey))
        {
            GenerateSafeDungeon();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (dungeonManager != null)
            {
                dungeonManager.ClearDungeon();
                Debug.Log("[SafeGen] Dungeon cleared");
            }
        }
    }
    
    void GenerateSafeDungeon()
    {
        if (dungeonManager == null)
        {
            Debug.LogError("[SafeGen] No DungeonManager!");
            return;
        }
        
        Debug.Log("[SafeGen] === STARTING SAFE GENERATION ===");
        
        try
        {
            // Paso 1: Limpiar
            Debug.Log("[SafeGen] Clearing...");
            dungeonManager.ClearDungeon();
            
            // Paso 2: Generar con timeout
            Debug.Log("[SafeGen] Generating structure...");
            float startTime = Time.realtimeSinceStartup;
            dungeonManager.GenerateMapStructure();
            float structureTime = Time.realtimeSinceStartup - startTime;
            
            if (structureTime > 2f)
            {
                Debug.LogWarning($"[SafeGen] Structure generation took {structureTime:F2}s - might be too complex!");
            }
            
            // Verificar que se generó algo
            if (dungeonManager.DungeonData == null || dungeonManager.DungeonData.rooms.Count == 0)
            {
                Debug.LogError("[SafeGen] No rooms generated!");
                return;
            }
            
            Debug.Log($"[SafeGen] Generated {dungeonManager.DungeonData.rooms.Count} rooms");
            
            // Limitar conexiones si está activado
            if (limitRoomConnections && dungeonManager.DungeonData.doors.Count > 50)
            {
                Debug.LogWarning($"[SafeGen] Too many doors ({dungeonManager.DungeonData.doors.Count}), stopping here");
                return;
            }
            
            // Continuar con el resto
            Debug.Log("[SafeGen] Selecting starting point...");
            dungeonManager.SelectStartingPoint();
            
            Debug.Log("[SafeGen] Setting up progression...");
            dungeonManager.SetupInitialProgression();
            
            Debug.Log("[SafeGen] Populating entities...");
            dungeonManager.PopulateWithEntities();
            
            Debug.Log("[SafeGen] Rendering...");
            dungeonManager.RenderDungeon();
            
            Debug.Log("[SafeGen] === GENERATION COMPLETE ===");
            Debug.Log($"[SafeGen] Final stats: {dungeonManager.DungeonData.rooms.Count} rooms, {dungeonManager.DungeonData.doors.Count} doors");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SafeGen] EXCEPTION: {e.Message}\n{e.StackTrace}");
        }
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Press [{generateKey}] to generate safely");
        GUI.Label(new Rect(10, 30, 300, 20), "Press [C] to clear");
        
        if (dungeonManager != null && dungeonManager.DungeonData != null)
        {
            GUI.Label(new Rect(10, 50, 300, 20), $"Rooms: {dungeonManager.DungeonData.rooms.Count}");
            GUI.Label(new Rect(10, 70, 300, 20), $"Doors: {dungeonManager.DungeonData.doors.Count}");
        }
    }
}