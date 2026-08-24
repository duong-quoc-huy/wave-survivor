using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GoldCoin : MonoBehaviour
{
    private int finalGoldValue = 1;
    private bool hasBeenCollected;

    public void Initialize(int enemyBaseGold, float stageMultiplier)
    {
        finalGoldValue = Mathf.Max(1, Mathf.RoundToInt(enemyBaseGold * stageMultiplier));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
        {
            hasBeenCollected = true;
            LocalSaveSystem.AddGold(finalGoldValue);
            Destroy(gameObject);
        }
    }
}