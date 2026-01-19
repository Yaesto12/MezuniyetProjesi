using UnityEngine;
using System.Collections;

public class ProjectileWeaponHandler : MonoBehaviour
{
    public WeaponData weaponData;
    private TargetingSystem targetingSystem;
    private Transform firePoint;
    private PlayerStats playerStats;
    private float cooldownTimer;

    // Ses Hoparlörü
    private AudioSource audioSource;
    [Tooltip("Pitch (Perde) rastgele olsun mu?")]
    [SerializeField] private bool randomizePitch = true;

    // ... (Diðer deðiþkenlerin aynen kalsýn) ...
    private float currentDamageMultiplier = 1f;
    private int extraProjectiles = 0;
    private float currentCooldownMultiplier = 1f;
    private float currentProjectileScaleMultiplier = 1f;
    private float currentProjectileSpeedMultiplier = 1f;
    private int extraBounce = 0;
    private float extraPierce = 0f;
    private float extraDuration = 0f;
    private float extraCritChance = 0f;
    private float extraCritDamage = 0f;
    private bool isFiring = false;

    public void Initialize(WeaponData data, Transform firePointRef, TargetingSystem targetingSys, PlayerStats pStats)
    {
        weaponData = data;
        firePoint = firePointRef;
        targetingSystem = targetingSys;
        playerStats = pStats;
        cooldownTimer = 0f;

        // Resetleme iþlemleri...
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

        // --- SES BÝLEÞENÝ (HOPARLÖR) HAZIRLIÐI ---
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f;
        }
        // ------------------------------------------

        // Hata kontrolleri...
        if (playerStats == null) Debug.LogError("PWH Initialize: PlayerStats null!", this);
        // ...
    }

    void Update()
    {
        // ... Update mantýðý aynen kalsýn ...
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

            // --- SESÝ DATA'DAN OKU VE ÇAL ---
            // Her mermide bir "týk" sesi çýkmasý için buraya koyduk
            if (weaponData.fireSound != null && audioSource != null)
            {
                if (randomizePitch) audioSource.pitch = Random.Range(0.9f, 1.1f);
                else audioSource.pitch = 1f;

                audioSource.PlayOneShot(weaponData.fireSound, weaponData.soundVolume);
            }
            // --------------------------------

            Projectile projectileScript = projectileGO.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                // Stat hesaplamalarý ve Setup aynen kalsýn...
                float finalBaseDamage = weaponData.baseDamage * (playerStats.CurrentDamageMultiplier / 100f);
                int calculatedDamage = Mathf.RoundToInt(finalBaseDamage * currentDamageMultiplier);
                float calculatedSpeed = weaponData.projectileSpeed * currentProjectileSpeedMultiplier * (playerStats.CurrentProjectileSpeedMultiplier / 100f);
                float calculatedScaleValue = weaponData.projectileScale * (playerStats.CurrentSizeMultiplier / 100f) * currentProjectileScaleMultiplier;
                Vector3 calculatedScale = Vector3.one * calculatedScaleValue;
                float calculatedLifetime = (weaponData.projectileLifetime + extraDuration) * (playerStats.CurrentDurationMultiplier / 100f);
                float finalCritChance = playerStats.CurrentCritChance + extraCritChance;
                float finalCritDamageMult = (playerStats.CurrentCritDamage + extraCritDamage) / 100f;
                float bleedPercent = playerStats.CurrentBleedPercent;
                float critBleedPercent = playerStats.CurrentCritBleedPercent;

                projectileScript.Setup(calculatedDamage, finalCritChance, finalCritDamageMult, calculatedSpeed, calculatedScale, calculatedLifetime, bleedPercent, critBleedPercent);
            }

            if (totalProjectiles > 1 && i < totalProjectiles - 1)
            {
                yield return new WaitForSeconds(weaponData.timeBetweenProjectiles);
            }
        }
        isFiring = false;
    }

    // Yükseltme metodlarý aynen kalsýn...
    public void IncreaseDamageMultiplier(float percentage) { currentDamageMultiplier *= (1 + percentage / 100f); }
    public void IncreaseProjectileCount(int amount) { extraProjectiles += amount; }
    public void DecreaseCooldown(float percentage) { currentCooldownMultiplier *= (1 - percentage / 100f); }
    public void IncreaseProjectileScale(float percentage) { currentProjectileScaleMultiplier *= (1 + percentage / 100f); }
    public void IncreaseProjectileSpeed(float percentage) { currentProjectileSpeedMultiplier *= (1 + percentage / 100f); }
    public void IncreaseDuration(float percentage) { extraDuration += (weaponData.projectileLifetime * percentage / 100f); }
    public void IncreasePierce(float amount) { extraPierce += amount; }
    public void IncreaseBounce(int amount) { extraBounce += amount; }
    public void IncreaseCritChance(float amount) { extraCritChance += amount; }
    public void IncreaseCritDamage(float percentage) { extraCritDamage += percentage; }
}