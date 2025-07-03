using UnityEngine;
using DungeonSystem;

/// <summary>
/// Portal FINAL que funciona - Sin corrutinas, sin problemas
/// </summary>
public class FinalPortalSetup : MonoBehaviour
{
    [Header("Configuration")]
    public DungeonManager dungeonManager;
    public float interactionRange = 3f;
    public bool generateNewSeed = true;
    public bool showPrompt = true;
    
    [Header("Optional UI")]
    public GameObject promptUI;
    public UnityEngine.UI.Text promptText;
    
    private Transform player;
    private bool playerInRange = false;
    private bool canGenerate = true;
    
    void Start()
    {
        // Buscar DungeonManager si no está asignado
        if (dungeonManager == null)
        {
            dungeonManager = FindObjectOfType<DungeonManager>();
            if (dungeonManager == null)
            {
                Debug.LogError("[Portal] No DungeonManager found! Please add one to the scene.");
                enabled = false;
                return;
            }
        }
        
        // IMPORTANTE: Desactivar auto generación
        dungeonManager.autoGenerateOnStart = false;
        
        // Buscar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[Portal] No player found with tag 'Player'!");
        }
        
        // Configurar UI
        if (promptUI) promptUI.SetActive(false);
        if (promptText) promptText.text = "Press [E] to Enter Dungeon";
        
        Debug.Log("[Portal] Setup complete. Press E near the portal to generate dungeon.");
    }
    
    void Update()
    {
        if (!player || !canGenerate) return;
        
        // Calcular distancia
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        // Actualizar UI
        if (playerInRange != wasInRange)
        {
            if (promptUI) promptUI.SetActive(playerInRange && showPrompt);
        }
        
        // Detectar input
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(GenerateDungeon());
        }
    }
    
    System.Collections.IEnumerator GenerateDungeon()
    {
        if (!canGenerate) yield break;
        
        canGenerate = false;
        
        Debug.Log("[Portal] === GENERATING DUNGEON ===");
        
        try
        {
            // Ocultar prompt
            if (promptUI) promptUI.SetActive(false);
            
            // Nueva semilla si está activado
            if (generateNewSeed)
            {
                int seed = Random.Range(0, 999999);
                dungeonManager.generationSettings.seed = seed;
                Debug.Log($"[Portal] New seed: {seed}");
            }
            
            // GENERAR (exactamente como funciona con G)
            yield return StartCoroutine(dungeonManager.GenerateCompleteDungeonAsync());
            
            // Verificar que se generó
            if (dungeonManager.DungeonData != null && dungeonManager.DungeonData.rooms.Count > 0)
            {
                Debug.Log($"[Portal] Success! Generated {dungeonManager.DungeonData.rooms.Count} rooms");
                
                // Teletransportar jugador
                TeleportPlayerToDungeon();
            }
            else
            {
                Debug.LogError("[Portal] Generation failed - no rooms created!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Portal] Error during generation: {e.Message}");
        }
        finally
        {
            // Re-habilitar generación después de 2 segundos
            Invoke("EnableGeneration", 2f);
        }
    }
    
    void TeleportPlayerToDungeon()
    {
        if (!player || dungeonManager.DungeonData == null) return;
        
        Vector3 spawnPosition = dungeonManager.transform.position;
        
        // Buscar entrada principal
        var entrance = dungeonManager.DungeonData.doors.Find(d => d.isEntrance);
        
        if (entrance != null)
        {
            // Spawn fuera de la entrada
            spawnPosition += new Vector3(entrance.position.x, 1f, entrance.position.y);
            
            // Offset según orientación de la puerta
            switch (entrance.orientation)
            {
                case DungeonSystem.Core.DoorOrientation.Horizontal:
                    spawnPosition.z += 3f;
                    break;
                case DungeonSystem.Core.DoorOrientation.Vertical:
                    spawnPosition.x += 3f;
                    break;
            }
            
            Debug.Log($"[Portal] Teleporting to entrance at {spawnPosition}");
        }
        else if (dungeonManager.DungeonData.startingRoom != null)
        {
            // Spawn en el centro de la habitación inicial
            var start = dungeonManager.DungeonData.startingRoom.centerPoint;
            spawnPosition += new Vector3(start.x, 1f, start.y);
            
            Debug.Log($"[Portal] Teleporting to starting room at {spawnPosition}");
        }
        else
        {
            // Fallback: centro del mapa
            spawnPosition += new Vector3(
                dungeonManager.DungeonData.width / 2f,
                1f,
                dungeonManager.DungeonData.height / 2f
            );
            
            Debug.Log($"[Portal] Teleporting to map center at {spawnPosition}");
        }
        
        // Ejecutar teletransporte
        player.position = spawnPosition;
        
        // Hacer que el jugador mire hacia el dungeon
        Vector3 lookTarget = dungeonManager.transform.position + new Vector3(
            dungeonManager.DungeonData.width / 2f,
            0,
            dungeonManager.DungeonData.height / 2f
        );
        
        Vector3 lookDirection = lookTarget - player.position;
        lookDirection.y = 0;
        
        if (lookDirection != Vector3.zero)
        {
            player.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
    
    void EnableGeneration()
    {
        canGenerate = true;
        
        // Mostrar prompt si el jugador sigue cerca
        if (playerInRange && promptUI && showPrompt)
        {
            promptUI.SetActive(true);
        }
    }
    
    void OnDrawGizmos()
    {
        // Mostrar rango de interacción
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Mostrar dirección hacia el dungeon
        if (dungeonManager != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, dungeonManager.transform.position);
        }
        
        // Label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, 
            playerInRange ? "Press E" : "Portal");
        #endif
    }
    
    // Método helper para crear UI básica si no existe
    [ContextMenu("Create Basic UI")]
    void CreateBasicUI()
    {
        if (promptUI != null) return;
        
        // Crear Canvas
        GameObject canvas = new GameObject("Portal UI");
        canvas.transform.SetParent(transform);
        canvas.transform.localPosition = new Vector3(0, 2, 0);
        
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        
        RectTransform rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(4, 1);
        rect.localScale = Vector3.one * 0.01f;
        
        // Panel de fondo
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform);
        
        UnityEngine.UI.Image img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Texto
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform);
        
        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "Press [E] to Enter Dungeon";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Asignar referencias
        promptUI = canvas;
        promptText = text;
        
        Debug.Log("Basic UI created!");
    }
}