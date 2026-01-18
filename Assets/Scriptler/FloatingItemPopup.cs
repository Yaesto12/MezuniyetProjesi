using UnityEngine;
using UnityEngine.UI;

public class FloatingItemPopup : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Yukarý çýkýþ hýzý")]
    [SerializeField] private float floatSpeed = 1.5f;

    [Tooltip("Kaç saniye sonra yok olacak?")]
    [SerializeField] private float destroyTime = 1.5f;

    [Tooltip("Yukarý çýkarken ayný zamanda büyüsün mü?")]
    [SerializeField] private Vector3 startScale = Vector3.zero;
    [SerializeField] private Vector3 targetScale = Vector3.one;

    [Header("Referanslar")]
    [SerializeField] private Image iconImage; // Ýkonu gösterecek UI Image

    private float timer = 0f;
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        transform.localScale = startScale; // Küçük baþla
    }

    // Bu fonksiyonu sandýk scriptinden çaðýracaðýz
    public void Initialize(Sprite itemSprite)
    {
        if (iconImage != null)
        {
            iconImage.sprite = itemSprite;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 1. YUKARI HAREKET
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 2. KAMERAYA BAKMA (Billboard Effect)
        if (mainCam != null)
        {
            // Kameranýn rotasyonunu kopyala (En temiz billboard yöntemidir)
            transform.rotation = mainCam.transform.rotation;
        }

        // 3. BÜYÜME VE YOK OLMA EFEKTÝ
        float progress = timer / destroyTime;

        // Ýlk %20'lik kýsýmda büyüsün (Pop-up hissi)
        if (progress < 0.2f)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress * 5f);
        }

        // Son %20'lik kýsýmda þeffaflaþsýn (Fade out)
        if (progress > 0.8f)
        {
            if (iconImage != null)
            {
                Color c = iconImage.color;
                c.a = Mathf.Lerp(1f, 0f, (progress - 0.8f) * 5f);
                iconImage.color = c;
            }
        }

        // Süre dolunca yok et
        if (timer >= destroyTime)
        {
            Destroy(gameObject);
        }
    }
}