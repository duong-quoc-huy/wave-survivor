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
    private float speedMultiplier = 1f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError(
                "EnemyController could not find a GameObject tagged Player.",
                this
            );

            enabled = false;
            return;
        }

        target = player.transform;
    }

    private void FixedUpdate()
    {
        if (target == null)
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
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
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

        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
}