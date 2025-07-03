using UnityEngine;
using System.Collections.Generic;
using DungeonSystem.Core;

namespace DungeonSystem.Utils
{
    public static class DungeonMath
    {
        public static float CalculateRoomArea(Room room)
        {
            return room.bounds.width * room.bounds.height;
        }

        public static Vector2 GetRoomCenter(Room room)
        {
            return new Vector2(room.centerPoint.x, room.centerPoint.y);
        }

        public static float DistanceBetweenRooms(Room roomA, Room roomB)
        {
            return Vector2.Distance(GetRoomCenter(roomA), GetRoomCenter(roomB));
        }

        public static List<GridPosition> GetPositionsInRadius(GridPosition center, float radius)
        {
            List<GridPosition> positions = new List<GridPosition>();
            int r = Mathf.CeilToInt(radius);

            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (Vector2.Distance(Vector2.zero, new Vector2(x, y)) <= radius)
                    {
                        positions.Add(new GridPosition(center.x + x, center.y + y));
                    }
                }
            }

            return positions;
        }

        public static List<GridPosition> GetRoomPerimeter(Room room)
        {
            List<GridPosition> perimeter = new List<GridPosition>();
            Rect bounds = room.bounds;

            // Bordes superior e inferior
            for (int x = Mathf.RoundToInt(bounds.x); x < bounds.x + bounds.width; x++)
            {
                perimeter.Add(new GridPosition(x, Mathf.RoundToInt(bounds.y)));
                perimeter.Add(new GridPosition(x, Mathf.RoundToInt(bounds.y + bounds.height - 1)));
            }

            // Bordes izquierdo y derecho (sin repetir esquinas)
            for (int y = Mathf.RoundToInt(bounds.y + 1); y < bounds.y + bounds.height - 1; y++)
            {
                perimeter.Add(new GridPosition(Mathf.RoundToInt(bounds.x), y));
                perimeter.Add(new GridPosition(Mathf.RoundToInt(bounds.x + bounds.width - 1), y));
            }

            return perimeter;
        }

        public static bool IsPositionInRoom(GridPosition position, Room room)
        {
            return room.bounds.Contains(new Vector2(position.x, position.y));
        }

        public static GridPosition GetClosestPositionInRoom(Room room, GridPosition target)
        {
            Vector2 targetVec = new Vector2(target.x, target.y);
            Vector2 roomCenter = GetRoomCenter(room);
            
            // Si el target está dentro de la habitación, devolverlo tal como está
            if (IsPositionInRoom(target, room))
                return target;

            // Encontrar el punto más cercano en el perímetro
            var perimeter = GetRoomPerimeter(room);
            GridPosition closest = perimeter[0];
            float minDistance = Vector2.Distance(targetVec, new Vector2(closest.x, closest.y));

            foreach (var pos in perimeter)
            {
                float distance = Vector2.Distance(targetVec, new Vector2(pos.x, pos.y));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = pos;
                }
            }

            return closest;
        }

        public static float CalculatePathLength(List<GridPosition> path)
        {
            if (path.Count < 2) return 0f;

            float length = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                length += Vector2.Distance(
                    new Vector2(path[i-1].x, path[i-1].y),
                    new Vector2(path[i].x, path[i].y)
                );
            }

            return length;
        }

        public static List<GridPosition> SimplifyPath(List<GridPosition> path, float tolerance = 1f)
        {
            if (path.Count <= 2) return new List<GridPosition>(path);

            List<GridPosition> simplified = new List<GridPosition> { path[0] };
            
            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector2 prev = new Vector2(path[i-1].x, path[i-1].y);
                Vector2 current = new Vector2(path[i].x, path[i].y);
                Vector2 next = new Vector2(path[i+1].x, path[i+1].y);

                // Calcular desviación de la línea recta
                float distanceToLine = DistanceFromPointToLine(current, prev, next);
                
                if (distanceToLine > tolerance)
                {
                    simplified.Add(path[i]);
                }
            }

            simplified.Add(path[path.Count - 1]);
            return simplified;
        }

        private static float DistanceFromPointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            Vector2 pointToStart = point - lineStart;
            
            float lineLength = line.magnitude;
            if (lineLength == 0f) return pointToStart.magnitude;
            
            float projection = Vector2.Dot(pointToStart, line) / lineLength;
            projection = Mathf.Clamp01(projection / lineLength);
            
            Vector2 closestPoint = lineStart + line * projection;
            return Vector2.Distance(point, closestPoint);
        }

        public static bool LineOfSight(DungeonData dungeonData, GridPosition start, GridPosition end)
        {
            // Algoritmo de línea de Bresenham para verificar line of sight
            int x0 = start.x, y0 = start.y;
            int x1 = end.x, y1 = end.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int x = x0, y = y0;

            while (true)
            {
                // Verificar si la posición actual es un muro
                if (dungeonData.IsValidPosition(x, y) && dungeonData.GetTile(x, y) == TileType.Wall)
                    return false;

                if (x == x1 && y == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }

            return true;
        }
    }
}