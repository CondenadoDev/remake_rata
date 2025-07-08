using TMPro;
using UISystem.Panels;
using UnityEngine;

public class HUDPanel : BaseUIPanel
{
    [Header("HUD Elements")]
    [SerializeField] private Transform healthBar;
    [SerializeField] private Transform manaBar;
    [SerializeField] private Transform experienceBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Transform minimap;
        
    private PlayerData playerData;
        
    protected override void OnInitialize()
    {
        // Subscribe to player data changes
        GameDataManager.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
            
        // Initial update
        UpdateHUD();
    }

    private void OnPlayerDataChanged(PlayerData data)
    {
        playerData = data;
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (playerData == null) return;
            
        // Bind to player data
        BindToData(playerData);
    }

    protected override void OnShow()
    {
        base.OnShow();
        UpdateHUD();
    }

    private void OnDestroy()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
        }
    }
}