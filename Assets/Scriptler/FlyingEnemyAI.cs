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

    // --- SES AYARLARI (YENÝ) ---
    [Header("Ses Ayarlarý")]
    [Tooltip("Uçan düþman saldýrdýðýnda çalacak ses.")]
    [SerializeField] private AudioClip attackSound;
    [Range(0f, 1f)][SerializeField] private float attackVolume = 0.6f;
    // ----------------------------

    [Header("Fizik Ayarlarý")]
    [Tooltip("Kuþ vurduðunda karakteri ne kadar geriye fýrlatsýn?")]
    [SerializeField] private float knockbackForce = 15f;

    private Rigidbody rb;
    private Transform player;
    private EnemyStats stats;
    private AudioSource audioSource;

    private bool canAttack = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<EnemyStats>();

        // Ses için hoparlör ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D Ses

        rb.useGravity = false;
    }

    void Start()
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

        // Unity 6 uyumlu linearVelocity
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

    // Trigger çarpýþmalarý için
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
            // --- SALDIRI SESÝ ---
            if (attackSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(attackSound, attackVolume);
            }
            // --------------------

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