using UnityEngine;
using System.Collections.Generic;

public enum RarityLevel { Common, Rare, Epic, Legendary }

public enum PassiveStatType
{
    None,
    // Temel
    MaxHealth, HpRegen, Armor, Evasion,
    // Saldýrý
    Damage, CritChance, CritDamage, AttackSpeed,
    ProjectileCount, Size, ProjectileSpeed, // <-- ProjectileSpeed EKLENDÝ
    Bleed, // <-- Bleed EKLENDÝ
    CritBleed,
    Pierce, ProjectileBounce,
    // Hareket
    MoveSpeed, JumpHeight, // <-- JumpHeight EKLENDÝ
    // Genel
    Luck, Curse, // <-- Curse EKLENDÝ
    MagnetRange, // (Pickup Range yerine bunu kullanabiliriz veya ayrý yapabiliriz)
    XpBonus, GoldBonus,
    DropChance, // <-- DropChance EKLENDÝ
    // Yetenek
    SkillCooldown, // <-- SkillCooldown EKLENDÝ
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