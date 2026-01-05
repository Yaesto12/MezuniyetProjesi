using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem; // Input System kütüphanesi

public class UIDebugger : MonoBehaviour
{
    void Update()
    {
        // 1. Mouse'un pozisyonunu al (Input System)
        if (Mouse.current == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 2. Mouse'a týklandýðýnda (Sol Týk) testi baþlat
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log($"<b>[TEST BAÞLADI]</b> Mouse Pozisyonu: {mousePos} | Cursor Görünür mü: {Cursor.visible} | Kilit Modu: {Cursor.lockState}");

            // UI Raycast Testi Yap
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = mousePos;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                Debug.Log($"<color=green>MOUSE ÞU AN BUNUN ÜZERÝNDE:</color> <b>{results[0].gameObject.name}</b>");
                foreach (var result in results)
                {
                    Debug.Log($"   -> Altýndaki diðer obje: {result.gameObject.name}");
                }
            }
            else
            {
                Debug.Log("<color=red>MOUSE HÝÇBÝR UI OBJESÝNÝ GÖRMÜYOR!</color> (Canvas ayarý veya EventSystem bozuk)");
            }
        }
    }
}