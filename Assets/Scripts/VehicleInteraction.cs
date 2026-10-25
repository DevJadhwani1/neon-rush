using UnityEngine;

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

    void Awake()
    {
        ResolveReferences();
    }

    [Header("Start Configuration")]
    public bool startInsideVehicle = false;

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

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.useGravity = false;
            playerRb.isKinematic = true;
        }

        if (driverSeat != null)
        {
            player.transform.position = driverSeat.position;
            player.transform.SetParent(driverSeat);
        }
        else if (vehicle != null)
        {
            player.transform.position = vehicle.transform.position;
            player.transform.SetParent(vehicle.transform);
        }

        if (playerRenderer != null) playerRenderer.enabled = false;

        if (vehicleController != null)
        {
            vehicleController.EnterVehicle();
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

        Debug.Log("Entered vehicle successfully. Switched to Vehicle Camera.");
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

            if (exitPoint != null)
            {
                player.transform.position = exitPoint.position;
            }
            else if (vehicle != null)
            {
                player.transform.position = vehicle.transform.position + Vector3.left * 2.4f;
            }

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.isKinematic = false;
                playerRb.useGravity = true;
            }

            if (playerRenderer != null) playerRenderer.enabled = true;
            if (playerCollider != null) playerCollider.enabled = true;
            if (playerController != null) playerController.enabled = true;
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

        Debug.Log("Exited vehicle successfully. Switched to Player Camera.");
    }
}
