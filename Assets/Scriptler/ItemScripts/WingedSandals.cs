using UnityEngine;

public class Item_WingedSandals : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Seviye takibi için ItemData referansý.")]
    [SerializeField] private ItemData myItemData;

    [Header("Dönüþüm Ayarlarý")]
    [Tooltip("1 birim hareket hýzý baþýna kazanýlan hasar yüzdesi (Base).")]
    [SerializeField] private float damagePerSpeedUnit = 2.0f; // Örn: Hýz 5 ise %10 hasar verir

    [Tooltip("Her stack (item kopyasý) için çarpan ne kadar artsýn?")]
    [SerializeField] private float bonusPerStack = 1.0f;

    // Dahili deðiþkenler
    private CharacterController controller;
    private float currentAppliedBonus = 0f;
    private float lastSpeed = -1f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);

        // Hýzý okumak için Controller'ý al
        controller = playerOwner.GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("Winged Sandals: Oyuncuda CharacterController bulunamadý!");
        }
    }

    public override void OnUnequip()
    {
        // Çýkarýlýrsa bonusu temizle
        if (currentAppliedBonus > 0 && playerStats != null)
        {
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
        }
        base.OnUnequip();
    }

    private void Update()
    {
        if (controller == null || playerStats == null) return;

        // 1. Anlýk Hýzý Ölç (Yatay düzlemde, Y eksenini yoksayabiliriz veya dahil edebiliriz)
        // Genelde sadece yatay hýz önemlidir:
        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        // Çok küçük hýzlarý 0 kabul et (titremeyi önler)
        if (currentSpeed < 0.1f) currentSpeed = 0f;

        // 2. Eðer hýz deðiþtiyse hesaplama yap (Performans için eþik koyuyoruz)
        if (Mathf.Abs(currentSpeed - lastSpeed) > 0.1f)
        {
            UpdateDamageBonus(currentSpeed);
            lastSpeed = currentSpeed;
        }
    }

    private void UpdateDamageBonus(float speed)
    {
        // 1. Item Seviyesini Bul
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Çarpaný Hesapla
        // Base Çarpan + (Stack * Bonus)
        float conversionRate = damagePerSpeedUnit + (bonusPerStack * (stack - 1));

        // 3. Hedef Bonusu Hesapla (Hýz * Çarpan)
        float targetBonus = speed * conversionRate;

        // 4. Farký Uygula
        float difference = targetBonus - currentAppliedBonus;

        if (Mathf.Abs(difference) > 0.01f)
        {
            // Eski bonusu çýkarýp yenisini eklemek yerine farký yönetiyoruz
            // Veya daha temiz: Eskiyi sil, yeniyi ekle.
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
            playerStats.AddTemporaryDamage(targetBonus);

            currentAppliedBonus = targetBonus;

            // Debug.Log($"Winged Sandals: Hýz {speed:F1} -> Bonus %{currentAppliedBonus:F1}");
        }
    }
}