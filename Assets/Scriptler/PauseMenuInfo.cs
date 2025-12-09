using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PauseMenuInfo : MonoBehaviour
{
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

    // Script Referanslarý
    private PlayerStats stats;
    private PlayerInventory inventory;

    void OnEnable()
    {
        // Scriptleri bul
        if (stats == null) stats = FindAnyObjectByType<PlayerStats>();
        if (inventory == null) inventory = FindAnyObjectByType<PlayerInventory>();

        // Güncellemeleri yap
        if (stats != null) UpdateStatsUI();
        if (inventory != null) UpdateItemsUI();
    }

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

        // Eski ikonlarý temizle
        foreach (Transform child in itemsGridParent) Destroy(child.gameObject);

        // Yeni ikonlarý diz
        foreach (ItemData item in inventory.ownedItems)
        {
            GameObject newIconObj = Instantiate(itemIconPrefab, itemsGridParent);
            Image imgComponent = newIconObj.GetComponent<Image>();

            if (item.icon != null) imgComponent.sprite = item.icon;
            else imgComponent.color = new Color(1, 1, 1, 0.5f);
        }
    }
}