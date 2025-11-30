using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("Eðer karakter seçilmemiþse kullanýlacak varsayýlan karakter.")]
    [SerializeField] private GameObject defaultCharacterPrefab;

    [Tooltip("Oyuncunun zeminden ne kadar yukarýda doðacaðý.")]
    [SerializeField] private float spawnHeightOffset = 20f;

    void Start()
    {
        StartCoroutine(SpawnPlayerRoutine());
    }

    IEnumerator SpawnPlayerRoutine()
    {
        // 1. Harita Oluþana Kadar Bekle
        // MapGenerator'ýn varlýðýný ve bitip bitmediðini kontrol et
        while (MapGenerator.Instance == null || !MapGenerator.Instance.IsMapGenerated)
        {
            yield return null; // Bir sonraki frame'i bekle
        }

        // 2. Rastgele Doðma Pozisyonunu Al
        Vector3 randomMapPos = MapGenerator.Instance.GetRandomSpawnPosition();

        // Yüksekliði ayarla (Karakterin zemine çakýlmamasý için yukarýdan býrakýyoruz)
        Vector3 finalSpawnPos = new Vector3(randomMapPos.x, randomMapPos.y + spawnHeightOffset, randomMapPos.z);
        Quaternion spawnRotation = Quaternion.identity;

        Debug.Log($"Spawn Noktasý Belirlendi: {finalSpawnPos}");

        // 3. Karakteri Oluþtur
        GameObject playerInstance = null;

        if (GameData.SelectedCharacterPrefab != null)
        {
            playerInstance = Instantiate(GameData.SelectedCharacterPrefab, finalSpawnPos, spawnRotation);
            Debug.Log($"{GameData.SelectedCharacterPrefab.name} rastgele bir noktada oluþturuldu.");
        }
        else if (defaultCharacterPrefab != null)
        {
            playerInstance = Instantiate(defaultCharacterPrefab, finalSpawnPos, spawnRotation);
            Debug.Log($"Varsayýlan karakter rastgele bir noktada oluþturuldu.");
        }
        else
        {
            Debug.LogError("Spawn edilecek karakter prefab'ý bulunamadý!");
        }

        // Spawner'ý yok et
        Destroy(gameObject);
    }
}