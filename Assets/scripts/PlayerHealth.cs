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
        Time.timeScale = 0.0f;
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