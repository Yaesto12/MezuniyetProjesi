using UnityEngine;

public class Item_OverflowingGoblet : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Hasar Dönüþümü")]
    [Tooltip("Her 1 birim Overheal için kazanýlan hasar yüzdesi (Örn: 0.5 = %0.5).")]
    [SerializeField] private float damagePerOverhealPoint = 0.5f;

    [Tooltip("Stack baþýna artan dönüþüm oraný.")]
    [SerializeField] private float bonusPerStack = 0.25f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);

        // Saðlýk deðiþim olayýna abone ol
        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.OnHealthChanged += UpdateBonus;
            // Ýlk alýndýðýnda hemen hesapla
            UpdateBonus();
        }
    }

    public override void OnUnequip()
    {
        // Bonusu sýfýrla
        if (playerStats != null) playerStats.SetOverhealDamageBonus(0);

        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.OnHealthChanged -= UpdateBonus;
        }
        base.OnUnequip();
    }

    private void UpdateBonus()
    {
        if (playerStats == null || owner == null) return;

        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health == null) return;

        // 1. Item Seviyesini Bul
        int stack = 1;
        if (myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Dönüþüm Oranýný Hesapla
        float conversionRate = damagePerOverhealPoint + (bonusPerStack * (stack - 1));

        // 3. Bonusu Hesapla (Overheal Miktarý * Oran)
        float currentOverheal = health.CurrentOverheal;
        float damageBonus = currentOverheal * conversionRate;

        // 4. PlayerStats'a Gönder
        playerStats.SetOverhealDamageBonus(damageBonus);

        // Debug.Log($"Goblet: Overheal {currentOverheal} -> Bonus Hasar %{damageBonus}");
    }
}