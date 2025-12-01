using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))] // Collider'ýn IsTrigger=true olmalý
public class Projectile : MonoBehaviour
{
    private int damage;
    private float critChance;
    private float critMultiplier;
    private Rigidbody rb;

    // --- YENÝ EKLENENLER (Kanama Ýçin) ---
    private float bleedPercent;
    private float critBleedPercent;
    // -------------------------------------

    /// <summary>
    /// Handler tarafýndan çaðrýlýr, mermiyi ayarlar ve fýrlatýr.
    /// </summary>
    public void Setup(
        int baseDamage,
        float baseCritChance,
        float baseCritMultiplier,
        float projectileSpeed,
        Vector3 scale,
        float lifetime,
        float bleed,      // 7. Parametre
        float critBleed   // 8. Parametre
    )
    {
        this.damage = baseDamage;
        this.critChance = baseCritChance;
        this.critMultiplier = baseCritMultiplier;

        // Kanama deðerlerini al
        this.bleedPercent = bleed;
        this.critBleedPercent = critBleed;

        transform.localScale = scale; // Boyutu ayarla

        rb = GetComponent<Rigidbody>();
        // Rigidbody ayarlarýný koddan da garanti edelim
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

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
                }

                // --- KANAMA MANTIÐI ---
                float totalBleed = bleedPercent;
                if (isCritical) totalBleed += critBleedPercent;

                if (totalBleed > 0)
                {
                    int bleedDamage = Mathf.RoundToInt(finalDamage * (totalBleed / 100f));
                    // Düþmana kanama uygula (3 saniye sürecek þekilde)
                    enemy.ApplyBleed(bleedDamage, 3f);
                }
                // ----------------------

                // Düþmana hasar ver
                enemy.TakeDamage(finalDamage);

                if (GameEventManager.Instance != null)
                {
                    GameEventManager.Instance.TriggerEnemyHit(enemy, finalDamage, isCritical);
                }

                // Düþmana çarptýktan sonra mermiyi yok et
                Destroy(gameObject);
            }
        }
    }
}