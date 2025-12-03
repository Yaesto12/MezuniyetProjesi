using UnityEngine;
using System;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public event Action<EnemyStats> onEnemySpawned;

    public void TriggerEnemySpawned(EnemyStats enemy)
    {
        onEnemySpawned?.Invoke(enemy);
    }
    // --- OLAYLAR (Events) ---

    public event Action<EnemyStats, int, bool> onEnemyHit;
    public event Action<EnemyStats> onEnemyKilled;

    // DEÐÝÞTÝ: Artýk saldýraný da gönderiyor (int damage, EnemyStats attacker)
    public event Action<int, EnemyStats> onPlayerTakeDamage;

    public event Action<int> onPlayerLevelUp;
    public event Action onPlayerSkillUsed;

    // --- TETÝKLEYÝCÝLER ---

    public void TriggerEnemyHit(EnemyStats enemy, int damage, bool isCrit)
    {
        if (onEnemyHit != null) onEnemyHit(enemy, damage, isCrit);
    }

    public void TriggerEnemyKilled(EnemyStats enemy)
    {
        if (onEnemyKilled != null) onEnemyKilled(enemy);
    }

    // DEÐÝÞTÝ: Saldýran parametresi eklendi
    public void TriggerPlayerTakeDamage(int damage, EnemyStats attacker)
    {
        if (onPlayerTakeDamage != null) onPlayerTakeDamage(damage, attacker);
    }

    public void TriggerPlayerLevelUp(int newLevel)
    {
        if (onPlayerLevelUp != null) onPlayerLevelUp(newLevel);
    }

    public void TriggerPlayerSkillUsed()
    {
        if (onPlayerSkillUsed != null) onPlayerSkillUsed();
    }
}