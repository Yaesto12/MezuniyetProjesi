using UnityEngine;
using System; // Action eventleri için gerekli

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour
{
    private PlayerStats playerStats;

    private float currentHealth;
    private float currentShield;

    // --- MEKANÝK DEÐÝÞKENLERÝ ---

    // 1. Caný Dýþarýdan Okuma
    public float CurrentHealth => currentHealth;

    // 2. Overheal
    public float CurrentOverheal { get; private set; }

    // 3. Kalkan / Hasar Yok Sayma
    public bool IsShielded { get; set; } = false;

    // 4. Can Deðiþim Olayý
    public event Action OnHealthChanged;

    // ----------------------------

    private bool isInitialized = false;
    private float regenTimer = 0f;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerHealth: PlayerStats bulunamadý!", this);
            enabled = false;
            return;
        }
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
            CurrentOverheal = 0f;

            isInitialized = true;
            UpdateHealthUI();
            OnHealthChanged?.Invoke();
        }
    }

    void Update()
    {
        HandleHpRegen();
    }

    private void HandleHpRegen()
    {
        if (!isInitialized || playerStats == null || playerStats.CurrentHpRegen <= 0) return;

        if (currentHealth >= playerStats.CurrentMaxHealth && CurrentOverheal >= playerStats.CurrentMaxOverheal) return;

        regenTimer += Time.deltaTime;
        if (regenTimer >= 1f)
        {
            Heal(playerStats.CurrentHpRegen);
            regenTimer = 0f;
        }
    }

    public void Heal(float amount)
    {
        if (!isInitialized || amount <= 0) return;

        // 1. Önce Ana Caný Doldur
        float missingHealth = playerStats.CurrentMaxHealth - currentHealth;
        float healToHealth = Mathf.Min(amount, missingHealth);

        currentHealth += healToHealth;
        amount -= healToHealth;

        // 2. Artan Miktar Varsa Overheal'e Ekle
        if (amount > 0 && playerStats.CurrentMaxOverheal > 0)
        {
            float missingOverheal = playerStats.CurrentMaxOverheal - CurrentOverheal;
            float healToOverheal = Mathf.Min(amount, missingOverheal);
            CurrentOverheal += healToOverheal;
        }

        UpdateHealthUI();
        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(int damageAmount, EnemyStats attacker = null)
    {
        if (!isInitialized || damageAmount <= 0) return;

        // --- 1. SOUL SHIELD KONTROLÜ ---
        if (IsShielded)
        {
            IsShielded = false;
            OnHealthChanged?.Invoke();
            return;
        }

        // Evasion (Kaçýnma)
        if (playerStats.CurrentEvasion > 0 && UnityEngine.Random.value * 100 < playerStats.CurrentEvasion) return;

        // Armor (Zýrh) Hesabý
        float armorReduction = Mathf.Clamp01(playerStats.CurrentArmor / 100f);
        float damageAfterArmor = damageAmount * (1f - armorReduction);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damageAfterArmor));

        // --- HASAR DAÐILIMI ---

        // 1. Önce Overheal'den Düþ
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

        // 3. En Son Can'dan Düþ (GERÇEK HASAR BURADA)
        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;

            // --- YENÝ EKLENEN KISIM: EKRAN KIZARMASI ---
            // Sadece gerçek can azaldýðýnda ekran kýzarýr.
            if (DamageFlash.Instance != null)
            {
                DamageFlash.Instance.TriggerFlash();
            }
            // -------------------------------------------

            if (GameEventManager.Instance != null)
                GameEventManager.Instance.TriggerPlayerTakeDamage(finalDamage, attacker);
        }

        // Thorns (Dikenler)
        if (attacker != null && playerStats.CurrentThorns > 0)
        {
            attacker.TakeDamage(Mathf.RoundToInt(playerStats.CurrentThorns));
        }

        // Ölüm Kontrolü
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (playerStats.UseRevival())
            {
                InitializeHealth();
            }
            else
            {
                Die();
            }
        }

        UpdateHealthUI();
        OnHealthChanged?.Invoke();
    }

    public void UpdateMaxValues(float newMaxHealth, float newMaxShield)
    {
        if (!isInitialized) return;

        currentHealth = Mathf.Min(currentHealth, newMaxHealth);
        currentShield = Mathf.Min(currentShield, newMaxShield);

        if (playerStats != null)
            CurrentOverheal = Mathf.Min(CurrentOverheal, playerStats.CurrentMaxOverheal);

        UpdateHealthUI();
        OnHealthChanged?.Invoke();
    }

    public bool CanAffordHealthCost(float cost)
    {
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
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("GameOverManager sahnede bulunamadý!");
        }

        // Karakteri kapatmadan önce scriptleri devre dýþý býrakmak daha güvenlidir, 
        // ama direkt kapatmak da çalýþýr.
        gameObject.SetActive(false);
    }

    private void UpdateHealthUI()
    {
        if (UIManager.Instance != null)
        {
            // UIManager'daki fonksiyon adýn neyse onu kullanmalýsýn.
            // Örneðin: UpdateHealth(float current, float max)
            // UIManager.Instance.UpdateHealth(currentHealth, playerStats.CurrentMaxHealth);
        }
    }
}