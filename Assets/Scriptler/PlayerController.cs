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

    // --- Hareket ve Dönüþ Ayarlarý ---
    [Header("Hareket Ayarlarý")]
    [Tooltip("Karakterin dönme hýzý.")]
    [SerializeField] private float rotationSpeed = 10f;

    // --- Duvar Týrmanma Ayarlarý ---
    [Header("Duvara Týrmanma Ayarlarý")]
    [SerializeField] private float wallClimbSpeed = 3f;
    [SerializeField] private float wallClimbStrafeSpeed = 2f;
    [SerializeField] private float wallSlideSpeed = 1f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 14f);
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallLayer;

    // --- Köþe Aþma Ayarlarý ---
    [Header("Köþe Aþma (Vaulting) Ayarlarý")]
    [SerializeField] private float vaultCheckOffsetY = 0.7f;
    [SerializeField] private Vector2 vaultForce = new Vector2(4f, 8f);

    // --- Savrulma Ayarlarý ---
    [Header("Savrulma Ayarlarý")]
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float knockbackDrag = 5f;

    // --- Referanslar ---
    private CharacterController controller;
    private PlayerInputActions playerInputActions;
    private PlayerStats playerStats;
    private Transform cameraTransform;

    // --- GENEL ANÝMASYON REFERANSI ---
    private Animator animator;

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
            Debug.LogError("PlayerController: PlayerStats bulunamadý!", this);
            enabled = false;
        }

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // --- KRÝTÝK NOKTA: OTOMATÝK ANIMATOR BULMA ---
        // Bu kod, scriptin takýlý olduðu objenin altýndaki (Child) modelleri tarar.
        // Cyborg, Mage, Archer fark etmez, Animator kimdeyse onu bulur.
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            // Eðer modelde Animator yoksa hata vermesin ama uyarýsýn.
            Debug.LogWarning($"{gameObject.name}: Alt objelerde Animator bulunamadý!");
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        HandleWallClimbingState();
        HandlePlayerInputAndGravity();
        HandleImpactForce();

        controller.Move((playerVelocity + impactForce) * Time.deltaTime);

        if (!isWallClimbing)
        {
            HandleRotation();
        }

        // Animasyonlarý her karede güncelle
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // 1. Speed (Koþma/Durma)
        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        animator.SetFloat("Speed", currentSpeed);

        // 2. IsGrounded (Düþme/Yerde Olma)
        animator.SetBool("IsGrounded", isGrounded);

        // 3. VerticalSpeed (Zýplama/Düþüþ harmanlamasý için opsiyonel)
        animator.SetFloat("VerticalSpeed", playerVelocity.y);
    }

    private void HandleRotation()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        if (inputVector.sqrMagnitude > 0.01f && cameraTransform != null)
        {
            Vector3 viewDir = cameraTransform.forward;
            viewDir.y = 0;
            viewDir.Normalize();

            Vector3 rightDir = cameraTransform.right;
            rightDir.y = 0;
            rightDir.Normalize();

            Vector3 targetDirection = viewDir * inputVector.y + rightDir * inputVector.x;
            targetDirection.Normalize();

            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void HandlePlayerInputAndGravity()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        float currentSpeed = (playerStats != null) ? playerStats.CurrentMoveSpeed : 5f;

        Vector3 horizontalVelocity;

        if (isWallClimbing)
        {
            Vector3 strafeMovement = transform.right * inputVector.x * wallClimbStrafeSpeed;
            Vector3 forwardMovement = transform.forward * (currentSpeed * 0.3f);
            horizontalVelocity = strafeMovement + forwardMovement;
        }
        else
        {
            if (cameraTransform != null)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 moveDir = (camForward * inputVector.y + camRight * inputVector.x).normalized;
                horizontalVelocity = moveDir * currentSpeed;
            }
            else
            {
                horizontalVelocity = (transform.forward * inputVector.y + transform.right * inputVector.x) * currentSpeed;
            }
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

            if (animator != null) animator.SetTrigger("Jump");
        }
        else if (jumpCount < maxTotalJumps)
        {
            float currentJumpHeightApplied = baseJumpHeight * (playerStats.CurrentJumpHeightMultiplier / 100f);
            playerVelocity.y = Mathf.Sqrt(currentJumpHeightApplied * -2.0f * gravityValue);
            jumpCount++;

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