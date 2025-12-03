using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class EnemySpawnData
{
    [Tooltip("Spawn olacak düþmanýn prefabý.")]
    public GameObject enemyPrefab;
    [Tooltip("Bu düþmanýn çýkma þansý/aðýrlýðý. (Örn: Mob=50, Boss=1)")]
    public float spawnWeight = 10f;
}

public class SpawnManager : MonoBehaviour
{
    [Header("Mesafe ve Süre Ayarlarý")]
    [Tooltip("Düþmanlarýn oyuncudan en az ne kadar uzakta doðacaðý.")]
    [SerializeField] private float minSpawnRadius = 15f;
    [Tooltip("Düþmanlarýn oyuncudan en fazla ne kadar uzakta doðacaðý.")]
    [SerializeField] private float maxSpawnRadius = 30f;
    [Tooltip("Kaç saniyede bir spawn dalgasý tetiklenecek.")]
    [SerializeField] private float spawnInterval = 2f;

    [Header("Miktar Ayarlarý")]
    [Tooltip("Her dalgada en az kaç düþman doðsun?")]
    [SerializeField] private int minSpawnAmount = 1;
    [Tooltip("Her dalgada en fazla kaç düþman doðsun?")]
    [SerializeField] private int maxSpawnAmount = 3;

    [Header("Düþman Havuzu")]
    [SerializeField] private List<EnemySpawnData> enemyList = new List<EnemySpawnData>();

    [Header("Zemin Kontrolü")]
    [SerializeField] private LayerMask groundLayer;

    private Transform playerTransform;

    void Start()
    {
        StartCoroutine(FindPlayerRoutine());
    }

    IEnumerator FindPlayerRoutine()
    {
        while (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                // Debug.Log("SpawnManager: Oyuncu bulundu, spawn baþlýyor.");
                StartCoroutine(SpawnEnemyRoutine());
            }
            yield return null;
        }
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (playerTransform != null)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 1. Bu dalgada kaç düþman doðacaðýný rastgele belirle
            int spawnCount = Random.Range(minSpawnAmount, maxSpawnAmount + 1);

            // 2. Belirlenen sayý kadar düþman oluþtur
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject prefabToSpawn = GetRandomEnemyPrefab();

                if (prefabToSpawn != null)
                {
                    // Her düþman için ayrý bir rastgele nokta bul
                    Vector3 spawnPos = GetValidSpawnPosition();

                    if (spawnPos != Vector3.zero)
                    {
                        // --- DEÐÝÞÝKLÝK YAPILAN KISIM ---

                        // Düþmaný oluþtur ve referansýný al
                        GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

                        // EnemyStats bileþenini al
                        EnemyStats stats = newEnemy.GetComponent<EnemyStats>();

                        if (stats != null)
                        {
                            // Eðer GameEventManager varsa, düþmanýn doðduðunu bildir (Midas Touched burada devreye girer)
                            if (GameEventManager.Instance != null)
                            {
                                GameEventManager.Instance.TriggerEnemySpawned(stats);
                            }
                        }

                        // --------------------------------
                    }
                }
            }
        }
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyList.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var enemy in enemyList)
        {
            totalWeight += enemy.spawnWeight;
        }

        float randomValue = Random.Range(0, totalWeight);
        float currentWeightSum = 0f;

        foreach (var enemy in enemyList)
        {
            currentWeightSum += enemy.spawnWeight;
            if (randomValue <= currentWeightSum)
            {
                return enemy.enemyPrefab;
            }
        }

        return enemyList[0].enemyPrefab;
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector3 potentialPos = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y) * randomDistance;

            // Yüksekten aþaðý ýþýn atarak zemini bul
            Vector3 rayOrigin = new Vector3(potentialPos.x, 100f, potentialPos.z);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 200f, groundLayer))
            {
                return hit.point + Vector3.up * 1f; // Zeminin biraz üstünde doðsun
            }
        }

        return Vector3.zero;
    }
}