using UnityEngine;
using System.Collections;

public class Item_HellSeed : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Ruh Ayarlarý")]
    [Tooltip("Oluþturulacak ruhun prefabý.")]
    [SerializeField] private GameObject spiritPrefab;

    [Tooltip("Ruhlarýn hasarý.")]
    [SerializeField] private int damage = 15;
    [Tooltip("Stack baþýna hasar artýþý.")]
    [SerializeField] private int damagePerStack = 5;

    [Tooltip("Ruhlarýn uçuþ hýzý.")]
    [SerializeField] private float speed = 8f;

    [Tooltip("Düþman arama menzili.")]
    [SerializeField] private float range = 10f;

    [Tooltip("Yaþam süresi.")]
    [SerializeField] private float lifeTime = 5f;

    [Header("Spawn Ayarlarý")]
    [Tooltip("Kaç saniyede bir ruhlar oluþsun?")]
    [SerializeField] private float spawnInterval = 3f;

    [Tooltip("Ýlk seviyede kaç ruh oluþsun?")]
    [SerializeField] private int baseCount = 1;
    [Tooltip("Her stack için kaç ekstra ruh?")]
    [SerializeField] private int countPerStack = 1;

    private float timer = 0f;

    // Update fonksiyonu ItemEffect'te yok, MonoBehaviour olduðu için otomatik çalýþýr
    void Update()
    {
        // Sadece item takýlýysa (veya obje aktifse) çalýþýr
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnSpirits();
            timer = spawnInterval;
        }
    }

    private void SpawnSpirits()
    {
        if (spiritPrefab == null) return;

        // 1. Item Seviyesini Bul
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Deðerleri Hesapla
        int totalCount = baseCount + (countPerStack * (stack - 1));
        int totalDamage = damage + (damagePerStack * (stack - 1));
        // Hasarý PlayerStats.CurrentDamageMultiplier ile de çarpabilirsin:
        if (playerStats != null) totalDamage = Mathf.RoundToInt(totalDamage * (playerStats.CurrentDamageMultiplier / 100f));

        // 3. Ruhlarý Oluþtur
        for (int i = 0; i < totalCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 1f; // Karakterin içinden/yanýndan çýksýn
            GameObject spiritGO = Instantiate(spiritPrefab, spawnPos, Quaternion.identity);

            HellSpirit spiritScript = spiritGO.GetComponent<HellSpirit>();
            if (spiritScript != null)
            {
                // Ruhu ayarla
                spiritScript.Setup(totalDamage, speed, range, lifeTime, transform);
            }
        }
    }
}