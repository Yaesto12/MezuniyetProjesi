using UnityEngine;

// Enum'lar artýk GameDefinitions.cs dosyasýnda tanýmlý

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Gameplay/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string upgradeName; // Örn: "Patates Fýrlatýcýyý Geliþtir", "Yeni Silah: Hasar Aurasý"
    [TextArea] public string description; // Genel açýklama
    public Sprite icon;

    [Header("Nadirlik")]
    [Tooltip("Bu 'Fýrsat'ýn bulunma ihtimali.")]
    public RarityLevel rarity = RarityLevel.Common;

    [Header("Yükseltme Kategorisi")]
    [Tooltip("Bu yükseltme ne yapýyor?")]
    public UpgradeCategory category; // Sadece WeaponUnlock veya WeaponUpgrade

    [Header("Ayarlar: Silah Açma")]
    [Tooltip("Eðer 'WeaponUnlock' ise, kilidi açýlacak olan WeaponData asset'i.")]
    public WeaponData weaponDataToUnlock;

    [Header("Ayarlar: Silah Geliþtirme")]
    [Tooltip("Eðer 'WeaponUpgrade' ise, geliþtirilecek olan WeaponData asset'i.")]
    public WeaponData targetWeaponData; // Örn: WD_BaslangicPatates
}