using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP = 100;
    public HealthBar healthBar;
    public int tokens = 0;

    public Image redScreen; // Reference to the UI red overlay
    public float fadeDuration = 2f; // Duration to fade to full red

    void Start()
    {
        currentHP = maxHP;
        healthBar.SetMaxHealth(maxHP);

        if (redScreen != null)
        {
            Color screenColor = redScreen.color;
            screenColor.a = 0f;
            redScreen.color = screenColor; // Set opacity to 0 initially
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(20);
        }
        if (currentHP <= 0)
        {
            StartCoroutine(FadeToRedAndGameOver());
        }
    }

    private void TakeDamage(int v)
    {
        currentHP -= v;
        healthBar.SetHealth(currentHP);

        if (currentHP <= 0)
        {
            StartCoroutine(FadeToRedAndGameOver());
        }
    }

    private IEnumerator FadeToRedAndGameOver()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration); // Gradually increase opacity
            redScreen.color = new Color(redScreen.color.r, redScreen.color.g, redScreen.color.b, alpha);
            yield return null;
        }

        SceneManager.LoadScene("GameOver");
        Debug.Log("I am DEAD");
    }
}
