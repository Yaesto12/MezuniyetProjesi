using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class XpCollector : MonoBehaviour
{
    private Transform playerTransform;
    private PlayerStats playerStats;
    private SphereCollider magnetCollider;

    void Awake()
    {
        playerTransform = transform.parent;
        // Referanslarý burada alýyoruz
        InitializeReferences();

        if (playerTransform == null) Debug.LogError("XpCollector: Üst obje (Player) bulunamadý!", this);

        // Baþlangýç boyutunu ayarla
        UpdateMagnetRadius();
    }

    /// <summary>
    /// Referanslarý bulup atayan yardýmcý metot.
    /// </summary>
    private void InitializeReferences()
    {
        if (playerStats == null) playerStats = GetComponentInParent<PlayerStats>();
        if (magnetCollider == null)
        {
            magnetCollider = GetComponent<SphereCollider>();
            if (magnetCollider != null) magnetCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out XpOrb orb))
        {
            if (playerTransform != null) orb.StartSeeking(playerTransform);
        }
        // Altýn toplama buraya dahil deðil (otomatik toplanýyor)
    }

    /// <summary>
    /// PlayerStats deðiþtiðinde çaðrýlýr ve mýknatýs collider'ýnýn boyutunu günceller.
    /// </summary>
    public void UpdateMagnetRadius()
    {
        // Eðer PlayerStats bunu Awake'ten önce çaðýrýrsa diye referanslarý kontrol et/al
        InitializeReferences();

        if (playerStats != null && magnetCollider != null)
        {
            magnetCollider.radius = playerStats.CurrentMagnetRange;
        }
        else
        {
            // Eðer hala null ise, sadece sessizce çýk. 
            // Çünkü Awake henüz çalýþmamýþ olabilir, hata vermeye gerek yok.
            // Awake çalýþtýðýnda zaten hatalarý kontrol edecek.
        }
    }
}