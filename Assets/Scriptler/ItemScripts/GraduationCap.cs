using UnityEngine;

public class Item_GraduationCap : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Scriptin item seviyesini (stack) okuyabilmesi için ItemData'yý buraya sürükleyin.")]
    [SerializeField] private ItemData myItemData;

    [Tooltip("Her seviye atlamada, sahip olunan Kep baþýna eklenecek XP Bonusu yüzdesi (Örn: 5 = +%5).")]
    [SerializeField] private float bonusPercentPerLevel = 5f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);

        // Level atlama olayýna abone ol
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerLevelUp += OnLevelUp;
        }
    }

    public override void OnUnequip()
    {
        // Olaydan çýk
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerLevelUp -= OnLevelUp;
        }
        base.OnUnequip();
    }

    private void OnLevelUp(int newLevel)
    {
        if (playerStats != null)
        {
            // 1. Item Seviyesini (Stack) Bul
            int stackCount = 1;
            if (owner != null && myItemData != null)
            {
                PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    stackCount = inventory.GetItemLevel(myItemData);
                }
            }

            // 2. Toplam eklenecek bonusu hesapla
            // Örn: 2 tane kep varsa ve oran %5 ise -> Her levelda +%10 XP bonusu kazanýr.
            float totalBonusToAdd = bonusPercentPerLevel * stackCount;

            // 3. PlayerStats'taki BASE deðeri kalýcý olarak artýr
            playerStats.IncreaseBaseXpBonus(totalBonusToAdd);

            Debug.Log($"Graduation Cap Çalýþtý! (Stack: {stackCount}) -> Seviye {newLevel} ile XP Bonusu +%{totalBonusToAdd} arttý.");
        }
    }
}