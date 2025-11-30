using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitController : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Kameranýn dikey dönüþ pivotu (CameraHolder objesi).")]
    [SerializeField] private Transform cameraPivot;
    [Tooltip("Takip edilecek ve bakýlacak ana kamera.")]
    [SerializeField] private Camera mainCamera;

    [Header("Ayarlar")]
    [SerializeField] private float mouseSensitivity = 150f;
    [SerializeField] private float minVerticalAngle = -35f;
    [SerializeField] private float maxVerticalAngle = 70f;

    // Dahili Deðiþkenler
    private PlayerInputActions playerInputActions;
    private float currentXAngle = 0f;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        // Kamerayý otomatik bulma (Inspector'da atanmamýþsa)
        if (mainCamera == null && cameraPivot != null)
        {
            mainCamera = cameraPivot.GetComponentInChildren<Camera>();
        }
        if (mainCamera == null)
        {
            Debug.LogError($"[{gameObject.name}] CameraOrbitController: MainCamera referansý bulunamadý!", this);
        }
        if (cameraPivot == null)
        {
            Debug.LogError($"[{gameObject.name}] CameraOrbitController: Camera Pivot referansý Inspector'da atanmamýþ!", this);
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // Baþlangýç açýsýný ayarla
        if (cameraPivot != null) currentXAngle = cameraPivot.localEulerAngles.x;
        if (currentXAngle > 180) currentXAngle -= 360;
        // Baþlangýç açýsýný logla
        Debug.Log($"[{gameObject.name}] Start - Initial Angle: {currentXAngle}");
    }

    void Update()
    {
        // Fare girdisini al
        Vector2 lookInput = playerInputActions.Player.Look.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // Yatay Dönüþ (Oyuncu Kök Objesi)
        transform.Rotate(Vector3.up * mouseX);

        // Dikey Dönüþü Hesapla (Pivot)
        currentXAngle -= mouseY;
        currentXAngle = Mathf.Clamp(currentXAngle, minVerticalAngle, maxVerticalAngle);

        // --- HATA AYIKLAMA LOGLARI EKLENDÝ ---
        // Her karedeki fare Y girdisini ve sonuçta oluþan dikey açýyý yazdýrýr.
        // Hangi karakterin logu olduðunu görmek için baþýna obje adýný ekledik.
        //Debug.Log($"[{gameObject.name}] Mouse Y Input: {mouseY:F4}, New Vertical Angle: {currentXAngle:F2}");
        // ------------------------------------

        // Dikey Dönüþü Pivota Uygula
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(currentXAngle, 0f, 0f);
        }
        else
        {
            // Bu log Awake'te zaten var ama Update içinde de kontrol etmek faydalý olabilir.
            Debug.LogError($"[{gameObject.name}] UPDATE - Camera Pivot referansý NULL!");
        }
    }

    void LateUpdate()
    {
        // Kameranýn oyuncuya bakmasýný saðla
        if (mainCamera != null)
        {
            // --- HATA AYIKLAMA ÇÝZGÝSÝ EKLENDÝ ---
            // Kameranýn tam olarak nereye baktýðýný Scene penceresinde görmek için
            Vector3 targetLookAt = transform.position; // Player'ýn kök objesinin merkezi
            // Ýsterseniz bakýþ noktasýný biraz yukarý taþýyabilirsiniz:
            // Vector3 targetLookAt = transform.position + Vector3.up * 1.0f;
            Debug.DrawLine(mainCamera.transform.position, targetLookAt, Color.cyan); // Mavi çizgi çizer
            // ------------------------------------

            mainCamera.transform.LookAt(targetLookAt);
        }
        else
        {
            // Bu log Awake'te zaten var ama LateUpdate içinde de kontrol etmek faydalý olabilir.
            Debug.LogError($"[{gameObject.name}] LATEUPDATE - Main Camera referansý NULL!");
        }
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
        // Fareyi serbest býrak
        if (Cursor.lockState == CursorLockMode.Locked) // Sadece kilitliyse aç
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}