// Module: UI   Owner: Member 3   Purpose: Temporary in-race starter for testing without MainMenu

using UnityEngine;

namespace NeonRush
{
    public class SimpleRaceStarter : MonoBehaviour
    {
        void Awake()
        {
            enabled = false;
            return; // Disabled: replaced by CompleteRaceStarter
        }

        void Start()
        {
            // Ensure GameManager exists
            if (GameManager.Instance == null)
            {
                GameObject gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
                Debug.Log("[SimpleRaceStarter] Created GameManager");
            }
            
            // Start race immediately
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Countdown);
                Debug.Log("[SimpleRaceStarter] Started countdown");
            }
            
            // Find vehicle and enable player control
            GameObject vehicle = GameObject.Find("Vehicle");
            if (vehicle != null)
            {
                VehicleController vc = vehicle.GetComponent<VehicleController>();
                if (vc != null)
                {
                    vc.isPlayerInVehicle = true;
                    Debug.Log("[SimpleRaceStarter] Enabled vehicle control");
                }
            }
            
            // Add RaceManager if missing
            if (FindAnyObjectByType<RaceManager>() == null)
            {
                GameObject rm = new GameObject("RaceManager");
                rm.AddComponent<RaceManager>();
                Debug.Log("[SimpleRaceStarter] Created RaceManager");
            }
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.normal.textColor = Color.cyan;
            
            GUI.Label(new Rect(10, 10, 400, 30), "NEON RUSH - Quick Test Mode", style);
            GUI.Label(new Rect(10, 40, 400, 30), "Press ESC for Menu (when implemented)", style);
        }
    }
}
