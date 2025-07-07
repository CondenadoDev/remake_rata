using System;
using UnityEngine;

namespace DungeonSystem.Core
{
    [Serializable]
    public enum TileType
    {
        Wall = 0,
        Floor = 1,
        Door = 2
    }

    [Serializable]
    public enum RoomType
    {
        StartingRoom,     // Habitación de inicio
        SmallRoom,        // Habitación pequeña (8-12 tiles)
        MediumRoom,       // Habitación mediana (13-20 tiles)
        LargeRoom,        // Habitación grande (21+ tiles)
        Corridor,         // Corredor
        TreasureRoom,     // Sala del tesoro
        GuardRoom,        // Sala de guardias
        Laboratory,       // Laboratorio/biblioteca
        BossRoom          // Sala del jefe
    }

    [Serializable]
    public enum DoorState
    {
        Open,             // Abierta
        Closed,           // Cerrada pero sin llave
        Locked,           // Cerrada con llave
        Sealed,           // Sellada (solo eventos pueden abrir)
        Hidden            // Puerta secreta
    }

    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int x;
        public int y;

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static readonly GridPosition zero = new GridPosition(0, 0);

        public static GridPosition operator +(GridPosition a, GridPosition b)
        {
            return new GridPosition(a.x + b.x, a.y + b.y);
        }

        public static bool operator ==(GridPosition a, GridPosition b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(GridPosition a, GridPosition b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is GridPosition)
                return this == (GridPosition)obj;
            return false;
        }

        public bool Equals(GridPosition other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}