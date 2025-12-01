using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text; // StringBuilder için

[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerWeaponController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerExperience : MonoBehaviour
{
    // --- Inspector Ayarlarý ---
    [Header("Seviye Ayarlarý")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int xpToNextLevel = 100;
    [SerializeField] private float xpMultiplierPerLevel = 1.2f;

    [Header("Animasyon Ayarlarý")]
    [SerializeField] private float xpFillSpeed = 150f;

    [Header("Yükseltme Sistemi (SÝLAH)")]
    [Tooltip("Tüm olasý 'Yükseltme Fýrsatlarý'nýn (UpgradeData) bulunduðu liste.")]
    [SerializeField] private List<UpgradeData> upgradePool;
    [Tooltip("Seviye atlama UI'ýný yöneten script.")]
    [SerializeField] private LevelUpUI levelUpUI;

    [Header("Nadirlik Ayarlarý (Yükseltme Deðerleri)")]
    [SerializeField] private float commonMultiplier = 1.0f;
    [SerializeField] private float rareMultiplier = 1.5f;
    [SerializeField] private float epicMultiplier = 2.0f;
    [SerializeField] private float legendaryMultiplier = 3.0f;

    [Header("Nadirlik Ayarlarý (Stat Sayýsý)")]
    [SerializeField] private int statsPerCommon = 1;
    [SerializeField] private int statsPerRare = 2;
    [SerializeField] private int statsPerEpic = 2;
    [SerializeField] private int statsPerLegendary = 3;

    // --- Referanslar ---
    private PlayerInventory playerInventory;
    private PlayerWeaponController weaponController;
    private PlayerStats playerStats;

    // --- Dahili Deðiþkenler ---
    private int targetXp = 0;
    private float animatingXp = 0;
    private bool isLevelUpSequenceActive = false;

    void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
        weaponController = GetComponent<PlayerWeaponController>();
        playerStats = GetComponent<PlayerStats>();
        if (playerInventory == null || weaponController == null || playerStats == null)
        {
            Debug.LogError("FATAL ERROR: PlayerExperience için gerekli bileþenler bulunamadý!", gameObject);
            enabled = false; return;
        }
        if (levelUpUI == null)
        {
            levelUpUI = FindFirstObjectByType<LevelUpUI>(FindObjectsInactive.Include);
            if (levelUpUI == null) Debug.LogWarning("LevelUpUI referansý atanmamýþ ve sahnede bulunamadý!", this);
        }
        if (upgradePool == null || upgradePool.Count == 0) { Debug.LogWarning("Upgrade Pool boþ!", this); }
    }

    void Start()
    {
        UpdateLevelTextInternal();
        UpdateXpBarInternal();
    }

    void Update()
    {
        if (isLevelUpSequenceActive || animatingXp >= targetXp) return;
        animatingXp += xpFillSpeed * Time.deltaTime;
        animatingXp = Mathf.Min(animatingXp, targetXp);
        if (animatingXp >= xpToNextLevel) { StartLevelUpSequence(); }
        else { UpdateXpBarInternal(); }
    }

    public void GainXp(int amount)
    {
        if (!isLevelUpSequenceActive && amount > 0)
        {
            float bonusMultiplier = (playerStats != null) ? playerStats.CurrentXpBonus / 100f : 1f;
            int finalXp = Mathf.FloorToInt(amount * bonusMultiplier);
            targetXp += finalXp;
        }
    }

    private void StartLevelUpSequence()
    {
        if (levelUpUI == null) { Debug.LogError("Seviye atlanamýyor, LevelUpUI script'i bulunamadý!"); ResumeGame(); return; }

        isLevelUpSequenceActive = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PerformLevelUpCalculations();

        // 1. Filtrelenmiþ "Silah Fýrsatlarý"ný (UpgradeData) al
        List<UpgradeData> possibleOpportunities = GetFilteredUpgradeOpportunities();
        // 2. Bu fýrsatlardan 3 tane rastgele (Þans'a göre) seç
        List<UpgradeData> chosenOpportunities = ChooseRandomOpportunities(possibleOpportunities, 3);
        // 3. Bu 3 fýrsatý, "Dinamik Yükseltme Sonuçlarý"na dönüþtür
        List<GeneratedUpgradeOption> finalOptions = GenerateOptionsFromOpportunities(chosenOpportunities);

        if (finalOptions.Count > 0)
        {
            levelUpUI.ShowOptions(finalOptions);
        }
        else
        {
            Debug.LogWarning("Sunulacak geçerli yükseltme seçeneði bulunamadý. Oyun devam ettiriliyor.");
            ApplyGeneratedUpgrade(null);
        }
    }

    private void PerformLevelUpCalculations()
    {
        float remainingXp = animatingXp - xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplierPerLevel);
        targetXp = Mathf.Max(0, (int)remainingXp);
        animatingXp = 0;
        UpdateLevelTextInternal();
        UpdateXpBarInternal();

        Debug.LogWarning($"SEVÝYE ATLANDI! Yeni Seviye: {currentLevel}");

        // --- YENÝ EKLENEN KISIM: Olayý Tetikle ---
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.TriggerPlayerLevelUp(currentLevel);
        }
        // ----------------------------------------
    }

    /// <summary>
    /// Oyuncunun alabileceði tüm geçerli "Silah Fýrsatlarýný" (UpgradeData) filtreler.
    /// </summary>
    private List<UpgradeData> GetFilteredUpgradeOpportunities()
    {
        List<UpgradeData> availableUpgrades = new List<UpgradeData>();
        if (upgradePool == null || playerInventory == null) return availableUpgrades;

        foreach (UpgradeData upgrade in upgradePool)
        {
            if (upgrade == null) continue;
            bool canAdd = false;
            switch (upgrade.category)
            {
                case UpgradeCategory.WeaponUnlock:
                    if (upgrade.weaponDataToUnlock != null && !playerInventory.HasWeaponData(upgrade.weaponDataToUnlock))
                        canAdd = true;
                    break;
                case UpgradeCategory.WeaponUpgrade:
                    if (upgrade.targetWeaponData != null && playerInventory.HasWeaponData(upgrade.targetWeaponData))
                        canAdd = true;
                    break;
            }
            if (canAdd) availableUpgrades.Add(upgrade);
        }
        return availableUpgrades;
    }

    /// <summary>
    /// Verilen listeden, Þans'a göre aðýrlýklý rastgele seçim yaparak 'count' adet seçer.
    /// </summary>
    private List<UpgradeData> ChooseRandomOpportunities(List<UpgradeData> opportunities, int count)
    {
        System.Random rng = new System.Random();
        List<UpgradeData> shuffledList = opportunities.OrderBy(a => rng.Next()).ToList();
        return shuffledList.Take(count).ToList();
    }

    /// <summary>
    /// Seçilen 3 "Fýrsat"ý, "Dinamik Sonuç"lara (GeneratedUpgradeOption) dönüþtürür.
    /// </summary>
    private List<GeneratedUpgradeOption> GenerateOptionsFromOpportunities(List<UpgradeData> opportunities)
    {
        List<GeneratedUpgradeOption> finalOptions = new List<GeneratedUpgradeOption>();
        System.Random rng = new System.Random();

        foreach (UpgradeData opportunity in opportunities)
        {
            GeneratedUpgradeOption option = new GeneratedUpgradeOption();
            option.BaseUpgradeData = opportunity;
            option.Icon = opportunity.icon;
            option.Modifications = new List<StatModification>();

            RarityLevel rolledRarity = RollForRarity(opportunity.rarity, playerStats.CurrentLuck);
            option.RolledRarity = rolledRarity;

            StringBuilder descBuilder = new StringBuilder();
            option.Name = $"[{rolledRarity}] {opportunity.upgradeName}";
            descBuilder.AppendLine(opportunity.description);
            descBuilder.AppendLine("--------------------");

            if (opportunity.category == UpgradeCategory.WeaponUnlock)
            {
                // Açýklama gerekmez
            }
            else if (opportunity.category == UpgradeCategory.WeaponUpgrade)
            {
                int statsToUpgrade = GetStatCountForRarity(rolledRarity);
                List<WeaponStatType> availableStats = new List<WeaponStatType>(opportunity.targetWeaponData.availableStatUpgrades);
                if (availableStats.Count == 0)
                {
                    descBuilder.Append("Yükseltilecek stat bulunamadý!");
                }
                else
                {
                    if (availableStats.Count < statsToUpgrade) statsToUpgrade = availableStats.Count;
                    List<WeaponStatType> chosenStats = availableStats.OrderBy(a => rng.Next()).Take(statsToUpgrade).ToList();

                    for (int i = 0; i < chosenStats.Count; i++)
                    {
                        WeaponStatType stat = chosenStats[i];
                        float baseValue = GetBaseValueForWeaponStat(stat);
                        float finalValue = GetValueForRarity(baseValue, rolledRarity);
                        bool isPercent = IsWeaponStatPercentage(stat);
                        string desc = $"{stat}: +{finalValue}{(isPercent ? "%" : "")}";
                        option.Modifications.Add(new StatModification { WeaponStat = stat, PassiveStat = PassiveStatType.None, Value = finalValue, IsPercentage = isPercent, Description = desc });
                        descBuilder.Append(desc);
                        if (i < chosenStats.Count - 1) descBuilder.Append("\n");
                    }
                }
            }

            option.Description = descBuilder.ToString();
            finalOptions.Add(option);
        }
        return finalOptions;
    }


    public void ApplyGeneratedUpgrade(GeneratedUpgradeOption chosenOption)
    {
        if (chosenOption == null) { ResumeGame(); return; }
        Debug.Log("Yükseltme Uygulanýyor: " + chosenOption.Name);
        UpgradeData opportunity = chosenOption.BaseUpgradeData;
        playerInventory.IncrementUpgradeLevel(opportunity);

        if (opportunity.category == UpgradeCategory.WeaponUnlock)
        {
            if (opportunity.weaponDataToUnlock != null)
            {
                if (!playerInventory.HasWeaponData(opportunity.weaponDataToUnlock))
                {
                    playerInventory.AddWeaponData(opportunity.weaponDataToUnlock);
                    WeaponData runtimeData = Instantiate(opportunity.weaponDataToUnlock);
                    weaponController.AddAndInitializeWeapon(runtimeData);
                }
            }
        }
        else if (opportunity.category == UpgradeCategory.WeaponUpgrade)
        {
            foreach (StatModification mod in chosenOption.Modifications)
            {
                ApplyStatModification(opportunity.targetWeaponData, mod);
            }
        }
        ResumeGame();
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isLevelUpSequenceActive = false;
        UpdateXpBarInternal();
    }

    private void ApplyStatModification(WeaponData targetWeapon, StatModification modification)
    {
        if (targetWeapon != null)
        {
            MonoBehaviour handler = weaponController.GetActiveWeaponHandler(targetWeapon);
            if (handler != null)
            {
                ApplyWeaponStatUpgrade(handler, modification);
            }
            else { Debug.LogWarning($"Aktif Handler bulunamadý: {targetWeapon.name}"); }
        }
    }


    private void ApplyWeaponStatUpgrade(MonoBehaviour weaponHandler, StatModification mod)
    {
        float value = mod.Value;
        bool isPercent = mod.IsPercentage;

        Debug.Log($"ApplyWeaponStatUpgrade: Handler={weaponHandler.GetType().Name}, Stat={mod.WeaponStat}, Deðer={value}");

        if (weaponHandler is ProjectileWeaponHandler projHandler)
        {
            switch (mod.WeaponStat)
            {
                case WeaponStatType.Damage: projHandler.IncreaseDamageMultiplier(value); break;
                case WeaponStatType.ProjectileCount: projHandler.IncreaseProjectileCount((int)value); break;
                case WeaponStatType.Cooldown: projHandler.DecreaseCooldown(value); break;
                case WeaponStatType.Speed: projHandler.IncreaseProjectileSpeed(value); break;
                case WeaponStatType.Size: projHandler.IncreaseProjectileScale(value); break;
                case WeaponStatType.Duration: projHandler.IncreaseDuration(value); break;
                case WeaponStatType.Pierce: projHandler.IncreasePierce(value); break;
                case WeaponStatType.ProjectileBounce: projHandler.IncreaseBounce((int)value); break;
            }
        }
        else if (weaponHandler is MeleeWeaponHandler meleeHandler)
        {
            switch (mod.WeaponStat)
            {
                case WeaponStatType.Damage: meleeHandler.IncreaseDamageMultiplier(value); break;
                case WeaponStatType.Range: meleeHandler.IncreaseRangeMultiplier(value); break;
                case WeaponStatType.Cooldown: meleeHandler.DecreaseCooldown(value); break;
            }
        }
        else if (weaponHandler is AreaEffectWeaponHandler areaHandler)
        {
            switch (mod.WeaponStat)
            {
                case WeaponStatType.Damage: areaHandler.IncreaseDamageMultiplier(value); break;
                case WeaponStatType.Range: areaHandler.IncreaseRangeMultiplier(value); break;
                case WeaponStatType.Cooldown: areaHandler.DecreaseTickRate(value); break;
            }
        }
        else if (weaponHandler is TargetedExplosionHandler explosionHandler)
        {
            switch (mod.WeaponStat)
            {
                case WeaponStatType.Damage: explosionHandler.IncreaseDamageMultiplier(value); break;
                case WeaponStatType.Range: explosionHandler.IncreaseRadiusMultiplier(value); break;
                case WeaponStatType.ProjectileCount: explosionHandler.IncreaseExplosionCount((int)value); break;
                case WeaponStatType.Cooldown: explosionHandler.DecreaseCooldown(value); break;
            }
        }
        else if (weaponHandler is WaveWeaponHandler waveHandler)
        {
            switch (mod.WeaponStat)
            {
                case WeaponStatType.Damage: waveHandler.IncreaseDamageMultiplier(value); break;
                case WeaponStatType.Range: waveHandler.IncreaseWaveWidth(value); break;
                case WeaponStatType.Speed: waveHandler.IncreaseWaveSpeed(value); break;
                case WeaponStatType.Cooldown: waveHandler.DecreaseCooldown(value); break;
                case WeaponStatType.Pierce: waveHandler.IncreasePierce((int)value); break;
            }
        }
        else if (weaponHandler is ProximityMineHandler mineHandler)
        {
            switch (mod.WeaponStat)
            {
                case WeaponStatType.Damage: mineHandler.IncreaseDamageMultiplier(value); break;
                case WeaponStatType.Range: mineHandler.IncreaseExplosionRadius(value); break;
                case WeaponStatType.ProjectileCount: mineHandler.IncreaseMineCount((int)value); break;
                case WeaponStatType.Cooldown: mineHandler.DecreaseCooldown(value); break;
                case WeaponStatType.Duration: mineHandler.IncreaseMineLifetime(value); break;
            }
        }
        else
        {
            Debug.LogWarning($"ApplyWeaponStatUpgrade bilinmeyen Handler türüyle çaðrýldý: {weaponHandler.GetType().Name}");
        }
    }


    // --- YARDIMCI METOTLAR (Nadirlik ve Deðer Hesaplama) ---
    private RarityLevel RollForRarity(RarityLevel baseRarity, float playerLuck)
    {
        float roll = Random.Range(0f, 100f);
        if (roll < playerLuck)
        {
            int currentRarityInt = (int)baseRarity;
            int maxRarityInt = System.Enum.GetValues(typeof(RarityLevel)).Length - 1;
            if (currentRarityInt < maxRarityInt)
            {
                Debug.Log("Þanslý Zar! Nadirlik 1 seviye arttý!");
                return (RarityLevel)(currentRarityInt + 1);
            }
        }
        return baseRarity;
    }
    private int GetStatCountForRarity(RarityLevel rarity)
    {
        switch (rarity)
        {
            case RarityLevel.Common: return statsPerCommon;
            case RarityLevel.Rare: return statsPerRare;
            case RarityLevel.Epic: return statsPerEpic;
            case RarityLevel.Legendary: return statsPerLegendary;
            default: return 1;
        }
    }
    private float GetValueForRarity(float baseValue, RarityLevel rarity)
    {
        float multiplier;
        switch (rarity)
        {
            case RarityLevel.Common: multiplier = commonMultiplier; break;
            case RarityLevel.Rare: multiplier = rareMultiplier; break;
            case RarityLevel.Epic: multiplier = epicMultiplier; break;
            case RarityLevel.Legendary: multiplier = legendaryMultiplier; break;
            default: multiplier = 1f; break;
        }
        float finalValue = baseValue * multiplier;
        if (Mathf.Approximately(finalValue % 1, 0)) return Mathf.RoundToInt(finalValue);
        return Mathf.Round(finalValue * 100f) / 100f;
    }
    private float GetBaseValueForWeaponStat(WeaponStatType stat)
    {
        switch (stat)
        {
            case WeaponStatType.Damage: return 10f;
            case WeaponStatType.Cooldown: return 5f;
            case WeaponStatType.ProjectileCount: return 1f;
            case WeaponStatType.Size: return 10f;
            case WeaponStatType.Speed: return 15f;
            case WeaponStatType.Range: return 15f;
            case WeaponStatType.Duration: return 20f;
            case WeaponStatType.Pierce: return 1f;
            case WeaponStatType.ProjectileBounce: return 1f;
            default: return 5f;
        }
    }
    private bool IsWeaponStatPercentage(WeaponStatType stat)
    {
        switch (stat)
        {
            case WeaponStatType.ProjectileCount: return false;
            case WeaponStatType.Pierce: return false;
            case WeaponStatType.ProjectileBounce: return false;
            default: return true;
        }
    }

    // --- UI GÜNCELLEME METOTLARI (UIManager veya Direkt) ---
    private void UpdateLevelTextInternal()
    {
        if (UIManager.Instance != null) { UIManager.Instance.UpdateLevelText(currentLevel); }
    }
    private void UpdateXpBarInternal()
    {
        if (UIManager.Instance != null && xpToNextLevel > 0) { UIManager.Instance.UpdateXpBar(animatingXp, xpToNextLevel); }
    }
}