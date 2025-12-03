using UnityEngine;

public class Item_Magnet : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Seviye takibi için gerekli.")]
    [SerializeField] private ItemData myItemData;

    [Header("Zamanlama")]
    [Tooltip("Ýlk seviyede kaç saniyede bir çekim yapsýn?")]
    [SerializeField] private float baseInterval = 20f;

    [Tooltip("Her seviye (stack) baþýna süre kaç saniye kýsalsýn?")]
    [SerializeField] private float timeReductionPerStack = 2f;

    [Tooltip("Süre en az kaça düþebilir?")]
    [SerializeField] private float minInterval = 5f;

    private float timer;
    private XpCollector xpCollector;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);

        // Player üzerindeki (veya altýndaki) XpCollector'ý bul
        xpCollector = playerOwner.GetComponentInChildren<XpCollector>();

        if (xpCollector == null)
        {
            Debug.LogError("Item Magnet: Oyuncuda 'XpCollector' bulunamadý!");
        }

        // Item ilk alýndýðýnda hemen bir çekim yapsýn (kullanýcý tatmini için)
        timer = 0.5f;
    }

    private void Update()
    {
        if (xpCollector == null || owner == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // Çekimi Baþlat
            xpCollector.PullAllActiveOrbs();

            // Sayacý sýfýrla (yeni süreye göre)
            timer = CalculateInterval();
        }
    }

    private float CalculateInterval()
    {
        int stack = 1;
        if (myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // Formül: Baz Süre - (Azaltma * (Seviye - 1))
        float currentInterval = baseInterval - (timeReductionPerStack * (stack - 1));

        // Minimum sýnýrýn altýna inmesin
        return Mathf.Max(currentInterval, minInterval);
    }
}