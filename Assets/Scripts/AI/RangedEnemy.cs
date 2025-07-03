using UnityEngine;
using System.Collections;

public class RangedEnemy : EnemyBase
{
    [Header("Configuración de Ataque")]
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public float desiredDistance = 5f;

    [Header("Burst Fire")]
    public float burstChance = 0.3f;
    public int burstCount = 3;
    public float burstDelay = 0.2f;

    [Header("Referencia a Disparo")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    [Header("Stun, Knockback y Flashing")]
    public float stunDuration = 1f;
    public float knockbackDistance = 1f;
    public float knockbackTime = 0.2f;
    public float flashDuration = 2f;
    public float flashInterval = 0.2f;

    private bool isStunned = false;
    private float attackTimer = 0f;
    private Renderer rend;

    protected override void Start()
    {
        base.Start();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        rend = GetComponent<Renderer>();
    }

    protected override void Update()
    {
        if (isStunned)
            return;

        base.Update();

        if (player != null && currentState == State.Attack)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist < desiredDistance * 0.8f || dist > desiredDistance * 1.2f)
            {
                currentState = State.Chase;
            }
            else
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    if (Random.value < burstChance)
                        StartCoroutine(BurstFireCoroutine());
                    else
                        ShootProjectile();
                    attackTimer = 0f;
                }
            }
        }
    }

    protected override void ChaseBehavior()
    {
        if (player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < desiredDistance * 0.8f)
        {
            Vector3 retreatDir = (transform.position - player.position).normalized;
            transform.position += retreatDir * moveSpeed * Time.deltaTime;
        }
        else if (dist > desiredDistance * 1.2f)
        {
            Vector3 approachDir = (player.position - transform.position).normalized;
            transform.position += approachDir * moveSpeed * Time.deltaTime;
        }
        else
        {
            currentState = State.Attack;
            attackTimer = 0f;
        }

        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);
    }

    protected override void AttackBehavior()
    {
        // No se usa, la lógica está en Update
    }

    void ShootProjectile()
    {
        if (projectilePrefab != null && shootPoint != null && player != null)
        {
            Vector3 target = new Vector3(player.position.x, shootPoint.position.y, player.position.z);
            Vector3 direction = (target - shootPoint.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);
            Instantiate(projectilePrefab, shootPoint.position, rotation);
        }
    }

    IEnumerator BurstFireCoroutine()
    {
        for (int i = 0; i < burstCount; i++)
        {
            ShootProjectile();
            yield return new WaitForSeconds(burstDelay);
        }
    }

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

        float elapsed = 0f;
        while (elapsed < knockbackTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / knockbackTime);
            yield return null;
        }

        float flashElapsed = 0f;
        while (flashElapsed < flashDuration)
        {
            flashElapsed += flashInterval;
            if (rend != null)
                rend.enabled = !rend.enabled;
            yield return new WaitForSeconds(flashInterval);
        }

        if (rend != null)
            rend.enabled = true;

        isStunned = false;
        currentState = State.Chase;
    }

    protected override float GetAttackRange()
    {
        return desiredDistance;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, desiredDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}
