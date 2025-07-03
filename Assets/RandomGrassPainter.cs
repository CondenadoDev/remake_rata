using UnityEngine;
using UnityEditor;

public class RandomGrassPainter : MonoBehaviour
{
    public Terrain terrain;
    public int[] detailIndexes; // Índices de los detalles que quieres usar
    public int amountPerClick = 100;
    public float radius = 5f;

#if UNITY_EDITOR
    [ContextMenu("Pintar detalles aleatorios")]
    void PaintRandomDetails()
    {
        if (terrain == null || detailIndexes.Length == 0) return;

        TerrainData data = terrain.terrainData;

        int detailWidth = data.detailWidth;
        int detailHeight = data.detailHeight;

        Vector3 terrainPos = terrain.transform.position;

        for (int i = 0; i < amountPerClick; i++)
        {
            float worldX = Random.Range(0f, data.size.x);
            float worldZ = Random.Range(0f, data.size.z);

            int mapX = Mathf.FloorToInt((worldX / data.size.x) * detailWidth);
            int mapZ = Mathf.FloorToInt((worldZ / data.size.z) * detailHeight);

            int detailIndex = detailIndexes[Random.Range(0, detailIndexes.Length)];

            int[,] layer = data.GetDetailLayer(mapX, mapZ, 1, 1, detailIndex);
            layer[0, 0] = 1; // Pintar una planta
            data.SetDetailLayer(mapX, mapZ, detailIndex, layer);
        }
    }
#endif
}