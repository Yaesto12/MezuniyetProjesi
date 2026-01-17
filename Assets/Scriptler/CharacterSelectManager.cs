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

    [Header("Seçilen Karakter Detaylarý")]
    [Tooltip("Seçilen karakterin BÜYÜK ikonunu gösteren Image.")]
    [SerializeField] private Image selectedCharacterIcon; // <-- Ýþte fotoðrafýn görüneceði yer burasý
    [Tooltip("Seçilen karakterin adýný gösteren Text.")]
    [SerializeField] private TextMeshProUGUI selectedCharacterName;
    [Tooltip("Seçilen karakterin açýklamasýný gösteren Text.")]
    [SerializeField] private TextMeshProUGUI selectedCharacterDescription;
    [Tooltip("Oyunu baþlatma butonu.")]
    [SerializeField] private Button startGameButton;

    [Header("Sahne Adý")]
    [Tooltip("Yüklenecek olan ana oyun sahnesinin adý.")]
    [SerializeField] private string gameSceneName = "GameScene";

    // Dahili
    private CharacterData currentlySelectedCharacter = null;

    void Start()
    {
        PopulateCharacterGrid();

        // Baþlangýçta hiçbir karakter seçili deðil, bilgileri temizle
        UpdateSelectedCharacterInfo(null);

        startGameButton.interactable = false;
        startGameButton.onClick.AddListener(StartGame);
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

            // Butonun üzerindeki küçük ikon görseli
            Transform iconTransform = buttonGO.transform.Find("IconImage");

            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();

                if (iconImage != null && character.icon != null)
                {
                    iconImage.sprite = character.icon;
                    iconImage.preserveAspect = true; // Resmi oranlý tut
                    iconImage.enabled = true;
                }
                else
                {
                    if (iconImage != null) iconImage.enabled = false;
                }
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
        UpdateSelectedCharacterInfo(character); // <--- Fotoðraf burada güncelleniyor
        startGameButton.interactable = true;
        Debug.Log("Karakter Seçildi: " + character.characterName);
    }

    /// <summary>
    /// Seçilen karakterin bilgilerini UI'da (Büyük Resim, Ýsim, Açýklama) gösterir.
    /// </summary>
    void UpdateSelectedCharacterInfo(CharacterData character)
    {
        if (character != null)
        {
            // --- FOTOÐRAF GÜNCELLEME KISMI ---
            if (selectedCharacterIcon != null)
            {
                if (character.icon != null)
                {
                    selectedCharacterIcon.sprite = character.icon;
                    selectedCharacterIcon.preserveAspect = true; // ÖNEMLÝ: Resmin kare/dikdörtgen oranýný korur, yamulmasýný engeller.
                    selectedCharacterIcon.enabled = true;

                    // Görünürlüðü (Alpha) tam yap (eðer baþlangýçta þeffafsa)
                    Color c = selectedCharacterIcon.color;
                    c.a = 1f;
                    selectedCharacterIcon.color = c;
                }
                else
                {
                    // Ýkon yoksa gizle
                    selectedCharacterIcon.enabled = false;
                }
            }
            // ----------------------------------

            if (selectedCharacterName != null) selectedCharacterName.text = character.characterName;
            if (selectedCharacterDescription != null) selectedCharacterDescription.text = character.description;
        }
        else // Hiçbir karakter seçili deðilse (Oyun baþý)
        {
            if (selectedCharacterIcon != null)
            {
                selectedCharacterIcon.enabled = false; // Resmi gizle
                selectedCharacterIcon.sprite = null;
            }

            if (selectedCharacterName != null) selectedCharacterName.text = "Karakter Seç";
            if (selectedCharacterDescription != null) selectedCharacterDescription.text = "";
        }
    }

    public void StartGame()
    {
        if (currentlySelectedCharacter != null && currentlySelectedCharacter.characterPrefab != null)
        {
            GameData.SelectedCharacterPrefab = currentlySelectedCharacter.characterPrefab;
            GameData.SelectedCharacterDataForGame = currentlySelectedCharacter;

            Debug.Log($"Oyuna baþlanýyor. Seçilen: {currentlySelectedCharacter.characterName}");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Hata: Karakter seçilmedi veya Prefab eksik!");
        }
    }
}