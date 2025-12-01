using UnityEngine;
using System.Collections.Generic; // List için

// --- BU DOSYA TÜM GLOBAL TANIMLAMALARI ÝÇERÝR ---

public enum RarityLevel { Common, Rare, Epic, Legendary }

public enum PassiveStatType
{
    None,
    // Temel
    MaxHealth, HpRegen, Armor, Evasion,
    // Saldýrý
    Damage, CritChance, CritDamage, AttackSpeed,
    ProjectileCount, Size, ProjectileSpeed,
    Bleed,
    CritBleed,
    Pierce, ProjectileBounce,

    // --- EKSÝK OLAN BU ---
    CritHeal, // Kritik vuruþta gelen iyileþme
    // --------------------

    // Hareket
    MoveSpeed, JumpHeight,
    // Genel
    Luck, Curse,
    MagnetRange,
    XpBonus, GoldBonus,
    DropChance,
    // Yetenek
    SkillCooldown,
    Duration,
    Revival, LifeSteal, Thorns
}

public enum WeaponStatType
{
    Damage, Cooldown, Speed, Range, ProjectileCount, Size, Duration, Pierce, ProjectileBounce
}

public enum WeaponBehaviorType { Projectile, Melee, AreaOfEffect, TargetedExplosion, Wave, ProximityMine }

public enum UpgradeCategory { WeaponUnlock, WeaponUpgrade, PassiveStat }

[System.Serializable]
public struct ItemStatModifier
{
    public PassiveStatType statType;
    public float baseAmount;
    public float amountPerLevel;
    public bool isPercentage;
}

public struct StatModification
{
    public WeaponStatType WeaponStat;
    public PassiveStatType PassiveStat;
    public float Value;
    public bool IsPercentage;
    public string Description;
}

public class GeneratedUpgradeOption
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public UpgradeData BaseUpgradeData;
    public RarityLevel RolledRarity;
    public List<StatModification> Modifications;
}