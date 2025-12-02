using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Item_TeslaCoil : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Þans Ayarlarý")]
    [Tooltip("Vuruþ baþýna tetiklenme þansý (%) (Örn: 20 = %20).")]
    [SerializeField] private float baseChance = 20f;
    [Tooltip("Stack baþýna eklenen þans.")]
    [SerializeField] private float chancePerStack = 5f;

    [Header("Hasar Ayarlarý")]
    [Tooltip("Orijinal vuruþun yüzde kaçý kadar hasar versin? (0.5 = %50).")]
    [SerializeField] private float baseDamagePercent = 0.5f;
    [Tooltip("Stack baþýna hasar çarpaný artýþý.")]
    [SerializeField] private float damagePercentPerStack = 0.1f;

    [Header("Sekme Ayarlarý")]
    [Tooltip("Kaç düþmana sekecek?")]
    [SerializeField] private int bounceCount = 3;
    [Tooltip("Sekme menzili (bir düþmandan diðerine).")]
    [SerializeField] private float bounceRange = 8f;

    [Header("Referanslar")]
    [Tooltip("Elektrik efekti prefabý (Üzerinde LightningEffect olmalý).")]
    [SerializeField] private GameObject lightningPrefab;
    [Tooltip("Düþman katmaný.")]
    [SerializeField] private LayerMask enemyLayer;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit += OnEnemyHit;
        }
        // Layer atanmamýþsa varsayýlaný bul
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("EnemyHitbox");
    }

    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit -= OnEnemyHit;
        }
        base.OnUnequip();
    }

    private void OnEnemyHit(EnemyStats primaryTarget, int damage, bool isCrit)
    {
        if (primaryTarget == null) return;

        // 1. Seviyeyi Bul
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Þans Kontrolü
        float currentChance = baseChance + (chancePerStack * (stack - 1));
        // Þans %100'ü geçmesin
        currentChance = Mathf.Clamp(currentChance, 0f, 100f);

        if (Random.value * 100f <= currentChance)
        {
            // 3. Hasarý Hesapla
            float damageMult = baseDamagePercent + (damagePercentPerStack * (stack - 1));
            int chainDamage = Mathf.RoundToInt(damage * damageMult);
            if (chainDamage < 1) chainDamage = 1;

            // Zincirlemeyi Baþlat
            StartCoroutine(ChainLightningRoutine(primaryTarget, chainDamage));
        }
    }

    private IEnumerator ChainLightningRoutine(EnemyStats startEnemy, int dmg)
    {
        List<EnemyStats> hitEnemies = new List<EnemyStats>();
        hitEnemies.Add(startEnemy); // Ýlk vurulaný listeye ekle ki ona tekrar sekmesin

        Transform currentSource = startEnemy.transform;
        int bouncesLeft = bounceCount;

        while (bouncesLeft > 0)
        {
            // Yakýndaki düþmanlarý bul
            Collider[] hits = Physics.OverlapSphere(currentSource.position, bounceRange, enemyLayer);

            EnemyStats nextTarget = null;
            float closestDist = Mathf.Infinity;

            // En yakýn ve henüz vurulmamýþ düþmaný seç
            foreach (var hit in hits)
            {
                EnemyStats enemy = hit.GetComponentInParent<EnemyStats>();
                if (enemy != null && !hitEnemies.Contains(enemy))
                {
                    float d = Vector3.Distance(currentSource.position, hit.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        nextTarget = enemy;
                    }
                }
            }

            if (nextTarget != null)
            {
                // Görsel Efekt
                if (lightningPrefab != null)
                {
                    GameObject vfx = Instantiate(lightningPrefab, Vector3.zero, Quaternion.identity);
                    LightningEffect script = vfx.GetComponent<LightningEffect>();
                    if (script != null)
                    {
                        // Efektin pozisyonlarýný ayarla (biraz yukarýdan)
                        script.Zap(currentSource.position + Vector3.up, nextTarget.transform.position + Vector3.up);
                    }
                }

                // Hasar Ver
                // Not: isDoT=false (direkt hasar), ama tekrar OnEnemyHit tetiklemesin diye dikkatli olunmalý.
                // EnemyStats.TakeDamage eventi tetikliyor mu? Mevcut kodumuzda tetiklemiyor (Health tetikliyor).
                // Bu yüzden güvenli.
                nextTarget.TakeDamage(dmg);

                // Listeye ekle ve kaynaðý güncelle
                hitEnemies.Add(nextTarget);
                currentSource = nextTarget.transform;
                bouncesLeft--;

                // Sekmeler arasýnda çok kýsa bekleme (görsel akýþ için)
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                // Sekecek baþka düþman yok
                break;
            }
        }
    }
}