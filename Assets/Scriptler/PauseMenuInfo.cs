using System.Collections.Generic;
using System.Linq; // GroupBy için gerekli
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuInfo : MonoBehaviour
{
    public static PauseMenuInfo Instance; // Singleton eriþimi için

    // =========================================================
    // --- 1. SAÐ TARAF: STATLAR (TEXT REFERANSLARI) ---
    // =========================================================

    [Header("--- SAVUNMA STATLARI ---")]
    public TextMeshProUGUI maxHealthText;   // Max Health
    public TextMeshProUGUI hpRegenText;     // HP Regen
    public TextMeshProUGUI armorText;       // Armor
    public TextMeshProUGUI evasionText;     // Evasion
    public TextMeshProUGUI lifestealText;   // Lifesteal
    public TextMeshProUGUI thornsText;      // Thorns

    [Header("--- SALDIRI STATLARI ---")]
    public TextMeshProUGUI damageText;      // Damage Multiplier
    public TextMeshProUGUI critChanceText;  // Crit Chance
    public TextMeshProUGUI critDamageText;  // Crit Damage
    public TextMeshProUGUI attackSpeedText; // Attack Speed

    [Header("--- MERMÝ / PROJEKTÝL ---")]
    public TextMeshProUGUI projCountText;   // Projectile Count
    public TextMeshProUGUI projBounceText;  // Projectile Bounce
    public TextMeshProUGUI sizeText;        // Size
    public TextMeshProUGUI durationText;    // Duration

    [Header("--- HAREKET VE FÝZÝK ---")]
    public TextMeshProUGUI moveSpeedText;   // Move Speed
    public TextMeshProUGUI extraJumpsText;  // Extra Jumps

    [Header("--- EKONOMÝ VE ÞANS ---")]
    public TextMeshProUGUI luckText;        // Luck
    public TextMeshProUGUI curseText;       // Curse
    public TextMeshProUGUI magnetText;      // Magnet Range
    public TextMeshProUGUI xpBonusText;     // XP Bonus
    public TextMeshProUGUI goldBonusText;   // Gold Bonus

    [Header("--- YETENEK VE DÝÐER ---")]
    public TextMeshProUGUI cooldownText;    // Cooldown Reduction
    public TextMeshProUGUI revivalsText;    // Revivals

    // =========================================================
    // --- 2. SOL TARAF: ENVANTER (ITEMLER) ---
    // =========================================================
    [Header("--- ENVANTER AYARLARI ---")]
    public Transform itemsGridParent; // Grid Layout Group olan obje
    public GameObject itemIconPrefab; // Ýkon Prefabý

    [Header("--- TOOLTIP AYARLARI (YENÝ) ---")]
    public GameObject tooltipPanel;       // Açýlacak küçük pencere
    public TextMeshProUGUI tooltipName;   // Ýtem ismi texti
    public TextMeshProUGUI tooltipDesc;   // Açýklama texti

    // Script Referanslarý
    private PlayerStats stats;
    private PlayerInventory inventory;

    void Awake()
    {
        // TooltipTrigger'ýn bu scripte ulaþabilmesi için Singleton yapýyoruz
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        // Scriptleri bul
        if (stats == null) stats = FindAnyObjectByType<PlayerStats>();
        if (inventory == null) inventory = FindAnyObjectByType<PlayerInventory>();

        // Güncellemeleri yap
        if (stats != null) UpdateStatsUI();
        if (inventory != null) UpdateItemsUI();

        // Menü açýldýðýnda tooltip kapalý baþlasýn
        HideTooltip();
    }

    // --- TOOLTIP FONKSÝYONLARI ---
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
    // -----------------------------

    void UpdateStatsUI()
    {
        // SAVUNMA
        if (maxHealthText) maxHealthText.text = $"Can: {Mathf.Round(stats.CurrentMaxHealth)}";
        if (hpRegenText) hpRegenText.text = $"Yenileme: {stats.CurrentHpRegen:F1}/sn";
        if (armorText) armorText.text = $"Zýrh: {Mathf.Round(stats.CurrentArmor)}";
        if (evasionText) evasionText.text = $"Kaçýnma: %{stats.CurrentEvasion:F1}";
        if (lifestealText) lifestealText.text = $"Can Çalma: %{stats.CurrentLifeSteal}";
        if (thornsText) thornsText.text = $"Diken: {stats.CurrentThorns}";

        // SALDIRI
        if (damageText) damageText.text = $"Hasar: %{Mathf.Round(stats.CurrentDamageMultiplier)}";
        if (critChanceText) critChanceText.text = $"Kritik Þans: %{stats.CurrentCritChance:F1}";
        if (critDamageText) critDamageText.text = $"Kritik Hasar: %{Mathf.Round(stats.CurrentCritDamage)}";
        if (attackSpeedText) attackSpeedText.text = $"Saldýrý Hýzý: %{Mathf.Round(stats.CurrentAttackSpeedMultiplier)}";

        // MERMÝ
        if (projCountText) projCountText.text = $"Ekstra Mermi: +{stats.CurrentProjectileCountBonus}";
        if (projBounceText) projBounceText.text = $"Sekme: +{stats.CurrentProjectileBounce}";
        if (sizeText) sizeText.text = $"Alan Boyutu: %{Mathf.Round(stats.CurrentSizeMultiplier)}";
        if (durationText) durationText.text = $"Süre: %{Mathf.Round(stats.CurrentDurationMultiplier)}";

        // HAREKET
        if (moveSpeedText) moveSpeedText.text = $"Hýz: {stats.CurrentMoveSpeed:F1}";
        if (extraJumpsText) extraJumpsText.text = $"Zýplama: +{stats.CurrentExtraJumps}";

        // EKONOMÝ / ÞANS
        if (luckText) luckText.text = $"Þans: {stats.CurrentLuck}";
        if (curseText) curseText.text = $"Lanet: {stats.CurrentCurse}";
        if (magnetText) magnetText.text = $"Mýknatýs: {stats.CurrentMagnetRange:F1}m";
        if (xpBonusText) xpBonusText.text = $"XP Bonusu: %{Mathf.Round(stats.CurrentXpBonus)}";
        if (goldBonusText) goldBonusText.text = $"Altýn Bonusu: %{Mathf.Round(stats.CurrentGoldBonus)}";

        // DÝÐER
        if (cooldownText) cooldownText.text = $"Bekleme Süresi: -%{Mathf.Round(stats.CurrentSkillCooldownReduction)}";
        if (revivalsText) revivalsText.text = $"Dirilme Hakký: {stats.CurrentRevivals}";
    }

    void UpdateItemsUI()
    {
        if (itemsGridParent == null || itemIconPrefab == null) return;

        foreach (Transform child in itemsGridParent) Destroy(child.gameObject);

        // Listeyi benzersiz itemlere göre grupla (LINQ kullanarak)
        var groupedItems = inventory.ownedItems
            .GroupBy(i => i.itemName)
            .Select(group => new { Data = group.First(), Count = group.Count() });

        foreach (var itemGroup in groupedItems)
        {
            GameObject newIconObj = Instantiate(itemIconPrefab, itemsGridParent);
            Image imgComponent = newIconObj.GetComponent<Image>();

            // Eðer ItemData'da deðiþken adý 'Icon' ise burayý 'itemGroup.Data.Icon' yapýn
            if (itemGroup.Data.icon != null)
                imgComponent.sprite = itemGroup.Data.icon;

            // Sayaç Yazýsý
            TextMeshProUGUI qText = newIconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (qText != null)
            {
                qText.text = itemGroup.Count > 1 ? $"x{itemGroup.Count}" : "";
            }

            // TOOLTIP SÝSTEMÝ
            // ItemTooltipTrigger scriptinin projenizde var olduðundan emin olun
            ItemTooltipTrigger tooltip = newIconObj.AddComponent<ItemTooltipTrigger>();
            tooltip.description = itemGroup.Data.description;
            tooltip.itemName = itemGroup.Data.itemName;
        }
    }
}