using UnityEngine;
using System.Collections.Generic; // Required for List

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))] // Collider should be IsTrigger=true
public class WaveProjectile : MonoBehaviour
{
    // --- Variables set by Setup ---
    private int damage;
    private float critChance;
    private float critMultiplier; // Changed from Mult to Multiplier for clarity
    private float speed;
    private float width; // Used to potentially scale the collider/visual
    private float lifetime;
    private int pierceCount;
    // --- End of Setup Variables ---

    private Rigidbody rb;
    private List<EnemyStats> hitEnemies = new List<EnemyStats>(); // To track pierced enemies

    /// <summary>
    /// Called by WaveWeaponHandler to initialize the wave.
    /// MUST accept 7 arguments.
    /// </summary>
    public void Setup(int dmg, float chance, float critMulti, float spd, float w, float life, int pierce) // <<<--- CORRECT 7 ARGUMENTS ---<<<
    {
        // Assign values from parameters
        this.damage = dmg;
        this.critChance = chance;
        this.critMultiplier = critMulti; // Use the correct parameter name
        this.speed = spd;
        this.width = w;
        this.lifetime = life;
        this.pierceCount = pierce;

        // Get Rigidbody and configure
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        // Attempt to scale the collider based on width (assuming BoxCollider)
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            // Assuming the width corresponds to the local X-axis of the collider
            boxCollider.size = new Vector3(width, boxCollider.size.y, boxCollider.size.z);
            boxCollider.isTrigger = true; // Ensure it's a trigger
        }
        else
        {
            Debug.LogWarning("WaveProjectile: BoxCollider not found for scaling.", this);
            // Ensure the existing collider is a trigger if it's not a BoxCollider
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            else Debug.LogError("WaveProjectile: No Collider found!", this);
        }

        // Set initial velocity
        rb.linearVelocity = transform.forward * speed;
        // Schedule destruction
        Destroy(gameObject, lifetime);

        // Clear the list of hit enemies initially
        hitEnemies.Clear();
    }

    // Called when entering a trigger collider
    void OnTriggerEnter(Collider other)
    {
        // Check if it hit an enemy hitbox layer
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
        {
            // Get the EnemyStats component from the hit object or its parent
            EnemyStats enemy = other.GetComponentInParent<EnemyStats>();

            // Check if it's a valid enemy and hasn't been hit by *this specific wave* yet
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                // Calculate critical hit
                bool isCritical = Random.value * 100 < critChance;
                int finalDamage = this.damage;
                if (isCritical)
                {
                    // Apply crit multiplier
                    finalDamage = Mathf.RoundToInt(this.damage * this.critMultiplier);
                }

                // Apply damage to the enemy
                enemy.TakeDamage(finalDamage);
                // Add enemy to the list of hit enemies for this wave
                hitEnemies.Add(enemy);
                // Decrease pierce count
                pierceCount--;

                // If pierce count is depleted, destroy the wave
                if (pierceCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
        // Optional: Destroy wave if it hits environment
        // else if (other.gameObject.CompareTag("Environment"))
        // {
        //     Destroy(gameObject);
        // }
    }
}