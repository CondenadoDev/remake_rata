using TMPro;
using UnityEngine;

public class ComboDisplayUI : MonoBehaviour
{
    [HideInInspector] public TextMeshProUGUI comboText;
    [HideInInspector] public GameObject comboPanel;
    private Coroutine hideCoroutine;
    
    void OnEnable()
    {
        UIEvents.OnComboChanged += ShowCombo;
    }
    
    void OnDisable()
    {
        UIEvents.OnComboChanged -= ShowCombo;
    }
    
    void ShowCombo(int comboCount)
    {
        if (comboCount > 1)
        {
            comboPanel?.SetActive(true);
            if (comboText != null)
            {
                comboText.text = $"COMBO x{comboCount}";
            }
            
            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideAfterDelay(2f));
        }
        else
        {
            comboPanel?.SetActive(false);
        }
    }
    
    System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        comboPanel?.SetActive(false);
    }
}