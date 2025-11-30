using UnityEngine;
// using TMPro; // Artýk gerekli deðil

public class PlayerWallet : MonoBehaviour
{
    // --- UI Referansý KALDIRILDI ---
    // [Header("UI Referanslarý")]
    // [SerializeField] private TextMeshProUGUI goldText; // KALDIRILDI

    private int currentGold = 0;

    void Start()
    {
        // Baþlangýç UI'ý UIManager üzerinden ayarla
        UpdateUIInternal();
    }

    /// <summary>
    /// Bu fonksiyon, bir altýn toplandýðýnda çaðrýlýr.
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return; // Negatif altýn eklemeyi engelle
        currentGold += amount;
        UpdateUIInternal(); // UI Manager'ý çaðýr
    }

    /// <summary>
    /// Altýn metnini UIManager üzerinden günceller.
    /// </summary>
    private void UpdateUIInternal() // Ýsmi deðiþtirdik
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGoldText(currentGold);
        }
        else
        {
            Debug.LogError("PlayerWallet: UIManager bulunamadý!");
        }
    }
}