using UnityEngine;

public class DamageReceiver : MonoBehaviour, IDamageable
{
    [Header("Player Reference")]
    [Tooltip("Must exactly match the player GameObject's name in the Hierarchy.")]
    [SerializeField] private string playerObjectName = "Player";

    [Header("Optional: Direct Reference")]
    [Tooltip("Drag the Player here to skip the name search entirely.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool logDamage = true;

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject playerObj = GameObject.Find(playerObjectName);

            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();

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

    public void TakeDamage(float damage)
    {
        if (playerHealth == null) return;

        if (logDamage)
            Debug.Log($"[DamageReceiver] '{gameObject.name}' hit — dealing {damage:F1} damage to player.");

        playerHealth.TakeDamage(damage);

        if (playerHealth.health <= 0f)
            HandleDeath();
    }

    private void HandleDeath()
    {
        if (logDamage)
            Debug.Log($"[DamageReceiver] Player is dead.");

        if (destroyOnDeath)
            Destroy(gameObject, deathDelay);
    }
}