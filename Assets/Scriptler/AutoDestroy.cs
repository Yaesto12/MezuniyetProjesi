using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("Bu obje kaç saniye sonra yok olsun?")]
    [SerializeField] private float lifetime = 1.0f; // Varsayýlan 1 saniye

    void Start()
    {
        // Oyun baþladýðý (veya bu obje oluþtuðu) an geri sayýmý baþlat
        // Destroy fonksiyonunun ikinci parametresi gecikme süresidir.
        Destroy(gameObject, lifetime);
    }
}