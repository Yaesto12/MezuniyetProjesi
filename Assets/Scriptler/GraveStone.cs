using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public class GraveStone : MonoBehaviour
{
    [Header("Ödül Ayarlarý (XP Küresi)")]
    [Tooltip("Düþmanlardan düþen XP Kristali prefabýný buraya sürükle.")]
    [SerializeField] private GameObject xpOrbPrefab;

    [Tooltip("Kaç tane küre düþsün?")]
    [SerializeField] private int orbCount = 5;

    [Tooltip("Küreler ne kadar geniþliðe saçýlsýn?")]
    [SerializeField] private float scatterRadius = 2.0f;

    [Header("UI Ayarlarý")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Görsel & Efekt")]
    [SerializeField] private GameObject visualModel;
    [SerializeField] private float destroyDelay = 0.5f;
    [SerializeField] private float shrinkDuration = 1.0f;

    private bool isPlayerNearby = false;
    private bool isCollected = false;

    void Awake()
    {
        GetComponent<SphereCollider>().isTrigger = true;
        if (interactionUI != null) interactionUI.SetActive(false);
        if (visualModel == null) visualModel = gameObject;
    }

    void Start()
    {
        if (infoText != null)
        {
            // Metni XP miktarý yerine "Ruhlarý Serbest Býrak" gibi genel bir þeye çevirdik
            infoText.text = $"<b>[E]</b> Free the Souls";
        }
    }

    void Update()
    {
        if (isPlayerNearby && !isCollected && Input.GetKeyDown(KeyCode.E))
        {
            ReleaseSouls();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactionUI != null) interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }

    private void ReleaseSouls()
    {
        isCollected = true;
        if (interactionUI != null) interactionUI.SetActive(false);

        // --- ORB SAÇMA ÝÞLEMÝ (YENÝ) ---
        if (xpOrbPrefab != null)
        {
            for (int i = 0; i < orbCount; i++)
            {
                // Mezar taþýnýn etrafýnda rastgele bir pozisyon bul
                Vector3 randomOffset = Random.insideUnitSphere * scatterRadius;
                randomOffset.y = 0.5f; // Yere gömülmesin, havada dursun

                Vector3 spawnPos = transform.position + randomOffset;

                // Küreyi oluþtur
                Instantiate(xpOrbPrefab, spawnPos, Quaternion.identity);
            }
            Debug.Log($"{orbCount} adet XP Küresi saçýldý!");
        }
        else
        {
            Debug.LogError("GraveStone: XP Orb Prefabý atanmamýþ!");
        }

        // --- YOK OLMA ---
        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);

        Vector3 originalScale = transform.localScale;
        float timer = 0f;

        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, timer / shrinkDuration);
            yield return null;
        }

        Destroy(gameObject);
    }
}