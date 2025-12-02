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

    // --- GÜNCELLENEN KISIM: Hýz ve Donma ---
    private bool isFrozen = false;
    private Coroutine freezeCoroutine;

    // Dýþarýdan hýz istendiðinde, donmuþsa 0, deðilse normal hýzý ver
    public float Speed => isFrozen ? 0f : speed;
    // ---------------------------------------

    public int Damage => damage;

    private int currentHealth;
    private Coroutine bleedCoroutine;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount, bool isDoT = false)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyBleed(int totalBleedDamage, float duration)
    {
        if (totalBleedDamage <= 0) return;
        if (bleedCoroutine != null) StopCoroutine(bleedCoroutine);
        bleedCoroutine = StartCoroutine(BleedRoutine(totalBleedDamage, duration));
    }

    private IEnumerator BleedRoutine(int totalDamage, float duration)
    {
        float damagePerSecond = totalDamage / duration;
        float tickRate = 0.5f;
        float timer = 0f;

        while (timer < duration && currentHealth > 0)
        {
            yield return new WaitForSeconds(tickRate);
            timer += tickRate;
            int tickDamage = Mathf.CeilToInt(damagePerSecond * tickRate);
            TakeDamage(tickDamage, true);
        }
        bleedCoroutine = null;
    }

    // --- YENÝ METOT: Dondurma ---
    public void Freeze(float duration)
    {
        // Eðer zaten donmuþsa, süreyi uzatabilir veya sýfýrlayabiliriz.
        // Basitlik için eskiyi durdurup yenisini baþlatýyoruz.
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        freezeCoroutine = StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;
        // Debug.Log($"{gameObject.name} dondu! ({duration} saniye)");

        // Opsiyonel: Görsel olarak donduðunu belli etmek için renk deðiþimi
        var renderer = GetComponentInChildren<Renderer>();
        Color originalColor = Color.white;
        if (renderer != null)
        {
            originalColor = renderer.material.color;
            renderer.material.color = Color.cyan; // Maviye boya
        }

        yield return new WaitForSeconds(duration);

        // Eski haline döndür
        isFrozen = false;
        if (renderer != null) renderer.material.color = originalColor;

        freezeCoroutine = null;
    }
    // ----------------------------

    private void Die()
    {
        if (xpOrbPrefab != null)
        {
            GameObject orbGO = Instantiate(xpOrbPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            XpOrb orbScript = orbGO.GetComponent<XpOrb>();
            if (orbScript != null) orbScript.Setup(xpValue);
        }

        int goldAmount = Random.Range(minGoldDrop, maxGoldDrop + 1);
        PlayerWallet wallet = FindFirstObjectByType<PlayerWallet>();
        if (wallet != null)
        {
            wallet.AddGold(goldAmount);
        }

        if (goldCoinVisualPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2f;
            Instantiate(goldCoinVisualPrefab, spawnPos, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public float GetHealthPercentage()
    {
        if (maxHealth <= 0) return 0;
        return (float)currentHealth / maxHealth;
    }
}