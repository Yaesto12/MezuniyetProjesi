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
        rb.linearDamping = 1f;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else enabled = false;
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
        rb.linearVelocity = direction * stats.Speed;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, stats.Speed * Time.fixedDeltaTime));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && canAttack)
        {
            // 1. Hasar Ver (Health Script)
            var health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(stats.Damage);
            }

            // 2. Knockback Uygula (Controller Script - YENÝ YÖNTEM)
            // Karakterin hareket scriptine ulaþýyoruz
            var moveController = collision.gameObject.GetComponent<PlayerController>();

            if (moveController != null)
            {
                // Kuþtan Oyuncuya doðru olan yönü bul
                Vector3 pushDir = (collision.transform.position - transform.position).normalized;

                // PlayerController içindeki yeni fonksiyonu çaðýr
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