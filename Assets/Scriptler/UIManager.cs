using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Singleton Deseni
    public static UIManager Instance { get; private set; }

    [Header("Player UI Referansları (Sahneden Atanacak)")]
    public Slider xpBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI priorityText;

    [Header("Kill Sayaç")]
    public TextMeshProUGUI killCountText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // --- Mevcut UI Güncelleme Metotları ---

    public void UpdateLevelText(int level)
    {
        if (levelText != null) levelText.text = level.ToString();
    }

    public void UpdateXpBar(float currentXp, float xpToNextLevel)
    {
        if (xpBar != null && xpToNextLevel > 0)
        {
            xpBar.value = Mathf.Clamp01(currentXp / xpToNextLevel);
        }
    }

    public void UpdateGoldText(int currentGold)
    {
        if (goldText != null) goldText.text = "x " + currentGold.ToString();
    }

    public void UpdatePriorityText(string text)
    {
        if (priorityText != null) priorityText.text = "Hedef: " + text;
    }

    // --- DÜZELTİLEN KISIM ---
    public void UpdateKillCount(int count)
    {
        if (killCountText != null)
        {
            // Simgeyi kaldırdık, yerine düz yazı yazdık.
            // Artık kutu olarak gözükmeyecektir.
            killCountText.text = "Kills: " + count.ToString();
        }
    }
}