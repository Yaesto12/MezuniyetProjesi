using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target;

    [Header("Mesafe Ayarlarý")]
    public float distanceFromTarget = 5.0f; // Hedeflenen uzaklýk
    public float heightFromTarget = 1.5f;   // Kafa hizasý
    public float minDistance = 0.5f;        // Kamera karaktere en fazla ne kadar yaklaþabilir?

    [Header("Hassasiyet Ayarlarý")]
    public float mouseSensitivity = 3.0f;
    public float rotationSmoothTime = 0.12f;
    public Vector2 pitchLimit = new Vector2(-40, 85); // Aþaðý/Yukarý bakma limiti

    [Header("Çarpýþma Ayarlarý (YENÝ)")]
    [Tooltip("Kameranýn neleri duvar olarak göreceðini seç (Player'ý seçme!)")]
    public LayerMask collisionLayers;
    [Tooltip("Kameranýn çarpýþma yarýçapý (Kameranýn kalýnlýðý)")]
    public float collisionRadius = 0.2f; // Iþýn kalýnlýðý
    [Tooltip("Çarpýþma yumuþaklýðý")]
    public float collisionSmoothTime = 0.05f;

    // Dahili deðiþkenler
    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;
    private float yaw;
    private float pitch;

    // Çarpýþma sonrasý anlýk uzaklýk
    private float currentDistance;
    private float distanceVelocity; // Yumuþak geçiþ için hýz referansý

    void Start()
    {
        // Baþlangýçta hedeflenen uzaklýðý ayarla
        currentDistance = distanceFromTarget;

        // LayerMask ayarlanmadýysa otomatik olarak 'Default' yapalým ki her þeyin içinden geçmesin
        if (collisionLayers.value == 0)
        {
            collisionLayers = 1; // Default layer
        }
    }

    void LateUpdate()
    {
        // --- EKLENEN KISIM (ÇÖZÜM 2) ---
        // Eðer oyun durdurulmuþsa (Level Up ekraný vs. açýksa) kamerayý dondur.
        // Bu, hem kameranýn saçmalamasýný engeller hem de mouse'u rahat býrakýr.
        if (Time.timeScale == 0f) return;
        // ------------------------------

        if (target == null)
        {
            FindPlayer();
            return;
        }

        // 1. Mouse Giriþleri
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchLimit.x, pitchLimit.y);

        // 2. Rotasyon Hesapla
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        // 3. ÇARPIÞMA KONTROLÜ (YENÝ KISIM)
        // Karakterin kafa hizasý (Bakýþýn baþladýðý yer)
        Vector3 focusPoint = target.position + Vector3.up * heightFromTarget;

        // Kameranýn bakýþ yönünün tersi (Kameranýn durmasý gereken yön)
        Vector3 cameraDirection = transform.rotation * -Vector3.forward;

        float targetDist = distanceFromTarget;

        // Karakterden kameraya doðru bir Küre fýrlat (SphereCast)
        RaycastHit hit;
        if (Physics.SphereCast(focusPoint, collisionRadius, cameraDirection, out hit, distanceFromTarget, collisionLayers))
        {
            // Eðer bir þeye çarparsa, mesafeyi çarpýlan yere kadar kýsalt
            targetDist = hit.distance;
        }

        // Mesafenin çok kýsalýp karakterin kafasýnýn içine girmesini engelle
        targetDist = Mathf.Max(targetDist, minDistance);

        // Mesafeyi yumuþakça uygula (Çok hýzlý duvar çarpmasý titreme yapmasýn diye)
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDist, ref distanceVelocity, collisionSmoothTime);

        // 4. Pozisyonu Uygula
        transform.position = focusPoint + cameraDirection * currentDistance;
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            Vector3 angles = transform.eulerAngles;
            yaw = playerObj.transform.eulerAngles.y;
            pitch = angles.x;
            currentRotation = new Vector3(pitch, yaw);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}