using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public class GraveStone : MonoBehaviour
{
    [Header("Ödül Ayarlarý")]
    [Tooltip("Bu mezar taþý ne kadar XP verecek?")]
    [SerializeField] private int xpAmount = 50;

    [Header("UI Ayarlarý")]
    [SerializeField] private GameObject interactionUI; // "E - Ruhunu Al" yazýsý
    [SerializeField] private TextMeshProUGUI infoText; // Ýstersen XP miktarýný yazdýrabilirsin

    [Header("Görsel & Efekt")]
    [SerializeField] private GameObject visualModel;
    [Tooltip("Etkileþimden sonra kaç saniye sonra yok olmaya baþlasýn?")]
    [SerializeField] private float destroyDelay = 1.0f;
    [Tooltip("Küçülme süresi")]
    [SerializeField] private float shrinkDuration = 1.0f;

    private bool isPlayerNearby = false;
    private bool isCollected = false;
    private PlayerStats playerStats;

    void Awake()
    {
        // Collider ayarýný garantiye alalým
        GetComponent<SphereCollider>().isTrigger = true;

        if (interactionUI != null) interactionUI.SetActive(false);
        if (visualModel == null) visualModel = gameObject;
    }

    void Start()
    {
        // Baþlangýçta UI metnini güncelleyebiliriz
        if (infoText != null)
        {
            infoText.text = $"<b>[E]</b> Ruhunu Al\n<color=cyan>+{xpAmount} XP</color>";
        }
    }

    void Update()
    {
        // Oyuncu yakýndaysa, henüz alýnmadýysa ve E'ye basýldýysa
        if (isPlayerNearby && !isCollected && Input.GetKeyDown(KeyCode.E))
        {
            CollectSoul();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            // PlayerStats scriptini bul
            playerStats = other.GetComponent<PlayerStats>();
            if (playerStats == null) playerStats = other.GetComponentInParent<PlayerStats>();

            if (playerStats != null)
            {
                isPlayerNearby = true;
                if (interactionUI != null) interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            playerStats = null;
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }

    private void CollectSoul()
    {
        if (playerStats == null) return;

        // --- 1. XP ÖDÜLÜNÜ VER ---
        // PlayerStats scriptinde "GainExperience" veya benzeri bir metodun olduðunu varsayýyoruz.
        // Eðer yoksa, PlayerStats.cs içine aþaðýda verdiðim eklemeyi yapmalýsýn.

        // Reflection kullanmadan direkt çaðýrmak en iyisidir ama scriptini görmediðim için SendMessage kullanýyorum
        // Veya direkt: playerStats.GainExperience(xpAmount);
        playerStats.SendMessage("GainExperience", xpAmount, SendMessageOptions.DontRequireReceiver);

        Debug.Log($"<color=cyan>{xpAmount} XP Kazanýldý!</color>");

        // --- 2. GÖRSEL ÝÞLEMLER ---
        isCollected = true;
        if (interactionUI != null) interactionUI.SetActive(false); // Yazýyý hemen kapat

        // --- 3. YOK OLMA SÜRECÝ ---
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