using UnityEngine;
using System.Collections.Generic; // List için

// --- BU DOSYA TÜM GLOBAL TANIMLAMALARI ÝÇERÝR ---

// Item ve Yükseltmelerin nadirlik seviyesi
public enum RarityLevel { Common, Rare, Epic, Legendary }

// Pasif stat türleri (PlayerStats, ItemData ve UpgradeData kullanýr)
public enum PassiveStatType
{
    None,
    MaxHealth,
    HpRegen,
    Armor,
    Evasion,
    Damage, // Genel Hasar Çarpaný
    CritChance,
    CritDamage,
    AttackSpeed, // Genel Saldýrý Hýzý Çarpaný
    ProjectileCount, // Genel Ekstra Mermi
    Size, // Genel Alan Çarpaný
    MoveSpeed,
    Luck,
    MagnetRange,
    XpBonus,
    GoldBonus,
    Duration, // Genel Süre Çarpaný
    Revival,
    LifeSteal,
    Thorns,
    // --- EKSÝK STATLAR BURAYA EKLENDÝ ---
    ProjectileBounce, // Mermi Sekmesi
    Pierce // Mermi Delmesi
    // ---------------------------------
}

// Silah stat türleri (WeaponData ve PlayerExperience kullanýr)
public enum WeaponStatType
{
    Damage,
    Cooldown,
    Speed,
    Range,
    ProjectileCount,
    Size,
    Duration,
    Pierce,
    ProjectileBounce
    // Ýhtiyaca göre ekleyin...
}

// Silahýn temel çalýþma þekli
public enum WeaponBehaviorType { Projectile, Melee, AreaOfEffect, TargetedExplosion, Wave, ProximityMine }

// Yükseltmenin kategorisi
public enum UpgradeCategory { WeaponUnlock, WeaponUpgrade, PassiveStat } // PlayerExperience.cs bunu kullanmayý býraktý ama ileride pasifler için lazým olabilir

// --- DÝNAMÝK YÜKSELTME SEÇENEÐÝ ÝÇÝN YARDIMCI SINIFLAR ---

/// <summary>
/// Bir stat deðiþikliðini tanýmlayan yapý.
/// </summary>
public struct StatModification
{
    public WeaponStatType WeaponStat;
    public PassiveStatType PassiveStat; // Artýk bu da kullanýlýyor
    public float Value;
    public bool IsPercentage;
    public string Description;
}

/// <summary>
/// PlayerExperience tarafýndan dinamik olarak oluþturulup LevelUpUI'a gönderilecek seçenek verisi.
/// </summary>
public class GeneratedUpgradeOption
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public UpgradeData BaseUpgradeData;
    public RarityLevel RolledRarity;
    public List<StatModification> Modifications;
}