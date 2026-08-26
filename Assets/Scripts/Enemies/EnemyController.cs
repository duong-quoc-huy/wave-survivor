using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField, Min(1)]
    private int maxHealth = 10;

    [SerializeField, Min(0f)]
    private float moveSpeed = 2f;

    [SerializeField, Min(1)]
    private int contactDamage = 1;

    [SerializeField, Min(0)]
    private int experienceValue = 1;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsStunned => isStunned;

    private int currentHealth;
    private Rigidbody2D body;
    private Transform target;
    private SpriteRenderer spriteRenderer;

    private float speedMultiplier = 1f;
    private bool isStunned = false;
    private Coroutine stunRoutine;
    private Coroutine flashRoutine;

    private Color defaultColor = Color.white;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        FindPlayerTarget();
    }

    private void FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void FixedUpdate()
    {
        // Re-acquire target dynamically if spawned before player instance
        if (target == null)
        {
            FindPlayerTarget();
            if (target == null) return;
        }

        if (isStunned) return;

        Vector2 nextPosition = Vector2.MoveTowards(
            body.position,
            target.position,
            moveSpeed * speedMultiplier * Time.fixedDeltaTime
        );

        body.MovePosition(nextPosition);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isStunned) return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || damage <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (gameObject.activeInHierarchy && spriteRenderer != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(HitFlashRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        if (!isStunned && spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = isStunned ? Color.cyan : defaultColor;
        }
    }

    public void Die()
    {
        if (target != null)
        {
            PlayerStats playerStats = target.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddExperience(experienceValue);
            }
        }

        Destroy(gameObject);
    }

    public void ApplyStun(float duration)
    {
        if (!gameObject.activeInHierarchy) return;

        if (stunRoutine != null) StopCoroutine(stunRoutine);
        stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
        }

        yield return new WaitForSeconds(duration);

        isStunned = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        speedMultiplier = 1f;
    }

    private void OnDisable()
    {
        speedMultiplier = 1f;
        isStunned = false;

        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
}