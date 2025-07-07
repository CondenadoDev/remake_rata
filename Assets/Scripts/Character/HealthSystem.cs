using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class HealthChangedEvent : UnityEvent<float, float> { }

[System.Serializable]
public class DeathEvent : UnityEvent { }

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public HealthChangedEvent OnHealthChanged;
    public DeathEvent OnDeath;

    // 🆕 Evento global estático
    public static event System.Action OnAnyPlayerDeath;

    [Header("Invencibilidad")]
    public float invincibilityDuration = 0.5f;
    private bool isInvincible = false;

    // Feedback visual
    private Renderer rend;
    private Color originalColor;

    private void Start()
    {
        currentHealth = maxHealth;
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (GetComponent<PlayerController>() != null)
        {
            GameEvents.TriggerPlayerHealthChanged(currentHealth);
        }

        if (rend != null)
        {
            StartCoroutine(FlashDamage());
        }

        // Notifica a otros componentes
        SendMessage("OnDamageTaken", damage, SendMessageOptions.DontRequireReceiver);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private IEnumerator FlashDamage()
    {
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        rend.material.color = originalColor;
    }

    private void Die()
    {
        Debug.Log($"{name} ha muerto.");
        OnDeath?.Invoke();

        bool isPlayer = GetComponent<PlayerController>() != null;

        if (isPlayer)
        {
            GameEvents.TriggerPlayerHealthChanged(0);
            GameEvents.TriggerPlayerDied();
            OnAnyPlayerDeath?.Invoke(); // 🆕 Evento global para GameManager
        }
        else
        {
            GameEvents.TriggerEnemyKilled(gameObject);
            Destroy(gameObject);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void RestoreHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
