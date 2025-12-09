using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    // EKONOMÝ KISMI SÝLÝNDÝ (Artýk PlayerStats'ta)

    [Header("Ýstatistikler")]
    public int EliteKills { get; private set; } = 0;

    [Header("Silahlar")]
    public List<WeaponData> weapons = new List<WeaponData>();

    [Header("Itemler")]
    public List<ItemData> ownedItems = new List<ItemData>();

    private PlayerStats playerStats;
    private PlayerWeaponController weaponController;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        // Controller yoksa hata vermesin diye generic kontrol ekledik
        weaponController = GetComponent<PlayerWeaponController>();
    }

    public void AddEliteKill()
    {
        EliteKills++;
    }

    // ========================================================================
    // --- ITEM YÖNETÝMÝ (BU KISIM ÖNEMLÝ) ---
    // ========================================================================

    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("HATA: AddItem'e boþ (null) item gönderildi!");
            return;
        }

        ownedItems.Add(item);

        // KONSOLA LOG BASIYORUZ KÝ GÖREBÝLELÝM
        Debug.Log($"<color=green>ENVANTERE EKLENDÝ: {item.itemName}</color>");

        // Item efektini baþlat (Eðer itemin bir efekti varsa)
        if (item.specialEffectPrefab != null && playerStats != null)
        {
            if (GetItemLevel(item) == 1)
            {
                GameObject effectObj = Instantiate(item.specialEffectPrefab, transform.position, Quaternion.identity);
                effectObj.transform.SetParent(transform);

                // Generic kontrol ile script var mý bakýyoruz
                var effectScript = effectObj.GetComponent<MonoBehaviour>();
                if (effectScript != null)
                {
                    // Reflection ile metodu çaðýrmayý dener (Script adýný bilmediðim için)
                    effectScript.SendMessage("OnEquip", new object[] { playerStats, this }, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    public int GetItemLevel(ItemData itemToCheck)
    {
        int count = 0;
        foreach (var item in ownedItems)
        {
            if (item == itemToCheck) count++;
        }
        return count;
    }

    // ========================================================================
    // --- SÝLAH YÖNETÝMÝ ---
    // ========================================================================

    public bool HasWeaponData(WeaponData weaponData)
    {
        return weapons.Contains(weaponData);
    }

    public void AddWeaponData(WeaponData weaponData)
    {
        if (!weapons.Contains(weaponData))
        {
            weapons.Add(weaponData);

            if (weaponController != null)
            {
                weaponController.SendMessage("AddWeapon", weaponData, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void IncrementUpgradeLevel(WeaponData weaponData)
    {
        if (weaponController != null)
        {
            weaponController.SendMessage("LevelUpWeapon", weaponData, SendMessageOptions.DontRequireReceiver);
        }
    }
}