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

        // Ýlk giyildiðinde bonusu hesapla
        UpdateDamageBonus();
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

        // DÜZELTME: Parayý artýk inventory'den deðil, playerStats'tan alýyoruz.
        // Ayrýca deðiþkene küçük harfle 'currentGold' olarak eriþiyoruz.
        if (playerStats.currentGold != lastGoldAmount)
        {
            UpdateDamageBonus();
            lastGoldAmount = playerStats.currentGold;
        }
    }

    private void UpdateDamageBonus()
    {
        if (inventory == null || playerStats == null) return;

        int stack = 1;
        // Item seviyesini öðrenmek için hala inventory'ye ihtiyacýmýz var (Bu doðru)
        if (myItemData != null) stack = inventory.GetItemLevel(myItemData);

        float currentRate = damagePercentPerGold + (bonusPerStack * (stack - 1));

        // DÜZELTME: Hesaplama yaparken de playerStats.currentGold kullanýyoruz.
        float targetBonus = playerStats.currentGold * currentRate;

        targetBonus = Mathf.Min(targetBonus, maxDamageCap);

        float difference = targetBonus - currentAppliedBonus;

        if (Mathf.Abs(difference) > 0.01f)
        {
            playerStats.AddTemporaryDamage(difference);
            currentAppliedBonus = targetBonus;
        }
    }
}