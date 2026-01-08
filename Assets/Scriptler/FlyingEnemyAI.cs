using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyStats))]
public class FlyingEnemyAI : MonoBehaviour
{
    [Header("Uçuþ Ayarlarý")]
    [SerializeField] private float hoverHeight = 3.5f;
    [SerializeField] private float attackStartDistance = 6.0f;

    [Header("Saldýrý Ayarlarý")]
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("Fizik Ayarlarý")]
    [Tooltip("Kuþ vurduðunda karakteri ne kadar geriye fýrlatsýn?")]
    [SerializeField] private float knockbackForce = 15f;

    private Rigidbody rb;
    private Transform player;
    private EnemyStats stats;

    private bool canAttack = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<EnemyStats>();

        rb.useGravity = false;
        // rb.linearDamping = 1f; // Unity sürümüne göre 'drag' veya 'linearDamping' olabilir. Hata verirse 'drag' yap.
    }

    void Start() // Player'ý Start'ta bulmak daha güvenlidir
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;
        HandleFlightMovement();
    }

    private void HandleFlightMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 targetPosition;

        if (distanceToPlayer > attackStartDistance)
            targetPosition = player.position + Vector3.up * hoverHeight;
        else
            targetPosition = player.position;

        Vector3 direction = (targetPosition - transform.position).normalized;

        // Unity 6 öncesi için 'velocity', Unity 6 sonrasý için 'linearVelocity'
        // Eðer hata alýrsan burayý rb.velocity = ... yap
        rb.linearVelocity = direction * stats.Speed;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, stats.Speed * Time.fixedDeltaTime));
        }
    }

    // Katý çarpýþmalar için
    private void OnCollisionStay(Collision collision)
    {
        CheckAndAttack(collision.gameObject);
    }

    // Trigger çarpýþmalarý için (YENÝ EKLENDÝ)
    private void OnTriggerStay(Collider other)
    {
        CheckAndAttack(other.gameObject);
    }

    // Ortak saldýrý fonksiyonu
    private void CheckAndAttack(GameObject targetObj)
    {
        // Oyuncu mu ve saldýrý süresi doldu mu?
        if (targetObj.CompareTag("Player") && canAttack)
        {
            // 1. Hasar Ver
            var health = targetObj.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(stats.Damage);
            }

            // 2. Knockback Uygula
            var moveController = targetObj.GetComponent<PlayerController>();
            if (moveController != null)
            {
                Vector3 pushDir = (targetObj.transform.position - transform.position).normalized;
                moveController.ApplyKnockback(pushDir, knockbackForce);
            }

            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}