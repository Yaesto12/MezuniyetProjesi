using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Temel Nitelikler")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float speed = 3f;
    [SerializeField] private int damage = 10;

    [Header("Tecrübe Puaný (XP)")]
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int xpValue = 10;

    // --- GÜNCELLENEN KISIM: Altýn Ayarlarý ---
    [Header("Altýn Düþürme")]
    [Tooltip("Sadece görsel efekt olarak kullanýlacak altýn prefabý (Fiziksiz).")]
    [SerializeField] private GameObject goldCoinVisualPrefab;
    [Tooltip("Bu düþman öldüðünde en az kaç altýn versin?")]
    [SerializeField] private int minGoldDrop = 1;
    [Tooltip("Bu düþman öldüðünde en fazla kaç altýn versin?")]
    [SerializeField] private int maxGoldDrop = 3;
    // ---------------------------------------

    public float Speed => speed;
    public int Damage => damage;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damageAmount;
        // Gereksiz loglar temizlendi

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Debug.LogWarning(gameObject.name + " öldü!"); // Logu temiz tutmak için kapattýk

        // XP Düþürme (Mevcut kod - Yere düþer ve toplanmayý bekler)
        if (xpOrbPrefab != null)
        {
            GameObject orbGO = Instantiate(xpOrbPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            XpOrb orbScript = orbGO.GetComponent<XpOrb>();
            if (orbScript != null)
            {
                orbScript.Setup(xpValue);
            }
        }

        // --- YENÝ SÝSTEM: Otomatik Altýn Toplama ve Efekt ---

        // 1. Verilecek Altýn Miktarýný Belirle
        int goldAmount = Random.Range(minGoldDrop, maxGoldDrop + 1);

        // 2. Oyuncunun Cüzdanýný Bul ve Altýný Ekle
        // (Not: PlayerWallet script'i PlayerStats'taki bonusu kendi içinde hesaplayacak)
        PlayerWallet wallet = FindFirstObjectByType<PlayerWallet>();
        if (wallet != null)
        {
            wallet.AddGold(goldAmount);
        }

        // 3. Görsel Efekt Oluþtur (Sadece 1 tane fýrlatýyoruz, "ödül alýndý" hissi için)
        if (goldCoinVisualPrefab != null)
        {
            // Düþmanýn biraz yukarýsýnda oluþtur
            Vector3 spawnPos = transform.position + Vector3.up * 2f;
            Instantiate(goldCoinVisualPrefab, spawnPos, Quaternion.identity);
        }
        // ----------------------------------------------------

        Destroy(gameObject);
    }
}