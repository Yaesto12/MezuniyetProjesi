using UnityEngine;

public class Item_IceBag : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Item seviyesini okumak için.")]
    [SerializeField] private ItemData myItemData;

    [Header("Donma Þansý")]
    [Tooltip("Vuruþ baþýna dondurma ihtimali (%) (Örn: 10 = %10).")]
    [SerializeField] private float freezeChance = 10f;

    [Header("Süre Ayarlarý (Azalan Getiri)")]
    [Tooltip("Ýlk seviyede donma süresi (saniye).")]
    [SerializeField] private float baseDuration = 1.0f;

    [Tooltip("Ýlk stack'te eklenecek ekstra süre.")]
    [SerializeField] private float initialBonus = 0.5f;

    [Tooltip("Her seviyede bonusun ne kadar azalacaðý.")]
    [SerializeField] private float decayAmount = 0.1f;

    [Tooltip("Maksimum donma süresi sýnýrý.")]
    [SerializeField] private float maxDurationCap = 3.0f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit += OnEnemyHit;
        }
    }

    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit -= OnEnemyHit;
        }
        base.OnUnequip();
    }

    private void OnEnemyHit(EnemyStats enemy, int damage, bool isCrit)
    {
        if (enemy == null) return;

        // Þans kontrolü
        if (Random.value * 100f < freezeChance)
        {
            float duration = CalculateDuration();
            enemy.Freeze(duration);
            Debug.Log($"Ice Bag Tetiklendi! {enemy.name} {duration} saniye dondu.");
        }
    }

    private float CalculateDuration()
    {
        int level = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) level = inventory.GetItemLevel(myItemData);
        }

        float currentDuration = baseDuration;
        float currentBonus = initialBonus;

        // Seviye 2 ve sonrasý için bonus ekle
        for (int i = 1; i < level; i++)
        {
            currentDuration += currentBonus;

            // Bonusu azalt (decay) ama 0'ýn altýna düþürme (en az 0.05 sn artsýn)
            currentBonus -= decayAmount;
            currentBonus = Mathf.Max(0.05f, currentBonus);
        }

        // Sýnýrý aþma
        return Mathf.Min(currentDuration, maxDurationCap);
    }
}