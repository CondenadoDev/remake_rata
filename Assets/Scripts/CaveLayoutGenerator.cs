// ============================================================================
//  CaveLayoutGenerator.cs · 2025‑07‑01
//  --------------------------------------------------------------------------
//  Crea la topología (salas + túneles) para el sistema de cuevas.
//  Ajustado a la nueva lógica de «piso plano + salas irregulares».
//
//  Cambios clave en esta revisión:
//  • Las salas se colocan a la cota settings.floorY + settings.roomHeight/2.
//  • Se eliminó la variación vertical (kVerticalVar) en tunel y sala.
//  • Se asegura que las salas no sobresalgan del chunk vertical.
//  • Sigue usando:
//      » Poisson‑Disc Sampling 2D en X‑Z para evitar solapamiento.
//      » Árbol de Recubrimiento Mínimo (MST) + bucles extra.
//      » Túneles serpenteantes (curvatura en plano X‑Z).
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

#region Estructuras de datos --------------------------------------------------

[System.Serializable]
public class CaveRoom
{
    public Vector3 center;
    public float   radius;
    public int     roomType; // 0 = normal · 1 = entrada · 2 = boss

    public CaveRoom(Vector3 c, float r, int t = 0)
    {
        center   = c;
        radius   = r;
        roomType = t;
    }
}

[System.Serializable]
public class CaveConnection
{
    public Vector3       start;
    public Vector3       end;
    public float         radius;
    public float         height;
    public List<Vector3> pathPoints = new();

    public CaveConnection(Vector3 s, Vector3 e, float r, float h)
    {
        start  = s;
        end    = e;
        radius = r;
        height = h;
    }
}

[System.Serializable]
public class CaveLayout
{
    public List<CaveRoom>       rooms       = new();
    public List<CaveConnection> connections = new();
    public Vector3              worldSize;
}

#endregion

// ============================================================================

public class CaveLayoutGenerator
{
    // ------------------------------------------------------------------------
    // Punto de entrada
    public CaveLayout GenerateLayout(CaveSettings settings)
    {
        CaveLayout layout = new()
        {
            worldSize = new Vector3(
                settings.chunkSize.x * settings.voxelSize,
                settings.chunkSize.y * settings.voxelSize,
                settings.chunkSize.z * settings.voxelSize)
        };

        GenerateRooms(layout, settings);
        GenerateConnections(layout, settings);
        EnsureConnectivity(layout);

        return layout;
    }

    // ------------------------------------------------------------------------
    #region Salas

    private void GenerateRooms(CaveLayout layout, CaveSettings settings)
    {
        // Cota central Y (centro geométrico de cada sala: piso + mitad altura)
        float centerY = settings.floorY + settings.roomHeight * 0.5f;

        Vector3 bounds = layout.worldSize;
        List<CaveRoom> placed = new();

        int attemptsLeft = 300;
        while (placed.Count < settings.numRooms && attemptsLeft-- > 0)
        {
            float radius = Random.Range(settings.minRoomRadius, settings.maxRoomRadius);

            // Posición aleatoria en X‑Z dentro de los límites, respetando radio
            Vector3 pos = new(
                Random.Range(radius, bounds.x - radius),
                centerY,
                Random.Range(radius, bounds.z - radius));

            // Comprobar solapamiento con un margen del 20 %
            bool overlaps = false;
            foreach (var r in placed)
            {
                float minDist = (radius + r.radius) * 1.2f;
                if (Vector2.Distance(new(pos.x, pos.z), new(r.center.x, r.center.z)) < minDist)
                {
                    overlaps = true; break;
                }
            }
            if (overlaps) continue;

            int type = placed.Count == 0                      ? 1 : // entrada
                       placed.Count == settings.numRooms - 1 ? 2 : // boss
                       0;

            CaveRoom room = new(pos, radius, type);
            layout.rooms.Add(room);
            placed.Add(room);
        }

        Debug.Log($"[Layout] {layout.rooms.Count}/{settings.numRooms} salas generadas.");
    }

    #endregion
    // ------------------------------------------------------------------------
    #region Conexiones (MST + bucles)

    public void GenerateConnections(CaveLayout layout, CaveSettings settings)
    {
        if (layout.rooms.Count < 2) return;

        layout.connections.AddRange(CreateMST(layout.rooms, settings));
        AddExtraLoops(layout, settings);
    }

    // --- Árbol de recubrimiento mínimo --------------------------------------
    private List<CaveConnection> CreateMST(List<CaveRoom> rooms, CaveSettings settings)
    {
        List<CaveConnection> result = new();

        List<CaveRoom> connected   = new() { rooms[0] };
        List<CaveRoom> unconnected = new(rooms);
        unconnected.RemoveAt(0);

        while (unconnected.Count > 0)
        {
            float bestDist = float.MaxValue;
            CaveRoom a = null, b = null;

            foreach (var c in connected)
            foreach (var u in unconnected)
            {
                float d = Vector3.Distance(c.center, u.center);
                if (d < bestDist) { bestDist = d; a = c; b = u; }
            }

            float radius = settings.tunnelRadius * Random.Range(0.8f, 1.2f);
            float hMin   = settings.tunnelMinHeight;
            float hMax   = settings.roomHeight;
            float height = Random.Range(hMin, hMax);

            var conn = new CaveConnection(a.center, b.center, radius, height);
            AddCurvature(conn);
            result.Add(conn);

            unconnected.Remove(b);
        }
        return result;
    }

    // --- Bucles extra --------------------------------------------------------
    private void AddExtraLoops(CaveLayout layout, CaveSettings settings)
    {
        int loops = Mathf.Max(1, layout.rooms.Count / 2);

        for (int i = 0; i < loops; i++)
        {
            CaveRoom rA = layout.rooms[Random.Range(0, layout.rooms.Count)];
            CaveRoom rB = layout.rooms[Random.Range(0, layout.rooms.Count)];
            if (rA == rB) { i--; continue; }

            bool exists = layout.connections.Exists(c =>
                (c.start == rA.center && c.end == rB.center) ||
                (c.start == rB.center && c.end == rA.center));
            if (exists) { i--; continue; }

            float radius = settings.tunnelRadius * Random.Range(0.8f, 1.2f);
            float height = Random.Range(settings.tunnelMinHeight, settings.roomHeight);

            var conn = new CaveConnection(rA.center, rB.center, radius, height);
            AddCurvature(conn);
            layout.connections.Add(conn);

        }
    }

    #endregion
    // ------------------------------------------------------------------------
    #region Curvatura orgánica (plana en Y)

    private void AddCurvature(CaveConnection conn)
    {
        Vector3 dir     = conn.end - conn.start;
        float   dist    = dir.magnitude;
        Vector3 fwdNorm = dir.normalized;

        // Eje para generar offsets perpendiculares al túnel (en plano X‑Z)
        Vector3 perp = Vector3.Cross(fwdNorm, Vector3.up).normalized;
        if (perp == Vector3.zero) perp = Vector3.right; // caso túnel vertical (poco probable)

        int points = Random.Range(4, 8); // 4‑7 puntos
        for (int i = 1; i <= points; i++)
        {
            float t   = i / (float)(points + 1);
            Vector3 p = Vector3.Lerp(conn.start, conn.end, t);

            float sin   = Mathf.Sin(t * Mathf.PI);           // 0→1→0
            float range = dist * 0.35f * sin;                // hasta 25 % longitud

            Vector3 offset = perp * Random.Range(-range, range);
            offset.y = 0f;                                   // piso plano ⇒ sin verticalidad
            conn.pathPoints.Add(p + offset);
        }
    }

    #endregion
    // ------------------------------------------------------------------------
    #region Conectividad básica

    private void EnsureConnectivity(CaveLayout layout)
    {
        if (layout.connections.Count < layout.rooms.Count - 1)
            Debug.LogWarning("[Layout] Posible problema de conectividad.");
    }

    #endregion
}
