// Module: Race   Owner: Member 2   Purpose: 3-2-1-GO countdown at race start

using UnityEngine;
using System.Collections;

namespace NeonRush
{
    public class CountdownController : MonoBehaviour
    {
        [Header("Countdown Settings")]
        public float countdownDuration = 4f; // 3 seconds + 1 for GO
        public bool countdownActive = false;
        
        private float countdownTimer = 0f;
        
        void Start()
        {
            // Start countdown automatically
            StartCountdown();
        }

        public void StartCountdown()
        {
            countdownActive = true;
            countdownTimer = 0f;
            
            // Notify all vehicles
            GameObject[] vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
            foreach (GameObject vehicle in vehicles)
            {
                PerfectStartSystem perfectStart = vehicle.GetComponent<PerfectStartSystem>();
                if (perfectStart != null)
                {
                    perfectStart.StartCountdown();
                }
            }
            
            // Freeze player vehicle
            GameObject playerVehicle = GameObject.Find("Vehicle");
            if (playerVehicle != null)
            {
                VehicleController vc = playerVehicle.GetComponent<VehicleController>();
                if (vc != null)
                {
                    vc.isPlayerInVehicle = false; // Disable input during countdown
                }
            }
            
            StartCoroutine(CountdownSequence());
        }

        IEnumerator CountdownSequence()
        {
            yield return new WaitForSeconds(1f);
            ShowCountdownNumber(3);
            
            yield return new WaitForSeconds(1f);
            ShowCountdownNumber(2);
            
            yield return new WaitForSeconds(1f);
            ShowCountdownNumber(1);
            
            yield return new WaitForSeconds(1f);
            ShowGo();
            
            countdownActive = false;
            
            // Enable player vehicle
            GameObject playerVehicle = GameObject.Find("Vehicle");
            if (playerVehicle != null)
            {
                VehicleController vc = playerVehicle.GetComponent<VehicleController>();
                if (vc != null)
                {
                    vc.isPlayerInVehicle = true;
                }
            }
            
            // Start race
            RaceManager raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager != null)
            {
                raceManager.StartRace();
            }
            
            // Change game state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Racing);
            }
        }

        void ShowCountdownNumber(int number)
        {
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowNotification(number.ToString(), 1f);
            }
        }

        void ShowGo()
        {
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowNotification("GO!", 1.5f);
            }
        }

        public int GetCurrentNumber()
        {
            if (!countdownActive) return 0;
            
            int remaining = Mathf.CeilToInt(countdownDuration - countdownTimer);
            return Mathf.Clamp(remaining, 0, 3);
        }
    }
}
