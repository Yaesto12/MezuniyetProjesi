using UnityEngine;

public class Item_StonksGraph : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Eþik Ayarý")]
    [Tooltip("Düþmanýn caný yüzde kaçýn altýndaysa ekstra hasar verilsin? (Örn: 40 = %40)")]
    [SerializeField] private float healthThreshold = 40f;

    [Header("Hasar Bonusu")]
    [Tooltip("Ýlk seviyede, verilen hasarýn yüzde kaçý kadar ekstra vurulsun? (Örn: 20 = %20)")]
    [SerializeField] private float baseDamageBonus = 20f;

    [Tooltip("Her stack için eklenen ekstra hasar yüzdesi.")]
    [SerializeField] private float bonusPerStack = 20f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit += OnEnemyHit;
        }
    }

    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit -= OnEnemyHit;
        }
        base.OnUnequip();
    }

    private void OnEnemyHit(EnemyStats enemy, int originalDamage, bool isCrit)
    {
        if (enemy == null || enemy.gameObject == null) return;

        // 1. Can Yüzdesini Kontrol Et
        // (Reaper's Scythe adýmýnda EnemyStats'a GetHealthPercentage eklemiþtik)
        float currentHpPercent = enemy.GetHealthPercentage() * 100f;

        if (currentHpPercent <= healthThreshold)
        {
            // 2. Item Seviyesini Bul
            int stack = 1;
            if (owner != null && myItemData != null)
            {
                PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
                if (inventory != null) stack = inventory.GetItemLevel(myItemData);
            }

            // 3. Ekstra Hasarý Hesapla
            float totalBonusPercent = baseDamageBonus + (bonusPerStack * (stack - 1));
            int extraDamage = Mathf.RoundToInt(originalDamage * (totalBonusPercent / 100f));

            if (extraDamage > 0)
            {
                // 4. Ekstra Hasarý Vur
                // Not: Bu TakeDamage çaðrýsý tekrar OnEnemyHit olayýný tetiklemez (Silahlar tetikler),
                // bu yüzden sonsuz döngüye girmez. Güvenlidir.
                enemy.TakeDamage(extraDamage);

                // Görsel veya Log (Ýsteðe baðlý)
                // Debug.Log($"Stonks! {enemy.name} caný az (%{currentHpPercent:F0}), +{extraDamage} ekstra hasar yedi!");
            }
        }
    }
}