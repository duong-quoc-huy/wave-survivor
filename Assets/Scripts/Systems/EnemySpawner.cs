using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private EnemyHealth enemyPrefab;

    [SerializeField]
    private Transform enemyParent;

    [Header("Arena")]
    [SerializeField]
    private Vector2 arenaHalfSize = new Vector2(14f, 9f);

    [Header("Spawn Timing")]
    [SerializeField, Min(0f)]
    private float startDelay = 1f;

    [SerializeField, Min(0.1f)]
    private float initialSpawnInterval = 2f;

    [SerializeField, Min(0.1f)]
    private float minimumSpawnInterval = 0.75f;

    [SerializeField, Min(0f)]
    private float intervalDecreasePerMinute = 0.25f;

    [Header("Limits")]
    [SerializeField, Min(1)]
    private int maxActiveEnemies = 60;

    private readonly List<EnemyHealth> activeEnemies = new();
    private float elapsedTime;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            RemoveDestroyedEnemies();

            if (activeEnemies.Count < maxActiveEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(GetCurrentSpawnInterval());
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPosition = GetRandomArenaEdgePosition();

        EnemyHealth enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            Quaternion.identity,
            enemyParent
        );

        activeEnemies.Add(enemy);
    }

    private Vector2 GetRandomArenaEdgePosition()
    {
        Vector2 center = transform.position;
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // Left
                return center + new Vector2(
                    -arenaHalfSize.x,
                    Random.Range(-arenaHalfSize.y, arenaHalfSize.y)
                );

            case 1: // Right
                return center + new Vector2(
                    arenaHalfSize.x,
                    Random.Range(-arenaHalfSize.y, arenaHalfSize.y)
                );

            case 2: // Bottom
                return center + new Vector2(
                    Random.Range(-arenaHalfSize.x, arenaHalfSize.x),
                    -arenaHalfSize.y
                );

            default: // Top
                return center + new Vector2(
                    Random.Range(-arenaHalfSize.x, arenaHalfSize.x),
                    arenaHalfSize.y
                );
        }
    }

    private float GetCurrentSpawnInterval()
    {
        float elapsedMinutes = elapsedTime / 60f;

        float currentInterval =
            initialSpawnInterval -
            intervalDecreasePerMinute * elapsedMinutes;

        return Mathf.Max(minimumSpawnInterval, currentInterval);
    }

    private void RemoveDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(
                arenaHalfSize.x * 2f,
                arenaHalfSize.y * 2f,
                0f
            )
        );
    }
}