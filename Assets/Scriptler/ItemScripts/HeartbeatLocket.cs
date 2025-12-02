using UnityEngine;

public class Item_HeartbeatLocket : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Can Kazanýmý")]
    [Tooltip("Her düþman öldüðünde kazanýlan Maksimum Can (Örn: 1).")]
    [SerializeField] private float healthPerKill = 1f;

    [Header("Sýnýr Ayarlarý")]
    [Tooltip("Ýlk seviyede ulaþýlabilen maksimum can bonusu.")]
    [SerializeField] private float baseMaxHealthCap = 50f;

    [Tooltip("Her stack (kopya) için sýnýra eklenecek miktar.")]
    [SerializeField] private float capPerStack = 25f;

    // Þu an birikmiþ olan toplam bonus
    private float currentAccumulatedBonus = 0f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyKilled += OnEnemyKilled;
        }
    }

    public override void OnUnequip()
    {
        // Item çýkarýlýrsa kazanýlan caný geri al
        if (currentAccumulatedBonus > 0 && playerStats != null)
        {
            playerStats.RemoveTemporaryMaxHealth(currentAccumulatedBonus);
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

        float currentCap = baseMaxHealthCap + (capPerStack * (stack - 1));

        // 2. Sýnýra ulaþtýk mý kontrol et
        if (currentAccumulatedBonus < currentCap)
        {
            // Eklenecek miktarý hesapla (Sýnýrý aþmamasýný saðla)
            float amountToAdd = healthPerKill;

            if (currentAccumulatedBonus + amountToAdd > currentCap)
            {
                amountToAdd = currentCap - currentAccumulatedBonus; // Sadece aradaki farký ekle
            }

            // 3. Caný Ekle
            if (amountToAdd > 0)
            {
                currentAccumulatedBonus += amountToAdd;

                // Max Caný Artýr
                playerStats.AddTemporaryMaxHealth(amountToAdd);

                // Oyuncuyu da o miktar kadar iyileþtir (ki yeni kazanýlan can dolu gelsin)
                PlayerHealth health = owner.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Heal(amountToAdd);
                }

                // Debug.Log($"Heartbeat Locket: +{amountToAdd} Max Can. Toplam: {currentAccumulatedBonus}/{currentCap}");
            }
        }
    }
}