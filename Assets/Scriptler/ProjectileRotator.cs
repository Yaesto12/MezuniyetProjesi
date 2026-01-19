using UnityEngine;

public class ProjectileRotator : MonoBehaviour
{
    [Header("--- Dönme Ayarlarý ---")]
    [Tooltip("Hangi eksende ne kadar hýzlý dönecek? (Örn: Z=360 yaparsan saniyede 1 tam tur atar)")]
    public Vector3 rotationSpeed = new Vector3(0, 0, 360);

    [Header("--- Rastgelelik (Opsiyonel) ---")]
    [Tooltip("Oyun baþladýðýnda rastgele bir açýda mý baþlasýn? (Daha doðal durur)")]
    public bool randomizeStartAngle = true;

    [Tooltip("Hýzda rastgelelik olsun mu? (Her fýrlatýþta farklý hýz)")]
    public bool randomizeSpeed = false;
    public Vector2 speedMultiplierRange = new Vector2(0.8f, 1.5f);

    private Vector3 currentSpeed;

    void Start()
    {
        currentSpeed = rotationSpeed;

        // Rastgele baþlangýç açýsý (Z ekseni için genelde en iyisidir)
        if (randomizeStartAngle)
        {
            float randomZ = Random.Range(0f, 360f);
            transform.Rotate(0, 0, randomZ);
        }

        // Rastgele hýz çarpaný
        if (randomizeSpeed)
        {
            float mult = Random.Range(speedMultiplierRange.x, speedMultiplierRange.y);
            currentSpeed *= mult;
        }
    }

    void Update()
    {
        // DeltaTime ile çarparak kare hýzýndan baðýmsýz (smooth) dönme saðlarýz
        transform.Rotate(currentSpeed * Time.deltaTime);
    }
}