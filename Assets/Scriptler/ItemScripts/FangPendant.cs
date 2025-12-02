using UnityEngine;

public class Item_FangPendant : ItemEffect
{
    [Header("Ayarlar")]
    [Tooltip("Scriptin hangi iteme ait olduðunu bilmesi için ItemData'yý buraya sürükleyin.")]
    [SerializeField] private ItemData myItemData; // <<<--- EKLENDÝ ---<<<

    [Header("Hasar Ayarlarý")]
    [Tooltip("Her Elite öldürüldüðünde kazanýlan hasar yüzdesi (Örn: 1 = %1).")]
    [SerializeField] private float damagePerEliteKill = 1.0f;

    // Bu kopyanýn kendi özel sayacý
    private int localEliteKills = 0;
    private float currentAppliedBonus = 0f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);

        // Düþman ölme olayýna abone ol
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyKilled += OnEnemyKilled;
        }
    }

    public override void OnUnequip()
    {
        // Çýkarken bu kopyanýn verdiði bonusu geri al
        if (currentAppliedBonus > 0 && playerStats != null)
        {
            playerStats.RemoveTemporaryDamage(currentAppliedBonus);
        }

        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onEnemyKilled -= OnEnemyKilled;
        }
        base.OnUnequip();
    }

    private void OnEnemyKilled(EnemyStats enemy)
    {
        // Sadece ELITE düþmanlarda çalýþýr
        if (enemy.CompareTag("Elite"))
        {
            // Bu kopyanýn sayacýný artýr
            localEliteKills++;

            // 1. Eski bonusu sil
            if (playerStats != null) playerStats.RemoveTemporaryDamage(currentAppliedBonus);

            // 2. Yeni toplam bonusu hesapla (Sadece bu kolyenin leþlerine göre)
            currentAppliedBonus = localEliteKills * damagePerEliteKill;

            // 3. Yeni bonusu ekle
            if (playerStats != null)
            {
                playerStats.AddTemporaryDamage(currentAppliedBonus);
                // Debug.Log($"Fang Pendant ({this.name}): Elite öldü! Sayaç: {localEliteKills}, Bonus: +%{currentAppliedBonus}");
            }
        }
    }
}