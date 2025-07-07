using UnityEngine;

namespace DungeonSystem.Settings
{
    [CreateAssetMenu(fileName = "New Generation Settings", menuName = "Dungeon System/Generation Settings", order = 1)]
    [System.Serializable]
    public class GenerationSettings : ScriptableObject
    {
        [Header("General Settings")]
        public int dungeonWidth = 100;
        public int dungeonHeight = 100;
        public int seed = 12345;
        
        [Header("BSP Settings")]
        public int minRoomSize = 8;
        public int maxRoomSize = 20;
        public int corridorWidth = 3;
        
        [Header("Room Type Distribution")]
        [Range(0f, 1f)] public float treasureRoomChance = 0.1f;
        [Range(0f, 1f)] public float guardRoomChance = 0.2f;
        [Range(0f, 1f)] public float laboratoryChance = 0.15f;
        [Range(0f, 1f)] public float bossRoomChance = 0.05f;
        
        [Header("Visualization")]
        public float tileSize = 1f;
        public bool showGizmos = true;
        
        [Header("Debug Colors")]
        public Color floorColor = Color.blue;
        public Color wallColor = Color.red;
        public Color doorColor = Color.yellow;
        public Color corridorColor = Color.cyan;
        public Color startingRoomColor = Color.green;

        // Método para validar settings
        void OnValidate()
        {
            dungeonWidth = Mathf.Max(20, dungeonWidth);
            dungeonHeight = Mathf.Max(20, dungeonHeight);
            minRoomSize = Mathf.Max(4, minRoomSize);
            maxRoomSize = Mathf.Max(minRoomSize + 2, maxRoomSize);
            corridorWidth = Mathf.Max(1, corridorWidth);
        }
    }
}