using UnityEngine;
using UnityEngine.UI;

public class Player_Heath : MonoBehaviour
{
    public float MaxHealth = 100f;
    public float health;
    public GameObject healthbar;
     Slider healthSlider;

    private void Awake()
    {
        
    }
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
            Debug.LogError("Health bar is not assigned in the Inspector!"+healthbar.GetComponent<Slider>()+"!!!"+healthbar.name+""+healthSlider.name);
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

    public void Take(float damage)
    {
        //Debug.Log("Before taking damage: " + health+" "+damage); // Check the health value before modification
        health =- damage;
        //Debug.Log("After taking damage: " + health+" "+damage);  // Check the health value after modification

        UpdateVisualHealth();
    }
}
