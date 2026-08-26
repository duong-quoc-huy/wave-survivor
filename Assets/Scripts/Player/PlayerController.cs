using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float moveSpeed = 5f;

    private Rigidbody2D body;
    private Vector2 movementInput;
    private PlayerStats playerStats;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
    }

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector2 direction = Vector2.ClampMagnitude(movementInput, 1f);

        float activeSpeed = playerStats != null ? playerStats.CurrentSpeed : moveSpeed;

        body.linearVelocity = direction * activeSpeed;
    }

    public void IncreaseMoveSpeed(float amount)
    {
        if (amount > 0f)
        {
            moveSpeed += amount;
        }
    }

    private void OnDisable()
    {
        movementInput = Vector2.zero;

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }
}