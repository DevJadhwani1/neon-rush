// Module: Core   Owner: Member 3   Purpose: Complete race initialization that spawns vehicle and sets up everything

using UnityEngine;

namespace NeonRush
{
    public class CompleteRaceStarter : MonoBehaviour
    {
        private bool hasInitialized = false;
        private GameObject vehicle;

        void Start()
        {
            Debug.Log("[NeonRush] CompleteRaceStarter.Start() - Initializing race");
            InitializeRace();
        }

        void InitializeRace()
        {
            if (hasInitialized) return;
            hasInitialized = true;

            Debug.Log("=== [NeonRush] COMPLETE RACE STARTER INITIALIZING ===");

            // 1. Create GameManager
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("_GameManager");
                gmObj.AddComponent<GameManager>();
                Debug.Log("[NeonRush] [✓] Created GameManager");
            }
            else
            {
                Debug.Log("[NeonRush] [✓] GameManager already exists");
            }

            // 2. Find or spawn vehicle
            vehicle = GameObject.Find("Vehicle");
            if (vehicle == null)
            {
                // Try to load Vehicle prefab
                GameObject vehiclePrefab = Resources.Load<GameObject>("Vehicle");
                if (vehiclePrefab == null)
                {
                    vehiclePrefab = Resources.Load<GameObject>("Prefabs/Vehicle");
                }
                if (vehiclePrefab == null)
                {
                    // Last resort: load any prefab with VehicleController
                    GameObject[] allPrefabs = Resources.LoadAll<GameObject>("");
                    foreach (GameObject prefab in allPrefabs)
                    {
                        if (prefab.GetComponent<VehicleController>() != null)
                        {
                            vehiclePrefab = prefab;
                            break;
                        }
                    }
                }

                if (vehiclePrefab != null)
                {
                    vehicle = Instantiate(vehiclePrefab, new Vector3(0f, 1.5f, -10f), Quaternion.identity);
                    vehicle.name = "Vehicle";
                    Debug.Log("[✓] Spawned Vehicle from prefab");
                }
                else
                {
                    // Create vehicle from scratch
                    vehicle = CreateVehicleFromScratch();
                    Debug.Log("[✓] Created Vehicle from scratch");
                }
            }
            else
            {
                Debug.Log("[✓] Vehicle already exists in scene");
            }

            // 3. Enable vehicle control
            if (vehicle != null)
            {
                VehicleController vc = vehicle.GetComponent<VehicleController>();
                if (vc != null)
                {
                    vc.isPlayerInVehicle = true;
                    vc.enabled = true;
                    Debug.Log("[✓] Enabled vehicle control");
                }
            }

            // 4. Set up camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                SimpleCameraFollow camFollow = mainCam.GetComponent<SimpleCameraFollow>();
                if (camFollow == null)
                {
                    camFollow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();
                }
                // Camera will auto-find Vehicle in its Start method
                Debug.Log("[✓] Camera follow script ready");
            }

            // 5. Check for track
            TrackBuilder tb = FindAnyObjectByType<TrackBuilder>();
            if (tb != null)
            {
                Debug.Log("[✓] TrackBuilder found - track should be generated");
            }
            else
            {
                Debug.LogWarning("[!] No TrackBuilder - track may be pre-built or missing");
            }

            // 6. Set game state and ensure Time.timeScale = 1
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.isGameActive = true;
                GameManager.Instance.ChangeState(GameState.Racing);
                Debug.Log("[NeonRush] [✓] Game state set to Racing, timeScale = 1");
            }

            Debug.Log("=== [NeonRush] RACE INITIALIZATION COMPLETE ===");
            Debug.Log("[NeonRush] CONTROLS: W/S=Drive | A/D=Steer | SPACE=Drift | SHIFT=Boost");
        }

        GameObject CreateVehicleFromScratch()
        {
            GameObject veh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            veh.name = "Vehicle";
            veh.transform.position = new Vector3(0f, 1.5f, -10f);
            veh.transform.localScale = new Vector3(2.4f, 0.9f, 4.4f);

            // Add Rigidbody
            Rigidbody rb = veh.AddComponent<Rigidbody>();
            rb.mass = 400f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 2f;

            // Add VehicleController
            VehicleController vc = veh.AddComponent<VehicleController>();

            // Set material color
            Renderer rend = veh.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0f, 0.9f, 1f); // Cyan
                rend.material = mat;
            }

            return veh;
        }

        void OnGUI()
        {
            // Title
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 28;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.cyan;
            titleStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(10, 10, 600, 35), "NEON RUSH: Grid Runners - TEST MODE", titleStyle);

            // Controls
            GUIStyle controlStyle = new GUIStyle(GUI.skin.label);
            controlStyle.fontSize = 18;
            controlStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(10, 45, 800, 25), "W/S=Accelerate/Brake  |  A/D=Steer  |  SPACE=Drift  |  SHIFT=Anti-Gravity Boost", controlStyle);

            // Vehicle stats
            if (vehicle != null)
            {
                VehicleController vc = vehicle.GetComponent<VehicleController>();
                if (vc != null)
                {
                    controlStyle.fontSize = 16;
                    string stats = $"Speed: {vc.CurrentSpeed:F1} m/s  |  Health: {vc.health}/{vc.maxHealth}  |  Boost Charges: {vc.antiGravityCharges}/{vc.maxCharges}  |  Active: {vc.isAntiGravityActive}";
                    GUI.Label(new Rect(10, 70, 900, 25), stats, controlStyle);
                }
            }

            // Instructions
            controlStyle.fontSize = 14;
            controlStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(10, 95, 800, 20), "Press SHIFT to use your FREE anti-gravity boost! Find Energy Cells to recharge.", controlStyle);
        }
    }
}
