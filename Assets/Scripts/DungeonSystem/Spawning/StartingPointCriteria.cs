using UnityEngine;

namespace DungeonSystem.Settings
{
    [CreateAssetMenu(
        fileName = "StartingPointCriteria",
        menuName = "Dungeon System/Settings/Starting Criteria")]
    public class StartingPointCriteria : ScriptableObject
    {
        [Header("Size Requirements")]
        public float minRoomArea = 64f;

        [Header("Connectivity Requirements")]
        public int minConnections = 1; // Reducido a 1 para permitir entrada única
        public int maxConnections = 3; // Límite superior para evitar demasiadas conexiones en entrada

        [Header("Position Preferences")]
        public bool preferMapEdge = true; // Nueva opción para preferir bordes
        public float edgePreferenceStrength = 50f; // Qué tanto preferir el borde (0-100)
        public bool allowCorners = false; // Si permitir esquinas del mapa
        public float cornerAvoidanceRadius = 20f;
        
        [Header("Entrance Settings")]
        public bool createExteriorEntrance = true; // Crear entrada desde el exterior
        public float entranceWidth = 3f; // Ancho de la entrada
        
        [Header("Map Edge Selection")]
        public EdgePreference preferredEdge = EdgePreference.Any;
        
        [Header("Accessibility")]
        public float minAccessibilityRatio = 0.8f; // Debe poder alcanzar al menos 80% de las habitaciones
    }
    
    public enum EdgePreference
    {
        Any,      // Cualquier borde
        North,    // Borde superior
        South,    // Borde inferior
        East,     // Borde derecho
        West,     // Borde izquierdo
        NorthSouth, // Solo arriba o abajo
        EastWest    // Solo izquierda o derecha
    }
}