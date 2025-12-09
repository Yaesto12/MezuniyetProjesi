using UnityEngine;
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
    [Tooltip("Sandýðýn üstündeki Canvas/Panel")]
    [SerializeField] private GameObject interactionUI;
    [Tooltip("Fiyatý yazacak TextMeshPro")]
    [SerializeField] private TMP_Text costText;

    [Header("Görsel")]
    [SerializeField] private GameObject visualModel;
    [SerializeField] private Material openedMaterial;

    private bool isPlayerNearby = false;
    private bool isOpen = false; // YENÝ: Sandýk açýk mý kilidi

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
        // YENÝ: Eðer sandýk zaten açýldýysa (isOpen) bir daha E'ye basýlamaz.
        if (isPlayerNearby && !isOpen && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenChest();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen) return; // Açýk sandýkla etkileþime girilmez

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
        if (isOpen) return; // Çift týklama korumasý
        if (currentStats == null || currentInventory == null) return;

        // 1. ÖNCE LÝSTE KONTROLÜ (Boþ sandýða para ödeme!)
        if (possibleItems == null || possibleItems.Count == 0)
        {
            Debug.LogError("HATA: Sandýðýn içi boþ! (possibleItems listesi 0). Prefab ayarlarýný kontrol et.");
            return;
        }

        int cost = CalculateCost(currentStats.chestsOpened);

        // 2. YETERLÝ PARA VAR MI?
        if (currentStats.currentGold >= cost)
        {
            // 3. ÖNCE ÝTEMÝ BELÝRLE
            ItemData reward = GetWeightedRandomItem();

            if (reward != null)
            {
                // 4. PARAYI ÞÝMDÝ HARCA
                bool paymentSuccess = currentStats.SpendGold(cost);

                if (paymentSuccess)
                {
                    // 5. HER ÞEY BAÞARILI, SANDIÐI AÇ
                    isOpen = true; // Kilidi kapat
                    currentStats.chestsOpened++; // Ýstatistik iþle

                    // Ýtemi ver
                    currentInventory.AddItem(reward);
                    Debug.Log($"<color=green>SANDIK BAÞARILI: {reward.itemName} kazanýldý. {cost} altýn harcandý.</color>");

                    OpenChestVisuals();
                }
            }
            else
            {
                Debug.LogError("HATA: Ýtem seçilemedi (Reward null döndü). Aðýrlýklar (Drop Weight) 0 olabilir mi?");
            }
        }
        else
        {
            Debug.Log($"Yetersiz Bakiye! Gereken: {cost}, Olan: {currentStats.currentGold}");
            // Yetersiz bakiye animasyonu vs. buraya
        }
    }

    private void OpenChestVisuals()
    {
        if (openedMaterial != null && visualModel.GetComponent<MeshRenderer>())
            visualModel.GetComponent<MeshRenderer>().material = openedMaterial;

        if (interactionUI != null) interactionUI.SetActive(false);

        // UI güncellemesi gerekmez çünkü UI kapandý, ama stats güncel kalsýn
        currentStats.UpdateAllUI();
    }

    private ItemData GetWeightedRandomItem()
    {
        if (possibleItems.Count == 0) return null;

        int totalWeight = 0;
        foreach (var item in possibleItems)
        {
            if (item != null) totalWeight += item.dropWeight;
        }

        if (totalWeight == 0) return possibleItems[0]; // Hepsinin aðýrlýðý 0 ise ilkini ver

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