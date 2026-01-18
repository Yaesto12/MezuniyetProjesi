using UnityEngine;
using UnityEngine.UI; // Image kontrolü için þart

public class DamageFlash : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Hasar alýnca ekran ne renk olsun? (Alpha'yý 0.5 gibi yap)")]
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    [Tooltip("Kýrmýzýlýk ne kadar hýzlý kaybolsun?")]
    [SerializeField] private float fadeSpeed = 5f;

    [Header("Referans")]
    [SerializeField] private Image flashImage;

    // Singleton: Her yerden kolayca ulaþmak için
    public static DamageFlash Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Baþlangýçta image atanmamýþsa otomatik bulmaya çalýþ
        if (flashImage == null) flashImage = GetComponent<Image>();
    }

    void Update()
    {
        // Eðer ekranda kýrmýzýlýk varsa (Alpha > 0), yavaþça yok et
        if (flashImage != null && flashImage.color.a > 0)
        {
            Color currentColor = flashImage.color;

            // --- DÜZELTME BURADA YAPILDI ---
            // Time.deltaTime yerine Time.unscaledDeltaTime kullandýk.
            // Bu sayede oyun dursa (Pause olsa veya ölünce) bile efekt çalýþmaya devam eder.
            currentColor.a = Mathf.Lerp(currentColor.a, 0f, fadeSpeed * Time.unscaledDeltaTime);
            // -------------------------------

            flashImage.color = currentColor;
        }
    }

    // Bu fonksiyonu çaðýrýnca ekran kýrmýzý yanar
    public void TriggerFlash()
    {
        if (flashImage != null)
        {
            flashImage.color = flashColor;
        }
    }
}