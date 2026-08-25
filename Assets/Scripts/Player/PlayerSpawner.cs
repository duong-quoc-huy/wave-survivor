using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<GameObject> availablePlayerPrefabs;
    [SerializeField] private GameObject defaultPlayerPrefab;
    [SerializeField] private Vector2 spawnPosition = Vector2.zero;

    private void Awake()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        // 1. Read equipped character ID from LocalSaveSystem
        string equippedCharName = LocalSaveSystem.GetEquippedCharacter();

        // 2. Safely search available prefabs (case-insensitive)
        GameObject prefabToSpawn = availablePlayerPrefabs.Find(p =>
            p != null && p.name.Equals(equippedCharName, StringComparison.OrdinalIgnoreCase)
        );

        // 3. Fallback to default if search fails
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"PlayerSpawner: Could not find prefab for '{equippedCharName}'. Using default character.", this);
            prefabToSpawn = defaultPlayerPrefab != null ? defaultPlayerPrefab : (availablePlayerPrefabs.Count > 0 ? availablePlayerPrefabs[0] : null);
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError("PlayerSpawner: No valid player prefab assigned!", this);
            return;
        }

        // 4. Instantiate character
        GameObject player = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        Debug.Log($"Spawned Prefab Name: {player.name}", player);

        if (!player.CompareTag("Player"))
        {
            player.tag = "Player";
        }

        // 5. Apply Skill Tree Stat Upgrades
        WeaponController weaponController = player.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.IncreaseProjectileDamage(LocalSaveSystem.GetBonusDamage());
        }

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.IncreaseMoveSpeed(LocalSaveSystem.GetBonusSpeed());
        }

        // 6. Bind Scene Managers & HUD
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.BindPlayerReferences(player);
        }

        HUDController hudController = FindFirstObjectByType<HUDController>();
        if (hudController != null)
        {
            hudController.BindPlayer(player);
            hudController.BindAbilities(player);
        }

        UpgradeManager upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (upgradeManager != null)
        {
            upgradeManager.BindPlayer(player);
        }

        BindCameraTarget(player.transform);
    }

    private void BindCameraTarget(Transform target)
    {
        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Target.TrackingTarget = target;
            Debug.Log("Cinemachine camera target assigned successfully.", this);
        }
    }
}