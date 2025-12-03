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
        InitializeReferences();

        if (playerTransform == null) Debug.LogError("XpCollector: Üst obje (Player) bulunamadý!", this);

        UpdateMagnetRadius();
    }

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
    }

    public void UpdateMagnetRadius()
    {
        InitializeReferences();

        if (playerStats != null && magnetCollider != null)
        {
            magnetCollider.radius = playerStats.CurrentMagnetRange;
        }
    }

    // --- YENÝ EKLENEN KISIM (Magnet Item'ý Ýçin) ---
    /// <summary>
    /// Sahnedeki aktif olan TÜM XpOrb'larý bulur ve oyuncuya doðru çeker.
    /// Magnet item'ý tarafýndan tetiklenir.
    /// </summary>
    public void PullAllActiveOrbs()
    {
        if (playerTransform == null) return;

        // Sahnedeki tüm XpOrb scriptlerini bul (Aðýr bir iþlemdir ama 
        // 20 saniyede bir çalýþacaðý için sorun olmaz)
        XpOrb[] allOrbs = FindObjectsByType<XpOrb>(FindObjectsSortMode.None);

        if (allOrbs != null && allOrbs.Length > 0)
        {
            // Debug.Log($"Magnet Aktif: {allOrbs.Length} XP küresi çekiliyor!");
            foreach (XpOrb orb in allOrbs)
            {
                // Zaten çekilmekte olanlarý tekrar tetiklemeye gerek yok ama
                // StartSeeking içinde kontrolü varsa sorun olmaz.
                orb.StartSeeking(playerTransform);
            }
        }
    }
    // -----------------------------------------------
}