using UnityEngine;
using NeonRush;

public class PlayerHealth : MonoBehaviour
{
    [Header("Assignment Required Health System")]
    public int health;
    public int maxHealth = 100;
    public int damageAmount = 25;

    [Header("Invulnerability & Damage Cooldown")]
    public float invulnerabilityDuration = 1.0f;
    private float invulnerabilityTimer = 0f;
    private bool isDead = false;

    void Start()
    {
        // Assignment Requirement: Initialize health in Start()
        health = 100;
        isDead = false;
        invulnerabilityTimer = 0f;
        Debug.Log("PlayerHealth initialized in Start(): health = " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
        }
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    // Assignment Requirement: Collision Handling with OnCollisionEnter()
    void OnCollisionEnter(Collision collision)
    {
        if (isDead || invulnerabilityTimer > 0f) return;

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

    public void RestoreHealth(int amount)
    {
        if (isDead || health <= 0) return;
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        Debug.Log("Player health restored! Current health: " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || invulnerabilityTimer > 0f) return;
        invulnerabilityTimer = invulnerabilityDuration;

        health = Mathf.Clamp(health - amount, 0, maxHealth);
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
            isDead = true;
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

            Destroy(gameObject, 0.05f);
        }
    }
}
