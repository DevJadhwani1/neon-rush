// Module: Modes   Owner: Member 2   Purpose: Time trial mode - beat the clock

using UnityEngine;

namespace NeonRush
{
    public class TimeTrialMode : GameModeBase
    {
        private float targetTime = 180f; // 3 minutes
        private float currentTime = 0f;
        private bool modeActive = false;
        private RaceManager raceManager;
        
        public override string GetModeName() => "TIME TRIAL";
        public override string GetModeDescription() => "Complete 3 laps as fast as possible!";

        public override void Initialize()
        {
            raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager != null)
            {
                raceManager.totalLaps = 3;
            }
        }

        public override void StartMode()
        {
            modeActive = true;
            currentTime = 0f;
            
            if (raceManager != null)
            {
                raceManager.StartRace();
            }
        }

        public override void UpdateMode()
        {
            if (!modeActive) return;
            
            currentTime += Time.deltaTime;
        }

        public override bool IsComplete()
        {
            if (raceManager == null) return false;
            return !raceManager.raceActive;
        }

        public override void EndMode()
        {
            modeActive = false;
        }

        public override int GetFinalScore()
        {
            return Mathf.FloorToInt(currentTime * 100f);
        }

        public override int GetCreditsEarned()
        {
            // Faster time = more credits
            float bonus = Mathf.Max(0f, targetTime - currentTime);
            return 500 + Mathf.FloorToInt(bonus * 10f);
        }
    }
}
