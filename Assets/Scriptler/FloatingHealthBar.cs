using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Caný gösterilecek Slider.")]
    [SerializeField] private Slider healthSlider;

    [Tooltip("Karakterin PlayerHealth scripti.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Barýn her zaman bakacaðý kamera (Genelde Main Camera).")]
    [SerializeField] private Camera mainCamera;

    void Start()
    {
        // Otomatik bulmaya çalýþalým (Eðer elle atanmadýysa)
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();

        // Event'e abone ol (Can deðiþince UpdateHealthBar çalýþsýn)
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthUI;
            // Ýlk açýlýþta barý güncelle
            UpdateHealthUI();
        }
    }

    // LateUpdate, kamera hareketinden sonra çalýþýr, titremeyi önler.
    void LateUpdate()
    {
        // Barýn yüzünü kameraya döndür (Billboarding)
        if (mainCamera != null)
        {
            // Canvas'ýn kameraya bakmasýný saðla
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }

    private void UpdateHealthUI()
    {
        if (playerHealth != null && healthSlider != null)
        {
            // PlayerHealth scriptinden can deðerlerini çekiyoruz
            // PlayerHealth scriptinde CurrentHealth float, Stats'ta MaxHealth float olduðu varsayýlýyor.

            // Eðer PlayerStats'a eriþim public ise:
            // (PlayerHealth scriptine 'CurrentMaxHealth' özelliði eklemiþtik diye hatýrlýyorum, yoksa PlayerStats üzerinden alýrýz)

            // Oraný hesapla (0 ile 1 arasýnda)
            // PlayerHealth scriptinde "playerStats" private olduðu için
            // PlayerHealth içine "public float MaxHealth => playerStats.CurrentMaxHealth;" gibi bir getter eklemiþ olabilirsin.
            // Ama biz þimdilik PlayerHealth.cs'deki CurrentHealth'i kullanacaðýz.

            // NOT: PlayerHealth scriptinde CurrentMaxHealth'e dýþarýdan ulaþmak için bir yol olmalý.
            // Önceki scriptinde 'CurrentHealth' public'ti. Max can için PlayerStats'a ihtiyacýmýz var.
            // En temizi PlayerHealth'e gidip 'GetMaxHealth()' fonksiyonu veya property'si eklemektir.
            // Ancak þimdilik PlayerHealth üzerinden Stats'a eriþmeye çalýþalým veya PlayerHealth'in kendisinden okuyalým.

            // Basit matematik: Þuanki Can / Maksimum Can
            // Bu kýsým senin PlayerHealth scriptindeki deðiþkenlerin eriþim seviyesine göre hata verebilir.
            // Çözüm aþaðýda.

            float current = playerHealth.CurrentHealth;
            // Max health'i bulmak için PlayerStats componentini ayný objeden çekelim:
            var stats = playerHealth.GetComponent<PlayerStats>();
            float max = (stats != null) ? stats.CurrentMaxHealth : 100f;

            healthSlider.value = current / max;
        }
    }

    void OnDestroy()
    {
        // Obje yok olunca eventten çýkmayý unutma (Hata vermemesi için)
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthUI;
        }
    }
}