using UnityEngine;

public class Item_ReapersScythe : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Ýnfaz Eþiði Ayarlarý")]
    [Tooltip("Ýlk seviyede infaz yüzdesi (Örn: 10 = %10 canýn altýndaysa öldür).")]
    [SerializeField] private float baseThreshold = 10f;

    [Tooltip("Ýlk stack'te eklenecek bonus yüzdesi (Örn: 6).")]
    [SerializeField] private float initialBonus = 6f;

    [Tooltip("Her seviyede bonusun ne kadar azalacaðý (Örn: 2).")]
    [SerializeField] private float decayAmount = 2f;

    [Tooltip("Maksimum ulaþabileceði infaz yüzdesi (Örn: 50 = %50).")]
    [SerializeField] private float maxThresholdCap = 50f;

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
        // Düþman zaten öldüyse iþlem yapma
        if (enemy == null || enemy.gameObject == null) return;

        // Mevcut Can Yüzdesini Hesapla (Hasar verildikten sonraki hali)
        // Not: EnemyStats'a currentHealth'i okumak için bir property eklemeniz gerekebilir 
        // veya GetComponent ile private alana eriþilemez. 
        // Çözüm: EnemyStats.cs'de currentHealth için bir 'getter' yazmak veya hasar sonrasý kalan caný tahmin etmek.

        // EnemyStats'a eriþim:
        // (EnemyStats scriptinde 'public float GetHealthPercentage()' metodu olduðunu varsayýyoruz
        // veya public bir 'CurrentHealth' property'si).
        // Þimdilik Reflection veya doðrudan eriþim yerine EnemyStats'a bir metot ekleyeceðiz (Aþaðýda açýklanmýþtýr).

        float healthPercent = enemy.GetHealthPercentage();

        // Ýnfaz Eþiðini Hesapla
        float executeThreshold = CalculateThreshold();

        // Ýnfaz Kontrolü
        if (healthPercent * 100f <= executeThreshold)
        {
            // Ýnfaz Gerçekleþir!
            // Kalan can kadar (veya fazlasý) hasar vererek öldür
            int executeDamage = 999999;
            Debug.Log($"<color=red>REAPER'S SCYTHE! (Can: %{healthPercent * 100:F1} < Eþik: %{executeThreshold}) - ÝNFAZ EDÝLDÝ!</color>");

            // Ýnfaz görsel efekti burada oluþturulabilir (Instantiate)

            enemy.TakeDamage(executeDamage);
        }
    }

    /// <summary>
    /// Item seviyesine göre azalan getiri (diminishing returns) mantýðýyla eþik hesaplar.
    /// Örnek: Base 10. Artýþlar: +6, +4, +2, +0...
    /// </summary>
    private float CalculateThreshold()
    {
        int level = 1;
        if (owner != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null && myItemData != null) level = inventory.GetItemLevel(myItemData);
        }

        float currentThreshold = baseThreshold;
        float currentBonus = initialBonus;

        // Seviye 1 ise direkt base döner.
        // Seviye 2 ve sonrasý için döngü:
        for (int i = 1; i < level; i++)
        {
            currentThreshold += currentBonus;

            // Bir sonraki artýþ miktarýný azalt (decay)
            currentBonus -= decayAmount;

            // Artýþ miktarý 0'ýn altýna düþerse 0 olsun (veya min 0.5f gibi bir sýnýr koyabilirsin)
            currentBonus = Mathf.Max(0f, currentBonus);
        }

        // Tavana takýlma kontrolü
        return Mathf.Min(currentThreshold, maxThresholdCap);
    }
}