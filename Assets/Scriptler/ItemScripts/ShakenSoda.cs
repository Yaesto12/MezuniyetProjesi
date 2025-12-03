using UnityEngine;

public class Item_ShakenSoda : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Seviye takibi için ItemData referansý.")]
    [SerializeField] private ItemData myItemData;

    [Header("Yük Ayarlarý")]
    [Tooltip("Patlama için gereken vuruþ sayýsý.")]
    [SerializeField] private int hitsRequired = 100;

    [Header("Patlama Ayarlarý")]
    [Tooltip("Patlama yarýçapý.")]
    [SerializeField] private float explosionRadius = 5f;

    [Tooltip("Ýlk seviyede patlama hasarý.")]
    [SerializeField] private int baseDamage = 50;

    [Tooltip("Her stack (kopya) için hasar ne kadar artsýn?")]
    [SerializeField] private int damagePerStack = 50;

    [Tooltip("Hangi katmandaki objelere hasar verilsin?")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Görsel")]
    [Tooltip("Patlama anýnda oluþacak efekt prefabý.")]
    [SerializeField] private GameObject explosionVFX;

    // Dahili deðiþken
    private int currentHits = 0;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);

        // Vuruþ olayýna abone ol
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit += OnEnemyHit;
        }

        // Layer maskesi ayarlanmamýþsa varsayýlaný bul
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("EnemyHitbox");
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
        // Sayaç arttýr
        currentHits++;

        // Hedefe ulaþtýk mý?
        if (currentHits >= hitsRequired)
        {
            Explode();
            currentHits = 0; // Sayacý sýfýrla
        }
    }

    private void Explode()
    {
        if (owner == null) return;

        // 1. Itemin Kendi Baz Hasarýný Hesapla (Örn: 50)
        int stack = 1;
        if (myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        float baseSodaDamage = baseDamage + (damagePerStack * (stack - 1));

        // --- DÜZELTME: KARAKTERÝN GÜCÜYLE ÇARP ---
        // Karakterin o anki hasar çarpanýný (Örn: 3.5x) alýp soda hasarýyla çarpýyoruz.
        // Böylece "Rook" veya "Scales" aldýðýnda sodan da güçleniyor!
        int finalDamage = Mathf.RoundToInt(baseSodaDamage * playerStats.CurrentDamageMultiplier);
        // ------------------------------------------

        // 2. Alan Hasarý Uygula
        Collider[] enemies = Physics.OverlapSphere(owner.transform.position, explosionRadius, enemyLayer);

        foreach (Collider hit in enemies)
        {
            EnemyStats targetStats = hit.GetComponentInParent<EnemyStats>();
            if (targetStats != null)
            {
                // Hasar ver (Artýk güncellenmiþ finalDamage'i kullanýyoruz)
                targetStats.TakeDamage(finalDamage);
            }
        }

        // ... (Görsel efekt kodlarý ayný kalýyor) ...
    }

    // Editörde patlama alanýný görmek için yardýmcý çizgi
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}