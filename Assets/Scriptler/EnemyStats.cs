using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    [Header("Temel Statlar")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 3.0f;

    // --- YENÝ EKLENDÝ: Bu düþman Elite mi? ---
    [Tooltip("Eðer bu bir Boss veya güçlü düþmansa bunu iþaretle.")]
    [SerializeField] private bool isElite = false;

    [Header("Ödül")]
    [SerializeField] private int xpValue = 10;
    [SerializeField] private int goldValue = 5;
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private GameObject goldCoinPrefab;


    [Header("Efektler")]
    [SerializeField] private GameObject damagePopupPrefab;

    // --- HESAPLANMIÞ ÖZELLÝKLER ---
    public float Speed => (isFrozen ? 0 : moveSpeed) * grossedOutSpeedMult;
    public int Damage => Mathf.Max(1, Mathf.RoundToInt(damage * grossedOutDamageMult));
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    // --- DURUM EFEKTLERÝ ---
    private bool isFrozen = false;
    private bool isBleeding = false;

    // Pungent Onion
    public bool isGrossedOut = false;
    private float grossedOutSpeedMult = 1f;
    private float grossedOutDamageMult = 1f;
    private Coroutine grossedOutCoroutine;

    // Midas
    public bool IsGolden { get; private set; } = false;

    private float currentHealth;
    private Coroutine freezeCoroutine;
    private Coroutine bleedCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount, bool isCritical = false)
    {
        currentHealth -= damageAmount;

        // --- HASAR YAZISI (POPUP) KISMI ---
        if (damagePopupPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 2f;
            GameObject popupObj = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);

            DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(damageAmount, isCritical);
            }
        }
        // ----------------------------------

        if (currentHealth <= 0) Die();
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;

    // --- EFEKTLER ---
    public void Freeze(float d) { if (freezeCoroutine != null) StopCoroutine(freezeCoroutine); freezeCoroutine = StartCoroutine(FreezeRoutine(d)); }
    private IEnumerator FreezeRoutine(float d) { isFrozen = true; yield return new WaitForSeconds(d); isFrozen = false; }

    public void ApplyBleed(int d, float t) { if (!isBleeding) StartCoroutine(BleedRoutine(d, t)); }
    private IEnumerator BleedRoutine(int d, float t) { isBleeding = true; float c = 0; while (c < t) { TakeDamage(d); yield return new WaitForSeconds(1); c++; } isBleeding = false; }

    public void ApplyGrossedOut(float d, float s, float dm) { if (grossedOutCoroutine != null) StopCoroutine(grossedOutCoroutine); grossedOutCoroutine = StartCoroutine(GrossedRoutine(d, s, dm)); }
    private IEnumerator GrossedRoutine(float duration, float sRed, float dRed)
    {
        isGrossedOut = true;
        grossedOutSpeedMult = Mathf.Clamp01(1f - (sRed / 100f));
        grossedOutDamageMult = Mathf.Clamp01(1f - (dRed / 100f));
        Renderer[] r = GetComponentsInChildren<Renderer>(); foreach (Renderer ren in r) ren.material.color = Color.green;
        yield return new WaitForSeconds(duration);
        isGrossedOut = false; grossedOutSpeedMult = 1f; grossedOutDamageMult = 1f;
        foreach (Renderer ren in r) ren.material.color = IsGolden ? Color.yellow : Color.white;
    }

    public void MakeGolden()
    {
        if (IsGolden) return; IsGolden = true;
        Renderer[] r = GetComponentsInChildren<Renderer>(); foreach (Renderer ren in r) { ren.material.color = Color.yellow; ren.material.EnableKeyword("_EMISSION"); ren.material.SetColor("_EmissionColor", Color.yellow * 2f); }
    }

    private void Die()
    {
        // 1. KORUMA: GameEventManager yoksa hata verme, devam et
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.TriggerEnemyKilled(this);
        }

        // ============================================================
        // --- KILL SAYACI ÝÇÝN EKLENEN KISIM ---
        // ============================================================
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                // Düþmanýn Elite olup olmadýðýný buradan gönderiyoruz
                inventory.RegisterKill(isElite);
            }
        }
        else
        {
            // Eðer Player tag'i unutulduysa konsolda uyarý verelim
            Debug.LogWarning("EnemyStats: Player bulunamadý! 'Player' Tag'ini kontrol et.");
        }
        // ============================================================

        int finalXp = IsGolden ? xpValue * 10 : xpValue;
        int finalGold = IsGolden ? goldValue * 10 : goldValue;

        // 2. Loot düþürmeye geç
        DropLoot(finalXp, finalGold);

        Destroy(gameObject);
    }

    private void DropLoot(int xpAmount, int goldAmount)
    {
        // A) XP ORB
        if (xpOrbPrefab != null)
        {
            Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
        }

        // B) ALTIN
        if (goldAmount > 0)
        {
            // PlayerStats'ý bulmak için Player Inventory'de bulduðumuz referansý da kullanabilirdik 
            // ama garanti olsun diye tekrar aratýyoruz.
            PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();

            if (playerStats != null)
            {
                playerStats.AddGold(goldAmount);
            }

            // Görsel efekt
            if (goldCoinPrefab != null)
                Instantiate(goldCoinPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        }
    }
}