using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Referanslarý")]
    [Tooltip("Oyun durduðunda açýlacak olan panel (Pause Menüsü).")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Oyun sýrasýnda görünen, duraklatýnca gizlenecek olan ana UI objesi (Can, XP, Altýn vb.).")]
    [SerializeField] private GameObject gameplayUI; // <<<--- YENÝ EKLENDÝ ---<<<

    [Header("Sahne Ayarlarý")]
    [Tooltip("Çýkýþ butonuna basýldýðýnda dönülecek Ana Menü sahnesinin adý.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // Oyunun durup durmadýðýný takip eden deðiþken
    public static bool IsGamePaused = false;

    void Update()
    {
        // ESC tuþuna basýldýðýný kontrol et
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
            {
                ResumeGame(); // Zaten durmuþsa devam et
            }
            else
            {
                PauseGame(); // Çalýþýyorsa durdur
            }
        }
    }

    /// <summary>
    /// Oyunu devam ettirir, menüyü kapatýr ve oyun arayüzünü geri açar.
    /// </summary>
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false); // Pause menüsünü gizle

        // --- YENÝ: Oyun Arayüzünü Geri Aç ---
        if (gameplayUI != null) gameplayUI.SetActive(true);
        // ------------------------------------

        Time.timeScale = 1f; // Zamaný normal akýþýna döndür
        IsGamePaused = false;

        // Mouse imlecini tekrar kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Oyunu durdurur, menüyü açar ve oyun arayüzünü gizler.
    /// </summary>
    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true); // Pause menüsünü göster

        // --- YENÝ: Oyun Arayüzünü Gizle ---
        if (gameplayUI != null) gameplayUI.SetActive(false);
        // ----------------------------------

        Time.timeScale = 0f; // Zamaný tamamen durdur
        IsGamePaused = true;

        // Menüde týklama yapabilmek için mouse'u serbest býrak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan Çýkýlýyor...");
        Application.Quit();
    }
}