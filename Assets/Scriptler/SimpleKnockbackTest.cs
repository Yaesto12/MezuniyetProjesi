using UnityEngine;

// Bu script, çalýþmak için bir CharacterController'a ihtiyaç duyar.
[RequireComponent(typeof(CharacterController))]
public class SimpleKnockbackTest : MonoBehaviour
{
    // --- DIÞ KUVVETLER ÝÇÝN DEÐÝÞKENLER ---
    private Vector3 impactForce;
    private CharacterController controller;

    // --- AYARLAR ---
    public float knockbackForce = 25f;  // Test için yüksek bir deðer
    public float knockbackDrag = 5f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Dýþarýdan gelen savrulma kuvvetini zamanla azalt (sürtünme gibi).
        if (impactForce.magnitude > 0.2f)
        {
            impactForce = Vector3.Lerp(impactForce, Vector3.zero, knockbackDrag * Time.deltaTime);
        }
        else
        {
            impactForce = Vector3.zero;
        }

        // Sadece ve sadece dýþarýdan gelen kuvveti uygula. Baþka hiçbir hareket yok.
        controller.Move(impactForce * Time.deltaTime);
    }

    // Bu, CharacterController'ýn bir þeye dokunduðunda çalýþan özel fonksiyondur.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 1. Dokunduðumuz objenin etiketinin "Enemy" olup olmadýðýný kontrol et.
        if (hit.gameObject.CompareTag("Enemy"))
        {
            // 2. Eðer "Enemy" ise, KONSOLA BÜYÜK BÝR MESAJ YAZDIR!
            Debug.LogError("!!! DÜÞMAN TESPÝT EDÝLDÝ! SAVRULMA UYGULANIYOR !!!");

            // 3. Savrulma yönünü ve kuvvetini hesapla.
            Vector3 knockbackDirection = (transform.position - hit.point).normalized;
            Vector3 knockback = knockbackDirection * knockbackForce;
            knockback.y = knockbackForce / 2; // Hafifçe havaya kaldýr

            // 4. Bu kuvveti anýnda uygula.
            impactForce = knockback;
        }
    }
}