using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
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
    // --- ITEM YÖNETÝMÝ ---
    // ========================================================================

    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("HATA: AddItem'e boþ (null) item gönderildi!");
            return;
        }

        ownedItems.Add(item);
        Debug.Log($"<color=green>ENVANTERE EKLENDÝ: {item.itemName}</color>");

        // --- STAT GÜNCELLEME SÝSTEMÝ (ARTIK AKTÝF!) ---
        // GameDefinitions.cs içindeki PassiveStatType ve ItemStatModifier kullanýlýyor.
        if (item.modifiers != null && playerStats != null)
        {
            foreach (var mod in item.modifiers)
            {
                // GameDefinitions'ta deðiþken adý 'baseAmount' olarak geçiyor.
                float val = mod.baseAmount;

                switch (mod.statType)
                {
                    // Temel Statlar
                    case PassiveStatType.MaxHealth: playerStats.IncreaseBaseMaxHealth(val); break;
                    case PassiveStatType.HpRegen: playerStats.IncreaseBaseHpRegen(val); break;
                    case PassiveStatType.Armor: playerStats.IncreaseBaseArmor(val); break;
                    case PassiveStatType.Evasion:
                        // PlayerStats'ta IncreaseBaseEvasion yoksa burasý hata verebilir, 
                        // varsa ekle: playerStats.IncreaseBaseEvasion(val); 
                        break;

                    // Hareket
                    case PassiveStatType.MoveSpeed: playerStats.IncreaseBaseMoveSpeed(val); break;

                    // Saldýrý
                    case PassiveStatType.Damage: playerStats.IncreaseBaseDamageMultiplier(val); break;
                    case PassiveStatType.AttackSpeed: playerStats.IncreaseBaseAttackSpeedMultiplier(val); break;
                    case PassiveStatType.CritChance: playerStats.IncreaseBaseCritChance(val); break;
                    // case PassiveStatType.CritDamage: playerStats.IncreaseBaseCritDamage(val); break; // PlayerStats'ta metodu varsa aç

                    // Þans ve Ekonomi
                    case PassiveStatType.Luck: playerStats.IncreaseBaseLuck(val); break;
                    case PassiveStatType.MagnetRange: playerStats.IncreaseBaseMagnetRange(val); break;
                    case PassiveStatType.XpBonus: playerStats.IncreaseBaseXpBonus(val); break;
                    case PassiveStatType.GoldBonus: playerStats.IncreaseBaseGoldBonus(val); break;

                    // Diðer
                    case PassiveStatType.Revival: playerStats.IncreaseBaseRevivals((int)val); break;

                        // PlayerStats.cs'de karþýlýðý olan diðer statlarý buraya ekleyebilirsin.
                        // Örneðin: Reroll, Skip, Banish vb. GameDefinitions'ta varsa buraya ekle.
                }
            }

            // Tüm eklemeler bitince statlarý yeniden hesapla
            playerStats.RecalculateStats();

            // UI'yý güncelle (PauseMenuInfo singleton'ý varsa)
            if (PauseMenuInfo.Instance != null && PauseMenuInfo.Instance.gameObject.activeInHierarchy)
            {
                // Pause menüsü açýksa anlýk güncellensin diye kapatýp açabilirsin
                // Veya PauseMenuInfo scriptine public void Refresh() ekleyip çaðýrabilirsin.
                PauseMenuInfo.Instance.gameObject.SetActive(false);
                PauseMenuInfo.Instance.gameObject.SetActive(true);
            }
        }

        // --- EFEKT KISMI (ÖZEL ITEMLER ÝÇÝN) ---
        if (item.specialEffectPrefab != null && playerStats != null)
        {
            // Eðer stacklenebilir deðilse veya ilk kez alýyorsak efekti yarat
            // 'createEffectPerStack' deðiþkeni ItemData'da yeni eklediðimiz özellik.
            if (!item.createEffectPerStack && GetItemLevel(item) > 1)
            {
                // Efekt zaten var, sadece stat arttýrdýk, tekrar obje yaratmýyoruz.
            }
            else
            {
                GameObject effectObj = Instantiate(item.specialEffectPrefab, transform.position, Quaternion.identity);
                effectObj.transform.SetParent(transform);

                // Generic kontrol ile script var mý bakýyoruz
                var effectScript = effectObj.GetComponent<MonoBehaviour>();
                if (effectScript != null)
                {
                    // Efekt scriptine statlarý ve envanteri gönderiyoruz
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