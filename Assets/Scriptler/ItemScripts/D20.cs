using UnityEngine;

public class Item_D20 : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Scriptin item seviyesini (kaç tane alýndýðýný) okuyabilmesi için ItemData'yý buraya sürükleyin.")]
    [SerializeField] private ItemData myItemData; // <<<--- EKLENDÝ ---<<<

    [Tooltip("Her level atlamada, sahip olunan D20 baþýna kaç Reroll verilsin?")]
    [SerializeField] private int rerollsPerLevel = 1;

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
            // 1. Item Seviyesini (Kaç tane D20 var?) Bul
            int stackCount = 1; // En az 1 tane vardýr ki bu kod çalýþýyor

            if (owner != null && myItemData != null)
            {
                PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    stackCount = inventory.GetItemLevel(myItemData);
                }
            }

            // 2. Toplam kazanýlacak Reroll miktarýný hesapla
            // Eðer stacklenmiyorsa stackCount 1 olur, 1 * 1 = 1 Reroll verir.
            // Eðer stackleniyorsa (örn 3 tane D20), 1 * 3 = 3 Reroll verir.
            int totalRerollsToAdd = rerollsPerLevel * stackCount;

            // 3. PlayerStats'a ekle
            playerStats.IncreaseBaseRerolls(totalRerollsToAdd);

            Debug.Log($"D20 Aktif! (Stack: {stackCount}) -> Seviye {newLevel} için {totalRerollsToAdd} Reroll eklendi.");
        }
    }
}