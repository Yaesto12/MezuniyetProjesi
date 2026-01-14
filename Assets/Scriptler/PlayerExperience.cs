using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerWeaponController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerExperience : MonoBehaviour
{
    [Header("Yükseltme Sistemi (SÝLAH)")]
    [Tooltip("Tüm olasý 'Yükseltme Fýrsatlarý'nýn bulunduðu liste.")]
    [SerializeField] private List<UpgradeData> upgradePool;
    [SerializeField] private LevelUpUI levelUpUI;

    [Header("Nadirlik Ayarlarý")]
    [SerializeField] private float commonMultiplier = 1.0f;
    [SerializeField] private float rareMultiplier = 1.5f;
    [SerializeField] private float epicMultiplier = 2.0f;
    [SerializeField] private float legendaryMultiplier = 3.0f;
    [SerializeField] private int statsPerCommon = 1;
    [SerializeField] private int statsPerRare = 2;
    [SerializeField] private int statsPerEpic = 2;
    [SerializeField] private int statsPerLegendary = 3;

    // --- Referanslar ---
    private PlayerInventory playerInventory;
    private PlayerWeaponController weaponController;
    private PlayerStats playerStats;

    void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
        weaponController = GetComponent<PlayerWeaponController>();
        playerStats = GetComponent<PlayerStats>();

        if (levelUpUI == null)
        {
            levelUpUI = FindFirstObjectByType<LevelUpUI>(FindObjectsInactive.Include);
        }
    }

    // ========================================================================
    // --- HATA ÇÖZÜCÜ KISIM (KÖPRÜ) ---
    // ========================================================================
    // PlayerHurtbox veya diðer scriptler hala buraya XP göndermeye çalýþýyor.
    // Biz de gelen bu isteði asýl patron olan PlayerStats'a iletiyoruz.
    public void GainXp(int amount)
    {
        if (playerStats != null)
        {
            playerStats.GainExperience(amount);
        }
    }
    // ========================================================================

    // PlayerStats level atladýðýnda burayý çaðýracak.
    public void StartLevelUpSequence()
    {
        if (levelUpUI == null)
        {
            Debug.LogError("PlayerExperience: LevelUpUI bulunamadý, ekran açýlamýyor!");
            return;
        }

        Debug.Log("Level Up Ekraný Tetiklendi!");

        // Oyunu durdur
        Time.timeScale = 0f;

        // Mouse'u aç
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Seçenekleri oluþtur
        List<UpgradeData> possibleOpportunities = GetFilteredUpgradeOpportunities();

        // Eðer havuzda hiç upgrade kalmadýysa (bütün silahlar max ise) boþ geç
        if (possibleOpportunities.Count == 0)
        {
            Debug.Log("Alýnacak upgrade kalmadý!");
            ResumeGame();
            return;
        }

        List<UpgradeData> chosenOpportunities = ChooseRandomOpportunities(possibleOpportunities, 3);
        List<GeneratedUpgradeOption> finalOptions = GenerateOptionsFromOpportunities(chosenOpportunities);

        if (finalOptions.Count > 0)
        {
            levelUpUI.ShowOptions(finalOptions);
        }
        else
        {
            ResumeGame();
        }
    }

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
                    // Silah bizde yoksa listeye ekle
                    if (upgrade.weaponDataToUnlock != null && !playerInventory.HasWeaponData(upgrade.weaponDataToUnlock))
                        canAdd = true;
                    break;
                case UpgradeCategory.WeaponUpgrade:
                    // Silah bizde varsa upgrade edilebilir
                    if (upgrade.targetWeaponData != null && playerInventory.HasWeaponData(upgrade.targetWeaponData))
                        canAdd = true;
                    break;
                default:
                    canAdd = true;
                    break;
            }
            if (canAdd) availableUpgrades.Add(upgrade);
        }
        return availableUpgrades;
    }

    private List<UpgradeData> ChooseRandomOpportunities(List<UpgradeData> opportunities, int count)
    {
        System.Random rng = new System.Random();
        return opportunities.OrderBy(a => rng.Next()).Take(count).ToList();
    }

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
            option.Category = opportunity.category;

            // Nadirlik Belirle
            float luck = (playerStats != null) ? playerStats.CurrentLuck : 0;
            RarityLevel rolledRarity = RollForRarity(opportunity.rarity, luck);
            option.RolledRarity = rolledRarity;

            StringBuilder descBuilder = new StringBuilder();

            // Renkli baþlýk
            string rarityColor = GetRarityColorHex(rolledRarity);
            option.Name = $"<color={rarityColor}>{opportunity.upgradeName}</color>";

            descBuilder.AppendLine(opportunity.description);

            // Silah Upgrade Detaylarý
            if (opportunity.category == UpgradeCategory.WeaponUpgrade && opportunity.targetWeaponData != null)
            {
                descBuilder.AppendLine("--------------------");
                int statsToUpgrade = GetStatCountForRarity(rolledRarity);
                List<WeaponStatType> availableStats = new List<WeaponStatType>(opportunity.targetWeaponData.availableStatUpgrades);

                if (availableStats.Count > 0)
                {
                    if (availableStats.Count < statsToUpgrade) statsToUpgrade = availableStats.Count;
                    List<WeaponStatType> chosenStats = availableStats.OrderBy(a => rng.Next()).Take(statsToUpgrade).ToList();

                    for (int i = 0; i < chosenStats.Count; i++)
                    {
                        WeaponStatType stat = chosenStats[i];
                        float baseValue = GetBaseValueForWeaponStat(stat);
                        float finalValue = GetValueForRarity(baseValue, rolledRarity);
                        bool isPercent = IsWeaponStatPercentage(stat);

                        string desc = $"+ {finalValue}{(isPercent ? "%" : "")} {stat}";
                        option.Modifications.Add(new StatModification { WeaponStat = stat, PassiveStat = PassiveStatType.None, Value = finalValue, IsPercentage = isPercent, Description = desc });

                        descBuilder.AppendLine(desc);
                    }
                }
            }
            // Silah Açma Detaylarý
            else if (opportunity.category == UpgradeCategory.WeaponUnlock)
            {
                descBuilder.AppendLine("<color=yellow>YENÝ SÝLAH!</color>");
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

        if (opportunity.category == UpgradeCategory.WeaponUnlock)
        {
            if (opportunity.weaponDataToUnlock != null && !playerInventory.HasWeaponData(opportunity.weaponDataToUnlock))
            {
                playerInventory.AddWeaponData(opportunity.weaponDataToUnlock);
            }
        }
        else if (opportunity.category == UpgradeCategory.WeaponUpgrade)
        {
            if (opportunity.targetWeaponData != null)
            {
                playerInventory.IncrementUpgradeLevel(opportunity.targetWeaponData);
            }

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
    }

    private void ApplyStatModification(WeaponData targetWeapon, StatModification modification)
    {
        if (targetWeapon != null)
        {
            MonoBehaviour handler = weaponController.GetActiveWeaponHandler(targetWeapon);
            if (handler != null) ApplyWeaponStatUpgrade(handler, modification);
        }
    }

    private void ApplyWeaponStatUpgrade(MonoBehaviour weaponHandler, StatModification mod)
    {
        float value = mod.Value;

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
    }

    private RarityLevel RollForRarity(RarityLevel baseRarity, float playerLuck)
    {
        float roll = Random.Range(0f, 100f);
        if (roll < playerLuck)
        {
            int currentRarityInt = (int)baseRarity;
            int maxRarityInt = System.Enum.GetValues(typeof(RarityLevel)).Length - 1;
            if (currentRarityInt < maxRarityInt) return (RarityLevel)(currentRarityInt + 1);
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
        float multiplier = 1f;
        switch (rarity)
        {
            case RarityLevel.Common: multiplier = commonMultiplier; break;
            case RarityLevel.Rare: multiplier = rareMultiplier; break;
            case RarityLevel.Epic: multiplier = epicMultiplier; break;
            case RarityLevel.Legendary: multiplier = legendaryMultiplier; break;
        }
        float finalValue = baseValue * multiplier;
        if (Mathf.Abs(finalValue % 1) < 0.01f) return Mathf.Round(finalValue);
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
            case WeaponStatType.Speed: return 10f;
            case WeaponStatType.Range: return 10f;
            case WeaponStatType.Duration: return 15f;
            case WeaponStatType.Pierce: return 1f;
            case WeaponStatType.ProjectileBounce: return 1f;
            default: return 5f;
        }
    }

    private bool IsWeaponStatPercentage(WeaponStatType stat)
    {
        switch (stat)
        {
            case WeaponStatType.ProjectileCount:
            case WeaponStatType.Pierce:
            case WeaponStatType.ProjectileBounce:
                return false;
            default: return true;
        }
    }

    private string GetRarityColorHex(RarityLevel rarity)
    {
        switch (rarity)
        {
            case RarityLevel.Common: return "#FFFFFF";
            case RarityLevel.Rare: return "#00FFFF";
            case RarityLevel.Epic: return "#FF00FF";
            case RarityLevel.Legendary: return "#FFA500";
            default: return "#FFFFFF";
        }
    }
}