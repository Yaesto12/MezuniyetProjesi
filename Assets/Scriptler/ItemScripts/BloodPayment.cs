using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Item_BloodPayment : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Ödeme Ayarlarý")]
    [Tooltip("Kaç saniyede bir kan ödemesi alýnsýn?")]
    [SerializeField] private float paymentInterval = 10f;

    [Tooltip("Ýlk seviyede oyuncudan ne kadar can alýnsýn?")]
    [SerializeField] private int baseHealthCost = 5;

    [Tooltip("Her item kopyasýnda (stack) can bedeli ne kadar artsýn?")]
    [SerializeField] private int costPerStack = 5; // YENÝ: Bedel artýþý

    [Header("Ödül Ayarlarý")]
    [Tooltip("Ýlk seviyede stat artýþ miktarý.")]
    [SerializeField] private float baseStatBoost = 1f;

    [Tooltip("Her stack için eklenecek ekstra artýþ miktarý.")]
    [SerializeField] private float boostPerStack = 0.5f;

    // Artýrýlabilecek Olasý Statlar Listesi
    private List<PassiveStatType> possibleStats = new List<PassiveStatType>
    {
        PassiveStatType.MoveSpeed,
        PassiveStatType.Damage,
        PassiveStatType.CritChance,
        PassiveStatType.AttackSpeed,
        PassiveStatType.Armor,
        PassiveStatType.Luck,
        PassiveStatType.XpBonus,
        PassiveStatType.GoldBonus,
        // Ýstediðin diðer statlarý ekleyebilirsin
    };

    private float timer = 0f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        timer = paymentInterval; // Baþlangýçta bekle
    }

    private void Update()
    {
        if (playerStats == null || owner == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TryMakePayment();
            timer = paymentInterval;
        }
    }

    private void TryMakePayment()
    {
        PlayerHealth health = owner.GetComponent<PlayerHealth>();
        if (health == null) return;

        // 1. Item Seviyesini (Stack) Bul
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        // 2. Güncel Bedeli Hesapla (Stack arttýkça bedel de artar)
        int currentCost = baseHealthCost + (costPerStack * (stack - 1));

        // 3. Can Kontrolü (Ýstek #1: Can yetmezse stat artýþý olmaz)
        // CanAffordHealthCost, oyuncuyu öldürmeyecek kadar caný olup olmadýðýný kontrol eder.
        if (health.CanAffordHealthCost(currentCost))
        {
            // Bedeli Öde
            health.TakeDamage(currentCost);
            // Debug.Log($"Blood Payment: {currentCost} can ödendi!");

            // Ödülü Ver (Stack bilgisini gönderiyoruz, tekrar hesaplamasýn)
            GiveRandomReward(stack);
        }
        else
        {
            // Debug.Log("Blood Payment: Yetersiz can, ödeme ve ödül atlandý.");
        }
    }

    private void GiveRandomReward(int stack)
    {
        // Artýþ Miktarýný Hesapla
        float boostAmount = baseStatBoost + (boostPerStack * (stack - 1));

        // Rastgele Stat Seç
        PassiveStatType randomStat = possibleStats[Random.Range(0, possibleStats.Count)];

        // Statý Artýr
        ApplyStatBoost(randomStat, boostAmount);

        Debug.Log($"Blood Payment Ödülü (Stack {stack}): {randomStat} +{boostAmount} arttý!");
    }

    private void ApplyStatBoost(PassiveStatType statType, float amount)
    {
        switch (statType)
        {
            case PassiveStatType.MoveSpeed: playerStats.IncreaseBaseMoveSpeed(amount * 0.1f); break; // Hýz hassas olduðu için böldük
            case PassiveStatType.Damage: playerStats.IncreaseBaseDamageMultiplier(amount); break;
            case PassiveStatType.CritChance: playerStats.IncreaseBaseCritChance(amount); break;
            case PassiveStatType.AttackSpeed: playerStats.IncreaseBaseAttackSpeedMultiplier(amount); break;
            case PassiveStatType.Armor: playerStats.IncreaseBaseArmor(amount); break;
            case PassiveStatType.Luck: playerStats.IncreaseBaseLuck(amount); break;
            case PassiveStatType.XpBonus: playerStats.IncreaseBaseXpBonus(amount); break;
            case PassiveStatType.GoldBonus: playerStats.IncreaseBaseGoldBonus(amount); break;
                // Diðer statlar...
        }
    }
}