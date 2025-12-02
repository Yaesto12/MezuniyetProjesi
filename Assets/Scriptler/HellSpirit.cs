using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))] // Trigger olmalý
public class HellSpirit : MonoBehaviour
{
    private int damage;
    private float moveSpeed;
    private float detectionRange;
    private float lifeTime;
    private Transform playerTransform; // Etrafýnda dolaþmak için

    private Transform targetEnemy;
    private float wanderTimer;
    private Vector3 wanderTarget;

    public void Setup(int dmg, float speed, float range, float life, Transform player)
    {
        this.damage = dmg;
        this.moveSpeed = speed;
        this.detectionRange = range;
        this.lifeTime = life;
        this.playerTransform = player;

        // Yerçekimini kapat (Ruhlar uçar)
        GetComponent<Rigidbody>().useGravity = false;

        // Belirli süre sonra yok ol (Düþman bulamazsa)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 1. Hedef Belirleme
        if (targetEnemy == null)
        {
            FindNearestEnemy();
        }

        // 2. Hareket Mantýðý
        if (targetEnemy != null)
        {
            // Düþmana doðru uç (Homing)
            MoveTowards(targetEnemy.position);
        }
        else
        {
            // Düþman yoksa oyuncu etrafýnda rastgele dolaþ (Wander)
            WanderAroundPlayer();
        }
    }

    private void FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, LayerMask.GetMask("EnemyHitbox")); // EnemyHitbox layerýný kontrol et
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.transform;
            }
        }
        targetEnemy = closest;
    }

    private void MoveTowards(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Yüze bak
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
    }

    private void WanderAroundPlayer()
    {
        // Rastgele bir nokta seç
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            // Oyuncunun etrafýnda rastgele bir küre içinde nokta bul
            Vector3 randomOffset = Random.insideUnitSphere * 3f;
            randomOffset.y = Random.Range(1f, 3f); // Yerden biraz yüksekte olsun
            wanderTarget = playerTransform.position + randomOffset;
            wanderTimer = Random.Range(0.5f, 1.5f); // Yeni hedef seçme sýklýðý
        }

        // O noktaya süzül
        MoveTowards(wanderTarget);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Düþmana çarptý mý?
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
        {
            EnemyStats enemy = other.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                // Hasar ver
                enemy.TakeDamage(damage);

                // Görsel efekt eklenebilir (Instantiate explosion...)

                // Ruhu yok et
                Destroy(gameObject);
            }
        }
    }
}