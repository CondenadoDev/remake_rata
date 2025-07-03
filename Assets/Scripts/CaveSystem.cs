/*  CaveSystem.cs
 *  Punto de entrada para generar un sistema de cuevas procedural mediante:
 *      1. CaveLayoutGenerator  – define salas y túneles.
 *      2. CaveDensityField     – traduce el layout a un campo escalar 3D.
 *      3. CaveMeshGenerator    – Marching‑Cubes sobre el campo.
 *
 *  Revisión 2025‑07‑01
 *  ------------------
 *  • Sin duplicados en CaveSettings (flat‑floor y parámetros de irregularidad).
 *  • Añadidos: irregularity, irrNoiseScale, irrSeed, floorY, roomHeight, tunnelHeight.
 *  • Renombrado floorHeight → heightNoiseThreshold (para distinguir de floorY).
 *  • Mantiene compatibilidad con scripts existentes (DensityField, MeshGenerator).
 */

using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Conjunto de parámetros globales para la generación de cuevas.
/// </summary>
[System.Serializable]
public class CaveSettings
{
    #region Size & Resolution
    [Header("Tamaño y Resolución")]
    public Vector3Int chunkSize = new(64, 64, 64);
    [Min(0.1f)] public float voxelSize = 1f;
    #endregion

    #region Rooms & Tunnels
    [Header("Salas y Túneles – Básico")]
    [Min(1)]  public int   numRooms       = 8;
    [Min(1f)] public float minRoomRadius = 8f;
    [Min(1f)] public float maxRoomRadius = 15f;
    [Min(0.5f)] public float tunnelRadius = 3f;
    
    [Header("Altura de tuneles")]
    [Min(1)] public float tunnelMinHeight = 3f;
    public float tunnelMaxHeight = 3f;

    [Header("Salas y Túneles – Pisos Planos e Irregularidad")]
    [Tooltip("Altura global del piso (metros, mundo).")]
    public float floorY = 0f;
    [Tooltip("Altura interna de las salas (metros).")]
    public float roomHeight = 4f;

    [Range(0f,1f)] public float irregularity   = 0.25f;   // 0 = círculo perfecto
    public float      irrNoiseScale = 0.5f;    // frecuencia del ruido
    public int        irrSeed       = 12345;   // semilla Perlin para contorno
    #endregion

    #region Terrain sculpting
    [Header("Esculpido de Terreno (Ruido Global)")]
    // Mantener este parámetro para compatibilidad con FlattenFloor()
    [Tooltip("Umbral vertical que define hasta dónde el ruido afecta el suelo. 0 = piso totalmente plano.")]
    [Range(0f,1f)] public float heightNoiseThreshold = 0f;

    public float wallNoiseStrength    = 2f;
    public float ceilingNoiseStrength = 3f;
    #endregion

    #region Noise
    [Header("Ruido Principal (interior de la cueva)")]
    [Range(0.01f,1f)] public float noiseScale = 0.1f;
    public Vector3 noiseOffset;
    public int     seed = 12345;

    // Warp noise – añade irregularidades finas ---------------------------------
    [Header("Warp Noise (domain‑warp)")]
    [Tooltip("Cuánto se distorsiona la posición antes de muestrear el ruido principal.")]
    public float warpStrength = 2f;
    [Tooltip("Frecuencia del ruido de warp (más alta = detalles pequeños).")]
    public float warpFrequency = 0.2f;
    #endregion

    #region Marching Cubes
    [Header("Marching Cubes")]
    [Range(0f,1f)] public float isoLevel = 0.5f;
    #endregion
}

/// <summary>
/// Orquesta la generación procedural del sistema de cuevas.
/// </summary>
[ExecuteAlways]
public class CaveSystem : MonoBehaviour
{
    #region Inspector
    [Header("Configuración Global")]
    public CaveSettings settings = new();

    [Header("Material de la Cueva")]
    public Material caveMaterial;

    [Header("Debug / Flujo")]
    public bool showGizmos      = true;
    public bool generateOnStart = false;
    #endregion

    #region Internals
    private CaveLayoutGenerator layoutGenerator;
    private CaveDensityField    densityField;
    private CaveMeshGenerator   meshGenerator;

    private CaveLayout  currentLayout;
    private GameObject  caveObject;
    #endregion

    #region Unity Events
    private void Awake()  => InitializeSystems();

    private void Start()
    {
        if (generateOnStart)
        {
            if (Application.isPlaying) StartCoroutine(GenerateCaveCoroutine());
            else                       GenerateCaveImmediate();
        }
    }
    #endregion

    //--------------------------------------------------------------------
    #region Initialization helpers
    private void InitializeSystems()
    {
        layoutGenerator ??= new CaveLayoutGenerator();
        densityField    ??= new CaveDensityField();
        meshGenerator   ??= new CaveMeshGenerator();
    }
    #endregion

    //--------------------------------------------------------------------
    #region Context‑menu Buttons
    [ContextMenu("Generate Cave")]
    public void GenerateCave()
    {
        if (Application.isPlaying) StartCoroutine(GenerateCaveCoroutine());
        else                       GenerateCaveImmediate();
    }

    [ContextMenu("Clear Cave")]
    public void ClearCave()
    {
        if (caveObject == null) return;

#if UNITY_EDITOR
        if (Application.isPlaying) Destroy(caveObject);
        else                       DestroyImmediate(caveObject);
#else
        Destroy(caveObject);
#endif
        caveObject = null;
    }
    #endregion

    //--------------------------------------------------------------------
    #region Generation (Edit‑mode immediate)
    public void GenerateCaveImmediate()
    {
        Debug.Log("[CaveSystem] Generating cave (Edit mode)…");

        InitializeSystems();
        ClearCave();
        Random.InitState(settings.seed);
        GenerateAllStages();

#if UNITY_EDITOR
        if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }
    #endregion

    //--------------------------------------------------------------------
    #region Generation (Play‑mode coroutine)
    private IEnumerator GenerateCaveCoroutine()
    {
        Debug.Log("[CaveSystem] Generating cave…");

        InitializeSystems();
        ClearCave();
        Random.InitState(settings.seed);

        yield return StartCoroutine(GenerateLayoutStage());
        yield return StartCoroutine(GenerateDensityStage());
        yield return StartCoroutine(GenerateMeshStage());

        Debug.Log("[CaveSystem] Cave generated!");
    }

    private IEnumerator GenerateLayoutStage()
    {
        currentLayout = layoutGenerator.GenerateLayout(settings);
        yield return null;
    }

    private IEnumerator GenerateDensityStage()
    {
        densityField.Initialize(settings);
        densityField.GenerateFromLayout(currentLayout);
        yield return null;
    }

    private IEnumerator GenerateMeshStage()
    {
        Mesh m = meshGenerator.GenerateMesh(densityField, settings.isoLevel);
        BuildMeshObject(m);
        yield return null;
    }
    #endregion

    //--------------------------------------------------------------------
    #region Shared generation logic
    private void GenerateAllStages()
    {
        currentLayout = layoutGenerator.GenerateLayout(settings);
        densityField.Initialize(settings);
        densityField.GenerateFromLayout(currentLayout);

        Mesh m = meshGenerator.GenerateMesh(densityField, settings.isoLevel);
        BuildMeshObject(m);
    }

    private void BuildMeshObject(Mesh caveMesh)
    {
        if (caveObject == null)
        {
            caveObject = new GameObject("Cave");
            caveObject.transform.SetParent(transform, false);
            caveObject.AddComponent<MeshFilter>();
            caveObject.AddComponent<MeshRenderer>();
            caveObject.AddComponent<MeshCollider>();
        }

        var mf = caveObject.GetComponent<MeshFilter>();
        var mr = caveObject.GetComponent<MeshRenderer>();
        var mc = caveObject.GetComponent<MeshCollider>();

        mf.sharedMesh     = caveMesh;
        mr.sharedMaterial = caveMaterial;
        mc.sharedMesh     = caveMesh;
    }
    #endregion

    //--------------------------------------------------------------------
    #region Gizmos & Debug
    private void OnDrawGizmos()
    {
        if (!showGizmos || currentLayout == null) return;

        Gizmos.color = Color.green;
        foreach (var room in currentLayout.rooms)
            Gizmos.DrawWireSphere(room.center, room.radius);

        Gizmos.color = Color.cyan;
        foreach (var c in currentLayout.connections)
            Gizmos.DrawLine(c.start, c.end);
    }
    #endregion

    //--------------------------------------------------------------------
    #region Validation
    private void OnValidate()
    {
        // Generar un offset pseudo‑aleatorio si está en (0,0,0)
        if (settings.noiseOffset == Vector3.zero)
        {
            settings.noiseOffset = new Vector3(
                Random.Range(-1000f, 1000f),
                Random.Range(-1000f, 1000f),
                Random.Range(-1000f, 1000f));
        }
    }
    #endregion
}
