using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeWeaponHandler : MonoBehaviour
{
    // --- SINIF SEVÝYESÝ DEÐÝÞKENLERÝ ---
    public WeaponData weaponData;
    private PlayerStats playerStats; // Referans
    private float cooldownTimer;
    private bool isAttacking;
    private LayerMask enemyLayer;
    // Yükseltme Deðerleri
    private float currentDamageMultiplier = 1f;
    private float currentRangeMultiplier = 1f;
    private float currentCooldownMultiplier = 1f;
    // --- Deðiþkenler Bitti ---


    // --- Metotlar ---
    public void Initialize(WeaponData data, LayerMask enemies, PlayerStats pStats)
    {
        weaponData = data;
        enemyLayer = enemies;
        playerStats = pStats; // Atama
        cooldownTimer = 0f;
        currentDamageMultiplier = 1f; // Sýfýrlama
        currentRangeMultiplier = 1f; // Sýfýrlama
        currentCooldownMultiplier = 1f; // Sýfýrlama
        isAttacking = false;

        // Null kontrolleri...
        if (weaponData == null) Debug.LogError("MWH Initialize: WeaponData null!", this);
        if (playerStats == null) Debug.LogError("MWH Initialize: PlayerStats null!", this);

        Debug.Log($"MeleeWeaponHandler baþlatýldý: {weaponData?.weaponName ?? "DATA YOK"}");
    }

    void Update()
    {
        if (weaponData == null || playerStats == null || !this.enabled) return;
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
            float baseCooldown = weaponData.cooldown;
            float playerAttackSpeedFactor = (playerStats.CurrentAttackSpeedMultiplier > 0) ? 100f / playerStats.CurrentAttackSpeedMultiplier : 1f;
            float finalCooldown = baseCooldown * playerAttackSpeedFactor * currentCooldownMultiplier;
            cooldownTimer = Mathf.Max(0.05f, finalCooldown);
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        // PlayerStats null kontrolü Coroutine içinde de önemli
        if (weaponData == null || playerStats == null) { isAttacking = false; yield break; }

        float finalRange = weaponData.attackRange * (playerStats.CurrentSizeMultiplier / 100f) * currentRangeMultiplier;
        float finalWidth = weaponData.attackWidthOrAngle * (playerStats.CurrentSizeMultiplier / 100f) * currentRangeMultiplier;
        float finalBaseDamage = weaponData.baseDamage * (playerStats.CurrentDamageMultiplier / 100f);
        int finalDamage = Mathf.RoundToInt(finalBaseDamage * currentDamageMultiplier);

        Vector3 boxCenter = transform.position + transform.forward * (finalRange / 2f);
        Vector3 halfExtents = new Vector3(finalWidth / 2f, 1.0f, finalRange / 2f);
        Collider[] hits = Physics.OverlapBox(boxCenter, halfExtents, transform.rotation, enemyLayer);
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
                GetComponent<PlayerHealth>()?.ApplyLifeSteal(damageToDeal); // Can çalma
            }
        }

        float finalDuration = weaponData.attackDuration * (playerStats.CurrentDurationMultiplier / 100f);
        if (finalDuration > 0) { yield return new WaitForSeconds(finalDuration); }

        isAttacking = false;
    }

    // --- Yükseltme Metotlarý ---
    public void IncreaseDamageMultiplier(float percentageIncrease) { currentDamageMultiplier *= (1 + percentageIncrease / 100f); }
    public void IncreaseRangeMultiplier(float percentageIncrease) { currentRangeMultiplier *= (1 + percentageIncrease / 100f); }
    public void DecreaseCooldown(float percentageDecrease) { currentCooldownMultiplier *= (1 - percentageDecrease / 100f); }
}