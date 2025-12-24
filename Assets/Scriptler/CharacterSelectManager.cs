using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Karakter Verileri")]
    [Tooltip("Tüm seçilebilir karakterlerin CharacterData asset'leri.")]
    [SerializeField] private List<CharacterData> allCharacters;

    [Header("UI Referanslarý")]
    [Tooltip("Karakter butonlarýnýn oluþturulacaðý parent transform.")]
    [SerializeField] private Transform characterGridPanel;
    [Tooltip("Oluþturulacak karakter butonu prefab'ý.")]
    [SerializeField] private GameObject characterButtonPrefab;
    [Tooltip("Seçilen karakterin ikonunu gösteren Image.")]
    [SerializeField] private Image selectedCharacterIcon;
    [Tooltip("Seçilen karakterin adýný gösteren Text.")]
    [SerializeField] private TextMeshProUGUI selectedCharacterName;
    [Tooltip("Seçilen karakterin açýklamasýný gösteren Text.")]
    [SerializeField] private TextMeshProUGUI selectedCharacterDescription;
    [Tooltip("Oyunu baþlatma butonu.")]
    [SerializeField] private Button startGameButton;

    [Header("Sahne Adý")]
    [Tooltip("Yüklenecek olan ana oyun sahnesinin adý.")]
    [SerializeField] private string gameSceneName = "GameScene"; // KENDÝ SAHNENÝZÝN ADINI YAZIN

    // Dahili
    private CharacterData currentlySelectedCharacter = null;

    void Start()
    {
        PopulateCharacterGrid();
        // Baþlangýçta hiçbir karakter seçili deðil, bilgileri temizle ve baþlat butonunu kapat
        UpdateSelectedCharacterInfo(null);
        startGameButton.interactable = false;
        startGameButton.onClick.AddListener(StartGame); // Butonun týklama olayýný ayarla
    }

    /// <summary>
    /// Karakter listesine göre UI'daki butonlarý oluþturur.
    /// </summary>
    void PopulateCharacterGrid()
    {
        // 1. Önce eski butonlarý temizle
        for (int i = characterGridPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(characterGridPanel.GetChild(i).gameObject);
        }

        // 2. Her karakter için yeni buton oluþtur
        foreach (CharacterData character in allCharacters)
        {
            GameObject buttonGO = Instantiate(characterButtonPrefab, characterGridPanel);
            Button button = buttonGO.GetComponent<Button>();

            // --- DEÐÝÞÝKLÝK BURADA ---
            // "Tahmin etme" yöntemini sildik. Direkt isminden buluyoruz.
            // Prefabýn içindeki objenin adýný "IconImage" yaptýðýndan emin ol!
            Transform iconTransform = buttonGO.transform.Find("IconImage");

            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();

                // Eðer data dosyasýnda ikon varsa ata
                if (iconImage != null && character.icon != null)
                {
                    iconImage.sprite = character.icon;
                    iconImage.preserveAspect = true; // Resmi sündürme, oranýný koru
                    iconImage.enabled = true; // Görünür olduðundan emin ol
                }
                else if (character.icon == null)
                {
                    // Ýkon yoksa beyaz kare görünmesin diye resmi kapat
                    // (veya iconImage.color = Color.clear; yapabilirsin)
                    iconImage.enabled = false;
                }
            }
            else
            {
                // Eðer ismi yanlýþ yazdýysan konsolda seni uyarýr
                Debug.LogWarning($"DÝKKAT: '{character.name}' butonunda 'IconImage' isimli obje bulunamadý! Prefabý kontrol et.");
            }
            // -------------------------

            button.onClick.AddListener(() => SelectCharacter(character));
        }
    }

    /// <summary>
    /// Bir karakter butonuna týklandýðýnda çaðrýlýr.
    /// </summary>
    public void SelectCharacter(CharacterData character)
    {
        currentlySelectedCharacter = character;
        UpdateSelectedCharacterInfo(character);
        startGameButton.interactable = true; // Karakter seçildi, butonu aktif et
        Debug.Log("Karakter Seçildi: " + character.characterName);
    }

    /// <summary>
    /// Seçilen karakterin bilgilerini UI'da gösterir.
    /// </summary>
    void UpdateSelectedCharacterInfo(CharacterData character)
    {
        if (character != null)
        {
            if (selectedCharacterIcon != null)
            {
                selectedCharacterIcon.sprite = character.icon;
                selectedCharacterIcon.enabled = (character.icon != null);
            }
            if (selectedCharacterName != null) selectedCharacterName.text = character.characterName;
            if (selectedCharacterDescription != null) selectedCharacterDescription.text = character.description;
        }
        else // Hiçbir karakter seçili deðilse
        {
            if (selectedCharacterIcon != null) selectedCharacterIcon.enabled = false;
            if (selectedCharacterName != null) selectedCharacterName.text = "Karakter Seç";
            if (selectedCharacterDescription != null) selectedCharacterDescription.text = "";
        }
    }

    /// <summary>
    /// "Oyuna Baþla" butonuna týklandýðýnda çaðrýlýr.
    /// </summary>
    public void StartGame()
    {
        if (currentlySelectedCharacter != null && currentlySelectedCharacter.characterPrefab != null)
        {
            // Seçilen karakterin prefab'ýný GameData'ya kaydet
            GameData.SelectedCharacterPrefab = currentlySelectedCharacter.characterPrefab;

            // --- BU SATIR ÇOK ÖNEMLÝ ---
            // Seçilen karakterin CharacterData asset'ini de GameData'ya kaydet
            GameData.SelectedCharacterDataForGame = currentlySelectedCharacter;
            // --------------------------

            Debug.Log($"Oyuna baþlanýyor. Seçilen Karakter: {currentlySelectedCharacter.characterName}, Prefab: {currentlySelectedCharacter.characterPrefab.name}");

            // Ana oyun sahnesini yükle
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Oyuna baþlanamýyor! Geçerli bir karakter seçilmedi veya karakterin prefab'ý atanmamýþ.");
        }
    }
}