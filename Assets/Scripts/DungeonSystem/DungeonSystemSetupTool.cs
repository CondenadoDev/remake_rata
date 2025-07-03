#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using DungeonSystem.Settings;
using DungeonSystem.Core;
using DungeonSystem.Examples;
using DungeonSystem.Progression;

// ←  añade SOLO esta línea


namespace DungeonSystem.EditorTools
{
    public class DungeonSystemSetupTool : EditorWindow
    {
        private string folderPath = "Assets/DungeonSystem/Settings";
        private bool createFolders = true;
        private bool setupPrefabs = true;
        private DungeonGenre selectedGenre = DungeonGenre.Metroidvania;

        [MenuItem("Tools/Dungeon System/Setup Tool", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<DungeonSystemSetupTool>("Dungeon Setup Tool");
            window.minSize = new Vector2(400, 600);
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Dungeon System Setup Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Esta herramienta creará automáticamente todos los ScriptableObjects necesarios " +
                "y configurará el sistema de dungeons.", MessageType.Info);

            EditorGUILayout.Space(10);

            // Configuración de carpetas
            EditorGUILayout.LabelField("Folder Settings", EditorStyles.boldLabel);
            createFolders = EditorGUILayout.Toggle("Create Folder Structure", createFolders);
            folderPath = EditorGUILayout.TextField("Settings Folder Path", folderPath);

            EditorGUILayout.Space(10);

            // Preset selection
            EditorGUILayout.LabelField("Preset Configuration", EditorStyles.boldLabel);
            selectedGenre = (DungeonGenre)EditorGUILayout.EnumPopup("Game Genre", selectedGenre);
            setupPrefabs = EditorGUILayout.Toggle("Setup Example Prefabs", setupPrefabs);

            EditorGUILayout.Space(10);

            // Botones principales
            EditorGUILayout.LabelField("Setup Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Create All Settings Assets", GUILayout.Height(30)))
            {
                CreateAllSettingsAssets();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Setup Scene with DungeonManager", GUILayout.Height(30)))
            {
                SetupScene();
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Create Example Prefabs", GUILayout.Height(30)))
            {
                CreateExamplePrefabs();
            }

            EditorGUILayout.Space(10);

            // Botones individuales
            EditorGUILayout.LabelField("Individual Assets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generation Settings"))
                CreateGenerationSettings();
            if (GUILayout.Button("Spawn Settings"))
                CreateSpawnSettings();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Render Settings"))
                CreateRenderSettings();
            if (GUILayout.Button("Starting Criteria"))
                CreateStartingCriteria();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Manual creation instructions
            EditorGUILayout.LabelField("Manual Creation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "You can also create settings manually:\n" +
                "Right-click in Project → Create → Dungeon System → [Setting Type]", MessageType.Info);
        }

        private void CreateAllSettingsAssets()
        {
            if (createFolders)
                CreateFolderStructure();

            CreateGenerationSettings();
            CreateSpawnSettings();
            CreateRenderSettings();
            CreateStartingCriteria();

            EditorUtility.DisplayDialog("Success", 
                "All settings assets created successfully!\n" +
                $"Check the folder: {folderPath}", "OK");
        }

        private void CreateFolderStructure()
        {
            string[] folders = {
                folderPath,
                folderPath + "/Presets",
                "Assets/DungeonSystem/Prefabs",
                "Assets/DungeonSystem/Materials"
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parentFolder = Path.GetDirectoryName(folder);
                    string folderName = Path.GetFileName(folder);
                    
                    if (!AssetDatabase.IsValidFolder(parentFolder))
                    {
                        // Crear carpetas padre recursivamente
                        CreateFolderRecursive(parentFolder);
                    }
                    
                    AssetDatabase.CreateFolder(parentFolder, folderName);
                }
            }
            AssetDatabase.Refresh();
        }

        private void CreateFolderRecursive(string path)
        {
            string parentPath = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(parentPath) && !string.IsNullOrEmpty(parentPath))
            {
                CreateFolderRecursive(parentPath);
            }
            
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path);
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private void CreateGenerationSettings()
        {
            var settings = CreatePresetSettings();
            CreateAsset(settings, $"{folderPath}/{selectedGenre}_GenerationSettings.asset");
        }

        private void CreateSpawnSettings()
        {
            var settings = CreatePresetSpawnSettings();
            CreateAsset(settings, $"{folderPath}/{selectedGenre}_SpawnSettings.asset");
        }

        private void CreateRenderSettings()
        {
            // Usa el alias ▸ RenderSettingsSO
            var settings = CreateInstance<RenderSettingsDungeon>();

            // Configuración básica
            settings.generate3DAssets       = true;
            settings.wallHeight             = 3f;
            settings.useObjectPooling       = true;
            settings.generateIrregularMeshes = false;
            settings.meshResolution          = 4;
            settings.useProceduralMaterials  = true;

            CreateAsset(settings, $"{folderPath}/{selectedGenre}_RenderSettings.asset");
        }
        private void CreateStartingCriteria()
        {
            var settings = CreateInstance<StartingPointCriteria>();
            
            // Configuración por género
            switch (selectedGenre)
            {
                case DungeonGenre.Metroidvania:
                    settings.minRoomArea = 80f;
                    settings.minConnections = 3;
                    settings.cornerAvoidanceRadius = 25f;
                    break;
                case DungeonGenre.DungeonCrawler:
                    settings.minRoomArea = 100f;
                    settings.minConnections = 2;
                    settings.cornerAvoidanceRadius = 20f;
                    break;
                case DungeonGenre.SurvivalHorror:
                    settings.minRoomArea = 64f;
                    settings.minConnections = 2;
                    settings.cornerAvoidanceRadius = 15f;
                    break;
            }
            
            CreateAsset(settings, $"{folderPath}/{selectedGenre}_StartingCriteria.asset");
        }

        private GenerationSettings CreatePresetSettings()
        {
            var settings = CreateInstance<GenerationSettings>();
            
            switch (selectedGenre)
            {
                case DungeonGenre.Metroidvania:
                    settings.dungeonWidth = 80;
                    settings.dungeonHeight = 60;
                    settings.minRoomSize = 6;
                    settings.maxRoomSize = 15;
                    settings.corridorWidth = 2;
                    settings.treasureRoomChance = 0.15f;
                    settings.guardRoomChance = 0.25f;
                    settings.laboratoryChance = 0.1f;
                    settings.bossRoomChance = 0.05f;
                    break;

                case DungeonGenre.DungeonCrawler:
                    settings.dungeonWidth = 100;
                    settings.dungeonHeight = 100;
                    settings.minRoomSize = 8;
                    settings.maxRoomSize = 25;
                    settings.corridorWidth = 3;
                    settings.treasureRoomChance = 0.2f;
                    settings.guardRoomChance = 0.3f;
                    settings.laboratoryChance = 0.05f;
                    settings.bossRoomChance = 0.1f;
                    break;

                case DungeonGenre.SurvivalHorror:
                    settings.dungeonWidth = 60;
                    settings.dungeonHeight = 60;
                    settings.minRoomSize = 5;
                    settings.maxRoomSize = 12;
                    settings.corridorWidth = 2;
                    settings.treasureRoomChance = 0.05f;
                    settings.guardRoomChance = 0.1f;
                    settings.laboratoryChance = 0.2f;
                    settings.bossRoomChance = 0.03f;
                    break;
            }

            settings.seed = Random.Range(0, 999999);
            return settings;
        }

        private SpawnSettings CreatePresetSpawnSettings()
        {
            var settings = CreateInstance<SpawnSettings>();
            
            // Configurar reglas generales según género
            switch (selectedGenre)
            {
                case DungeonGenre.Metroidvania:
                    settings.generalRules.maxSpawnsPerRoom = 8;
                    settings.generalRules.spawnDensity = 0.4f;
                    settings.generalRules.minDistanceBetweenSpawns = 2f;
                    settings.allowItemsInStartingRoom = true;
                    settings.allowEnemiesInStartingRoom = false;
                    break;

                case DungeonGenre.DungeonCrawler:
                    settings.generalRules.maxSpawnsPerRoom = 12;
                    settings.generalRules.spawnDensity = 0.6f;
                    settings.generalRules.minDistanceBetweenSpawns = 1.5f;
                    settings.allowItemsInStartingRoom = false;
                    settings.allowEnemiesInStartingRoom = false;
                    break;

                case DungeonGenre.SurvivalHorror:
                    settings.generalRules.maxSpawnsPerRoom = 4;
                    settings.generalRules.spawnDensity = 0.2f;
                    settings.generalRules.minDistanceBetweenSpawns = 3f;
                    settings.allowItemsInStartingRoom = true;
                    settings.allowEnemiesInStartingRoom = false;
                    break;
            }

            // Crear algunos items y enemigos de ejemplo
            CreateExampleSpawnData(settings);
            
            return settings;
        }

        private void CreateExampleSpawnData(SpawnSettings settings)
        {
            // Items de ejemplo (sin prefabs, solo configuración)
            settings.itemSpawns = new List<ItemSpawnData>            {
                new ItemSpawnData
                {
                    itemId = "health_potion",
                    weight = 1f,
                    maxPerRoom = 2,
                    allowedRoomTypes = new RoomType[] { RoomType.MediumRoom, RoomType.LargeRoom, RoomType.Laboratory },
                    minDistanceFromStart = 0,
                    avoidPlayerSpawn = true
                },
                new ItemSpawnData
                {
                    itemId = "key",
                    weight = 0.8f,
                    maxPerRoom = 1,
                    allowedRoomTypes = new RoomType[] { RoomType.GuardRoom, RoomType.TreasureRoom },
                    minDistanceFromStart = 2,
                    avoidPlayerSpawn = true
                },
                new ItemSpawnData
                {
                    itemId = "treasure",
                    weight = 0.5f,
                    maxPerRoom = 1,
                    allowedRoomTypes = new RoomType[] { RoomType.TreasureRoom, RoomType.BossRoom },
                    minDistanceFromStart = 3,
                    avoidPlayerSpawn = true
                }
            };

            // Enemigos de ejemplo
            settings.enemySpawns = new List<EnemySpawnData>
            {
                new EnemySpawnData
                {
                    enemyId = "basic_enemy",
                    weight = 1f,
                    maxPerRoom = 3,
                    difficultyLevel = 1f,
                    isBoss = false,
                    allowedRoomTypes = new RoomType[] { RoomType.SmallRoom, RoomType.MediumRoom },
                    minDistanceFromStart = 1,
                    minDistanceFromOtherEnemies = 2f
                },
                new EnemySpawnData
                {
                    enemyId = "guard",
                    weight = 0.8f,
                    maxPerRoom = 2,
                    difficultyLevel = 2f,
                    isBoss = false,
                    allowedRoomTypes = new RoomType[] { RoomType.GuardRoom, RoomType.LargeRoom },
                    minDistanceFromStart = 2,
                    minDistanceFromOtherEnemies = 3f,
                    requiresGuardPost = true
                },
                new EnemySpawnData
                {
                    enemyId = "boss",
                    weight = 1f,
                    maxPerRoom = 1,
                    difficultyLevel = 5f,
                    isBoss = true,
                    allowedRoomTypes = new RoomType[] { RoomType.BossRoom },
                    minDistanceFromStart = 4,
                    minDistanceFromOtherEnemies = 0f
                }
            };

            // Configuraciones por tipo de habitación
            settings.roomTypeConfigs = new RoomTypeSpawnConfig[]
            {
                new RoomTypeSpawnConfig
                {
                    roomType = RoomType.TreasureRoom,
                    guaranteeItem = true,
                    itemSpawnMultiplier = 2f,
                    preferredItemIds = new string[] { "treasure", "key" },
                    guaranteeEnemy = true,
                    enemySpawnMultiplier = 1.5f,
                    preferredEnemyIds = new string[] { "guard" }
                },
                new RoomTypeSpawnConfig
                {
                    roomType = RoomType.BossRoom,
                    guaranteeEnemy = true,
                    enemySpawnMultiplier = 1f,
                    preferredEnemyIds = new string[] { "boss" },
                    guaranteeItem = true,
                    itemSpawnMultiplier = 1f,
                    preferredItemIds = new string[] { "treasure" }
                },
                new RoomTypeSpawnConfig
                {
                    roomType = RoomType.StartingRoom,
                    isSecure = true,
                    guaranteeEnemy = false,
                    guaranteeItem = false
                }
            };
        }

        private void SetupScene()
        {
            // Crear GameObject principal
            GameObject dungeonSystemObj = new GameObject("Dungeon System");
            var dungeonManager = dungeonSystemObj.AddComponent<DungeonManager>();

            // Cargar settings si existen
            string genSettingsPath = $"{folderPath}/{selectedGenre}_GenerationSettings.asset";
            string spawnSettingsPath = $"{folderPath}/{selectedGenre}_SpawnSettings.asset";
            string renderSettingsPath = $"{folderPath}/{selectedGenre}_RenderSettings.asset";
            string criteriaPath = $"{folderPath}/{selectedGenre}_StartingCriteria.asset";

            dungeonManager.generationSettings = AssetDatabase.LoadAssetAtPath<GenerationSettings>(genSettingsPath);
            dungeonManager.spawnSettings = AssetDatabase.LoadAssetAtPath<SpawnSettings>(spawnSettingsPath);
            dungeonManager.renderSettingsDungeon =
                AssetDatabase.LoadAssetAtPath<RenderSettingsDungeon>(renderSettingsPath);
            dungeonManager.startingPointCriteria = AssetDatabase.LoadAssetAtPath<StartingPointCriteria>(criteriaPath);

            // Añadir QuickSetup component
            var quickSetup = dungeonSystemObj.AddComponent<QuickSetupComponent>();
            quickSetup.genre = selectedGenre;

            // Seleccionar el objeto creado
            Selection.activeGameObject = dungeonSystemObj;

            EditorUtility.DisplayDialog("Scene Setup Complete", 
                $"DungeonManager created with {selectedGenre} settings!\n" +
                "You can now generate a dungeon or modify the settings.", "OK");
        }

        private void CreateExamplePrefabs()
        {
            string prefabFolder = "Assets/DungeonSystem/Prefabs";
            
            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/DungeonSystem", "Prefabs");
            }

            // Crear prefabs básicos para tiles
            CreateBasicTilePrefab("Floor", PrimitiveType.Cube, new Vector3(1, 0.1f, 1), prefabFolder);
            CreateBasicTilePrefab("Wall", PrimitiveType.Cube, new Vector3(1, 3, 1), prefabFolder);
            CreateBasicTilePrefab("Door", PrimitiveType.Cube, new Vector3(1, 2.5f, 0.2f), prefabFolder);

            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Prefabs Created", 
                $"Basic tile prefabs created in {prefabFolder}\n" +
                "You can customize these prefabs or replace them with your own.", "OK");
        }

        private void CreateBasicTilePrefab(string name, PrimitiveType primitiveType, Vector3 scale, string folder)
        {
            GameObject obj = GameObject.CreatePrimitive(primitiveType);
            obj.name = name;
            obj.transform.localScale = scale;

            // Añadir material básico
            var renderer = obj.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            
            switch (name)
            {
                case "Floor":
                    mat.color = new Color(0.6f, 0.6f, 0.6f);
                    break;
                case "Wall":
                    mat.color = new Color(0.4f, 0.4f, 0.4f);
                    break;
                case "Door":
                    mat.color = new Color(0.6f, 0.4f, 0.2f);
                    break;
            }
            
            renderer.material = mat;

            // Crear prefab
            string prefabPath = $"{folder}/{name}Prefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            
            // Limpiar escena
            DestroyImmediate(obj);
        }

        private void CreateAsset<T>(T asset, string path) where T : ScriptableObject
        {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created asset: {path}");
        }
    }
}
#endif