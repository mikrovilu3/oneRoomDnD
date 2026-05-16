using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour , IDamageable
{
    public float MaxHealth = 100f;
    public float health;
    public GameObject healthbar;
    Slider healthSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        else
        {
            //  Debug.LogError("Health slider is null");
        }
    }
    
    void Death() {       
        Time.timeScale = 0.0f;
    }
    public void TakeDamage(float damage) {
        //Debug.Log("Before taking damage: " + health+" "+damage); // Check the health value before modification
        health = -damage;
        //Debug.Log("After taking damage: " + health+" "+damage);  // Check the health value after modification

        UpdateVisualHealth();
        if (health < 1) {
            Death();
        }    
    }
}
