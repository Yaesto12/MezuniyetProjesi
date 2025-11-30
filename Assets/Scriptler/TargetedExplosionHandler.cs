using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetedExplosionHandler : MonoBehaviour
{
    // --- SINIF SEVÝYESÝ DEÐÝÞKENLERÝ ---
    public WeaponData weaponData;
    private TargetingSystem targetingSystem;
    private PlayerStats playerStats; // Referans
    private float cooldownTimer;
    // Yükseltme Deðerleri
    private float currentDamageMultiplier = 1f;
    private float currentRadiusMultiplier = 1f;
    private int extraExplosions = 0;
    private float currentCooldownMultiplier = 1f;
    // --- Deðiþkenler Bitti ---


    // --- Metotlar ---
    public void Initialize(WeaponData data, TargetingSystem targetingSys, PlayerStats pStats)
    {
        weaponData = data;
        targetingSystem = targetingSys;
        playerStats = pStats; // Atama
        cooldownTimer = 0f;
        currentDamageMultiplier = 1f; // Sýfýrlama
        currentRadiusMultiplier = 1f; // Sýfýrlama
        extraExplosions = 0; // Sýfýrlama
        currentCooldownMultiplier = 1f; // Sýfýrlama

        // Null kontrolleri...
        if (weaponData == null) Debug.LogError("TEH Initialize: WeaponData null!", this);
        if (targetingSystem == null) Debug.LogError("TEH Initialize: TargetingSystem null!", this);
        if (playerStats == null) Debug.LogError("TEH Initialize: PlayerStats null!", this);
        if (weaponData != null && weaponData.explosionPrefab == null) Debug.LogError($"TEH Initialize: {weaponData.weaponName} için Explosion Prefab atanmamýþ!", this);

        Debug.Log($"TargetedExplosionHandler baþlatýldý: {weaponData?.weaponName ?? "DATA YOK"}");
    }

    void Update()
    {
        if (weaponData == null || targetingSystem == null || playerStats == null || !this.enabled) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && targetingSystem.CurrentTarget != null)
        {
            TriggerExplosions(); // Coroutine'e çevrilmedi, anlýk hasar
            float baseCooldown = weaponData.cooldown;
            float playerAttackSpeedFactor = (playerStats.CurrentAttackSpeedMultiplier > 0) ? 100f / playerStats.CurrentAttackSpeedMultiplier : 1f;
            float finalCooldown = baseCooldown * playerAttackSpeedFactor * currentCooldownMultiplier;
            cooldownTimer = Mathf.Max(0.05f, finalCooldown);
        }
    }

    private void TriggerExplosions()
    {
        if (targetingSystem.CurrentTarget == null || weaponData.explosionPrefab == null || playerStats == null) return;

        Transform target = targetingSystem.CurrentTarget;
        int totalExplosions = weaponData.explosionCount + extraExplosions; // PlayerStats Amount?
        float finalRadius = weaponData.explosionRadius * (playerStats.CurrentSizeMultiplier / 100f) * currentRadiusMultiplier;
        float finalBaseDamage = weaponData.baseDamage * (playerStats.CurrentDamageMultiplier / 100f);
        int finalDamage = Mathf.RoundToInt(finalBaseDamage * currentDamageMultiplier);

        for (int i = 0; i < totalExplosions; i++)
        {
            GameObject explosionEffect = Instantiate(weaponData.explosionPrefab, target.position, Quaternion.identity);
            Destroy(explosionEffect, 1.5f); // Efekt süresine göre ayarla

            // Patlama alanýndaki düþmanlarý bul (Enemy Layer kullanýlmalý)
            LayerMask enemyLayer = LayerMask.GetMask("Enemy"); // Veya PlayerWeaponController'dan al
            Collider[] hits = Physics.OverlapSphere(target.position, finalRadius, enemyLayer);
            List<EnemyStats> hitEnemies = new List<EnemyStats>();

            foreach (Collider hit in hits)
            {
                EnemyStats enemy = hit.GetComponentInParent<EnemyStats>();
                if (enemy != null && !hitEnemies.Contains(enemy))
                {
                    bool isCritical = Random.value * 100 < playerStats.CurrentCritChance;
                    int damageToDeal = finalDamage;
                    if (isCritical) { damageToDeal = Mathf.RoundToInt(finalDamage * (playerStats.CurrentCritDamage / 100f)); }
                    // TODO: Düþman tipi hasarý, Pierce

                    enemy.TakeDamage(damageToDeal);
                    hitEnemies.Add(enemy);
                    GetComponent<PlayerHealth>()?.ApplyLifeSteal(damageToDeal);
                }
            }
        }
    }

    // --- Yükseltme Metotlarý ---
    public void IncreaseDamageMultiplier(float percentageIncrease) { currentDamageMultiplier *= (1 + percentageIncrease / 100f); }
    public void IncreaseRadiusMultiplier(float percentageIncrease) { currentRadiusMultiplier *= (1 + percentageIncrease / 100f); }
    public void IncreaseExplosionCount(int amount) { extraExplosions += amount; }
    public void DecreaseCooldown(float percentageDecrease) { currentCooldownMultiplier *= (1 - percentageDecrease / 100f); }
}