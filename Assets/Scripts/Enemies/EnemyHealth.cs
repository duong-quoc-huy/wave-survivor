using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int maxHealth = 3;

    private int currentHealth;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }
}