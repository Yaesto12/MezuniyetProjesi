using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class BossHealthConnector : MonoBehaviour
{
    private EnemyStats stats;
    private float lastKnownHealth;

    void Start()
    {
        stats = GetComponent<EnemyStats>();

        Debug.Log("BossHealthConnector Baþladý."); // 1. Kontrol

        if (BossUIManager.Instance == null)
        {
            Debug.LogError("HATA: BossUIManager sahnede bulunamadý!"); // Sorun buysa burasý yazar
        }
        else
        {
            Debug.Log("UI Yöneticisi bulundu, Bar açýlýyor..."); // Her þey yolundaysa burasý yazar
            BossUIManager.Instance.ShowBossBar(stats.CurrentHealth, stats.MaxHealth);
        }

        lastKnownHealth = stats.CurrentHealth;
    }

    void Update()
    {
        // Her karede can deðiþti mi diye kontrol et
        // DÜZELTME: stats.CurrentHealth (Büyük harf) kullanýldý.
        if (Mathf.Abs(stats.CurrentHealth - lastKnownHealth) > 0.1f)
        {
            lastKnownHealth = stats.CurrentHealth;

            if (BossUIManager.Instance != null)
            {
                BossUIManager.Instance.UpdateHealth(stats.CurrentHealth, stats.MaxHealth);
            }
        }
    }

    void OnDestroy()
    {
        // Ejderha ölünce veya yok olunca barý kapat
        if (BossUIManager.Instance != null)
        {
            BossUIManager.Instance.HideBossBar();
        }
    }
}