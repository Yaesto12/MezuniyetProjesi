using UnityEngine;
using System.Collections.Generic; // Listeler ve Dictionary için
using System.Linq; // Find metodu için

// Player objesinde TargetingSystem ve PlayerInventory olmasýný zorunlu kýlalým
[RequireComponent(typeof(TargetingSystem))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerStats))] // PlayerStats da zorunlu
public class PlayerWeaponController : MonoBehaviour
{
    [Header("Genel Referanslar")]
    [Tooltip("Mermilerin/Saldýrýlarýn baþlayacaðý nokta (Player prefab'ýnýn alt objesi).")]
    [SerializeField] private Transform firePoint;
    [Tooltip("Silahlarýn hasar vereceði düþman katmaný.")]
    [SerializeField] private LayerMask enemyLayer;

    // --- Dahili Referanslar (Awake içinde bulunur) ---
    private TargetingSystem targetingSystem;
    private PlayerInventory playerInventory;
    private PlayerStats playerStats; // <<<--- EKLENDÝ ---<<<

    // --- Yönetilen Silahlar ---
    // Aktif silah Handler script'lerini tutar
    private List<MonoBehaviour> activeWeaponHandlers = new List<MonoBehaviour>();
    // Hangi Handler'ýn hangi (klonlanmýþ) WeaponData'yý kullandýðýný eþleþtirir
    private Dictionary<MonoBehaviour, WeaponData> handlerDataMap = new Dictionary<MonoBehaviour, WeaponData>();

    void Awake()
    {
        // Gerekli bileþenleri al (RequireComponent sayesinde varlýklarý garantidir)
        targetingSystem = GetComponent<TargetingSystem>();
        playerInventory = GetComponent<PlayerInventory>();
        playerStats = GetComponent<PlayerStats>(); // <<<--- PlayerStats referansýný al ---<<<

        // FirePoint'i kontrol et (Bu manuel atanmalý veya bulunmalý)
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint"); // Çocuk objelerde ara
            if (firePoint == null)
            {
                Debug.LogError("PlayerWeaponController: FirePoint atanmamýþ ve bulunamadý!", this);
            }
        }
        // EnemyLayer atanmamýþsa uyarý ver
        if (enemyLayer == 0) // LayerMask 0 ise atanmamýþ demektir
        {
            Debug.LogWarning("PlayerWeaponController: Enemy Layer atanmamýþ! Melee, AoE, Mine, Explosion hasar vermeyebilir.", this);
            // Alternatif: Varsayýlan bir katman ata
            // enemyLayer = LayerMask.GetMask("Enemy"); // Eðer "Enemy" katmanýnýz varsa
        }
        if (playerStats == null) Debug.LogError("FATAL ERROR: PlayerStats bulunamadý!", gameObject); // Ekstra kontrol
    }

    void Start()
    {
        // Oyun sahnesi baþladýðýnda, seçilen karakterin baþlangýç silahýný kur
        SetupStartingWeapon();
    }

    /// <summary>
    /// GameData'dan seçilen karakterin baþlangýç silahýný yükler ve uygun Handler'ý baþlatýr.
    /// </summary>
    private void SetupStartingWeapon()
    {
        // Karakter seçiminden gelen veriyi al
        CharacterData selectedCharData = GameData.SelectedCharacterDataForGame;

        if (selectedCharData != null && selectedCharData.startingWeaponData != null)
        {
            Debug.Log($"Baþlangýç silahý kuruluyor: {selectedCharData.startingWeaponData.weaponName}");
            // Baþlangýç silahýný klonla
            WeaponData runtimeWeaponData = Instantiate(selectedCharData.startingWeaponData);
            // Klonlanmýþ veriyi kullanarak silahý ekle ve baþlat
            AddAndInitializeWeapon(runtimeWeaponData);

            // Baþlangýç silahýný (orijinal asset) envantere ekle
            if (playerInventory != null)
            {
                if (!playerInventory.HasWeaponData(selectedCharData.startingWeaponData))
                {
                    playerInventory.AddWeaponData(selectedCharData.startingWeaponData);
                }
            }
        }
        else
        {
            Debug.LogWarning("PlayerWeaponController: GameData'da seçili karakter veya baþlangýç silahý verisi bulunamadý!");
        }
    }

    /// <summary>
    /// Verilen (genellikle klonlanmýþ) WeaponData'ya göre uygun Handler'ý Player'a ekler ve baþlatýr.
    /// </summary>
    public void AddAndInitializeWeapon(WeaponData weaponDataInstance) // Klonlanmýþ data'yý alýr
    {
        if (weaponDataInstance == null || playerStats == null) // PlayerStats kontrolü eklendi
        {
            Debug.LogError($"AddAndInitializeWeapon: Geçersiz WeaponData ({weaponDataInstance == null}) veya PlayerStats ({playerStats == null})!", this);
            return;
        }

        MonoBehaviour weaponHandler = null;
        System.Type handlerType = null;

        // WeaponData'daki türe göre doðru Handler script'ini belirle
        switch (weaponDataInstance.behaviorType)
        {
            case WeaponBehaviorType.Projectile: handlerType = typeof(ProjectileWeaponHandler); break;
            case WeaponBehaviorType.Melee: handlerType = typeof(MeleeWeaponHandler); break;
            case WeaponBehaviorType.AreaOfEffect: handlerType = typeof(AreaEffectWeaponHandler); break;
            case WeaponBehaviorType.TargetedExplosion: handlerType = typeof(TargetedExplosionHandler); break;
            case WeaponBehaviorType.Wave: handlerType = typeof(WaveWeaponHandler); break;
            case WeaponBehaviorType.ProximityMine: handlerType = typeof(ProximityMineHandler); break;
            default:
                Debug.LogError($"Bilinmeyen silah türü: {weaponDataInstance.behaviorType}");
                return;
        }

        // Bu türde bir Handler zaten Player üzerinde var mý?
        weaponHandler = (MonoBehaviour)GetComponent(handlerType);

        if (weaponHandler == null) // Yoksa ekle
        {
            weaponHandler = (MonoBehaviour)gameObject.AddComponent(handlerType);
            Debug.Log($"{handlerType.Name} bileþeni Player'a eklendi.");

            // Eklenen Handler'ý baþlat (Initialize metodu ile ve PlayerStats göndererek)
            bool initialized = false;
            switch (weaponDataInstance.behaviorType)
            {
                case WeaponBehaviorType.Projectile:
                    ((ProjectileWeaponHandler)weaponHandler).Initialize(weaponDataInstance, firePoint, targetingSystem, playerStats); // playerStats eklendi
                    initialized = true;
                    break;
                case WeaponBehaviorType.Melee:
                    ((MeleeWeaponHandler)weaponHandler).Initialize(weaponDataInstance, enemyLayer, playerStats); // playerStats eklendi
                    initialized = true;
                    break;
                case WeaponBehaviorType.AreaOfEffect:
                    ((AreaEffectWeaponHandler)weaponHandler).Initialize(weaponDataInstance, enemyLayer, playerStats); // playerStats eklendi
                    initialized = true;
                    break;
                case WeaponBehaviorType.TargetedExplosion:
                    ((TargetedExplosionHandler)weaponHandler).Initialize(weaponDataInstance, targetingSystem, playerStats); // playerStats eklendi
                    initialized = true;
                    break;
                case WeaponBehaviorType.Wave:
                    ((WaveWeaponHandler)weaponHandler).Initialize(weaponDataInstance, firePoint, targetingSystem, playerStats); // playerStats eklendi
                    initialized = true;
                    break;
                case WeaponBehaviorType.ProximityMine:
                    ((ProximityMineHandler)weaponHandler).Initialize(weaponDataInstance, targetingSystem, playerStats); // playerStats eklendi
                    initialized = true;
                    break;
                    // Yeni Handler'lar için Initialize çaðrýlarýný ekle...
            }

            if (initialized)
            {
                activeWeaponHandlers.Add(weaponHandler);
                handlerDataMap.Add(weaponHandler, weaponDataInstance);
                Debug.Log($"{weaponDataInstance.weaponName} silahý baþlatýldý.");
            }
            else
            {
                Debug.LogError($"{handlerType.Name} eklendi ama Initialize edilemedi!");
                Destroy(weaponHandler);
            }
        }
        else
        {
            Debug.LogWarning($"{handlerType.Name} zaten mevcut. Tekrar eklenmedi veya baþlatýlmadý.");
        }
    }

    /// <summary>
    /// Belirtilen Orijinal WeaponData asset'ine karþýlýk gelen AKTÝF Handler script'ini bulur.
    /// </summary>
    public MonoBehaviour GetActiveWeaponHandler(WeaponData originalWeaponData)
    {
        if (originalWeaponData == null) return null;
        foreach (var kvp in handlerDataMap)
        {
            if (kvp.Value != null && kvp.Value.name.StartsWith(originalWeaponData.name))
            {
                if (kvp.Key != null && kvp.Key.enabled && kvp.Key.gameObject == this.gameObject)
                {
                    return kvp.Key;
                }
            }
        }
        return null;
    }

    // Oyun Bittiðinde Handler'larý Temizleme
    void OnDestroy()
    {
        foreach (var handler in activeWeaponHandlers)
        {
            if (handler != null) Destroy(handler);
        }
        activeWeaponHandlers.Clear();
        handlerDataMap.Clear();
    }
}