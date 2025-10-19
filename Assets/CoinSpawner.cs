using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoinSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject coinPrefab; 
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn Settings")]
    public int totalCoinsToSpawn = 10;  
    public float spawnInterval = 5f;     
    public int maxActiveCoins = 5;       

    private int coinsSpawned = 0;
    private List<GameObject> activeCoins = new List<GameObject>();

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.totalCoinsNeeded = totalCoinsToSpawn;

        StartCoroutine(SpawnCoinsOverTime());
    }

    private IEnumerator SpawnCoinsOverTime()
    {
        while (coinsSpawned < totalCoinsToSpawn)
        {
            activeCoins.RemoveAll(c => c == null);

            if (activeCoins.Count < maxActiveCoins)
            {
                SpawnOne();
                coinsSpawned++;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnOne()
    {
        if (coinPrefab == null || spawnPoints.Count == 0) return;

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject coin = Instantiate(coinPrefab, sp.position, sp.rotation);
        activeCoins.Add(coin);
    }
}
