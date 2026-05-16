using UnityEngine;

/// <summary>
/// Attach this to any enemy or object that should deal damage to the player.
/// Implements IDamageable so VRSword can hit it.
/// Finds the player by GameObject name and forwards damage to Player_Heath.
/// </summary>
public class DamageReceiver : MonoBehaviour, IDamageable
{
    [Header("Player Reference")]
    [Tooltip("Must exactly match the player GameObject's name in the Hierarchy.")]
    [SerializeField] private string playerObjectName = "Player";

    [Header("Optional: Direct Reference")]
    [Tooltip("Drag the Player here to skip the name search entirely.")]
    [SerializeField] private Player_Heath playerHealth;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool logDamage = true;

    private void Start()
    {
        // Only search by name if not directly assigned
        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.Find(playerObjectName);

            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<Player_Heath>();

                if (playerHealth == null)
                    Debug.LogError($"[DamageReceiver] Found '{playerObjectName}' but it has no Player_Heath component.");
            }
            else
            {
                Debug.LogError($"[DamageReceiver] Could not find a GameObject named '{playerObjectName}'. " +
                               "Check the name matches exactly (case-sensitive).");
            }
        }
    }

    /// <summary>
    /// Called by VRSword (or anything else implementing IDamageable hits).
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (playerHealth == null) return;

        if (logDamage)
            Debug.Log($"[DamageReceiver] '{gameObject.name}' hit — dealing {damage:F1} damage to player.");

        playerHealth.Take(damage);

        if (playerHealth.health <= 0f)
            HandleDeath();
    }

    private void HandleDeath()
    {
        if (logDamage)
            Debug.Log($"[DamageReceiver] Player is dead.");

        // Add your death logic here — e.g. reload scene, show game over screen, etc.
        // SceneManager.LoadScene("GameOver");

        if (destroyOnDeath)
            Destroy(gameObject, deathDelay);
    }
}