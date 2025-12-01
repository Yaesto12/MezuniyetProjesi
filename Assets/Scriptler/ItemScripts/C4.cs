using UnityEngine;

public class Item_C4 : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Item seviyesini okumak için.")]
    [SerializeField] private ItemData myItemData;

    [Header("Patlama Ayarlarý")]
    [Tooltip("Patlamanýn yarýçapý.")]
    [SerializeField] private float explosionRadius = 3f;

    [Tooltip("Patlama görsel efekti.")]
    [SerializeField] private GameObject explosionVFX;

    [Tooltip("Düþman katmaný (Patlamanýn kimlere vuracaðýný seç).")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Hasar Ayarlarý")]
    [Tooltip("Ýlk seviyede, verilen kritik hasarýn yüzde kaçý patlama hasarý olarak vurulsun? (1.0 = %100, aynýsý)")]
    [SerializeField] private float baseDamageMultiplier = 1.0f;

    [Tooltip("Her ek stack için hasar çarpaný ne kadar artsýn? (Örn: 0.5 = +%50)")]
    [SerializeField] private float bonusPerStack = 0.5f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit += OnEnemyHit;
        }

        // Varsayýlan layer atamasý (Eðer inspector'dan unutulursa)
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("EnemyHitbox"); // Veya sizin enemy layer adýnýz
    }

    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyHit -= OnEnemyHit;
        }
        base.OnUnequip();
    }

    private void OnEnemyHit(EnemyStats targetEnemy, int damageDealt, bool isCrit)
    {
        // Sadece KRÝTÝK vuruþlarda ve hedef ölmediyse (veya ölse bile cesedinde) patla
        if (!isCrit) return;

        // 1. Item Seviyesini Bul
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Patlama Hasarýný Hesapla
        // Formül: Kritik Hasar * (BaseÇarpan + (Ekstra * (Seviye-1)))
        float multiplier = baseDamageMultiplier + (bonusPerStack * (stack - 1));
        int explosionDamage = Mathf.RoundToInt(damageDealt * multiplier);

        // 3. Görsel Efekt
        if (explosionVFX != null)
        {
            // Patlama efektini hedefin merkezinde oluþtur
            Instantiate(explosionVFX, targetEnemy.transform.position, Quaternion.identity);
        }

        // 4. Alan Hasarý Uygula (Physics.OverlapSphere)
        Collider[] hits = Physics.OverlapSphere(targetEnemy.transform.position, explosionRadius, enemyLayer);

        foreach (Collider hit in hits)
        {
            // Hasar alanýn EnemyStats scriptini bul
            EnemyStats enemy = hit.GetComponentInParent<EnemyStats>();

            if (enemy != null)
            {
                // Patlama hasarý ver (isDoT=false, çünkü bu anlýk bir patlama)
                // Not: Bu hasar tekrar 'OnEnemyHit' tetiklemez çünkü doðrudan TakeDamage çaðýrýyoruz,
                // Projectile scripti üzerinden gitmiyoruz. Bu da sonsuz döngüyü engeller.
                enemy.TakeDamage(explosionDamage);
            }
        }

        // Debug.Log($"C4 Patladý! {explosionDamage} hasar verildi. (Çarpan: {multiplier}x)");
    }

    // Gizmos ile alaný editörde gör
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}