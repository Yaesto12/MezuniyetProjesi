using UnityEngine;

// ItemEffect sýnýfýndan türetiyoruz (Yeni sistemimiz)
public class Item_SuckerPunch : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Bu scriptin hangi iteme ait olduðunu bilmesi için ItemData'yý buraya sürükleyin.")]
    [SerializeField] private ItemData myItemData;

    [SerializeField] private float baseHealAmount = 1f; // Ýlk seviyede kaç iyileþsin
    [SerializeField] private float bonusPerStack = 1f;  // Her ekstra itemde kaç artsýn

    // Item alýndýðýnda çalýþýr
    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        // Düþmana vurulma olayýna abone ol
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit += OnEnemyHit;
        }
    }

    // Item silinirse veya oyun biterse
    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit -= OnEnemyHit;
        }
        base.OnUnequip();
    }

    // Olay gerçekleþtiðinde çalýþacak fonksiyon
    private void OnEnemyHit(EnemyStats enemy, int damage, bool isCrit)
    {
        // Sadece KRÝTÝK vuruþlarda çalýþ
        if (isCrit)
        {
            // Oyuncunun envanterinden bu itemin kaç tane olduðunu (seviyesini) bul
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            int level = 1;

            if (inventory != null && myItemData != null)
            {
                level = inventory.GetItemLevel(myItemData);
            }

            // Ýyileþme miktarýný hesapla: Base + (Ekstra * (Seviye - 1))
            float totalHeal = baseHealAmount + (bonusPerStack * (level - 1));

            // Oyuncuyu iyileþtir
            PlayerHealth health = owner.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.Heal(totalHeal);
                // Debug.Log($"Sucker Punch! Kritik vuruþla {totalHeal} can yenilendi.");
            }
        }
    }
}