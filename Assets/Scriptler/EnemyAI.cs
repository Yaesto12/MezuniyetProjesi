using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyAI : MonoBehaviour
{
    [Header("Saldýrý Ayarlarý")]
    [SerializeField] private float attackCooldown = 2.0f;

    // --- SES AYARLARI (YENÝ) ---
    [Header("Ses Ayarlarý")]
    [Tooltip("Düþman oyuncuya saldýrdýðýnda çalacak ses.")]
    [SerializeField] private AudioClip attackSound;
    [Range(0f, 1f)][SerializeField] private float attackVolume = 0.6f;
    // ----------------------------

    private Rigidbody rb;
    private Transform player;
    private EnemyStats stats;
    private bool canAttack = true;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<EnemyStats>();

        // Ses için hoparlör ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D Ses

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Sahnede 'Player' etiketli bir obje bulunamadý! AI devre dýþý býrakýlýyor.", this);
            enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 directionToPlayer = (player.position - rb.position).normalized;
        directionToPlayer.y = 0;

        Vector3 targetVelocity = directionToPlayer * stats.Speed;

        // Mevcut dikey hýzý koru
        targetVelocity.y = rb.linearVelocity.y;

        // Hýzý uygula
        rb.linearVelocity = targetVelocity;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            rb.MoveRotation(targetRotation);
        }
    }

    public void NotifyPlayerContact()
    {
        if (canAttack)
        {
            // --- SALDIRI SESÝ ---
            if (attackSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(attackSound, attackVolume);
            }
            // --------------------

            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(stats.Damage);
            }
            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Eðer oyuncuya deðiyorsak ve saldýrabiliyorsak
        if (collision.gameObject.CompareTag("Player") && canAttack)
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(stats.Damage);
                NotifyPlayerContact(); // Bu fonksiyon zaten sesi çalýp cooldown baþlatýyor
            }
        }
    }
}