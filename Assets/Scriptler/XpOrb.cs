using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class XpOrb : MonoBehaviour
{
    [Tooltip("Bu top ne kadar XP verecek? (Setup ile atanmazsa varsayýlan)")]
    [SerializeField] private int xpValue = 10;

    private Transform seekTarget;
    private Rigidbody rb;
    private bool isSeeking = false;
    private bool isGrounded = false;

    [Tooltip("Tecrübe topunun oyuncuya doðru çekilme hýzý.")]
    [SerializeField] private float moveSpeed = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    void Update()
    {
        if (isSeeking || isGrounded) return;

        // --- HATA DÜZELTME: velocity -> linearVelocity ---
        if (rb.linearVelocity.y < 0)
        {
            RaycastHit hit;
            // LayerMask: ~0 (Her þey), QueryTriggerInteraction.Ignore (Triggerlara çarpma)
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.3f, ~0, QueryTriggerInteraction.Ignore))
            {
                LandOnGround(hit.point);
            }
        }
    }

    private void LandOnGround(Vector3 groundPoint)
    {
        isGrounded = true;
        rb.isKinematic = true; // Fizik motorunu durdur, olduðu yere sabitle
        transform.position = groundPoint + new Vector3(0, 0.1f, 0);
    }

    public void Setup(int value)
    {
        this.xpValue = value;
        // Havaya fýrlama efekti
        Vector3 force = new Vector3(Random.Range(-3f, 3f), 4f, Random.Range(-3f, 3f));
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void StartSeeking(Transform target)
    {
        rb.isKinematic = false; // Mýknatýs baþlayýnca fizik tekrar açýlmalý
        isGrounded = false;     // Artýk yerde deðil, uçuyor
        seekTarget = target;
        isSeeking = true;
        rb.useGravity = false;  // Yerçekimi kapat ki direkt sana uçsun

        // --- HATA DÜZELTME: drag -> linearDamping ---
        rb.linearDamping = 0f;
    }

    public int GetValue()
    {
        return xpValue;
    }

    void FixedUpdate()
    {
        if (isSeeking && seekTarget != null)
        {
            Vector3 direction = (seekTarget.position - rb.position).normalized;

            // --- HATA DÜZELTME: velocity -> linearVelocity ---
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    // --- YENÝ EKLENEN TOPLAMA VE DEBUG KODU ---
    // Bu fonksiyon Unity Fizik motoru tarafýndan otomatik çaðrýlýr
    private void OnTriggerEnter(Collider other)
    {
        // 1. Çarpýþma Analizi (Konsola bakarak sorunu anlayabiliriz)
        // Debug.Log($"[XP ORB] Temas var! Çarpan Obje: '{other.name}' - Tag: '{other.tag}'");

        if (other.CompareTag("Player"))
        {
            // Debug.Log("[XP ORB] Oyuncu tespit edildi, Stats scripti aranýyor...");

            // Oyuncunun üzerindeki (veya ebeveynindeki) PlayerStats scriptini bul
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats == null) stats = other.GetComponentInParent<PlayerStats>();

            if (stats != null)
            {
                // Debug.Log($"[XP ORB] BAÞARILI! {xpValue} XP oyuncuya gönderiliyor.");

                // XP Ekle
                stats.AddXp(xpValue);

                // Topu yok et
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("[XP ORB HATA] Oyuncu çarptý ama 'PlayerStats' scripti bulunamadý!");
            }
        }
    }
}