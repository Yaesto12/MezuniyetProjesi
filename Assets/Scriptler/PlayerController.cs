using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))] // PlayerStats zorunlu
public class PlayerController : MonoBehaviour
{
    // --- Zýplama Ayarlarý (Temel Deðer) ---
    [Header("Zýplama Ayarlarý")]
    [SerializeField] private float baseJumpHeight = 1.5f; // PlayerStats'tan gelen çarpanla kullanýlacak
    [SerializeField] private float gravityValue = -9.81f;

    // --- Duvar Týrmanma Ayarlarý (KULLANILIYOR) ---
    [Header("Duvara Týrmanma Ayarlarý")]
    [SerializeField] private float wallClimbSpeed = 3f;
    [SerializeField] private float wallClimbStrafeSpeed = 2f;
    [SerializeField] private float wallSlideSpeed = 1f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 14f);
    [Tooltip("Karakterin önünde duvar aramasý için kullanýlacak mesafe.")]
    [SerializeField] private float wallCheckDistance = 0.5f; // <<<--- BU KULLANILACAK ---<<<
    [SerializeField] private LayerMask wallLayer;

    // --- Köþe Aþma Ayarlarý (KULLANILIYOR) ---
    [Header("Köþe Aþma (Vaulting) Ayarlarý")]
    [Tooltip("Köþe tespiti için üst ýþýnýn yüksekliði (karakterin merkezine göre).")]
    [SerializeField] private float vaultCheckOffsetY = 0.7f; // <<<--- BU KULLANILACAK ---<<<
    [Tooltip("Köþeyi aþmak için uygulanacak itme kuvveti (X=Ýleri, Y=Yukarý).")]
    [SerializeField] private Vector2 vaultForce = new Vector2(4f, 8f);

    // --- Savrulma Ayarlarý (KULLANILIYOR) ---
    [Header("Savrulma Ayarlarý")]
    [Tooltip("Düþman çarptýðýnda karaktere uygulanacak anlýk itme kuvveti.")]
    [SerializeField] private float knockbackForce = 15f; // <<<--- BU KULLANILACAK ---<<<
    [Tooltip("Savrulma etkisinin ne kadar hýzlý azalacaðý (sürtünme gibi).")]
    [SerializeField] private float knockbackDrag = 5f; // <<<--- BU KULLANILACAK ---<<<

    // --- Referanslar ---
    private CharacterController controller;
    private PlayerInputActions playerInputActions;
    private PlayerStats playerStats; // Statlarý okumak için

    // --- Dahili Deðiþkenler ---
    private Vector3 playerVelocity; // Oyuncu girdisi + yerçekimi
    private Vector3 impactForce;    // Dýþ kuvvetler (savrulma, köþe aþma)
    private bool isGrounded;
    private bool isWallClimbing;
    private RaycastHit wallHit;
    private int jumpCount = 0; // Mevcut zýplama sayacý

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
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        HandleWallClimbingState(); // Duvar ve Köþe Aþma mantýðý
        HandlePlayerInputAndGravity(); // Hareket ve Yerçekimi
        HandleImpactForce(); // Savrulma kuvvetinin azalmasý

        // Son Hareketi Uygula (Oyuncu hareketi + Dýþ etkenler)
        controller.Move((playerVelocity + impactForce) * Time.deltaTime);
    }

    /// <summary>
    /// Oyuncu girdisine ve PlayerStats'a göre hareketi ve yerçekimini yönetir.
    /// </summary>
    private void HandlePlayerInputAndGravity()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        // Güncel hareket hýzýný PlayerStats'tan al
        float currentSpeed = (playerStats != null) ? playerStats.CurrentMoveSpeed : 5f; // Hata durumunda 5

        Vector3 horizontalVelocity;
        if (isWallClimbing)
        {
            Vector3 strafeMovement = transform.right * inputVector.x * wallClimbStrafeSpeed;
            Vector3 forwardMovement = transform.forward * (currentSpeed * 0.3f); // Köþeyi aþmak için hafif ileri itme
            horizontalVelocity = strafeMovement + forwardMovement;
        }
        else
        {
            // Normal yer ve hava hareketi
            horizontalVelocity = (transform.forward * inputVector.y + transform.right * inputVector.x) * currentSpeed;
        }
        playerVelocity.x = horizontalVelocity.x;
        playerVelocity.z = horizontalVelocity.z;

        // Dikey Hýz (Yerçekimi / Týrmanma)
        if (isWallClimbing)
        {
            if (inputVector.y > 0) playerVelocity.y = wallClimbSpeed; // 'W' basýyorsa týrman
            else playerVelocity.y = -wallSlideSpeed; // Basmýyorsa kay
        }
        else
        {
            // Yerdeyse ve düþmüyorsa yerçekimini sýfýrla
            if (isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = -2f;
                jumpCount = 0; // Zýplama sayacýný sýfýrla
            }
            playerVelocity.y += gravityValue * Time.deltaTime; // Yerçekimini uygula
        }
    }

    /// <summary>
    /// Duvara týrmanma, kayma ve köþe aþma (vaulting) mantýðýný yönetir.
    /// </summary>
    private void HandleWallClimbingState()
    {
        Vector2 moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();

        // Ayak hizasýndan duvarý kontrol et (wallCheckDistance KULLANILIYOR)
        bool isAgainstWall = Physics.Raycast(transform.position, transform.forward, out wallHit, wallCheckDistance, wallLayer);

        // 1. Köþe Aþma (Vaulting) Kontrolü
        if (isWallClimbing && moveInput.y > 0) // Týrmanýrken ve 'W' basarken
        {
            // Göðüs hizasýndan (vaultCheckOffsetY KULLANILIYOR) ikinci bir ýþýn gönder
            Vector3 upperRaycastOrigin = transform.position + new Vector3(0, vaultCheckOffsetY, 0);

            // Eðer ayak hizasýnda duvar VARSA ama göðüs hizasýnda duvar YOKSA...
            if (isAgainstWall && !Physics.Raycast(upperRaycastOrigin, transform.forward, wallCheckDistance, wallLayer))
            {
                // Bu bir köþedir! Köþeyi aþma hareketini tetikle.
                isWallClimbing = false; // Týrmanmayý býrak
                playerVelocity = Vector3.zero; // Oyuncu kontrolündeki hýzý sýfýrla

                // Köþeden yukarý ve ileri doðru bir itme kuvveti uygula (vaultForce KULLANILIYOR)
                Vector3 vaultVelocity = transform.forward * vaultForce.x + Vector3.up * vaultForce.y;
                impactForce = vaultVelocity; // Bu itmeyi dýþ kuvvet olarak ata

                return; // Bu frame için diðer kontrolleri atla
            }
        }

        // 2. Normal Týrmanma Durumu Kontrolü
        if (isWallClimbing)
        {
            // Duvardan ayrýldýysak VEYA yere indiysek týrmanmayý býrak
            if (!isAgainstWall || isGrounded)
            {
                isWallClimbing = false;
            }
        }
        else // Týrmanmýyorsak, týrmanmaya baþlama þartlarýný kontrol et
        {
            // Yerde deðilsek, duvara karþýysak ve 'W' basýyorsak
            if (!isGrounded && isAgainstWall && moveInput.y > 0)
            {
                isWallClimbing = true;
                playerVelocity = Vector3.zero; // Baþlarken dikey hýzý sýfýrla
                jumpCount = 0; // Duvara tutununca zýplama haklarý yenilensin
            }
        }
    }

    /// <summary>
    /// Dýþ kuvvetlerin (savrulma, köþe aþma) etkisini zamanla azaltýr.
    /// </summary>
    private void HandleImpactForce()
    {
        // Dýþ kuvvet varsa
        if (impactForce.magnitude > 0.2f)
        {
            // Kuvveti yavaþça azalt (knockbackDrag KULLANILIYOR)
            impactForce = Vector3.Lerp(impactForce, Vector3.zero, knockbackDrag * Time.deltaTime);
        }
        else
        {
            impactForce = Vector3.zero; // Yeterince azalýnca sýfýrla
        }
    }

    /// <summary>
    /// Zýplama tuþuna basýldýðýnda çaðrýlýr.
    /// </summary>
    private void Jump(InputAction.CallbackContext context)
    {
        if (playerStats == null) return; // Stats yoksa zýplama

        // Toplam zýplama hakký = 1 (yerden) + Ekstra Haklar
        int maxTotalJumps = 1 + playerStats.CurrentExtraJumps;

        if (isWallClimbing)
        {
            // Duvardan Zýplama
            isWallClimbing = false;
            jumpCount = 1; // Duvardan zýpladý, 1 hakký gitti
            Vector3 wallJumpVelocity = (wallHit.normal * wallJumpForce.x) + (Vector3.up * wallJumpForce.y);
            impactForce += wallJumpVelocity; // Dýþ kuvvete ekle
        }
        else if (jumpCount < maxTotalJumps) // Yerde veya havada zýplama hakký var mý?
        {
            // Zýplama
            float currentJumpHeightApplied = baseJumpHeight * (playerStats.CurrentJumpHeightMultiplier / 100f);
            playerVelocity.y = Mathf.Sqrt(currentJumpHeightApplied * -2.0f * gravityValue);
            jumpCount++; // Sayacý artýr
        }
    }

    /// <summary>
    /// Hurtbox tarafýndan çaðrýlýr, savrulma uygular.
    /// </summary>
    public void ProcessHitByEnemy(EnemyAI enemy)
    {
        // Düþmanýn saldýrý fonksiyonunu çaðýr (Hasar alma vb. PlayerHealth'te yönetilir)
        enemy.NotifyPlayerContact();

        // Savrulma mantýðýný uygula (knockbackForce KULLANILIYOR)
        Vector3 knockbackDirection = (transform.position - enemy.transform.position).normalized;
        Vector3 knockback = knockbackDirection * knockbackForce;
        knockback.y = knockbackForce / 2; // Hafifçe havaya kaldýr
        impactForce = knockback; // Dýþ kuvveti ayarla (eskisinin üzerine yaz)
    }

    private void OnDisable()
    {
        playerInputActions?.Player.Disable(); // Güvenli kapatma
    }
}