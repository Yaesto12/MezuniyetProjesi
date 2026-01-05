using UnityEngine;

public class DragonSummoner : MonoBehaviour
{
    [Header("Spawn Ayarlarý")]
    [Tooltip("Hangi düþman spawn edilecek? (Ejderha Prefabý)")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Oyun baþladýktan kaç saniye sonra spawn olsun?")]
    [SerializeField] private float timeToSpawn = 60f; // Örnek: 60 saniye

    [Tooltip("Ejderha spawn olduðunda bu obje yok olsun mu?")]
    [SerializeField] private bool destroySelfAfterSpawn = true;

    [Header("Efektler")]
    [SerializeField] private GameObject spawnEffect; // Patlama/Duman efekti
    [SerializeField] private float spawnHeightOffset = 5f; // Ejderha havada mý doðsun?

    private float timer;
    private bool hasSpawned = false;

    void Update()
    {
        // Eðer zaten spawn ettiysek tekrar etme
        if (hasSpawned) return;

        // Süreyi say
        timer += Time.deltaTime;

        // Süre dolduysa iþlemi yap
        if (timer >= timeToSpawn)
        {
            SummonDragon();
        }
    }

    void SummonDragon()
    {
        hasSpawned = true;

        if (enemyPrefab != null)
        {
            // 1. Ejderhayý oluþtur (Biraz yukarýda)
            Vector3 spawnPos = transform.position + Vector3.up * spawnHeightOffset;
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // 2. Efekt varsa oynat
            if (spawnEffect != null)
            {
                Instantiate(spawnEffect, transform.position, Quaternion.identity);
            }

            Debug.Log("EJDERHA OYUNA DAHÝL OLDU!");
        }
        else
        {
            Debug.LogError("DragonSummoner: Ejderha Prefabý atanmamýþ!");
        }

        // 3. Bu çaðýrýcý obje artýk iþini bitirdiyse yok olsun
        if (destroySelfAfterSpawn)
        {
            Destroy(gameObject);
        }
        else
        {
            // Yok olmasýn ama scripti kapatalým ki Update çalýþmasýn
            this.enabled = false;
        }
    }
}