using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float moveSpeed = 2f;

    [SerializeField, Min(1)]
    private int contactDamage = 1;

    private Rigidbody2D body;
    private Transform target;
    private SpriteRenderer spriteRenderer;

    private float speedMultiplier = 1f;
    private bool isStunned = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("EnemyController could not find a GameObject tagged Player.", this);
            enabled = false;
            return;
        }

        target = player.transform;
    }

    private void FixedUpdate()
    {
        // Stop movement if there is no target or if enemy is currently stunned
        if (target == null || isStunned)
        {
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            body.position,
            target.position,
            moveSpeed * speedMultiplier * Time.fixedDeltaTime
        );

        body.MovePosition(nextPosition);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Don't deal damage to player while stunned
        if (isStunned) return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }

    // --- Stun Functionality ---
    public void ApplyStun(float duration)
    {
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        // Visual feedback: Tint enemy blue/cyan while frozen
        Color originalColor = Color.white;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.cyan;
        }

        yield return new WaitForSeconds(duration);

        isStunned = false;

        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
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