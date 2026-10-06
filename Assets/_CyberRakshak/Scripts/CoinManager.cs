using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int numberOfCoins = 10;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool preventDuplicateSpawnPoints = true;

    [Header("UI")]
    [SerializeField] private TMP_Text coinCounterText;

    // Event fired when all coins have been collected
    public event Action OnAllCoinsCollected;

    private int collectedCoins = 0;
    private readonly List<GameObject> spawnedCoins = new List<GameObject>();

    public int TotalCoins => numberOfCoins;
    public int CollectedCoins => collectedCoins;

    private void Start()
    {
        SpawnCoins();
        UpdateUI();
    }

    private void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogError("CoinManager: Coin Prefab has not been assigned.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("CoinManager: No spawn points assigned.");
            return;
        }

        if (numberOfCoins > spawnPoints.Length && preventDuplicateSpawnPoints)
        {
            Debug.LogWarning(
                $"CoinManager: Requested {numberOfCoins} coins, " +
                $"but only {spawnPoints.Length} spawn points exist. " +
                $"Spawning {spawnPoints.Length} coins instead."
            );

            numberOfCoins = spawnPoints.Length;
        }

        List<Transform> availableSpawnPoints =
            new List<Transform>(spawnPoints);

        for (int i = 0; i < numberOfCoins; i++)
        {
            int randomIndex = UnityEngine.Random.Range(
                0,
                availableSpawnPoints.Count
            );

            Transform spawnPoint = availableSpawnPoints[randomIndex];

            GameObject coin = Instantiate(
                coinPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            spawnedCoins.Add(coin);

            // Give the coin access to this manager
            Coin coinComponent = coin.GetComponent<Coin>();

            if (coinComponent != null)
            {
                coinComponent.Initialize(this);
            }
            else
            {
                Debug.LogError(
                    "Coin prefab does not contain a Coin component."
                );
            }

            if (preventDuplicateSpawnPoints)
            {
                availableSpawnPoints.RemoveAt(randomIndex);
            }
        }
    }

    public void CollectCoin(Coin coin)
    {
        collectedCoins++;

        UpdateUI();

        if (coin != null)
        {
            Destroy(coin.gameObject);
        }

        Debug.Log(
            $"Coins collected: {collectedCoins}/{numberOfCoins}"
        );

        if (collectedCoins >= numberOfCoins)
        {
            AllCoinsCollected();
        }
    }

    private void UpdateUI()
    {
        if (coinCounterText != null)
        {
            coinCounterText.text =
                $"Coins: {collectedCoins} / {numberOfCoins}";
        }
    }

    private void AllCoinsCollected()
    {
        Debug.Log("All coins collected! Maze exit unlocked.");

        OnAllCoinsCollected?.Invoke();
    }
}
