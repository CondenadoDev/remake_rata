using UnityEngine;
using UnityEngine.UI;
using DungeonSystem;
using DungeonSystem.Interaction;

/// <summary>
/// Ejemplo de cómo integrar el sistema de generación de dungeons en tu juego
/// </summary>
public class DungeonGameExample : MonoBehaviour
{
    [Header("Dungeon Portals")]
    [SerializeField] private GameObject[] dungeonPortals;
    [SerializeField] private int unlockedPortals = 1;
    
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform currentPlayer;
    
    [Header("UI")]
    [SerializeField] private Text seedDisplayText;
    [SerializeField] private Text portalInfoText;
    [SerializeField] private GameObject dungeonSelectMenu;
    
    [Header("Dungeon Themes")]
    [SerializeField] private DungeonTheme[] availableThemes;
    
    [System.Serializable]
    public class DungeonTheme
    {
        public string themeName = "Classic Dungeon";
        public int seedRangeMin = 0;
        public int seedRangeMax = 999999;
        public Color portalColor = Color.cyan;
        public GameObject portalVFXPrefab;
        public string description = "A standard dungeon";
    }
    
    void Start()
    {
        // Spawn inicial del jugador si no existe
        if (currentPlayer == null && playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            currentPlayer = player.transform;
            player.tag = "Player";
        }
        
        // Configurar portales
        SetupPortals();
        
        // Ocultar menú al inicio
        if (dungeonSelectMenu) dungeonSelectMenu.SetActive(false);
    }
    
    void SetupPortals()
    {
        for (int i = 0; i < dungeonPortals.Length; i++)
        {
            if (dungeonPortals[i] == null) continue;
            
            // Configurar cada portal con un tema diferente
            if (i < availableThemes.Length)
            {
                ConfigurePortalTheme(dungeonPortals[i], availableThemes[i], i);
            }
            
            // Bloquear portales no desbloqueados
            if (i >= unlockedPortals)
            {
                LockPortal(dungeonPortals[i]);
            }
        }
    }
    
    void ConfigurePortalTheme(GameObject portal, DungeonTheme theme, int index)
    {
        // Buscar el generador en el portal
        var generator = portal.GetComponent<SimpleDungeonGenerator>();
        var advancedGen = portal.GetComponent<DungeonPortalInteractable>();
        
        // Configurar semilla basada en el tema
        int themeSeed = Random.Range(theme.seedRangeMin, theme.seedRangeMax);
        
        if (generator != null)
        {
            // Configurar generador simple
            var dungeonManager = FindObjectOfType<DungeonManager>();
            if (dungeonManager && dungeonManager.generationSettings)
            {
                // Puedes guardar la semilla del tema para este portal
                portal.name = $"Portal_{theme.themeName}_{themeSeed}";
            }
        }
        
        // Cambiar visual del portal
        var renderers = portal.GetComponentsInChildren<MeshRenderer>();
        foreach (var rend in renderers)
        {
            if (rend.material.HasProperty("_EmissionColor"))
            {
                rend.material.SetColor("_EmissionColor", theme.portalColor * 2f);
            }
        }
        
        // Añadir VFX del tema
        if (theme.portalVFXPrefab != null)
        {
            Instantiate(theme.portalVFXPrefab, portal.transform);
        }
        
        // Añadir información del tema
        AddPortalInfo(portal, theme, index + 1);
    }
    
    void AddPortalInfo(GameObject portal, DungeonTheme theme, int portalNumber)
    {
        // Crear cartel informativo
        GameObject infoSign = new GameObject("Info Sign");
        infoSign.transform.SetParent(portal.transform);
        infoSign.transform.localPosition = new Vector3(0, 3, 0);
        
        // Canvas para el cartel
        Canvas canvas = infoSign.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = infoSign.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(4, 2);
        canvasRect.localScale = Vector3.one * 0.01f;
        
        // Texto del cartel
        GameObject textObj = new GameObject("Theme Text");
        textObj.transform.SetParent(infoSign.transform);
        
        Text text = textObj.AddComponent<Text>();
        text.text = $"Portal {portalNumber}\n{theme.themeName}\n{theme.description}";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = theme.portalColor;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Hacer que mire a la cámara
        infoSign.AddComponent<FaceCamera>();
    }
    
    void LockPortal(GameObject portal)
    {
        // Desactivar interacción
        var generator = portal.GetComponent<SimpleDungeonGenerator>();
        if (generator) generator.enabled = false;
        
        var advancedGen = portal.GetComponent<DungeonPortalInteractable>();
        if (advancedGen) advancedGen.enabled = false;
        
        // Visual de bloqueado
        var renderers = portal.GetComponentsInChildren<MeshRenderer>();
        foreach (var rend in renderers)
        {
            rend.material.color = Color.gray;
            if (rend.material.HasProperty("_EmissionColor"))
            {
                rend.material.SetColor("_EmissionColor", Color.black);
            }
        }
        
        // Añadir barrera visual
        GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrier.name = "Barrier";
        barrier.transform.SetParent(portal.transform);
        barrier.transform.localPosition = Vector3.up;
        barrier.transform.localScale = new Vector3(3, 3, 0.1f);
        
        var barrierRend = barrier.GetComponent<MeshRenderer>();
        barrierRend.material = new Material(Shader.Find("Standard"));
        barrierRend.material.color = new Color(1, 0, 0, 0.5f);
        barrierRend.material.SetFloat("_Mode", 3); // Transparent
        barrierRend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        barrierRend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        barrierRend.material.EnableKeyword("_ALPHABLEND_ON");
        barrierRend.material.renderQueue = 3000;
    }
    
    public void UnlockNextPortal()
    {
        if (unlockedPortals < dungeonPortals.Length)
        {
            unlockedPortals++;
            
            // Desbloquear el siguiente portal
            var portal = dungeonPortals[unlockedPortals - 1];
            if (portal)
            {
                // Reactivar componentes
                var generator = portal.GetComponent<SimpleDungeonGenerator>();
                if (generator) generator.enabled = true;
                
                var advancedGen = portal.GetComponent<DungeonPortalInteractable>();
                if (advancedGen) advancedGen.enabled = true;
                
                // Quitar barrera
                Transform barrier = portal.transform.Find("Barrier");
                if (barrier) Destroy(barrier.gameObject);
                
                // Restaurar colores
                if (unlockedPortals - 1 < availableThemes.Length)
                {
                    ConfigurePortalTheme(portal, availableThemes[unlockedPortals - 1], unlockedPortals - 1);
                }
                
                Debug.Log($"Portal {unlockedPortals} unlocked!");
            }
        }
    }
    
    void Update()
    {
        // Teclas de debug
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UnlockNextPortal();
        }
        
        // Mostrar información de la semilla actual
        if (seedDisplayText)
        {
            var dungeonManager = FindObjectOfType<DungeonManager>();
            if (dungeonManager && dungeonManager.generationSettings)
            {
                seedDisplayText.text = $"Current Seed: {dungeonManager.generationSettings.seed}";
            }
        }
        
        // Detectar portal cercano
        if (currentPlayer && portalInfoText)
        {
            GameObject nearestPortal = GetNearestPortal();
            if (nearestPortal != null)
            {
                float distance = Vector3.Distance(currentPlayer.position, nearestPortal.transform.position);
                if (distance < 5f)
                {
                    portalInfoText.text = $"Near: {nearestPortal.name}";
                }
                else
                {
                    portalInfoText.text = "";
                }
            }
        }
    }
    
    GameObject GetNearestPortal()
    {
        GameObject nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var portal in dungeonPortals)
        {
            if (portal == null) continue;
            
            float dist = Vector3.Distance(currentPlayer.position, portal.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = portal;
            }
        }
        
        return nearest;
    }
    
    // Método llamado cuando el jugador completa un dungeon
    public void OnDungeonCompleted(int dungeonSeed)
    {
        Debug.Log($"Dungeon {dungeonSeed} completed!");
        
        // Desbloquear siguiente portal
        UnlockNextPortal();
        
        // Teletransportar al jugador de vuelta al hub
        if (currentPlayer)
        {
            currentPlayer.position = Vector3.zero;
        }
        
        // Limpiar el dungeon
        var dungeonManager = FindObjectOfType<DungeonManager>();
        if (dungeonManager)
        {
            dungeonManager.ClearDungeon();
        }
    }
}

// Script helper que ya incluimos antes
public class FaceCamera : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
    }
}