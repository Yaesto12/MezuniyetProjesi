using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için bu satýr GEREKLÝDÝR.

public class MainMenu : MonoBehaviour
{
    // [SerializeField] private string gameSceneName = "GameScene"; // <<<--- BU SATIR KALDIRILDI (Gereksiz) ---<<<

    /// <summary>
    /// "Oyna" butonuna týklandýðýnda bu fonksiyon çaðrýlýr.
    /// </summary>
    public void StartGame()
    {
        // Doðrudan Karakter Seçim sahnesini yükle
        SceneManager.LoadScene("CharacterSelect"); // Sahne adýnýzýn bu olduðundan emin olun
        Debug.Log("CharacterSelect sahnesi yükleniyor...");
    }

    /// <summary>
    /// "Çýkýþ" butonuna týklandýðýnda bu fonksiyon çaðrýlýr.
    /// </summary>
    public void ExitGame()
    {
        // Oyunu kapatýr (Sadece derlenmiþ oyunda çalýþýr, Editör'de deðil).
        Debug.Log("Oyundan çýkýlýyor..."); // Editörde çalýþtýðýný görmek için log mesajý
        Application.Quit();
    }
}