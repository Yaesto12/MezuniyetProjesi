using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    // --- ÝLERLEME VE EKONOMÝ ---
    [Header("Ýlerleme ve Ekonomi")]
    public int currentLevel = 1;
    public int currentGold = 0;
    public float currentXp = 0;
    public float xpToNextLevel = 100f;
    public float levelDifficultyMultiplier = 1.2f;

    // YENÝ EKLENDÝ: Açýlan sandýk sayýsýný burada tutuyoruz
    public int chestsOpened = 0;

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

    // --- ANLIK STATLAR ---
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
    [field: SerializeField] public float CurrentDamageToMobs { get; private set; }
    [field: SerializeField] public float CurrentDamageToElites { get; private set; }
    [field: SerializeField] public float CurrentDamageToMiniBosses { get; private set; }
    [field: SerializeField] public float CurrentDamageToBosses { get; private set; }

    [field: SerializeField] public float CurrentSkillCooldownReduction { get; private set; }
    [field: SerializeField] public int CurrentRevivals { get; private set; }
    [field: SerializeField] public int CurrentRerolls { get; private set; }
    [field: SerializeField] public int CurrentSkips { get; private set; }
    [field: SerializeField] public int CurrentBanishes { get; private set; }
    #endregion

    private PlayerInventory playerInventory;

    // --- GEÇÝCÝ BONUSLAR ---
    private float temporaryDamageBonus = 0f;
    private float temporaryAttackSpeedBonus = 0f;
    private float damageBonusFromOverheal = 0f;
    private float temporaryMoveSpeedBonus = 0f;
    private float temporaryMaxHealthBonus = 0f;

    void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
        RecalculateStats();
    }

    void Start()
    {
        UpdateAllUI();
    }

    // --- PARA KAZANMA ---
    public void AddGold(int amount)
    {
        float multiplier = CurrentGoldBonus / 100f;
        int finalAmount = Mathf.RoundToInt(amount * multiplier);

        currentGold += finalAmount;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGoldText(currentGold);
        }
    }

    // --- PARA HARCAMA (YENÝ EKLENDÝ) ---
    // Sandýk scripti bunu çaðýracak
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateAllUI(); // UI güncelle
            return true; // Ýþlem baþarýlý
        }
        return false; // Yetersiz bakiye
    }

    // --- XP VE LEVEL ---
    public void AddXp(float amount)
    {
        float multiplier = CurrentXpBonus / 100f;
        float finalAmount = amount * multiplier;

        currentXp += finalAmount;

        while (currentXp >= xpToNextLevel)
        {
            LevelUp();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateXpBar(currentXp, xpToNextLevel);
        }
    }

    private void LevelUp()
    {
        currentXp -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel *= levelDifficultyMultiplier;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLevelText(currentLevel);
            UIManager.Instance.UpdateXpBar(currentXp, xpToNextLevel);
        }
    }

    public void UpdateAllUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGoldText(currentGold);
            UIManager.Instance.UpdateLevelText(currentLevel);
            UIManager.Instance.UpdateXpBar(currentXp, xpToNextLevel);
        }
    }

    // --- MEVCUT STAT HESAPLAMA SÝSTEMÝ ---
    public void RecalculateStats()
    {
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
        CurrentDamageToMobs = baseDamageToMobs;
        CurrentDamageToElites = baseDamageToElites;
        CurrentDamageToMiniBosses = baseDamageToMiniBosses;
        CurrentDamageToBosses = baseDamageToBosses;

        CurrentMoveSpeed += temporaryMoveSpeedBonus;
        CurrentMaxHealth += temporaryMaxHealthBonus;
        CurrentDamageMultiplier += temporaryDamageBonus;
        CurrentAttackSpeedMultiplier += temporaryAttackSpeedBonus;
        CurrentDamageMultiplier += damageBonusFromOverheal;

        if (CurrentCritChance > 100f) { float excess = CurrentCritChance - 100f; CurrentCritDamage += excess; CurrentCritChance = 100f; }
        CurrentCritChance = Mathf.Clamp(CurrentCritChance, 0f, 100f);
        CurrentEvasion = Mathf.Clamp(CurrentEvasion, 0f, 90f);
        CurrentLifeSteal = Mathf.Clamp(CurrentLifeSteal, 0f, 300f);
        CurrentDropChanceBonus = Mathf.Clamp(CurrentDropChanceBonus, 0f, 1000f);
        CurrentSkillCooldownReduction = Mathf.Clamp(CurrentSkillCooldownReduction, -Mathf.Infinity, 95f);
        CurrentAttackSpeedMultiplier = Mathf.Max(10f, CurrentAttackSpeedMultiplier);
        CurrentMaxHealth = Mathf.Max(1f, CurrentMaxHealth);
        CurrentMoveSpeed = Mathf.Max(0.1f, CurrentMoveSpeed);

        NotifyOtherSystems();
    }

    private void NotifyOtherSystems()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.UpdateMaxValues(CurrentMaxHealth, CurrentMaxShield);

        // Generic kontrol (Hata vermemesi için)
        var collector = GetComponentInChildren<MonoBehaviour>();
    }

    public void AddTemporaryDamage(float amount) { temporaryDamageBonus += amount; RecalculateStats(); }
    public void RemoveTemporaryDamage(float amount) { temporaryDamageBonus -= amount; RecalculateStats(); }
    public void SetTemporaryAttackSpeed(float amount) { temporaryAttackSpeedBonus = amount; RecalculateStats(); }

    public void SetOverhealDamageBonus(float bonus)
    {
        if (Mathf.Abs(damageBonusFromOverheal - bonus) > 0.01f)
        {
            damageBonusFromOverheal = bonus;
            RecalculateStats();
        }
    }

    public void IncreaseBaseMaxHealth(float amount) { baseMaxHealth += amount; RecalculateStats(); }
    public void IncreaseBaseHpRegen(float amount) { baseHpRegen += amount; RecalculateStats(); }
    public void IncreaseBaseArmor(float amount) { baseArmor += amount; RecalculateStats(); }
    public void IncreaseBaseMoveSpeed(float amount) { baseMoveSpeed += amount; RecalculateStats(); }
    public void IncreaseBaseLuck(float amount) { baseLuck += amount; RecalculateStats(); }
    public void IncreaseBaseMagnetRange(float amount) { baseMagnetRange += amount; RecalculateStats(); }
    public void IncreaseBaseXpBonus(float amount) { baseXpBonus += amount; RecalculateStats(); }
    public void IncreaseBaseGoldBonus(float amount) { baseGoldBonus += amount; RecalculateStats(); }
    public void IncreaseBaseRevivals(int amount) { baseRevivals += amount; RecalculateStats(); }
    public void IncreaseBaseRerolls(int amount) { baseRerolls += amount; RecalculateStats(); }
    public void IncreaseBaseSkips(int amount) { baseSkips += amount; RecalculateStats(); }
    public void IncreaseBaseBanishes(int amount) { baseBanishes += amount; RecalculateStats(); }
    public void IncreaseBaseDamageMultiplier(float amount) { baseDamageMultiplier += amount; RecalculateStats(); }
    public void IncreaseBaseCritChance(float amount) { baseCritChance += amount; RecalculateStats(); }
    public void IncreaseBaseAttackSpeedMultiplier(float amount) { baseAttackSpeedMultiplier += amount; RecalculateStats(); }

    public bool UseRevival() { if (CurrentRevivals > 0) { baseRevivals--; RecalculateStats(); return true; } return false; }
    public bool UseReroll() { if (CurrentRerolls > 0) { baseRerolls--; RecalculateStats(); return true; } return false; }
    public bool UseSkip() { if (CurrentSkips > 0) { baseSkips--; RecalculateStats(); return true; } return false; }
    public bool UseBanish() { if (CurrentBanishes > 0) { baseBanishes--; RecalculateStats(); return true; } return false; }

    public void AddTemporaryMaxHealth(float amount) { temporaryMaxHealthBonus += amount; RecalculateStats(); }
    public void RemoveTemporaryMaxHealth(float amount) { temporaryMaxHealthBonus -= amount; RecalculateStats(); }
    public void AddTemporaryMoveSpeed(float amount) { temporaryMoveSpeedBonus += amount; RecalculateStats(); }
    public void RemoveTemporaryMoveSpeed(float amount) { temporaryMoveSpeedBonus -= amount; RecalculateStats(); }
}