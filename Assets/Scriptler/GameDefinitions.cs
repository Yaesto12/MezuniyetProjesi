using UnityEngine;
using System.Collections.Generic;

// ============================================================================
// --- ENUMLAR (SEÇENEK LÝSTELERÝ) ---
// ============================================================================

public enum PassiveStatType
{
    None = 0,

    // --- Temel ---
    MaxHealth,
    HpRegen,
    Armor,
    Evasion,
    LifeSteal,
    Thorns,

    // --- Hareket ---
    MoveSpeed,
    ExtraJumps,
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
    AreaSize,
    Duration,
    CooldownReduction,

    // --- Ekonomi ve Þans ---
    Luck,
    MagnetRange,
    XpBonus,
    GoldBonus,
    Curse,
    DropChance,

    // --- Meta ---
    Revival,
    Reroll,
    Skip,
    Banish
}

public enum WeaponStatType
{
    Damage,
    Cooldown,
    Area,
    Speed,
    Duration,
    Amount,
    Pierce,
    Knockback,

    // --- Eksik Olanlar Eklendi ---
    ProjectileCount,
    Size,
    ProjectileBounce,
    Range
}

public enum WeaponBehaviorType
{
    Melee,
    Projectile,
    Aura,
    Directional,

    // --- Eksik Olanlar Eklendi ---
    AreaOfEffect,
    TargetedExplosion,
    Wave,
    ProximityMine
}

public enum UpgradeCategory
{
    StatUpgrade,
    WeaponUpgrade,
    NewWeapon,
    NewItem,
    Heal,

    // --- Eksik Olan Eklendi ---
    WeaponUnlock
}

public enum RarityLevel
{
    Common,
    Rare,
    Epic,
    Legendary
}

// ============================================================================
// --- YARDIMCI SINIFLAR ---
// ============================================================================

// PlayerExperience ve LevelUpUI'ýn kullandýðý yapý
[System.Serializable]
public class GeneratedUpgradeOption
{
    // Senin kodlarýn Büyük Harf (PascalCase) arýyor, o yüzden isimleri deðiþtirdim.
    public string Name;
    public string Description;
    public Sprite Icon;

    public UpgradeCategory Category; // Küçük harf hata verirse 'category' yapabilirsin ama genelde Name büyükse bu da büyüktür.

    public UpgradeData BaseUpgradeData; // Hata veren kýsým eklendi
    public RarityLevel RolledRarity;    // Hata veren kýsým eklendi

    public List<StatModification> Modifications = new List<StatModification>(); // Hata veren liste eklendi

    // Eski uyumluluk için (Eðer inventory bunlarý kullanýyorsa)
    public WeaponData weaponData;
    public ItemData itemData;
}

// Stat deðiþimlerini tutan sýnýf
[System.Serializable]
public class StatModification
{
    public WeaponStatType WeaponStat;   // Hata veren eklendi
    public PassiveStatType PassiveStat; // Hata veren eklendi
    public float Value;                 // Hata veren eklendi
    public bool IsPercentage;           // Hata veren eklendi
    public string Description;          // Hata veren eklendi
}

// ItemData'nýn kullandýðý basit yapý (Bunu deðiþtirmedim, PlayerInventory kullanýyor)
[System.Serializable]
public class ItemStatModifier
{
    public PassiveStatType statType;
    public float baseAmount;
}