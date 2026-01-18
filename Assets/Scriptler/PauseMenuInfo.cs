using System.Collections.Generic;
using System.Linq; // GroupBy için gerekli
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuInfo : MonoBehaviour
{
    public static PauseMenuInfo Instance; // Singleton eriþimi için

    [Header("--- RENK AYARLARI ---")]
    [Tooltip("Stat Ýsimlerinin Rengi (Örn: 'Max Health', 'Armor')")]
    [SerializeField] private Color titleColor = Color.yellow; // Baþlýk Rengi

    [Tooltip("Stat Deðerlerinin Rengi (Örn: '100', '%50')")]
    [SerializeField] private Color valueColor = Color.white; // Deðer Rengi

    // Hex kodlarý
    private string titleHex;
    private string valueHex;

    // =========================================================
    // --- 1. SAÐ TARAF: STATLAR (TEXT REFERANSLARI) ---
    // =========================================================

    [Header("--- DEFENSE STATS ---")]
    public TextMeshProUGUI maxHealthText;   // Max Health
    public TextMeshProUGUI hpRegenText;     // HP Regen
    public TextMeshProUGUI armorText;       // Armor
    public TextMeshProUGUI evasionText;     // Evasion
    public TextMeshProUGUI lifestealText;   // Lifesteal
    public TextMeshProUGUI thornsText;      // Thorns

    [Header("--- OFFENSE STATS ---")]
    public TextMeshProUGUI damageText;      // Damage Multiplier
    public TextMeshProUGUI critChanceText;  // Crit Chance
    public TextMeshProUGUI critDamageText;  // Crit Damage
    public TextMeshProUGUI attackSpeedText; // Attack Speed

    [Header("--- WEAPON STATS ---")]
    public TextMeshProUGUI projCountText;   // Projectile Count
    public TextMeshProUGUI projBounceText;  // Projectile Bounce
    public TextMeshProUGUI sizeText;        // Size
    public TextMeshProUGUI durationText;    // Duration

    [Header("--- MOVEMENT ---")]
    public TextMeshProUGUI moveSpeedText;   // Move Speed
    public TextMeshProUGUI extraJumpsText;  // Extra Jumps

    [Header("--- MISC ---")]
    public TextMeshProUGUI luckText;        // Luck
    public TextMeshProUGUI curseText;       // Curse
    public TextMeshProUGUI magnetText;      // Magnet Range
    public TextMeshProUGUI xpBonusText;     // XP Bonus
    public TextMeshProUGUI goldBonusText;   // Gold Bonus

    [Header("--- ABILITIES ---")]
    public TextMeshProUGUI cooldownText;    // Cooldown Reduction
    public TextMeshProUGUI revivalsText;    // Revivals

    // =========================================================
    // --- 2. SOL TARAF: ENVANTER (ITEMLER) ---
    // =========================================================
    [Header("--- INVENTORY SETTINGS ---")]
    public Transform itemsGridParent;
    public GameObject itemIconPrefab;

    [Header("--- TOOLTIP SETTINGS ---")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipName;
    public TextMeshProUGUI tooltipDesc;

    // Script Referanslarý
    private PlayerStats stats;
    private PlayerInventory inventory;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Renkleri Hex koduna çeviriyoruz
        titleHex = "#" + ColorUtility.ToHtmlStringRGB(titleColor);
        valueHex = "#" + ColorUtility.ToHtmlStringRGB(valueColor);
    }

    void OnEnable()
    {
        if (stats == null) stats = FindAnyObjectByType<PlayerStats>();
        if (inventory == null) inventory = FindAnyObjectByType<PlayerInventory>();

        if (stats != null) UpdateStatsUI();
        if (inventory != null) UpdateItemsUI();

        HideTooltip();
    }

    public void ShowTooltip(string name, string desc)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            if (tooltipName != null) tooltipName.text = name;
            if (tooltipDesc != null) tooltipDesc.text = desc;
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private string FormatStat(string title, string value)
    {
        return $"<color={titleHex}>{title}:</color> <color={valueHex}>{value}</color>";
    }

    void UpdateStatsUI()
    {
        // DEFENSE
        if (maxHealthText) maxHealthText.text = FormatStat("Max Health", Mathf.Round(stats.CurrentMaxHealth).ToString());
        if (hpRegenText) hpRegenText.text = FormatStat("Recovery", $"{stats.CurrentHpRegen:F1}/s");
        if (armorText) armorText.text = FormatStat("Armor", Mathf.Round(stats.CurrentArmor).ToString());
        if (evasionText) evasionText.text = FormatStat("Evasion", $"%{stats.CurrentEvasion:F1}");
        if (lifestealText) lifestealText.text = FormatStat("Lifesteal", $"%{stats.CurrentLifeSteal}");
        if (thornsText) thornsText.text = FormatStat("Thorns", stats.CurrentThorns.ToString());

        // OFFENSE
        if (damageText) damageText.text = FormatStat("Might", $"%{Mathf.Round(stats.CurrentDamageMultiplier)}"); // "Hasar" yerine Might (Güç) havalý durur, istersen "Damage" yapabilirsin.
        if (critChanceText) critChanceText.text = FormatStat("Crit Rate", $"%{stats.CurrentCritChance:F1}");
        if (critDamageText) critDamageText.text = FormatStat("Crit DMG", $"%{Mathf.Round(stats.CurrentCritDamage)}");
        if (attackSpeedText) attackSpeedText.text = FormatStat("Cooldown", $"%{Mathf.Round(stats.CurrentAttackSpeedMultiplier)}"); // Attack Speed genelde Cooldown olarak geçer ama "Attack Speed" de olur.

        // WEAPON
        if (projCountText) projCountText.text = FormatStat("Amount", $"+{stats.CurrentProjectileCountBonus}");
        if (projBounceText) projBounceText.text = FormatStat("Bounce", $"+{stats.CurrentProjectileBounce}");
        if (sizeText) sizeText.text = FormatStat("Area", $"%{Mathf.Round(stats.CurrentSizeMultiplier)}");
        if (durationText) durationText.text = FormatStat("Duration", $"%{Mathf.Round(stats.CurrentDurationMultiplier)}");

        // MOVEMENT
        if (moveSpeedText) moveSpeedText.text = FormatStat("Speed", $"{stats.CurrentMoveSpeed:F1}");
        if (extraJumpsText) extraJumpsText.text = FormatStat("Extra Jumps", $"+{stats.CurrentExtraJumps}");

        // MISC
        if (luckText) luckText.text = FormatStat("Luck", stats.CurrentLuck.ToString());
        if (curseText) curseText.text = FormatStat("Curse", stats.CurrentCurse.ToString());
        if (magnetText) magnetText.text = FormatStat("Magnet", $"{stats.CurrentMagnetRange:F1}m");
        if (xpBonusText) xpBonusText.text = FormatStat("Growth", $"%{Mathf.Round(stats.CurrentXpBonus)}"); // Growth = XP Bonus
        if (goldBonusText) goldBonusText.text = FormatStat("Greed", $"%{Mathf.Round(stats.CurrentGoldBonus)}"); // Greed = Gold Bonus

        // ABILITIES
        if (cooldownText) cooldownText.text = FormatStat("Cooldown", $"-%{Mathf.Round(stats.CurrentSkillCooldownReduction)}");
        if (revivalsText) revivalsText.text = FormatStat("Revival", stats.CurrentRevivals.ToString());
    }

    void UpdateItemsUI()
    {
        if (itemsGridParent == null || itemIconPrefab == null) return;

        foreach (Transform child in itemsGridParent) Destroy(child.gameObject);

        var groupedItems = inventory.ownedItems
            .GroupBy(i => i.itemName)
            .Select(group => new { Data = group.First(), Count = group.Count() });

        foreach (var itemGroup in groupedItems)
        {
            GameObject newIconObj = Instantiate(itemIconPrefab, itemsGridParent);
            Image imgComponent = newIconObj.GetComponent<Image>();

            if (itemGroup.Data.icon != null)
                imgComponent.sprite = itemGroup.Data.icon;

            TextMeshProUGUI qText = newIconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (qText != null)
            {
                qText.text = itemGroup.Count > 1 ? $"x{itemGroup.Count}" : "";
            }

            ItemTooltipTrigger tooltip = newIconObj.AddComponent<ItemTooltipTrigger>();
            tooltip.description = itemGroup.Data.description;
            tooltip.itemName = itemGroup.Data.itemName;
        }
    }
}