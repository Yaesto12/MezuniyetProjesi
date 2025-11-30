using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Singleton Deseni
    public static UIManager Instance { get; private set; }

    [Header("Player UI Referanslarý (Sahneden Atanacak)")]
    public Slider xpBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI priorityText; // Targeting System için

    // --- Hedef Göstergesi ile ilgili HER ÞEY KALDIRILDI ---
    // public GameObject targetIndicatorPrefab; // KALDIRILDI
    // private RectTransform currentTargetIndicatorInstance; // KALDIRILDI
    // InitializeTargetIndicator() metodu KALDIRILDI
    // SetTargetIndicatorActive() metodu KALDIRILDI
    // UpdateTargetIndicatorPosition() metodu KALDIRILDI
    // --- Hedef Göstergesi Bitti ---


    void Awake()
    {
        // Singleton kurulumu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Opsiyonel
        }
        // Hedef göstergesi oluþturma kodu KALDIRILDI
    }

    // --- Mevcut UI Güncelleme Metotlarý ---

    public void UpdateLevelText(int level)
    {
        if (levelText != null) levelText.text = level.ToString();
        else Debug.LogError("UIManager: Level Text atanmamýþ!");
    }

    public void UpdateXpBar(float currentXp, float xpToNextLevel)
    {
        if (xpBar != null && xpToNextLevel > 0)
        {
            xpBar.value = Mathf.Clamp01(currentXp / xpToNextLevel);
        }
        else if (xpBar == null) Debug.LogError("UIManager: XP Bar atanmamýþ!");
    }

    public void UpdateGoldText(int currentGold)
    {
        if (goldText != null) goldText.text = "x " + currentGold.ToString();
        else Debug.LogError("UIManager: Gold Text atanmamýþ!");
    }

    public void UpdatePriorityText(string text)
    {
        if (priorityText != null) priorityText.text = "Hedef: " + text;
        else Debug.LogError("UIManager: Priority Text atanmamýþ!");
    }

    // Hedef göstergesi metotlarý KALDIRILDI
}