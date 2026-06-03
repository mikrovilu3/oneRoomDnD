using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float MaxHealth = 100f;
    public float health;
    public GameObject healthbar;
    Slider healthSlider;

    void Start()
    {
        health = MaxHealth;

        if (healthbar != null)
        {
            healthSlider = healthbar.GetComponent<Slider>();
            healthSlider.maxValue = MaxHealth;
            healthSlider.value = health;
        }
        else
        {
            Debug.LogError("Health bar is not assigned in the Inspector!" + healthbar.GetComponent<Slider>() + "!!!" + healthbar.name + "" + healthSlider.name);
        }
    }

    public void UpdateVisualHealth()
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }

    void Death()
    {
        // Search the scene for the VRPauseMenu script
        VRPauseMenu menuSystem = FindAnyObjectByType<VRPauseMenu>();

        if (menuSystem != null)
        {
            // Trigger the death screen panel, center it on the player, and freeze time
            menuSystem.PlayerDied();
            Time.timeScale = 0.0f;
        }
        else
        {
            // Backup fallback just in case the menu script is missing from the scene
            Debug.LogError("[PlayerHealth] Looked for VRPauseMenu in the scene but couldn't find it! Freezing game anyway.");
            Time.timeScale = 0.0f;
        }
    }

    public void TakeDamage(float damage)
    {
        // Subtracts the incoming damage safely
        health -= damage;

        UpdateVisualHealth();
        if (health < 1)
        {
            Death();
        }
    }
}