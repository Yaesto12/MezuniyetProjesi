using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(TargetingSystem))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerWeaponController : MonoBehaviour
{
    [Header("Genel Referanslar")]
    [Tooltip("Mermilerin/Saldýrýlarýn baþlayacaðý nokta.")]
    [SerializeField] private Transform firePoint;
    [Tooltip("Silahlarýn hasar vereceði düþman katmaný.")]
    [SerializeField] private LayerMask enemyLayer;

    // --- Dahili Referanslar ---
    private TargetingSystem targetingSystem;
    private PlayerInventory playerInventory;
    private PlayerStats playerStats;

    // --- Yönetilen Silahlar ---
    private List<MonoBehaviour> activeWeaponHandlers = new List<MonoBehaviour>();
    private Dictionary<MonoBehaviour, WeaponData> handlerDataMap = new Dictionary<MonoBehaviour, WeaponData>();

    void Awake()
    {
        targetingSystem = GetComponent<TargetingSystem>();
        playerInventory = GetComponent<PlayerInventory>();
        playerStats = GetComponent<PlayerStats>();

        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
            if (firePoint == null) Debug.LogError("PlayerWeaponController: FirePoint bulunamadý!", this);
        }

        // Eðer EnemyLayer seçilmediyse otomatik ata (Varsayýlan olarak her þeye vurmasýn diye kontrol)
        if (enemyLayer == 0) Debug.LogWarning("PlayerWeaponController: Enemy Layer atanmamýþ!", this);
    }

    void Start()
    {
        SetupStartingWeapon();
    }

    private void SetupStartingWeapon()
    {
        CharacterData selectedCharData = GameData.SelectedCharacterDataForGame;

        if (selectedCharData != null && selectedCharData.startingWeaponData != null)
        {
            // Baþlangýç silahýný klonla ve ekle
            WeaponData runtimeWeaponData = Instantiate(selectedCharData.startingWeaponData);
            AddAndInitializeWeapon(runtimeWeaponData);

            // Envantere de kaydet (Eðer Inventory otomatik eklemiyorsa)
            if (playerInventory != null && !playerInventory.HasWeaponData(selectedCharData.startingWeaponData))
            {
                playerInventory.AddWeaponData(selectedCharData.startingWeaponData);
            }
        }
    }

    // ========================================================================
    // --- PLAYER INVENTORY ÝLE ÝLETÝÞÝM ÝÇÝN EKLENEN KISIMLAR ---
    // ========================================================================

    // PlayerInventory "SendMessage('AddWeapon')" dediðinde burasý çalýþacak
    public void AddWeapon(WeaponData weaponData)
    {
        // Envanterden gelen silah orijinaldir (Asset), onu klonlayýp oyuna dahil ediyoruz
        WeaponData runtimeInstance = Instantiate(weaponData);
        AddAndInitializeWeapon(runtimeInstance);
    }

    // PlayerInventory "SendMessage('LevelUpWeapon')" dediðinde burasý çalýþacak
    public void LevelUpWeapon(WeaponData weaponData)
    {
        // Aktif handler'ý bul
        MonoBehaviour handler = GetActiveWeaponHandler(weaponData);
        if (handler != null)
        {
            // Handler'a "Seviye Atladýn, deðerlerini güncelle" mesajý gönderiyoruz
            // Not: Handler scriptlerinde "OnLevelUp" fonksiyonu olmalý
            handler.SendMessage("OnLevelUp", SendMessageOptions.DontRequireReceiver);
            Debug.Log($"{weaponData.weaponName} seviye atladý!");
        }
    }

    // ========================================================================

    public void AddAndInitializeWeapon(WeaponData weaponDataInstance)
    {
        if (weaponDataInstance == null || playerStats == null) return;

        // Ayný silahtan zaten varsa tekrar ekleme (Level atlatma mantýðý ayrýdýr)
        // Not: Bazý oyunlarda ayný silahtan 2 tane olabilir, öyleyse bu kontrolü kaldýr.
        foreach (var pair in handlerDataMap)
        {
            if (pair.Value.name == weaponDataInstance.name) return;
        }

        System.Type handlerType = null;

        switch (weaponDataInstance.behaviorType)
        {
            case WeaponBehaviorType.Projectile: handlerType = typeof(ProjectileWeaponHandler); break;
            case WeaponBehaviorType.Melee: handlerType = typeof(MeleeWeaponHandler); break;
            case WeaponBehaviorType.AreaOfEffect: handlerType = typeof(AreaEffectWeaponHandler); break;
            case WeaponBehaviorType.TargetedExplosion: handlerType = typeof(TargetedExplosionHandler); break;
            case WeaponBehaviorType.Wave: handlerType = typeof(WaveWeaponHandler); break;
            case WeaponBehaviorType.ProximityMine: handlerType = typeof(ProximityMineHandler); break;
            default: Debug.LogError($"Bilinmeyen silah türü: {weaponDataInstance.behaviorType}"); return;
        }

        MonoBehaviour weaponHandler = (MonoBehaviour)gameObject.AddComponent(handlerType);

        bool initialized = false;
        // Burada Initialize çaðrýlarýný yapýyoruz. Handler scriptlerinde bu metodun imzasý
        // (WeaponData, Transform/TargetingSystem, PlayerStats) þeklinde olmalý.
        switch (weaponDataInstance.behaviorType)
        {
            case WeaponBehaviorType.Projectile:
                ((ProjectileWeaponHandler)weaponHandler).Initialize(weaponDataInstance, firePoint, targetingSystem, playerStats);
                initialized = true;
                break;
            case WeaponBehaviorType.Melee:
                ((MeleeWeaponHandler)weaponHandler).Initialize(weaponDataInstance, enemyLayer, playerStats);
                initialized = true;
                break;
            case WeaponBehaviorType.AreaOfEffect:
                ((AreaEffectWeaponHandler)weaponHandler).Initialize(weaponDataInstance, enemyLayer, playerStats);
                initialized = true;
                break;
            case WeaponBehaviorType.TargetedExplosion:
                ((TargetedExplosionHandler)weaponHandler).Initialize(weaponDataInstance, targetingSystem, playerStats);
                initialized = true;
                break;
            case WeaponBehaviorType.Wave:
                ((WaveWeaponHandler)weaponHandler).Initialize(weaponDataInstance, firePoint, targetingSystem, playerStats);
                initialized = true;
                break;
            case WeaponBehaviorType.ProximityMine:
                ((ProximityMineHandler)weaponHandler).Initialize(weaponDataInstance, targetingSystem, playerStats);
                initialized = true;
                break;
        }

        if (initialized)
        {
            activeWeaponHandlers.Add(weaponHandler);
            handlerDataMap.Add(weaponHandler, weaponDataInstance);
        }
        else
        {
            Destroy(weaponHandler);
        }
    }

    public MonoBehaviour GetActiveWeaponHandler(WeaponData originalWeaponData)
    {
        if (originalWeaponData == null) return null;

        // Ýsim kontrolü ile (Clone) ekini görmezden gelerek eþleþtirme yapýyoruz
        string searchName = originalWeaponData.name;

        foreach (var kvp in handlerDataMap)
        {
            // Klonlanan verinin adý "Sword(Clone)" olabilir, orijinali "Sword"
            if (kvp.Value != null && kvp.Value.name.Contains(searchName))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    void OnDestroy()
    {
        // Handler'lar zaten Player'ýn üzerinde component olduðu için Player yok olunca onlar da gider.
        // Ama listeleri temizlemek iyidir.
        activeWeaponHandlers.Clear();
        handlerDataMap.Clear();
    }
}