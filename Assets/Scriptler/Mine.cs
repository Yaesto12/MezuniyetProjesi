using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Mine : MonoBehaviour
{
    // Setup metoduyla ayarlanacak deðerler
    private int damage;
    private float critChance;    // <<<--- EKLENDÝ ---<<<
    private float critMultiplier; // <<<--- EKLENDÝ ---<<<
    private float radius;
    private float lifetime;
    private bool exploded = false;

    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private LayerMask enemyLayer = 1 << 7; // Varsayýlan Enemy katmaný

    /// <summary>
    /// ProximityMineHandler tarafýndan çaðrýlýr, mayýný baþlatýr.
    /// Parametreler artýk Handler'ýn gönderdiðiyle eþleþiyor (5 adet).
    /// </summary>
    public void Setup(int dmg, float chance, float multi, float rad, float life) // <<<--- PARAMETRELER GÜNCELLENDÝ ---<<<
    {
        this.damage = dmg;
        this.critChance = chance;       // <<<--- ATAMA EKLENDÝ ---<<<
        this.critMultiplier = multi;    // <<<--- ATAMA EKLENDÝ ---<<<
        this.radius = rad;
        this.lifetime = life;

        StartCoroutine(FuseCoroutine()); // Patlama sayacýný baþlat
    }

    IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(lifetime);
        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        List<EnemyStats> hitEnemies = new List<EnemyStats>();

        foreach (Collider hit in hits)
        {
            EnemyStats enemy = hit.GetComponentInParent<EnemyStats>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                // Kritik vuruþ hesaplamasý
                bool isCritical = Random.value * 100 < critChance; // <<<--- CRIT KULLANILIYOR ---<<<
                int finalDamage = this.damage;
                if (isCritical)
                {
                    finalDamage = Mathf.RoundToInt(this.damage * this.critMultiplier); // <<<--- CRIT KULLANILIYOR ---<<<
                }

                enemy.TakeDamage(finalDamage);
                hitEnemies.Add(enemy);
            }
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}