// Utils/DungeonValidator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonSystem.Core;

namespace DungeonSystem.Utils
{
    public static class DungeonValidator
    {
        public struct ValidationResult
        {
            public bool isValid;
            public List<string> warnings;
            public List<string> errors;
            public float completabilityScore;
            public float balanceScore;

            public ValidationResult(bool valid)
            {
                isValid = valid;
                warnings = new List<string>();
                errors = new List<string>();
                completabilityScore = 0f;
                balanceScore = 0f;
            }
        }

        public static ValidationResult ValidateDungeon(DungeonData dungeonData)
        {
            ValidationResult result = new ValidationResult(true);

            // Validaciones críticas
            ValidateConnectivity(dungeonData, ref result);
            ValidateStartingRoom(dungeonData, ref result);
            ValidateRoomDistribution(dungeonData, ref result);
            
            // Validaciones de balance
            ValidateRoomSizes(dungeonData, ref result);
            ValidateDoorPlacement(dungeonData, ref result);
            ValidateProgression(dungeonData, ref result);

            // Calcular scores finales
            result.completabilityScore = CalculateCompletabilityScore(dungeonData);
            result.balanceScore = CalculateBalanceScore(dungeonData);

            // El mapa es válido si no hay errores críticos
            result.isValid = result.errors.Count == 0;

            return result;
        }

        private static void ValidateConnectivity(DungeonData dungeonData, ref ValidationResult result)
        {
            if (dungeonData.rooms.Count == 0)
            {
                result.errors.Add("No rooms found in dungeon");
                return;
            }

            // Verificar que todas las habitaciones sean alcanzables
            HashSet<Room> reachableRooms = GetReachableRooms(dungeonData, dungeonData.rooms[0]);
            
            if (reachableRooms.Count != dungeonData.rooms.Count)
            {
                int unreachableCount = dungeonData.rooms.Count - reachableRooms.Count;
                result.errors.Add($"{unreachableCount} rooms are unreachable from starting area");
            }

            // Verificar conectividad por tipo de habitación
            var roomsByType = dungeonData.roomsByType;
            foreach (var roomType in roomsByType.Keys)
            {
                if (roomType == RoomType.Corridor) continue;
                
                var roomsOfType = roomsByType[roomType];
                var reachableOfType = roomsOfType.Where(r => reachableRooms.Contains(r)).Count();
                
                if (reachableOfType < roomsOfType.Count)
                {
                    result.warnings.Add($"{roomsOfType.Count - reachableOfType} {roomType} rooms are unreachable");
                }
            }
        }

        private static void ValidateStartingRoom(DungeonData dungeonData, ref ValidationResult result)
        {
            if (dungeonData.startingRoom == null)
            {
                result.errors.Add("No starting room assigned");
                return;
            }

            // Verificar tamaño mínimo
            float area = dungeonData.startingRoom.bounds.width * dungeonData.startingRoom.bounds.height;
            if (area < 64f)
            {
                result.warnings.Add("Starting room is very small - may feel cramped");
            }

            // Verificar conexiones
            int connections = CountRoomConnections(dungeonData.startingRoom, dungeonData);
            if (connections < 2)
            {
                result.warnings.Add("Starting room has few connections - may feel like a dead end");
            }
            else if (connections > 4)
            {
                result.warnings.Add("Starting room has many connections - may be overwhelming");
            }

            // Verificar posición (evitar esquinas)
            Vector2 roomCenter = new Vector2(dungeonData.startingRoom.centerPoint.x, dungeonData.startingRoom.centerPoint.y);
            Vector2 mapCenter = new Vector2(dungeonData.width / 2f, dungeonData.height / 2f);
            float distanceFromCenter = Vector2.Distance(roomCenter, mapCenter);
            float maxDistance = Vector2.Distance(Vector2.zero, mapCenter);
            
            if (distanceFromCenter > maxDistance * 0.8f)
            {
                result.warnings.Add("Starting room is near map edge - consider a more central location");
            }
        }

        private static void ValidateRoomDistribution(DungeonData dungeonData, ref ValidationResult result)
        {
            var roomsByType = dungeonData.roomsByType;
            int totalRooms = dungeonData.rooms.Count;

            // Verificar que hay suficiente variedad
            int typesWithRooms = roomsByType.Count(kvp => kvp.Value.Count > 0);
            if (typesWithRooms < 3)
            {
                result.warnings.Add("Low room type variety - consider adding more room types");
            }

            // Verificar distribución de tamaños
            int smallRooms = roomsByType[RoomType.SmallRoom].Count;
            int mediumRooms = roomsByType[RoomType.MediumRoom].Count;
            int largeRooms = roomsByType[RoomType.LargeRoom].Count;

            float smallRatio = (float)smallRooms / totalRooms;
            float largeRatio = (float)largeRooms / totalRooms;

            if (smallRatio > 0.7f)
            {
                result.warnings.Add("Too many small rooms - may feel repetitive");
            }
            else if (largeRatio > 0.4f)
            {
                result.warnings.Add("Too many large rooms - may feel empty");
            }

            // Verificar habitaciones especiales
            if (roomsByType[RoomType.TreasureRoom].Count == 0)
            {
                result.warnings.Add("No treasure rooms found - consider adding rewards");
            }

            if (roomsByType[RoomType.BossRoom].Count > 1)
            {
                result.warnings.Add("Multiple boss rooms found - may dilute climax");
            }
        }

        private static void ValidateRoomSizes(DungeonData dungeonData, ref ValidationResult result)
        {
            foreach (var room in dungeonData.rooms)
            {
                float area = room.bounds.width * room.bounds.height;
                
                // Verificar tamaños mínimos por tipo
                switch (room.roomType)
                {
                    case RoomType.BossRoom:
                        if (area < 200f)
                            result.warnings.Add($"Boss room at {room.centerPoint} is small for epic encounters");
                        break;
                        
                    case RoomType.TreasureRoom:
                        if (area < 100f)
                            result.warnings.Add($"Treasure room at {room.centerPoint} may not feel rewarding");
                        break;
                        
                    case RoomType.StartingRoom:
                        if (area < 64f)
                            result.warnings.Add($"Starting room at {room.centerPoint} is cramped");
                        break;
                }

                // Verificar habitaciones excesivamente grandes
                if (area > 600f)
                {
                    result.warnings.Add($"Room at {room.centerPoint} is very large - may feel empty");
                }
            }
        }

        private static void ValidateDoorPlacement(DungeonData dungeonData, ref ValidationResult result)
        {
            foreach (var door in dungeonData.doors)
            {
                // Verificar que la puerta conecta dos habitaciones válidas
                if (door.roomA == null || door.roomB == null)
                {
                    result.errors.Add($"Door at {door.position} has invalid room connections");
                    continue;
                }

                // Verificar que la puerta está en un lugar lógico
                if (!IsValidDoorPosition(door, dungeonData))
                {
                    result.warnings.Add($"Door at {door.position} may be in an awkward position");
                }
            }

            // Verificar habitaciones sin puertas
            foreach (var room in dungeonData.rooms)
            {
                int doorCount = CountRoomConnections(room, dungeonData);
                if (doorCount == 0)
                {
                    result.errors.Add($"Room at {room.centerPoint} has no doors - is unreachable");
                }
            }
        }

        private static void ValidateProgression(DungeonData dungeonData, ref ValidationResult result)
        {
            if (dungeonData.startingRoom == null) return;

            // Verificar que hay una progresión lógica de dificultad
            CalculateRoomDistances(dungeonData);

            var roomsByDistance = dungeonData.rooms
                .Where(r => r.distanceFromStart >= 0)
                .GroupBy(r => r.distanceFromStart)
                .OrderBy(g => g.Key);

            foreach (var distanceGroup in roomsByDistance)
            {
                int distance = distanceGroup.Key;
                var rooms = distanceGroup.ToList();

                // Verificar que no hay boss rooms muy cerca del inicio
                if (distance <= 2)
                {
                    var bossRooms = rooms.Where(r => r.roomType == RoomType.BossRoom);
                    if (bossRooms.Any())
                    {
                        result.warnings.Add($"Boss room found at distance {distance} - may be too early");
                    }
                }

                // Verificar que hay habitaciones importantes en distancias medias/lejanas
                if (distance >= 4)
                {
                    var treasureRooms = rooms.Where(r => r.roomType == RoomType.TreasureRoom);
                    if (!treasureRooms.Any() && distance == 4)
                    {
                        result.warnings.Add("No treasure rooms found at medium distances - consider adding rewards");
                    }
                }
            }
        }

        private static HashSet<Room> GetReachableRooms(DungeonData dungeonData, Room startRoom)
        {
            HashSet<Room> visited = new HashSet<Room>();
            Queue<Room> queue = new Queue<Room>();
            
            queue.Enqueue(startRoom);
            visited.Add(startRoom);

            while (queue.Count > 0)
            {
                Room current = queue.Dequeue();
                
                foreach (var door in dungeonData.doors)
                {
                    Room neighbor = null;
                    if (door.roomA == current) neighbor = door.roomB;
                    else if (door.roomB == current) neighbor = door.roomA;
                    
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return visited;
        }

        private static int CountRoomConnections(Room room, DungeonData dungeonData)
        {
            return dungeonData.doors.Count(door => door.roomA == room || door.roomB == room);
        }

        private static bool IsValidDoorPosition(DungeonDoor door, DungeonData dungeonData)
        {
            // Verificar que la puerta está en el borde de al menos una habitación
            bool onRoomAEdge = IsOnRoomEdge(door.position, door.roomA);
            bool onRoomBEdge = IsOnRoomEdge(door.position, door.roomB);
            
            return onRoomAEdge || onRoomBEdge;
        }

        private static bool IsOnRoomEdge(GridPosition position, Room room)
        {
            if (room == null) return false;
            
            Rect bounds = room.bounds;
            return (position.x == Mathf.RoundToInt(bounds.x) || 
                    position.x == Mathf.RoundToInt(bounds.x + bounds.width - 1) ||
                    position.y == Mathf.RoundToInt(bounds.y) || 
                    position.y == Mathf.RoundToInt(bounds.y + bounds.height - 1)) &&
                   bounds.Contains(new Vector2(position.x, position.y));
        }

        private static void CalculateRoomDistances(DungeonData dungeonData)
        {
            if (dungeonData.startingRoom == null) return;

            // Resetear distancias
            foreach (var room in dungeonData.rooms)
            {
                room.distanceFromStart = -1;
            }

            // BFS para calcular distancias
            Queue<Room> queue = new Queue<Room>();
            HashSet<Room> visited = new HashSet<Room>();
            
            dungeonData.startingRoom.distanceFromStart = 0;
            queue.Enqueue(dungeonData.startingRoom);
            visited.Add(dungeonData.startingRoom);

            while (queue.Count > 0)
            {
                Room current = queue.Dequeue();
                
                foreach (var door in dungeonData.doors)
                {
                    Room neighbor = null;
                    if (door.roomA == current) neighbor = door.roomB;
                    else if (door.roomB == current) neighbor = door.roomA;
                    
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        neighbor.distanceFromStart = current.distanceFromStart + 1;
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }

        private static float CalculateCompletabilityScore(DungeonData dungeonData)
        {
            float score = 0f;
            
            // Puntuación base por conectividad
            HashSet<Room> reachableRooms = GetReachableRooms(dungeonData, dungeonData.startingRoom);
            float connectivityRatio = (float)reachableRooms.Count / dungeonData.rooms.Count;
            score += connectivityRatio * 40f;

            // Puntuación por progresión lógica
            if (dungeonData.roomsByType[RoomType.BossRoom].Count == 1)
            {
                var bossRoom = dungeonData.roomsByType[RoomType.BossRoom][0];
                if (bossRoom.distanceFromStart >= 3)
                    score += 20f;
            }

            // Puntuación por distribución de recompensas
            var treasureRooms = dungeonData.roomsByType[RoomType.TreasureRoom];
            if (treasureRooms.Count > 0 && treasureRooms.All(r => r.distanceFromStart >= 2))
                score += 20f;

            // Puntuación por variedad de habitaciones
            int roomTypesUsed = dungeonData.roomsByType.Count(kvp => kvp.Value.Count > 0);
            score += (roomTypesUsed / 8f) * 20f; // Máximo 8 tipos de habitación

            return Mathf.Clamp01(score / 100f);
        }

        private static float CalculateBalanceScore(DungeonData dungeonData)
        {
            float score = 0f;
            
            // Balance de tamaños de habitación
            int total = dungeonData.rooms.Count;
            if (total > 0)
            {
                int small = dungeonData.roomsByType[RoomType.SmallRoom].Count;
                int medium = dungeonData.roomsByType[RoomType.MediumRoom].Count;
                int large = dungeonData.roomsByType[RoomType.LargeRoom].Count;

                float smallRatio = (float)small / total;
                float mediumRatio = (float)medium / total;
                float largeRatio = (float)large / total;

                // Distribución ideal: 50% medium, 30% small, 20% large
                float idealSmall = 0.3f, idealMedium = 0.5f, idealLarge = 0.2f;
                
                float sizeDiff = Mathf.Abs(smallRatio - idealSmall) + 
                               Mathf.Abs(mediumRatio - idealMedium) + 
                               Mathf.Abs(largeRatio - idealLarge);
                
                score += (1f - sizeDiff) * 30f;
            }
            

            // Balance de distribución espacial
            float spatialBalance = CalculateSpatialBalance(dungeonData);
            score += spatialBalance * 40f;

            return Mathf.Clamp01(score / 100f);
        }

        private static float CalculateSpatialBalance(DungeonData dungeonData)
        {
            if (dungeonData.rooms.Count < 2) return 1f;

            // Calcular centro de masa de las habitaciones
            Vector2 centerOfMass = Vector2.zero;
            foreach (var room in dungeonData.rooms)
            {
                centerOfMass += new Vector2(room.centerPoint.x, room.centerPoint.y);
            }
            centerOfMass /= dungeonData.rooms.Count;

            // Calcular qué tan cerca está del centro del mapa
            Vector2 mapCenter = new Vector2(dungeonData.width / 2f, dungeonData.height / 2f);
            float distanceFromMapCenter = Vector2.Distance(centerOfMass, mapCenter);
            float maxDistance = Vector2.Distance(Vector2.zero, mapCenter);
            
            return 1f - (distanceFromMapCenter / maxDistance);
        }
    }
}