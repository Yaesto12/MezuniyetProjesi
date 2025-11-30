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

    [Header("Pasif Item Envanteri")]
    [Tooltip("Oyuncunun sahip olduðu pasif item'larý ve mevcut seviyelerini takip eder.")]
    public Dictionary<ItemData, int> ownedItems = new Dictionary<ItemData, int>();

    // --- Silah Metotlarý ---

    /// <summary>
    /// Oyuncunun belirtilen orijinal WeaponData asset'ine sahip olup olmadýðýný kontrol eder.
    /// </summary>
    public bool HasWeaponData(WeaponData originalWeaponData)
    {
        if (originalWeaponData == null)
        {
            return false; // Null ise sahip deðildir, 'return' eklendi.
        }

        // Klonlanmýþ instance'larýn isimlerini de kontrol et (StartsWith)
        // 'return' ifadesi eklendi.
        return ownedWeaponBlueprints.Contains(originalWeaponData) ||
               ownedWeaponBlueprints.Exists(instance => instance != null && instance.name.StartsWith(originalWeaponData.name));
    }

    /// <summary>
    /// Yeni bir silahýn orijinal WeaponData asset'ini envantere ekler.
    /// </summary>
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

    /// <summary>
    /// Belirtilen yükseltmenin mevcut seviyesini döndürür (0 eðer hiç alýnmamýþsa).
    /// </summary>
    public int GetUpgradeLevel(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return 0; // 'return' eklendi.
        }

        upgradeLevels.TryGetValue(upgrade, out int level);
        return level; // 'return' zaten vardý, ama 'if' bloðu eklendi.
    }

    /// <summary>
    /// Belirtilen yükseltmenin seviyesini bir artýrýr veya 1 olarak baþlatýr.
    /// </summary>
    public void IncrementUpgradeLevel(UpgradeData upgrade)
    {
        if (upgrade == null) return;
        if (upgradeLevels.ContainsKey(upgrade)) { upgradeLevels[upgrade]++; }
        else { upgradeLevels.Add(upgrade, 1); }
        Debug.Log($"[PlayerInventory] '{upgrade.upgradeName}' yükseltmesinin seviyesi arttý. Yeni seviye: {upgradeLevels[upgrade]}");
    }


    // --- Item Metotlarý ---

    /// <summary>
    /// Envantere yeni bir item ekler veya mevcut item'ýn seviyesini artýrýr.
    /// </summary>
    public void AddItem(ItemData item)
    {
        if (item == null) return;

        if (ownedItems.ContainsKey(item))
        {
            if (item.isStackable)
            {
                ownedItems[item]++;
                Debug.Log($"[PlayerInventory] '{item.itemName}' item'ýnýn seviyesi arttý. Yeni seviye: {ownedItems[item]}");
            }
            else
            {
                Debug.Log($"[PlayerInventory] '{item.itemName}' zaten envanterde ve stacklenemez.");
            }
        }
        else
        {
            ownedItems.Add(item, 1);
            Debug.Log($"[PlayerInventory] Yeni item eklendi: '{item.itemName}' (Seviye 1)");
        }

        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.RecalculateStats();
        }
        else
        {
            Debug.LogError("[PlayerInventory] AddItem: PlayerStats bulunamadý! Statlar güncellenmedi.");
        }
    }

    /// <summary>
    /// Belirtilen item'ýn mevcut seviyesini döndürür (0 eðer sahip deðilse).
    /// </summary>
    public int GetItemLevel(ItemData item)
    {
        if (item == null)
        {
            return 0; // 'return' eklendi.
        }

        ownedItems.TryGetValue(item, out int level);
        return level; // 'return' zaten vardý, ama 'if' bloðu eklendi.
    }
}