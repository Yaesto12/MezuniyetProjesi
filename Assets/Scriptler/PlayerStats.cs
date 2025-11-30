using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    // --- TEMEL STATLAR ---
    #region Base Stats
    [Header("Savunma Statlarý")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseHpRegen = 0f;
    [SerializeField] private float baseMaxOverheal = 0f;
    [SerializeField] private float baseArmor = 0f;
    [SerializeField] private float baseMaxShield = 0f;
    [SerializeField][Range(0f, 90f)] private float baseEvasion = 0f;
    [Header("Yaþam ve Etkileþim Statlarý")]
    [SerializeField][Range(0f, 300f)] private float baseLifeSteal = 0f;
    [SerializeField] private float baseThorns = 0f;
    [Header("Saldýrý Statlarý")]
    [SerializeField] private float baseDamageMultiplier = 100f;
    [SerializeField][Range(0f, 100f)] private float baseCritChance = 5f;
    [SerializeField] private float baseCritDamage = 200f;
    [SerializeField] private float baseAttackSpeedMultiplier = 100f;
    [SerializeField] private int baseProjectileCountBonus = 0;
    [SerializeField] private int baseProjectileBounce = 0;
    [SerializeField] private float basePierce = 0f;
    [Header("Alan ve Süre Statlarý")]
    [SerializeField] private float baseSizeMultiplier = 100f;
    [SerializeField] private float baseDurationMultiplier = 100f;
    [Header("Düþman Tipi Hasar Çarpanlarý (%)")]
    [SerializeField] private float baseDamageToMobs = 100f;
    [SerializeField] private float baseDamageToElites = 100f;
    [SerializeField] private float baseDamageToMiniBosses = 100f;
    [SerializeField] private float baseDamageToBosses = 100f;
    [Header("Hareket Statlarý")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private int baseExtraJumps = 1;
    [SerializeField] private float baseJumpHeightMultiplier = 100f;
    [Header("Genel Statlar")]
    [SerializeField] private float baseLuck = 0f;
    [SerializeField] private float baseCurse = 0f;
    [SerializeField] private float basePickupRange = 1.5f;
    [SerializeField] private float baseMagnetRange = 8f;
    [SerializeField] private float baseXpBonus = 100f;
    [SerializeField] private float baseGoldBonus = 100f;
    [SerializeField] private float baseEliteSpawnChanceBonus = 0f;
    [SerializeField][Range(0f, 1000f)] private float baseDropChanceBonus = 0f;
    [Header("Yetenek ve Diðer")]
    [SerializeField] private float baseSkillCooldownReduction = 0f;
    [SerializeField] private int baseRevivals = 0;
    [SerializeField] private int baseRerolls = 0;
    [SerializeField] private int baseSkips = 0;
    [SerializeField] private int baseBanishes = 0;
    #endregion

    // --- ANLIK HESAPLANAN STATLAR ---
    #region Current Stats
    [field: SerializeField] public float CurrentMaxHealth { get; private set; }
    [field: SerializeField] public float CurrentHpRegen { get; private set; }
    [field: SerializeField] public float CurrentMaxOverheal { get; private set; }
    [field: SerializeField] public float CurrentArmor { get; private set; }
    [field: SerializeField] public float CurrentMaxShield { get; private set; }
    [field: SerializeField] public float CurrentEvasion { get; private set; }
    [field: SerializeField] public float CurrentLifeSteal { get; private set; }
    [field: SerializeField] public float CurrentThorns { get; private set; }
    [field: SerializeField] public float CurrentDamageMultiplier { get; private set; }
    [field: SerializeField] public float CurrentCritChance { get; private set; }
    [field: SerializeField] public float CurrentCritDamage { get; private set; }
    [field: SerializeField] public float CurrentAttackSpeedMultiplier { get; private set; }
    [field: SerializeField] public int CurrentProjectileCountBonus { get; private set; }
    [field: SerializeField] public int CurrentProjectileBounce { get; private set; }
    [field: SerializeField] public float CurrentPierce { get; private set; }
    [field: SerializeField] public float CurrentSizeMultiplier { get; private set; }
    [field: SerializeField] public float CurrentDurationMultiplier { get; private set; }
    [field: SerializeField] public float CurrentDamageToMobs { get; private set; }
    [field: SerializeField] public float CurrentDamageToElites { get; private set; }
    [field: SerializeField] public float CurrentDamageToMiniBosses { get; private set; }
    [field: SerializeField] public float CurrentDamageToBosses { get; private set; }
    [field: SerializeField] public float CurrentMoveSpeed { get; private set; }
    [field: SerializeField] public int CurrentExtraJumps { get; private set; }
    [field: SerializeField] public float CurrentJumpHeightMultiplier { get; private set; }
    [field: SerializeField] public float CurrentLuck { get; private set; }
    [field: SerializeField] public float CurrentCurse { get; private set; }
    [field: SerializeField] public float CurrentPickupRange { get; private set; }
    [field: SerializeField] public float CurrentMagnetRange { get; private set; }
    [field: SerializeField] public float CurrentXpBonus { get; private set; }
    [field: SerializeField] public float CurrentGoldBonus { get; private set; }
    [field: SerializeField] public float CurrentEliteSpawnChanceBonus { get; private set; }
    [field: SerializeField] public float CurrentDropChanceBonus { get; private set; }
    [field: SerializeField] public float CurrentSkillCooldownReduction { get; private set; }
    [field: SerializeField] public int CurrentRevivals { get; private set; }
    [field: SerializeField] public int CurrentRerolls { get; private set; }
    [field: SerializeField] public int CurrentSkips { get; private set; }
    [field: SerializeField] public int CurrentBanishes { get; private set; }
    #endregion

    private PlayerInventory playerInventory;

    void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
        if (playerInventory == null) Debug.LogError("[PlayerStats] PlayerInventory bileþeni bulunamadý!", this);
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        // 1. Temel deðerleri ata
        CurrentMaxHealth = baseMaxHealth;
        CurrentMoveSpeed = baseMoveSpeed;
        CurrentHpRegen = baseHpRegen;
        CurrentMaxOverheal = baseMaxOverheal;
        CurrentArmor = baseArmor;
        CurrentMaxShield = baseMaxShield;
        CurrentEvasion = baseEvasion;
        CurrentLifeSteal = baseLifeSteal;
        CurrentThorns = baseThorns;
        CurrentDamageMultiplier = baseDamageMultiplier;
        CurrentCritChance = baseCritChance;
        CurrentCritDamage = baseCritDamage;
        CurrentAttackSpeedMultiplier = baseAttackSpeedMultiplier;
        CurrentProjectileCountBonus = baseProjectileCountBonus;
        CurrentProjectileBounce = baseProjectileBounce;
        CurrentPierce = basePierce;
        CurrentSizeMultiplier = baseSizeMultiplier;
        CurrentDurationMultiplier = baseDurationMultiplier;
        CurrentDamageToMobs = baseDamageToMobs;
        CurrentDamageToElites = baseDamageToElites;
        CurrentDamageToMiniBosses = baseDamageToMiniBosses;
        CurrentDamageToBosses = baseDamageToBosses;
        CurrentExtraJumps = baseExtraJumps;
        CurrentJumpHeightMultiplier = baseJumpHeightMultiplier;
        CurrentLuck = baseLuck;
        CurrentCurse = baseCurse;
        CurrentPickupRange = basePickupRange;
        CurrentMagnetRange = baseMagnetRange;
        CurrentXpBonus = baseXpBonus;
        CurrentGoldBonus = baseGoldBonus;
        CurrentEliteSpawnChanceBonus = baseEliteSpawnChanceBonus;
        CurrentDropChanceBonus = baseDropChanceBonus;
        CurrentSkillCooldownReduction = baseSkillCooldownReduction;
        CurrentRevivals = baseRevivals;
        CurrentRerolls = baseRerolls;
        CurrentSkips = baseSkips;
        CurrentBanishes = baseBanishes;

        // Debug.Log($"[RecalculateStats] Statlar temele sýfýrlandý. (Hýz = {CurrentMoveSpeed}, Can = {CurrentMaxHealth})");

        // 2. Sahip olunan Pasif Item'larýn bonuslarýný uygula
        if (playerInventory != null && playerInventory.ownedItems != null && playerInventory.ownedItems.Count > 0)
        {
            // Debug.Log($"[RecalculateStats] Envanterde {playerInventory.ownedItems.Count} çeþit item bulundu. Bonuslar uygulanýyor...");
            foreach (KeyValuePair<ItemData, int> itemPair in playerInventory.ownedItems)
            {
                ItemData item = itemPair.Key;
                int level = itemPair.Value;
                if (item == null) continue;

                // Debug.Log($" > Item Ýþleniyor: {item.itemName} (Seviye {level})");

                float totalValue = 0;
                if (item.isStackable) { totalValue = item.effectValue + (item.valuePerLevel * (level - 1)); }
                else { totalValue = item.effectValue; }

                // Debug.Log($"   > Uygulanacak Tür: {item.effectType}, Hesaplanan Deðer: {totalValue}, Yüzde: {item.isPercentageBased}");

                ApplyItemBonus(item.effectType, totalValue, item.isPercentageBased);
            }
        }
        // else { Debug.Log("[RecalculateStats] Envanterde uygulanacak item yok."); }

        // 3. TODO: Pasif Yetenekler ve Pasif Yükseltmeler

        // 4. Deðerleri Sýnýrla ve Özel Mantýklarý Uygula
        if (CurrentCritChance > 100f) { float excessCrit = CurrentCritChance - 100f; CurrentCritDamage += excessCrit; CurrentCritChance = 100f; }
        CurrentCritChance = Mathf.Clamp(CurrentCritChance, 0f, 100f);
        CurrentEvasion = Mathf.Clamp(CurrentEvasion, 0f, 90f);
        CurrentAttackSpeedMultiplier = Mathf.Max(10f, CurrentAttackSpeedMultiplier);
        CurrentSkillCooldownReduction = Mathf.Clamp(CurrentSkillCooldownReduction, -Mathf.Infinity, 95f);
        CurrentLifeSteal = Mathf.Clamp(CurrentLifeSteal, 0f, 300f);
        CurrentDropChanceBonus = Mathf.Clamp(CurrentDropChanceBonus, 0f, 1000f);
        CurrentMaxHealth = Mathf.Max(1f, CurrentMaxHealth);

        Debug.Log($"[RecalculateStats] Hesaplama Bitti. SON DEÐERLER (Hýz = {CurrentMoveSpeed}, Can = {CurrentMaxHealth})");
        // Debug.LogWarning("--- RecalculateStats Bitti ---");

        // 5. Diðer sistemleri uyar
        NotifyOtherSystems();
    }


    // --- BU FONKSÝYON DÜZELTÝLDÝ (ItemEffectType -> PassiveStatType) ---
    /// <summary>
    /// Item'lardan veya diðer kaynaklardan gelen bonuslarý anlýk statlara ekler/çarpar.
    /// </summary>
    private void ApplyItemBonus(PassiveStatType type, float value, bool isPercentage) // <<<--- TÜR DEÐÝÞTÝ ---<<<
    {
        // Debug.Log($"   >> ApplyItemBonus çaðrýldý: Tür={type}, Deðer={value}");
        switch (type) // <<<--- SWITCH ARTIK PassiveStatType KULLANIYOR ---<<<
        {
            case PassiveStatType.MaxHealth:
                CurrentMaxHealth += isPercentage ? (baseMaxHealth * value / 100f) : value;
                break;
            case PassiveStatType.MoveSpeed:
                CurrentMoveSpeed += isPercentage ? (baseMoveSpeed * value / 100f) : value;
                break;

            case PassiveStatType.HpRegen: CurrentHpRegen += value; break;
            case PassiveStatType.Armor: CurrentArmor += value; break;
            case PassiveStatType.Evasion: CurrentEvasion += value; break;
            case PassiveStatType.Damage: CurrentDamageMultiplier += value; break;
            case PassiveStatType.CritChance: CurrentCritChance += value; break;
            case PassiveStatType.CritDamage: CurrentCritDamage += value; break;
            case PassiveStatType.AttackSpeed: CurrentAttackSpeedMultiplier += value; break;
            case PassiveStatType.ProjectileCount: CurrentProjectileCountBonus += (int)value; break;
            case PassiveStatType.Size: CurrentSizeMultiplier += value; break;
            case PassiveStatType.Luck: CurrentLuck += value; break;
            case PassiveStatType.MagnetRange: CurrentMagnetRange += isPercentage ? (baseMagnetRange * value / 100f) : value; break;
            case PassiveStatType.XpBonus: CurrentXpBonus += value; break;
            case PassiveStatType.GoldBonus: CurrentGoldBonus += value; break;
            case PassiveStatType.LifeSteal: CurrentLifeSteal += value; break;
            case PassiveStatType.Thorns: CurrentThorns += value; break;
            case PassiveStatType.ProjectileBounce: CurrentProjectileBounce += (int)value; break;
            case PassiveStatType.Pierce: CurrentPierce += value; break;
            case PassiveStatType.Duration: CurrentDurationMultiplier += value; break;
            case PassiveStatType.Revival: CurrentRevivals += (int)value; break;

            case PassiveStatType.None: // Hiçbir þey yapma
                break;
            default:
                Debug.LogWarning($"     >> Bilinmeyen item etki türü: {type}");
                break;
        }
    }
    // ------------------------------------


    private void NotifyOtherSystems()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.UpdateMaxValues(CurrentMaxHealth, CurrentMaxShield);

        XpCollector collector = GetComponentInChildren<XpCollector>();
        if (collector != null) collector.UpdateMagnetRadius();
    }

    // --- TEMEL STATLARI ARTIRAN METOTLAR ---
    public void IncreaseBaseMaxHealth(float amount) { baseMaxHealth += amount; RecalculateStats(); }
    public void IncreaseBaseHpRegen(float amount) { baseHpRegen += amount; RecalculateStats(); }
    public void IncreaseBaseMaxOverheal(float amount) { baseMaxOverheal += amount; RecalculateStats(); }
    public void IncreaseBaseArmor(float amount) { baseArmor += amount; RecalculateStats(); }
    public void IncreaseBaseMaxShield(float amount) { baseMaxShield += amount; RecalculateStats(); }
    public void IncreaseBaseEvasion(float amount) { baseEvasion += amount; RecalculateStats(); }
    public void IncreaseBaseLifeSteal(float amount) { baseLifeSteal += amount; RecalculateStats(); }
    public void IncreaseBaseThorns(float amount) { baseThorns += amount; RecalculateStats(); }
    public void IncreaseBaseDamageMultiplier(float percentageAmount) { baseDamageMultiplier += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseCritChance(float amount) { baseCritChance += amount; RecalculateStats(); }
    public void IncreaseBaseCritDamage(float percentageAmount) { baseCritDamage += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseAttackSpeedMultiplier(float percentageAmount) { baseAttackSpeedMultiplier += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseProjectileCountBonus(int amount) { baseProjectileCountBonus += amount; RecalculateStats(); }
    public void IncreaseBaseProjectileBounce(int amount) { baseProjectileBounce += amount; RecalculateStats(); }
    public void IncreaseBasePierce(float amount) { basePierce += amount; RecalculateStats(); }
    public void IncreaseBaseSizeMultiplier(float percentageAmount) { baseSizeMultiplier += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseDurationMultiplier(float percentageAmount) { baseDurationMultiplier += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseDamageToMobs(float percentageAmount) { baseDamageToMobs += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseDamageToElites(float percentageAmount) { baseDamageToElites += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseDamageToMiniBosses(float percentageAmount) { baseDamageToMiniBosses += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseDamageToBosses(float percentageAmount) { baseDamageToBosses += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseMoveSpeed(float amount) { baseMoveSpeed += amount; RecalculateStats(); }
    public void IncreaseBaseExtraJumps(int amount) { baseExtraJumps += amount; RecalculateStats(); }
    public void IncreaseBaseJumpHeightMultiplier(float percentageAmount) { baseJumpHeightMultiplier += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseLuck(float amount) { baseLuck += amount; RecalculateStats(); }
    public void IncreaseBaseCurse(float amount) { baseCurse += amount; RecalculateStats(); }
    public void IncreaseBasePickupRange(float amount) { basePickupRange += amount; RecalculateStats(); }
    public void IncreaseBaseMagnetRange(float amount) { baseMagnetRange += amount; RecalculateStats(); NotifyOtherSystems(); }
    public void IncreaseBaseXpBonus(float percentageAmount) { baseXpBonus += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseGoldBonus(float percentageAmount) { baseGoldBonus += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseEliteSpawnChanceBonus(float percentageAmount) { baseEliteSpawnChanceBonus += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseDropChanceBonus(float percentageAmount) { baseDropChanceBonus += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseSkillCooldownReduction(float percentageAmount) { baseSkillCooldownReduction += percentageAmount; RecalculateStats(); }
    public void IncreaseBaseRevivals(int amount) { baseRevivals += amount; RecalculateStats(); }
    public void IncreaseBaseRerolls(int amount) { baseRerolls += amount; RecalculateStats(); }
    public void IncreaseBaseSkips(int amount) { baseSkips += amount; RecalculateStats(); }
    public void IncreaseBaseBanishes(int amount) { baseBanishes += amount; RecalculateStats(); }

    // --- HARCANABÝLÝR STATLAR ÝÇÝN AZALTMA METOTLARI ---
    public bool UseRevival() { if (CurrentRevivals > 0) { baseRevivals--; RecalculateStats(); return true; } return false; }
    public bool UseReroll() { if (CurrentRerolls > 0) { baseRerolls--; RecalculateStats(); return true; } return false; }
    public bool UseSkip() { if (CurrentSkips > 0) { baseSkips--; RecalculateStats(); return true; } return false; }
    public bool UseBanish() { if (CurrentBanishes > 0) { baseBanishes--; RecalculateStats(); return true; } return false; }
}