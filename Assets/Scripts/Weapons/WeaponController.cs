using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField]
    private Projectile projectilePrefab;

    [SerializeField]
    private LayerMask enemyLayer;

    [SerializeField, Min(0.1f)]
    private float attackRange = 8f;

    [SerializeField, Min(0.05f)]
    private float attackInterval = 0.75f;

    private float attackTimer;

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        if (TryAttack())
        {
            attackTimer = attackInterval;
        }
        else
        {
            // Check again soon when no enemy is currently in range.
            attackTimer = 0.1f;
        }
    }

    private bool TryAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            enemyLayer
        );

        EnemyHealth nearestEnemy = null;
        float nearestDistanceSquared = float.PositiveInfinity;

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null)
            {
                continue;
            }

            float distanceSquared =
                ((Vector2)enemy.transform.position -
                 (Vector2)transform.position).sqrMagnitude;

            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy == null)
        {
            return false;
        }

        Vector2 direction =
            nearestEnemy.transform.position - transform.position;

        Projectile projectile = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        projectile.Initialize(direction);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}