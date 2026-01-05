using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;

    // Hareket Ayarlarý
    private const float DISAPPEAR_TIMER_MAX = 1f;
    private Vector3 moveVector;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount, bool isCritical)
    {
        textMesh.text = damageAmount.ToString();

        if (!isCritical)
        {
            // Normal Vuruþ
            textMesh.fontSize = 6;
            textColor = Color.yellow; // Veya beyaz
        }
        else
        {
            // Kritik Vuruþ
            textMesh.fontSize = 9; // Daha büyük
            textMesh.text += "!"; // Ünlem ekle
            textColor = Color.red; // Kýrmýzý yap
        }

        textMesh.color = textColor;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        // Yukarý ve rastgele saða/sola hareket vektörü (Daha doðal durur)
        moveVector = new Vector3(Random.Range(-1f, 1f), 2f, 0) * 5f; // Hýz çarpaný
    }

    private void Update()
    {
        // 1. Yukarý Hareket
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime; // Hýzla yavaþlasýn (sürtünme etkisi)

        // 2. Kameraya Dön (Billboard)
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        // 3. Yok Olma (Fade Out)
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Yazýnýn þeffaflýðýný (Alpha) azalt
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}