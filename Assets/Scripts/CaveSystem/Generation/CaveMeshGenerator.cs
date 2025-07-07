// ============================================================================
//  CaveMeshGenerator.cs · 2025-07-01
//  --------------------------------------------------------------------------
//  Marching-Cubes básico (uint32) + opción de malla doble-cara.
//  - makeTwoSided duplica cada triángulo en orden invertido.
//  - Normales suavizadas después de la duplicación.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;   // IndexFormat

public class CaveMeshGenerator
{
    // ------------------------------------------------------------------ ▼
    #region Parámetros públicos

    /// <summary>
    /// Si está en <c>true</c>, la malla resultante tendrá
    /// caras hacia dentro y hacia fuera (ideal para cuevas).
    /// </summary>
    public bool insideOnly = true;
    public bool makeTwoSided = false;

    #endregion
    // ------------------------------------------------------------------ ▼
    #region API principal

    public Mesh GenerateMesh(CaveDensityField field, float isoLevel = 0.5f)
    {
        ResetCache();

        Vector3Int size = field.Size;
        float step      = field.VoxelSize;

        // --- Marching-Cubes ------------------------------------------------
        for (int x = 0; x < size.x - 1; x++)
        for (int y = 0; y < size.y - 1; y++)
        for (int z = 0; z < size.z - 1; z++)
            MarchCube(field, x, y, z, step, isoLevel);

        // --- Construir malla ----------------------------------------------
        var mesh = new Mesh
        {
            indexFormat = IndexFormat.UInt32,
            vertices    = _vertices.ToArray(),
            normals     = _normals .ToArray(),
            triangles   = _tris    .ToArray()
        };

        // Malla doble-cara opcional
        if (makeTwoSided) MakeMeshTwoSided(mesh);
        if(insideOnly) FlipMesh(mesh);
        mesh.RecalculateBounds();
        mesh.Optimize();
        return mesh;
    }

    #endregion
    // ------------------------------------------------------------------ ▼
    #region Buffers internos

    private readonly List<Vector3> _vertices = new(4096);
    private readonly List<Vector3> _normals  = new(4096);
    private readonly List<int>     _tris     = new(4096);
    private readonly Dictionary<LongEdgeKey,int> _lookup = new(4096);

    private void ResetCache()
    {
        _vertices.Clear();
        _normals .Clear();
        _tris    .Clear();
        _lookup  .Clear();
    }

    #endregion
    // ------------------------------------------------------------------ ▼
    #region Marching-Cubes núcleo

    private void MarchCube(CaveDensityField f,
                           int x, int y, int z,
                           float step, float iso)
    {
        // Densidades de los 8 vértices del voxel
        float[] d = new float[8];
        d[0] = f.GetDensity(x,     y,     z);
        d[1] = f.GetDensity(x + 1, y,     z);
        d[2] = f.GetDensity(x + 1, y,     z + 1);
        d[3] = f.GetDensity(x,     y,     z + 1);
        d[4] = f.GetDensity(x,     y + 1, z);
        d[5] = f.GetDensity(x + 1, y + 1, z);
        d[6] = f.GetDensity(x + 1, y + 1, z + 1);
        d[7] = f.GetDensity(x,     y + 1, z + 1);

        // Índice del caso MC
        int ci = 0;
        for (int i = 0; i < 8; i++) if (d[i] < iso) ci |= 1 << i;
        int edgeMask = MCTables.edgeTable[ci];
        if (edgeMask == 0) return;

        // Posiciones de los 8 vértices del voxel
        Vector3 p0 = new(x * step,         y * step,         z * step);
        Vector3 p1 = new((x + 1) * step,   y * step,         z * step);
        Vector3 p2 = new((x + 1) * step,   y * step,       (z + 1) * step);
        Vector3 p3 = new(x * step,         y * step,       (z + 1) * step);
        Vector3 p4 = new(x * step,       (y + 1) * step,    z * step);
        Vector3 p5 = new((x + 1) * step, (y + 1) * step,    z * step);
        Vector3 p6 = new((x + 1) * step, (y + 1) * step,  (z + 1) * step);
        Vector3 p7 = new(x * step,       (y + 1) * step,  (z + 1) * step);

        // Interpolar en aristas presentes
        Vector3[] v = new Vector3[12];
        if ((edgeMask &   1) != 0) v[0]  = Lerp(p0, p1, d[0], d[1], iso);
        if ((edgeMask &   2) != 0) v[1]  = Lerp(p1, p2, d[1], d[2], iso);
        if ((edgeMask &   4) != 0) v[2]  = Lerp(p2, p3, d[2], d[3], iso);
        if ((edgeMask &   8) != 0) v[3]  = Lerp(p3, p0, d[3], d[0], iso);
        if ((edgeMask &  16) != 0) v[4]  = Lerp(p4, p5, d[4], d[5], iso);
        if ((edgeMask &  32) != 0) v[5]  = Lerp(p5, p6, d[5], d[6], iso);
        if ((edgeMask &  64) != 0) v[6]  = Lerp(p6, p7, d[6], d[7], iso);
        if ((edgeMask & 128) != 0) v[7]  = Lerp(p7, p4, d[7], d[4], iso);
        if ((edgeMask & 256) != 0) v[8]  = Lerp(p0, p4, d[0], d[4], iso);
        if ((edgeMask & 512) != 0) v[9]  = Lerp(p1, p5, d[1], d[5], iso);
        if ((edgeMask &1024) != 0) v[10] = Lerp(p2, p6, d[2], d[6], iso);
        if ((edgeMask &2048) != 0) v[11] = Lerp(p3, p7, d[3], d[7], iso);

        // Añadir triángulos
        int[] row = MCTables.triTable[ci];
        for (int i = 0; row[i] != -1; i += 3)
        {
            int a = AddVertex(v[row[i    ]]);
            int b = AddVertex(v[row[i + 1]]);
            int c = AddVertex(v[row[i + 2]]);

            _tris.Add(a); _tris.Add(b); _tris.Add(c);

            Vector3 n = Vector3.Cross(_vertices[b] - _vertices[a],
                                      _vertices[c] - _vertices[a]).normalized;
            _normals[a] += n;
            _normals[b] += n;
            _normals[c] += n;
        }
    }

    #endregion
    // ------------------------------------------------------------------ ▼
    #region Helpers internos

    private static Vector3 Lerp(Vector3 p1, Vector3 p2, float v1, float v2, float iso)
    {
        if (Mathf.Abs(iso - v1) < 1e-6f) return p1;
        if (Mathf.Abs(iso - v2) < 1e-6f) return p2;
        if (Mathf.Abs(v1  - v2) < 1e-6f) return p1;
        float t = (iso - v1) / (v2 - v1);
        return p1 + t * (p2 - p1);
    }

    private int AddVertex(Vector3 v)
    {
        var key = LongEdgeKey.FromVector(v);
        if (_lookup.TryGetValue(key, out int idx)) return idx;

        idx = _vertices.Count;
        _vertices.Add(v);
        _normals .Add(Vector3.zero);
        _lookup[key] = idx;
        return idx;
    }

    /// <summary>Convierte la malla en doble-cara duplicando e invirtiendo triángulos.</summary>
    private static void MakeMeshTwoSided(Mesh mesh)
    {
        int[] tris = mesh.triangles;
        int    len = tris.Length;

        int[] newTris = new int[len * 2];
        tris.CopyTo(newTris, 0);

        // Copia invertida (B-A-C)
        for (int i = 0; i < len; i += 3)
        {
            newTris[len + i]     = tris[i];
            newTris[len + i + 1] = tris[i + 2];
            newTris[len + i + 2] = tris[i + 1];
        }

        mesh.triangles = newTris;
        mesh.RecalculateNormals();
    }

    // Key compacta (21 bits por eje, ~2 mm resolución) para evitar vértices duplicados
    private readonly struct LongEdgeKey
    {
        private readonly long _pack;
        private LongEdgeKey(long p) => _pack = p;

        public static LongEdgeKey FromVector(Vector3 v)
        {
            const float scale = 10000f;                // 1/scale metros de precisión
            int xi = Mathf.RoundToInt(v.x * scale) & 0x1FFFFF;
            int yi = Mathf.RoundToInt(v.y * scale) & 0x1FFFFF;
            int zi = Mathf.RoundToInt(v.z * scale) & 0x1FFFFF;

            long pack =  (long)xi        |
                        ((long)yi << 21) |
                        ((long)zi << 42);
            return new LongEdgeKey(pack);
        }
        public override int GetHashCode() => _pack.GetHashCode();
        public override bool Equals(object o) => o is LongEdgeKey k && k._pack == _pack;
    }
    /// <summary>Invierte el orden de los vértices en cada triángulo
    /// y recalcula normales (quedan orientadas al interior).</summary>
    private static void FlipMesh(Mesh mesh)
    {
        int[] t = mesh.triangles;
        for (int i = 0; i < t.Length; i += 3)
        {
            // B-A-C en vez de A-B-C
            (t[i + 1], t[i + 2]) = (t[i + 2], t[i + 1]);
        }
        mesh.triangles = t;
        mesh.RecalculateNormals();
    }
    #endregion
}
