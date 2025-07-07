using UnityEngine;
using System.Collections;

public class MeleeEnemy : EnemyBase
{
    [Header("Configuración de Ataque")]
    public float attackDamage = 15f;       // Daño del ataque
    public float attackCooldown = 1.5f;      // Tiempo entre ataques
    public float attackRange = 1.5f;         // Rango de impacto para el espadaazo

    [Header("Configuración de Retirada")]
    public float retreatDistance = 1f;       // Distancia de retroceso tras el ataque
    public float retreatDuration = 0.3f;       // Duración del retroceso

    [Header("Referencia a la Espada")]
    public Transform sword;                // Objeto hijo que representa la espada (puede ser un cubo)

    [Header("Animación Idle de la Espada")]
    public float idleAmplitude = 0.1f;       // Amplitud del movimiento vertical
    public float idleFrequency = 1f;         // Frecuencia del movimiento idle
    private Vector3 swordIdlePos;            // Posición local original de la espada

    [Header("Stun y Knockback (al recibir daño)")]
    public float stunDuration = 1f;          // Tiempo de stun tras recibir daño
    public float knockbackDistance = 1f;     // Distancia de knockback
    public float knockbackTime = 0.2f;       // Duración del knockback
    private bool isStunned = false;

    private float attackTimer = 0f;          // Temporizador para el cooldown del ataque
    private bool isSwinging = false;         // Indica si la espada está en medio del ataque

    private new Transform player;

    protected override void Start()
    {
        base.Start();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
        if (sword != null)
            swordIdlePos = sword.localPosition;
    }

    protected override void Update()
    {
        if (isStunned)
            return;

        base.Update();

        // Animación idle de la espada cuando no se está atacando.
        if (sword != null && !isSwinging)
        {
            sword.localPosition = swordIdlePos + new Vector3(0, Mathf.Sin(Time.time * idleFrequency) * idleAmplitude, 0);
        }
    }

    protected override void AttackBehavior()
    {
        if (player == null) return;

        // Orienta al enemigo hacia el jugador (plano XZ)
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPos - transform.position), Time.deltaTime * 10f);

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            StartCoroutine(PerformAttack());
            attackTimer = 0f;
        }
        // Si el jugador se aleja del área de ataque, vuelve a Chase.
        if (Vector3.Distance(transform.position, targetPos) > attackRange)
        {
            currentState = State.Chase;
        }
    }

    IEnumerator PerformAttack()
    {
        currentState = State.Attack;
        // Ejecuta el espadaazo
        yield return StartCoroutine(SwordSwingCoroutine());
        // Realiza una breve retirada para darle dinamismo
        yield return StartCoroutine(RetreatCoroutine());
        currentState = State.Chase;
    }

    IEnumerator SwordSwingCoroutine()
    {
        if (sword == null)
            yield break;

        isSwinging = true;
        Quaternion originalRot = sword.localRotation;
        int swingDir = Random.value < 0.5f ? 1 : -1;
        float swingAngle = 45f; // Ángulo del swing en grados
        Quaternion startRot = originalRot * Quaternion.Euler(0, -swingDir * swingAngle, 0);
        Quaternion endRot = originalRot * Quaternion.Euler(0, swingDir * swingAngle, 0);
        float swingDuration = 0.4f;
        float t = 0f;
        bool damageApplied = false;

        sword.localRotation = startRot;
        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float progress = t / swingDuration;
            sword.localRotation = Quaternion.Slerp(startRot, endRot, progress);

            // A la mitad del swing se verifica si el jugador está en el área de impacto.
            if (!damageApplied && t >= swingDuration * 0.5f)
            {
                float dist = Vector3.Distance(sword.position, player.position);
                if (dist <= attackRange)
                {
                    HealthSystem playerHealth = player.GetComponent<HealthSystem>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                        Debug.Log("La espada impacta al jugador. Daño: " + attackDamage);
                    }
                }
                damageApplied = true;
            }
            yield return null;
        }
        sword.localRotation = originalRot;
        isSwinging = false;
    }

    IEnumerator RetreatCoroutine()
    {
        Vector3 retreatDir = (transform.position - player.position).normalized;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + retreatDir * retreatDistance;
        //targetPos = ForzarPosicionY(targetPos);
        float elapsed = 0f;
        while (elapsed < retreatDuration)
        {
            elapsed += Time.deltaTime;
            //transform.position = ForzarPosicionY(Vector3.Lerp(startPos, targetPos, elapsed / retreatDuration));
            yield return null;
        }
    }

    // Este método se llamará vía SendMessage desde HealthSystem al recibir daño.
    private void OnDamageTaken(float damage)
    {
        if (!isStunned && player != null)
        {
            Vector3 knockbackDir = (transform.position - player.position).normalized;
            StartCoroutine(StunAndKnockback(knockbackDir));
        }
    }

    IEnumerator StunAndKnockback(Vector3 knockbackDir)
    {
        isStunned = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + knockbackDir * knockbackDistance;
        //targetPos = ForzarPosicionY(targetPos);
        float elapsed = 0f;
        while (elapsed < knockbackTime)
        {
            elapsed += Time.deltaTime;
            //transform.position = ForzarPosicionY(Vector3.Lerp(startPos, targetPos, elapsed / knockbackTime));
            yield return null;
        }
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        currentState = State.Chase;
    }

    protected override float GetAttackRange()
    {
        return attackRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}
