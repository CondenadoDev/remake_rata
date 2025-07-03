#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DungeonSystem;
using DungeonSystem.Settings;
using DungeonSystem.Examples;

public class CompleteDungeonSetup : EditorWindow
{
    [MenuItem("Tools/Dungeon System/Complete Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<CompleteDungeonSetup>("Dungeon Setup Wizard");
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("Complete Dungeon Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This will create a complete working dungeon system:\n" +
            "• Player object\n" +
            "• Dungeon System with all settings\n" +
            "• Portal with interaction\n" +
            "• Basic prefabs", 
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Complete Setup", GUILayout.Height(40)))
        {
            CreateCompleteSetup();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Individual Components:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Player Only"))
            CreatePlayer();
            
        if (GUILayout.Button("Create Dungeon System Only"))
            CreateDungeonSystem();
            
        if (GUILayout.Button("Create Portal Only"))
            CreatePortal();
            
        if (GUILayout.Button("Create Basic Prefabs"))
            CreateBasicPrefabs();
    }
    
    void CreateCompleteSetup()
    {
        // 1. Create folders
        CreateFolders();
        
        // 2. Create prefabs
        CreateBasicPrefabs();
        
        // 3. Create settings
        var settings = CreateAllSettings();
        
        // 4. Create player
        GameObject player = CreatePlayer();
        
        // 5. Create dungeon system
        GameObject dungeonSystem = CreateDungeonSystem();
        
        // 6. Create portal
        GameObject portal = CreatePortal();
        
        // 7. Setup checker
        GameObject checker = new GameObject("Setup Checker");
        checker.AddComponent<QuickSetupChecker>();
        
        // 8. Connect everything
        ConnectComponents(player, dungeonSystem, portal, settings);
        
        EditorUtility.DisplayDialog("Success!", 
            "Complete dungeon system created!\n\n" +
            "Press Play and walk to the portal, then press E to generate a dungeon.", 
            "OK");
        
        Debug.Log("✅ COMPLETE SETUP FINISHED! Press E near the portal to generate dungeon.");
    }
    
    void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/DungeonSystem"))
            AssetDatabase.CreateFolder("Assets", "DungeonSystem");
            
        if (!AssetDatabase.IsValidFolder("Assets/DungeonSystem/Settings"))
            AssetDatabase.CreateFolder("Assets/DungeonSystem", "Settings");
            
        if (!AssetDatabase.IsValidFolder("Assets/DungeonSystem/Prefabs"))
            AssetDatabase.CreateFolder("Assets/DungeonSystem", "Prefabs");
    }
    
    GameObject CreatePlayer()
    {
        // Check if player exists
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            Debug.Log("Player already exists");
            return existingPlayer;
        }
        
        // Create player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1, -5);
        
        // Add basic movement
        var rb = player.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        // Add simple movement script
        player.AddComponent<SimplePlayerMovement>();
        
        // Add camera
        GameObject cam = new GameObject("Player Camera");
        cam.transform.SetParent(player.transform);
        cam.transform.localPosition = new Vector3(0, 0.5f, 0);
        cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        
        return player;
    }
    
    GameObject CreateDungeonSystem()
    {
        GameObject dungeonSystem = new GameObject("Dungeon System");
        dungeonSystem.transform.position = new Vector3(0, 0, 20); // Away from player
        
        var manager = dungeonSystem.AddComponent<DungeonManager>();
        manager.autoGenerateOnStart = false;
        
        return dungeonSystem;
    }
    
    GameObject CreatePortal()
    {
        // Create portal visuals
        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portal.name = "Portal";
        portal.transform.position = new Vector3(0, 0.1f, 0);
        portal.transform.localScale = new Vector3(3, 0.2f, 3);
        
        // Make it glow
        var renderer = portal.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.cyan * 2f);
        mat.SetColor("_Color", Color.cyan);
        renderer.material = mat;
        
        // Add interaction
        portal.AddComponent<FinalPortalSetup>();
        
        // Add light
        GameObject light = new GameObject("Portal Light");
        light.transform.SetParent(portal.transform);
        light.transform.localPosition = Vector3.up;
        var lightComp = light.AddComponent<Light>();
        lightComp.color = Color.cyan;
        lightComp.intensity = 2f;
        lightComp.range = 5f;
        
        return portal;
    }
    
    void CreateBasicPrefabs()
    {
        string path = "Assets/DungeonSystem/Prefabs/";
        
        // Floor
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(path + "FloorTile.prefab"))
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "FloorTile";
            floor.transform.localScale = new Vector3(1, 0.1f, 1);
            var floorMat = new Material(Shader.Find("Standard"));
            floorMat.color = new Color(0.5f, 0.5f, 0.5f);
            floor.GetComponent<MeshRenderer>().material = floorMat;
            PrefabUtility.SaveAsPrefabAsset(floor, path + "FloorTile.prefab");
            DestroyImmediate(floor);
        }
        
        // Wall
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(path + "WallTile.prefab"))
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "WallTile";
            wall.transform.localScale = new Vector3(1, 3, 1);
            var wallMat = new Material(Shader.Find("Standard"));
            wallMat.color = new Color(0.3f, 0.3f, 0.3f);
            wall.GetComponent<MeshRenderer>().material = wallMat;
            PrefabUtility.SaveAsPrefabAsset(wall, path + "WallTile.prefab");
            DestroyImmediate(wall);
        }
        
        // Door
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(path + "DoorTile.prefab"))
        {
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "DoorTile";
            door.transform.localScale = new Vector3(1, 2.5f, 0.2f);
            var doorMat = new Material(Shader.Find("Standard"));
            doorMat.color = new Color(0.6f, 0.4f, 0.2f);
            door.GetComponent<MeshRenderer>().material = doorMat;
            PrefabUtility.SaveAsPrefabAsset(door, path + "DoorTile.prefab");
            DestroyImmediate(door);
        }
        
        AssetDatabase.Refresh();
    }
    
    (GenerationSettings, RenderSettingsDungeon, SpawnSettings, StartingPointCriteria) CreateAllSettings()
    {
        string path = "Assets/DungeonSystem/Settings/";
        
        // Generation Settings
        GenerationSettings genSettings = AssetDatabase.LoadAssetAtPath<GenerationSettings>(path + "GenerationSettings.asset");
        if (genSettings == null)
        {
            genSettings = CreateInstance<GenerationSettings>();
            genSettings.dungeonWidth = 80;
            genSettings.dungeonHeight = 80;
            genSettings.minRoomSize = 6;
            genSettings.maxRoomSize = 15;
            genSettings.corridorWidth = 2;
            genSettings.treasureRoomChance = 0.1f;
            genSettings.guardRoomChance = 0.2f;
            genSettings.showGizmos = true;
            AssetDatabase.CreateAsset(genSettings, path + "GenerationSettings.asset");
        }
        
        // Render Settings
        RenderSettingsDungeon renderSettings = AssetDatabase.LoadAssetAtPath<RenderSettingsDungeon>(path + "RenderSettings.asset");
        if (renderSettings == null)
        {
            renderSettings = CreateInstance<RenderSettingsDungeon>();
            renderSettings.generate3DAssets = true;
            renderSettings.wallHeight = 3f;
            renderSettings.useObjectPooling = true;
            
            // Assign prefabs
            renderSettings.floorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DungeonSystem/Prefabs/FloorTile.prefab");
            renderSettings.wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DungeonSystem/Prefabs/WallTile.prefab");
            renderSettings.doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DungeonSystem/Prefabs/DoorTile.prefab");
            
            AssetDatabase.CreateAsset(renderSettings, path + "RenderSettings.asset");
        }
        
        // Spawn Settings
        SpawnSettings spawnSettings = AssetDatabase.LoadAssetAtPath<SpawnSettings>(path + "SpawnSettings.asset");
        if (spawnSettings == null)
        {
            spawnSettings = CreateInstance<SpawnSettings>();
            AssetDatabase.CreateAsset(spawnSettings, path + "SpawnSettings.asset");
        }
        
        // Starting Point Criteria
        StartingPointCriteria startCriteria = AssetDatabase.LoadAssetAtPath<StartingPointCriteria>(path + "StartingCriteria.asset");
        if (startCriteria == null)
        {
            startCriteria = CreateInstance<StartingPointCriteria>();
            startCriteria.preferMapEdge = true;
            startCriteria.createExteriorEntrance = true;
            startCriteria.edgePreferenceStrength = 80f;
            AssetDatabase.CreateAsset(startCriteria, path + "StartingCriteria.asset");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        return (genSettings, renderSettings, spawnSettings, startCriteria);
    }
    
    void ConnectComponents(GameObject player, GameObject dungeonSystem, GameObject portal, 
        (GenerationSettings gen, RenderSettingsDungeon render, SpawnSettings spawn, StartingPointCriteria start) settings)
    {
        // Connect DungeonManager settings
        var manager = dungeonSystem.GetComponent<DungeonManager>();
        if (manager != null)
        {
            manager.generationSettings = settings.gen;
            manager.renderSettingsDungeon = settings.render;
            manager.spawnSettings = settings.spawn;
            manager.startingPointCriteria = settings.start;
            manager.autoGenerateOnStart = false;
        }
        
        // Connect portal to dungeon manager
        var portalScript = portal.GetComponent<FinalPortalSetup>();
        if (portalScript != null)
        {
            portalScript.dungeonManager = manager;
        }
        
        // Save changes
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(portalScript);
    }
}

// Simple player movement for testing
public class SimplePlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;
    
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        transform.Translate(new Vector3(h, 0, v) * moveSpeed * Time.deltaTime);
        
        if (Input.GetKey(KeyCode.Q))
            transform.Rotate(0, -rotateSpeed * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.E))
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
#endif