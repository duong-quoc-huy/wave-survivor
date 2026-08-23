using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class XPOrb : MonoBehaviour
{
    [SerializeField, Min(1)]
    private int experienceValue = 1;

    private bool hasBeenCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected)
        {
            return;
        }

        PlayerStats playerStats =
            other.GetComponentInParent<PlayerStats>();

        if (playerStats == null)
        {
            return;
        }

        hasBeenCollected = true;
        playerStats.AddExperience(experienceValue);
        Destroy(gameObject);
    }
}