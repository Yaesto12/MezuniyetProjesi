using UnityEngine;

public class Item_PocketWatch : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Scriptin item seviyesini okuyabilmesi için ItemData'yý buraya sürükleyin.")]
    [SerializeField] private ItemData myItemData; // <<<--- BU EKLENDÝ ---<<<

    [Tooltip("Kalan süreden düþülecek yüzdelik oran (Örn: 1 = %1).")]
    [SerializeField] private float baseReducePercentage = 1f; // Ýlk seviye deðeri

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        // Düþman ölme olayýna abone ol
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyKilled += OnEnemyKilled;
        }
    }

    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyKilled -= OnEnemyKilled;
        }
        base.OnUnequip();
    }

    private void OnEnemyKilled(EnemyStats enemy)
    {
        // 1. Item Seviyesini Bul
        int level = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                level = inventory.GetItemLevel(myItemData);
            }
        }

        // 2. Toplam Yüzdeyi Hesapla (Seviye baþýna artan)
        // Örn: Seviye 1 = %1, Seviye 2 = %2, ...
        float totalReducePercent = baseReducePercentage * level;

        // 3. Ýleride Eklenecek Yetenek Mantýðý
        /*
        var abilityController = owner.GetComponent<PlayerAbilityController>();
        if (abilityController != null)
        {
            abilityController.ReduceCooldownPercent(totalReducePercent);
        }
        */

        // Test Logu
        Debug.Log($"Pocket Watch (Seviye {level}): Bekleme süresinden %{totalReducePercent} düþüldü (Simülasyon).");
    }
}