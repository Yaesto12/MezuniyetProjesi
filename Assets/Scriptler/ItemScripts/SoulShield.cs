using UnityEngine;

public class Item_SoulShield : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Seviye takibi için.")]
    [SerializeField] private ItemData myItemData;

    [Header("Bekleme Süresi")]
    [Tooltip("Kalkan kýrýldýktan sonra tekrar gelmesi için gereken temel süre (saniye).")]
    [SerializeField] private float baseCooldown = 10f;

    [Tooltip("Her stack (kopya) baþýna bekleme süresi ne kadar azalsýn?")]
    [SerializeField] private float reductionPerStack = 1.0f;

    [Tooltip("Süre en az kaça inebilir?")]
    [SerializeField] private float minCooldown = 2.0f;

    [Header("Görsel")]
    [Tooltip("Kalkan aktifken karakterin etrafýnda dönecek/görünecek efekt prefabý.")]
    [SerializeField] private GameObject shieldVisualPrefab;

    // Dahili deðiþkenler
    private float cooldownTimer = 0f;
    private GameObject activeVisual; // O an sahnede duran görsel
    private PlayerHealth healthScript;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        healthScript = playerOwner.GetComponent<PlayerHealth>();

        if (healthScript != null)
        {
            // Ýtemi alýr almaz kalkaný ver
            ActivateShield();
        }

        // Görseli oluþtur (ama görünürlüðünü Update'te yöneteceðiz)
        if (shieldVisualPrefab != null)
        {
            activeVisual = Instantiate(shieldVisualPrefab, playerOwner.transform.position, Quaternion.identity, playerOwner.transform);
            activeVisual.SetActive(false); // Baþlangýçta gizle, ActivateShield açacak
        }
    }

    public override void OnUnequip()
    {
        // Ýtem çýkarýlýrsa kalkan özelliðini al
        if (healthScript != null)
        {
            healthScript.IsShielded = false;
        }

        if (activeVisual != null) Destroy(activeVisual);

        base.OnUnequip();
    }

    private void Update()
    {
        if (healthScript == null) return;

        // EÐER KALKAN KIRIKSA (IsShielded == false)
        if (!healthScript.IsShielded)
        {
            // Görseli kapat (Kýrýldý efekti)
            if (activeVisual != null) activeVisual.SetActive(false);

            // Süreyi say
            cooldownTimer -= Time.deltaTime;

            // Süre bitti mi?
            if (cooldownTimer <= 0f)
            {
                ActivateShield();
            }
        }
        else
        {
            // KALKAN AKTÝFKEN
            // Görseli açýk tut
            if (activeVisual != null && !activeVisual.activeSelf)
            {
                activeVisual.SetActive(true);
            }
        }
    }

    private void ActivateShield()
    {
        healthScript.IsShielded = true;

        // Bir sonraki kýrýlma için süreyi þimdiden hesapla (kýrýlýnca bu süreden saymaya baþlayacak)
        cooldownTimer = CalculateCooldown();

        // Debug.Log("Soul Shield: Kalkan Yenilendi!");
    }

    private float CalculateCooldown()
    {
        int stack = 1;
        if (myItemData != null && owner != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // Formül: Base - (Azaltma * (Seviye - 1))
        float currentCD = baseCooldown - (reductionPerStack * (stack - 1));

        return Mathf.Max(currentCD, minCooldown);
    }
}