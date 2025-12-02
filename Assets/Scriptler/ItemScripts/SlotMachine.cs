using UnityEngine;
using System.Collections.Generic;

public class Item_SlotMachine : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Scriptin item seviyesini okuyabilmesi için ItemData'yı buraya sürükleyin.")]
    [SerializeField] private ItemData myItemData; // <<<--- EKLENEN KISIM ---<<<

    [Header("Zaman Ayarları")]
    [Tooltip("Kaç saniyede bir ödül verilsin?")]
    [SerializeField] private float payoutInterval = 60f;

    [Header("Ödül Havuzu")]
    [Tooltip("Makinenin verebileceği itemlerin listesi. Buraya ItemData assetlerini sürükleyin.")]
    [SerializeField] private List<ItemData> prizePool;

    private float timer;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        timer = payoutInterval; // Geri sayımı başlat
    }

    private void Update()
    {
        if (owner == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            PullTheLever();
            timer = payoutInterval;
        }
    }

    private void PullTheLever()
    {
        if (prizePool == null || prizePool.Count == 0)
        {
            Debug.LogWarning("Slot Machine: Ödül havuzu boş! Lütfen Inspector'dan item ekleyin.");
            return;
        }

        // 1. Rastgele bir item seç
        int randomIndex = Random.Range(0, prizePool.Count);
        ItemData selectedPrize = prizePool[randomIndex];

        // 2. Envantere ekle
        PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            // Ses veya efekt eklenebilir
            Debug.Log($"🎰 JACKPOT! Slot Machine verdi: {selectedPrize.itemName}");

            inventory.AddItem(selectedPrize);
        }
    }
}