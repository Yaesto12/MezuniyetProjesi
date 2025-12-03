using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerInventory : MonoBehaviour
{
    [Header("Silah Envanteri")]
    [Tooltip("Oyuncunun sahip olduðu orijinal WeaponData asset'leri.")]
    public List<WeaponData> ownedWeaponBlueprints = new List<WeaponData>();

    [Header("Yükseltme Seviyeleri")]
    [Tooltip("Alýnan yükseltmelerin (UpgradeData) seviyelerini takip eder.")]
    public Dictionary<UpgradeData, int> upgradeLevels = new Dictionary<UpgradeData, int>();

    [Header("Para")]
    public int CurrentGold { get; private set; } = 0;

    [Header("Pasif Item Envanteri")]
    [Tooltip("Oyuncunun sahip olduðu pasif item'larý ve mevcut seviyelerini takip eder.")]
    public Dictionary<ItemData, int> ownedItems = new Dictionary<ItemData, int>();

    // --- Silah Metotlarý ---

    public bool HasWeaponData(WeaponData originalWeaponData)
    {
        if (originalWeaponData == null) return false;
        return ownedWeaponBlueprints.Contains(originalWeaponData) ||
               ownedWeaponBlueprints.Exists(instance => instance != null && instance.name.StartsWith(originalWeaponData.name));
    }

    public void AddWeaponData(WeaponData originalWeaponData)
    {
        if (originalWeaponData == null) return;
        if (!HasWeaponData(originalWeaponData))
        {
            ownedWeaponBlueprints.Add(originalWeaponData);
            Debug.Log($"[PlayerInventory] {originalWeaponData.weaponName} (Orijinal Asset) envantere eklendi.");
        }
    }

    // --- Yükseltme Metotlarý ---

    public int GetUpgradeLevel(UpgradeData upgrade)
    {
        if (upgrade == null) return 0;
        upgradeLevels.TryGetValue(upgrade, out int level);
        return level;
    }

    public void IncrementUpgradeLevel(UpgradeData upgrade)
    {
        if (upgrade == null) return;
        if (upgradeLevels.ContainsKey(upgrade)) { upgradeLevels[upgrade]++; }
        else { upgradeLevels.Add(upgrade, 1); }
        Debug.Log($"[PlayerInventory] '{upgrade.upgradeName}' yükseltmesinin seviyesi arttý. Yeni seviye: {upgradeLevels[upgrade]}");
    }


    // --- Item Metotlarý (GÜNCELLENMÝÞ HALÝ) ---

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        // 1. Item'ý Dictionary'e Ekle / Artýr
        if (ownedItems.ContainsKey(item))
        {
            if (item.isStackable)
            {
                ownedItems[item]++;
                Debug.Log($"[PlayerInventory] '{item.itemName}' seviyesi arttý: {ownedItems[item]}");
            }
            else
            {
                Debug.Log($"[PlayerInventory] '{item.itemName}' zaten var ve stacklenemez.");
                return;
            }
        }
        else
        {
            ownedItems.Add(item, 1);
            Debug.Log($"[PlayerInventory] Yeni item alýndý: '{item.itemName}'");
        }

        // 2. --- ÖZEL EFEKT / MEKANÝK BAÞLATMA ---
        if (item.specialEffectPrefab != null)
        {
            // KURAL: Ya item ilk kez alýnmýþtýr (Seviye 1)
            // YA DA item "Her Stack Ýçin Oluþtur" (createEffectPerStack) modundadýr.
            if (ownedItems[item] == 1 || item.createEffectPerStack)
            {
                // Prefab'ý oyuncunun çocuðu olarak oluþtur
                GameObject effectObj = Instantiate(item.specialEffectPrefab, transform.position, Quaternion.identity, transform);

                // Ýsimlendirme (Karýþýklýðý önlemek için seviyeyi ekleyelim)
                effectObj.name = $"{item.itemName}_Effect_Stack{ownedItems[item]}";

                // ItemEffect scriptini bul ve baþlat
                ItemEffect effectScript = effectObj.GetComponent<ItemEffect>();
                if (effectScript != null)
                {
                    effectScript.OnEquip(GetComponent<PlayerStats>(), this);
                }

                Debug.Log($"[PlayerInventory] Özel efekt oluþturuldu: {effectObj.name}");
            }
        }
        // ---------------------------------------------

        // 3. Statlarý Yeniden Hesapla
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.RecalculateStats();
        }
        else
        {
            Debug.LogError("[PlayerInventory] PlayerStats bulunamadý! Statlar güncellenmedi.");
        }
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        // Debug.Log($"Altýn Kazanýldý: {amount}. Toplam: {CurrentGold}");
        // Burada UI güncellemesi (UIManager) çaðýrýlabilir.
    }

    public int GetItemLevel(ItemData item)
    {
        if (item == null) return 0;
        ownedItems.TryGetValue(item, out int level);
        return level;
    }
}