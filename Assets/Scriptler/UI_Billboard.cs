using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Sahnedeki ana kamerayý bul
        mainCamera = Camera.main;
    }

    // Kamera hareketinden sonra çalýþmasý için LateUpdate kullanýlýr (Titremeyi önler)
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // UI, kameranýn baktýðý yöne baksýn (Ters dönmemesi için en temiz yöntem)
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}