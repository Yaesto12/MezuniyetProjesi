using UnityEngine;

public class Item_SpeedCounter : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Hýz Kazanýmý")]
    [Tooltip("Her düþman öldüðünde kazanýlan hareket hýzý (Örn: 0.1).")]
    [SerializeField] private float speedPerKill = 0.1f;

    [Header("Sýnýr Ayarlarý")]
    [Tooltip("Ýlk seviyede ulaþýlabilen maksimum hýz bonusu.")]
    [SerializeField] private float baseMaxSpeedBonus = 2.0f;

    [Tooltip("Her stack (kopya) için sýnýra eklenecek miktar.")]
    [SerializeField] private float capPerStack = 1.0f;

    // Þu an birikmiþ olan toplam bonusu takip eder
    private float currentAccumulatedBonus = 0f;

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
        // Item çýkarýlýrsa kazanýlan hýzý geri al
        if (currentAccumulatedBonus > 0 && playerStats != null)
        {
            playerStats.RemoveTemporaryMoveSpeed(currentAccumulatedBonus);
        }

        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyKilled -= OnEnemyKilled;
        }
        base.OnUnequip();
    }

    private void OnEnemyKilled(EnemyStats enemy)
    {
        if (playerStats == null) return;

        // 1. Item Seviyesini ve Sýnýrý (Cap) Hesapla
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        float currentCap = baseMaxSpeedBonus + (capPerStack * (stack - 1));

        // 2. Sýnýra ulaþtýk mý kontrol et
        if (currentAccumulatedBonus < currentCap)
        {
            // Eklenecek miktarý hesapla (Sýnýrý aþmamasýný saðla)
            float amountToAdd = speedPerKill;

            if (currentAccumulatedBonus + amountToAdd > currentCap)
            {
                amountToAdd = currentCap - currentAccumulatedBonus; // Sadece aradaki farký ekle
            }

            // 3. Hýzý Ekle
            if (amountToAdd > 0)
            {
                currentAccumulatedBonus += amountToAdd;
                playerStats.AddTemporaryMoveSpeed(amountToAdd);

                // Debug.Log($"Speed Counter: +{amountToAdd} Hýz. Toplam: {currentAccumulatedBonus}/{currentCap}");
            }
        }
    }
}