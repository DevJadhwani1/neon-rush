// Module: Core   Owner: Member 3   Purpose: Direct race starter without MainMenu dependency

using UnityEngine;

namespace NeonRush
{
    public class DirectRaceStarter : MonoBehaviour
    {
        void Awake()
        {
            enabled = false;
            return; // Disabled: replaced by CompleteRaceStarter
        }

        private bool hasStarted = false;

        void Start()
        {
            StartRace();
        }

        void StartRace()
        {
            if (hasStarted) return;
            hasStarted = true;

            Debug.Log("[DirectRaceStarter] Initializing race...");

            // 1. Create GameManager if missing
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
                Debug.Log("[DirectRaceStarter] Created GameManager");
            }

            // 2. Set game state to Racing
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Racing);
                Debug.Log("[DirectRaceStarter] Changed state to Racing");
            }

            // 3. Find and enable vehicle
            GameObject vehicle = GameObject.Find("Vehicle");
            if (vehicle != null)
            {
                VehicleController vc = vehicle.GetComponent<VehicleController>();
                if (vc != null)
                {
                    vc.isPlayerInVehicle = true;
                    Debug.Log("[DirectRaceStarter] Enabled vehicle control");
                }
                else
                {
                    Debug.LogWarning("[DirectRaceStarter] Vehicle found but no VehicleController!");
                }
            }
            else
            {
                Debug.LogWarning("[DirectRaceStarter] No Vehicle found in scene!");
            }

            // 4. Check for TrackBuilder
            TrackBuilder tb = FindAnyObjectByType<TrackBuilder>();
            if (tb != null)
            {
                Debug.Log("[DirectRaceStarter] TrackBuilder found and should generate track");
            }
            else
            {
                Debug.LogWarning("[DirectRaceStarter] No TrackBuilder found!");
            }

            // 5. Check camera
            SimpleCameraFollow cam = FindAnyObjectByType<SimpleCameraFollow>();
            if (cam != null)
            {
                Debug.Log("[DirectRaceStarter] Camera follow script found");
            }
            else
            {
                Debug.LogWarning("[DirectRaceStarter] No SimpleCameraFollow script found!");
            }

            Debug.Log("[DirectRaceStarter] ===== RACE READY! =====");
            Debug.Log("[DirectRaceStarter] Controls: WASD to drive, SPACE to drift, SHIFT for boost");
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            style.normal.textColor = Color.cyan;
            style.alignment = TextAnchor.UpperLeft;

            GUI.Label(new Rect(10, 10, 600, 30), "NEON RUSH - Direct Test Mode", style);
            
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(10, 35, 600, 25), "Controls: W/S=Accel/Brake | A/D=Steer | SPACE=Drift | SHIFT=Boost", style);
            
            // Show vehicle info
            GameObject vehicle = GameObject.Find("Vehicle");
            if (vehicle != null)
            {
                VehicleController vc = vehicle.GetComponent<VehicleController>();
                if (vc != null)
                {
                    string info = $"Speed: {vc.CurrentSpeed:F1} | Health: {vc.health} | Boost Charges: {vc.antiGravityCharges}";
                    GUI.Label(new Rect(10, 60, 600, 25), info, style);
                }
            }
        }
    }
}
