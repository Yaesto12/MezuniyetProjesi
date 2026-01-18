using UnityEngine;
using System; // Action eventleri için gerekli

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour
{
    private PlayerStats playerStats;

    private float currentHealth;
    private float currentShield;

    // --- MEKANÝK DEÐÝÞKENLERÝ ---
    public float CurrentHealth => currentHealth;
    public float CurrentOverheal { get; private set; }
    public bool IsShielded { get; set; } = false;
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
        // --- YENÝ: Karakter Ýkonunu UI'ya Gönder ---
        // GameData senin karakter seçimini tutan statik sýnýfýn olmalý
        if (GameData.SelectedCharacterDataForGame != null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetupPlayerHUD(GameData.SelectedCharacterDataForGame);
            }
        }
        // ------------------------------------------

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

        float missingHealth = playerStats.CurrentMaxHealth - currentHealth;
        float healToHealth = Mathf.Min(amount, missingHealth);

        currentHealth += healToHealth;
        amount -= healToHealth;

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

        if (IsShielded)
        {
            IsShielded = false;
            OnHealthChanged?.Invoke();
            return;
        }

        if (playerStats.CurrentEvasion > 0 && UnityEngine.Random.value * 100 < playerStats.CurrentEvasion) return;

        float armorReduction = Mathf.Clamp01(playerStats.CurrentArmor / 100f);
        float damageAfterArmor = damageAmount * (1f - armorReduction);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damageAfterArmor));

        if (CurrentOverheal > 0)
        {
            float dmgToOverheal = Mathf.Min(CurrentOverheal, finalDamage);
            CurrentOverheal -= dmgToOverheal;
            finalDamage -= Mathf.RoundToInt(dmgToOverheal);
        }

        if (finalDamage > 0 && currentShield > 0)
        {
            float dmgToShield = Mathf.Min(currentShield, finalDamage);
            currentShield -= dmgToShield;
            finalDamage -= Mathf.RoundToInt(dmgToShield);
        }

        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;

            if (DamageFlash.Instance != null)
            {
                DamageFlash.Instance.TriggerFlash();
            }

            if (GameEventManager.Instance != null)
                GameEventManager.Instance.TriggerPlayerTakeDamage(finalDamage, attacker);
        }

        if (attacker != null && playerStats.CurrentThorns > 0)
        {
            attacker.TakeDamage(Mathf.RoundToInt(playerStats.CurrentThorns));
        }

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
        if (GameOverManager.Instance != null) GameOverManager.Instance.ShowDeathScreen();
        else Debug.LogError("GameOverManager sahnede bulunamadý!");
        gameObject.SetActive(false);
    }

    private void UpdateHealthUI()
    {
        // --- DÜZELTÝLEN KISIM: UI MANAGER BAÐLANTISI ---
        if (UIManager.Instance != null && playerStats != null)
        {
            // Yeni oluþturduðumuz fonksiyonu çaðýrýyoruz
            UIManager.Instance.UpdateHealthBar(currentHealth, playerStats.CurrentMaxHealth);
        }
        // ----------------------------------------------
    }
}