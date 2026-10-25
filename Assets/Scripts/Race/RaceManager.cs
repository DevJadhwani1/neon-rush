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
        [SerializeField] private List<ShipController> racers = new List<ShipController>();

        [Header("Checkpoints")]
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

        // Race state
        private Dictionary<ShipController, RacerData> racerDataMap = new Dictionary<ShipController, RacerData>();
        private RaceState currentState = RaceState.PreRace;
        private float raceTime = 0f;
        private float countdownTimer = 0f;

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
            // Find all checkpoints if not set
            if (checkpoints.Count == 0)
            {
                checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).ToList();
                checkpoints = checkpoints.OrderBy(c => c.Index).ToList();
            }

            // Find all ships if not set
            if (racers.Count == 0)
            {
                racers = FindObjectsByType<ShipController>(FindObjectsSortMode.None).ToList();
            }

            // Initialize racer data
            foreach (ShipController ship in racers)
            {
                racerDataMap[ship] = new RacerData();
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

        private void StartRace()
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
        }

        private void UpdateRace()
        {
            raceTime += Time.deltaTime;

            // Update positions
            UpdateRacePositions();

            // Check if all racers finished
            bool allFinished = racerDataMap.Values.All(data => data.finished);
            if (allFinished)
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

        public void OnShipPassCheckpoint(ShipController ship, Checkpoint checkpoint)
        {
            if (currentState != RaceState.Racing) return;
            if (!racerDataMap.ContainsKey(ship)) return;

            RacerData data = racerDataMap[ship];
            if (data.finished) return;

            // Check if this is the correct next checkpoint
            if (checkpoint.Index == data.nextCheckpointIndex)
            {
                data.nextCheckpointIndex++;

                // Check if finished lap
                if (checkpoint.IsFinishLine)
                {
                    data.currentLap++;
                    data.nextCheckpointIndex = 0;

                    Debug.Log($"{ship.name} completed lap {data.currentLap}/{totalLaps}");

                    // Check if finished race
                    if (data.currentLap >= totalLaps)
                    {
                        data.finished = true;
                        data.raceTime = raceTime;
                        Debug.Log($"{ship.name} finished the race! Time: {data.raceTime:F2}s");

                        // Notify UI
                        RaceHUD hud = FindAnyObjectByType<RaceHUD>();
                        if (hud != null && ship.GetComponent<ShipInput>() != null)
                        {
                            hud.ShowNotification($"Race Complete! Time: {data.raceTime:F2}s", 5f);
                        }
                    }
                }
            }
        }

        private void EndRace()
        {
            currentState = RaceState.Finished;
            Debug.Log("Race finished! All racers completed.");
        }

        public int GetPlayerPosition()
        {
            ShipController playerShip = racers.FirstOrDefault(s => s.GetComponent<ShipInput>() != null);
            if (playerShip != null && racerDataMap.ContainsKey(playerShip))
            {
                return racerDataMap[playerShip].position;
            }
            return 1;
        }

        public int GetPlayerLap()
        {
            ShipController playerShip = racers.FirstOrDefault(s => s.GetComponent<ShipInput>() != null);
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
