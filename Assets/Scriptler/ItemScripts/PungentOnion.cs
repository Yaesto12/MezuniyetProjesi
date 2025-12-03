using UnityEngine;

public class Item_PungentOnion : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Bekleme Süresi (Cooldown)")]
    [Tooltip("Ýlk seviyede kaç saniyede bir tetiklenebilir?")]
    [SerializeField] private float baseCooldown = 10f;
    [Tooltip("Her stack baþýna bekleme süresi ne kadar azalsýn?")]
    [SerializeField] private float cooldownReductionPerStack = 1f;
    [Tooltip("Süre en az kaça inebilir?")]
    [SerializeField] private float minCooldownLimit = 3f;

    [Header("Etki Alaný ve Ýtme")]
    [SerializeField] private float effectRadius = 6f;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("GrossedOut Zayýflatmasý")]
    [Tooltip("Zayýflatma kaç saniye sürsün?")]
    [SerializeField] private float debuffDuration = 5f;

    [Tooltip("Ýlk seviyede Hýz ve Hasar kýrma yüzdesi (Örn: 20 = %20 azalma).")]
    [SerializeField] private float baseDebuffPercent = 20f;
    [Tooltip("Her stack baþýna zayýflatma yüzdesi ne kadar artsýn?")]
    [SerializeField] private float debuffPercentPerStack = 5f;
    [Tooltip("Zayýflatma en fazla yüzde kaç olabilir?")]
    [SerializeField] private float maxDebuffLimit = 60f;

    [Header("Görsel")]
    [SerializeField] private GameObject gasCloudVFX;

    // Dahili deðiþkenler
    private float cooldownTimer = 0f;
    private bool isOnCooldown = false;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerTakeDamage += OnTakeDamage;
        }
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("EnemyHitbox");
    }

    public override void OnUnequip()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerTakeDamage -= OnTakeDamage;
        }
        base.OnUnequip();
    }

    private void Update()
    {
        // Bekleme süresi sayacý
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isOnCooldown = false;
                // Debug.Log("Pungent Onion hazýr!");
            }
        }
    }

    private void OnTakeDamage(int damage, EnemyStats attacker)
    {
        if (isOnCooldown) return;

        ActivateOnionEffect();
    }

    private void ActivateOnionEffect()
    {
        if (owner == null) return;

        // 1. Hesaplamalar (Limitli)
        int stack = 1;
        if (myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // Bekleme Süresi Hesabý (Limitli)
        float currentCooldown = baseCooldown - (cooldownReductionPerStack * (stack - 1));
        currentCooldown = Mathf.Max(currentCooldown, minCooldownLimit);

        // Zayýflatma Gücü Hesabý (Limitli)
        float currentDebuff = baseDebuffPercent + (debuffPercentPerStack * (stack - 1));
        currentDebuff = Mathf.Min(currentDebuff, maxDebuffLimit);

        // 2. Alan Etkisi ve Ýtme
        Collider[] enemies = Physics.OverlapSphere(owner.transform.position, effectRadius, enemyLayer);

        foreach (Collider hit in enemies)
        {
            // EnemyStats'a ulaþ
            EnemyStats enemyStats = hit.GetComponentInParent<EnemyStats>();
            if (enemyStats != null)
            {
                // A. Zayýflatma Uygula
                enemyStats.ApplyGrossedOut(debuffDuration, currentDebuff, currentDebuff);

                // B. Ýtme Uygula (Knockback)
                Rigidbody rb = enemyStats.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Oyuncudan düþmana doðru vektör
                    Vector3 pushDir = (enemyStats.transform.position - owner.transform.position).normalized;
                    pushDir.y = 0.5f; // Hafif yukarý kaldýrsýn ki yere sürtünmesin

                    // NavMeshAgent kullanýyorsa geçici olarak durdurmak gerekebilir
                    // Þimdilik fiziksel itme uyguluyoruz:
                    rb.AddForce(pushDir * knockbackForce, ForceMode.Impulse);
                }
                else
                {
                    // Rigidbody yoksa transform ile basitçe it
                    enemyStats.transform.position += (enemyStats.transform.position - owner.transform.position).normalized * 2f;
                }
            }
        }

        // 3. Görsel Efekt
        if (gasCloudVFX != null)
        {
            Instantiate(gasCloudVFX, owner.transform.position, Quaternion.identity);
        }

        // 4. Cooldown'a gir
        cooldownTimer = currentCooldown;
        isOnCooldown = true;

        Debug.Log($"<color=green>Pungent Onion!</color> Düþmanlar itildi. Debuff: %{currentDebuff}, CD: {currentCooldown}s");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}