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
        // Önce eski butonlarý temizle (varsa)
        foreach (Transform child in characterGridPanel)
        {
            Destroy(child.gameObject);
        }

        // Her karakter için bir buton oluþtur
        foreach (CharacterData character in allCharacters)
        {
            GameObject buttonGO = Instantiate(characterButtonPrefab, characterGridPanel);
            Button button = buttonGO.GetComponent<Button>();
            Image iconImage = buttonGO.GetComponentInChildren<Image>(); // Butonun içindeki Image'ý bulduðunu varsayýyoruz
            // TextMeshProUGUI nameText = buttonGO.GetComponentInChildren<TextMeshProUGUI>(); // Ýsterseniz butona isim de ekleyebilirsiniz

            if (iconImage != null && character.icon != null)
            {
                iconImage.sprite = character.icon;
            }
            // if (nameText != null) nameText.text = character.characterName;

            // Butona týklandýðýnda SelectCharacter fonksiyonunu çaðýrmasýný saðla
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