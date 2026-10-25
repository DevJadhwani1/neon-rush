using UnityEngine;
using NeonRush;

public class VehicleController : MonoBehaviour
{
    [Header("Arcade Supercar Physics")]
    public float acceleration = 55f; // Increased from 48
    public float maxSpeed = 45f; // Increased from 40
    public float reverseSpeed = 18f; // Increased from 16
    public float turnSpeed = 140f; // Increased from 115 for more responsive
    public float brakeForce = 70f; // Increased from 55
    public float downforce = 22f; // Increased from 16 for more grip
    public float uprightTorque = 95f; // Increased from 55 - much stronger stabilization

    [Header("Suspension Cushion (Prevents Sinking Under Road)")]
    public float rideHeight = 0.65f;
    public float suspensionForce = 65f;
    public float suspensionDamping = 12f;

    [Header("Anti-Gravity Charges (1 Free, Earn More from Energy Cells)")]
    public int antiGravityCharges = 1; // Starts with 1 FREE charge
    public int maxCharges = 3;         // Up to 3 charges total (1 initial + 2 starting area pickups)
    public float hoverHeight = 2.2f;
    public float hoverForce = 75f;
    public float hoverDamping = 14f;
    public float antiGravityMaxSpeed = 58f;
    public float antiGravityDuration = 3.5f;
    public bool isAntiGravityActive = false;
    private float antiGravityTimer = 0f;

    [Header("Drift & Mini-Turbo System")]
    public bool isDrifting = false;
    public float driftCharge = 0f;
    private float driftTimer = 0f;
    private int driftDirection = 0;

    [Header("Health & Damage")]
    public int health = 100;
    public int maxHealth = 100;
    public int hazardDamage = 25;
    public float invulnerabilityDuration = 1.0f;
    private float invulnerabilityTimer = 0f;
    public bool isDestroyed = false;

    [Header("Player Control State")]
    public bool isPlayerInVehicle = false;

    // Runtime state
    private Rigidbody rb;
    private BoxCollider col;
    private float moveInput;
    private float turnInput;
    private bool isBraking;
    private bool driftInput;

    // Visuals & Underglow
    private Light underglowLight;

    public float CurrentSpeed => (rb != null) ? Vector3.Dot(rb.linearVelocity, transform.forward) : 0f;
    public bool IsAntiGravityReady => (!isAntiGravityActive && antiGravityCharges > 0);

    void Awake()
    {
        health = maxHealth;
        isDestroyed = false;
        antiGravityCharges = 1; // Exactly 1 FREE charge at the start
        antiGravityDuration = 3.5f; // Exactly 3.5 seconds time limit

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 400f;
            rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.center = new Vector3(0f, 0.45f, 0f);
            col.size = new Vector3(2.4f, 0.9f, 4.4f);
        }

        // Setup CarVisuals component for high-poly model and dynamic body banking
        if (GetComponent<CarVisuals>() == null)
        {
            gameObject.AddComponent<CarVisuals>();
        }

        SetupUnderglow();
    }

    void Start()
    {
        health = maxHealth;
        isDestroyed = false;
        antiGravityCharges = 1; // Strictly 1 free charge
        antiGravityDuration = 3.5f; // Strictly 3.5s duration
        this.enabled = true;

        Transform oldSmoke = transform.Find("WreckageSmoke");
        if (oldSmoke != null) Destroy(oldSmoke.gameObject);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
            GameUIManager.Instance.UpdateAntiGravity(false, 0f, antiGravityDuration, antiGravityCharges, maxCharges);
        }
    }

    void SetupUnderglow()
    {
        Transform existingUg = transform.Find("UnderglowLight");
        if (existingUg == null)
        {
            GameObject ug = new GameObject("UnderglowLight");
            ug.transform.SetParent(transform, false);
            ug.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            underglowLight = ug.AddComponent<Light>();
            underglowLight.type = LightType.Point;
            underglowLight.range = 8f;
            underglowLight.color = new Color(0f, 0.9f, 1f); // Neon Cyan
            underglowLight.intensity = 4.0f;
        }
        else
        {
            underglowLight = existingUg.GetComponent<Light>();
        }
    }

    void Update()
    {
        // If destroyed, game inactive, or countdown active, halt driving inputs
        if (isDestroyed || (GameManager.Instance != null && !GameManager.Instance.isGameActive) || CountdownController.IsCountingDown)
        {
            moveInput = 0f;
            turnInput = 0f;
            isBraking = CountdownController.IsCountingDown;
            driftInput = false;
            return;
        }

        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }

        if (isPlayerInVehicle)
        {
            GetDrivingInput();
        }
        else
        {
            moveInput = 0f;
            turnInput = 0f;
            isBraking = false;
            driftInput = false;
        }

        UpdateDriftState();
        UpdateAntiGravityTimers();

        // Fall Protection: NEVER falls into an infinite void!
        if (transform.position.y < -3.5f)
        {
            Respawn();
        }
    }

    void FixedUpdate()
    {
        if (isDestroyed || (GameManager.Instance != null && !GameManager.Instance.isGameActive) || rb == null)
        {
            return;
        }

        // Ground clearance suspension keeps vehicle floating smoothly above road
        ApplySuspensionCushion();

        if (isAntiGravityActive)
        {
            ApplyAntiGravityPhysics();
        }
        else
        {
            ApplyDownforceAndStability();
        }

        ApplyDrive();
        ApplySteering();
    }

    void ApplySuspensionCushion()
    {
        Vector3[] rayPoints = {
            new Vector3(-0.9f, 0.5f, 1.4f),
            new Vector3(0.9f, 0.5f, 1.4f),
            new Vector3(-0.9f, 0.5f, -1.4f),
            new Vector3(0.9f, 0.5f, -1.4f)
        };

        float targetHeight = isAntiGravityActive ? hoverHeight : rideHeight;
        float force = isAntiGravityActive ? hoverForce : suspensionForce;
        float damping = isAntiGravityActive ? hoverDamping : suspensionDamping;

        for (int i = 0; i < rayPoints.Length; i++)
        {
            Vector3 worldOrigin = transform.TransformPoint(rayPoints[i]);
            RaycastHit hit;
            if (Physics.Raycast(worldOrigin, -Vector3.up, out hit, targetHeight * 2.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!hit.transform.IsChildOf(transform) && hit.transform != transform)
                {
                    float distance = hit.distance - 0.5f;
                    float compression = targetHeight - distance;
                    if (compression > 0f)
                    {
                        float upwardSpeed = Vector3.Dot(rb.GetPointVelocity(worldOrigin), Vector3.up);
                        float springForce = (compression * force) - (upwardSpeed * damping);
                        if (springForce > 0f)
                        {
                            rb.AddForceAtPosition(Vector3.up * (springForce * 0.25f), worldOrigin, ForceMode.Acceleration);
                        }
                    }
                }
            }
        }
    }

    void GetDrivingInput()
    {
        moveInput = GameInput.GetVertical();
        turnInput = GameInput.GetHorizontal();
        isBraking = GameInput.GetBrake();
        driftInput = GameInput.GetDrift();

        if (GameInput.GetAntiGravityDown())
        {
            if (isAntiGravityActive)
            {
                // Already in hover mode
            }
            else if (antiGravityCharges > 0)
            {
                ActivateAntiGravity();
            }
            else
            {
                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ShowNotification("⚠️ NO ANTI-GRAV CHARGES! (FIND ENERGY CELLS IN START AREA)", 2.0f);
                }
            }
        }
    }

    void UpdateDriftState()
    {
        float speed = CurrentSpeed;
        bool wantsDrift = (driftInput || (isBraking && Mathf.Abs(turnInput) > 0.3f)) && (speed > 10f);

        if (wantsDrift && !isAntiGravityActive)
        {
            if (!isDrifting)
            {
                isDrifting = true;
                driftTimer = 0f;
                driftDirection = (turnInput >= 0f) ? 1 : -1;
            }

            driftTimer += Time.deltaTime;
            driftCharge = Mathf.Clamp01(driftTimer / 2.2f);

            // Dynamic Underglow color shifting during drift
            if (underglowLight != null)
            {
                if (driftTimer >= 1.6f)
                    underglowLight.color = new Color(1f, 0.1f, 0.85f);
                else if (driftTimer >= 0.8f)
                    underglowLight.color = new Color(1f, 0.6f, 0.1f);
                else
                    underglowLight.color = new Color(0f, 0.8f, 1f);
            }
        }
        else
        {
            if (isDrifting)
            {
                if (driftTimer >= 0.75f)
                {
                    TriggerMiniTurbo(driftTimer >= 1.6f);
                }

                isDrifting = false;
                driftTimer = 0f;
                driftCharge = 0f;

                if (underglowLight != null && !isAntiGravityActive)
                {
                    underglowLight.color = new Color(0f, 0.9f, 1f);
                }
            }
        }

        if (GameUIManager.Instance != null && isPlayerInVehicle)
        {
            GameUIManager.Instance.UpdateDrift(isDrifting, driftCharge);
        }
    }

    void TriggerMiniTurbo(bool isMega)
    {
        float boostImpulse = isMega ? 22f : 14f;
        if (rb != null)
        {
            rb.linearVelocity += transform.forward * boostImpulse;
        }

        if (GameUIManager.Instance != null)
        {
            string msg = isMega ? "🔥 MEGA TURBO BOOST! 🔥" : "⚡ MINI-TURBO RELEASED! ⚡";
            GameUIManager.Instance.ShowNotification(msg, 1.8f);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMiniTurbo();
        }

        Debug.Log("Mini-turbo triggered! Power: " + boostImpulse);
    }

    void ApplyDrive()
    {
        float currentMax = isAntiGravityActive ? antiGravityMaxSpeed : maxSpeed;
        float forwardVelocity = Vector3.Dot(rb.linearVelocity, transform.forward);

        if (isBraking && !isDrifting)
        {
            Vector3 brakeForceVector = -rb.linearVelocity.normalized * brakeForce;
            rb.AddForce(brakeForceVector, ForceMode.Acceleration);
        }
        else if (Mathf.Abs(moveInput) > 0.05f)
        {
            if (moveInput > 0 && forwardVelocity < currentMax)
            {
                Vector3 accelForce = transform.forward * moveInput * acceleration;
                rb.AddForce(accelForce, ForceMode.Acceleration);
            }
            else if (moveInput < 0 && forwardVelocity > -reverseSpeed)
            {
                Vector3 reverseForce = transform.forward * moveInput * acceleration * 0.75f;
                rb.AddForce(reverseForce, ForceMode.Acceleration);
            }
        }

        Vector3 rightVelocity = Vector3.Dot(rb.linearVelocity, transform.right) * transform.right;
        float gripFactor = isDrifting ? 0.45f : 0.90f;
        rb.linearVelocity -= rightVelocity * gripFactor * Time.fixedDeltaTime * 10f;
    }

    void ApplySteering()
    {
        if (Mathf.Abs(turnInput) > 0.05f)
        {
            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float steerDirection = (forwardSpeed < -0.5f) ? -1f : 1f;

            float effectiveTurnSpeed = isDrifting ? (turnSpeed * 1.35f) : turnSpeed;
            float turn = turnInput * effectiveTurnSpeed * steerDirection * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    void ApplyDownforceAndStability()
    {
        float forwardSpeed = rb.linearVelocity.magnitude;
        Vector3 downForceVector = -transform.up * forwardSpeed * downforce;
        rb.AddForce(downForceVector, ForceMode.Force);

        Vector3 currentUp = transform.up;
        Vector3 targetUp = Vector3.up;
        Vector3 torque = Vector3.Cross(currentUp, targetUp) * uprightTorque;
        rb.AddTorque(torque, ForceMode.Acceleration);
    }

    void ApplyAntiGravityPhysics()
    {
        rb.AddForce(-Physics.gravity * 1.02f, ForceMode.Acceleration);
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.85f, rb.linearVelocity.z);
        }

        rb.AddForce(transform.forward * 12f, ForceMode.Acceleration);

        Vector3 currentUp = transform.up;
        Vector3 torque = Vector3.Cross(currentUp, Vector3.up) * (uprightTorque * 2.5f);
        rb.AddTorque(torque, ForceMode.Acceleration);
    }

    public void ActivateAntiGravity()
    {
        if (antiGravityCharges <= 0) return;

        antiGravityCharges--;
        isAntiGravityActive = true;
        antiGravityTimer = antiGravityDuration;

        if (underglowLight != null)
        {
            underglowLight.color = new Color(1f, 0.1f, 0.85f);
            underglowLight.intensity = 6.0f;
            underglowLight.range = 10f;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAntiGravityActive();
        }

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowNotification($"🚀 3.5s ANTI-GRAV ACTIVATED! ({(antiGravityCharges > 0 ? antiGravityCharges + " LEFT" : "0 LEFT - DEPLETED")})", 2.0f);
        }

        Debug.Log($"Anti-Gravity ACTIVATED! Remaining Charges: {antiGravityCharges}");
    }

    public void DeactivateAntiGravity()
    {
        isAntiGravityActive = false;
        antiGravityTimer = 0f;

        if (underglowLight != null)
        {
            underglowLight.color = new Color(0f, 0.9f, 1f);
            underglowLight.intensity = 4.0f;
            underglowLight.range = 8f;
        }

        if (rb != null)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y *= 0.25f;
            rb.linearVelocity = vel;
        }

        Debug.Log("Anti-Gravity Deactivated. Charges remaining: " + antiGravityCharges);
    }

    // Recharges an anti-gravity use when picking up an Energy Zone in the starting section
    public void RechargeAntiGravity()
    {
        if (antiGravityCharges < maxCharges)
        {
            antiGravityCharges++;
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowNotification($"⚡ ANTI-GRAV CELL COLLECTED! ({antiGravityCharges}/{maxCharges} CHARGES)", 2.2f);
            }
            Debug.Log("Anti-Gravity Charge Acquired via Energy Zone! Total: " + antiGravityCharges);
        }
        else
        {
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowNotification("⚡ ANTI-GRAV CHARGES ALREADY FULL!", 1.8f);
            }
        }
    }

    void UpdateAntiGravityTimers()
    {
        if (isAntiGravityActive)
        {
            antiGravityTimer -= Time.deltaTime;
            if (antiGravityTimer <= 0f)
            {
                DeactivateAntiGravity();
            }
        }

        if (isPlayerInVehicle && GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateSpeed(Mathf.Abs(CurrentSpeed) * 3.6f);
            GameUIManager.Instance.UpdateAntiGravity(isAntiGravityActive, antiGravityTimer, antiGravityDuration, antiGravityCharges, maxCharges);
            GameUIManager.Instance.UpdateHealth(health, maxHealth);

            float trackProgress = Mathf.Clamp01((transform.position.z + 15f) / 530f);
            float distToFinish = Mathf.Max(0f, 515f - transform.position.z);
            GameUIManager.Instance.UpdateProgress(trackProgress, distToFinish);
        }
    }

    public void Respawn()
    {
        if (isDestroyed) return;

        Vector3 respawnPos = new Vector3(0f, 1.5f, -10f);

        if (transform.position.z >= 410f)
        {
            respawnPos = new Vector3(0f, 1.5f, 415f);
        }
        else if (transform.position.z >= 265f)
        {
            respawnPos = new Vector3(0f, 12.5f, 275f);
        }
        else if (transform.position.z >= 170f)
        {
            respawnPos = new Vector3(0f, 1.5f, 175f);
        }
        else if (transform.position.z >= 105f)
        {
            respawnPos = new Vector3(26f, 1.5f, 110f);
        }
        else if (transform.position.z >= 45f)
        {
            respawnPos = new Vector3(14f, 1.5f, 65f);
        }

        transform.position = respawnPos;
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (isAntiGravityActive)
        {
            DeactivateAntiGravity();
        }

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowNotification("⚠️ FELL OFF TRACK — SAFELY RESPAWNED!", 2.5f);
        }

        Debug.Log("Vehicle fell off track and respawned safely at " + respawnPos);
    }

    public void EnterVehicle()
    {
        if (isDestroyed) return;
        isPlayerInVehicle = true;
        Debug.Log("Vehicle entered.");
    }

    public void ExitVehicle()
    {
        isPlayerInVehicle = false;
        if (isAntiGravityActive)
        {
            DeactivateAntiGravity();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Vehicle exited.");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        if (collision.gameObject.CompareTag("Hazard") || collision.gameObject.name.Contains("Hazard"))
        {
            if (isAntiGravityActive)
            {
                Debug.Log("Anti-Gravity protected vehicle from hazard damage!");
                return;
            }

            if (invulnerabilityTimer <= 0f)
            {
                TakeDamage(hazardDamage);
                invulnerabilityTimer = invulnerabilityDuration;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDestroyed || invulnerabilityTimer > 0f) return;
        invulnerabilityTimer = invulnerabilityDuration;

        health -= amount;
        Debug.Log("Vehicle took damage! Current Health: " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
            GameUIManager.Instance.TriggerHitFlash();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHazardHit();
        }

        if (health <= 0)
        {
            health = 0;
            DestroyVehicle();
        }
        else if (rb != null)
        {
            rb.AddForce(-transform.forward * 14f + Vector3.up * 5f, ForceMode.Impulse);
        }
    }

    // Complete vehicle destruction: Disables all movement, driving, and steering completely
    void DestroyVehicle()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        isPlayerInVehicle = false;

        Debug.Log("VEHICLE DESTROYED! Disabling all controls and halting gameplay.");

        // 1. Lock physics completely so vehicle cannot move
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 2. Clear all input
        moveInput = 0f;
        turnInput = 0f;
        isBraking = false;
        driftInput = false;

        // 3. Deactivate hover boost
        if (isAntiGravityActive)
        {
            DeactivateAntiGravity();
        }

        // 4. Create visual wreckage smoke
        CreateWreckageVisual();

        // 5. Notify GameManager to end the game and show Game Over screen
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnVehicleDestroyed();
        }

        // 6. Disable this component so Update and FixedUpdate will NEVER run again
        this.enabled = false;
    }

    void CreateWreckageVisual()
    {
        GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        smoke.name = "WreckageSmoke";
        smoke.transform.SetParent(transform, false);
        smoke.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        smoke.transform.localScale = new Vector3(2.6f, 1.8f, 3.6f);
        Collider sc = smoke.GetComponent<Collider>();
        if (sc != null) Destroy(sc);

#if UNITY_EDITOR
        Material darkMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DarkMetalMaterial.mat");
        if (darkMat != null)
        {
            MeshRenderer mr = smoke.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = darkMat;
        }
#endif

        if (underglowLight != null)
        {
            underglowLight.color = Color.red;
            underglowLight.intensity = 5.0f;
        }
    }

    // === NEON RUSH: Stat Setters for Vehicle Customization ===
    public void SetMaxSpeed(float value)
    {
        maxSpeed = value;
    }

    public void SetAcceleration(float value)
    {
        acceleration = value;
    }

    public void SetTurnSpeed(float value)
    {
        turnSpeed = value;
    }

    public void SetAntiGravityDuration(float value)
    {
        antiGravityDuration = value;
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        if (health > maxHealth) health = maxHealth;
    }
    // === END NEON RUSH ===
}
