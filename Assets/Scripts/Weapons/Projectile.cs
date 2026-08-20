using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float speed = 8f;

    [SerializeField, Min(1)]
    private int damage = 1;

    [SerializeField, Min(0.1f)]
    private float lifetime = 3f;

    private Rigidbody2D body;
    private bool hasHit;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction)
    {
        body.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
        {
            return;
        }

        hasHit = true;
        enemyHealth.TakeDamage(damage);
        Destroy(gameObject);
    }
}