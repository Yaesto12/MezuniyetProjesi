using UnityEngine;

public class Item_Metronome : ItemEffect
{
    [Header("Ayarlar")]
    [SerializeField] private ItemData myItemData;
    [SerializeField] private int chargesPerSecond = 1;
    [SerializeField] private float attackSpeedPerCharge = 3f;
    [SerializeField] private int maxCharges = 50;

    private float timer = 0f;
    private int currentCharges = 0;

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
        if (playerStats != null) playerStats.SetTemporaryAttackSpeed(0);
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.onPlayerTakeDamage -= OnTakeDamage;
        }
        base.OnUnequip();
    }

    private void Update()
    {
        if (currentCharges < maxCharges)
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer = 0f;
                AddCharge();
            }
        }
    }

    private void AddCharge()
    {
        currentCharges += chargesPerSecond;
        if (currentCharges > maxCharges) currentCharges = maxCharges;
        UpdateStats();
    }

    // DEÐÝÞTÝ: Parametreye 'EnemyStats attacker' eklendi (Kullanmasak bile imza uymalý)
    private void OnTakeDamage(int damage, EnemyStats attacker)
    {
        if (currentCharges > 0)
        {
            currentCharges = 0;
            timer = 0f;
            UpdateStats();
            // Debug.Log("Metronome sýfýrlandý.");
        }
    }

    private void UpdateStats()
    {
        if (playerStats != null)
        {
            int itemLevel = 1;
            if (owner != null && myItemData != null)
            {
                PlayerInventory inventory = owner.GetComponent<PlayerInventory>();
                if (inventory != null) itemLevel = inventory.GetItemLevel(myItemData);
            }
            float totalBonus = currentCharges * (attackSpeedPerCharge * itemLevel);
            playerStats.SetTemporaryAttackSpeed(totalBonus);
        }
    }
}