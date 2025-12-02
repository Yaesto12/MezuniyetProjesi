using UnityEngine;

public class Item_Rook : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Zamanlama")]
    [Tooltip("Maksimum bonusa ulaþmak için gereken süre (saniye).")]
    [SerializeField] private float rampUpTime = 3f;

    [Header("Hasar Ayarlarý")]
    [Tooltip("Ýlk seviyede ulaþýlacak maksimum hasar bonusu (Örn: 30 = %30).")]
    [SerializeField] private float baseMaxDamage = 30f;

    [Tooltip("Her stack için maksimum hasara eklenecek miktar.")]
    [SerializeField] private float bonusPerStack = 30f;

    // Dahili deðiþkenler
    private CharacterController controller;
    private float currentTimer = 0f;
    private float currentAppliedBonus = 0f;
    private bool isStationary = false;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        controller = playerOwner.GetComponent<CharacterController>();
    }

    public override void OnUnequip()
    {
        // Çýkarýlýrsa mevcut bonusu sil
        if (currentAppliedBonus > 0 && playerStats != null)
        {
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
        }
        base.OnUnequip();
    }

    private void Update()
    {
        if (controller == null || playerStats == null) return;

        // Hareket kontrolü (Hýz çok düþükse duruyor sayalým)
        // velocity.sqrMagnitude performans için magnitude'den daha iyidir
        bool isMoving = controller.velocity.sqrMagnitude > 0.01f;

        if (!isMoving)
        {
            // --- DURUYOR ---
            isStationary = true;

            // Zamaný artýr (ama limiti geçme)
            if (currentTimer < rampUpTime)
            {
                currentTimer += Time.deltaTime;
                UpdateDamageBonus();
            }
        }
        else
        {
            // --- HAREKET EDÝYOR ---
            if (isStationary)
            {
                // Hareket baþladýðý an sýfýrla
                ResetBonus();
                isStationary = false;
            }
        }
    }

    private void UpdateDamageBonus()
    {
        // 1. Maksimum Limiti Hesapla (Stack'e göre)
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        float maxBonus = baseMaxDamage + (bonusPerStack * (stack - 1));

        // 2. Þu anki saniyeye göre ne kadar bonus almalýyýz?
        // Oran (0.0 ile 1.0 arasý) = Geçen Süre / Hedef Süre
        float ratio = Mathf.Clamp01(currentTimer / rampUpTime);

        // Hedeflenen anlýk bonus
        float targetBonus = Mathf.Lerp(0, maxBonus, ratio);

        // 3. PlayerStats'a farký uygula
        // (Önceki karede eklediðimiz bonus ile þimdiki arasýndaki farký ekliyoruz)
        float difference = targetBonus - currentAppliedBonus;

        if (Mathf.Abs(difference) > 0.01f) // Gereksiz küçük güncellemeleri yapma
        {
            playerStats.AddTemporaryDamage(difference);
            currentAppliedBonus += difference; // Toplam eklediðimiz bonusu güncelle

            // Debug.Log($"Rook: Duruyor... Bonus: +%{currentAppliedBonus:F1} (Süre: {currentTimer:F1}s)");
        }
    }

    private void ResetBonus()
    {
        if (currentAppliedBonus > 0)
        {
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
            // Debug.Log("Rook: Hareket edildi! Bonus sýfýrlandý.");
        }
        currentTimer = 0f;
        currentAppliedBonus = 0f;
    }
}