using UnityEngine;
using System.Collections;

public class ProjectileWeaponHandler : MonoBehaviour
{
    // --- SINIF SEVÝYESÝ DEÐÝÞKENLERÝ ---
    public WeaponData weaponData;
    private TargetingSystem targetingSystem;
    private Transform firePoint;
    private PlayerStats playerStats;
    private float cooldownTimer;

    // --- Yükseltme Deðerleri ---
    private float currentDamageMultiplier = 1f;
    private int extraProjectiles = 0;
    private float currentCooldownMultiplier = 1f;
    private float currentProjectileScaleMultiplier = 1f;
    private float currentProjectileSpeedMultiplier = 1f;
    private int extraBounce = 0;
    private float extraPierce = 0f;
    private float extraDuration = 0f; // Lifetime için
    private float extraCritChance = 0f;
    private float extraCritDamage = 0f;

    private bool isFiring = false;
    // --- Deðiþkenler Bitti ---


    public void Initialize(WeaponData data, Transform firePointRef, TargetingSystem targetingSys, PlayerStats pStats)
    {
        weaponData = data;
        firePoint = firePointRef;
        targetingSystem = targetingSys;
        playerStats = pStats;
        cooldownTimer = 0f;
        // Yükseltmeleri sýfýrla
        currentDamageMultiplier = 1f;
        extraProjectiles = 0;
        currentCooldownMultiplier = 1f;
        currentProjectileScaleMultiplier = 1f;
        currentProjectileSpeedMultiplier = 1f;
        extraBounce = 0;
        extraPierce = 0f;
        extraDuration = 0f;
        extraCritChance = 0f;
        extraCritDamage = 0f;
        isFiring = false;

        if (playerStats == null) Debug.LogError("PWH Initialize: PlayerStats null!", this);
        if (weaponData == null) Debug.LogError("PWH Initialize: WeaponData null!", this);
        if (firePoint == null) Debug.LogError("PWH Initialize: FirePoint null!", this);
        if (targetingSystem == null) Debug.LogError("PWH Initialize: TargetingSystem null!", this);
    }

    void Update()
    {
        if (weaponData == null || firePoint == null || targetingSystem == null || playerStats == null || !this.enabled) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && !isFiring && targetingSystem.CurrentTarget != null)
        {
            StartCoroutine(FireBurstCoroutine());
            float baseCooldown = weaponData.cooldown;
            float playerAttackSpeedFactor = (playerStats.CurrentAttackSpeedMultiplier > 0) ? 100f / playerStats.CurrentAttackSpeedMultiplier : 1f;
            float finalCooldown = baseCooldown * playerAttackSpeedFactor * currentCooldownMultiplier;
            cooldownTimer = Mathf.Max(0.05f, finalCooldown);
        }
    }

    private IEnumerator FireBurstCoroutine()
    {
        if (targetingSystem.CurrentTarget == null || weaponData == null || firePoint == null || playerStats == null) { isFiring = false; yield break; }
        if (weaponData.projectilePrefab == null) { Debug.LogError($"{weaponData.weaponName} için Projectile Prefab atanmamýþ!"); isFiring = false; yield break; }

        isFiring = true;
        firePoint.LookAt(targetingSystem.CurrentTarget);

        int totalProjectiles = weaponData.projectileCount + extraProjectiles + playerStats.CurrentProjectileCountBonus;
        totalProjectiles = Mathf.Max(1, totalProjectiles);

        float angleStep = (totalProjectiles > 1 && weaponData.spreadAngle > 0) ? weaponData.spreadAngle / (totalProjectiles - 1) : 0f;
        float startAngle = (totalProjectiles > 1 && weaponData.spreadAngle > 0) ? -weaponData.spreadAngle / 2f : 0f;

        for (int i = 0; i < totalProjectiles; i++)
        {
            Quaternion spreadRotation = Quaternion.Euler(0, startAngle + (i * angleStep), 0);
            Quaternion finalRotation = firePoint.rotation * spreadRotation;
            GameObject projectileGO = Instantiate(weaponData.projectilePrefab, firePoint.position, finalRotation);
            Projectile projectileScript = projectileGO.GetComponent<Projectile>();

            if (projectileScript != null)
            {
                float finalBaseDamage = weaponData.baseDamage * (playerStats.CurrentDamageMultiplier / 100f);
                int calculatedDamage = Mathf.RoundToInt(finalBaseDamage * currentDamageMultiplier);
                float calculatedSpeed = weaponData.projectileSpeed * currentProjectileSpeedMultiplier;
                float calculatedScaleValue = weaponData.projectileScale * (playerStats.CurrentSizeMultiplier / 100f) * currentProjectileScaleMultiplier;
                Vector3 calculatedScale = Vector3.one * calculatedScaleValue;
                float calculatedLifetime = (weaponData.projectileLifetime + extraDuration) * (playerStats.CurrentDurationMultiplier / 100f);
                float finalCritChance = playerStats.CurrentCritChance + extraCritChance;
                float finalCritDamageMult = (playerStats.CurrentCritDamage + extraCritDamage) / 100f;
                int bounceCount = playerStats.CurrentProjectileBounce + extraBounce;
                float pierceValue = playerStats.CurrentPierce + extraPierce;

                // TODO: Projectile.cs'in Setup'ýnýn bounce ve pierce alacak þekilde güncellenmesi GEREKÝR
                projectileScript.Setup(calculatedDamage, finalCritChance, finalCritDamageMult, calculatedSpeed, calculatedScale, calculatedLifetime);
            }
            else { Debug.LogError($"{weaponData.projectilePrefab.name} prefab'ýnda Projectile script'i bulunamadý!"); }

            if (totalProjectiles > 1 && i < totalProjectiles - 1)
            {
                yield return new WaitForSeconds(weaponData.timeBetweenProjectiles);
            }
        }
        isFiring = false;
    }

    // --- Yükseltme Metotlarý (Eksikler Eklendi) ---
    public void IncreaseDamageMultiplier(float percentage) { currentDamageMultiplier *= (1 + percentage / 100f); }
    public void IncreaseProjectileCount(int amount) { extraProjectiles += amount; }
    public void DecreaseCooldown(float percentage) { currentCooldownMultiplier *= (1 - percentage / 100f); }
    public void IncreaseProjectileScale(float percentage) { currentProjectileScaleMultiplier *= (1 + percentage / 100f); }
    public void IncreaseProjectileSpeed(float percentage) { currentProjectileSpeedMultiplier *= (1 + percentage / 100f); }
    public void IncreaseDuration(float percentage) { extraDuration += (weaponData.projectileLifetime * percentage / 100f); } // Veya sabit?
    public void IncreasePierce(float amount) { extraPierce += amount; }
    public void IncreaseBounce(int amount) { extraBounce += amount; }
    public void IncreaseCritChance(float amount) { extraCritChance += amount; }
    public void IncreaseCritDamage(float percentage) { extraCritDamage += percentage; }
}