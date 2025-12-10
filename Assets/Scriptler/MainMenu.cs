using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçiþleri için þart

public class MainMenuManager : MonoBehaviour
{
    // Oyna butonuna basýnca çalýþacak
    public void PlayGame()
    {
        // Buraya gitmek istediðin sahnenin tam adýný yazmalýsýn.
        // Karakter seçimi sahnenin adý neyse onu týrnak içine yaz.
        // Örn: "CharacterSelect" veya "GameScene"
        SceneManager.LoadScene("CharacterSelect");
    }

    // Çýkýþ butonuna basýnca çalýþacak
    public void QuitGame()
    {
        Debug.Log("Oyundan Çýkýldý!"); // Editörde çýkýþ çalýþmaz, bunu konsolda görmek için yazýyoruz.
        Application.Quit();
    }
}