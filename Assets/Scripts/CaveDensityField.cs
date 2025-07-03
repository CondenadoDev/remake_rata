// ============================================================================
//  CaveDensityField.cs (versión corregida)
//  --------------------------------------------------------------------------
//  • Pisos completamente planos compartiendo la cota settings.floorY.
//  • Habitaciones de contorno orgánico (radio modulado por Perlin 2D).
//  • Pasillos opcionalmente orgánicos y limitados en altura.
//  • Mantiene el resto del pipeline (ruido, suavizado selectivo, etc.).
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

public class CaveDensityField
{
    // --------------------------------------------------------------------
    #region Campos privados

    private float[,,]  density;
    private Vector3Int size;
    private float      voxelSize;
    private CaveSettings settings;

    #endregion
    // --------------------------------------------------------------------
    #region Accesores públicos

    public Vector3Int Size      => size;
    public float      VoxelSize => voxelSize;

    #endregion
    // --------------------------------------------------------------------
    #region Inicialización

    public void Initialize(CaveSettings s)
    {
        settings   = s;
        size       = s.chunkSize;
        voxelSize  = s.voxelSize;
        density    = new float[size.x, size.y, size.z];

        // Rellena todo el campo con roca sólida (densidad = 1)
        for (int x = 0; x < size.x; x++)
        for (int y = 0; y < size.y; y++)
        for (int z = 0; z < size.z; z++)
            density[x, y, z] = 1f;
    }

    #endregion
    // --------------------------------------------------------------------
    #region Pipeline principal

    public void GenerateFromLayout(CaveLayout layout)
    {
        ExcavateRooms(layout);
        ExcavateTunnels(layout);

        ApplyOrganicNoise();
        SmoothTransitions();

        // Asegura que las 1‑2 primeras capas sigan perfectamente planas
        if (settings.floorY <= 0.001f) FlattenFloor(2);
    }

    #endregion
    // --------------------------------------------------------------------
    #region Excavación de Salas (planas + irregulares)

    // Devuelve un radio modulado por ruido en función del ángulo
    private float RadiusAtAngle(float baseRadius, float angleRad)
    {
        float ir   = settings.irregularity;           // 0‑1
        if (ir <= 0f) return baseRadius;              // círculo perfecto

        float scale = settings.irrNoiseScale;
        int   seed  = settings.irrSeed;

        float nx = Mathf.Cos(angleRad) * scale + seed * 0.173f;
        float ny = Mathf.Sin(angleRad) * scale + seed * 0.719f;

        float n  = Mathf.PerlinNoise(nx, ny);         // 0‑1
        float k  = Mathf.Lerp(-ir, ir, n);            // –ir … +ir
        return baseRadius * (1f + k);
    }

    private void ExcavateRooms(CaveLayout layout)
    {
        int floorVoxelY   = Mathf.RoundToInt(settings.floorY / voxelSize);
        int roomHeightVox = Mathf.CeilToInt(settings.roomHeight / voxelSize);

        foreach (var room in layout.rooms)
        {
            Vector3Int centerV = WorldToVoxel(room.center);
            int rVoxelMax      = Mathf.CeilToInt(room.radius / voxelSize);

            // Bucle sobre X‑Z (plantilla extruida)
            for (int dx = -rVoxelMax; dx <= rVoxelMax; dx++)
            for (int dz = -rVoxelMax; dz <= rVoxelMax; dz++)
            {
                float wx = dx * voxelSize;
                float wz = dz * voxelSize;

                float ang     = Mathf.Atan2(wz, wx);                   // –π…π
                float rAtAng  = RadiusAtAngle(room.radius, ang);
                if (wx * wx + wz * wz > rAtAng * rAtAng) continue;     // fuera

                // Excava verticalmente desde el piso al techo de la sala
                for (int dy = 0; dy < roomHeightVox; dy++)
                {
                    Vector3Int vp = new Vector3Int(
                        centerV.x + dx,
                        floorVoxelY + dy,
                        centerV.z + dz);

                    if (!IsValidVoxel(vp)) continue;
                    density[vp.x, vp.y, vp.z] = -1f;                   // aire
                }
            }
        }
    }

    #endregion
    // --------------------------------------------------------------------
    #region Excavación de Túneles (altura fija)

    private void ExcavateTunnels(CaveLayout layout)
    {
        foreach (var c in layout.connections) ExcavateTunnel(c);
    }

    private void ExcavateTunnel(CaveConnection conn)
    {
        // Lista completa de puntos (inicio, curvatura interna, final)
        List<Vector3> pts = new() { conn.start };
        pts.AddRange(conn.pathPoints);
        pts.Add(conn.end);

        for (int i = 0; i < pts.Count - 1; i++)
            ExcavateSegment(pts[i], pts[i + 1], conn.radius, conn.height);
    }

    // Excava a lo largo del segmento usando discos extruidos (plano)
    private void ExcavateSegment(Vector3 a, Vector3 b, float baseRadius, float segHeight)
    {
        Vector3 dir    = b - a;
        float   length = dir.magnitude;
        int     steps  = Mathf.Max(1, Mathf.CeilToInt(length / (voxelSize * 0.5f)));

        for (int s = 0; s <= steps; s++)
        {
            float   t = s / (float)steps;
            Vector3 p = Vector3.Lerp(a, b, t);

            // Ángulo del tramo (para irregularidad) en el plano X‑Z
            float ang  = Mathf.Atan2(dir.z, dir.x);
            float r    = RadiusAtAngle(baseRadius, ang);

            ExcavateDisk(p, r, segHeight);   // disco extruido verticalmente
        }
    }

    // Excava un "cilindro" con piso plano: footprint circular + altura fija
    private void ExcavateDisk(Vector3 center, float radius, float height)
    {
        int floorVoxelY    = Mathf.RoundToInt(settings.floorY / voxelSize);
        int tunnelHeightVx = Mathf.CeilToInt(height / voxelSize);

        Vector3Int cV = WorldToVoxel(center);
        int rV        = Mathf.CeilToInt(radius / voxelSize);

        for (int dx = -rV; dx <= rV; dx++)
        for (int dz = -rV; dz <= rV; dz++)
        {
            float wx = dx * voxelSize;
            float wz = dz * voxelSize;
            if (wx * wx + wz * wz > radius * radius) continue;

            for (int dy = 0; dy < tunnelHeightVx; dy++)
            {
                Vector3Int vp = new Vector3Int(
                    cV.x + dx,
                    floorVoxelY + dy,
                    cV.z + dz);

                if (!IsValidVoxel(vp)) continue;
                density[vp.x, vp.y, vp.z] = -1f;
            }
        }
    }

    #endregion
    // --------------------------------------------------------------------
    #region Ruido & post‑procesado (sin cambios sustanciales)

    private void ApplyOrganicNoise()
    {
        float warpFreq = settings.warpFrequency;
        float warpAmp  = settings.warpStrength;

        for (int x = 0; x < size.x; x++)
        for (int y = 0; y < size.y; y++)
        for (int z = 0; z < size.z; z++)
        {
            float d = density[x, y, z];
            if (d <= 0.1f || d >= 0.9f) continue; // sólo cercanos a superficie

            Vector3 wPos = VoxelToWorld(new Vector3Int(x, y, z));

            // Domain‑warp antes del ruido principal
            Vector3 warp = new(
                Perlin3D(wPos * warpFreq + Vector3.right   * 17.1f),
                Perlin3D(wPos * warpFreq + Vector3.up      * 11.3f),
                Perlin3D(wPos * warpFreq + Vector3.forward * 29.7f));

            wPos += warp * warpAmp;

            // Ruido 3D con 5 octavas
            float noise = Perlin3D(wPos + settings.noiseOffset);
            float strength = GetNoiseStrengthForY(y);

            density[x, y, z] = Mathf.Clamp01(d + noise * strength);
        }
    }

    private float GetNoiseStrengthForY(int y)
    {
        float t = y / (float)size.y;
        if (t < 0.3f)      return 0f;                                     // sin ruido en piso
        else if (t > 0.7f) return settings.ceilingNoiseStrength * 0.1f;   // techo
        else               return settings.wallNoiseStrength    * 0.1f;   // paredes laterales
    }

    // Perlin "pseudo‑3D" con 5 octavas. Devuelve –0.5 … +0.5
    private float Perlin3D(Vector3 p)
    {
        float total = 0f, amp = 1f, freq = settings.noiseScale;
        for (int o = 0; o < 5; o++)
        {
            total += (Mathf.PerlinNoise(p.x * freq, p.y * freq) +
                      Mathf.PerlinNoise(p.y * freq, p.z * freq) +
                      Mathf.PerlinNoise(p.z * freq, p.x * freq)) * (amp / 3f);
            amp  *= 0.5f;
            freq *= 2f;
        }
        return total - 0.5f;
    }

    #endregion
    // --------------------------------------------------------------------
    #region Suavizado selectivo + piso plano final

    private void SmoothTransitions()
    {
        float iso = settings.isoLevel;
        float[,,] temp = (float[,,])density.Clone();

        for (int x = 1; x < size.x - 1; x++)
        for (int y = 1; y < size.y - 1; y++)
        for (int z = 1; z < size.z - 1; z++)
        {
            float d = density[x, y, z];
            if (d < iso - 0.25f || d > iso + 0.25f) continue;

            float sum = 0f;
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            for (int dz = -1; dz <= 1; dz++)
                sum += density[x + dx, y + dy, z + dz];

            temp[x, y, z] = sum / 27f;
        }

        density = temp;
    }

    private void FlattenFloor(int layers)
    {
        for (int y = 0; y < layers; y++)
        for (int x = 0; x < size.x; x++)
        for (int z = 0; z < size.z; z++)
           if (density[x, y, z] < settings.isoLevel) density[x, y, z] =  1f;
    }

    #endregion
    // --------------------------------------------------------------------
    #region Helpers World↔Voxel & Bounds

    private Vector3Int WorldToVoxel(Vector3 w)
    {
        return new Vector3Int(
            Mathf.RoundToInt(w.x / voxelSize),
            Mathf.RoundToInt(w.y / voxelSize),
            Mathf.RoundToInt(w.z / voxelSize));
    }

    private Vector3 VoxelToWorld(Vector3Int v)
    {
        return new Vector3(v.x * voxelSize, v.y * voxelSize, v.z * voxelSize);
    }

    private bool IsValidVoxel(int x, int y, int z)
        => x >= 0 && x < size.x && y >= 0 && y < size.y && z >= 0 && z < size.z;

    private bool IsValidVoxel(Vector3Int v) => IsValidVoxel(v.x, v.y, v.z);

    // Para Marching‑Cubes
    public float GetDensity(int x, int y, int z)
        => IsValidVoxel(x, y, z) ? density[x, y, z] : 1f;

    public float GetDensity(Vector3Int v) => GetDensity(v.x, v.y, v.z);

    #endregion
}
