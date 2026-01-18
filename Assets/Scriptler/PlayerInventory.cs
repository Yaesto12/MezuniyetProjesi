using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerInventory : MonoBehaviour
{
    [Header("Ýstatistikler")]
    public int EliteKills { get; private set; } = 0;
    public int TotalKills { get; private set; } = 0;

    [Header("Silahlar")]
    public List<WeaponData> weapons = new List<WeaponData>();

    [Header("Itemler (Sadece Pasif Eþyalar Görünsün)")]
    public List<ItemData> ownedItems = new List<ItemData>();

    private PlayerStats playerStats;
    private PlayerWeaponController weaponController;

    // --- ÖZEL ITEMLER ÝÇÝN OLAY SÝSTEMÝ ---
    public event Action OnEnemyKilled;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        weaponController = GetComponent<PlayerWeaponController>();
    }

    public void RegisterKill(bool isElite)
    {
        TotalKills++;
        if (isElite) EliteKills++;

        OnEnemyKilled?.Invoke();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateKillCount(TotalKills);
        }
    }

    public void AddEliteKill()
    {
        EliteKills++;
        OnEnemyKilled?.Invoke();
        if (UIManager.Instance != null) UIManager.Instance.UpdateKillCount(TotalKills);
    }

    // ========================================================================
    // --- ITEM YÖNETÝMÝ ---
    // ========================================================================

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        // Eþyayý listeye ekle (UI'da görünmesi için)
        ownedItems.Add(item);
        Debug.Log($"<color=green>ENVANTERE EKLENDÝ: {item.itemName}</color>");

        // --- STAT GÜNCELLEME ---
        if (item.modifiers != null && playerStats != null)
        {
            foreach (var mod in item.modifiers)
            {
                ApplyPassiveStat(mod.statType, mod.baseAmount);
            }
            playerStats.RecalculateStats();
        }

        // --- EFEKT / ÖZEL SCRIPT KISMI ---
        if (item.specialEffectPrefab != null && playerStats != null)
        {
            if (!item.createEffectPerStack && GetItemLevel(item) > 1) { }
            else
            {
                GameObject effectObj = Instantiate(item.specialEffectPrefab, transform.position, Quaternion.identity);
                effectObj.transform.SetParent(transform);
                var effectScript = effectObj.GetComponent<MonoBehaviour>();
                if (effectScript != null)
                {
                    effectScript.SendMessage("OnEquip", new object[] { playerStats, this }, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        // UI GÜNCELLEME (Pause Menüsü için)
        RefreshUI();
    }

    private void ApplyPassiveStat(PassiveStatType type, float val)
    {
        switch (type)
        {
            case PassiveStatType.MaxHealth: playerStats.IncreaseBaseMaxHealth(val); break;
            case PassiveStatType.HpRegen: playerStats.IncreaseBaseHpRegen(val); break;
            case PassiveStatType.Armor: playerStats.IncreaseBaseArmor(val); break;
            case PassiveStatType.Evasion: playerStats.IncreaseBaseEvasion(val); break;
            case PassiveStatType.LifeSteal: playerStats.IncreaseBaseLifeSteal(val); break;
            case PassiveStatType.Thorns: playerStats.IncreaseBaseThorns(val); break;
            case PassiveStatType.MoveSpeed: playerStats.IncreaseBaseMoveSpeed(val); break;
            case PassiveStatType.ExtraJumps: playerStats.IncreaseBaseExtraJumps((int)val); break;
            case PassiveStatType.JumpHeight: playerStats.IncreaseBaseJumpHeight(val); break;
            case PassiveStatType.Damage: playerStats.IncreaseBaseDamageMultiplier(val); break;
            case PassiveStatType.AttackSpeed: playerStats.IncreaseBaseAttackSpeedMultiplier(val); break;
            case PassiveStatType.CritChance: playerStats.IncreaseBaseCritChance(val); break;
            case PassiveStatType.CritDamage: playerStats.IncreaseBaseCritDamage(val); break;
            case PassiveStatType.ProjectileCount: playerStats.IncreaseBaseProjectileCount((int)val); break;
            case PassiveStatType.ProjectileSpeed: playerStats.IncreaseBaseProjectileSpeed(val); break;
            case PassiveStatType.ProjectileBounce: playerStats.IncreaseBaseProjectileBounce((int)val); break;
            case PassiveStatType.Pierce: playerStats.IncreaseBasePierce(val); break;
            case PassiveStatType.AreaSize: playerStats.IncreaseBaseSize(val); break;
            case PassiveStatType.Duration: playerStats.IncreaseBaseDuration(val); break;
            case PassiveStatType.CooldownReduction: playerStats.IncreaseBaseCooldownReduction(val); break;
            case PassiveStatType.Luck: playerStats.IncreaseBaseLuck(val); break;
            case PassiveStatType.MagnetRange: playerStats.IncreaseBaseMagnetRange(val); break;
            case PassiveStatType.XpBonus: playerStats.IncreaseBaseXpBonus(val); break;
            case PassiveStatType.GoldBonus: playerStats.IncreaseBaseGoldBonus(val); break;
            case PassiveStatType.Curse: playerStats.IncreaseBaseCurse(val); break;
            case PassiveStatType.DropChance: playerStats.IncreaseBaseDropChance(val); break;
            case PassiveStatType.Revival: playerStats.IncreaseBaseRevivals((int)val); break;
            case PassiveStatType.Reroll: playerStats.IncreaseBaseRerolls((int)val); break;
            case PassiveStatType.Skip: playerStats.IncreaseBaseSkips((int)val); break;
            case PassiveStatType.Banish: playerStats.IncreaseBaseBanishes((int)val); break;
        }
    }

    public int GetItemLevel(ItemData itemToCheck)
    {
        int count = 0;
        foreach (var item in ownedItems) if (item == itemToCheck) count++;
        return count;
    }

    // ========================================================================
    // --- SÝLAH YÖNETÝMÝ ---
    // ========================================================================

    public bool HasWeaponData(WeaponData weaponData) { return weapons.Contains(weaponData); }

    public void AddWeaponData(WeaponData weaponData)
    {
        if (!weapons.Contains(weaponData))
        {
            // 1. Silahlar listesine ekle (Çalýþmasý için ZORUNLU)
            weapons.Add(weaponData);

            // 2. Controller'a haber ver (Ateþ etmesi için ZORUNLU)
            if (weaponController != null)
            {
                // Doðrudan fonksiyonu çaðýrýyoruz. Hata varsa kesin çalýþýr.
                weaponController.AddWeapon(weaponData);
            }
            // --- DEÐÝÞÝKLÝK BURADA ---
            // ownedItems.Add(...) kýsmýný kaldýrdýk.
            // Böylece silahlar mekanik olarak çalýþacak ama UI listesinde görünmeyecek.

            Debug.Log($"Silah Eklendi ve Aktif Edildi: {weaponData.name} (UI'da gizli)");
        }
    }

    public void IncrementUpgradeLevel(WeaponData weaponData)
    {
        if (weaponController != null)
            weaponController.SendMessage("LevelUpWeapon", weaponData, SendMessageOptions.DontRequireReceiver);

        // Silah seviyesi artsa bile UI'da göstermiyoruz.
    }

    private void RefreshUI()
    {
        if (PauseMenuInfo.Instance != null && PauseMenuInfo.Instance.gameObject.activeInHierarchy)
        {
            PauseMenuInfo.Instance.gameObject.SetActive(false);
            PauseMenuInfo.Instance.gameObject.SetActive(true);
        }
    }
}