using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    [Header("Temel Nitelikler")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float speed = 3f;
    [SerializeField] private int damage = 10;

    [Header("Tecrübe Puaný (XP)")]
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int xpValue = 10;

    [Header("Altýn Düþürme")]
    [Tooltip("Sadece görsel efekt olarak kullanýlacak altýn prefabý (Fiziksiz).")]
    [SerializeField] private GameObject goldCoinVisualPrefab;
    [Tooltip("Bu düþman öldüðünde en az kaç altýn versin?")]
    [SerializeField] private int minGoldDrop = 1;
    [Tooltip("Bu düþman öldüðünde en fazla kaç altýn versin?")]
    [SerializeField] private int maxGoldDrop = 3;

    public float Speed => speed;
    public int Damage => damage;

    private int currentHealth;
    private Coroutine bleedCoroutine; // Kanama takibi için

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Düþmana hasar verir.
    /// </summary>
    /// <param name="damageAmount">Hasar miktarý</param>
    /// <param name="isDoT">Bu hasar Kanama vb. zamanla hasar kaynaðýndan mý geliyor?</param>
    public void TakeDamage(int damageAmount, bool isDoT = false)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;

        // Opsiyonel: isDoT true ise farklý renkte hasar yazýsý çýkarýlabilir.
        // if (isDoT) Debug.Log($"{gameObject.name} kanama hasarý: {damageAmount}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Düþmana kanama (Bleed) uygular.
    /// </summary>
    /// <param name="totalBleedDamage">Toplam verilecek kanama hasarý.</param>
    /// <param name="duration">Bu hasarýn kaç saniyeye yayýlacaðý.</param>
    public void ApplyBleed(int totalBleedDamage, float duration)
    {
        if (totalBleedDamage <= 0) return;

        // Eðer zaten kanýyorsa, basitlik için eskiyi durdurup yenisini baþlatýyoruz.
        // (Ýstenirse hasar biriktirme mantýðý da eklenebilir)
        if (bleedCoroutine != null) StopCoroutine(bleedCoroutine);

        bleedCoroutine = StartCoroutine(BleedRoutine(totalBleedDamage, duration));
    }

    private IEnumerator BleedRoutine(int totalDamage, float duration)
    {
        float damagePerSecond = totalDamage / duration;
        float tickRate = 0.5f; // Yarým saniyede bir hasar ver
        float timer = 0f;

        // Hasarýn tamamý verilene kadar veya süre bitene kadar
        while (timer < duration && currentHealth > 0)
        {
            yield return new WaitForSeconds(tickRate);
            timer += tickRate;

            // Tick baþýna düþen hasarý hesapla (Yukarý yuvarla ki en az 1 vursun)
            int tickDamage = Mathf.CeilToInt(damagePerSecond * tickRate);

            // DoT (Damage over Time) olarak hasar ver
            TakeDamage(tickDamage, true);
        }
        bleedCoroutine = null;
    }

    private void Die()
    {
        // Debug.LogWarning(gameObject.name + " öldü!");

        // 1. XP Orb Düþür
        if (xpOrbPrefab != null)
        {
            GameObject orbGO = Instantiate(xpOrbPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            XpOrb orbScript = orbGO.GetComponent<XpOrb>();
            if (orbScript != null) orbScript.Setup(xpValue);
        }

        // 2. Altýn Ver (Otomatik Cüzdana)
        int goldAmount = Random.Range(minGoldDrop, maxGoldDrop + 1);
        PlayerWallet wallet = FindFirstObjectByType<PlayerWallet>(); // PlayerWallet'ý bul
        if (wallet != null)
        {
            wallet.AddGold(goldAmount);
        }

        // 3. Altýn Görsel Efekti (Kafasýndan fýrlasýn)
        if (goldCoinVisualPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2f;
            Instantiate(goldCoinVisualPrefab, spawnPos, Quaternion.identity);
        }

        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.TriggerEnemyKilled(this);
        }


        Destroy(gameObject);
    }
}