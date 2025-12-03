using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    [Header("Temel Statlar")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 3.0f;

    [Header("Ödül / Loot")]
    [SerializeField] private int xpValue = 10;
    [SerializeField] private int goldValue = 5;
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private GameObject goldCoinPrefab;

    // --- HESAPLANMIÞ ÖZELLÝKLER (Properties) ---
    // Donmuþsa hýz 0, deðilse (Hýz * Ýðrenme Çarpaný)
    public float Speed => (isFrozen ? 0 : moveSpeed) * grossedOutSpeedMult;

    // (Hasar * Ýðrenme Çarpaný)
    public int Damage => Mathf.Max(1, Mathf.RoundToInt(damage * grossedOutDamageMult));

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    // -------------------------------------------

    // --- DURUM EFEKTLERÝ ---
    private bool isFrozen = false;
    private bool isBleeding = false;

    // --- GROSSED OUT (PUNGENT ONION) ---
    private bool isGrossedOut = false;
    private float grossedOutSpeedMult = 1f; // 1.0 = Normal, 0.5 = Yarý Hýz
    private float grossedOutDamageMult = 1f;
    private Coroutine grossedOutCoroutine;
    // -----------------------------------

    public bool IsGolden { get; private set; } = false;

    private float currentHealth;
    private Coroutine freezeCoroutine;
    private Coroutine bleedCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0) Die();
    }

    // --- FREEZE ---
    public void Freeze(float duration)
    {
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        freezeCoroutine = StartCoroutine(FreezeRoutine(duration));
    }
    private IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
    }

    // --- BLEED ---
    public void ApplyBleed(int damagePerTick, float duration)
    {
        if (!isBleeding)
        {
            bleedCoroutine = StartCoroutine(BleedRoutine(damagePerTick, duration));
        }
    }
    private IEnumerator BleedRoutine(int dmg, float time)
    {
        isBleeding = true;
        float timer = 0;
        while (timer < time)
        {
            TakeDamage(dmg);
            yield return new WaitForSeconds(1f);
            timer += 1f;
        }
        isBleeding = false;
    }

    // --- YENÝ EKLENEN: GROSSED OUT ---
    public void ApplyGrossedOut(float duration, float speedReductionPercent, float damageReductionPercent)
    {
        // Coroutine zaten çalýþýyorsa durdurup yeniden baþlat
        if (grossedOutCoroutine != null) StopCoroutine(grossedOutCoroutine);
        grossedOutCoroutine = StartCoroutine(GrossedOutRoutine(duration, speedReductionPercent, damageReductionPercent));
    }

    private IEnumerator GrossedOutRoutine(float duration, float speedRed, float damageRed)
    {
        isGrossedOut = true;

        // Örn: %30 azalma için (1 - 0.30) = 0.70 çarpaný
        grossedOutSpeedMult = Mathf.Clamp01(1f - (speedRed / 100f));
        grossedOutDamageMult = Mathf.Clamp01(1f - (damageRed / 100f));

        // Görsel efekt (Yeþilimsi renk)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.material.color = Color.green;

        yield return new WaitForSeconds(duration);

        // Eski haline dön
        isGrossedOut = false;
        grossedOutSpeedMult = 1f;
        grossedOutDamageMult = 1f;

        // Rengi düzelt (Eðer Altýn ise sarýya, deðilse beyaza)
        foreach (Renderer r in renderers)
        {
            r.material.color = IsGolden ? Color.yellow : Color.white;
        }
    }
    // ---------------------------------

    public float GetHealthPercentage() => currentHealth / maxHealth;

    public void MakeGolden()
    {
        if (IsGolden) return;
        IsGolden = true;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.color = Color.yellow;
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", Color.yellow * 2f);
        }
    }

    private void Die()
    {
        if (GameEventManager.Instance != null) GameEventManager.Instance.TriggerEnemyKilled(this);
        int finalXp = IsGolden ? xpValue * 10 : xpValue;
        int finalGold = IsGolden ? goldValue * 10 : goldValue;
        DropLoot(finalXp, finalGold);
        Destroy(gameObject);
    }

    private void DropLoot(int xpAmount, int goldAmount)
    {
        // 1. XP Küresi (Fiziksel olarak düþmeye devam etsin, onu XpCollector topluyor)
        if (xpOrbPrefab != null)
        {
            Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
        }

        // 2. Altýn (OTOMATÝK TOPLAMA)
        if (goldAmount > 0)
        {
            // Oyuncuyu bul ve parayý direkt ekle
            // (FindObject performanslý deðildir ama sadece ölünce çalýþtýðý için sorun olmaz)
            PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();

            if (playerInventory != null)
            {
                playerInventory.AddGold(goldAmount);
            }

            // 3. Görsel Efekt (Altýn Logosu)
            // Bu obje sadece görsellik içindir, içinde script olmasýna gerek yok.
            // Üzerinde "AutoDestroy" scripti olmasý yeterli.
            if (goldCoinPrefab != null)
            {
                // Biraz yukarýda çýksýn
                Instantiate(goldCoinPrefab, transform.position + Vector3.up * 1.0f, Quaternion.identity);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(Damage, this); // Özellikten gelen Damage'i kullanýr
        }
    }
}