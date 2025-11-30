using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class XpOrb : MonoBehaviour
{
    private int xpValue;
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
        // ---------------------------------------------
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.3f, ~0, QueryTriggerInteraction.Ignore))
            {
                LandOnGround(hit.point);
            }
        }
    }

    private void LandOnGround(Vector3 groundPoint)
    {
        isGrounded = true;
        rb.isKinematic = true;
        transform.position = groundPoint + new Vector3(0, 0.1f, 0);
    }

    public void Setup(int value)
    {
        this.xpValue = value;
        Vector3 force = new Vector3(Random.Range(-3f, 3f), 4f, Random.Range(-3f, 3f));
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void StartSeeking(Transform target)
    {
        rb.isKinematic = false;
        isGrounded = true;
        seekTarget = target;
        isSeeking = true;
        rb.useGravity = false;

        // --- HATA DÜZELTME: drag -> linearDamping ---
        rb.linearDamping = 0f;
        // ------------------------------------------
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
            // ---------------------------------------------
        }
    }
}