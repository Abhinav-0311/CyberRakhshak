using System;
using System.Collections.Generic;
using CyberRakshak.PATCH;
using CyberRakshak.Platformer;
using TMPro;
using UnityEngine;

namespace CyberRakshak.Runtime
{
public class CoinManager : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField, Min(1)] private int numberOfCoins = 40;
    [SerializeField, Min(1)] private int requiredCoins = 30;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool preventDuplicateSpawnPoints = true;

    [Header("UI")]
    [SerializeField] private TMP_Text coinCounterText;
    [SerializeField] private TMP_Text pickupFeedback;
    [SerializeField] private TMP_Text exitSign;
    private float feedbackSeconds;

    // Event fired when all coins have been collected
    public event Action OnAllCoinsCollected;

    private int collectedCoins = 0;
    private readonly List<GameObject> spawnedCoins = new List<GameObject>();

    public int TotalCoins => numberOfCoins;
    public int CollectedCoins => collectedCoins;
    public int RequiredCoins => requiredCoins;
    public bool CanExit => collectedCoins >= requiredCoins;

    private void Start()
    {
        SpawnCoins();
        UpdateUI();
        if (pickupFeedback != null) pickupFeedback.alpha = 0f;
    }

    private void Update()
    {
        if (pickupFeedback == null || feedbackSeconds <= 0f) return;
        feedbackSeconds = Mathf.Max(0f, feedbackSeconds - Time.unscaledDeltaTime);
        pickupFeedback.alpha = Mathf.Min(1f, feedbackSeconds * 3f);
    }

    private void SpawnCoins()
    {
        if (coinPrefab == null || coinPrefab.GetComponent<Coin>() == null)
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
            Debug.LogError("CoinManager needs enough spawn points for all coins.", this);
            return;
        }

        if (requiredCoins < 1 || requiredCoins > numberOfCoins)
        {
            Debug.LogError("CoinManager required count must be between 1 and the total coin count.", this);
            return;
        }
        var positions = new HashSet<Vector3>();
        foreach (var point in spawnPoints)
            if (point == null || (preventDuplicateSpawnPoints && !positions.Add(point.position)))
            {
                Debug.LogError("CoinManager spawn points must be non-null and distinct.", this);
                return;
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
                spawnPoint.rotation,
                transform
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
        if (coin == null || !spawnedCoins.Remove(coin.gameObject)) return;
        collectedCoins++;

        UpdateUI();
        PlatformerSfx.PlayCoinChime();
        if (pickupFeedback != null)
        {
            pickupFeedback.text = collectedCoins == requiredCoins ? "EXIT UNLOCKED" : "+1 COIN";
            pickupFeedback.alpha = 1f;
            feedbackSeconds = 1f;
        }
        if (collectedCoins == requiredCoins)
        {
            PlatformerSfx.PlayCoinChime(true);
            PatchDialoguePresenter.Ensure().ShowOnce("maze-coins-ready", "PATCH",
                "Enough coins secured. The exit is unlocked! Follow the green exit marker.", 5f);
        }

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
        if (exitSign != null)
        {
            exitSign.text = CanExit ? "PHISHING MAZE\nEXIT UNLOCKED" : $"PHISHING MAZE\nEXIT / NEED {requiredCoins} COINS";
            exitSign.color = CanExit ? new Color(.3f, 1f, .65f) : new Color(1f, .8f, .3f);
        }
        if (coinCounterText != null)
        {
            coinCounterText.text =
                $"COINS: {collectedCoins}/{numberOfCoins}  /  NEED {requiredCoins}\n" +
                (CanExit ? "EXIT UNLOCKED" : "EXIT LOCKED");
        }
    }

    private void AllCoinsCollected()
    {
        Debug.Log("All coins collected!");

        OnAllCoinsCollected?.Invoke();
    }
}
}
