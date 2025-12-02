using UnityEngine;

public class Item_ScarredHeart : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Hasar Bonusu Ayarlarý")]
    [Tooltip("Can %0'a düþtüðünde (teorik olarak) kazanýlacak maksimum hasar yüzdesi (Base).")]
    [SerializeField] private float baseMaxDamageBonus = 50f; // Ýlk itemde max %50 hasar

    [Tooltip("Her stack için maksimum hasara eklenecek miktar.")]
    [SerializeField] private float bonusPerStack = 25f;

    // Þu an aktif olan bonus miktarýný takip etmek için
    private float currentAppliedBonus = 0f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);

        // Can deðiþimini dinle
        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.OnHealthChanged += UpdateDamageBonus;
            // Ýlk alýndýðýnda hemen hesapla
            UpdateDamageBonus();
        }
    }

    public override void OnUnequip()
    {
        // Bonusu sil
        if (currentAppliedBonus > 0 && playerStats != null)
        {
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
        }

        // Olaydan çýk
        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.OnHealthChanged -= UpdateDamageBonus;
        }
        base.OnUnequip();
    }

    private void UpdateDamageBonus()
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

        // 2. Maksimum Bonusu Hesapla (Can %0 iken verilecek bonus)
        float maxBonus = baseMaxDamageBonus + (bonusPerStack * (stack - 1));

        // 3. Eksik Can Yüzdesini Hesapla
        // Örn: Max 100, Mevcut 70 -> Eksik 30 -> %30 Bonus kazanýr
        float currentHealth = health.CurrentHealth;
        float maxHealth = playerStats.CurrentMaxHealth;

        // (1 - (Mevcut / Max)) formülü eksik can oranýný verir (1.0 = Hepsi eksik, 0.0 = Hiç eksik yok)
        float missingHealthPercent = 1f - Mathf.Clamp01(currentHealth / maxHealth);

        // 4. Bonusu Hesapla
        float newBonus = maxBonus * missingHealthPercent;

        // 5. Farký Uygula (Eski bonusu çýkar, yenisini ekle veya farký ekle)
        // Temiz yöntem: Eskiyi sil, yeniyi ekle.
        if (Mathf.Abs(currentAppliedBonus - newBonus) > 0.1f) // Gereksiz güncellemeyi önle
        {
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
            playerStats.AddTemporaryDamage(newBonus);

            currentAppliedBonus = newBonus;
            // Debug.Log($"Scarred Heart: Can %{(1-missingHealthPercent)*100:F0}, Bonus +%{newBonus:F1}");
        }
    }
}