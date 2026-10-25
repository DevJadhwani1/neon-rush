using UnityEngine;
using NeonRush;

public class PlayerHealth : MonoBehaviour
{
    [Header("Assignment Required Health System")]
    public int health;
    public int maxHealth = 100;
    public int damageAmount = 25;

    void Start()
    {
        // Assignment Requirement: Initialize health in Start()
        health = 100;
        Debug.Log("PlayerHealth initialized in Start(): health = " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
        }
    }

    // Assignment Requirement: Collision Handling with OnCollisionEnter()
    void OnCollisionEnter(Collision collision)
    {
        // Safe check for damaging objects
        bool isHazard = false;
        try
        {
            isHazard = collision.gameObject.CompareTag("Hazard") || collision.gameObject.CompareTag("Enemy");
        }
        catch
        {
            isHazard = false;
        }

        if (!isHazard)
        {
            isHazard = collision.gameObject.name.Contains("Hazard") || collision.gameObject.name.Contains("Police") || collision.gameObject.name.Contains("Enemy");
        }

        if (isHazard)
        {
            TakeDamage(damageAmount);
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log("Collision damage taken! Current health: " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
            GameUIManager.Instance.TriggerHitFlash();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHazardHit();
        }

        // Assignment Requirement: Destroy on zero health
        if (health <= 0)
        {
            health = 0;
            Debug.Log("Health depleted to zero. Destroying GameObject as required by assignment.");

            if (GameManager.Instance != null)
            {
                if (gameObject.name.Contains("Player"))
                {
                    GameManager.Instance.OnPlayerDied();
                }
                else
                {
                    GameManager.Instance.OnVehicleDestroyed();
                }
            }

            Destroy(gameObject);
        }
    }
}
