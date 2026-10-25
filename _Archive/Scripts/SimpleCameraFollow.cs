using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public bool isVehicleCamera = false;

    [Header("Follow Settings")]
    public float distance = 6.5f; // Increased slightly from 6.0
    public float height = 2.5f; // Raised from 2.2 for better view
    public float positionDamping = 10.0f; // Smoother from 8.0
    public float rotationDamping = 12.0f; // Smoother from 8.0

    [Header("Look-At Settings")]
    public float lookAheadDistance = 6.0f;
    public float lookUpOffset = 1.0f;

    [Header("Environment Collision")]
    public bool preventWallClipping = true;
    public float minDistance = 2.0f;
    public LayerMask collisionLayers = ~0;

    [Header("Speed Dynamics")]
    public bool dynamicFOV = true;
    public float baseFOV = 65f; // Increased from 62
    public float maxFOV = 78f; // Increased from 72 for more speed sensation

    private Camera cam;
    private Rigidbody targetRb;
    private VehicleController vehicleCtrl;
    private Vector3 currentVelocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = baseFOV;
        }
    }

    void Start()
    {
        InitializeTarget();
    }

    public void InitializeTarget()
    {
        if (target == null)
        {
            if (isVehicleCamera)
            {
                GameObject v = GameObject.Find("Vehicle");
                if (v != null) target = v.transform;
            }
            else
            {
                GameObject p = GameObject.Find("Player");
                if (p != null) target = p.transform;
            }
        }

        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
            vehicleCtrl = target.GetComponent<VehicleController>();

            if (isVehicleCamera || (vehicleCtrl != null))
            {
                isVehicleCamera = true;
                distance = 6.0f;
                height = 2.2f;
                lookAheadDistance = 6.0f;
                lookUpOffset = 1.0f;
            }
            else
            {
                distance = 3.5f;
                height = 2.0f;
                lookAheadDistance = 2.0f;
                lookUpOffset = 1.2f;
            }

            SnapToTarget();
        }
    }

    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 forward = target.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 targetPos = target.position - forward * distance + Vector3.up * height;
        transform.position = targetPos;

        Vector3 lookTarget = target.position + forward * lookAheadDistance + Vector3.up * lookUpOffset;
        transform.LookAt(lookTarget);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            InitializeTarget();
            if (target == null) return;
        }

        UpdateCameraPositionAndRotation();
        UpdateCameraFOV();
    }

    void UpdateCameraPositionAndRotation()
    {
        // Follow car forward direction steadily
        Vector3 forward = target.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();

        float targetDistance = distance;
        float targetHeight = height;

        // Slight elevation during anti-gravity hover boost
        if (vehicleCtrl != null && vehicleCtrl.isAntiGravityActive)
        {
            targetDistance += 0.8f;
            targetHeight += 0.4f;
        }

        Vector3 desiredPosition = target.position - forward * targetDistance + Vector3.up * targetHeight;
        Vector3 lookTarget = target.position + forward * lookAheadDistance + Vector3.up * lookUpOffset;

        // Wall anti-clipping
        if (preventWallClipping)
        {
            RaycastHit hit;
            Vector3 rayOrigin = target.position + Vector3.up * lookUpOffset;
            Vector3 rayDir = desiredPosition - rayOrigin;
            float maxRayDist = rayDir.magnitude;

            if (Physics.Raycast(rayOrigin, rayDir.normalized, out hit, maxRayDist, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                if (!hit.transform.IsChildOf(target) && hit.transform != target)
                {
                    float clampedDist = Mathf.Max(hit.distance - 0.4f, minDistance);
                    desiredPosition = rayOrigin + rayDir.normalized * clampedDist;
                }
            }
        }

        // Smooth position follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionDamping * Time.deltaTime);

        // Smooth orientation look-at
        Vector3 directionToLook = lookTarget - transform.position;
        if (directionToLook.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToLook, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationDamping * Time.deltaTime);
        }
    }

    void UpdateCameraFOV()
    {
        if (!dynamicFOV || cam == null) return;

        float targetFOV = baseFOV;

        if (isVehicleCamera && targetRb != null)
        {
            float speedRatio = Mathf.Clamp01(targetRb.linearVelocity.magnitude / 32f);
            targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedRatio);

            if (vehicleCtrl != null && vehicleCtrl.isAntiGravityActive)
            {
                targetFOV = Mathf.Max(targetFOV, baseFOV + 5f);
            }
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 3f * Time.deltaTime);
    }
}
