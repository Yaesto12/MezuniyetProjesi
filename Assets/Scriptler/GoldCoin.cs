using UnityEngine;

public class GoldCoin : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float rotateSpeed = 360f;
    [SerializeField] private float lifeTime = 1f; // 1 saniye sonra yok olsun

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        // Belirlenen süre sonunda kendini yok et (Sadece görsel olduðu için)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Kendi etrafýnda dön
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // Aþaðý yukarý hafifçe yüz (Floating effect)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * 0.5f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // ÖNEMLÝ: OnTriggerEnter veya OnCollisionEnter KULLANMIYORUZ.
    // Çünkü para EnemyStats içinde zaten verildi. Bu sadece bir süs.
}