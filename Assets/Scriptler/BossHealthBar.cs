using UnityEngine;
using UnityEngine.UI;

public class BossUIManager : MonoBehaviour
{
    // Her yerden ulaþabilmek için Singleton yapýyoruz
    public static BossUIManager Instance { get; private set; }

    [Header("UI Bileþenleri")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject barContainer; // Slider'ýn kendisi (Açýp kapatmak için)

    void Awake()
    {
        // Singleton Kurulumu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Eðer slider atanmamýþsa kendi üzerindekini al
        if (healthSlider == null) healthSlider = GetComponent<Slider>();
        if (barContainer == null) barContainer = gameObject;

        // Baþlangýçta gizle
        HideBossBar();
    }

    // Boss doðduðunda bu fonksiyonu çaðýracak
    public void ShowBossBar(float currentHealth, float maxHealth)
    {
        barContainer.SetActive(true);
        UpdateHealth(currentHealth, maxHealth);
    }

    // Boss hasar aldýðýnda bunu çaðýracak
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        // 0 ile 1 arasýnda bir oran buluyoruz
        healthSlider.value = currentHealth / maxHealth;
    }

    // Boss öldüðünde bunu çaðýracak
    public void HideBossBar()
    {
        barContainer.SetActive(false);
    }
}