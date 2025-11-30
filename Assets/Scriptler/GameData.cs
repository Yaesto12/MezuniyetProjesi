using UnityEngine;

// Bu script bir objeye eklenmez, sadece sahneler arasý veri tutar.
public static class GameData
{
    // Karakter seçim ekranýnda seçilen karakterin prefab'ý.
    // PlayerSpawner bunu kullanarak doðru karakteri oluþturur.
    public static GameObject SelectedCharacterPrefab;

    // Karakter seçim ekranýnda seçilen karakterin CharacterData asset'i.
    // PlayerWeaponController (ve belki PlayerExperience/PlayerStats)
    // baþlangýç ayarlarýný yapmak için bunu kullanýr.
    public static CharacterData SelectedCharacterDataForGame; // <<<--- EKSÝK OLAN SATIR BUYDU ---<<<
}