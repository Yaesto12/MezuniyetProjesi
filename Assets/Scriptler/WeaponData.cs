using UnityEngine;
using System.Collections.Generic; // List için gerekli

// Enum tanýmlarý (WeaponBehaviorType) artýk GameDefinitions.cs'de
// WeaponStatType tanýmý da GameDefinitions.cs'de

[CreateAssetMenu(fileName = "New WeaponData", menuName = "Gameplay/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string weaponName = "Yeni Silah";
    [TextArea] public string description = "Silah açýklamasý...";
    public Sprite icon;
    public WeaponBehaviorType behaviorType; // GameDefinitions.cs'den

    // --- YENÝ EKLENEN KISIM: SES AYARLARI ---
    [Header("Ses Ayarlarý")]
    [Tooltip("Bu silah ateþlendiðinde çalacak ses dosyasý.")]
    public AudioClip fireSound;

    [Range(0f, 1f)]
    [Tooltip("Sesin þiddeti (0 = Sessiz, 1 = Tam Ses).")]
    public float soundVolume = 0.5f;
    // ----------------------------------------

    [Header("Temel Statlar (Tüm Silahlar Ýçin Ortak)")]
    public float baseDamage = 10f;
    public float cooldown = 1f;
    public float critChance = 5f;
    public float critMultiplier = 2f;

    [Header("Mermi Silahý Ayarlarý (behaviorType = Projectile ise)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public int projectileCount = 1;
    public float spreadAngle = 0f;
    public float projectileScale = 1f;
    public float projectileLifetime = 5f;
    public float timeBetweenProjectiles = 0.05f;

    [Header("Yakýn Dövüþ Ayarlarý (behaviorType = Melee ise)")]
    public float attackRange = 1.5f;
    public float attackWidthOrAngle = 90f;
    public float attackDuration = 0.3f;

    [Header("Alan Etkisi Ayarlarý (behaviorType = AreaOfEffect ise)")]
    public float effectRadius = 3f;
    public float damagePerSecond = 5f;
    public float tickRate = 0.5f;
    public GameObject areaEffectPrefab;

    [Header("Hedefli Patlama Ayarlarý (behaviorType = TargetedExplosion ise)")]
    public GameObject explosionPrefab;
    public float explosionRadius = 2f;
    public int explosionCount = 1;

    [Header("Dalga Silahý Ayarlarý (behaviorType = Wave ise)")]
    public GameObject wavePrefab;
    public float waveSpeed = 15f;
    public float waveWidth = 1f;
    public float waveLifetime = 3f;
    public int wavePierceCount = 1;

    [Header("Yakýnlýk Mayýný Ayarlarý (behaviorType = ProximityMine ise)")]
    public GameObject minePrefab;
    public int mineCount = 1;
    public float mineLifetime = 3f;
    public float mineExplosionRadius = 2.5f;
    public float mineDropRadius = 1f;
    // ... Diðer türler ...


    // --- YÜKSELTME HAVUZU ---
    [Header("Yükseltme Havuzu (Bu Silah Ýçin)")]
    [Tooltip("Bu silah geliþtirildiðinde rastgele seçilebilecek olasý stat geliþtirmeleri.")]
    // Bu liste, GameDefinitions.cs'de tanýmlý olan WeaponStatType enum'unu kullanýr
    public List<WeaponStatType> availableStatUpgrades = new List<WeaponStatType>();
    // ---------------------------------
}