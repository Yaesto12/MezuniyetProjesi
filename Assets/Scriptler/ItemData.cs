using UnityEngine;

// Enum'lar artýk GameDefinitions.cs dosyasýnda tanýmlý

[CreateAssetMenu(fileName = "New ItemData", menuName = "Gameplay/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string itemName = "Yeni Eþya";
    [TextArea] public string description = "Eþya açýklamasý...";
    public Sprite icon;
    public RarityLevel rarity = RarityLevel.Common; // GameDefinitions'tan

    [Header("Eþya Etkisi")]
    [Tooltip("Bu eþyanýn hangi statý etkileyeceði.")]
    public PassiveStatType effectType; // GameDefinitions'tan

    [Tooltip("Statý ne kadar deðiþtireceði (sabit deðer veya yüzde).")]
    public float effectValue;

    [Tooltip("Deðer yüzde olarak mý (True) yoksa sabit bir ekleme mi (False)?")]
    public bool isPercentageBased = false;

    [Header("Seviye Atlama (Opsiyonel)")]
    [Tooltip("Bu eþya birden fazla kez toplanabilir mi (seviye atlar mý)?")]
    public bool isStackable = true;

    [Tooltip("Eðer 'isStackable' iþaretliyse, her seviyede eklenecek deðer.")]
    public float valuePerLevel = 2f;
}