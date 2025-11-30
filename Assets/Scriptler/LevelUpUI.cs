using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Yardýmcý sýnýflar (GeneratedUpgradeOption, StatModification)
// artýk GameDefinitions.cs dosyasýnda tanýmlý.

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private GameObject panelObject; // Panelin kendisi

    [Header("Seçenek Yuvalarý (3 adet olmalý)")]
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionNameTexts;
    [SerializeField] private TextMeshProUGUI[] optionDescriptionTexts;
    [SerializeField] private Image[] optionIcons;

    private List<GeneratedUpgradeOption> currentOptions; // O an gösterilen dinamik seçenekler
    private PlayerExperience playerExperience; // Seçimi bildirmek için

    void Awake()
    {
        playerExperience = FindFirstObjectByType<PlayerExperience>();
        if (playerExperience == null) Debug.LogError("LevelUpUI: PlayerExperience bulunamadý!");

        if (optionButtons == null || optionButtons.Length == 0)
        {
            Debug.LogError("LevelUpUI: Butonlar (Option Buttons) Inspector'dan atanmamýþ!", this);
            return;
        }

        // Butonlara listenerlarý (týklama olaylarý) ekle
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // Lambda için index'i yakala
            if (optionButtons[i] != null)
            {
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => SelectUpgrade(index));
            }
        }
    }

    /// <summary>
    /// Paneli gösterir ve dinamik olarak oluþturulan seçenekleri butonlara atar.
    /// </summary>
    public void ShowOptions(List<GeneratedUpgradeOption> availableOptions)
    {
        if (panelObject == null) { Debug.LogError("LevelUpUI: Panel Object atanmamýþ!"); return; }

        panelObject.SetActive(true);
        this.currentOptions = availableOptions;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null || optionNameTexts[i] == null || optionDescriptionTexts[i] == null || optionIcons[i] == null)
            {
                Debug.LogWarning($"LevelUpUI: Option Slot {i} için tüm UI elementleri atanmamýþ.");
                continue;
            }

            if (i < availableOptions.Count && availableOptions[i] != null)
            {
                GeneratedUpgradeOption option = availableOptions[i];

                optionNameTexts[i].text = option.Name;
                optionDescriptionTexts[i].text = option.Description;

                if (option.Icon != null)
                {
                    optionIcons[i].sprite = option.Icon;
                    optionIcons[i].enabled = true;
                }
                else
                {
                    optionIcons[i].enabled = false;
                }
                optionButtons[i].gameObject.SetActive(true);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Bir butona týklandýðýnda çaðrýlýr.
    /// </summary>
    private void SelectUpgrade(int optionIndex)
    {
        if (playerExperience != null && currentOptions != null && optionIndex < currentOptions.Count)
        {
            playerExperience.ApplyGeneratedUpgrade(currentOptions[optionIndex]);
        }
        else
        {
            Debug.LogError($"SelectUpgrade: Geçersiz seçim!");
        }

        if (panelObject != null) panelObject.SetActive(false);
    }
}