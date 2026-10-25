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

    [Header("Braking & Reverse Behavior")]
    public float reverseThreshold = 1.2f; // Low-speed threshold before S engages reverse
    public float reverseAcceleration = 35f; // Controlled smooth reverse acceleration

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
    public int collisionDamage = 5;
    public int hazardDamage = 5;
    public int policeRamDamage = 20;
    public float invulnerabilityDuration = 1.0f;
    private float invulnerabilityTimer = 0f;
    public float spawnGraceTimer = 3.0f; // Initial start / drop damage protection
    public bool isDestroyed = false;
    public bool IsInvulnerable => (invulnerabilityTimer > 0f || spawnGraceTimer > 0f || isRecovering);

    [Header("Player Control State")]
    public bool isPlayerInVehicle = false;

    [Header("Checkpoint Recovery")]
    private Checkpoint lastValidCheckpoint = null;
    private Vector3 lastSafePosition = new Vector3(0f, 1.5f, -10f);
    private Quaternion lastSafeRotation = Quaternion.identity;
    private bool isRecovering = false;
    private float recoveryCooldown = 0f;

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

    public void ResetDrivingState()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        moveInput = 0f;
        turnInput = 0f;
        isBraking = false;
        driftInput = false;
        isDrifting = false;
        driftTimer = 0f;
        driftCharge = 0f;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Awake()
    {
        health = maxHealth;
        isDestroyed = false;
        spawnGraceTimer = CountdownController.IsCountingDown ? 3.0f : 0f;
        invulnerabilityTimer = 0f;
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
        ResetDrivingState();
    }

    void Start()
    {
        health = maxHealth;
        isDestroyed = false;
        spawnGraceTimer = CountdownController.IsCountingDown ? 3.0f : 0f;
        invulnerabilityTimer = 0f;
        antiGravityCharges = 1; // Strictly 1 free charge
        antiGravityDuration = 3.5f; // Strictly 3.5s duration
        this.enabled = true;
        ResetDrivingState();

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
        // If destroyed, game inactive, countdown active, or race finished, halt driving inputs and apply brakes
        bool isRaceOver = (RaceManager.Instance != null && RaceManager.Instance.IsRaceFinished) || 
                          (GameManager.Instance != null && (GameManager.Instance.currentState == GameState.Finish || GameManager.Instance.currentState == GameState.Results || GameManager.Instance.currentState == GameState.GameOver));

        bool disableInputs = isDestroyed || (GameManager.Instance != null && !GameManager.Instance.isGameActive) || isRaceOver || CountdownController.IsCountingDown;

        if (disableInputs)
        {
            moveInput = 0f;
            turnInput = 0f;
            isBraking = false;
            driftInput = false;
        }
        else if (isPlayerInVehicle)
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

        if (recoveryCooldown > 0f)
        {
            recoveryCooldown -= Time.deltaTime;
        }

        if (spawnGraceTimer > 0f)
        {
            spawnGraceTimer -= Time.deltaTime;
        }

        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }

        // Fall Protection & Out-of-Bounds Detection:
        // Genuine fall below track or off high skyway deck in gap (Z = 225 to 265)
        bool isOutOfBounds = transform.position.y < -3.5f;
        if (transform.position.z >= 225f && transform.position.z <= 265f && transform.position.y < 2.0f)
        {
            isOutOfBounds = true;
        }

        bool isRaceFinished = (GameManager.Instance != null && (GameManager.Instance.hasWon || GameManager.Instance.currentState == GameState.Finish || GameManager.Instance.currentState == GameState.Results));
        if (isOutOfBounds && !isRecovering && recoveryCooldown <= 0f && !isRaceFinished)
        {
            RecoverToCheckpoint();
        }
    }

    void FixedUpdate()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (isDestroyed || rb == null)
        {
            return;
        }

        // During countdown, vehicle must remain completely stationary at the starting position
        if (CountdownController.IsCountingDown)
        {
            if (rb.linearVelocity.sqrMagnitude > 0.0001f || rb.angularVelocity.sqrMagnitude > 0.0001f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.isGameActive)
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
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        float currentMax = isAntiGravityActive ? antiGravityMaxSpeed : maxSpeed;
        float forwardVelocity = Vector3.Dot(rb.linearVelocity, transform.forward);

        bool wantsForward = (moveInput > 0.05f);
        bool wantsReverse = (moveInput < -0.05f);
        bool wantsBrakeOnly = (isBraking && !wantsReverse);

        // Prevent forward and reverse acceleration from being applied simultaneously
        if (wantsForward && wantsReverse)
        {
            wantsForward = false;
            wantsReverse = false;
        }

        if (wantsReverse)
        {
            // S key pressed:
            // 1. Moving forward faster than low-speed threshold -> Smoothly Brake forward motion
            if (forwardVelocity > reverseThreshold)
            {
                float currentBrakePower = isDrifting ? (brakeForce * 0.5f) : brakeForce;
                float maxDecel = forwardVelocity / Time.fixedDeltaTime;
                float appliedBrake = Mathf.Min(currentBrakePower, maxDecel);
                rb.AddForce(-transform.forward * appliedBrake, ForceMode.Acceleration);
            }
            // 2. Stopped or moving backward/slowly -> Smoothly engage Reverse up to reverseSpeed
            else
            {
                if (forwardVelocity > -reverseSpeed)
                {
                    float revInputMag = Mathf.Abs(moveInput);
                    Vector3 reverseForceVector = -transform.forward * (reverseAcceleration * revInputMag);
                    rb.AddForce(reverseForceVector, ForceMode.Acceleration);
                }
            }
        }
        else if (wantsForward)
        {
            // W key pressed:
            // 3. Moving backward (reversing) faster than threshold -> Smoothly Brake reverse motion
            if (forwardVelocity < -reverseThreshold)
            {
                float maxDecel = (-forwardVelocity) / Time.fixedDeltaTime;
                float appliedBrake = Mathf.Min(brakeForce, maxDecel);
                rb.AddForce(transform.forward * appliedBrake, ForceMode.Acceleration);
            }
            // 4. Stopped or moving forward -> Accelerate forward up to currentMax
            else
            {
                if (forwardVelocity < currentMax)
                {
                    Vector3 accelForce = transform.forward * (moveInput * acceleration);
                    rb.AddForce(accelForce, ForceMode.Acceleration);
                }
            }
        }
        else if (wantsBrakeOnly)
        {
            // Dedicated brake / handbrake active without reverse input:
            // Smoothly oppose existing motion only; never accelerate or reverse from standstill
            if (forwardVelocity > 0.05f)
            {
                float maxDecel = forwardVelocity / Time.fixedDeltaTime;
                float appliedBrake = Mathf.Min(brakeForce, maxDecel);
                rb.AddForce(-transform.forward * appliedBrake, ForceMode.Acceleration);
            }
            else if (forwardVelocity < -0.05f)
            {
                float maxDecel = (-forwardVelocity) / Time.fixedDeltaTime;
                float appliedBrake = Mathf.Min(brakeForce, maxDecel);
                rb.AddForce(transform.forward * appliedBrake, ForceMode.Acceleration);
            }
        }

        // Lateral grip: cancel sideways drift while preserving drifting mechanics
        Vector3 rightVelocity = Vector3.Dot(rb.linearVelocity, transform.right) * transform.right;
        float gripFactor = isDrifting ? 0.45f : 0.90f;
        rb.linearVelocity -= rightVelocity * (gripFactor * Time.fixedDeltaTime * 10f);
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

    public void UpdateLastCheckpoint(Checkpoint cp)
    {
        if (cp == null) return;
        lastValidCheckpoint = cp;
        lastSafePosition = cp.GetSafeRespawnPosition();
        lastSafeRotation = cp.GetSafeRespawnRotation();
    }

    public void RecoverToCheckpoint()
    {
        if (isDestroyed || isRecovering) return;
        isRecovering = true;
        recoveryCooldown = 1.0f;

        Vector3 spawnPos = lastSafePosition + Vector3.up * 0.35f;
        Quaternion spawnRot = lastSafeRotation;

        transform.position = spawnPos;
        transform.rotation = spawnRot;

        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Reset old momentum and provide clean forward heading along the road
            rb.linearVelocity = spawnRot * Vector3.forward * 4f;
            rb.angularVelocity = Vector3.zero;
        }

        if (isAntiGravityActive)
        {
            DeactivateAntiGravity();
        }

        invulnerabilityTimer = 1.5f;

        var cam = FindAnyObjectByType<SimpleCameraFollow>();
        if (cam != null)
        {
            cam.SnapToTarget();
        }

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowNotification("⚠️ FELL OFF TRACK — SAFELY RECOVERED!", 2.5f);
        }

        StartCoroutine(ResetRecoveryStateRoutine());
        Debug.Log($"Vehicle safely recovered to checkpoint: {spawnPos}");
    }

    private System.Collections.IEnumerator ResetRecoveryStateRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        isRecovering = false;
    }

    public void Respawn()
    {
        RecoverToCheckpoint();
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

        // Protection during initial start/drop, countdown, or active invulnerability window
        if (spawnGraceTimer > 0f || CountdownController.IsCountingDown || invulnerabilityTimer > 0f)
        {
            return;
        }

        // Check if colliding with Police interceptor
        bool isPolice = collision.gameObject.name.Contains("Police") || 
                        collision.gameObject.CompareTag("Enemy") ||
                        collision.gameObject.GetComponent<PoliceVehicle>() != null ||
                        collision.gameObject.GetComponentInParent<PoliceVehicle>() != null;

        if (isPolice)
        {
            if (isAntiGravityActive)
            {
                Debug.Log("Anti-Gravity protected vehicle from police ram!");
                return;
            }

            TakeDamage(policeRamDamage);
            invulnerabilityTimer = invulnerabilityDuration;
            return;
        }

        // Ignore ground road / floor / track / boost pads / checkpoint triggers / runway / slabs / terrain
        string colName = collision.gameObject.name.ToLower();
        string colTag = collision.gameObject.tag;
        if (colName.Contains("road") || colName.Contains("floor") || colName.Contains("slab") || 
            colName.Contains("track") || colName.Contains("ground") || colName.Contains("terrain") ||
            colName.Contains("runway") || colName.Contains("grid") || colName.Contains("boostpad") || 
            colName.Contains("checkpoint") || colName.Contains("surface") || colTag == "Finish")
        {
            return;
        }

        // Landing / downward contact detection: if contact surface normal faces upward, it is a ground landing
        if (collision.contactCount > 0)
        {
            ContactPoint contact = collision.GetContact(0);
            if (contact.normal.y > 0.55f)
            {
                // Surface is horizontal ground beneath the vehicle (landing/driving) - NEVER damage
                return;
            }
        }

        bool isHazardOrObstacle = collision.gameObject.CompareTag("Hazard") ||
                                  collision.gameObject.name.Contains("Hazard") ||
                                  collision.gameObject.name.Contains("Barrier") ||
                                  collision.gameObject.name.Contains("Wall") ||
                                  collision.gameObject.name.Contains("Pillar") ||
                                  collision.gameObject.name.Contains("Rail") ||
                                  collision.gameObject.name.Contains("Gate") ||
                                  collision.gameObject.name.Contains("Obstacle");

        // When colliding with barriers, walls, hazards, or track obstacles, reduce 5 health
        if (isHazardOrObstacle || (collision.relativeVelocity.magnitude > 2.5f && collision.contacts.Length > 0 && collision.contacts[0].normal.y < 0.5f))
        {
            if (isAntiGravityActive)
            {
                Debug.Log("Anti-Gravity protected vehicle from collision damage!");
                return;
            }

            TakeDamage(collisionDamage);
            invulnerabilityTimer = invulnerabilityDuration;
        }
    }

    public void RestoreHealth(int amount)
    {
        if (isDestroyed || health <= 0) return;
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        Debug.Log("Vehicle health restored! Current Health: " + health);

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHealth(health, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDestroyed || invulnerabilityTimer > 0f) return;
        invulnerabilityTimer = invulnerabilityDuration;

        health = Mathf.Clamp(health - amount, 0, maxHealth);
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
            rb.AddForce(-transform.forward * 6f + Vector3.up * 0.8f, ForceMode.Impulse);
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

        Material darkMat = Resources.Load<Material>("Materials/DarkMetalMaterial") ?? Resources.Load<Material>("DarkMetalMaterial");
#if UNITY_EDITOR
        if (darkMat == null) darkMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DarkMetalMaterial.mat");
#endif
        if (darkMat != null)
        {
            MeshRenderer mr = smoke.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = darkMat;
        }

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
