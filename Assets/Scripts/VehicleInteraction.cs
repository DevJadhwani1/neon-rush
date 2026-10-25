using UnityEngine;
using NeonRush;

public class VehicleInteraction : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public GameObject vehicle;
    public Transform driverSeat;
    public Transform exitPoint;
    public Camera playerCamera;
    public Camera vehicleCamera;

    [Header("Interaction Settings")]
    public float interactionDistance = 6.0f;

    [Header("State")]
    public bool isInVehicle = false;

    private PlayerController playerController;
    private VehicleController vehicleController;
    private CapsuleCollider playerCollider;
    private MeshRenderer playerRenderer;
    private Rigidbody playerRb;
    private System.Collections.Generic.Dictionary<Renderer, bool> playerRendererStates = new System.Collections.Generic.Dictionary<Renderer, bool>();

    void Awake()
    {
        ResolveReferences();
    }

    [Header("Start Configuration")]
    public bool startInsideVehicle = true;

    void Start()
    {
        ResolveReferences();

        if (startInsideVehicle)
        {
            EnterVehicle();
        }
        else
        {
            // Start in on-foot mode properly:
            isInVehicle = false;

            if (vehicleCamera != null)
            {
                vehicleCamera.gameObject.SetActive(false);
            }

            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                playerCamera.enabled = true;
                playerCamera.tag = "MainCamera";

                SimpleCameraFollow scf = playerCamera.GetComponent<SimpleCameraFollow>();
                if (scf != null && player != null)
                {
                    scf.target = player.transform;
                    scf.distance = 3.8f;
                    scf.height = 1.8f;
                    scf.lookAheadDistance = 3.0f;
                    scf.lookUpOffset = 1.0f;
                    scf.SnapToTarget();
                }
            }
        }
    }

    public void ResolveReferences()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null)
            {
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null) player = pc.gameObject;
            }
        }

        if (vehicle == null)
        {
            vehicle = GameObject.Find("Vehicle");
            if (vehicle == null)
            {
                VehicleController vc = FindAnyObjectByType<VehicleController>();
                if (vc != null) vehicle = vc.gameObject;
            }
        }

        if (player != null)
        {
            if (playerController == null) playerController = player.GetComponent<PlayerController>();
            if (playerCollider == null) playerCollider = player.GetComponent<CapsuleCollider>();
            if (playerRenderer == null) playerRenderer = player.GetComponent<MeshRenderer>();
            if (playerRb == null) playerRb = player.GetComponent<Rigidbody>();
        }

        if (vehicle != null)
        {
            if (vehicleController == null) vehicleController = vehicle.GetComponent<VehicleController>();

            if (driverSeat == null)
            {
                Transform ds = vehicle.transform.Find("DriverSeat");
                if (ds != null) driverSeat = ds;
                else
                {
                    GameObject seat = new GameObject("DriverSeat");
                    seat.transform.SetParent(vehicle.transform, false);
                    seat.transform.localPosition = new Vector3(0f, 0.5f, 0.1f);
                    driverSeat = seat.transform;
                }
            }

            if (exitPoint == null)
            {
                Transform ep = vehicle.transform.Find("ExitPoint");
                if (ep != null) exitPoint = ep;
                else
                {
                    GameObject exit = new GameObject("ExitPoint");
                    exit.transform.SetParent(vehicle.transform, false);
                    exit.transform.localPosition = new Vector3(-2.4f, 0f, 0f);
                    exitPoint = exit.transform;
                }
            }
        }

        if (playerCamera == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null) playerCamera = camObj.GetComponent<Camera>();
        }

        if (vehicleCamera == null)
        {
            GameObject vCamObj = GameObject.Find("VehicleCamera");
            if (vCamObj != null) vehicleCamera = vCamObj.GetComponent<Camera>();
        }
    }

    void Update()
    {
        // If game is over or vehicle is destroyed, lock out interaction completely
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive)
        {
            if (GameUIManager.Instance != null) GameUIManager.Instance.SetPrompt("");
            return;
        }

        if (vehicleController != null && vehicleController.isDestroyed)
        {
            if (GameUIManager.Instance != null) GameUIManager.Instance.SetPrompt("");
            return;
        }

        if (GameInput.GetInteractDown())
        {
            if (!isInVehicle)
            {
                TryEnterVehicle();
            }
            else
            {
                ExitVehicle();
            }
        }

        UpdateInteractionPrompt();
    }

    void UpdateInteractionPrompt()
    {
        if (GameUIManager.Instance == null) return;

        if (isInVehicle)
        {
            int charges = (vehicleController != null) ? vehicleController.antiGravityCharges : 0;
            string hoverPrompt = charges > 0 ? $"[SPACE] HOVER ({charges}x 3.5s)" : "[SPACE] (NO CHARGES)";
            GameUIManager.Instance.SetPrompt($"[W/S] DRIVE | [A/D] STEER | [SHIFT] DRIFT | {hoverPrompt} | [E] EXIT");
            return;
        }

        if (player == null || vehicle == null)
        {
            ResolveReferences();
        }

        if (player != null && vehicle != null)
        {
            float dist = Vector3.Distance(player.transform.position, vehicle.transform.position);
            if (dist <= interactionDistance)
            {
                GameUIManager.Instance.SetPrompt("[E] ENTER VEHICLE");
            }
            else
            {
                GameUIManager.Instance.SetPrompt("[WASD] WALK TO VEHICLE | [E] ENTER");
            }
        }
    }

    public void TryEnterVehicle()
    {
        if (player == null || vehicle == null)
        {
            ResolveReferences();
            if (player == null || vehicle == null) return;
        }

        if (vehicleController != null && vehicleController.isDestroyed) return;

        float distance = Vector3.Distance(player.transform.position, vehicle.transform.position);
        if (distance <= interactionDistance)
        {
            EnterVehicle();
        }
        else
        {
            Debug.Log("Too far from vehicle: " + distance.ToString("F1") + "m");
        }
    }

    public void EnterVehicle()
    {
        if (vehicleController != null && vehicleController.isDestroyed) return;

        isInVehicle = true;

        if (playerController != null) playerController.enabled = false;
        if (playerCollider != null) playerCollider.enabled = false;

        if (player != null)
        {
            // Disable all colliders on player hierarchy to prevent physics interference with vehicle
            Collider[] allPlayerCols = player.GetComponentsInChildren<Collider>(true);
            foreach (var c in allPlayerCols)
            {
                c.enabled = false;
            }

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.useGravity = false;
                playerRb.detectCollisions = false;
                playerRb.isKinematic = true;
            }

            if (driverSeat != null)
            {
                player.transform.SetParent(driverSeat, false);
                player.transform.localPosition = new Vector3(0f, -500f, 0f);
                player.transform.localRotation = Quaternion.identity;
            }
            else if (vehicle != null)
            {
                player.transform.SetParent(vehicle.transform, false);
                player.transform.localPosition = new Vector3(0f, -500f, 0f);
                player.transform.localRotation = Quaternion.identity;
            }

            // Hide ALL player renderers (including child meshes, jetpack, visor, etc.)
            Renderer[] allRenderers = player.GetComponentsInChildren<Renderer>(true);
            foreach (var r in allRenderers)
            {
                if (r != null)
                {
                    if (!playerRendererStates.ContainsKey(r))
                    {
                        playerRendererStates[r] = r.enabled;
                    }
                    r.enabled = false;
                }
            }

            // Also disable any lights (e.g. HelmetLight) to prevent light gizmos and halos
            Light[] allLights = player.GetComponentsInChildren<Light>(true);
            foreach (var l in allLights)
            {
                if (l != null) l.enabled = false;
            }
        }

        if (vehicleController != null)
        {
            vehicleController.EnterVehicle();

            // Update HUD to show vehicle health
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateHealth(vehicleController.health, vehicleController.maxHealth);
            }
        }

        // Camera Switch: Vehicle Camera becomes primary MainCamera
        if (playerCamera != null)
        {
            playerCamera.tag = "Untagged";
            playerCamera.gameObject.SetActive(false);
        }

        if (vehicleCamera != null)
        {
            vehicleCamera.gameObject.SetActive(true);
            vehicleCamera.enabled = true;
            vehicleCamera.tag = "MainCamera";

            SimpleCameraFollow scf = vehicleCamera.GetComponent<SimpleCameraFollow>();
            if (scf != null)
            {
                scf.target = vehicle.transform;
                scf.SnapToTarget();
            }
        }

        Debug.Log("Entered vehicle successfully. Switched to Vehicle Camera and concealed all character renderers.");
    }

    public void ExitVehicle()
    {
        isInVehicle = false;

        if (vehicleController != null)
        {
            vehicleController.ExitVehicle();
        }

        if (player != null)
        {
            player.transform.SetParent(null);

            Vector3 targetExit = (exitPoint != null) ? exitPoint.position : (vehicle != null ? vehicle.transform.position + vehicle.transform.TransformDirection(Vector3.left * 2.4f) : player.transform.position);

            // Ground alignment raycast to prevent falling through floor
            RaycastHit hit;
            if (Physics.Raycast(targetExit + Vector3.up * 2f, Vector3.down, out hit, 6f, ~0, QueryTriggerInteraction.Ignore))
            {
                targetExit.y = hit.point.y + 1f;
            }

            player.transform.position = targetExit;
            if (vehicle != null) player.transform.rotation = Quaternion.Euler(0f, vehicle.transform.eulerAngles.y, 0f);

            // Restore all player renderers to their original states
            foreach (var kvp in playerRendererStates)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.enabled = kvp.Value;
                }
            }
            playerRendererStates.Clear();

            // Re-enable player lights
            Light[] allLights = player.GetComponentsInChildren<Light>(true);
            foreach (var l in allLights)
            {
                if (l != null) l.enabled = true;
            }

            // Re-enable player colliders and physics
            Collider[] allPlayerCols = player.GetComponentsInChildren<Collider>(true);
            foreach (var c in allPlayerCols)
            {
                c.enabled = true;
            }

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.isKinematic = false;
                playerRb.detectCollisions = true;
                playerRb.useGravity = true;
            }

            if (playerCollider != null) playerCollider.enabled = true;
            if (playerController != null) playerController.enabled = true;

            // Update HUD to show player suit health
            if (GameUIManager.Instance != null)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) GameUIManager.Instance.UpdateHealth(ph.health, ph.maxHealth);
                else if (playerController != null) GameUIManager.Instance.UpdateHealth(playerController.health, playerController.maxHealth);
            }
        }

        // Camera Switch: Player Camera becomes primary MainCamera
        if (vehicleCamera != null)
        {
            vehicleCamera.tag = "Untagged";
            vehicleCamera.gameObject.SetActive(false);
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.enabled = true;
            playerCamera.tag = "MainCamera";

            SimpleCameraFollow scf = playerCamera.GetComponent<SimpleCameraFollow>();
            if (scf != null)
            {
                scf.target = player.transform;
                scf.SnapToTarget();
            }
        }

        Debug.Log("Exited vehicle successfully. Restored character renderers, physics, and switched to Player Camera.");
    }
}
