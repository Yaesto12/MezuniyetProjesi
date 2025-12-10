using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    // --- Zýplama Ayarlarý ---
    [Header("Zýplama Ayarlarý")]
    [SerializeField] private float baseJumpHeight = 1.5f;
    [SerializeField] private float gravityValue = -9.81f;

    // --- Duvar Týrmanma Ayarlarý ---
    [Header("Duvara Týrmanma Ayarlarý")]
    [SerializeField] private float wallClimbSpeed = 3f;
    [SerializeField] private float wallClimbStrafeSpeed = 2f;
    [SerializeField] private float wallSlideSpeed = 1f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 14f);
    [Tooltip("Karakterin önünde duvar aramasý için kullanýlacak mesafe.")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallLayer;

    // --- Köþe Aþma Ayarlarý ---
    [Header("Köþe Aþma (Vaulting) Ayarlarý")]
    [Tooltip("Köþe tespiti için üst ýþýnýn yüksekliði.")]
    [SerializeField] private float vaultCheckOffsetY = 0.7f;
    [Tooltip("Köþeyi aþmak için uygulanacak itme kuvveti.")]
    [SerializeField] private Vector2 vaultForce = new Vector2(4f, 8f);

    // --- Savrulma Ayarlarý ---
    [Header("Savrulma Ayarlarý")]
    [Tooltip("Düþman çarptýðýnda karaktere uygulanacak anlýk itme kuvveti.")]
    [SerializeField] private float knockbackForce = 15f;
    [Tooltip("Savrulma etkisinin ne kadar hýzlý azalacaðý.")]
    [SerializeField] private float knockbackDrag = 5f;

    // --- Referanslar ---
    private CharacterController controller;
    private PlayerInputActions playerInputActions;
    private PlayerStats playerStats;
    private Animator animator; // <--- ANÝMASYON ÝÇÝN EKLENDÝ

    // --- Dahili Deðiþkenler ---
    private Vector3 playerVelocity;
    private Vector3 impactForce;
    private bool isGrounded;
    private bool isWallClimbing;
    private RaycastHit wallHit;
    private int jumpCount = 0;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;

        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerController: PlayerStats bileþeni bulunamadý!", this);
            enabled = false;
        }

        // <--- ANÝMASYON ÝÇÝN EKLENDÝ (BAÞLANGIÇ) ---
        // Animator bu objede mi yoksa alt objede (modelde) mi kontrol et
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        // <--- ANÝMASYON ÝÇÝN EKLENDÝ (BÝTÝÞ) ---
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        HandleWallClimbingState();
        HandlePlayerInputAndGravity();
        HandleImpactForce();

        // Son Hareketi Uygula (Oyuncu hareketi + Dýþ etkenler)
        controller.Move((playerVelocity + impactForce) * Time.deltaTime);
    }

    private void HandlePlayerInputAndGravity()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        float currentSpeed = (playerStats != null) ? playerStats.CurrentMoveSpeed : 5f;

        // <--- ANÝMASYON ÝÇÝN EKLENDÝ (BAÞLANGIÇ) ---
        // Eðer input deðeri varsa (karakter hareket etmeye çalýþýyorsa) isWalking true olsun
        if (animator != null)
        {
            bool isMoving = inputVector.sqrMagnitude > 0.01f;
            animator.SetBool("isWalking", isMoving);
        }
        // <--- ANÝMASYON ÝÇÝN EKLENDÝ (BÝTÝÞ) ---

        Vector3 horizontalVelocity;
        if (isWallClimbing)
        {
            Vector3 strafeMovement = transform.right * inputVector.x * wallClimbStrafeSpeed;
            Vector3 forwardMovement = transform.forward * (currentSpeed * 0.3f);
            horizontalVelocity = strafeMovement + forwardMovement;
        }
        else
        {
            horizontalVelocity = (transform.forward * inputVector.y + transform.right * inputVector.x) * currentSpeed;
        }
        playerVelocity.x = horizontalVelocity.x;
        playerVelocity.z = horizontalVelocity.z;

        if (isWallClimbing)
        {
            if (inputVector.y > 0) playerVelocity.y = wallClimbSpeed;
            else playerVelocity.y = -wallSlideSpeed;
        }
        else
        {
            if (isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = -2f;
                jumpCount = 0;
            }
            playerVelocity.y += gravityValue * Time.deltaTime;
        }
    }

    private void HandleWallClimbingState()
    {
        Vector2 moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();
        bool isAgainstWall = Physics.Raycast(transform.position, transform.forward, out wallHit, wallCheckDistance, wallLayer);

        if (isWallClimbing && moveInput.y > 0)
        {
            Vector3 upperRaycastOrigin = transform.position + new Vector3(0, vaultCheckOffsetY, 0);

            if (isAgainstWall && !Physics.Raycast(upperRaycastOrigin, transform.forward, wallCheckDistance, wallLayer))
            {
                isWallClimbing = false;
                playerVelocity = Vector3.zero;

                Vector3 vaultVelocity = transform.forward * vaultForce.x + Vector3.up * vaultForce.y;
                impactForce = vaultVelocity;

                // Opsiyonel: Vault yaparken zýplama animasyonu çalýþtýrýlabilir
                if (animator != null) animator.SetTrigger("Jump");

                return;
            }
        }

        if (isWallClimbing)
        {
            if (!isAgainstWall || isGrounded)
            {
                isWallClimbing = false;
            }
        }
        else
        {
            if (!isGrounded && isAgainstWall && moveInput.y > 0)
            {
                isWallClimbing = true;
                playerVelocity = Vector3.zero;
                jumpCount = 0;
            }
        }
    }

    private void HandleImpactForce()
    {
        if (impactForce.magnitude > 0.2f)
        {
            impactForce = Vector3.Lerp(impactForce, Vector3.zero, knockbackDrag * Time.deltaTime);
        }
        else
        {
            impactForce = Vector3.zero;
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (playerStats == null) return;

        int maxTotalJumps = 1 + playerStats.CurrentExtraJumps;

        if (isWallClimbing)
        {
            isWallClimbing = false;
            jumpCount = 1;
            Vector3 wallJumpVelocity = (wallHit.normal * wallJumpForce.x) + (Vector3.up * wallJumpForce.y);
            impactForce += wallJumpVelocity;

            // <--- ANÝMASYON ÝÇÝN EKLENDÝ ---
            if (animator != null) animator.SetTrigger("Jump");
        }
        else if (jumpCount < maxTotalJumps)
        {
            float currentJumpHeightApplied = baseJumpHeight * (playerStats.CurrentJumpHeightMultiplier / 100f);
            playerVelocity.y = Mathf.Sqrt(currentJumpHeightApplied * -2.0f * gravityValue);
            jumpCount++;

            // <--- ANÝMASYON ÝÇÝN EKLENDÝ ---
            if (animator != null) animator.SetTrigger("Jump");
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.Normalize();
        direction.y = 0.5f;
        impactForce = direction * force;
    }

    public void ProcessHitByEnemy(EnemyAI enemy)
    {
        enemy.NotifyPlayerContact();
        Vector3 knockbackDirection = (transform.position - enemy.transform.position).normalized;
        ApplyKnockback(knockbackDirection, knockbackForce);
    }

    private void OnDisable()
    {
        playerInputActions?.Player.Disable();
    }
}