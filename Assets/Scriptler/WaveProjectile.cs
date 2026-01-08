using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class WaveProjectile : MonoBehaviour
{
    // --- Variables set by Setup ---
    private int damage;
    private float critChance;
    private float critMultiplier;
    private float speed;
    private float width;
    private float lifetime;
    private int pierceCount;
    // --- End of Setup Variables ---

    private Rigidbody rb;
    private List<EnemyStats> hitEnemies = new List<EnemyStats>();

    public void Setup(int dmg, float chance, float critMulti, float spd, float w, float life, int pierce)
    {
        this.damage = dmg;
        this.critChance = chance;
        this.critMultiplier = critMulti;
        this.speed = spd;
        this.width = w;
        this.lifetime = life;
        this.pierceCount = pierce;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        // SENÝN ÇÖZÜMÜN: BoxCollider hatasýný çözdüðünü söyledin.
        // Burayý olduðu gibi býrakýyorum, sadece geniþliði ayarlýyoruz.
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.size = new Vector3(width, boxCollider.size.y, boxCollider.size.z);
            boxCollider.isTrigger = true;
        }
        // Eðer BoxCollider yoksa bile hata verdirtmiyorum, mevcut collider neyse onu kullanýr.
        else
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);

        hitEnemies.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        // DÜZELTME: Hem Layer'ý ("EnemyHitbox") hem de Tag'i ("Enemy") kontrol ediyoruz.
        // Biri yanlýþsa diðeri yakalar.
        if (other.CompareTag("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
        {
            // DÜZELTME: Script, collider'ýn olduðu objede deðil de onun babasýnda olabilir.
            // Bu yüzden "GetComponentInParent" kullanmak en güvenlisidir.
            EnemyStats enemy = other.GetComponentInParent<EnemyStats>();

            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                bool isCritical = Random.value * 100 < critChance;
                int finalDamage = this.damage;

                if (isCritical)
                {
                    finalDamage = Mathf.RoundToInt(this.damage * this.critMultiplier);
                }

                // Hasarý ver
                enemy.TakeDamage(finalDamage);

                // Vurulanlar listesine ekle
                hitEnemies.Add(enemy);
                pierceCount--;

                if (pierceCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}