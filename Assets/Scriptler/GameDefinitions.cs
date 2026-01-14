using UnityEngine;
using System.Collections.Generic; // List için

// --- BU DOSYA TÜM GLOBAL TANIMLAMALARI ÝÇERÝR ---

public enum RarityLevel { Common, Rare, Epic, Legendary }

public enum PassiveStatType
{
    // --- Temel ---
    MaxHealth,
    HpRegen,
    Armor,
    Evasion,      // Hata veriyordu, eklendi
    LifeSteal,
    Thorns,

    // --- Hareket ---
    MoveSpeed,
    ExtraJumps,   // Hata veriyordu, eklendi
    JumpHeight,

    // --- Saldýrý ---
    Damage,
    AttackSpeed,
    CritChance,
    CritDamage,

    // --- Mermi / Alan ---
    ProjectileCount,
    ProjectileSpeed,
    ProjectileBounce,
    Pierce,
    AreaSize,         // Hata veriyordu, eklendi
    Duration,
    CooldownReduction, // Hata veriyordu, eklendi

    // --- Ekonomi ve Þans ---
    Luck,
    MagnetRange,
    XpBonus,
    GoldBonus,
    Curse,
    DropChance,

    // --- Meta / Oyun Ýçi ---
    Revival,
    Reroll,   // Hata veriyordu, eklendi
    Skip,     // Hata veriyordu, eklendi
    Banish    // Hata veriyordu, eklendi
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