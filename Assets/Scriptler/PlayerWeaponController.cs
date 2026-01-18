using UnityEngine;
using System.Collections.Generic;

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
            // Silahý ekle (Bu fonksiyon zaten klonlama yapýyor)
            AddWeapon(selectedCharData.startingWeaponData);

            // Envantere de kaydet (Görünürlük için)
            if (playerInventory != null && !playerInventory.HasWeaponData(selectedCharData.startingWeaponData))
            {
                // NOT: PlayerInventory'ye eklerken sonsuz döngüye girmemesi için
                // oradaki AddWeaponData fonksiyonunda WeaponController'a haber verme kýsmýný
                // "HasWeaponData" kontrolü ile engellediðinden emin olmalýsýn.
                // Þimdilik burayý pasif býrakýyorum, Inventory kendi halletsin.
                // playerInventory.AddWeaponData(selectedCharData.startingWeaponData);
            }
        }
    }

    // ========================================================================
    // --- PLAYER INVENTORY ÝLE ÝLETÝÞÝM ---
    // ========================================================================

    public void AddWeapon(WeaponData weaponData)
    {
        if (weaponData == null) return;

        Debug.Log($"WeaponController: {weaponData.name} ekleniyor...");

        // 1. Orijinal veriyi klonla
        WeaponData runtimeInstance = Instantiate(weaponData);

        // 2. "(Clone)" ekini sil ki isim kontrolleri bozulmasýn
        runtimeInstance.name = weaponData.name;

        // 3. Baþlat
        AddAndInitializeWeapon(runtimeInstance);
    }

    public void LevelUpWeapon(WeaponData weaponData)
    {
        MonoBehaviour handler = GetActiveWeaponHandler(weaponData);
        if (handler != null)
        {
            handler.SendMessage("OnLevelUp", SendMessageOptions.DontRequireReceiver);
            Debug.Log($"{weaponData.weaponName} seviye atladý!");
        }
        else
        {
            Debug.LogWarning($"LevelUpWeapon Hatasý: {weaponData.name} için aktif handler bulunamadý!");
        }
    }

    // ========================================================================

    private void AddAndInitializeWeapon(WeaponData weaponDataInstance)
    {
        if (weaponDataInstance == null || playerStats == null) return;

        // Zaten var mý kontrolü
        foreach (var pair in handlerDataMap)
        {
            // Ýsimleri eþlerken artýk (Clone) sorunu olmayacak
            if (pair.Value.name == weaponDataInstance.name)
            {
                Debug.LogWarning($"{weaponDataInstance.name} zaten ekli, tekrar eklenmedi.");
                return;
            }
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

        // Component'i ekle
        MonoBehaviour weaponHandler = (MonoBehaviour)gameObject.AddComponent(handlerType);

        bool initialized = false;

        // Initialize çaðrýlarý
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
            Debug.Log($"<color=cyan>SÝLAH AKTÝF EDÝLDÝ: {weaponDataInstance.name}</color>");
        }
        else
        {
            Debug.LogError($"SÝLAH BAÞLATILAMADI: {weaponDataInstance.name}");
            Destroy(weaponHandler);
        }
    }

    public MonoBehaviour GetActiveWeaponHandler(WeaponData originalWeaponData)
    {
        if (originalWeaponData == null) return null;
        string searchName = originalWeaponData.name;

        foreach (var kvp in handlerDataMap)
        {
            if (kvp.Value != null && kvp.Value.name == searchName)
            {
                return kvp.Key;
            }
        }
        return null;
    }

    void OnDestroy()
    {
        activeWeaponHandlers.Clear();
        handlerDataMap.Clear();
    }
}