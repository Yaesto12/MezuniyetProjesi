using UnityEngine;

public class Item_Scales : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Dönüþüm Oraný")]
    [Tooltip("Her 1 Altýn baþýna kazanýlan hasar yüzdesi (Örn: 0.05 = 100 altýnda %5 hasar).")]
    [SerializeField] private float damagePercentPerGold = 0.05f;

    [Tooltip("Her stack (kopya) bu oraný ne kadar arttýrsýn?")]
    [SerializeField] private float bonusPerStack = 0.05f;

    [Tooltip("Maksimum hasar bonusu yüzdesi.")]
    [SerializeField] private float maxDamageCap = 200f;

    // Dahili deðiþkenler
    private PlayerInventory inventory;
    private float currentAppliedBonus = 0f;
    private int lastGoldAmount = -1;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        inventory = playerOwner.GetComponent<PlayerInventory>();
    }

    public override void OnUnequip()
    {
        if (currentAppliedBonus > 0 && playerStats != null)
        {
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
        }
        base.OnUnequip();
    }

    private void Update()
    {
        if (inventory == null || playerStats == null) return;

        // Para deðiþtiyse güncelle
        if (inventory.CurrentGold != lastGoldAmount)
        {
            UpdateDamageBonus();
            lastGoldAmount = inventory.CurrentGold;
        }
    }

    private void UpdateDamageBonus()
    {
        int stack = 1;
        if (myItemData != null) stack = inventory.GetItemLevel(myItemData);

        float currentRate = damagePercentPerGold + (bonusPerStack * (stack - 1));
        float targetBonus = inventory.CurrentGold * currentRate;

        targetBonus = Mathf.Min(targetBonus, maxDamageCap);

        float difference = targetBonus - currentAppliedBonus;

        if (Mathf.Abs(difference) > 0.01f)
        {
            playerStats.AddTemporaryDamage(difference);
            currentAppliedBonus = targetBonus;
        }
    }
}