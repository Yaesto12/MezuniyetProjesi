using UnityEngine;
using UnityEngine.UI; // Image ve Slider kontrolü için şart
using TMPro;

public class UIManager : MonoBehaviour
{
    // Singleton Deseni
    public static UIManager Instance { get; private set; }

    [Header("Player UI Referansları (Sahneden Atanacak)")]

    // --- DEĞİŞİKLİK 1: XP Slider'ı iptal, yerine Image (Mask) geldi ---
    // public Slider xpBar; // <-- ESKİ KOD
    public Image xpBarMaskImage; // <-- YENİ KOD (XP Barının 'Mask' objesini buraya sürükle)
    // -----------------------------------------------------------------

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI priorityText;

    [Header("Kill Sayaç")]
    public TextMeshProUGUI killCountText;

    // --- YENİ EKLENENLER (SOL ÜST KÖŞE HUD) ---
    [Header("Player HUD (Sol Üst)")]
    public Image playerIconImage;      // Karakterin vesikalık fotosu

    // Can Barı için Maskeleme Image'i
    public Image healthBarMaskImage;

    public TextMeshProUGUI healthText; // (Opsiyonel) Barın üzerindeki sayı (150/200)

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
        if (levelText != null)
        {
            // "Level: " yazısının sonuna boşluk koymayı unutma,
            // yoksa "Level:1" gibi bitişik durur.
            levelText.text = "Level: " + level.ToString();
        }
    }

    // --- DEĞİŞİKLİK 2: Fonksiyonun içi Image fillAmount'a göre ayarlandı ---
    public void UpdateXpBar(float currentXp, float xpToNextLevel)
    {
        // Slider yerine Image'in fillAmount özelliğini değiştiriyoruz
        if (xpBarMaskImage != null && xpToNextLevel > 0)
        {
            // 0 ile 1 arasında oranla
            xpBarMaskImage.fillAmount = Mathf.Clamp01(currentXp / xpToNextLevel);
        }
    }
    // ----------------------------------------------------------------------

    public void UpdateGoldText(int currentGold)
    {
        if (goldText != null) goldText.text = "x " + currentGold.ToString();
    }

    public void UpdatePriorityText(string text)
    {
        if (priorityText != null) priorityText.text = "Hedef: " + text;
    }

    public void UpdateKillCount(int count)
    {
        if (killCountText != null)
        {
            killCountText.text = "Kills: " + count.ToString();
        }
    }

    // --- YENİ EKLENEN FONKSİYONLAR ---

    // 1. Karakter İkonunu Ayarla (Oyun Başında Çalışır)
    public void SetupPlayerHUD(CharacterData charData)
    {
        if (playerIconImage != null && charData != null)
        {
            playerIconImage.sprite = charData.icon;
            // Resmi sündürmemek için (kare kalması için):
            // playerIconImage.preserveAspect = true;
        }
    }

    // 2. Can Barını Güncelle (Maskeleme Sistemi)
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarMaskImage != null)
        {
            healthBarMaskImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)} / {Mathf.Ceil(maxHealth)}";
        }
    }
}