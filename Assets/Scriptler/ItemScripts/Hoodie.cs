using UnityEngine;

public class Item_Hoodie : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Menzil Ayarlarý")]
    [Tooltip("Düþmanlarý saymak için tarama yarýçapý.")]
    [SerializeField] private float detectionRange = 10f;

    [Tooltip("Düþman katmaný.")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Hasar Ayarlarý")]
    [Tooltip("Ýlk seviyede, etraftaki her düþman baþýna kazanýlan hasar yüzdesi (Örn: 2 = %2).")]
    [SerializeField] private float damagePerEnemyBase = 2f;

    [Tooltip("Her stack için düþman baþýna eklenecek ekstra hasar yüzdesi.")]
    [SerializeField] private float bonusPerStack = 1f;

    [Tooltip("Maksimum ulaþýlabilir hasar bonusu sýnýrý (Örn: 50 = %50).")]
    [SerializeField] private float maxDamageCap = 50f;

    // Dahili deðiþkenler
    private float currentAppliedBonus = 0f;
    private float checkInterval = 0.2f; // Her frame deðil, saniyede 5 kere kontrol et (Performans için)
    private float timer = 0f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        // Layer atanmamýþsa varsayýlaný bul
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("EnemyHitbox");
    }

    public override void OnUnequip()
    {
        // Çýkarýlýrsa mevcut bonusu sil
        if (currentAppliedBonus > 0 && playerStats != null)
        {
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
        }
        base.OnUnequip();
    }

    private void Update()
    {
        if (playerStats == null || owner == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            UpdateBonus();
            timer = checkInterval;
        }
    }

    private void UpdateBonus()
    {
        // 1. Etraftaki Düþmanlarý Say
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        int enemyCount = hits.Length;

        // 2. Item Seviyesini Bul
        int stack = 1;
        if (myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 3. Hedef Bonusu Hesapla
        // Formül: (Base + (Ekstra * (Seviye-1))) * DüþmanSayýsý
        float percentPerEnemy = damagePerEnemyBase + (bonusPerStack * (stack - 1));
        float targetBonus = percentPerEnemy * enemyCount;

        // Sýnýrý uygula
        targetBonus = Mathf.Min(targetBonus, maxDamageCap);

        // 4. PlayerStats'a Farký Uygula
        // (Eski bonus ile yeni bonus arasýndaki farký ekleriz/çýkarýrýz)
        float difference = targetBonus - currentAppliedBonus;

        if (Mathf.Abs(difference) > 0.01f) // Küçük deðiþimleri yoksay
        {
            playerStats.AddTemporaryDamage(difference); // Fark negatifse otomatik düþer
            currentAppliedBonus = targetBonus;

            // Debug.Log($"Hoodie: Etrafta {enemyCount} düþman var. Hasar Bonusu: %{currentAppliedBonus:F1}");
        }
    }

    // Editörde alaný görmek için
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}