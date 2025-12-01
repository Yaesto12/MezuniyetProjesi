using UnityEngine;
using System.Collections;

public class Item_Steroids : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Scriptin item seviyesini okuyabilmesi için ItemData'yý buraya sürükleyin.")]
    [SerializeField] private ItemData myItemData;

    [Tooltip("Kaç saniye boyunca hasar artacak?")]
    [SerializeField] private float duration = 5f;

    [Tooltip("Ýlk seviyede hasar artýþ yüzdesi (Örn: 20 = %20).")]
    [SerializeField] private float baseDamageBoost = 20f;

    [Tooltip("Her ek seviyede eklenecek hasar yüzdesi (Örn: 10 = +%10).")]
    [SerializeField] private float bonusPerStack = 10f;

    private Coroutine buffCoroutine;
    private bool isBuffActive = false;
    private float currentAppliedAmount = 0f; // Þu an ne kadar güç veriyoruz?

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerSkillUsed += OnSkillUsed;
        }
    }

    public override void OnUnequip()
    {
        // Eðer item çýkarýlýrsa ve buff hala aktifse, buff'ý temizle
        if (isBuffActive)
        {
            RemoveBuff();
        }

        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerSkillUsed -= OnSkillUsed;
        }
        base.OnUnequip();
    }

    private void OnSkillUsed()
    {
        // Eðer zaten bir buff varsa, süreyi sýfýrlamak için önce eskisini kaldýr
        if (isBuffActive)
        {
            RemoveBuff();
            if (buffCoroutine != null) StopCoroutine(buffCoroutine);
        }

        // Yeni buff'ý baþlat
        buffCoroutine = StartCoroutine(BuffRoutine());
    }

    private IEnumerator BuffRoutine()
    {
        isBuffActive = true;

        // 1. Seviyeyi ve Hasar Miktarýný Hesapla
        int level = 1;
        if (owner != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null && myItemData != null)
            {
                level = inventory.GetItemLevel(myItemData);
            }
        }

        // Formül: Base + (Level-1 * Bonus)
        currentAppliedAmount = baseDamageBoost + (bonusPerStack * (level - 1));

        // 2. PlayerStats'a Ekle
        if (playerStats != null)
        {
            playerStats.AddTemporaryDamage(currentAppliedAmount);
            Debug.Log($"Steroids Aktif! {duration} saniye boyunca hasar +%{currentAppliedAmount}");
        }

        // 3. Süre Kadar Bekle
        yield return new WaitForSeconds(duration);

        // 4. Buff'ý Kaldýr
        RemoveBuff();
    }

    private void RemoveBuff()
    {
        if (playerStats != null && isBuffActive)
        {
            playerStats.RemoveTemporaryDamage(currentAppliedAmount);
            // Debug.Log("Steroids Etkisi Bitti.");
        }
        isBuffActive = false;
        currentAppliedAmount = 0f;
    }
}