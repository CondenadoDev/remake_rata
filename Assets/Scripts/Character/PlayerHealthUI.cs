using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public HealthSystem playerHealth;       // Referencia al sistema de salud del jugador
    public Image healthBarFill;             // Imagen de la barra de vida

    void Start()
    {
        // Si no se asignó manualmente el HealthSystem, se busca el jugador por etiqueta.
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<HealthSystem>();
            }

            if (playerHealth == null)
            {
                Debug.LogError("PlayerHealthUI: No se encontró el componente HealthSystem en el jugador.");
                return;
            }
        }

        if (healthBarFill == null)
        {
            Debug.LogError("PlayerHealthUI: No se asignó la imagen de la barra de vida (healthBarFill).");
        }

        // Suscribirse al evento usando AddListener.
        playerHealth.OnHealthChanged.AddListener(UpdateHealthUI);

        // Actualizar la UI con el valor inicial de la salud.
        UpdateHealthUI(playerHealth.GetCurrentHealth(), playerHealth.maxHealth);
    }

    void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        float fillAmount = currentHealth / maxHealth;
        Debug.Log("Actualizando barra de vida: " + fillAmount);
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = fillAmount;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthUI);
        }
    }
}