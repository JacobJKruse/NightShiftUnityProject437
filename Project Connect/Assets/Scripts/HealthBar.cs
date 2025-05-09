using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Slider healthBar;
    public Gradient gradient;
    public Image fill;


    public void SetMaxHealth(int maxHealth) { 
    
    healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        healthBar.value = health;

        fill.color = gradient.Evaluate(healthBar.normalizedValue);
    }
    
  
}
