using UnityEngine;
using DungeonSystem;

/// <summary>
/// Script de prueba MUY básico para encontrar el problema
/// </summary>
public class TestDungeonBasic : MonoBehaviour
{
    public DungeonManager dungeonManager;
    private bool testStarted = false;
    
    void Start()
    {
        Debug.Log("=== TEST DUNGEON BASIC STARTED ===");
        Debug.LogWarning("This is a WARNING test - you should see this!");
        Debug.LogError("This is an ERROR test - you should see this too!");
        
        if (dungeonManager == null)
        {
            dungeonManager = FindObjectOfType<DungeonManager>();
            Debug.Log($"DungeonManager found: {dungeonManager != null}");
        }
        
        if (dungeonManager != null)
        {
            Debug.Log($"AutoGenerate was: {dungeonManager.autoGenerateOnStart}");
            dungeonManager.autoGenerateOnStart = false;
            Debug.Log("AutoGenerate set to: false");
        }
    }
    
    void Update()
    {
        // Probar con diferentes teclas
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("===== E KEY PRESSED =====");
            TestGenerateSimple();
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("===== T KEY PRESSED - Testing logs =====");
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("===== Y KEY PRESSED - Clearing =====");
            if (dungeonManager != null)
            {
                dungeonManager.ClearDungeon();
                Debug.Log("Dungeon cleared");
            }
        }
    }
    
    void TestGenerateSimple()
    {
        if (testStarted)
        {
            Debug.Log("Test already in progress!");
            return;
        }
        
        if (dungeonManager == null)
        {
            Debug.LogError("NO DUNGEON MANAGER!");
            return;
        }
        
        testStarted = true;
        Debug.Log("Starting generation test...");
        
        try
        {
            // Probar SOLO la primera parte
            Debug.Log("Step 1: Clearing...");
            dungeonManager.ClearDungeon();
            Debug.Log("Step 1: DONE");
            
            Debug.Log("Step 2: Generating structure...");
            dungeonManager.GenerateMapStructure();
            Debug.Log("Step 2: DONE");
            
            // Parar aquí para ver si es el problema
            Debug.Log("STOPPING HERE - Check if it works");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EXCEPTION: {e.Message}");
            Debug.LogError($"STACK: {e.StackTrace}");
        }
        finally
        {
            testStarted = false;
            Debug.Log("Test finished");
        }
    }
}