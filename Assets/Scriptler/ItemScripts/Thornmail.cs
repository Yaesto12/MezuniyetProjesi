using UnityEngine;

public class Item_Thornmail : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Yansýtma Ayarlarý")]
    [Tooltip("Ýlk seviyede, alýnan hasarýn yüzde kaçý yansýtýlsýn? (10 = %10).")]
    [SerializeField] private float baseReflectPercent = 10f;

    [Tooltip("Her stack için eklenen yüzde.")]
    [SerializeField] private float bonusPerStack = 10f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerTakeDamage += OnTakeDamage;
        }
    }

    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerTakeDamage -= OnTakeDamage;
        }
        base.OnUnequip();
    }

    private void OnTakeDamage(int damageTaken, EnemyStats attacker)
    {
        // Saldýran yoksa (tuzak vb.) veya ölmüþse yansýtma yapma
        if (attacker == null || attacker.gameObject == null) return;

        // 1. Item Seviyesini Bul
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Yansýtma Oranýný Hesapla
        float totalPercent = baseReflectPercent + (bonusPerStack * (stack - 1));

        // 3. Hasarý Hesapla (Alýnan hasar üzerinden)
        int reflectDamage = Mathf.RoundToInt(damageTaken * (totalPercent / 100f));

        // En az 1 hasar yansýtsýn (Eðer oran > 0 ise)
        if (reflectDamage < 1 && totalPercent > 0 && damageTaken > 0) reflectDamage = 1;

        if (reflectDamage > 0)
        {
            // 4. Hasarý Düþmana Geri Vur
            attacker.TakeDamage(reflectDamage);
            // Debug.Log($"Thornmail: {reflectDamage} hasar yansýtýldý!");
        }
    }
}