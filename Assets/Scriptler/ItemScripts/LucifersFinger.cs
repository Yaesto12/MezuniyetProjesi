using UnityEngine;

public class Item_LucifersFinger : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Þans Ayarlarý")]
    [Tooltip("Vuruþ baþýna tetiklenme þansý (%) (Örn: 1 = %1).")]
    [SerializeField] private float baseChance = 1f;

    [Tooltip("Her stack (kopya) için eklenen þans yüzdesi.")]
    [SerializeField] private float chancePerStack = 1f;

    [Header("Hasar Ayarlarý")]
    [Tooltip("Hasar kaç katýna çýkacak? (Örn: 10 = 10x Hasar).")]
    [SerializeField] private float damageMultiplier = 10f;

    [Header("Görsel")]
    [SerializeField] private GameObject triggerVFX; // Tetiklendiðinde çýkacak efekt

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

    private void OnEnemyHit(EnemyStats enemy, int originalDamage, bool isCrit)
    {
        if (enemy == null) return;

        // 1. Item Seviyesini Bul
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Þansý Hesapla (1 + (Stack-1 * 1) = Toplam %)
        float currentChance = baseChance + (chancePerStack * (stack - 1));
        currentChance = Mathf.Clamp(currentChance, 0f, 100f);

        // 3. Zar At
        if (Random.value * 100f <= currentChance)
        {
            // 4. Ekstra Hasarý Hesapla
            // Hedef: Toplam hasar = Original * Multiplier
            // Verilmiþ Olan: Original
            // Gereken Ekstra: Original * (Multiplier - 1)
            // Örn: 10 vurduk. Hedef 100. Ekstra = 10 * (10-1) = 90.

            int extraDamage = Mathf.RoundToInt(originalDamage * (damageMultiplier - 1));

            if (extraDamage > 0)
            {
                // Ekstra hasarý ver
                enemy.TakeDamage(extraDamage);

                Debug.Log($"<color=red>LUCIFER'S FINGER! {originalDamage} -> {originalDamage + extraDamage} (10x) Hasar!</color>");

                // Görsel Efekt
                if (triggerVFX != null)
                {
                    Instantiate(triggerVFX, enemy.transform.position, Quaternion.identity);
                }
            }
        }
    }
}