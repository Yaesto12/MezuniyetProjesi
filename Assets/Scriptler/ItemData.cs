using UnityEngine;
using System.Collections.Generic; // List için

[CreateAssetMenu(fileName = "New ItemData", menuName = "Gameplay/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string itemName = "Yeni Eþya";
    [TextArea] public string description = "Eþya açýklamasý...";
    public Sprite icon;
    public RarityLevel rarity = RarityLevel.Common; // GameDefinitions'tan

    [Header("Eþya Etkileri")]
    [Tooltip("Bu eþyanýn etkilediði statlar listesi. Pozitif veya Negatif olabilir.")]
    public List<ItemStatModifier> modifiers = new List<ItemStatModifier>();

    [Header("Ayarlar")]
    [Tooltip("Bu eþya birden fazla kez toplanabilir mi?")]
    public bool isStackable = true;

    [Header("Düþme Þansý (Aðýrlýk)")]
    [Tooltip("Bu sayý ne kadar yüksekse, sandýktan çýkma ihtimali o kadar artar. (Örn: Common=100, Legendary=5)")]
    [Range(1, 1000)]
    public int dropWeight = 100; // Varsayýlan deðer 100

    [Header("Özel Mekanik (Opsiyonel)")]
    [Tooltip("Sadece bu item alýndýðýnda çalýþacak özel bir script/görsel varsa, o prefabý buraya sürükle.")]
    public GameObject specialEffectPrefab;

    // --- YENÝ EKLENEN AYAR ---
    [Tooltip("Eðer iþaretliyse, bu item her alýndýðýnda (stacklendiðinde) yeni bir efekt prefabý oluþturulur. (Fang Pendant gibi baðýmsýz çalýþan itemler için).")]
    public bool createEffectPerStack = false;
    // -------------------------
}