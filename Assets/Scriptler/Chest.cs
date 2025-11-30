using UnityEngine;

[RequireComponent(typeof(Collider))] // Tetikleyici bir collider olmalý
public class Chest : MonoBehaviour
{
    [Header("Ödül Ayarlarý")]
    [Tooltip("Bu sandýðýn vereceði spesifik item (Test için).")]
    [SerializeField] private ItemData itemToGive;
    // Ýleride buraya 'LootTable' (içinden rastgele item çýkan liste) eklenebilir.

    [Header("Görsel Ayarlar")]
    [Tooltip("Sandýk açýldýktan sonraki görünümü (opsiyonel).")]
    [SerializeField] private Material openedMaterial;
    [SerializeField] private GameObject visualObject; // Sandýðýn modeli (eðer çocuk obje ise)

    private bool isOpened = false; // Sandýðýn tekrar tekrar açýlmasýný engeller

    void Awake()
    {
        // Eðer görsel obje atanmamýþsa, kendisini kullan
        if (visualObject == null)
        {
            visualObject = this.gameObject;
        }
    }

    // Oyuncu (PlayerHurtbox) bu trigger'a girdiðinde
    private void OnTriggerEnter(Collider other)
    {
        // Eðer zaten açýlmadýysa VE çarpan þey PlayerHurtbox ise...
        // Not: PlayerHurtbox'ýn katmanýný (PlayerHurtbox) veya tag'ini (Player) kontrol edebilirsiniz.
        // Þimdilik PlayerHurtbox script'ini arayalým.
        if (!isOpened && other.GetComponentInParent<PlayerHurtbox>() != null)
        {
            // PlayerInventory'yi bul
            PlayerInventory playerInventory = other.GetComponentInParent<PlayerInventory>();

            if (playerInventory != null && itemToGive != null)
            {
                OpenChest(playerInventory);
            }
            else if (playerInventory == null)
            {
                Debug.LogError("Chest: Oyuncuda PlayerInventory bulunamadý!");
            }
            else if (itemToGive == null)
            {
                Debug.LogError("Chest: Sandýða atanmýþ bir 'itemToGive' yok!", this);
            }
        }
    }

    private void OpenChest(PlayerInventory playerInventory)
    {
        Debug.Log($"Sandýk Açýldý! Ödül: {itemToGive.itemName}");
        isOpened = true;

        // 1. Item'ý envantere ekle
        playerInventory.AddItem(itemToGive);

        // 2. Sandýðýn görünümünü deðiþtir (eðer atandýysa)
        MeshRenderer meshRenderer = visualObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null && openedMaterial != null)
        {
            meshRenderer.material = openedMaterial;
        }

        // 3. (Opsiyonel) Bu script'i veya collider'ý devre dýþý býrak
        // GetComponent<Collider>().enabled = false;
        // this.enabled = false;
    }
}