using UnityEngine;
using System.Collections;

public class WaveWeaponHandler : MonoBehaviour
{
    public WeaponData weaponData;
    private TargetingSystem targetingSystem;
    private Transform firePoint;
    private PlayerStats playerStats;
    private float cooldownTimer;

    // Ses için gerekli hoparlör (Script otomatik ekleyecek)
    private AudioSource audioSource;
    [Tooltip("Pitch (Perde) rastgele olsun mu?")]
    [SerializeField] private bool randomizePitch = true; // Bu genel ayar olarak burada kalabilir

    // ... (Diðer deðiþkenlerin aynen kalsýn) ...
    private float currentDamageMultiplier = 1f;
    private float currentWidthMultiplier = 1f;
    private float currentSpeedMultiplier = 1f;
    private float currentCooldownMultiplier = 1f;
    private int extraPierce = 0;

    public void Initialize(WeaponData data, Transform firePointRef, TargetingSystem targetingSys, PlayerStats pStats)
    {
        weaponData = data;
        firePoint = firePointRef;
        targetingSystem = targetingSys;
        playerStats = pStats;
        cooldownTimer = 0f;

        // Resetleme iþlemleri...
        currentDamageMultiplier = 1f;
        currentWidthMultiplier = 1f;
        currentSpeedMultiplier = 1f;
        currentCooldownMultiplier = 1f;
        extraPierce = 0;

        // --- SES BÝLEÞENÝ (HOPARLÖR) HAZIRLIÐI ---
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f;
        }
        // ------------------------------------------

        // Hata kontrolleri aynen kalsýn...
    }

    void Update()
    {
        if (weaponData == null || firePoint == null || targetingSystem == null || playerStats == null || !this.enabled) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && targetingSystem.CurrentTarget != null)
        {
            FireWave();
            // Cooldown hesaplamalarý aynen kalsýn...
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

        // --- SESÝ DATA'DAN OKU VE ÇAL ---
        if (weaponData.fireSound != null && audioSource != null)
        {
            // Pitch (Perde) varyasyonu ekle ki makineli tüfek gibi durmasýn
            if (randomizePitch) audioSource.pitch = Random.Range(0.9f, 1.1f);
            else audioSource.pitch = 1f;

            // Data'daki sesi ve Data'daki ses þiddetini kullan
            audioSource.PlayOneShot(weaponData.fireSound, weaponData.soundVolume);
        }
        // --------------------------------

        WaveProjectile waveScript = waveGO.GetComponent<WaveProjectile>();
        if (waveScript != null)
        {
            // Stat hesaplamalarý ve Setup aynen kalsýn...
            float finalBaseDamage = weaponData.baseDamage * (playerStats.CurrentDamageMultiplier / 100f);
            int calculatedDamage = Mathf.RoundToInt(finalBaseDamage * currentDamageMultiplier);
            float calculatedSpeed = weaponData.waveSpeed * currentSpeedMultiplier;
            float calculatedWidth = weaponData.waveWidth * (playerStats.CurrentSizeMultiplier / 100f) * currentWidthMultiplier;
            float calculatedLifetime = weaponData.waveLifetime * (playerStats.CurrentDurationMultiplier / 100f);
            int calculatedPierce = weaponData.wavePierceCount + extraPierce + Mathf.RoundToInt(playerStats.CurrentPierce);
            float finalCritChance = playerStats.CurrentCritChance;
            float finalCritDamageMult = playerStats.CurrentCritDamage / 100f;

            waveScript.Setup(calculatedDamage, finalCritChance, finalCritDamageMult, calculatedSpeed, calculatedWidth, calculatedLifetime, calculatedPierce);
        }
    }

    // Yükseltme metodlarý aynen kalsýn...
    public void IncreaseDamageMultiplier(float percentageIncrease) { currentDamageMultiplier *= (1 + percentageIncrease / 100f); }
    public void IncreaseWaveWidth(float percentageIncrease) { currentWidthMultiplier *= (1 + percentageIncrease / 100f); }
    public void IncreaseWaveSpeed(float percentageIncrease) { currentSpeedMultiplier *= (1 + percentageIncrease / 100f); }
    public void DecreaseCooldown(float percentageDecrease) { currentCooldownMultiplier *= (1 - percentageDecrease / 100f); }
    public void IncreasePierce(int amount) { extraPierce += amount; }
}