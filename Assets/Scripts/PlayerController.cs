using UnityEngine;
using NeonRush;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float rotationSpeed = 14f;

    [Header("Health Settings (Assignment Required)")]
    public int health;
    public int maxHealth = 100;
    public int damageAmount = 5;

    // Component References
    private Rigidbody rb;
    private CapsuleCollider col;

    // Input values
    private float moveHorizontal;
    private float moveVertical;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        if (rb != null)
        {
            rb.mass = 1f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Attach PlayerVisuals for high-poly astronaut model with golden visor and jetpack
        if (GetComponent<PlayerVisuals>() == null)
        {
            gameObject.AddComponent<PlayerVisuals>();
        }
    }

    void Start()
    {
        // Assignment Requirement: Initialize player health in Start()
        health = 100;
        Debug.Log("Player health initialized in Start(): " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
        }
    }

    void Update()
    {
        // If game is over, freeze input completely
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive)
        {
            moveHorizontal = 0f;
            moveVertical = 0f;
            return;
        }

        GetPlayerInput();

        // Fall detection
        if (transform.position.y < -3.5f)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        transform.position = new Vector3(0f, 1.5f, -8f);
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowNotification("⚠️ FELL OFF TRACK — RESPAWNED!", 2.5f);
        }

        Debug.Log("Player fell off track and respawned.");
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive)
        {
            return;
        }

        MovePlayer();
    }

    void GetPlayerInput()
    {
        moveHorizontal = GameInput.GetHorizontal();
        moveVertical = GameInput.GetVertical();
    }

    void MovePlayer()
    {
        if (rb == null) return;

        Vector3 moveDirection = new Vector3(moveHorizontal, 0f, moveVertical);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            // Smooth rotation towards movement direction (Visor clearly points forward!)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    // Assignment Requirement: Collision Handling with OnCollisionEnter()
    void OnCollisionEnter(Collision collision)
    {
        // If PlayerHealth component is attached to this GameObject, PlayerHealth is the authoritative handler
        if (GetComponent<PlayerHealth>() != null) return;

        if (collision.gameObject.CompareTag("Hazard") || collision.gameObject.CompareTag("Enemy") || collision.gameObject.name.Contains("Hazard") || collision.gameObject.name.Contains("Police") || collision.gameObject.name.Contains("Enemy"))
        {
            TakeDamage(damageAmount);
        }
    }

    public void RestoreHealth(int amount)
    {
        PlayerHealth ph = GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.RestoreHealth(amount);
            health = ph.health;
            return;
        }

        if (health <= 0) return;
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        Debug.Log("Player health restored! Current health: " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        PlayerHealth ph = GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(amount);
            health = ph.health;
            return;
        }

        health = Mathf.Clamp(health - amount, 0, maxHealth);
        Debug.Log("Player took damage! Current Health: " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
            GameUIManager.Instance.TriggerHitFlash();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHazardHit();
        }

        // Assignment Requirement: When health <= 0, destroy the Player GameObject
        if (health <= 0)
        {
            health = 0;
            Debug.Log("Player health reached zero. Destroying Player GameObject as required by assignment.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied();
            }

            Destroy(gameObject, 0.05f);
        }
    }
}
