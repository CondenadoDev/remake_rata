using UnityEngine;

/// <summary>
/// Ensures the HUD is visible and the player spawns at the designated spawn point
/// when a gameplay scene loads.
/// </summary>
public class GameplayInitializer : MonoBehaviour
{
    [SerializeField] private bool showHUDOnStart = true;

    void Start()
    {
        SpawnPlayerAtPoint();
        if (showHUDOnStart)
        {
            ShowHUD();
        }
    }

    void SpawnPlayerAtPoint()
    {
        if (GameManager.Instance == null)
            return;

        var player = GameManager.Instance.player;
        if (player == null)
            player = FindObjectOfType<PlayerStateMachine>();

        if (player == null)
            return;

        Transform spawn = GameManager.Instance.playerSpawnPoint;
        if (spawn != null)
        {
            player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        }
        else
        {
            Debug.LogWarning("[GameplayInitializer] No player spawn point assigned.");
        }
    }

    void ShowHUD()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanel("HUD");
        }
        else
        {
            Debug.LogWarning("[GameplayInitializer] UIManager not found to show HUD.");
        }
    }
}
