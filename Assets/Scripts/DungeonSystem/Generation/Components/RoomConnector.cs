// Core/RoomConnector.cs - VERSIÓN ARREGLADA
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonSystem.Settings;

namespace DungeonSystem.Core
{
    public static class RoomConnector
    {
        private static int maxConnectionAttempts = 10; // LÍMITE DE SEGURIDAD
        
        public static void ConnectRooms(BSPNode rootNode, DungeonData data, GenerationSettings settings)
        {
            Debug.Log("[RoomConnector] Starting room connection process");
            
            // Primero conectar habitaciones vecinas del BSP
            ConnectBSPRooms(rootNode, data, settings);
            
            // Luego asegurar que TODAS las habitaciones estén conectadas
            EnsureAllRoomsConnected(data, settings);
            
            // Validar conexiones
            ValidateConnections(data);
            
            Debug.Log($"[RoomConnector] Connection complete. Total doors: {data.doors.Count}");
        }

        private static void ConnectBSPRooms(BSPNode node, DungeonData data, GenerationSettings settings)
        {
            if (!node.isLeaf)
            {
                ConnectBSPRooms(node.leftChild, data, settings);
                ConnectBSPRooms(node.rightChild, data, settings);
                
                if (node.leftChild != null && node.rightChild != null)
                {
                    Room leftRoom = node.leftChild.GetRandomRoom();
                    Room rightRoom = node.rightChild.GetRandomRoom();
                    
                    if (leftRoom != null && rightRoom != null)
                    {
                        ConnectTwoRooms(leftRoom, rightRoom, data, settings);
                    }
                }
            }
        }

        private static void EnsureAllRoomsConnected(DungeonData data, GenerationSettings settings)
        {
            if (data.rooms.Count <= 1) return;

            int attempts = 0; // CONTADOR DE SEGURIDAD
            
            // Encontrar componentes desconectados
            List<HashSet<Room>> components = FindDisconnectedComponents(data);
            
            // Si hay más de un componente, conectarlos
            while (components.Count > 1 && attempts < maxConnectionAttempts)
            {
                attempts++;
                Debug.Log($"[RoomConnector] Connection attempt {attempts}, components: {components.Count}");
                
                // Conectar el componente más grande con el más cercano
                HashSet<Room> largestComponent = components.OrderByDescending(c => c.Count).First();
                
                Room bestRoom1 = null;
                Room bestRoom2 = null;
                float minDistance = float.MaxValue;
                
                // Encontrar las dos habitaciones más cercanas entre componentes
                foreach (var component in components.Where(c => c != largestComponent))
                {
                    foreach (Room room1 in largestComponent)
                    {
                        foreach (Room room2 in component)
                        {
                            float distance = Vector2.Distance(
                                new Vector2(room1.centerPoint.x, room1.centerPoint.y),
                                new Vector2(room2.centerPoint.x, room2.centerPoint.y)
                            );
                            
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                bestRoom1 = room1;
                                bestRoom2 = room2;
                            }
                        }
                    }
                }
                
                // Conectar las habitaciones más cercanas
                if (bestRoom1 != null && bestRoom2 != null)
                {
                    Debug.Log($"[RoomConnector] Connecting: {bestRoom1.centerPoint} -> {bestRoom2.centerPoint}");
                    ConnectTwoRooms(bestRoom1, bestRoom2, data, settings);
                }
                else
                {
                    Debug.LogWarning("[RoomConnector] Could not find rooms to connect!");
                    break;
                }
                
                // Recalcular componentes
                components = FindDisconnectedComponents(data);
            }
            
            if (attempts >= maxConnectionAttempts)
            {
                Debug.LogError($"[RoomConnector] MAX ATTEMPTS REACHED! Some rooms may be disconnected.");
            }
        }

        private static List<HashSet<Room>> FindDisconnectedComponents(DungeonData data)
        {
            List<HashSet<Room>> components = new List<HashSet<Room>>();
            HashSet<Room> visited = new HashSet<Room>();
            
            foreach (Room room in data.rooms)
            {
                if (!visited.Contains(room))
                {
                    HashSet<Room> component = new HashSet<Room>();
                    DFS(room, visited, component, data);
                    components.Add(component);
                }
            }
            
            return components;
        }

        private static void DFS(Room room, HashSet<Room> visited, HashSet<Room> component, DungeonData data)
        {
            visited.Add(room);
            component.Add(room);
            
            // Encontrar habitaciones conectadas a través de puertas
            foreach (var door in data.doors)
            {
                Room neighbor = null;
                if (door.roomA == room) neighbor = door.roomB;
                else if (door.roomB == room) neighbor = door.roomA;
                
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    DFS(neighbor, visited, component, data);
                }
            }
        }

        private static void ConnectTwoRooms(Room room1, Room room2, DungeonData data, GenerationSettings settings)
        {
            // Verificar si ya están conectadas
            if (AreRoomsConnected(room1, room2, data))
            {
                Debug.Log($"[RoomConnector] Rooms already connected: {room1.centerPoint} <-> {room2.centerPoint}");
                return;
            }

            // LÍMITE DE SEGURIDAD para el corredor
            if (Vector2.Distance(
                new Vector2(room1.centerPoint.x, room1.centerPoint.y),
                new Vector2(room2.centerPoint.x, room2.centerPoint.y)) > 100f)
            {
                Debug.LogWarning("[RoomConnector] Rooms too far apart, skipping connection");
                return;
            }

            GridPosition point1 = room1.centerPoint;
            GridPosition point2 = room2.centerPoint;
            
            // Crear corredor en L
            List<GridPosition> corridor = CreateLCorridor(point1, point2, settings);
            
            if (corridor.Count == 0 || corridor.Count > 1000) // LÍMITE DE SEGURIDAD
            {
                Debug.LogWarning($"[RoomConnector] Invalid corridor (size: {corridor.Count})");
                return;
            }
            
            // Carvar corredor en el mapa
            foreach (GridPosition pos in corridor)
            {
                if (data.IsValidPosition(pos.x, pos.y))
                {
                    data.SetTile(pos.x, pos.y, TileType.Floor);
                    data.corridors.Add(pos);
                }
            }

            // Crear puertas en los puntos de conexión con mejor posicionamiento
            CreateOptimizedDoors(room1, room2, corridor, data, settings);
        }

        private static void CreateOptimizedDoors(Room room1, Room room2, List<GridPosition> corridor, 
                                               DungeonData data, GenerationSettings settings)
        {
            // Limitar número de puertas por habitación
            if (data.GetRoomDoors(room1).Count >= 4 || data.GetRoomDoors(room2).Count >= 4)
            {
                Debug.Log("[RoomConnector] Room has too many doors, skipping");
                return;
            }
            
            // Encontrar mejor posición para puerta 1
            GridPosition door1Pos = FindBestDoorPosition(room1, corridor, data);
            DoorOrientation door1Orient = GetDoorOrientation(room1, door1Pos, corridor);
            
            // Encontrar mejor posición para puerta 2
            GridPosition door2Pos = FindBestDoorPosition(room2, corridor, data);
            DoorOrientation door2Orient = GetDoorOrientation(room2, door2Pos, corridor);

            // Crear puerta 1
            if (door1Pos != GridPosition.zero && data.IsValidPosition(door1Pos.x, door1Pos.y))
            {
                data.SetTile(door1Pos.x, door1Pos.y, TileType.Door);
                DungeonDoor door1 = new DungeonDoor(door1Pos, room1, room2)
                {
                    orientation = door1Orient
                };
                data.doors.Add(door1);
                room1.doorPositions.Add(door1Pos);
            }

            // Crear puerta 2 (solo si es diferente a la puerta 1)
            if (door2Pos != door1Pos && door2Pos != GridPosition.zero && 
                data.IsValidPosition(door2Pos.x, door2Pos.y))
            {
                data.SetTile(door2Pos.x, door2Pos.y, TileType.Door);
                DungeonDoor door2 = new DungeonDoor(door2Pos, room2, room1)
                {
                    orientation = door2Orient
                };
                data.doors.Add(door2);
                room2.doorPositions.Add(door2Pos);
            }
        }

        private static GridPosition FindBestDoorPosition(Room room, List<GridPosition> corridor, DungeonData data)
        {
            if (corridor == null || corridor.Count == 0) return GridPosition.zero;

            List<GridPosition> candidates = new List<GridPosition>();

            // Buscar posiciones donde el corredor toca el borde de la habitación
            foreach (GridPosition corridorPos in corridor)
            {
                if (IsPositionOnRoomEdge(room, corridorPos))
                {
                    // Verificar que tenga espacio para una puerta
                    if (HasSpaceForDoor(room, corridorPos, data))
                    {
                        candidates.Add(corridorPos);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                // Si no hay candidatos en el borde, buscar el punto del corredor más cercano
                return FindClosestCorridorPoint(room, corridor);
            }

            // Elegir el candidato más centrado en la pared
            return SelectBestCandidate(candidates, room);
        }

        private static bool HasSpaceForDoor(Room room, GridPosition pos, DungeonData data)
        {
            // Verificar que haya suficiente espacio alrededor para una puerta
            int checkRadius = 1;
            for (int dx = -checkRadius; dx <= checkRadius; dx++)
            {
                for (int dy = -checkRadius; dy <= checkRadius; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    
                    GridPosition checkPos = new GridPosition(pos.x + dx, pos.y + dy);
                    if (!data.IsValidPosition(checkPos.x, checkPos.y))
                        return false;
                }
            }
            return true;
        }

        private static GridPosition SelectBestCandidate(List<GridPosition> candidates, Room room)
        {
            // Preferir posiciones en el centro de las paredes
            GridPosition bestPos = candidates[0];
            float bestScore = float.MaxValue;

            foreach (var pos in candidates)
            {
                float score = 0;
                
                // Calcular distancia al centro de la pared
                if (IsOnHorizontalEdge(room, pos))
                {
                    float centerX = room.bounds.x + room.bounds.width / 2;
                    score = Mathf.Abs(pos.x - centerX);
                }
                else if (IsOnVerticalEdge(room, pos))
                {
                    float centerY = room.bounds.y + room.bounds.height / 2;
                    score = Mathf.Abs(pos.y - centerY);
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestPos = pos;
                }
            }

            return bestPos;
        }

        private static DoorOrientation GetDoorOrientation(Room room, GridPosition doorPos, List<GridPosition> corridor)
        {
            // Determinar la orientación basándose en qué pared está la puerta
            if (IsOnHorizontalEdge(room, doorPos))
            {
                return DoorOrientation.Horizontal;
            }
            else if (IsOnVerticalEdge(room, doorPos))
            {
                return DoorOrientation.Vertical;
            }
            
            // Por defecto, usar la dirección del corredor
            if (corridor != null && corridor.Count > 1)
            {
                GridPosition nextPos = corridor[1];
                if (Mathf.Abs(nextPos.x - doorPos.x) > Mathf.Abs(nextPos.y - doorPos.y))
                    return DoorOrientation.Horizontal;
                else
                    return DoorOrientation.Vertical;
            }
            
            return DoorOrientation.Horizontal;
        }

        private static bool IsOnHorizontalEdge(Room room, GridPosition pos)
        {
            return (pos.y == Mathf.RoundToInt(room.bounds.y) || 
                    pos.y == Mathf.RoundToInt(room.bounds.y + room.bounds.height - 1)) &&
                   pos.x > room.bounds.x && pos.x < room.bounds.x + room.bounds.width - 1;
        }

        private static bool IsOnVerticalEdge(Room room, GridPosition pos)
        {
            return (pos.x == Mathf.RoundToInt(room.bounds.x) || 
                    pos.x == Mathf.RoundToInt(room.bounds.x + room.bounds.width - 1)) &&
                   pos.y > room.bounds.y && pos.y < room.bounds.y + room.bounds.height - 1;
        }

        private static GridPosition FindClosestCorridorPoint(Room room, List<GridPosition> corridor)
        {
            float minDistance = float.MaxValue;
            GridPosition bestPos = corridor[0];

            foreach (GridPosition corridorPos in corridor)
            {
                // Calcular distancia al perímetro de la habitación
                float distance = DistanceToRoomPerimeter(room, corridorPos);
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestPos = corridorPos;
                }
            }

            return bestPos;
        }

        private static float DistanceToRoomPerimeter(Room room, GridPosition pos)
        {
            float x = pos.x;
            float y = pos.y;
            
            // Calcular distancia a cada borde
            float distLeft = Mathf.Abs(x - room.bounds.x);
            float distRight = Mathf.Abs(x - (room.bounds.x + room.bounds.width - 1));
            float distTop = Mathf.Abs(y - room.bounds.y);
            float distBottom = Mathf.Abs(y - (room.bounds.y + room.bounds.height - 1));
            
            return Mathf.Min(distLeft, distRight, distTop, distBottom);
        }

        private static bool AreRoomsConnected(Room room1, Room room2, DungeonData data)
        {
            return data.doors.Any(door => 
                (door.roomA == room1 && door.roomB == room2) || 
                (door.roomA == room2 && door.roomB == room1));
        }

        private static void ValidateConnections(DungeonData data)
        {
            int totalRooms = data.rooms.Count;
            int connectedRooms = 0;
            
            if (totalRooms > 0)
            {
                HashSet<Room> visited = new HashSet<Room>();
                DFS(data.rooms[0], visited, new HashSet<Room>(), data);
                connectedRooms = visited.Count;
            }
            
            if (connectedRooms < totalRooms)
            {
                Debug.LogWarning($"[RoomConnector] Only {connectedRooms} of {totalRooms} rooms are connected!");
            }
            else
            {
                Debug.Log($"[RoomConnector] ✓ All {totalRooms} rooms are connected");
            }
        }

        private static List<GridPosition> CreateLCorridor(GridPosition start, GridPosition end, GenerationSettings settings)
        {
            List<GridPosition> corridor = new List<GridPosition>();
            
            // LÍMITE DE SEGURIDAD
            int maxCorridorLength = 200;
            
            // Decidir si ir primero horizontal o vertical basándose en la distancia
            bool goHorizontalFirst = Random.Range(0f, 1f) > 0.5f;
            
            GridPosition intermediate;
            if (goHorizontalFirst)
                intermediate = new GridPosition(end.x, start.y);
            else
                intermediate = new GridPosition(start.x, end.y);
            
            CreateStraightCorridor(start, intermediate, corridor, settings, maxCorridorLength);
            CreateStraightCorridor(intermediate, end, corridor, settings, maxCorridorLength);
            
            return corridor;
        }

        private static void CreateStraightCorridor(GridPosition start, GridPosition end, 
            List<GridPosition> corridor, GenerationSettings settings, int maxLength)
        {
            GridPosition current = start;
            GridPosition direction = new GridPosition(
                end.x > start.x ? 1 : end.x < start.x ? -1 : 0,
                end.y > start.y ? 1 : end.y < start.y ? -1 : 0
            );
            
            // Asegurar que avanzamos al menos en una dirección
            if (direction.x == 0 && direction.y == 0) return;
            
            int steps = 0;
            int maxSteps = Mathf.Min(maxLength, Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y) + 10);
            
            while (current != end && steps < maxSteps && corridor.Count < maxLength)
            {
                // Crear corredor con ancho
                for (int i = -settings.corridorWidth/2; i <= settings.corridorWidth/2; i++)
                {
                    for (int j = -settings.corridorWidth/2; j <= settings.corridorWidth/2; j++)
                    {
                        GridPosition corridorPos = new GridPosition(current.x + i, current.y + j);
                        if (!corridor.Contains(corridorPos))
                            corridor.Add(corridorPos);
                    }
                }
                
                // Avanzar en la dirección correcta
                bool moved = false;
                if (direction.x != 0 && current.x != end.x)
                {
                    current.x += direction.x;
                    moved = true;
                }
                if (direction.y != 0 && current.y != end.y)
                {
                    current.y += direction.y;
                    moved = true;
                }
                
                if (!moved) break; // No podemos avanzar más
                steps++;
            }
            
            // Añadir el punto final con su ancho
            for (int i = -settings.corridorWidth/2; i <= settings.corridorWidth/2; i++)
            {
                for (int j = -settings.corridorWidth/2; j <= settings.corridorWidth/2; j++)
                {
                    GridPosition corridorPos = new GridPosition(end.x + i, end.y + j);
                    if (!corridor.Contains(corridorPos) && corridor.Count < maxLength)
                        corridor.Add(corridorPos);
                }
            }
        }

        private static bool IsPositionOnRoomEdge(Room room, GridPosition pos)
        {
            Rect bounds = room.bounds;
            return ((pos.x == Mathf.RoundToInt(bounds.x) || pos.x == Mathf.RoundToInt(bounds.x + bounds.width - 1)) ||
                    (pos.y == Mathf.RoundToInt(bounds.y) || pos.y == Mathf.RoundToInt(bounds.y + bounds.height - 1))) &&
                   bounds.Contains(new Vector2(pos.x, pos.y));
        }
    }
}