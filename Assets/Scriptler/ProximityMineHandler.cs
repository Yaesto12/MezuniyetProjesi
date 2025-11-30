using UnityEngine;
using System.Collections;

public class ProximityMineHandler : MonoBehaviour
{
    // --- SINIF SEVÝYESÝ DEÐÝÞKENLERÝ ---
    public WeaponData weaponData;
    private PlayerStats playerStats;
    private float cooldownTimer;
    private TargetingSystem targetingSystem; // Opsiyonel
    // Yükseltme Deðerleri
    private float currentDamageMultiplier = 1f;
    private int extraMines = 0;
    private float currentRadiusMultiplier = 1f;
    private float currentCooldownMultiplier = 1f;
    private float currentDurationMultiplier = 1f;
    // --- Deðiþkenler Bitti ---

    public void Initialize(WeaponData data, TargetingSystem targetingSys, PlayerStats pStats)
    {
        weaponData = data;
        targetingSystem = targetingSys;
        playerStats = pStats;
        cooldownTimer = 0f;
        currentDamageMultiplier = 1f;
        extraMines = 0;
        currentRadiusMultiplier = 1f;
        currentCooldownMultiplier = 1f;
        currentDurationMultiplier = 1f;

        if (weaponData == null) Debug.LogError("PMH Initialize: WeaponData null!", this);
        if (playerStats == null) Debug.LogError("PMH Initialize: PlayerStats null!", this);
        if (weaponData != null && weaponData.minePrefab == null) Debug.LogError($"PMH Initialize: {weaponData.weaponName} için Mine Prefab atanmamýþ!", this);

        Debug.Log($"ProximityMineHandler baþlatýldý: {weaponData?.weaponName ?? "DATA YOK"}");
    }

    void Update()
    {
        if (weaponData == null || playerStats == null || !this.enabled) return;
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            DropMines();
            float baseCooldown = weaponData.cooldown;
            float playerAttackSpeedFactor = (playerStats.CurrentAttackSpeedMultiplier > 0) ? 100f / playerStats.CurrentAttackSpeedMultiplier : 1f;
            float finalCooldown = baseCooldown * playerAttackSpeedFactor * currentCooldownMultiplier;
            cooldownTimer = Mathf.Max(0.1f, finalCooldown);
        }
    }

    private void DropMines()
    {
        if (weaponData.minePrefab == null || playerStats == null) return;

        int totalMines = weaponData.mineCount + extraMines + playerStats.CurrentProjectileCountBonus; // Genel bonus
        totalMines = Mathf.Max(1, totalMines);

        float finalBaseDamage = weaponData.baseDamage * (playerStats.CurrentDamageMultiplier / 100f);
        int finalDamage = Mathf.RoundToInt(finalBaseDamage * currentDamageMultiplier);
        float finalRadius = weaponData.mineExplosionRadius * (playerStats.CurrentSizeMultiplier / 100f) * currentRadiusMultiplier;
        float finalLifetime = weaponData.mineLifetime * (playerStats.CurrentDurationMultiplier / 100f) * currentDurationMultiplier;
        float finalCritChance = playerStats.CurrentCritChance; // Crit þansýný PlayerStats'tan al
        float finalCritDamageMult = playerStats.CurrentCritDamage / 100f; // Crit çarpanýný PlayerStats'tan al

        for (int i = 0; i < totalMines; i++)
        {
            float dropDist = weaponData.mineDropRadius;
            Vector3 spawnOffset = Random.insideUnitSphere * dropDist;
            spawnOffset.y = 0.1f;
            Vector3 spawnPos = transform.position + spawnOffset;

            GameObject mineGO = Instantiate(weaponData.minePrefab, spawnPos, Quaternion.identity);
            Mine mineScript = mineGO.GetComponent<Mine>();

            if (mineScript != null)
            {
                // Mine.Setup metodunu doðru 5 parametre ile çaðýr
                mineScript.Setup(finalDamage, finalCritChance, finalCritDamageMult, finalRadius, finalLifetime); // <<<--- DOÐRU ÇAÐRI ---<<<
            }
            else { Debug.LogError($"{weaponData.minePrefab.name} prefab'ýnda Mine script'i bulunamadý!"); }
        }
    }

    // --- Yükseltme Metotlarý (percentageDecrease Düzeltildi) ---
    public void IncreaseDamageMultiplier(float value) // Parametre adý 'value' olarak düzeltildi (yüzde olup olmadýðý PlayerExperience'da)
    {
        // Yüzde artýþ varsayýmý
        currentDamageMultiplier *= (1 + value / 100f);
        Debug.Log($"{weaponData?.weaponName ?? "..."} Hasar Çarpaný: x{currentDamageMultiplier}");
    }
    public void IncreaseMineCount(int amount)
    {
        extraMines += amount;
        Debug.Log($"{weaponData?.weaponName ?? "..."} Ekstra Mayýn: +{extraMines}");
    }
    public void IncreaseExplosionRadius(float value) // Parametre adý 'value' olarak düzeltildi
    {
        // Yüzde artýþ varsayýmý
        currentRadiusMultiplier *= (1 + value / 100f);
        Debug.Log($"{weaponData?.weaponName ?? "..."} Patlama Alan Çarpaný: x{currentRadiusMultiplier}");
    }
    public void DecreaseCooldown(float value) // Parametre adý 'value' olarak düzeltildi
    {
        // Yüzde azaltma varsayýmý
        currentCooldownMultiplier *= (1 - value / 100f);
        Debug.Log($"{weaponData?.weaponName ?? "..."} Cooldown Çarpaný: x{currentCooldownMultiplier}");
    }
    public void IncreaseMineLifetime(float value) // Parametre adý 'value' olarak düzeltildi
    {
        // Yüzde artýþ varsayýmý
        currentDurationMultiplier *= (1 + value / 100f);
        Debug.Log($"{weaponData?.weaponName ?? "..."} Mayýn Ömür Çarpaný: x{currentDurationMultiplier}");
    }
}