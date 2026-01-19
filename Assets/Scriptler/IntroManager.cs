using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçiþleri için þart

public class IntroManager : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Animasyon veya video kaç saniye sürüyor?")]
    public float waitTime = 5f;

    [Tooltip("Süre bitince hangi sahne açýlacak? (Tam adýný yaz)")]
    public string nextSceneName = "MainMenu";

    [Tooltip("Oyuncu týklayarak introyu geçebilsin mi?")]
    public bool allowSkip = true;

    private float timer;

    void Update()
    {
        // Zamanlayýcýyý çalýþtýr
        timer += Time.deltaTime;

        // Süre dolduysa GEÇ
        if (timer >= waitTime)
        {
            LoadNextScene();
        }

        // Eðer týklama ile geçiþ açýksa ve oyuncu týkladýysa GEÇ
        if (allowSkip && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)))
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        // Sahne yüklenirken hata almamak için var mý diye kontrol edebiliriz ama
        // þimdilik basit tutalým. Doðrudan yükle:
        SceneManager.LoadScene(nextSceneName);
    }
}