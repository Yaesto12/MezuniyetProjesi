using UnityEngine;
using System;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance;

    void Awake()
    {
        // Singleton (Sahnede tek olmasýný saðlar)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- OLAYLAR (Events) ---

    // Düþmana vurulduðunda tetiklenir (Düþman Stats, Verilen Hasar, Kritik mi?)
    public event Action<EnemyStats, int, bool> onEnemyHit;

    // Düþman öldüðünde tetiklenir
    public event Action<EnemyStats> onEnemyKilled;

    // Oyuncu hasar aldýðýnda tetiklenir
    public event Action<int> onPlayerTakeDamage;

    // --- YENÝ EKLENEN: Seviye Atlandýðýnda Tetiklenir ---
    public event Action<int> onPlayerLevelUp; // Parametre olarak yeni seviyeyi gönderir
    // ---------------------------------------------------
    public event Action onPlayerSkillUsed;

    // --- TETÝKLEYÝCÝLER (Ana scriptler bunlarý çaðýrýr) ---

    public void TriggerEnemyHit(EnemyStats enemy, int damage, bool isCrit)
    {
        if (onEnemyHit != null) onEnemyHit(enemy, damage, isCrit);
    }

    public void TriggerEnemyKilled(EnemyStats enemy)
    {
        if (onEnemyKilled != null) onEnemyKilled(enemy);
    }

    public void TriggerPlayerSkillUsed()
    {
        if (onPlayerSkillUsed != null) onPlayerSkillUsed();
        // Debug.Log("EVENT: Yetenek Kullanýldý!");
    }

    public void TriggerPlayerTakeDamage(int damage)
    {
        if (onPlayerTakeDamage != null) onPlayerTakeDamage(damage);
    }

    // --- YENÝ TETÝKLEYÝCÝ ---
    public void TriggerPlayerLevelUp(int newLevel)
    {
        if (onPlayerLevelUp != null) onPlayerLevelUp(newLevel);
        Debug.Log($"EVENT: Seviye Atlama Olayý Tetiklendi! (Seviye {newLevel})");
    }
    // ------------------------
}