using UnityEngine;

public class RapidExpansion : MonoBehaviour
{
    [Header("Büyüme Ayarlarý")]
    [Tooltip("Efektin ulaþacaðý son boyut (Scale).")]
    [SerializeField] private float targetScale = 6f;

    [Tooltip("Ne kadar hýzlý büyüsün? (Yüksek deðer = Daha ani patlama).")]
    [SerializeField] private float expansionSpeed = 15f;

    void Start()
    {
        // 1. Baþlangýçta boyutu sýfýr yap (Görünmez baþla)
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        // 2. Hedef boyuta doðru hýzlýca büyü (Lerp fonksiyonu yumuþak ama hýzlý bir geçiþ saðlar)
        // Vector3.one * targetScale demek -> (6, 6, 6) gibi bir vektör demektir.
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, Time.deltaTime * expansionSpeed);
    }
}