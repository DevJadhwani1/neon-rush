using UnityEngine;

public class PoliceVehicle : MonoBehaviour
{
    [Header("Interceptor AI Settings")]
    public float patrolSpeed = 18f;
    public float maxChaseSpeed = 32f;
    public float ramForce = 20f;
    public float pursuitTriggerZ = 40f;

    [Header("Siren Settings")]
    public Light redSiren;
    public Light blueSiren;
    public float strobeRate = 12f;

    private Rigidbody rb;
    private Transform playerTarget;
    private VehicleController playerVehicleCtrl;
    private float strobeTimer = 0f;
    private float lastRamTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 450f;
            rb.centerOfMass = new Vector3(0f, -0.4f, 0f);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.center = new Vector3(0f, 0.4f, 0f);
            col.size = new Vector3(2.4f, 0.9f, 4.2f);
        }

        SetupPoliceSiren();
    }

    void Start()
    {
        // Start waiting in Section 2 Chicane entry
        transform.position = new Vector3(14f, 1.5f, 65f);
        transform.rotation = Quaternion.Euler(0f, 30f, 0f);
        FindPlayerTarget();
    }

    void FindPlayerTarget()
    {
        GameObject v = GameObject.Find("Vehicle");
        if (v != null)
        {
            playerTarget = v.transform;
            playerVehicleCtrl = v.GetComponent<VehicleController>();
        }
    }

    void SetupPoliceSiren()
    {
        Transform bar = transform.Find("PoliceSirenBar");
        if (bar == null)
        {
            GameObject barObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barObj.name = "PoliceSirenBar";
            barObj.transform.SetParent(transform, false);
            barObj.transform.localPosition = new Vector3(0f, 1.15f, -0.1f);
            barObj.transform.localScale = new Vector3(1.2f, 0.18f, 0.35f);
            Collider c = barObj.GetComponent<Collider>();
            if (c != null)
            {
                if (Application.isPlaying) Destroy(c);
                else DestroyImmediate(c);
            }

            GameObject rLight = new GameObject("Siren_Red");
            rLight.transform.SetParent(barObj.transform, false);
            rLight.transform.localPosition = new Vector3(-0.4f, 0.2f, 0f);
            redSiren = rLight.AddComponent<Light>();
            redSiren.type = LightType.Point;
            redSiren.range = 14f;
            redSiren.color = Color.red;
            redSiren.intensity = 4.5f;

            GameObject bLight = new GameObject("Siren_Blue");
            bLight.transform.SetParent(barObj.transform, false);
            bLight.transform.localPosition = new Vector3(0.4f, 0.2f, 0f);
            blueSiren = bLight.AddComponent<Light>();
            blueSiren.type = LightType.Point;
            blueSiren.range = 14f;
            blueSiren.color = Color.blue;
            blueSiren.intensity = 4.5f;
        }
    }

    void Update()
    {
        UpdateDisableState(); // === NEON RUSH ===

        if (!isDisabled) // === NEON RUSH ===
        {
            strobeTimer += Time.deltaTime * strobeRate;
            bool isRedPhase = Mathf.Sin(strobeTimer) > 0f;
            if (redSiren != null) redSiren.enabled = isRedPhase;
            if (blueSiren != null) blueSiren.enabled = !isRedPhase;
        }

        // Fall Protection
        if (transform.position.y < -3.5f)
        {
            RespawnOnTrack();
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (isDisabled) // === NEON RUSH ===
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        if (playerTarget == null)
        {
            FindPlayerTarget();
            if (playerTarget == null) return;
        }

        // Before player enters Sector 2, stay parked
        if (playerTarget.position.z < pursuitTriggerZ)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // If player has reached Sector 4 (Gauntlet) and police is still behind in Sector 2/3
        if (playerTarget.position.z > 330f && transform.position.z < 300f)
        {
            // Ambush relocation to Section 4
            transform.position = new Vector3(0f, 1.5f, 350f);
            transform.rotation = Quaternion.identity;
            rb.linearVelocity = Vector3.forward * 15f;
            return;
        }

        Vector3 toPlayer = playerTarget.position - transform.position;
        float distToPlayer = toPlayer.magnitude;

        // If police fell far behind on straightaway, catch up
        if (distToPlayer > 85f && playerTarget.position.z > transform.position.z)
        {
            RespawnBehindPlayer();
            return;
        }

        // Steer towards player
        Vector3 targetPos = playerTarget.position;
        Vector3 steerDir = (targetPos - transform.position).normalized;
        steerDir.y = 0f;

        if (steerDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(steerDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 5.5f * Time.fixedDeltaTime));
        }

        float targetSpeed = patrolSpeed;
        if (distToPlayer > 15f)
        {
            targetSpeed = maxChaseSpeed;
        }
        else if (distToPlayer < 6f)
        {
            targetSpeed = maxChaseSpeed * 1.15f;
        }

        Vector3 desiredVel = transform.forward * targetSpeed;
        rb.linearVelocity = new Vector3(desiredVel.x, rb.linearVelocity.y, desiredVel.z);

        Vector3 torque = Vector3.Cross(transform.up, Vector3.up) * 40f;
        rb.AddTorque(torque, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.Contains("Vehicle") || col.gameObject.CompareTag("Player"))
        {
            if (Time.time - lastRamTime > 1.2f)
            {
                lastRamTime = Time.time;
                Rigidbody targetRb = col.gameObject.GetComponent<Rigidbody>();
                if (targetRb != null)
                {
                    // Apply controlled horizontal push rather than vertical launcher
                    Vector3 forwardPush = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                    targetRb.AddForce(forwardPush * 4.5f, ForceMode.Impulse);
                }

                VehicleController vc = col.gameObject.GetComponent<VehicleController>();
                if (vc != null && !vc.isAntiGravityActive)
                {
                    vc.TakeDamage(20);
                }

                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ShowNotification("🚨 POLICE INTERCEPTOR RAMMED YOU!", 2.0f);
                }

                Debug.Log("Police interceptor rammed player!");
            }
        }
    }

    public void RespawnOnTrack()
    {
        if (playerTarget != null && playerTarget.position.z > 300f)
        {
            transform.position = new Vector3(0f, 1.5f, 350f);
            transform.rotation = Quaternion.identity;
        }
        else if (playerTarget != null && playerTarget.position.z > 50f)
        {
            RespawnBehindPlayer();
        }
        else
        {
            transform.position = new Vector3(0f, 1.5f, 65f);
            transform.rotation = Quaternion.Euler(0f, 30f, 0f);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void RespawnBehindPlayer()
    {
        Vector3 spawnPos = playerTarget.position - playerTarget.forward * 22f + Vector3.up * 1.5f;
        spawnPos.x = Mathf.Clamp(spawnPos.x, -10f, 10f);
        transform.position = spawnPos;
        transform.rotation = playerTarget.rotation;

        if (rb != null)
        {
            rb.linearVelocity = playerTarget.forward * 16f;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Police Interceptor re-engaged behind player at " + spawnPos);
    }

    // === NEON RUSH: PowerUp System & Combat ===
    private bool isDisabled = false;
    private float disableTimer = 0f;
    private int health = 150;

    public void Disable(float duration)
    {
        isDisabled = true;
        disableTimer = duration;
        if (redSiren != null) redSiren.enabled = false;
        if (blueSiren != null) blueSiren.enabled = false;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"[PoliceVehicle] Took {amount} damage. Health: {health}");
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    void UpdateDisableState()
    {
        if (isDisabled)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0f)
            {
                isDisabled = false;
            }
        }
    }
    // === END NEON RUSH ===
}
