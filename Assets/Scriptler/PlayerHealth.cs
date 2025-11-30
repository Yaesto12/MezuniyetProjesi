using UnityEngine;

[RequireComponent(typeof(PlayerStats))] // PlayerStats zorunlu
public class PlayerHealth : MonoBehaviour
{
    // --- Referanslar ---
    private PlayerStats playerStats;

    // --- Mevcut Durum ---
    private float currentHealth;
    private float currentShield;
    // private float currentOverheal; // Ýleride eklenebilir

    // --- Maksimum Deðerler (PlayerStats'tan anlýk alýnýr) ---
    // private float maxHealthFromStats; // Artýk doðrudan playerStats'tan okunabilir
    // private float maxShieldFromStats;

    private bool isInitialized = false;
    private float regenTimer = 0f; // Can yenileme zamanlayýcýsý

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
        // PlayerStats Awake'te hesaplama yapmýþ olmalý, güncel deðerleri al
        if (playerStats != null && !isInitialized)
        {
            currentHealth = playerStats.CurrentMaxHealth; // Baþlangýç canýný max yap
            currentShield = playerStats.CurrentMaxShield; // Baþlangýç kalkanýný max yap

            isInitialized = true;
            Debug.Log($"PlayerHealth Baþlatýldý. Can: {currentHealth:F0}/{playerStats.CurrentMaxHealth:F0}, Kalkan: {currentShield:F0}/{playerStats.CurrentMaxShield:F0}");

            UpdateHealthUI(); // Baþlangýç UI Güncellemesi
        }
    }

    void Update()
    {
        // Can Yenileme (HpRegen)
        HandleHpRegen();
    }

    /// <summary>
    /// Pasif can yenilemesini yönetir.
    /// </summary>
    private void HandleHpRegen()
    {
        if (!isInitialized || playerStats == null || playerStats.CurrentHpRegen <= 0 || currentHealth >= playerStats.CurrentMaxHealth)
        {
            // Yenileme yoksa, can full ise veya script hazýr deðilse çýk
            return;
        }

        regenTimer += Time.deltaTime;
        float regenInterval = 1f; // Her saniye yenileme yapalým (ayarlanabilir)

        if (regenTimer >= regenInterval)
        {
            float healAmount = playerStats.CurrentHpRegen * regenInterval; // Saniyelik regen * geçen süre
            Heal(healAmount); // Ýyileþtirme fonksiyonunu çaðýr
            regenTimer -= regenInterval; // Zamanlayýcýyý sýfýrla (kalan süreyi koru)
        }
    }

    /// <summary>
    /// Karakterin canýný/kalkanýný iyileþtirir (Overheal hariç).
    /// </summary>
    public void Heal(float amount)
    {
        if (!isInitialized || amount <= 0) return;

        // Önce caný doldur
        float neededHealth = playerStats.CurrentMaxHealth - currentHealth;
        float healToHealth = Mathf.Min(amount, neededHealth);
        currentHealth += healToHealth;
        amount -= healToHealth; // Kalan iyileþtirme miktarý

        // TODO: Sonra kalkaný doldur? (Oyun tasarýmýna baðlý)
        // float neededShield = playerStats.CurrentMaxShield - currentShield;
        // float healToShield = Mathf.Min(amount, neededShield);
        // currentShield += healToShield;
        // amount -= healToShield;

        // TODO: Sonra Overheal'i doldur?
        // currentOverheal = Mathf.Min(currentOverheal + amount, playerStats.CurrentMaxOverheal);

        Debug.Log($"Ýyileþtirme: +{healToHealth} Can. Yeni Can: {currentHealth:F0}/{playerStats.CurrentMaxHealth:F0}");
        UpdateHealthUI();
    }


    /// <summary>
    /// Karakterin hasar almasýný yönetir (Armor, Evasion, Shield, Thorns).
    /// </summary>
    public void TakeDamage(int damageAmount, EnemyStats attacker = null) // Saldýraný bilmek Thorns için gerekli
    {
        if (!isInitialized || damageAmount <= 0) return;

        // 1. Evasion Kontrolü
        if (playerStats.CurrentEvasion > 0 && Random.value * 100 < playerStats.CurrentEvasion)
        {
            Debug.Log("Kaçýnýldý!");
            // TODO: Kaçýnma efekti/sesi oynatýlabilir
            // Savrulma hala PlayerController'da tetiklenir (çarpýþma oldu çünkü)
            return; // Hasar almadan çýk
        }

        // 2. Armor Hesaplamasý
        // Örnek: Armor=50 ise Hasar %50 azalýr. Armor=100 ise Hasar %100 azalýr (0 olur).
        // Farklý formüller kullanýlabilir (örn: Hasar * (100 / (100 + Armor)))
        float armorReduction = Mathf.Clamp01(playerStats.CurrentArmor / 100f); // Basit % azaltma
        float damageAfterArmor = damageAmount * (1f - armorReduction);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damageAfterArmor)); // Hasar en az 1 olsun

        Debug.Log($"Hasar Alýnýyor: {damageAmount} -> Zýrh Sonrasý: {finalDamage}");

        // 3. Kalkan Hasarý
        float damageAbsorbedByShield = 0;
        if (currentShield > 0)
        {
            damageAbsorbedByShield = Mathf.Min(currentShield, finalDamage);
            currentShield -= damageAbsorbedByShield;
            finalDamage -= Mathf.RoundToInt(damageAbsorbedByShield); // Kalan hasar
            Debug.Log($"Kalkana {damageAbsorbedByShield:F0} hasar verildi. Kalan Kalkan: {currentShield:F0}");
        }

        // 4. Can Hasarý (ve Overheal?)
        if (finalDamage > 0)
        {
            // TODO: Overheal varsa önce ondan düþülebilir
            if (currentHealth > 0)
            {
                currentHealth -= finalDamage;
                Debug.Log($"Cana {finalDamage} hasar verildi. Kalan Can: {currentHealth:F0}");
            }
        }

        // 5. Dikenler (Thorns) - Eðer saldýran belliyse
        if (attacker != null && playerStats.CurrentThorns > 0)
        {
            int thornsDamage = Mathf.RoundToInt(playerStats.CurrentThorns);
            attacker.TakeDamage(thornsDamage); // Düþmana hasar ver
            Debug.Log($"Dikenler {attacker.name}'e {thornsDamage} hasar verdi.");
        }

        // 6. Can Sýfýr Kontrolü ve Canlandýrma
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (playerStats.UseRevival()) // Canlandýrma hakký kullanýldý mý?
            {
                InitializeHealth(); // Caný/Kalkaný full'le
                Debug.LogWarning("CANLANDIRMA KULLANILDI!");
                // TODO: Canlandýrma efekti/sesi
            }
            else
            {
                Die(); // Canlandýrma yoksa veya bittiyse öl
            }
        }

        // 7. UI Güncelle
        UpdateHealthUI();
    }

    /// <summary>
    /// Can Çalma uygular (Silah Handler'larý tarafýndan çaðrýlýr).
    /// </summary>
    public void ApplyLifeSteal(float damageDealt)
    {
        if (!isInitialized || playerStats == null || playerStats.CurrentLifeSteal <= 0 || damageDealt <= 0) return;

        float stealAmount = damageDealt * (playerStats.CurrentLifeSteal / 100f);
        if (stealAmount > 0)
        {
            Heal(stealAmount); // Ýyileþtirme fonksiyonunu çaðýr
                               // Debug.Log($"Can Çalma: {damageDealt} hasardan {stealAmount:F1} iyileþtirildi."); // Çok fazla log basabilir
        }
    }


    private void Die()
    {
        Debug.LogWarning("Oyuncu Öldü!");
        // TODO: Ölüm ekraný, yeniden baþlatma vb.
        gameObject.SetActive(false); // Basit ölüm
    }

    /// <summary>
    /// PlayerStats'tan Max Can veya Max Kalkan deðiþtiðinde çaðrýlýr.
    /// </summary>
    public void UpdateMaxValues(float newMaxHealth, float newMaxShield)
    {
        if (!isInitialized) return;

        // Max deðerleri saklamaya gerek yok, doðrudan PlayerStats'tan okuyabiliriz
        // maxHealthFromStats = newMaxHealth;
        // maxShieldFromStats = newMaxShield;

        // Mevcut can ve kalkaný yeni maksimumlarý geçmeyecek þekilde ayarla
        currentHealth = Mathf.Min(currentHealth, newMaxHealth);
        currentShield = Mathf.Min(currentShield, newMaxShield);

        Debug.Log($"Max Deðerler Güncellendi. Yeni Can: {currentHealth:F0}/{newMaxHealth:F0}, Yeni Kalkan: {currentShield:F0}/{newMaxShield:F0}");

        UpdateHealthUI(); // UI'ý yeni max deðerlerle güncelle
    }

    /// <summary>
    /// Can/Kalkan UI'ýný UIManager aracýlýðýyla günceller.
    /// </summary>
    private void UpdateHealthUI()
    {
        if (UIManager.Instance != null && isInitialized && playerStats != null)
        {
            // TODO: UIManager'a can/kalkan/overheal için güncelleme metotlarý ekle
            // UIManager.Instance.UpdateHealthBar(currentHealth, playerStats.CurrentMaxHealth);
            // UIManager.Instance.UpdateShieldBar(currentShield, playerStats.CurrentMaxShield);
            // Debug.Log("PlayerHealth: UI Güncelleme isteði gönderildi.");
        }
    }
}