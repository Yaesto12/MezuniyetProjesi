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
    [SerializeField] private float baseCritHeal = 0f;
    [Header("Saldýrý Statlarý")]
    [SerializeField] private float baseDamageMultiplier = 100f;
    [SerializeField][Range(0f, 100f)] private float baseCritChance = 5f;
    [SerializeField] private float baseCritDamage = 200f;
    [SerializeField] private float baseAttackSpeedMultiplier = 100f;
    [SerializeField] private float baseProjectileSpeedMultiplier = 100f;
    [SerializeField] private int baseProjectileCountBonus = 0;
    [SerializeField] private int baseProjectileBounce = 0;
    [SerializeField] private float basePierce = 0f;
    [Header("Alan ve Süre Statlarý")]
    [SerializeField] private float baseSizeMultiplier = 100f;
    [SerializeField] private float baseDurationMultiplier = 100f;
    [SerializeField] private float baseBleedPercent = 0f;
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

    [field: SerializeField] public float CurrentDamageMultiplier { get; private set; }
    [field: SerializeField] public float CurrentCritChance { get; private set; }
    [field: SerializeField] public float CurrentCritDamage { get; private set; }
    [field: SerializeField] public float CurrentAttackSpeedMultiplier { get; private set; }
    [field: SerializeField] public float CurrentProjectileSpeedMultiplier { get; private set; }
    [field: SerializeField] public int CurrentProjectileCountBonus { get; private set; }
    [field: SerializeField] public int CurrentProjectileBounce { get; private set; }
    [field: SerializeField] public float CurrentPierce { get; private set; }
    [field: SerializeField] public float CurrentSizeMultiplier { get; private set; }
    [field: SerializeField] public float CurrentDurationMultiplier { get; private set; }
    [field: SerializeField] public float CurrentBleedPercent { get; private set; }
    [field: SerializeField] public float CurrentCritBleedPercent { get; private set; }

    [field: SerializeField] public float CurrentLifeSteal { get; private set; }
    [field: SerializeField] public float CurrentThorns { get; private set; }
    [field: SerializeField] public float CurrentCritHeal { get; private set; }

    [field: SerializeField] public float CurrentMoveSpeed { get; private set; }
    [field: SerializeField] public int CurrentExtraJumps { get; private set; }
    [field: SerializeField] public float CurrentJumpHeightMultiplier { get; private set; }

    [field: SerializeField] public float CurrentLuck { get; private set; }
    [field: SerializeField] public float CurrentCurse { get; private set; }
    [field: SerializeField] public float CurrentMagnetRange { get; private set; }
    [field: SerializeField] public float CurrentXpBonus { get; private set; }
    [field: SerializeField] public float CurrentGoldBonus { get; private set; }
    [field: SerializeField] public float CurrentDropChanceBonus { get; private set; }
    [field: SerializeField] public float CurrentEliteSpawnChanceBonus { get; private set; }

    [field: SerializeField] public float CurrentSkillCooldownReduction { get; private set; }
    [field: SerializeField] public int CurrentRevivals { get; private set; }
    [field: SerializeField] public int CurrentRerolls { get; private set; }
    [field: SerializeField] public int CurrentSkips { get; private set; }
    [field: SerializeField] public int CurrentBanishes { get; private set; }
    #endregion

    private PlayerInventory playerInventory;

    // --- YENÝ: Geçici Bonuslar ---
    private float temporaryDamageBonus = 0f;
    // -----------------------------

    void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        // 1. Temel deðerleri ata
        CurrentMaxHealth = baseMaxHealth;
        CurrentHpRegen = baseHpRegen;
        CurrentMaxOverheal = baseMaxOverheal;
        CurrentArmor = baseArmor;
        CurrentMaxShield = baseMaxShield;
        CurrentEvasion = baseEvasion;
        CurrentDamageMultiplier = baseDamageMultiplier;
        CurrentCritChance = baseCritChance;
        CurrentCritDamage = baseCritDamage;
        CurrentAttackSpeedMultiplier = baseAttackSpeedMultiplier;
        CurrentProjectileSpeedMultiplier = baseProjectileSpeedMultiplier;
        CurrentProjectileCountBonus = baseProjectileCountBonus;
        CurrentProjectileBounce = baseProjectileBounce;
        CurrentPierce = basePierce;
        CurrentSizeMultiplier = baseSizeMultiplier;
        CurrentDurationMultiplier = baseDurationMultiplier;
        CurrentBleedPercent = baseBleedPercent;
        CurrentCritBleedPercent = 0f;
        CurrentLifeSteal = baseLifeSteal;
        CurrentThorns = baseThorns;
        CurrentCritHeal = baseCritHeal;
        CurrentMoveSpeed = baseMoveSpeed;
        CurrentExtraJumps = baseExtraJumps;
        CurrentJumpHeightMultiplier = baseJumpHeightMultiplier;
        CurrentLuck = baseLuck;
        CurrentCurse = baseCurse;
        CurrentMagnetRange = baseMagnetRange;
        CurrentXpBonus = baseXpBonus;
        CurrentGoldBonus = baseGoldBonus;
        CurrentDropChanceBonus = baseDropChanceBonus;
        CurrentEliteSpawnChanceBonus = baseEliteSpawnChanceBonus;
        CurrentSkillCooldownReduction = baseSkillCooldownReduction;
        CurrentRevivals = baseRevivals;
        CurrentRerolls = baseRerolls;
        CurrentSkips = baseSkips;
        CurrentBanishes = baseBanishes;

        // 2. Item Bonuslarýný Uygula
        if (playerInventory != null && playerInventory.ownedItems != null)
        {
            foreach (KeyValuePair<ItemData, int> itemPair in playerInventory.ownedItems)
            {
                ItemData item = itemPair.Key;
                int level = itemPair.Value;
                if (item == null) continue;

                foreach (ItemStatModifier mod in item.modifiers)
                {
                    float totalValue = 0;
                    if (item.isStackable) totalValue = mod.baseAmount + (mod.amountPerLevel * (level - 1));
                    else totalValue = mod.baseAmount;

                    ApplyItemBonus(mod.statType, totalValue, mod.isPercentage);
                }
            }
        }

        // --- 3. GEÇÝCÝ BONUSLARI EKLE (YENÝ) ---
        CurrentDamageMultiplier += temporaryDamageBonus;
        // ---------------------------------------

        // 4. Limitler ve Mantýk
        if (CurrentCritChance > 100f) { float excess = CurrentCritChance - 100f; CurrentCritDamage += excess; CurrentCritChance = 100f; }
        CurrentCritChance = Mathf.Clamp(CurrentCritChance, 0f, 100f);
        CurrentEvasion = Mathf.Clamp(CurrentEvasion, 0f, 90f);
        CurrentLifeSteal = Mathf.Clamp(CurrentLifeSteal, 0f, 300f);
        CurrentDropChanceBonus = Mathf.Clamp(CurrentDropChanceBonus, 0f, 1000f);
        CurrentSkillCooldownReduction = Mathf.Clamp(CurrentSkillCooldownReduction, -Mathf.Infinity, 95f);
        CurrentAttackSpeedMultiplier = Mathf.Max(10f, CurrentAttackSpeedMultiplier);
        CurrentMaxHealth = Mathf.Max(1f, CurrentMaxHealth);
        CurrentMoveSpeed = Mathf.Max(0.1f, CurrentMoveSpeed);

        Debug.Log("Statlar Yeniden Hesaplandý.");
        NotifyOtherSystems();
    }

    private void ApplyItemBonus(PassiveStatType type, float value, bool isPercentage)
    {
        switch (type)
        {
            case PassiveStatType.CritHeal: CurrentCritHeal += value; break;
            case PassiveStatType.Bleed: CurrentBleedPercent += value; break;
            case PassiveStatType.CritBleed: CurrentCritBleedPercent += value; break;
            case PassiveStatType.Curse: CurrentCurse += value; break;
            case PassiveStatType.SkillCooldown: CurrentSkillCooldownReduction += value; break;
            case PassiveStatType.JumpHeight: CurrentJumpHeightMultiplier += value; break;
            case PassiveStatType.DropChance: CurrentDropChanceBonus += value; break;
            case PassiveStatType.ProjectileSpeed: CurrentProjectileSpeedMultiplier += value; break;

            case PassiveStatType.MaxHealth: CurrentMaxHealth += isPercentage ? (baseMaxHealth * value / 100f) : value; break;
            case PassiveStatType.MoveSpeed: CurrentMoveSpeed += isPercentage ? (baseMoveSpeed * value / 100f) : value; break;
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
            case PassiveStatType.None: break;
        }
    }

    private void NotifyOtherSystems()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.UpdateMaxValues(CurrentMaxHealth, CurrentMaxShield);

        XpCollector collector = GetComponentInChildren<XpCollector>();
        if (collector != null) collector.UpdateMagnetRadius();
    }

    // --- GEÇÝCÝ STAT YÖNETÝMÝ (YENÝ) ---
    public void AddTemporaryDamage(float amount)
    {
        temporaryDamageBonus += amount;
        RecalculateStats();
    }
    public void RemoveTemporaryDamage(float amount)
    {
        temporaryDamageBonus -= amount;
        RecalculateStats();
    }
    // ----------------------------------

    // --- TEMEL STATLARI ARTIRAN METOTLAR ---
    public void IncreaseBaseMaxHealth(float amount) { baseMaxHealth += amount; RecalculateStats(); }
    public void IncreaseBaseRevivals(int amount) { baseRevivals += amount; RecalculateStats(); }
    public void IncreaseBaseRerolls(int amount) { baseRerolls += amount; RecalculateStats(); }

    // --- HARCANABÝLÝR STATLAR ---
    public bool UseRevival() { if (CurrentRevivals > 0) { baseRevivals--; RecalculateStats(); return true; } return false; }
    public bool UseReroll() { if (CurrentRerolls > 0) { baseRerolls--; RecalculateStats(); return true; } return false; }
    public bool UseSkip() { if (CurrentSkips > 0) { baseSkips--; RecalculateStats(); return true; } return false; }
    public bool UseBanish() { if (CurrentBanishes > 0) { baseBanishes--; RecalculateStats(); return true; } return false; }
}