using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [HideInInspector] public TextMeshProUGUI promptText;
    [HideInInspector] public GameObject promptPanel;
    
    void OnEnable()
    {
        UIEvents.OnShowInteractionPrompt += ShowPrompt;
        UIEvents.OnHideInteractionPrompt += HidePrompt;
    }
    
    void OnDisable()
    {
        UIEvents.OnShowInteractionPrompt -= ShowPrompt;
        UIEvents.OnHideInteractionPrompt -= HidePrompt;
    }
    
    void ShowPrompt(string text)
    {
        promptPanel?.SetActive(true);
        if (promptText != null)
        {
            promptText.text = text;
        }
    }
    
    void HidePrompt()
    {
        promptPanel?.SetActive(false);
    }
}