using UnityEngine;
using System.Collections;

public class WaveWeaponHandler : MonoBehaviour
{
    // --- SINIF SEVÝYESÝ DEÐÝÞKENLERÝ ---
    public WeaponData weaponData;
    private TargetingSystem targetingSystem;
    private Transform firePoint;
    private PlayerStats playerStats; // Referans
    private float cooldownTimer;
    // Yükseltme Deðerleri
    private float currentDamageMultiplier = 1f;
    private float currentWidthMultiplier = 1f;
    private float currentSpeedMultiplier = 1f;
    private float currentCooldownMultiplier = 1f;
    private int extraPierce = 0;
    // --- Deðiþkenler Bitti ---


    // --- Metotlar ---
    public void Initialize(WeaponData data, Transform firePointRef, TargetingSystem targetingSys, PlayerStats pStats)
    {
        weaponData = data;
        firePoint = firePointRef;
        targetingSystem = targetingSys;
        playerStats = pStats; // Atama
        cooldownTimer = 0f;
        currentDamageMultiplier = 1f; // Sýfýrlama
        currentWidthMultiplier = 1f; // Sýfýrlama
        currentSpeedMultiplier = 1f; // Sýfýrlama
        currentCooldownMultiplier = 1f; // Sýfýrlama
        extraPierce = 0; // Sýfýrlama

        // Null kontrolleri...
        if (weaponData == null) Debug.LogError("WWH Initialize: WeaponData null!", this);
        if (firePoint == null) Debug.LogError("WWH Initialize: FirePoint null!", this);
        if (targetingSystem == null) Debug.LogError("WWH Initialize: TargetingSystem null!", this);
        if (playerStats == null) Debug.LogError("WWH Initialize: PlayerStats null!", this);
        if (weaponData != null && weaponData.wavePrefab == null) Debug.LogError($"WWH Initialize: {weaponData.weaponName} için Wave Prefab atanmamýþ!", this);

        Debug.Log($"WaveWeaponHandler baþlatýldý: {weaponData?.weaponName ?? "DATA YOK"}");
    }

    void Update()
    {
        if (weaponData == null || firePoint == null || targetingSystem == null || playerStats == null || !this.enabled) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && targetingSystem.CurrentTarget != null) // Hedef gerektiriyor
        {
            FireWave();
            float baseCooldown = weaponData.cooldown;
            float playerAttackSpeedFactor = (playerStats.CurrentAttackSpeedMultiplier > 0) ? 100f / playerStats.CurrentAttackSpeedMultiplier : 1f;
            float finalCooldown = baseCooldown * playerAttackSpeedFactor * currentCooldownMultiplier;
            cooldownTimer = Mathf.Max(0.05f, finalCooldown);
        }
    }

    private void FireWave()
    {
        if (targetingSystem.CurrentTarget == null || weaponData.wavePrefab == null || playerStats == null) return;

        firePoint.LookAt(targetingSystem.CurrentTarget);
        GameObject waveGO = Instantiate(weaponData.wavePrefab, firePoint.position, firePoint.rotation);
        WaveProjectile waveScript = waveGO.GetComponent<WaveProjectile>();

        if (waveScript != null)
        {
            float finalBaseDamage = weaponData.baseDamage * (playerStats.CurrentDamageMultiplier / 100f);
            int calculatedDamage = Mathf.RoundToInt(finalBaseDamage * currentDamageMultiplier);
            float calculatedSpeed = weaponData.waveSpeed * currentSpeedMultiplier; // PlayerStats SpeedMultiplier?
            float calculatedWidth = weaponData.waveWidth * (playerStats.CurrentSizeMultiplier / 100f) * currentWidthMultiplier;
            float calculatedLifetime = weaponData.waveLifetime * (playerStats.CurrentDurationMultiplier / 100f);
            int calculatedPierce = weaponData.wavePierceCount + extraPierce + Mathf.RoundToInt(playerStats.CurrentPierce); // PlayerStats Pierce
            float finalCritChance = playerStats.CurrentCritChance;
            float finalCritDamageMult = playerStats.CurrentCritDamage / 100f;

            waveScript.Setup(calculatedDamage, finalCritChance, finalCritDamageMult, calculatedSpeed, calculatedWidth, calculatedLifetime, calculatedPierce);
        }
        else { Debug.LogError($"{weaponData.wavePrefab.name} prefab'ýnda WaveProjectile script'i bulunamadý!"); }
    }

    // --- Yükseltme Metotlarý ---
    public void IncreaseDamageMultiplier(float percentageIncrease) { currentDamageMultiplier *= (1 + percentageIncrease / 100f); }
    public void IncreaseWaveWidth(float percentageIncrease) { currentWidthMultiplier *= (1 + percentageIncrease / 100f); }
    public void IncreaseWaveSpeed(float percentageIncrease) { currentSpeedMultiplier *= (1 + percentageIncrease / 100f); }
    public void DecreaseCooldown(float percentageDecrease) { currentCooldownMultiplier *= (1 - percentageDecrease / 100f); }
    public void IncreasePierce(int amount) { extraPierce += amount; }
}