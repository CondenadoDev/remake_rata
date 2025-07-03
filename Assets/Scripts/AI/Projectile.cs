using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // Velocidad del proyectil
    public float lifeTime = 5f; // Tiempo antes de destruirse
    public float damage = 8f; // Daño infligido

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("El proyectil ha impactado con: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Proyectil impactó al jugador! Daño aplicado: " + damage);
        
            // Obtener el sistema de salud del jugador
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("Se aplicó daño al jugador: " + damage);
            }
            else
            {
                Debug.LogWarning("El jugador no tiene un HealthSystem asignado.");
            }

            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}