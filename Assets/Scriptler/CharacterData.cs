using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Gameplay/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string characterName = "Yeni Karakter";
    [TextArea] public string description = "Karakter açýklamasý...";
    public Sprite icon; // Seçim ekranýnda görünecek ikon

    [Header("Oyun Ýçi Veri")]
    [Tooltip("Bu karakter seçildiðinde oyunda oluþturulacak olan ana Prefab.")]
    public GameObject characterPrefab; // Karakterin kendisinin prefab'ý

    [Header("Baþlangýç Silahý")]
    [Tooltip("Bu karakterin baþlangýç silahýný tanýmlayan WeaponData asset'i.")]
    // <<<--- BU SATIRIN YORUMUNU KALDIRDIK ve DOÐRU HALÝYLE EKLEDÝK ---<<<
    public WeaponData startingWeaponData;
    // --------------------------------------------------------------------

    // Ýsteðe baðlý baþlangýç statlarý eklenebilir
    // public int startingHealth = 100;
    // public float startingSpeed = 5f;
}