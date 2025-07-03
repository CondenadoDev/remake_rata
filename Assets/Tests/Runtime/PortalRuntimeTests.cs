using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DungeonSystem.Interaction;
using DungeonSystem.Settings;
using DungeonSystem;

public class PortalRuntimeTests
{
    [UnityTest]
    public IEnumerator PortalFindsManagerAndGenerates()
    {
        var managerGO = new GameObject("DM");
        var dm = managerGO.AddComponent<DungeonManager>();
        dm.generationSettings = ScriptableObject.CreateInstance<GenerationSettings>();

        var portalGO = new GameObject("Portal");
        var portal = portalGO.AddComponent<DungeonPortalInteractable>();
        portalGO.tag = "Player";

        yield return null; // wait a frame for Start()

        Assert.IsNotNull(portal);
        Object.DestroyImmediate(managerGO);
        Object.DestroyImmediate(portalGO);
    }
}
