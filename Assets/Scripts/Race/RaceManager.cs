// Module: Race Manager   Owner: Member 2   Purpose: Controls single-run race flow, progression, and race state

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonRush
{
    /// <summary>
    /// Central manager for single-run race state, checkpoint progression, and race results.
    /// Operates without any lap system: player completes one continuous run from start to finish endpoint.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        [Header("Race Settings")]
        [SerializeField] private int totalLaps = 1;
        [SerializeField] private float countdownTime = 3f;
        [SerializeField] private float raceTimeout = 180f; // 3 min DNF timeout
        [SerializeField] private List<ShipController> racers = new List<ShipController>();
        [SerializeField] private List<VehicleController> vehicleRacers = new List<VehicleController>();

        [Header("Checkpoints")]
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

        private static RaceManager _instance;
        public static RaceManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<RaceManager>();
                return _instance;
            }
            private set => _instance = value;
        }

        // Race state
        private Dictionary<object, RacerData> racerDataMap = new Dictionary<object, RacerData>();
        private RaceState currentState = RaceState.PreRace;
        private float raceTime = 0f;
        private float countdownTimer = 0f;
        private float postFinishTimer = 0f;
        private bool hasTriggeredFinish = false;
        private VehicleController cachedPlayerVehicle = null;

        public RaceState CurrentState => currentState;
        public float RaceTime => raceTime;
        public int TotalLaps
        {
            get => totalLaps;
            set => totalLaps = Mathf.Max(1, value);
        }

        public void ConfigureCircuitLaps(string circuitId)
        {
            switch (circuitId)
            {
                case "GRID_CIRCUIT":
                    totalLaps = 2;
                    break;
                case "CYBER_SPEEDWAY":
                    totalLaps = 3;
                    break;
                case "NEON_DOWNTOWN":
                    totalLaps = 3;
                    break;
                default:
                    totalLaps = 2;
                    break;
            }
        }

        private class RacerData
        {
            public int currentLap = 0;
            public int nextCheckpointIndex = 0;
            public float raceTime = 0f;
            public bool finished = false;
            public int position = 0;
            public float lastFinishTime = -10f;
        }

        public enum RaceState
        {
            PreRace = 0,
            WaitingToStart = 0,
            NotStarted = 0,
            Countdown = 1,
            Racing = 2,
            Finished = 3,
            Completed = 3,
            GameOver = 4
        }

        public bool IsRaceFinished => currentState == RaceState.Finished;
        public bool IsCompleted => currentState == RaceState.Finished;

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public VehicleController GetPlayerVehicle()
        {
            if (cachedPlayerVehicle != null) return cachedPlayerVehicle;
            cachedPlayerVehicle = vehicleRacers.FirstOrDefault(v => v != null && (v.isPlayerInVehicle || v.CompareTag("Player") || v.name.Contains("Player") || v.name == "Vehicle"));
            if (cachedPlayerVehicle == null)
            {
                cachedPlayerVehicle = FindObjectsByType<VehicleController>().FirstOrDefault(v => v != null && (v.isPlayerInVehicle || v.CompareTag("Player") || v.name.Contains("Player") || v.name == "Vehicle"));
            }
            return cachedPlayerVehicle;
        }

        public void SetGameOver()
        {
            currentState = RaceState.GameOver;
        }

        private void Start()
        {
            // Reset reward session for a new race
            ResultsScreenUI.ResetRewardSession();
            hasTriggeredFinish = false;
            postFinishTimer = 0f;

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
                if (ship != null) racerDataMap[ship] = new RacerData();
            }

            // Initialize racer data for vehicles
            foreach (VehicleController vehicle in vehicleRacers)
            {
                if (vehicle != null) racerDataMap[vehicle] = new RacerData();
            }

            // Align player vehicle at starting position on the intended road
            var playerVeh = GetPlayerVehicle();
            if (playerVeh != null)
            {
                playerVeh.transform.position = new Vector3(0f, 0.5f, -10f);
                playerVeh.transform.rotation = Quaternion.identity;
                Rigidbody pRb = playerVeh.GetComponent<Rigidbody>();
                if (pRb != null)
                {
                    pRb.linearVelocity = Vector3.zero;
                    pRb.angularVelocity = Vector3.zero;
                }

                var cam = FindAnyObjectByType<SimpleCameraFollow>();
                if (cam != null)
                {
                    cam.SnapToTarget();
                }
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
                if (ship == null) continue;
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
                if (vehicle == null) continue;
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

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateTimer(raceTime);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.raceTime = raceTime;
            }

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
            // Sort racers by finished status, then checkpoint progression, then race time
            var sortedRacers = racerDataMap
                .OrderByDescending(kvp => kvp.Value.finished)
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
            if (vehicle == null) return false;

            // Auto-register vehicle if not in racerDataMap yet
            if (!racerDataMap.ContainsKey(vehicle))
            {
                racerDataMap[vehicle] = new RacerData();
                if (!vehicleRacers.Contains(vehicle))
                {
                    vehicleRacers.Add(vehicle);
                }
            }
            bool isPlayer = vehicle.isPlayerInVehicle || vehicle.CompareTag("Player") || vehicle.name.Contains("Player") || vehicle.name == "Vehicle";
            return RegisterCheckpointPass(vehicle, checkpoint, vehicle.name, isPlayer);
        }

        private bool RegisterCheckpointPass(object racerKey, Checkpoint checkpoint, string racerName, bool isPlayer)
        {
            if (checkpoint == null) return false;

            if (checkpoints.Count == 0)
            {
                RefreshCheckpoints();
            }

            if (!racerDataMap.ContainsKey(racerKey))
            {
                racerDataMap[racerKey] = new RacerData();
            }

            RacerData data = racerDataMap[racerKey];
            if (data.finished || currentState == RaceState.Finished || currentState == RaceState.GameOver) return false;

            // 1. Direction check: prevent reversing across checkpoints or finish line
            if (racerKey is VehicleController vc)
            {
                Vector3 fwd = vc.transform.forward;
                Rigidbody vRb = vc.GetComponent<Rigidbody>();
                Vector3 vel = (vRb != null && vRb.linearVelocity.sqrMagnitude > 0.5f) ? vRb.linearVelocity.normalized : fwd;
                float fwdDot = Vector3.Dot(checkpoint.transform.forward, fwd);
                float velDot = Vector3.Dot(checkpoint.transform.forward, vel);
                if (fwdDot < -0.2f && velDot < -0.2f)
                {
                    Debug.LogWarning($"[RaceManager] Checkpoint {checkpoint.Index} crossed in WRONG direction by {racerName}. Ignored.");
                    return false;
                }
            }
            else if (racerKey is ShipController sc)
            {
                float fwdDot = Vector3.Dot(checkpoint.transform.forward, sc.transform.forward);
                if (fwdDot < -0.2f)
                {
                    Debug.LogWarning($"[RaceManager] Checkpoint {checkpoint.Index} crossed in WRONG direction by {racerName}. Ignored.");
                    return false;
                }
            }

            // 2. Validate sequence or finish line
            bool isExpected = (checkpoint.Index == data.nextCheckpointIndex);
            bool isFinishExpected = (checkpoint.IsFinishLine && (checkpoints.Count <= 1 || data.nextCheckpointIndex >= checkpoints.Count - 1));

            // Prevent finish line crossing if intermediate checkpoints were skipped
            if (checkpoint.IsFinishLine && checkpoints.Count > 1 && data.nextCheckpointIndex < checkpoints.Count - 1)
            {
                Debug.LogWarning($"[RaceManager] Finish line crossed prematurely by {racerName} ({data.nextCheckpointIndex}/{checkpoints.Count - 1} checkpoints). Run not completed.");
                return false;
            }

            if (isExpected || isFinishExpected)
            {
                data.nextCheckpointIndex++;

                if (checkpoint.IsFinishLine)
                {
                    data.finished = true;
                    data.raceTime = raceTime;
                    data.lastFinishTime = Time.time;
                    currentState = RaceState.Finished;

                    Debug.Log($"{racerName} reached finish endpoint! Time: {data.raceTime:F2}s");

                    if (isPlayer)
                    {
                        if (racerKey is VehicleController pVehicle)
                        {
                            DisableVehicleControls(pVehicle);
                        }
                        OnPlayerCompletedRace(data);
                    }
                }
                return true;
            }
            return false;
        }

        public void FinishPlayerRun()
        {
            if (hasTriggeredFinish || currentState == RaceState.Finished) return;

            var playerVehicle = GetPlayerVehicle();
            RacerData data = null;
            if (playerVehicle != null && racerDataMap.ContainsKey(playerVehicle))
            {
                data = racerDataMap[playerVehicle];
            }
            else
            {
                data = new RacerData();
                if (playerVehicle != null) racerDataMap[playerVehicle] = data;
            }

            data.finished = true;
            data.raceTime = raceTime;
            currentState = RaceState.Finished;

            if (playerVehicle != null)
            {
                DisableVehicleControls(playerVehicle);
            }

            OnPlayerCompletedRace(data);
        }

        private void DisableVehicleControls(VehicleController vehicle)
        {
            if (vehicle == null) return;
            vehicle.isPlayerInVehicle = false;
            Rigidbody rb = vehicle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void OnPlayerCompletedRace(RacerData data)
        {
            if (hasTriggeredFinish) return;
            hasTriggeredFinish = true;
            currentState = RaceState.Finished;

            float finalTime = raceTime;
            data.raceTime = finalTime;

            Debug.Log($"[RaceManager] Single Run Completed! Final Time: {finalTime:F2}s, Position: {data.position}");

            RaceHUD hud = FindAnyObjectByType<RaceHUD>();
            if (hud != null)
            {
                hud.ShowNotification($"Course Complete! Time: {finalTime:F2}s", 5f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.raceTime = finalTime;
                GameManager.Instance.OnCourseCompleted();
            }

            // Halt all vehicles and freeze controls
            foreach (var v in vehicleRacers)
            {
                if (v != null)
                {
                    DisableVehicleControls(v);
                }
            }

            // Results screen UI
            var resultsUI = FindAnyObjectByType<ResultsScreenUI>(FindObjectsInactive.Include);
            if (resultsUI != null)
            {
                resultsUI.ShowResults(data.position, finalTime);
            }
            else
            {
                var newRs = gameObject.AddComponent<ResultsScreenUI>();
                newRs.ShowResults(data.position, finalTime);
            }
        }

        private bool IsPlayerFinished()
        {
            // Check vehicle racer
            var playerVehicle = GetPlayerVehicle();
            if (playerVehicle != null && racerDataMap.ContainsKey(playerVehicle))
            {
                return racerDataMap[playerVehicle].finished;
            }

            // Check ship racer
            var playerShip = racers.FirstOrDefault(s => s != null && (s.GetComponent<ShipInput>() != null || s.CompareTag("Player")));
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
            var playerVehicle = GetPlayerVehicle();
            if (playerVehicle != null && racerDataMap.ContainsKey(playerVehicle))
            {
                return racerDataMap[playerVehicle].position;
            }

            var playerShip = racers.FirstOrDefault(s => s != null && (s.GetComponent<ShipInput>() != null || s.CompareTag("Player")));
            if (playerShip != null && racerDataMap.ContainsKey(playerShip))
            {
                return racerDataMap[playerShip].position;
            }

            return 1;
        }


        public void ResetRace()
        {
            currentState = RaceState.PreRace;
            raceTime = 0f;
            countdownTimer = countdownTime;
            postFinishTimer = 0f;
            hasTriggeredFinish = false;
            racerDataMap.Clear();

            // Re-initialize for all registered racers
            foreach (ShipController ship in racers)
            {
                if (ship != null) racerDataMap[ship] = new RacerData();
            }
            foreach (VehicleController vehicle in vehicleRacers)
            {
                if (vehicle != null) racerDataMap[vehicle] = new RacerData();
            }
        }

        public float GetCountdownTime()
        {
            return currentState == RaceState.Countdown ? countdownTimer : 0f;
        }
    }
}
