using UnityEngine;

public class Item_MidasTouched : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Þans Ayarlarý")]
    [Tooltip("Düþmanýn altýn doðma þansý (%) (Örn: 5 = %5).")]
    [SerializeField] private float baseChance = 5f;

    [Tooltip("Her stack (kopya) için eklenen þans yüzdesi.")]
    [SerializeField] private float chancePerStack = 2.5f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        // Düþman doðma olayýný dinle
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemySpawned += OnEnemySpawned;
        }
    }

    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemySpawned -= OnEnemySpawned;
        }
        base.OnUnequip();
    }

    private void OnEnemySpawned(EnemyStats enemy)
    {
        // Eðer düþman zaten altýnsa (baþka bir mekanik yüzünden) iþlem yapma
        if (enemy.IsGolden) return;

        // 1. Item Seviyesini Bul
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Þansý Hesapla
        float currentChance = baseChance + (chancePerStack * (stack - 1));
        currentChance = Mathf.Clamp(currentChance, 0f, 100f);

        // 3. Zar At
        if (Random.value * 100f <= currentChance)
        {
            enemy.MakeGolden();
            // Debug.Log("Midas Touched: Bir düþman altýna dönüþtü!");
        }
    }
}