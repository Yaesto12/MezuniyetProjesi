using UnityEngine;
using System.Collections.Generic; // List için

[CreateAssetMenu(fileName = "New ItemData", menuName = "Gameplay/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string itemName = "Yeni Eþya";
    [TextArea] public string description = "Eþya açýklamasý...";
    public Sprite icon;
    public RarityLevel rarity = RarityLevel.Common;

    [Header("Eþya Etkileri")]
    [Tooltip("Bu eþyanýn etkilediði statlar listesi. Pozitif veya Negatif olabilir.")]
    public List<ItemStatModifier> modifiers = new List<ItemStatModifier>();

    [Header("Ayarlar")]
    [Tooltip("Bu eþya birden fazla kez toplanabilir mi?")]
    public bool isStackable = true;

    [Header("Özel Mekanik (Opsiyonel)")]
    [Tooltip("Sadece bu item alýndýðýnda çalýþacak özel bir script/görsel varsa, o prefabý buraya sürükle.")]
    public GameObject specialEffectPrefab;
}