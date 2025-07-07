using UnityEngine;
using DungeonSystem;
using DungeonSystem.Settings;

/// <summary>
/// Verifica que todo esté configurado correctamente
/// </summary>
public class DungeonSetupVerifier : MonoBehaviour
{
    [Header("Components to Check")]
    public DungeonManager dungeonManager;
    public GameObject player;
    
    [Header("Verification Results")]
    [TextArea(10, 20)]
    public string verificationReport = "Click 'Verify Setup' to check";
    
    [ContextMenu("Verify Setup")]
    public void VerifySetup()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== DUNGEON SETUP VERIFICATION ===");
        report.AppendLine($"Time: {System.DateTime.Now}");
        report.AppendLine();
        
        // 1. Verificar DungeonManager
        report.AppendLine("1. DUNGEON MANAGER:");
        if (dungeonManager == null)
        {
            dungeonManager = FindFirstObjectByType<DungeonManager>();
        }
        
        if (dungeonManager != null)
        {
            report.AppendLine("✓ DungeonManager found");
            report.AppendLine($"  - Auto Generate: {dungeonManager.autoGenerateOnStart}");
            report.AppendLine($"  - Position: {dungeonManager.transform.position}");
            
            // Verificar settings
            report.AppendLine("\n2. SETTINGS:");
            
            if (dungeonManager.generationSettings != null)
            {
                report.AppendLine("✓ Generation Settings OK");
                report.AppendLine($"  - Seed: {dungeonManager.generationSettings.seed}");
                report.AppendLine($"  - Size: {dungeonManager.generationSettings.dungeonWidth}x{dungeonManager.generationSettings.dungeonHeight}");
            }
            else
            {
                report.AppendLine("✗ Generation Settings MISSING!");
            }
            
            if (dungeonManager.renderSettingsDungeon != null)
            {
                report.AppendLine("✓ Render Settings OK");
                report.AppendLine($"  - 3D Assets: {dungeonManager.renderSettingsDungeon.generate3DAssets}");
                report.AppendLine($"  - Floor Prefab: {(dungeonManager.renderSettingsDungeon.floorPrefab != null ? "OK" : "MISSING")}");
                report.AppendLine($"  - Wall Prefab: {(dungeonManager.renderSettingsDungeon.wallPrefab != null ? "OK" : "MISSING")}");
                report.AppendLine($"  - Door Prefab: {(dungeonManager.renderSettingsDungeon.doorPrefab != null ? "OK" : "MISSING")}");
            }
            else
            {
                report.AppendLine("✗ Render Settings MISSING!");
            }
            
            if (dungeonManager.spawnSettings != null)
            {
                report.AppendLine("✓ Spawn Settings OK");
            }
            else
            {
                report.AppendLine("✗ Spawn Settings MISSING!");
            }
            
            if (dungeonManager.startingPointCriteria != null)
            {
                report.AppendLine("✓ Starting Point Criteria OK");
                report.AppendLine($"  - Prefer Edge: {dungeonManager.startingPointCriteria.preferMapEdge}");
                report.AppendLine($"  - Create Entrance: {dungeonManager.startingPointCriteria.createExteriorEntrance}");
            }
            else
            {
                report.AppendLine("✗ Starting Point Criteria MISSING!");
            }
        }
        else
        {
            report.AppendLine("✗ NO DUNGEON MANAGER FOUND!");
        }
        
        // 3. Verificar Player
        report.AppendLine("\n3. PLAYER:");
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player != null)
        {
            report.AppendLine("✓ Player found");
            report.AppendLine($"  - Name: {player.name}");
            report.AppendLine($"  - Position: {player.transform.position}");
            report.AppendLine($"  - Tag: {player.tag}");
        }
        else
        {
            report.AppendLine("✗ NO PLAYER FOUND! (Check 'Player' tag)");
        }
        
        // 4. Verificar Portal
        report.AppendLine("\n4. PORTAL:");
        var portal = GetComponent<DungeonSystem.Interaction.DungeonPortalInteractable>();
        var simpleGen = GetComponent<DungeonSystem.Interaction.SimpleDungeonGenerator>();

        if (portal != null || simpleGen != null)
        {
            report.AppendLine("✓ Portal script found");
            report.AppendLine($"  - Type: {(portal != null ? "DungeonPortalInteractable" : "SimpleDungeonGenerator")}");
        }
        else
        {
            report.AppendLine("✗ No portal script found on this object!");
        }
        
        // 5. Verificar estado actual
        report.AppendLine("\n5. CURRENT STATE:");
        if (dungeonManager != null && dungeonManager.DungeonData != null)
        {
            report.AppendLine("⚠ DUNGEON ALREADY GENERATED!");
            report.AppendLine($"  - Rooms: {dungeonManager.DungeonData.rooms.Count}");
            report.AppendLine($"  - Has Starting Room: {dungeonManager.DungeonData.startingRoom != null}");
        }
        else
        {
            report.AppendLine("✓ No dungeon generated yet");
        }
        
        // Guardar reporte
        verificationReport = report.ToString();
        Debug.Log(verificationReport);
    }
    
    [ContextMenu("Fix Common Issues")]
    public void FixCommonIssues()
    {
        Debug.Log("Fixing common issues...");
        
        // 1. Desactivar auto generación
        if (dungeonManager != null)
        {
            dungeonManager.autoGenerateOnStart = false;
            Debug.Log("✓ Disabled auto generation");
        }
        
        // 2. Asignar tag al jugador si no lo tiene
        if (player != null && player.tag != "Player")
        {
            player.tag = "Player";
            Debug.Log("✓ Set player tag to 'Player'");
        }
        
        // 3. Limpiar dungeon si ya existe
        if (dungeonManager != null && dungeonManager.DungeonData != null)
        {
            dungeonManager.ClearDungeon();
            Debug.Log("✓ Cleared existing dungeon");
        }
        
        Debug.Log("Common issues fixed!");
    }
    
    void Start()
    {
        // Auto verificar al inicio
        Invoke("VerifySetup", 0.1f);
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(DungeonSetupVerifier))]
public class DungeonSetupVerifierEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        UnityEditor.EditorGUILayout.Space();
        
        if (GUILayout.Button("Verify Setup", GUILayout.Height(30)))
        {
            ((DungeonSetupVerifier)target).VerifySetup();
        }
        
        if (GUILayout.Button("Fix Common Issues", GUILayout.Height(25)))
        {
            ((DungeonSetupVerifier)target).FixCommonIssues();
        }
        
        UnityEditor.EditorGUILayout.HelpBox(
            "This tool checks if everything is configured correctly.\n" +
            "Run 'Verify Setup' to see what might be wrong.", 
            UnityEditor.MessageType.Info);
    }
}
#endif