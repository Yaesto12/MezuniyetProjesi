using UnityEngine;
using System.Collections; // Coroutine için gerekli
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public class Chest : MonoBehaviour
{
    [Header("Ödül Havuzu")]
    [Tooltip("Bu sandýktan çýkabilecek itemleri buraya sürükleyin.")]
    [SerializeField] private List<ItemData> possibleItems = new List<ItemData>();

    [Header("Ekonomi")]
    [SerializeField] private int baseCost = 100;
    [SerializeField] private float costMultiplier = 1.2f;

    [Header("UI Ayarlarý")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TMP_Text costText;

    [Header("Görsel")]
    [SerializeField] private GameObject visualModel;
    [SerializeField] private Material openedMaterial;

    // --- YENÝ EKLENEN KISIM: YOK OLMA AYARLARI ---
    [Header("Yok Olma Ayarlarý")]
    [Tooltip("Sandýk açýldýktan kaç saniye sonra yok olmaya baþlasýn?")]
    [SerializeField] private float destroyDelay = 2.0f;

    [Tooltip("Küçülerek yok olma animasyonu ne kadar sürsün?")]
    [SerializeField] private float shrinkDuration = 1.0f;
    // ---------------------------------------------

    private bool isPlayerNearby = false;
    private bool isOpen = false;

    private PlayerStats currentStats;
    private PlayerInventory currentInventory;

    void Awake()
    {
        if (visualModel == null) visualModel = gameObject;
        if (interactionUI != null) interactionUI.SetActive(false);
        GetComponent<SphereCollider>().isTrigger = true;
    }

    void Update()
    {
        if (isPlayerNearby && !isOpen && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenChest();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null) stats = other.GetComponentInParent<PlayerStats>();

        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv == null) inv = other.GetComponentInParent<PlayerInventory>();

        if (stats != null && inv != null)
        {
            isPlayerNearby = true;
            currentStats = stats;
            currentInventory = inv;

            UpdateUI();
            if (interactionUI != null) interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            currentStats = null;
            currentInventory = null;
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }

    private void TryOpenChest()
    {
        if (isOpen) return;
        if (currentStats == null || currentInventory == null) return;

        if (possibleItems == null || possibleItems.Count == 0)
        {
            Debug.LogError("HATA: Sandýðýn içi boþ!");
            return;
        }

        int cost = CalculateCost(currentStats.chestsOpened);

        if (currentStats.currentGold >= cost)
        {
            ItemData reward = GetWeightedRandomItem();

            if (reward != null)
            {
                bool paymentSuccess = currentStats.SpendGold(cost);

                if (paymentSuccess)
                {
                    // --- BAÞARILI AÇILIÞ ---
                    isOpen = true;
                    currentStats.chestsOpened++;
                    currentInventory.AddItem(reward);

                    OpenChestVisuals();

                    // --- YENÝ: YOK OLMA SÜRECÝNÝ BAÞLAT ---
                    StartCoroutine(DestroyChestRoutine());
                }
            }
        }
        else
        {
            Debug.Log("Yetersiz Bakiye!");
        }
    }

    private void OpenChestVisuals()
    {
        if (openedMaterial != null && visualModel.GetComponent<MeshRenderer>())
            visualModel.GetComponent<MeshRenderer>().material = openedMaterial;

        // UI'ý hemen kapat ki oyuncu tekrar basmaya çalýþmasýn
        if (interactionUI != null) interactionUI.SetActive(false);

        currentStats.UpdateAllUI();
    }

    // --- YENÝ: KÜÇÜLEREK YOK OLMA KODU ---
    private IEnumerator DestroyChestRoutine()
    {
        // 1. Belirlenen süre kadar bekle (Örn: 2 saniye)
        yield return new WaitForSeconds(destroyDelay);

        // 2. Küçülme Animasyonu
        Vector3 originalScale = transform.localScale;
        float timer = 0f;

        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            // Mevcut boyuttan 0'a doðru yavaþça küçült (Lerp)
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, timer / shrinkDuration);
            yield return null; // Bir sonraki kareyi bekle
        }

        // 3. Tamamen yok et
        Destroy(gameObject);
    }
    // -------------------------------------

    private ItemData GetWeightedRandomItem()
    {
        if (possibleItems.Count == 0) return null;

        int totalWeight = 0;
        foreach (var item in possibleItems) if (item != null) totalWeight += item.dropWeight;

        if (totalWeight == 0) return possibleItems[0];

        int randomPoint = Random.Range(0, totalWeight);
        int currentSum = 0;

        foreach (var item in possibleItems)
        {
            if (item == null) continue;
            currentSum += item.dropWeight;
            if (randomPoint < currentSum) return item;
        }
        return possibleItems[0];
    }

    private int CalculateCost(int openCount)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, openCount));
    }

    private void UpdateUI()
    {
        if (costText == null || currentStats == null) return;

        int cost = CalculateCost(currentStats.chestsOpened);
        string color = (currentStats.currentGold >= cost) ? "yellow" : "red";
        costText.text = $"<b>[E]</b> AÇ\n<color={color}>{cost} G</color>";
    }

#if UNITY_EDITOR
    [ContextMenu("Tüm Itemleri Otomatik Bul")]
    private void FillItemsAutomatically()
    {
        possibleItems.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null) possibleItems.Add(item);
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}