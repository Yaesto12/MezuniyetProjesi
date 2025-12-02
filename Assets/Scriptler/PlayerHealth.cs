using UnityEngine;
using System; // Action için gerekli

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour
{
    private PlayerStats playerStats;

    private float currentHealth;
    private float currentShield;

    // --- YENÝ: Overheal Deðiþkeni ---
    public float CurrentOverheal { get; private set; }
    // Overheal deðiþtiðinde tetiklenecek olay (Item bunu dinleyecek)
    public event Action OnHealthChanged;
    // -------------------------------

    private bool isInitialized = false;
    private float regenTimer = 0f;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null) { Debug.LogError("PlayerHealth: PlayerStats bulunamadý!", this); enabled = false; return; }
    }

    void Start()
    {
        InitializeHealth();
    }

    void InitializeHealth()
    {
        if (playerStats != null && !isInitialized)
        {
            currentHealth = playerStats.CurrentMaxHealth;
            currentShield = playerStats.CurrentMaxShield;
            CurrentOverheal = 0f; // Baþlangýçta 0

            isInitialized = true;
            UpdateHealthUI();
            OnHealthChanged?.Invoke(); // Baþlangýçta bildir
        }
    }

    void Update()
    {
        HandleHpRegen();
    }

    private void HandleHpRegen()
    {
        if (!isInitialized || playerStats == null || playerStats.CurrentHpRegen <= 0) return;

        // Eðer Can ve Overheal doluysa regen yapma
        if (currentHealth >= playerStats.CurrentMaxHealth && CurrentOverheal >= playerStats.CurrentMaxOverheal) return;

        regenTimer += Time.deltaTime;
        if (regenTimer >= 1f)
        {
            Heal(playerStats.CurrentHpRegen);
            regenTimer = 0f;
        }
    }

    /// <summary>
    /// Ýyileþtirme fonksiyonu. Fazlalýk Overheal'e akar.
    /// </summary>
    public void Heal(float amount)
    {
        if (!isInitialized || amount <= 0) return;

        // 1. Önce Ana Caný Doldur
        float missingHealth = playerStats.CurrentMaxHealth - currentHealth;
        float healToHealth = Mathf.Min(amount, missingHealth);

        currentHealth += healToHealth;
        amount -= healToHealth; // Kalan iyileþtirme miktarý

        // 2. Artan Miktar Varsa Overheal'e Ekle
        if (amount > 0 && playerStats.CurrentMaxOverheal > 0)
        {
            float missingOverheal = playerStats.CurrentMaxOverheal - CurrentOverheal;
            float healToOverheal = Mathf.Min(amount, missingOverheal);
            CurrentOverheal += healToOverheal;
        }

        UpdateHealthUI();
        OnHealthChanged?.Invoke(); // Deðiþikliði bildir
    }

    public void TakeDamage(int damageAmount, EnemyStats attacker = null)
    {
        if (!isInitialized || damageAmount <= 0) return;

        // Evasion
        if (playerStats.CurrentEvasion > 0 && UnityEngine.Random.value * 100 < playerStats.CurrentEvasion) return;

        // Armor
        float armorReduction = Mathf.Clamp01(playerStats.CurrentArmor / 100f);
        float damageAfterArmor = damageAmount * (1f - armorReduction);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damageAfterArmor));

        // 1. Önce Overheal'den Düþ (YENÝ)
        if (CurrentOverheal > 0)
        {
            float dmgToOverheal = Mathf.Min(CurrentOverheal, finalDamage);
            CurrentOverheal -= dmgToOverheal;
            finalDamage -= Mathf.RoundToInt(dmgToOverheal);
        }

        // 2. Sonra Kalkan'dan Düþ
        if (finalDamage > 0 && currentShield > 0)
        {
            float dmgToShield = Mathf.Min(currentShield, finalDamage);
            currentShield -= dmgToShield;
            finalDamage -= Mathf.RoundToInt(dmgToShield);
        }

        // 3. Sonra Can'dan Düþ
        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;
            if (GameEventManager.Instance != null) GameEventManager.Instance.TriggerPlayerTakeDamage(finalDamage, attacker);
        }

        // Thorns
        if (attacker != null && playerStats.CurrentThorns > 0)
        {
            attacker.TakeDamage(Mathf.RoundToInt(playerStats.CurrentThorns));
        }

        // Ölüm
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (playerStats.UseRevival()) InitializeHealth();
            else Die();
        }

        UpdateHealthUI();
        OnHealthChanged?.Invoke(); // Deðiþikliði bildir
    }

    public void UpdateMaxValues(float newMaxHealth, float newMaxShield)
    {
        if (!isInitialized) return;
        currentHealth = Mathf.Min(currentHealth, newMaxHealth);
        currentShield = Mathf.Min(currentShield, newMaxShield);
        // Overheal limiti PlayerStats'ta tutuluyor, burada sadece clamp yapabiliriz
        if (playerStats != null) CurrentOverheal = Mathf.Min(CurrentOverheal, playerStats.CurrentMaxOverheal);

        UpdateHealthUI();
    }

    public float CurrentHealth => currentHealth;
    public bool CanAffordHealthCost(float cost)
    {
        // Overheal harcanabilir mi? Genelde hayýr, "Kan" bedeli ana candan gider.
        return currentHealth > cost;
    }

    public void ApplyLifeSteal(float damageDealt)
    {
        if (!isInitialized || playerStats == null || playerStats.CurrentLifeSteal <= 0 || damageDealt <= 0) return;
        float stealAmount = damageDealt * (playerStats.CurrentLifeSteal / 100f);
        if (stealAmount > 0) Heal(stealAmount);
    }

    private void Die()
    {
        Debug.LogWarning("Oyuncu Öldü!");
        gameObject.SetActive(false);
    }

    private void UpdateHealthUI()
    {
        // UIManager entegrasyonu
    }
}