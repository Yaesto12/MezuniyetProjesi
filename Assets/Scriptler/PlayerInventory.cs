using UnityEngine;
using System.Collections.Generic;
using System; // Action için gerekli

public class PlayerInventory : MonoBehaviour
{
    [Header("Ýstatistikler")]
    public int EliteKills { get; private set; } = 0;
    public int TotalKills { get; private set; } = 0; // Genel kill sayacý

    [Header("Silahlar")]
    public List<WeaponData> weapons = new List<WeaponData>();

    [Header("Itemler")]
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

    // --- BU FONKSÝYONU DÜÞMAN ÖLDÜÐÜNDE ÇAÐIRMALISIN ---
    public void RegisterKill(bool isElite)
    {
        TotalKills++;
        if (isElite) EliteKills++;

        // Özel itemlere haber ver
        OnEnemyKilled?.Invoke();

        // --- YENÝ EKLENEN KISIM: UI GÜNCELLEME ---
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateKillCount(TotalKills);
        }
        // ----------------------------------------
    }

    public void AddEliteKill()
    {
        EliteKills++;
        // Geriye dönük uyumluluk için RegisterKill'i buradan da çaðýrabiliriz
        OnEnemyKilled?.Invoke();

        // EliteKill de bir kill olduðu için UI güncellensin (isteðe baðlý)
        if (UIManager.Instance != null) UIManager.Instance.UpdateKillCount(TotalKills);
    }

    // ========================================================================
    // --- ITEM YÖNETÝMÝ ---
    // ========================================================================

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        ownedItems.Add(item);
        Debug.Log($"<color=green>ENVANTERE EKLENDÝ: {item.itemName}</color>");

        // --- STAT GÜNCELLEME ---
        if (item.modifiers != null && playerStats != null)
        {
            foreach (var mod in item.modifiers)
            {
                float val = mod.baseAmount;

                switch (mod.statType)
                {
                    // Savunma
                    case PassiveStatType.MaxHealth: playerStats.IncreaseBaseMaxHealth(val); break;
                    case PassiveStatType.HpRegen: playerStats.IncreaseBaseHpRegen(val); break;
                    case PassiveStatType.Armor: playerStats.IncreaseBaseArmor(val); break;
                    case PassiveStatType.Evasion: playerStats.IncreaseBaseEvasion(val); break;
                    case PassiveStatType.LifeSteal: playerStats.IncreaseBaseLifeSteal(val); break;
                    case PassiveStatType.Thorns: playerStats.IncreaseBaseThorns(val); break;

                    // Hareket
                    case PassiveStatType.MoveSpeed: playerStats.IncreaseBaseMoveSpeed(val); break;
                    case PassiveStatType.ExtraJumps: playerStats.IncreaseBaseExtraJumps((int)val); break;
                    case PassiveStatType.JumpHeight: playerStats.IncreaseBaseJumpHeight(val); break;

                    // Saldýrý
                    case PassiveStatType.Damage: playerStats.IncreaseBaseDamageMultiplier(val); break;
                    case PassiveStatType.AttackSpeed: playerStats.IncreaseBaseAttackSpeedMultiplier(val); break;
                    case PassiveStatType.CritChance: playerStats.IncreaseBaseCritChance(val); break;
                    case PassiveStatType.CritDamage: playerStats.IncreaseBaseCritDamage(val); break;

                    // Mermi/Alan
                    case PassiveStatType.ProjectileCount: playerStats.IncreaseBaseProjectileCount((int)val); break;
                    case PassiveStatType.ProjectileSpeed: playerStats.IncreaseBaseProjectileSpeed(val); break;
                    case PassiveStatType.ProjectileBounce: playerStats.IncreaseBaseProjectileBounce((int)val); break;
                    case PassiveStatType.Pierce: playerStats.IncreaseBasePierce(val); break;
                    case PassiveStatType.AreaSize: playerStats.IncreaseBaseSize(val); break;
                    case PassiveStatType.Duration: playerStats.IncreaseBaseDuration(val); break;
                    case PassiveStatType.CooldownReduction: playerStats.IncreaseBaseCooldownReduction(val); break;

                    // Ekonomi/Þans
                    case PassiveStatType.Luck: playerStats.IncreaseBaseLuck(val); break;
                    case PassiveStatType.MagnetRange: playerStats.IncreaseBaseMagnetRange(val); break;
                    case PassiveStatType.XpBonus: playerStats.IncreaseBaseXpBonus(val); break;
                    case PassiveStatType.GoldBonus: playerStats.IncreaseBaseGoldBonus(val); break;
                    case PassiveStatType.Curse: playerStats.IncreaseBaseCurse(val); break;
                    case PassiveStatType.DropChance: playerStats.IncreaseBaseDropChance(val); break;

                    // Meta
                    case PassiveStatType.Revival: playerStats.IncreaseBaseRevivals((int)val); break;
                    case PassiveStatType.Reroll: playerStats.IncreaseBaseRerolls((int)val); break;
                    case PassiveStatType.Skip: playerStats.IncreaseBaseSkips((int)val); break;
                    case PassiveStatType.Banish: playerStats.IncreaseBaseBanishes((int)val); break;
                }
            }
            playerStats.RecalculateStats();

            // UI Güncelleme
            if (PauseMenuInfo.Instance != null && PauseMenuInfo.Instance.gameObject.activeInHierarchy)
            {
                PauseMenuInfo.Instance.gameObject.SetActive(false);
                PauseMenuInfo.Instance.gameObject.SetActive(true);
            }
        }

        // --- EFEKT / ÖZEL SCRIPT KISMI ---
        if (item.specialEffectPrefab != null && playerStats != null)
        {
            if (!item.createEffectPerStack && GetItemLevel(item) > 1)
            {
                // Zaten var, tekrar yaratma
            }
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
    }

    public int GetItemLevel(ItemData itemToCheck)
    {
        int count = 0;
        foreach (var item in ownedItems) if (item == itemToCheck) count++;
        return count;
    }

    // --- SÝLAH YÖNETÝMÝ ---
    public bool HasWeaponData(WeaponData weaponData) { return weapons.Contains(weaponData); }
    public void AddWeaponData(WeaponData weaponData)
    {
        if (!weapons.Contains(weaponData))
        {
            weapons.Add(weaponData);
            if (weaponController != null) weaponController.SendMessage("AddWeapon", weaponData, SendMessageOptions.DontRequireReceiver);
        }
    }
    public void IncrementUpgradeLevel(WeaponData weaponData)
    {
        if (weaponController != null) weaponController.SendMessage("LevelUpWeapon", weaponData, SendMessageOptions.DontRequireReceiver);
    }
}