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
        // 1. Önce eski butonlarý temizle (Editörde kalanlarý temizlemek için önemli)
        // Transform.GetChild ile tersten dönerek silmek daha güvenlidir.
        for (int i = characterGridPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(characterGridPanel.GetChild(i).gameObject);
        }

        // 2. Her karakter için yeni buton oluþtur
        foreach (CharacterData character in allCharacters)
        {
            GameObject buttonGO = Instantiate(characterButtonPrefab, characterGridPanel);
            Button button = buttonGO.GetComponent<Button>();

            // ÝKONU BULMA:
            // Yöntem A: Prefabýn içinde "Icon" adýnda bir obje varsa onu bul
            // Transform iconTransform = buttonGO.transform.Find("Icon");
            // Image iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;

            // Yöntem B (Daha genel): Butonun kendisindeki Image (arka plan) deðil, altýndaki ilk Image'i bul
            Image[] images = buttonGO.GetComponentsInChildren<Image>();
            Image iconImage = null;

            // Genelde [0] butonun kendisi, [1] ise ikondur.
            if (images.Length > 1) iconImage = images[1];
            else if (images.Length == 1 && images[0].gameObject != buttonGO) iconImage = images[0];

            if (iconImage != null && character.icon != null)
            {
                iconImage.sprite = character.icon;
                iconImage.preserveAspect = true; // Ýkonun en-boy oranýný koru
            }

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