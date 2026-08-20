using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float moveSpeed = 2f;

    private Rigidbody2D body;
    private Transform target;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

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
            moveSpeed * Time.fixedDeltaTime
        );

        body.MovePosition(nextPosition);
    }

    private void OnDisable()
    {
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }
}