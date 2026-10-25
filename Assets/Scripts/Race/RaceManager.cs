// Module: Race Manager   Owner: Member 2   Purpose: Controls race flow, lap counting, and race state

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Central manager for race state, lap counting, and race results
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        [Header("Race Settings")]
        [SerializeField] private int totalLaps = 3;
        [SerializeField] private float countdownTime = 3f;
        [SerializeField] private float raceTimeout = 180f; // 3 min DNF timeout
        [SerializeField] private List<ShipController> racers = new List<ShipController>();
        [SerializeField] private List<VehicleController> vehicleRacers = new List<VehicleController>();

        [Header("Checkpoints")]
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

        // Race state
        private Dictionary<object, RacerData> racerDataMap = new Dictionary<object, RacerData>();
        private RaceState currentState = RaceState.PreRace;
        private float raceTime = 0f;
        private float countdownTimer = 0f;
        private float postFinishTimer = 0f;
        private bool hasTriggeredFinish = false;

        public RaceState CurrentState => currentState;
        public float RaceTime => raceTime;
        public int TotalLaps => totalLaps;

        private class RacerData
        {
            public int currentLap = 0;
            public int nextCheckpointIndex = 0;
            public float raceTime = 0f;
            public bool finished = false;
            public int position = 0;
        }

        public enum RaceState
        {
            PreRace,
            Countdown,
            Racing,
            Finished
        }

        private void Start()
        {
            // Reset reward session for a new race
            ResultsScreenUI.ResetRewardSession();

            // Find all checkpoints if not set
            if (checkpoints.Count == 0)
            {
                checkpoints = FindObjectsByType<Checkpoint>().ToList();
                checkpoints = checkpoints.OrderBy(c => c.Index).ToList();
            }

            // Find all ships if not set
            if (racers.Count == 0)
            {
                racers = FindObjectsByType<ShipController>().ToList();
            }

            // Find all vehicles if not set
            if (vehicleRacers.Count == 0)
            {
                vehicleRacers = FindObjectsByType<VehicleController>().ToList();
            }

            // Initialize racer data for ships
            foreach (ShipController ship in racers)
            {
                racerDataMap[ship] = new RacerData();
            }

            // Initialize racer data for vehicles
            foreach (VehicleController vehicle in vehicleRacers)
            {
                racerDataMap[vehicle] = new RacerData();
            }

            StartCountdown();
        }

        private void Update()
        {
            switch (currentState)
            {
                case RaceState.Countdown:
                    UpdateCountdown();
                    break;

                case RaceState.Racing:
                    UpdateRace();
                    break;
            }
        }

        private void StartCountdown()
        {
            currentState = RaceState.Countdown;
            countdownTimer = countdownTime;

            // Freeze ships
            foreach (ShipController ship in racers)
            {
                Rigidbody rb = ship.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            // Freeze vehicles during countdown
            foreach (VehicleController vehicle in vehicleRacers)
            {
                Rigidbody rb = vehicle.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            Debug.Log($"Race countdown started: {countdownTime}s");
        }

        private void UpdateCountdown()
        {
            countdownTimer -= Time.deltaTime;

            if (countdownTimer <= 0f)
            {
                StartRace();
            }
        }

        public void StartRace()
        {
            currentState = RaceState.Racing;
            raceTime = 0f;

            Debug.Log("Race started! GO GO GO!");

            // Notify UI
            RaceHUD hud = FindAnyObjectByType<RaceHUD>();
            if (hud != null)
            {
                hud.ShowNotification("GO!", 1f);
            }
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowNotification("GO!", 1.5f);
            }
        }

        private void UpdateRace()
        {
            raceTime += Time.deltaTime;

            // Update positions
            UpdateRacePositions();

            // Check if player has finished; if so, start countdown to finish remaining racers
            bool playerFinished = IsPlayerFinished();
            if (playerFinished)
            {
                postFinishTimer += Time.deltaTime;
            }

            // Check if all racers finished or timeout occurred
            bool allFinished = racerDataMap.Values.Count > 0 && racerDataMap.Values.All(data => data.finished);
            bool dnfTimeout = (playerFinished && postFinishTimer >= 15f) || (raceTime >= raceTimeout);

            if ((allFinished || dnfTimeout) && !hasTriggeredFinish)
            {
                EndRace();
            }
        }

        private void UpdateRacePositions()
        {
            // Sort racers by lap and checkpoint progress
            var sortedRacers = racerDataMap
                .OrderByDescending(kvp => kvp.Value.currentLap)
                .ThenByDescending(kvp => kvp.Value.nextCheckpointIndex)
                .ThenBy(kvp => kvp.Value.raceTime)
                .ToList();

            // Assign positions
            for (int i = 0; i < sortedRacers.Count; i++)
            {
                sortedRacers[i].Value.position = i + 1;
            }
        }

        public void RefreshCheckpoints()
        {
            checkpoints = FindObjectsByType<Checkpoint>().ToList();
            checkpoints = checkpoints.OrderBy(c => c.Index).ToList();
        }

        public bool OnShipPassCheckpoint(ShipController ship, Checkpoint checkpoint)
        {
            return RegisterCheckpointPass(ship, checkpoint, ship.name, ship.GetComponent<ShipInput>() != null);
        }

        public bool OnVehiclePassCheckpoint(VehicleController vehicle, Checkpoint checkpoint)
        {
            // Auto-register vehicle if not in racerDataMap yet
            if (!racerDataMap.ContainsKey(vehicle))
            {
                racerDataMap[vehicle] = new RacerData();
                if (!vehicleRacers.Contains(vehicle))
                {
                    vehicleRacers.Add(vehicle);
                }
            }
            return RegisterCheckpointPass(vehicle, checkpoint, vehicle.name, vehicle.isPlayerInVehicle || vehicle.CompareTag("Player") || vehicle.name.Contains("Player"));
        }

        private bool RegisterCheckpointPass(object racerKey, Checkpoint checkpoint, string racerName, bool isPlayer)
        {
            if (checkpoints.Count == 0)
            {
                RefreshCheckpoints();
            }

            if (!racerDataMap.ContainsKey(racerKey))
            {
                racerDataMap[racerKey] = new RacerData();
            }

            RacerData data = racerDataMap[racerKey];
            if (data.finished) return false;

            // Check if this is the correct next checkpoint or finish line
            bool isExpected = (checkpoint.Index == data.nextCheckpointIndex);
            bool isFinishExpected = (checkpoint.IsFinishLine && (checkpoints.Count == 0 || data.nextCheckpointIndex >= checkpoints.Count - 1));

            if (isExpected || isFinishExpected)
            {
                data.nextCheckpointIndex++;

                // Check if finished lap
                if (checkpoint.IsFinishLine)
                {
                    data.currentLap++;
                    data.nextCheckpointIndex = 0;

                    Debug.Log($"{racerName} completed lap {data.currentLap}/{totalLaps}");

                    // Check if finished race
                    if (data.currentLap >= totalLaps)
                    {
                        data.finished = true;
                        data.raceTime = raceTime;
                        Debug.Log($"{racerName} finished the race! Time: {data.raceTime:F2}s");

                        if (isPlayer)
                        {
                            OnPlayerCompletedRace(data);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private void OnPlayerCompletedRace(RacerData data)
        {
            RaceHUD hud = FindAnyObjectByType<RaceHUD>();
            if (hud != null)
            {
                hud.ShowNotification($"Race Complete! Time: {data.raceTime:F2}s", 5f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCourseCompleted();
            }

            // Also attach or enable ResultsScreenUI if present
            var resultsUI = FindAnyObjectByType<ResultsScreenUI>();
            if (resultsUI != null)
            {
                resultsUI.enabled = true;
            }
            else
            {
                gameObject.AddComponent<ResultsScreenUI>();
            }
        }

        private bool IsPlayerFinished()
        {
            // Check vehicle racer
            var playerVehicle = vehicleRacers.FirstOrDefault(v => v.isPlayerInVehicle || v.CompareTag("Player") || v.name.Contains("Player"));
            if (playerVehicle != null && racerDataMap.ContainsKey(playerVehicle))
            {
                return racerDataMap[playerVehicle].finished;
            }

            // Check ship racer
            var playerShip = racers.FirstOrDefault(s => s.GetComponent<ShipInput>() != null);
            if (playerShip != null && racerDataMap.ContainsKey(playerShip))
            {
                return racerDataMap[playerShip].finished;
            }

            return false;
        }

        private void EndRace()
        {
            hasTriggeredFinish = true;
            currentState = RaceState.Finished;

            // Mark any remaining racers as DNF
            foreach (var kvp in racerDataMap)
            {
                if (!kvp.Value.finished)
                {
                    kvp.Value.finished = true;
                    kvp.Value.raceTime = raceTime;
                }
            }

            Debug.Log("Race finished! All racers processed (or DNF applied).");
        }

        public int GetPlayerPosition()
        {
            var playerVehicle = vehicleRacers.FirstOrDefault(v => v.isPlayerInVehicle || v.CompareTag("Player") || v.name.Contains("Player"));
            if (playerVehicle != null && racerDataMap.ContainsKey(playerVehicle))
            {
                return racerDataMap[playerVehicle].position;
            }

            var playerShip = racers.FirstOrDefault(s => s.GetComponent<ShipInput>() != null);
            if (playerShip != null && racerDataMap.ContainsKey(playerShip))
            {
                return racerDataMap[playerShip].position;
            }

            return 1;
        }

        public int GetPlayerLap()
        {
            var playerVehicle = vehicleRacers.FirstOrDefault(v => v.isPlayerInVehicle || v.CompareTag("Player") || v.name.Contains("Player"));
            if (playerVehicle != null && racerDataMap.ContainsKey(playerVehicle))
            {
                return racerDataMap[playerVehicle].currentLap;
            }

            var playerShip = racers.FirstOrDefault(s => s.GetComponent<ShipInput>() != null);
            if (playerShip != null && racerDataMap.ContainsKey(playerShip))
            {
                return racerDataMap[playerShip].currentLap;
            }

            return 0;
        }

        public float GetCountdownTime()
        {
            return currentState == RaceState.Countdown ? countdownTimer : 0f;
        }
    }
}
