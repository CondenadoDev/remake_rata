// Rendering/MaterialManager.cs
using UnityEngine;
using System.Collections.Generic;
using DungeonSystem.Core;
using DungeonSystem.Settings;

namespace DungeonSystem.Rendering
{
    public class MaterialManager : MonoBehaviour
    {
        private Dictionary<TileType, Material> proceduralMaterials = new Dictionary<TileType, Material>();
        private RenderSettingsDungeon renderSettingsDungeon;

        public void Initialize(RenderSettingsDungeon settingsDungeon)
        {
            renderSettingsDungeon = settingsDungeon;
            
            if (renderSettingsDungeon.useProceduralMaterials)
            {
                CreateProceduralMaterials();
            }
        }

        public Material GetMaterialForTileType(TileType tileType)
        {
            // Primero intentar obtener material procedural
            if (renderSettingsDungeon.useProceduralMaterials && proceduralMaterials.ContainsKey(tileType))
            {
                return proceduralMaterials[tileType];
            }
            
            // Luego intentar obtener material asignado manualmente
            switch (tileType)
            {
                case TileType.Floor:
                    return renderSettingsDungeon.floorMaterial;
                case TileType.Wall:
                    return renderSettingsDungeon.wallMaterial;
                case TileType.Door:
                    return renderSettingsDungeon.doorMaterial;
                default:
                    return null;
            }
        }

        private void CreateProceduralMaterials()
        {
            proceduralMaterials.Clear();
            
            // Material para suelo
            Material floorMat = new Material(Shader.Find("Standard"));
            floorMat.name = "ProceduralFloor";
            floorMat.color = renderSettingsDungeon.floorBaseColor;
            floorMat.SetFloat("_Smoothness", 0.3f);
            proceduralMaterials[TileType.Floor] = floorMat;
            
            // Material para muro
            Material wallMat = new Material(Shader.Find("Standard"));
            wallMat.name = "ProceduralWall";
            wallMat.color = renderSettingsDungeon.wallBaseColor;
            wallMat.SetFloat("_Smoothness", 0.1f);
            proceduralMaterials[TileType.Wall] = wallMat;
            
            // Material para puerta (usar color del muro pero más brillante)
            Material doorMat = new Material(Shader.Find("Standard"));
            doorMat.name = "ProceduralDoor";
            doorMat.color = renderSettingsDungeon.wallBaseColor * 1.2f;
            doorMat.SetFloat("_Smoothness", 0.5f);
            doorMat.SetFloat("_Metallic", 0.3f);
            proceduralMaterials[TileType.Door] = doorMat;
        }

        public void ClearProceduralMaterials()
        {
            foreach (var mat in proceduralMaterials.Values)
            {
                if (mat != null)
                    DestroyImmediate(mat);
            }
            proceduralMaterials.Clear();
        }
    }
}