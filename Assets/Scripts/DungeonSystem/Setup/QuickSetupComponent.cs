using UnityEngine;
using DungeonSystem.Settings;
using DungeonSystem.Examples;
using DungeonSystem.Core;
using UnityEngine.Serialization;


namespace DungeonSystem.Examples
{

    public class QuickSetupComponent : MonoBehaviour
    {
        [Header("Quick Setup")]
        public DungeonGenre genre = DungeonGenre.Metroidvania;
        public bool applyPresetOnStart = true;
        
        [Header("Generated Settings")]
        public GenerationSettings generationSettings;
        public SpawnSettings spawnSettings;
        [FormerlySerializedAs("renderSettings")] public RenderSettingsDungeon renderSettingsDungeon;

        void Start()
        {
            if (applyPresetOnStart)
            {
                ApplyPreset();
            }
        }

        [ContextMenu("Apply Preset")]
        public void ApplyPreset()
        {
            switch (genre)
            {
                case DungeonGenre.Metroidvania:
                    generationSettings = DungeonPresets.CreateMetroidvaniaSettings();
                    spawnSettings = DungeonPresets.CreateMetroidvaniaSpawning();
                    break;
                    
                case DungeonGenre.DungeonCrawler:
                    generationSettings = DungeonPresets.CreateDungeonCrawlerSettings();
                    spawnSettings = DungeonPresets.CreateDungeonCrawlerSpawning();
                    break;
                    
                case DungeonGenre.SurvivalHorror:
                    generationSettings = DungeonPresets.CreateSurvivalHorrorSettings();
                    spawnSettings = DungeonPresets.CreateSurvivalHorrorSpawning();
                    break;
            }

            // Aplicar al DungeonManager si existe
            var dungeonManager = FindFirstObjectByType<DungeonManager>();
            if (dungeonManager != null)
            {
                dungeonManager.generationSettings = generationSettings;
                dungeonManager.spawnSettings = spawnSettings;
                
                if (renderSettingsDungeon != null)
                    dungeonManager.renderSettingsDungeon = renderSettingsDungeon;
            }

            Debug.Log($"Applied {genre} preset to dungeon system");
        }

        [ContextMenu("Generate with Current Settings")]
        public void GenerateWithCurrentSettings()
        {
            var dungeonManager = FindFirstObjectByType<DungeonManager>();
            if (dungeonManager != null)
            {
                dungeonManager.GenerateCompleteDungeon();
            }
            else
            {
                Debug.LogError("No DungeonManager found in scene");
            }
        }
    }

    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(QuickSetupComponent))]
    public class QuickSetupComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            QuickSetupComponent setup = (QuickSetupComponent)target;
            
            UnityEditor.EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Apply Preset", GUILayout.Height(30)))
            {
                setup.ApplyPreset();
            }
            
            if (GUILayout.Button("Generate Dungeon", GUILayout.Height(30)))
            {
                setup.GenerateWithCurrentSettings();
            }
            
            UnityEditor.EditorGUILayout.Space(5);
            
            UnityEditor.EditorGUILayout.HelpBox(
                "1. Select a genre preset\n" +
                "2. Click 'Apply Preset' to load settings\n" +
                "3. Click 'Generate Dungeon' to create the dungeon\n" +
                "4. Tweak individual settings as needed", 
                UnityEditor.MessageType.Info
            );
        }
    }
    #endif
}