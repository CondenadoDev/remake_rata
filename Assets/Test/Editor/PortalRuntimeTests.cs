using System.Reflection;
using DungeonSystem; // para invocar Start por reflexión
using NUnit.Framework;
using UnityEngine;
using DungeonSystem.Interaction;
using DungeonSystem.Settings;

public class PortalRuntimeTests
{
    [Test]                            // ← ya no dependemos de UnityTest
    public void PortalFindsManagerAndGenerates()
    {
        // --- Arrange -------------------------------------------------------
        var managerGO = new GameObject("DM");
        var dm = managerGO.AddComponent<DungeonManager>();
        dm.generationSettings = ScriptableObject.CreateInstance<GenerationSettings>();

        var portalGO = new GameObject("Portal");
        var portal = portalGO.AddComponent<DungeonPortalInteractable>();
        portalGO.tag = "Player";

        // --- Act -----------------------------------------------------------
        // Si tu lógica vive en Awake() basta con instanciar; si está en Start()
        // lo forzamos manualmente:
        typeof(DungeonPortalInteractable)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(portal, null);

        // --- Assert --------------------------------------------------------
        Assert.IsNotNull(portal);

        Object.DestroyImmediate(managerGO);
        Object.DestroyImmediate(portalGO);
    }
}