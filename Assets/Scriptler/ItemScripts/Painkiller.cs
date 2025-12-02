using UnityEngine;
using System.Collections;

public class Item_Painkiller : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;

    [Header("Bekleme Süresi")]
    [SerializeField] private float cooldown = 10f;
    private float lastTriggerTime = -999f;

    [Header("Etkiler")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float baseDamageBoost = 10f;
    [SerializeField] private float damageBoostPerStack = 10f;
    [SerializeField] private float baseRegenTotal = 10f;
    [SerializeField] private float regenPerStack = 5f;

    private Coroutine activeRoutine;
    private float currentAppliedDamage = 0f;

    public override void OnEquip(PlayerStats stats, MonoBehaviour playerOwner)
    {
        base.OnEquip(stats, playerOwner);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerTakeDamage += OnTakeDamage;
        }
    }

    public override void OnUnequip()
    {
        if (currentAppliedDamage > 0 && playerStats != null) playerStats.RemoveTemporaryDamage(currentAppliedDamage);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerTakeDamage -= OnTakeDamage;
        }
        base.OnUnequip();
    }

    // DEÐÝÞTÝ: Parametreye 'EnemyStats attacker' eklendi
    private void OnTakeDamage(int damageAmount, EnemyStats attacker)
    {
        if (Time.time < lastTriggerTime + cooldown) return;

        lastTriggerTime = Time.time;

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            if (playerStats != null) playerStats.RemoveTemporaryDamage(currentAppliedDamage);
            currentAppliedDamage = 0;
        }
        activeRoutine = StartCoroutine(PainkillerRoutine());
    }

    private IEnumerator PainkillerRoutine()
    {
        int stack = 1;
        if (owner != null && myItemData != null)
        {
            PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
            if (inventory != null) stack = inventory.GetItemLevel(myItemData);
        }

        currentAppliedDamage = baseDamageBoost + (damageBoostPerStack * (stack - 1));
        float totalRegen = baseRegenTotal + (regenPerStack * (stack - 1));
        float regenPerSecond = totalRegen / duration;

        if (playerStats != null) playerStats.AddTemporaryDamage(currentAppliedDamage);

        float timer = 0f;
        float tickRate = 0.5f;
        PlayerHealth health = owner.GetComponent<PlayerHealth>();

        while (timer < duration)
        {
            yield return new WaitForSeconds(tickRate);
            timer += tickRate;
            if (health != null) health.Heal(regenPerSecond * tickRate);
        }

        if (playerStats != null) playerStats.RemoveTemporaryDamage(currentAppliedDamage);
        currentAppliedDamage = 0f;
        activeRoutine = null;
    }
}