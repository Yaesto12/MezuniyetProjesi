using UnityEngine;

// Tüm özel item scriptleri bundan türeyecek
public abstract class ItemEffect : MonoBehaviour
{
    protected PlayerStats playerStats;
    protected MonoBehaviour owner; // Player objesi

    // Item ilk alýndýðýnda çalýþýr (Setup)
    public virtual void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        playerStats = stats;
        owner = playerOwner;
    }

    // Item silinirse (temizlik)
    public virtual void OnUnequip() { }
}