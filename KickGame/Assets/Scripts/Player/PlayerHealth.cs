using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthBar;

    [SerializeField] public GameOverMenu gameOverMenu;

    [SerializeField] private int regenerationAmount = 5; // health to be regenerated
    [SerializeField] private float regenerationDelay = 3f;// seconds before player regens
    [SerializeField] private float regenerationInterval = 1f; // interval during the regen

    private Coroutine regenCoroutine;

    private string currentSceneName;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        if (currentHealth < maxHealth && regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(RegenerateHealth());
        }
        if(currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        healthBar.SetHealth(currentHealth);

        // Stop ongoing regeneration when taking damage
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    private IEnumerator RegenerateHealth()
    {
        // Wait for delay before starting regeneration
        yield return new WaitForSeconds(regenerationDelay);

        while (currentHealth < maxHealth)
        {
            currentHealth += regenerationAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth); // health does not go past our maxHealth
            healthBar.SetHealth(currentHealth);

            yield return new WaitForSeconds(regenerationInterval);
        }

        // Stop once fully healed
        regenCoroutine = null;
    }

    void OnEnable()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
    }

    public void GameOver()
    {
        gameOverMenu.Setup();
    }

}
