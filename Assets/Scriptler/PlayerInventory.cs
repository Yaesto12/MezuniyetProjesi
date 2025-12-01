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

    public bool HasWeaponData(WeaponData originalWeaponData)
    {
        if (originalWeaponData == null) return false;

        // Klonlanmýþ instance'larýn isimlerini de kontrol et
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


    // --- Item Metotlarý (GÜNCELLENEN KISIM) ---

    /// <summary>
    /// Envantere yeni bir item ekler. Statlarý günceller ve varsa özel efekti baþlatýr.
    /// </summary>
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

        // 2. --- YENÝ: Özel Efekt / Mekanik Baþlatma ---
        // Eðer bu item'ýn özel bir prefabý varsa ve item ilk kez alýndýysa oluþtur.
        if (item.specialEffectPrefab != null)
        {
            // Sadece ilk seviyede (1) oluþturuyoruz ki her stack'te tekrar tekrar oluþmasýn.
            if (ownedItems[item] == 1)
            {
                // Prefab'ý oyuncunun çocuðu (child) olarak oluþtur
                GameObject effectObj = Instantiate(item.specialEffectPrefab, transform.position, Quaternion.identity, transform);
                effectObj.name = $"{item.itemName}_Effect";

                // ItemEffect scriptini bul ve baþlat (Strategy Pattern)
                ItemEffect effectScript = effectObj.GetComponent<ItemEffect>();
                if (effectScript != null)
                {
                    // PlayerStats'ý ve Player'ýn kendisini (this) gönder
                    effectScript.OnEquip(GetComponent<PlayerStats>(), this);
                }

                Debug.Log($"[PlayerInventory] Özel efekt oluþturuldu ve baþlatýldý: {effectObj.name}");
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

    public int GetItemLevel(ItemData item)
    {
        if (item == null) return 0;
        if (item == null) return 0;
        if (item == null) return 0;
        if (item == null) return 0;
        ownedItems.TryGetValue(item, out int level);
        return level;
    }
}