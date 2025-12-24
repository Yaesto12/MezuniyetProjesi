using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("UI Referansý")]
    [Tooltip("Sürenin yazacaðý TextMeshPro objesi")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Ayarlar")]
    [Tooltip("Oyun baþlar baþlamaz sayaç çalýþsýn mý?")]
    [SerializeField] private bool startImmediately = true;

    // Geçen süreyi dýþarýdan okumak istersen (örn: rekor sistemi için)
    public float CurrentTime { get; private set; }

    private bool isRunning = false;

    void Start()
    {
        CurrentTime = 0f;
        if (startImmediately)
        {
            isRunning = true;
        }
    }

    void Update()
    {
        if (isRunning)
        {
            // Zamaný artýr
            CurrentTime += Time.deltaTime;

            // Ekranda göster
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        // Saniyeyi Dakika:Saniye formatýna çevir
        // Mathf.FloorToInt tam sayýya yuvarlar
        int minutes = Mathf.FloorToInt(CurrentTime / 60);
        int seconds = Mathf.FloorToInt(CurrentTime % 60);

        // {0:00} -> Sayý tek haneli olsa bile baþýna 0 koy (örn: 5 -> 05)
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Oyuncu ölünce veya oyun bitince bunu çaðýrabilirsin
    public void StopTimer()
    {
        isRunning = false;
    }
}