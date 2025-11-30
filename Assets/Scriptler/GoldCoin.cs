using UnityEngine;

// DÝKKAT: Burada [RequireComponent...] yazan hiçbir satýr OLMAMALI.
public class GoldCoin : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    [Tooltip("Yukarý doðru ne kadar hýzlý süzülecek?")]
    [SerializeField] private float floatSpeed = 2f;
    [Tooltip("Kendi etrafýnda ne kadar hýzlý dönecek?")]
    [SerializeField] private float rotateSpeed = 360f;
    [Tooltip("Ne kadar süre ekranda kalýp yok olacak?")]
    [SerializeField] private float lifeTime = 1.0f;

    // Setup fonksiyonu dýþarýdan çaðrýlmasa bile Start ile kendini yok etsin
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Setup()
    {
        // Ekstra ayarlar gerekirse buraya
    }

    void Update()
    {
        // Sadece görsel hareket (Yukarý çýk ve dön)
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
    }
}