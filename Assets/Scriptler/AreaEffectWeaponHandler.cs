using UnityEngine;

public class AreaEffectWeaponHandler : MonoBehaviour
{
    // --- SINIF SEVÝYESÝ DEÐÝÞKENLERÝ ---
    public WeaponData weaponData;
    private PlayerStats playerStats; // Referans
    private float tickTimer;
    private float damagePerTick;
    private Collider[] hitColliders = new Collider[50];
    private GameObject currentAreaVisual;
    private LayerMask enemyLayer;
    // Yükseltme Deðerleri
    private float currentDamageMultiplier = 1f;
    private float currentRadiusMultiplier = 1f;
    private float currentTickRateMultiplier = 1f;
    // --- Deðiþkenler Bitti ---


    // --- Metotlar ---
    public void Initialize(WeaponData data, LayerMask enemies, PlayerStats pStats)
    {
        weaponData = data;
        enemyLayer = enemies;
        playerStats = pStats; // Atama
        currentDamageMultiplier = 1f; // Sýfýrlama
        currentRadiusMultiplier = 1f; // Sýfýrlama
        currentTickRateMultiplier = 1f; // Sýfýrlama

        // Null kontrolleri...
        if (weaponData == null) Debug.LogError("AEWH Initialize: WeaponData null!", this);
        if (playerStats == null) Debug.LogError("AEWH Initialize: PlayerStats null!", this);

        // Hesaplamalarý ve görseli Initialize içinde yapalým
        if (weaponData != null && playerStats != null)
        {
            CalculateDamagePerTick();
            tickTimer = GetCurrentTickRate(); // Tick rate'i hesapla
            UpdateVisual(); // Görseli oluþtur/güncelle
        }

        Debug.Log($"AreaEffectWeaponHandler baþlatýldý: {weaponData?.weaponName ?? "DATA YOK"}");
    }

    void Update()
    {
        if (weaponData == null || playerStats == null || !this.enabled) return;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            ApplyAuraDamage();
            tickTimer = GetCurrentTickRate(); // Tick rate'i tekrar hesapla (yükseltmelerden etkilenmiþ olabilir)
        }
    }

    private void ApplyAuraDamage()
    {
        float finalRadius = weaponData.effectRadius * (playerStats.CurrentSizeMultiplier / 100f) * currentRadiusMultiplier;
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, finalRadius, hitColliders, enemyLayer);

        for (int i = 0; i < numColliders; i++)
        {
            EnemyStats enemy = hitColliders[i].GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                float finalBaseDamagePerTick = damagePerTick * (playerStats.CurrentDamageMultiplier / 100f);
                int finalDamageToDeal = Mathf.RoundToInt(finalBaseDamagePerTick * currentDamageMultiplier);
                // TODO: Crit, Düþman Tipi Hasarý, Pierce?

                enemy.TakeDamage(finalDamageToDeal);
                GetComponent<PlayerHealth>()?.ApplyLifeSteal(finalDamageToDeal); // Can çalma
            }
        }
    }

    private void CalculateDamagePerTick()
    {
        if (weaponData != null)
        {
            damagePerTick = weaponData.damagePerSecond * GetCurrentTickRate(); // Saniye baþýna hasar * güncel tick süresi
        }
    }

    private float GetCurrentTickRate()
    {
        if (weaponData == null || playerStats == null) return 1f;
        float playerAttackSpeedFactor = (playerStats.CurrentAttackSpeedMultiplier > 0) ? 100f / playerStats.CurrentAttackSpeedMultiplier : 1f;
        float finalTickRate = weaponData.tickRate * playerAttackSpeedFactor * currentTickRateMultiplier;
        return Mathf.Max(0.05f, finalTickRate);
    }

    private void UpdateVisual()
    {
        if (weaponData == null || playerStats == null) return;
        float finalRadius = weaponData.effectRadius * (playerStats.CurrentSizeMultiplier / 100f) * currentRadiusMultiplier;

        if (weaponData.areaEffectPrefab != null && currentAreaVisual == null)
        {
            currentAreaVisual = Instantiate(weaponData.areaEffectPrefab, transform.position, Quaternion.identity, transform);
        }
        if (currentAreaVisual != null)
        {
            currentAreaVisual.transform.localScale = Vector3.one * (finalRadius * 2);
        }
    }

    void OnDisable()
    {
        if (currentAreaVisual != null) Destroy(currentAreaVisual);
    }

    // --- Yükseltme Metotlarý ---
    public void IncreaseDamageMultiplier(float percentageIncrease) { currentDamageMultiplier *= (1 + percentageIncrease / 100f); CalculateDamagePerTick(); }
    public void IncreaseRangeMultiplier(float percentageIncrease) { currentRadiusMultiplier *= (1 + percentageIncrease / 100f); UpdateVisual(); }
    public void DecreaseTickRate(float percentageDecrease) { currentTickRateMultiplier *= (1 - percentageDecrease / 100f); CalculateDamagePerTick(); } // Tick Rate = Cooldown gibi düþünülebilir
}