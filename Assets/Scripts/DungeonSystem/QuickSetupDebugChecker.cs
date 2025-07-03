using UnityEngine;
using DungeonSystem;

/// <summary>
/// Verifica rápidamente que todo esté configurado correctamente
/// </summary>
public class QuickSetupChecker : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== DUNGEON SYSTEM SETUP CHECK ===");
        
        bool allGood = true;
        
        // 1. Check Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("✅ Player found: " + player.name);
        }
        else
        {
            Debug.LogError("❌ No GameObject with tag 'Player' found!");
            allGood = false;
        }
        
        // 2. Check DungeonManager
        DungeonManager dm = FindFirstObjectByType<DungeonManager>();
        if (dm != null)
        {
            Debug.Log("✅ DungeonManager found");
            
            // Check settings
            if (dm.generationSettings != null)
                Debug.Log("  ✅ Generation Settings OK");
            else
            {
                Debug.LogError("  ❌ Generation Settings MISSING!");
                allGood = false;
            }
            
            if (dm.renderSettingsDungeon != null)
            {
                Debug.Log("  ✅ Render Settings OK");
                
                // Check prefabs
                if (dm.renderSettingsDungeon.floorPrefab == null)
                {
                    Debug.LogError("    ❌ Floor Prefab MISSING!");
                    allGood = false;
                }
                if (dm.renderSettingsDungeon.wallPrefab == null)
                {
                    Debug.LogError("    ❌ Wall Prefab MISSING!");
                    allGood = false;
                }
                if (dm.renderSettingsDungeon.doorPrefab == null)
                {
                    Debug.LogError("    ❌ Door Prefab MISSING!");
                    allGood = false;
                }
            }
            else
            {
                Debug.LogError("  ❌ Render Settings MISSING!");
                allGood = false;
            }
            
            if (dm.spawnSettings != null)
                Debug.Log("  ✅ Spawn Settings OK");
            else
                Debug.LogWarning("  ⚠️ Spawn Settings missing (optional)");
                
            if (dm.startingPointCriteria != null)
                Debug.Log("  ✅ Starting Point Criteria OK");
            else
                Debug.LogWarning("  ⚠️ Starting Point Criteria missing (will use defaults)");
                
            // Check auto generate
            if (dm.autoGenerateOnStart)
            {
                Debug.LogWarning("  ⚠️ Auto Generate is ON - turning it OFF");
                dm.autoGenerateOnStart = false;
            }
        }
        else
        {
            Debug.LogError("❌ No DungeonManager found!");
            allGood = false;
        }
        
        // 3. Check Portal
        var portal = FindFirstObjectByType<DungeonSystem.Interaction.DungeonPortalInteractable>();
        if (portal != null)
        {
            Debug.Log("✅ Portal found");
        }
        else
        {
            Debug.LogWarning("⚠️ No DungeonPortalInteractable found - add it to your portal object");
        }
        
        // Summary
        Debug.Log("=== SETUP CHECK COMPLETE ===");
        if (allGood)
        {
            Debug.Log("✅ ALL SYSTEMS GO! Press E near the portal to generate dungeon.");
        }
        else
        {
            Debug.LogError("❌ SETUP INCOMPLETE! Fix the errors above.");
        }
    }
}