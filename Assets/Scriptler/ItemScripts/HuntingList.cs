using UnityEngine;
using System.Collections.Generic;

public class Item_HuntingList : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Scriptin seviyesini okuyabilmesi için ItemData'yý buraya sürükleyin.")]
    [SerializeField] private ItemData myItemData;

    [Header("Hasar Ayarlarý")]
    [Tooltip("Ýlk seviyede verilecek ekstra hasar yüzdesi (Örn: 25 = %25).")]
    [SerializeField] private float baseDamagePercent = 25f;

    [Tooltip("Her seviyede eklenecek ekstra yüzde.")]
    [SerializeField] private float bonusPerStack = 25f;

    [Header("Hedef Listesi")]
    [Tooltip("Bu item hangi düþman türlerinde çalýþacak? (Tag isimleri)")]
    [SerializeField] private List<string> targetTags = new List<string> { "Elite", "MiniBoss", "Boss" };

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        // Vuruþ olayýna abone ol
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
        // 1. Düþman null ise veya tag listesinde yoksa iþlem yapma
        if (enemy == null || !targetTags.Contains(enemy.gameObject.tag)) return;

        // 2. Item Seviyesini Bul
        int level = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) level = inventory.GetItemLevel(myItemData);
        }

        // 3. Ekstra Hasarý Hesapla
        // Formül: Orijinal Hasar * (Toplam Yüzde / 100)
        float totalPercent = baseDamagePercent + (bonusPerStack * (level - 1));
        int bonusDamage = Mathf.RoundToInt(originalDamage * (totalPercent / 100f));

        if (bonusDamage > 0)
        {
            // 4. Ekstra Hasarý Uygula
            // Not: isDoT=false gönderiyoruz, ama isterseniz farklý efekt için true yapabilirsiniz.
            enemy.TakeDamage(bonusDamage);

            // Debug.Log($"Hunting List Tetiklendi! {enemy.name} hedefine +{bonusDamage} ekstra hasar.");
        }
    }
}