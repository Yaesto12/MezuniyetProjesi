using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçiþi için gerekli

public class GameOverManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    [SerializeField] private GameObject deathScreenPanel;

    // Singleton: Her yerden ulaþabilmek için
    public static GameOverManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void ShowDeathScreen()
    {
        // 1. Paneli Aç
        deathScreenPanel.SetActive(true);

        // 2. Oyunu Durdur
        Time.timeScale = 0f;

        // 3. Mouse Ýmlecini Serbest Býrak (Kilitliyse aç)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- BUTON 1: ANA MENÜYE DÖN ---
    public void GoToMainMenu()
    {
        // Zamaný tekrar akýt (Önemli! Yoksa menü de donuk açýlýr)
        Time.timeScale = 1f;

        // Ana menü sahnemizin adý "MainMenu" olmalý.
        // Eðer senin sahne adýn farklýysa parantez içini deðiþtir.
        SceneManager.LoadScene("MainMenu");
    }

    // --- BUTON 2: OYUNDAN ÇIK ---
    public void QuitGame()
    {
        Debug.Log("Oyundan Çýkýldý! (Editörde pencere kapanmaz, Build alýnca çalýþýr)");

        // Oyunu tamamen kapatýr
        Application.Quit();
    }
}