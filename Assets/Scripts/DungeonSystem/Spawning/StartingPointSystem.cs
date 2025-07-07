// Progression/StartingPointSystem.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonSystem.Core;
using DungeonSystem.Settings;

namespace DungeonSystem.Progression
{
    public class StartingPointSystem
    {
        public class StartingPointCandidate
        {
            public Room room;
            public float score;
            public string reason;
            public MapEdge nearestEdge;
            public float distanceToEdge;
        }

        public enum MapEdge
        {
            None,
            North,
            South,
            East,
            West
        }

        public static Room SelectStartingRoom(DungeonData dungeonData, StartingPointCriteria criteriaProgression = null)
        {
            if (criteriaProgression == null) 
            {
                criteriaProgression = ScriptableObject.CreateInstance<StartingPointCriteria>();
                criteriaProgression.preferMapEdge = true;
            }

            List<StartingPointCandidate> candidates = EvaluateStartingRoomCandidates(dungeonData, criteriaProgression);
            
            if (candidates.Count == 0)
            {
                Debug.LogWarning("No valid starting room candidates found. Using first room.");
                return dungeonData.rooms.FirstOrDefault();
            }

            // Ordenar por puntuación y seleccionar el mejor
            candidates = candidates.OrderByDescending(c => c.score).ToList();
            
            Room selectedRoom = candidates[0].room;
            selectedRoom.roomType = RoomType.StartingRoom;
            selectedRoom.isStartingRoom = true;
            
            // Actualizar diccionario de tipos
            if (dungeonData.roomsByType.ContainsValue(new List<Room> { selectedRoom }))
            {
                var oldType = dungeonData.roomsByType.FirstOrDefault(kvp => kvp.Value.Contains(selectedRoom)).Key;
                dungeonData.roomsByType[oldType].Remove(selectedRoom);
            }
            dungeonData.roomsByType[RoomType.StartingRoom].Add(selectedRoom);
            
            dungeonData.startingRoom = selectedRoom;
            
            Debug.Log($"Selected starting room: {selectedRoom.centerPoint} on {candidates[0].nearestEdge} edge (Score: {candidates[0].score:F2})");
            
            // Crear entrada desde el exterior si está habilitado
            if (criteriaProgression.createExteriorEntrance)
            {
                CreateExteriorEntrance(dungeonData, selectedRoom, candidates[0]);
            }
            
            return selectedRoom;
        }

        private static void CreateExteriorEntrance(DungeonData dungeonData, Room startingRoom, StartingPointCandidate candidate)
        {
            Debug.Log($"Creating exterior entrance for room at {startingRoom.centerPoint} on {candidate.nearestEdge} edge");
            
            // Determinar la posición de la entrada basándose en qué borde está más cerca
            GridPosition entrancePos = GetEntrancePosition(startingRoom, candidate.nearestEdge, dungeonData);
            
            if (entrancePos == GridPosition.zero)
            {
                Debug.LogWarning("Could not find valid entrance position");
                return;
            }
            
            // Crear la puerta de entrada
            DungeonDoor entranceDoor = new DungeonDoor(entrancePos, startingRoom, null)
            {
                state = DoorState.Open, // La entrada siempre está abierta
                isEntrance = true,
                orientation = GetEntranceOrientation(candidate.nearestEdge)
            };
            
            dungeonData.doors.Add(entranceDoor);
            startingRoom.doorPositions.Add(entrancePos);
            
            // Marcar el tile como puerta
            if (dungeonData.IsValidPosition(entrancePos.x, entrancePos.y))
            {
                dungeonData.SetTile(entrancePos.x, entrancePos.y, TileType.Door);
            }
            
            // Crear un pequeño "vestíbulo" exterior si es necesario
            CreateExteriorVestibule(dungeonData, entrancePos, candidate.nearestEdge);
            
            Debug.Log($"Created entrance at {entrancePos} facing {candidate.nearestEdge}");
        }

        private static GridPosition GetEntrancePosition(Room room, MapEdge edge, DungeonData dungeonData)
        {
            List<GridPosition> candidates = new List<GridPosition>();
            
            switch (edge)
            {
                case MapEdge.North:
                    // Buscar en el borde superior de la habitación
                    for (int x = Mathf.RoundToInt(room.bounds.x + 1); x < room.bounds.x + room.bounds.width - 1; x++)
                    {
                        int y = Mathf.RoundToInt(room.bounds.y + room.bounds.height - 1);
                        if (IsValidEntrancePosition(new GridPosition(x, y), edge, dungeonData))
                            candidates.Add(new GridPosition(x, y));
                    }
                    break;
                    
                case MapEdge.South:
                    // Buscar en el borde inferior
                    for (int x = Mathf.RoundToInt(room.bounds.x + 1); x < room.bounds.x + room.bounds.width - 1; x++)
                    {
                        int y = Mathf.RoundToInt(room.bounds.y);
                        if (IsValidEntrancePosition(new GridPosition(x, y), edge, dungeonData))
                            candidates.Add(new GridPosition(x, y));
                    }
                    break;
                    
                case MapEdge.East:
                    // Buscar en el borde derecho
                    for (int y = Mathf.RoundToInt(room.bounds.y + 1); y < room.bounds.y + room.bounds.height - 1; y++)
                    {
                        int x = Mathf.RoundToInt(room.bounds.x + room.bounds.width - 1);
                        if (IsValidEntrancePosition(new GridPosition(x, y), edge, dungeonData))
                            candidates.Add(new GridPosition(x, y));
                    }
                    break;
                    
                case MapEdge.West:
                    // Buscar en el borde izquierdo
                    for (int y = Mathf.RoundToInt(room.bounds.y + 1); y < room.bounds.y + room.bounds.height - 1; y++)
                    {
                        int x = Mathf.RoundToInt(room.bounds.x);
                        if (IsValidEntrancePosition(new GridPosition(x, y), edge, dungeonData))
                            candidates.Add(new GridPosition(x, y));
                    }
                    break;
            }
            
            // Elegir el punto más centrado
            if (candidates.Count > 0)
            {
                return candidates[candidates.Count / 2];
            }
            
            return GridPosition.zero;
        }

        private static bool IsValidEntrancePosition(GridPosition pos, MapEdge edge, DungeonData dungeonData)
        {
            // Verificar que haya espacio para la entrada
            GridPosition exteriorPos = GetExteriorPosition(pos, edge);
            
            // Verificar que el exterior esté vacío o sea muro
            if (dungeonData.IsValidPosition(exteriorPos.x, exteriorPos.y))
            {
                TileType exteriorTile = dungeonData.GetTile(exteriorPos.x, exteriorPos.y);
                return exteriorTile == TileType.Wall; // Solo crear entrada donde hay muro
            }
            
            return true; // Si está fuera del mapa, es válido
        }

        private static GridPosition GetExteriorPosition(GridPosition interiorPos, MapEdge edge)
        {
            switch (edge)
            {
                case MapEdge.North: return new GridPosition(interiorPos.x, interiorPos.y + 1);
                case MapEdge.South: return new GridPosition(interiorPos.x, interiorPos.y - 1);
                case MapEdge.East: return new GridPosition(interiorPos.x + 1, interiorPos.y);
                case MapEdge.West: return new GridPosition(interiorPos.x - 1, interiorPos.y);
                default: return interiorPos;
            }
        }

        private static DoorOrientation GetEntranceOrientation(MapEdge edge)
        {
            switch (edge)
            {
                case MapEdge.North:
                case MapEdge.South:
                    return DoorOrientation.Horizontal;
                case MapEdge.East:
                case MapEdge.West:
                    return DoorOrientation.Vertical;
                default:
                    return DoorOrientation.Horizontal;
            }
        }

        private static void CreateExteriorVestibule(DungeonData dungeonData, GridPosition entrancePos, MapEdge edge)
        {
            // Crear un pequeño área exterior frente a la entrada
            List<GridPosition> vestibulePositions = new List<GridPosition>();
            
            switch (edge)
            {
                case MapEdge.North:
                    for (int x = -1; x <= 1; x++)
                        for (int y = 1; y <= 3; y++)
                            vestibulePositions.Add(new GridPosition(entrancePos.x + x, entrancePos.y + y));
                    break;
                case MapEdge.South:
                    for (int x = -1; x <= 1; x++)
                        for (int y = -3; y <= -1; y++)
                            vestibulePositions.Add(new GridPosition(entrancePos.x + x, entrancePos.y + y));
                    break;
                case MapEdge.East:
                    for (int x = 1; x <= 3; x++)
                        for (int y = -1; y <= 1; y++)
                            vestibulePositions.Add(new GridPosition(entrancePos.x + x, entrancePos.y + y));
                    break;
                case MapEdge.West:
                    for (int x = -3; x <= -1; x++)
                        for (int y = -1; y <= 1; y++)
                            vestibulePositions.Add(new GridPosition(entrancePos.x + x, entrancePos.y + y));
                    break;
            }
            
            // Marcar las posiciones del vestíbulo como piso
            foreach (var pos in vestibulePositions)
            {
                if (dungeonData.IsValidPosition(pos.x, pos.y))
                {
                    dungeonData.SetTile(pos.x, pos.y, TileType.Floor);
                    dungeonData.corridors.Add(pos); // Añadir como corredor para renderizado
                }
            }
        }

        private static List<StartingPointCandidate> EvaluateStartingRoomCandidates(DungeonData dungeonData, StartingPointCriteria criteriaProgression)
        {
            List<StartingPointCandidate> candidates = new List<StartingPointCandidate>();

            foreach (Room room in dungeonData.rooms)
            {
                StartingPointCandidate candidate = EvaluateRoom(room, dungeonData, criteriaProgression);
                if (candidate.score > 0) // Solo candidatos válidos
                {
                    candidates.Add(candidate);
                }
            }

            return candidates;
        }

        private static StartingPointCandidate EvaluateRoom(Room room, DungeonData dungeonData, StartingPointCriteria criteriaProgression)
        {
            float score = 0f;
            List<string> reasons = new List<string>();

            // 1. Tamaño mínimo requerido
            float area = room.bounds.width * room.bounds.height;
            if (area < criteriaProgression.minRoomArea)
            {
                return new StartingPointCandidate { room = room, score = 0, reason = "Too small" };
            }
            score += 10f;

            // 2. Número de conexiones
            int connectionCount = CountRoomConnections(room, dungeonData);
            if (connectionCount < criteriaProgression.minConnections)
            {
                return new StartingPointCandidate { room = room, score = 0, reason = "Not enough connections" };
            }
            if (connectionCount > criteriaProgression.maxConnections)
            {
                score -= (connectionCount - criteriaProgression.maxConnections) * 5f;
                reasons.Add($"Too many connections ({connectionCount})");
            }
            else
            {
                score += connectionCount * 5f;
                reasons.Add($"{connectionCount} connections");
            }

            // 3. Calcular distancia al borde más cercano
            MapEdge nearestEdge;
            float distanceToEdge = GetDistanceToNearestEdge(room, dungeonData, out nearestEdge);
            
            // 4. Preferencia por bordes del mapa
            if (criteriaProgression.preferMapEdge)
            {
                // Cuanto más cerca del borde, mejor
                float edgeScore = (1f - (distanceToEdge / (dungeonData.width / 2f))) * criteriaProgression.edgePreferenceStrength;
                score += edgeScore;
                reasons.Add($"Edge distance: {distanceToEdge:F1} ({nearestEdge})");
                
                // Bonus si está tocando el borde
                if (distanceToEdge < 2f)
                {
                    score += 25f;
                    reasons.Add("Touching map edge!");
                }
                
                // Verificar preferencia de borde específico
                if (criteriaProgression.preferredEdge != EdgePreference.Any)
                {
                    if (IsPreferredEdge(nearestEdge, criteriaProgression.preferredEdge))
                    {
                        score += 15f;
                        reasons.Add("Preferred edge");
                    }
                    else
                    {
                        score -= 10f;
                        reasons.Add("Not preferred edge");
                    }
                }
            }
            else
            {
                // Si no preferimos bordes, usar la lógica original (preferir centro)
                Vector2 mapCenter = new Vector2(dungeonData.width / 2f, dungeonData.height / 2f);
                Vector2 roomCenter = new Vector2(room.centerPoint.x, room.centerPoint.y);
                float distanceFromCenter = Vector2.Distance(roomCenter, mapCenter);
                float maxDistance = Vector2.Distance(Vector2.zero, mapCenter);
                float centralityScore = (1f - (distanceFromCenter / maxDistance)) * 15f;
                score += centralityScore;
                reasons.Add($"Centrality: {centralityScore:F1}");
            }

            // 5. Evitar esquinas del mapa
            if (IsInMapCorner(room, dungeonData, criteriaProgression.cornerAvoidanceRadius))
            {
                if (!criteriaProgression.allowCorners)
                {
                    score -= 30f;
                    reasons.Add("In corner (penalized)");
                }
                else
                {
                    reasons.Add("In corner (allowed)");
                }
            }

            // 6. Accesibilidad
            int reachableRooms = CountReachableRooms(room, dungeonData);
            float accessibilityRatio = reachableRooms / (float)dungeonData.rooms.Count;
            
            if (accessibilityRatio < criteriaProgression.minAccessibilityRatio)
            {
                return new StartingPointCandidate { room = room, score = 0, reason = "Poor accessibility" };
            }
            
            float accessibilityScore = accessibilityRatio * 20f;
            score += accessibilityScore;
            reasons.Add($"Reaches {reachableRooms}/{dungeonData.rooms.Count} rooms");

            // 7. Preferir habitaciones medianas
            if (room.roomType == RoomType.MediumRoom)
            {
                score += 8f;
                reasons.Add("Medium size");
            }
            else if (room.roomType == RoomType.LargeRoom)
            {
                score += 5f;
                reasons.Add("Large room");
            }

            return new StartingPointCandidate 
            { 
                room = room, 
                score = score, 
                reason = string.Join(", ", reasons),
                nearestEdge = nearestEdge,
                distanceToEdge = distanceToEdge
            };
        }

        private static float GetDistanceToNearestEdge(Room room, DungeonData dungeonData, out MapEdge nearestEdge)
        {
            Vector2 roomCenter = new Vector2(room.centerPoint.x, room.centerPoint.y);
            
            float distNorth = dungeonData.height - room.bounds.y - room.bounds.height;
            float distSouth = room.bounds.y;
            float distEast = dungeonData.width - room.bounds.x - room.bounds.width;
            float distWest = room.bounds.x;
            
            float minDistance = Mathf.Min(distNorth, distSouth, distEast, distWest);
            
            if (minDistance == distNorth) nearestEdge = MapEdge.North;
            else if (minDistance == distSouth) nearestEdge = MapEdge.South;
            else if (minDistance == distEast) nearestEdge = MapEdge.East;
            else nearestEdge = MapEdge.West;
            
            return minDistance;
        }

        private static bool IsPreferredEdge(MapEdge edge, EdgePreference preference)
        {
            switch (preference)
            {
                case EdgePreference.Any:
                    return true;
                case EdgePreference.North:
                    return edge == MapEdge.North;
                case EdgePreference.South:
                    return edge == MapEdge.South;
                case EdgePreference.East:
                    return edge == MapEdge.East;
                case EdgePreference.West:
                    return edge == MapEdge.West;
                case EdgePreference.NorthSouth:
                    return edge == MapEdge.North || edge == MapEdge.South;
                case EdgePreference.EastWest:
                    return edge == MapEdge.East || edge == MapEdge.West;
                default:
                    return false;
            }
        }

        private static int CountRoomConnections(Room room, DungeonData dungeonData)
        {
            return dungeonData.doors.Count(door => door.roomA == room || door.roomB == room);
        }

        private static bool IsInMapCorner(Room room, DungeonData dungeonData, float cornerRadius)
        {
            Vector2 roomCenter = new Vector2(room.centerPoint.x, room.centerPoint.y);
            
            // Verificar distancia a cada esquina del mapa
            Vector2[] corners = {
                new Vector2(0, 0),
                new Vector2(dungeonData.width, 0),
                new Vector2(dungeonData.width, dungeonData.height),
                new Vector2(0, dungeonData.height)
            };

            foreach (Vector2 corner in corners)
            {
                if (Vector2.Distance(roomCenter, corner) < cornerRadius)
                    return true;
            }

            return false;
        }

        private static int CountReachableRooms(Room startRoom, DungeonData dungeonData)
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

            return visited.Count;
        }

        public static void InitializeDoorStates(DungeonData dungeonData)
        {
            foreach (var door in dungeonData.doors)
            {
                // La puerta de entrada siempre está abierta
                if (door.isEntrance)
                {
                    door.state = DoorState.Open;
                    continue;
                }
                
                // Puertas conectadas al starting room permanecen cerradas pero accesibles
                if (door.roomA == dungeonData.startingRoom || door.roomB == dungeonData.startingRoom)
                {
                    door.state = DoorState.Closed;
                }
                else
                {
                    door.state = DoorState.Sealed; // Completamente selladas hasta progresión
                }
            }
        }
    }
}