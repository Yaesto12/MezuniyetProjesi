using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))] // Collider'ýn IsTrigger=true olmalý
public class Projectile : MonoBehaviour
{
    // Silah script'i tarafýndan doldurulacak olan özellikler
    private int damage;
    private float critChance;
    private float critMultiplier;
    private Rigidbody rb;

    /// <summary>
    /// Handler tarafýndan çaðrýlýr, mermiyi ayarlar ve fýrlatýr.
    /// </summary>
    public void Setup(int baseDamage, float baseCritChance, float baseCritMultiplier, float projectileSpeed, Vector3 scale, float lifetime)
    {
        this.damage = baseDamage;
        this.critChance = baseCritChance;
        this.critMultiplier = baseCritMultiplier;
        transform.localScale = scale; // Boyutu ayarla

        rb = GetComponent<Rigidbody>();
        // Rigidbody ayarlarýný koddan da garanti edelim
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Daha akýcý hareket için (opsiyonel)
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // Hýzlý mermiler için önerilir

        // Fýrlat (Unity'nin yeni API'si ile)
        rb.linearVelocity = transform.forward * projectileSpeed;

        // Belirtilen süre sonra yok et
        Destroy(gameObject, lifetime);
    }

    // Bir trigger alanýna girdiðinde bu fonksiyon çalýþýr.
    private void OnTriggerEnter(Collider other)
    {
        // Düþmanýn Hitbox katmanýna mý çarptýk?
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
        {
            // Çarptýðýmýz objenin üst objelerinden EnemyStats script'ini ara
            EnemyStats enemy = other.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                // Kritik vuruþ hesapla
                bool isCritical = Random.value * 100 < critChance;
                int finalDamage = this.damage;

                if (isCritical)
                {
                    finalDamage = Mathf.RoundToInt(this.damage * this.critMultiplier);
                    // Debug.Log("Kritik Mermi Vuruþu!"); // Ýsteðe baðlý log
                }

                // Düþmana hasar ver
                enemy.TakeDamage(finalDamage);

                // Düþmana çarptýktan sonra mermiyi yok et
                Destroy(gameObject);
            }
        }
        // Opsiyonel: Merminin duvar gibi baþka þeylere çarpýnca da yok olmasý
        // else if (other.gameObject.CompareTag("Environment") || other.gameObject.layer == LayerMask.NameToLayer("Ground")) // Katman veya Tag kullanabilirsiniz
        // {
        //     Destroy(gameObject);
        // }
    }
}