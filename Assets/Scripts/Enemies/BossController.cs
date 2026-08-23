using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class BossController : MonoBehaviour
{
    [Header("Charge Ability")]
    [SerializeField, Min(0f)]
    private float firstChargeDelay = 3f;

    [SerializeField, Min(0.1f)]
    private float chargeInterval = 5f;

    [SerializeField, Min(1f)]
    private float chargeSpeedMultiplier = 3f;

    [SerializeField, Min(0.1f)]
    private float chargeDuration = 0.8f;

    [SerializeField, Min(0f)]
    private float recoveryDuration = 0.8f;

    [Header("Summon Ability")]
    [SerializeField]
    private EnemyHealth minionPrefab;

    [SerializeField, Min(0f)]
    private float firstSummonDelay = 5f;

    [SerializeField, Min(0.1f)]
    private float summonInterval = 9f;

    [SerializeField, Range(1, 6)]
    private int minionsPerWave = 2;

    [SerializeField, Min(0.5f)]
    private float summonRadius = 1.5f;

    private EnemyController movement;

    private void Awake()
    {
        movement = GetComponent<EnemyController>();
    }

    private void OnEnable()
    {
        StartCoroutine(ChargeLoop());

        if (minionPrefab != null)
            StartCoroutine(SummonLoop());
    }

    private IEnumerator ChargeLoop()
    {
        yield return new WaitForSeconds(firstChargeDelay);

        while (true)
        {
            movement.SetSpeedMultiplier(
                chargeSpeedMultiplier
            );

            yield return new WaitForSeconds(
                chargeDuration
            );

            movement.ResetSpeedMultiplier();

            yield return new WaitForSeconds(
                recoveryDuration
            );

            yield return new WaitForSeconds(
                chargeInterval
            );
        }
    }

    private IEnumerator SummonLoop()
    {
        yield return new WaitForSeconds(
            firstSummonDelay
        );

        while (true)
        {
            SummonMinions();

            yield return new WaitForSeconds(
                summonInterval
            );
        }
    }

    private void SummonMinions()
    {
        for (int i = 0; i < minionsPerWave; i++)
        {
            float angle =
                360f * i / minionsPerWave;

            Vector2 direction =
                Quaternion.Euler(0f, 0f, angle) *
                Vector2.right;

            Vector2 spawnPosition =
                (Vector2)transform.position +
                direction * summonRadius;

            Instantiate(
                minionPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
    }

    private void OnDisable()
    {
        if (movement != null)
            movement.ResetSpeedMultiplier();
    }
}