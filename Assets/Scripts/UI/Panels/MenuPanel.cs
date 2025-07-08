using System;
using System.Collections.Generic;
using UISystem.Core;
using UISystem.Panels;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : BaseUIPanel
{
    [Header("Menu Settings")]
    [SerializeField] private List<MenuButton> menuButtons;
    [SerializeField] private bool closeOnSelection = true;
        
    protected override void OnInitialize()
    {
        foreach (var button in menuButtons)
        {
            if (button.button != null)
            {
                var targetPanel = button.targetPanelId;
                button.button.onClick.AddListener(() => OnMenuButtonClicked(targetPanel));
            }
        }
    }

    private void OnMenuButtonClicked(string targetPanelId)
    {
        UIManager.Instance.ShowPanel(targetPanelId);
            
        if (closeOnSelection)
        {
            Hide();
        }
    }

    [Serializable]
    public class MenuButton
    {
        public Button button;
        public string targetPanelId;
        public bool requiresAuthentication;
    }
}