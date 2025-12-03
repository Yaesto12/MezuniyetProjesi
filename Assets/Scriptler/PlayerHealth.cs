using UnityEngine;
using System; // Action eventleri için gerekli

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour
{
    private PlayerStats playerStats;

    private float currentHealth;
    private float currentShield;

    // --- MEKANÝK DEÐÝÞKENLERÝ ---

    // 1. Caný Dýþarýdan Okuma (Scarred Heart vb. için)
    public float CurrentHealth => currentHealth;

    // 2. Overheal (Overflowing Goblet vb. için)
    public float CurrentOverheal { get; private set; }

    // 3. Kalkan / Hasar Yok Sayma (Soul Shield vb. için)
    // Bu deðiþken true ise alýnan hasar tamamen engellenir ve false yapýlýr.
    public bool IsShielded { get; set; } = false;

    // 4. Can Deðiþim Olayý (UI ve Itemlerin dinlemesi için)
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
            OnHealthChanged?.Invoke(); // Baþlangýç durumunu bildir
        }
    }

    void Update()
    {
        HandleHpRegen();
    }

    private void HandleHpRegen()
    {
        if (!isInitialized || playerStats == null || playerStats.CurrentHpRegen <= 0) return;

        // Eðer Can ve Overheal doluysa regen yapma (Boþuna iþlem yapmasýn)
        if (currentHealth >= playerStats.CurrentMaxHealth && CurrentOverheal >= playerStats.CurrentMaxOverheal) return;

        regenTimer += Time.deltaTime;
        if (regenTimer >= 1f)
        {
            Heal(playerStats.CurrentHpRegen);
            regenTimer = 0f;
        }
    }

    /// <summary>
    /// Ýyileþtirme fonksiyonu. Can dolarsa fazlalýk Overheal'e akar.
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

    /// <summary>
    /// Hasar alma fonksiyonu.
    /// </summary>
    public void TakeDamage(int damageAmount, EnemyStats attacker = null)
    {
        if (!isInitialized || damageAmount <= 0) return;

        // --- 1. SOUL SHIELD KONTROLÜ ---
        if (IsShielded)
        {
            IsShielded = false; // Kalkaný kýr
            // Debug.Log("PlayerHealth: Soul Shield hasarý engelledi!");
            OnHealthChanged?.Invoke(); // UI güncellemesi için tetikleyelim
            return; // Hasarý tamamen yok say ve çýk
        }
        // -------------------------------

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

        // 2. Sonra Kalkan'dan (Shield Stat) Düþ
        if (finalDamage > 0 && currentShield > 0)
        {
            float dmgToShield = Mathf.Min(currentShield, finalDamage);
            currentShield -= dmgToShield;
            finalDamage -= Mathf.RoundToInt(dmgToShield);
        }

        // 3. En Son Can'dan Düþ
        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;

            // Hasar alýndý eventi (Metronom vb. için)
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
                InitializeHealth(); // Canlanma
                // Belki küçük bir ölümsüzlük süresi veya efekt eklenebilir
            }
            else
            {
                Die();
            }
        }

        UpdateHealthUI();
        OnHealthChanged?.Invoke(); // Deðiþikliði bildir (Scarred Heart vb. için önemli)
    }

    /// <summary>
    /// Statlar deðiþtiðinde (örn: Max Can arttýðýnda) deðerleri günceller.
    /// </summary>
    public void UpdateMaxValues(float newMaxHealth, float newMaxShield)
    {
        if (!isInitialized) return;

        // Mevcut can, yeni max caný geçmesin
        currentHealth = Mathf.Min(currentHealth, newMaxHealth);

        // Kalkan mantýðý (oyun tasarýmýna göre deðiþebilir, genelde max'a çekilmez ama burada limitliyoruz)
        currentShield = Mathf.Min(currentShield, newMaxShield);

        // Overheal limit kontrolü
        if (playerStats != null)
            CurrentOverheal = Mathf.Min(CurrentOverheal, playerStats.CurrentMaxOverheal);

        UpdateHealthUI();
        OnHealthChanged?.Invoke();
    }

    /// <summary>
    /// Blood Payment gibi itemlerin "Caným buna yeter mi?" diye sormasý için.
    /// </summary>
    public bool CanAffordHealthCost(float cost)
    {
        // Bedeli ödeyince can 0'ýn üstünde kalmalý
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
        Debug.LogWarning("OYUNCU ÖLDÜ!");
        // Oyun bitiþ ekraný veya mantýðý buraya
        gameObject.SetActive(false);
    }

    private void UpdateHealthUI()
    {
        // Eðer bir UIManager varsa burada slider'larý güncelleyebilirsin.
        // Örn: UIManager.Instance.UpdateHealthBar(currentHealth, playerStats.CurrentMaxHealth, CurrentOverheal);
    }
}