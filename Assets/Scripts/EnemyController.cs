using UnityEngine;
using NeonRush;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Attributes")]
    public string enemyName = "CyberInterceptor_Drone";
    public int health = 50;
    public int damageAmount = 5;
    public float moveSpeed = 5.0f;
    public float detectionRadius = 25.0f;
    public float attackDistance = 1.8f;

    [Header("Patrol Settings")]
    public bool isPatrolling = true;
    public float patrolDistance = 12.0f;
    private Vector3 spawnPosition;
    private int patrolDirection = 1;

    [Header("Target Tracking")]
    public Transform playerTarget;
    private Rigidbody rb;
    private Collider col;
    private bool isDestroyed = false;

    // Visuals
    private Light scannerLight;
    private Material hazardMat;

    void Awake()
    {
        gameObject.tag = "Enemy";

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.mass = 2.0f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.freezeRotation = true;

        col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1.4f, 1.4f, 1.8f);
            box.center = new Vector3(0f, 0.7f, 0f);
        }

        spawnPosition = transform.position;
        BuildEnemyVisuals();
    }

    void Start()
    {
        Debug.Log($"[EnemyController] '{enemyName}' spawned at {transform.position}. Ready for player engagement.");
        FindPlayerTarget();
    }

    public void FindPlayerTarget()
    {
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null) p = GameObject.Find("Player");
            if (p != null) playerTarget = p.transform;
        }
    }

    void BuildEnemyVisuals()
    {
        // Red glowing scanner eye light
        GameObject lightObj = new GameObject("ScannerEye");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = new Vector3(0f, 0.8f, 0.8f);
        scannerLight = lightObj.AddComponent<Light>();
        scannerLight.type = LightType.Point;
        scannerLight.color = Color.red;
        scannerLight.range = 7f;
        scannerLight.intensity = 4f;

        // Apply hazard material if available
        hazardMat = Resources.Load<Material>("Materials/HazardMaterial") ?? Resources.Load<Material>("HazardMaterial");
#if UNITY_EDITOR
        if (hazardMat == null)
        {
            hazardMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/HazardMaterial.mat");
        }
#endif
        if (hazardMat == null)
        {
            Shader s = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            if (s != null)
            {
                hazardMat = new Material(s) { name = "HazardMaterial", color = new Color(1f, 0.2f, 0.2f) };
                hazardMat.EnableKeyword("_EMISSION");
                hazardMat.SetColor("_EmissionColor", new Color(1f, 0.1f, 0.1f) * 1.5f);
            }
        }
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && hazardMat != null)
        {
            mr.sharedMaterial = hazardMat;
        }

        // Add distinct combat drone wing stabilizers so it looks completely different from the player
        if (transform.Find("DroneWing_L") == null)
        {
            GameObject wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingL.name = "DroneWing_L";
            wingL.transform.SetParent(transform, false);
            wingL.transform.localPosition = new Vector3(-1.1f, 0.5f, 0f);
            wingL.transform.localScale = new Vector3(1.2f, 0.15f, 0.8f);
            wingL.transform.localRotation = Quaternion.Euler(0f, 15f, -15f);
            if (hazardMat != null) wingL.GetComponent<MeshRenderer>().sharedMaterial = hazardMat;
            Collider cL = wingL.GetComponent<Collider>();
            if (cL != null) Destroy(cL);

            GameObject wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingR.name = "DroneWing_R";
            wingR.transform.SetParent(transform, false);
            wingR.transform.localPosition = new Vector3(1.1f, 0.5f, 0f);
            wingR.transform.localScale = new Vector3(1.2f, 0.15f, 0.8f);
            wingR.transform.localRotation = Quaternion.Euler(0f, -15f, 15f);
            if (hazardMat != null) wingR.GetComponent<MeshRenderer>().sharedMaterial = hazardMat;
            Collider cR = wingR.GetComponent<Collider>();
            if (cR != null) Destroy(cR);

            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "RedEyeVisor";
            eye.transform.SetParent(transform, false);
            eye.transform.localPosition = new Vector3(0f, 0.7f, 0.75f);
            eye.transform.localScale = new Vector3(0.5f, 0.25f, 0.3f);
            if (hazardMat != null) eye.GetComponent<MeshRenderer>().sharedMaterial = hazardMat;
            Collider cE = eye.GetComponent<Collider>();
            if (cE != null) Destroy(cE);
        }
    }

    void Update()
    {
        if (isDestroyed) return;

        UpdateDisableState(); // === NEON RUSH ===

        if (playerTarget == null)
        {
            FindPlayerTarget();
        }

        // Scanner eye strobe
        if (scannerLight != null)
        {
            scannerLight.intensity = 3.5f + Mathf.PingPong(Time.time * 6f, 2.5f);
        }

        if (!isDisabled) // === NEON RUSH ===
        {
            HandleMovement();
        }
    }

    void HandleMovement()
    {
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            // Chase player if in detection radius
            if (distanceToPlayer <= detectionRadius)
            {
                ChasePlayer(distanceToPlayer);
                return;
            }
        }

        // Otherwise patrol locally
        if (isPatrolling)
        {
            PatrolArea();
        }
    }

    void ChasePlayer(float distance)
    {
        Vector3 direction = (playerTarget.position - transform.position);
        direction.y = 0f; // Stay grounded on road plane

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);

            if (distance > attackDistance)
            {
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
        }
    }

    void PatrolArea()
    {
        float offset = transform.position.z - spawnPosition.z;
        if (Mathf.Abs(offset) >= patrolDistance)
        {
            patrolDirection *= -1;
        }

        Vector3 moveDir = new Vector3(0f, 0f, patrolDirection);
        transform.position += moveDir * (moveSpeed * 0.5f) * Time.deltaTime;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);
        }
    }

    // Assignment Requirement: Collision Handling with Player
    void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        // Check if collided with Player GameObject
        PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>() ?? collision.gameObject.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            Debug.Log($"[EnemyController] Rammed Player! Inflicting {damageAmount} damage points.");
            ph.TakeDamage(damageAmount);
            return;
        }

        PlayerController pc = collision.gameObject.GetComponent<PlayerController>() ?? collision.gameObject.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            Debug.Log($"[EnemyController] Rammed PlayerController! Inflicting {damageAmount} damage.");
            pc.TakeDamage(damageAmount);
            return;
        }

        // If rammed by high-speed vehicle, enemy takes damage / gets destroyed
        VehicleController vc = collision.gameObject.GetComponent<VehicleController>() ?? collision.gameObject.GetComponentInParent<VehicleController>();
        if (vc != null)
        {
            if (vc.CurrentSpeed > 15f)
            {
                Debug.Log("[EnemyController] Enemy destroyed by Supercar ram!");
                TakeDamage(health);
            }
            else
            {
                vc.TakeDamage(damageAmount);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"[EnemyController] Enemy took {amount} damage. Current health: {health}");

        if (health <= 0)
        {
            isDestroyed = true;
            Debug.Log("[EnemyController] Enemy eliminated!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(200);
            }
            Destroy(gameObject, 0.1f);
        }
    }

    // === NEON RUSH: PowerUp System ===
    private bool isDisabled = false;
    private float disableTimer = 0f;

    public void Disable(float duration)
    {
        isDisabled = true;
        disableTimer = duration;
        if (scannerLight != null) scannerLight.enabled = false;
    }

    void UpdateDisableState()
    {
        if (isDisabled)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0f)
            {
                isDisabled = false;
                if (scannerLight != null) scannerLight.enabled = true;
            }
        }
    }
    // === END NEON RUSH ===
}
