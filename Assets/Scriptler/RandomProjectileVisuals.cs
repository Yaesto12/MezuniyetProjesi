using UnityEngine;

public class RandomProjectileVisuals : MonoBehaviour
{
    [Header("--- Görsel Seçenekleri ---")]
    [Tooltip("Buraya fýrlatýlacak 3 farklý görseli (Sprite) sürükle.")]
    public Sprite[] possibleSprites;

    [Header("--- Renk Ayarlarý ---")]
    [Tooltip("Rastgele seçilmesi için buraya istediðin renkleri ekle.")]
    public Color[] possibleColors;

    [Tooltip("Eðer yukarýdaki listeyi boþ býrakýrsan, tamamen rastgele (Gökkuþaðý) renkler üretir.")]
    public bool generateRandomColorsIfEmpty = true;

    // Sprite Renderer Referansý
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // SpriteRenderer'ý bul (Bu objede yoksa çocuklarýnda ara)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: SpriteRenderer bulunamadý! Görsel deðiþtirilemiyor.");
            return;
        }

        ChangeVisuals();
    }

    void ChangeVisuals()
    {
        // 1. RASTGELE GÖRSEL (SPRITE) SEÇÝMÝ
        if (possibleSprites != null && possibleSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, possibleSprites.Length);
            spriteRenderer.sprite = possibleSprites[randomIndex];
        }

        // 2. RASTGELE RENK SEÇÝMÝ
        if (possibleColors != null && possibleColors.Length > 0)
        {
            // Listeden rastgele bir renk seç
            int randomColorIndex = Random.Range(0, possibleColors.Length);
            spriteRenderer.color = possibleColors[randomColorIndex];
        }
        else if (generateRandomColorsIfEmpty)
        {
            // Liste boþsa tamamen rastgele canlý bir renk üret
            spriteRenderer.color = new Color(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), 1f);
        }
    }
}