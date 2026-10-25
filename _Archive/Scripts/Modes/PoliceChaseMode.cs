// Module: Modes   Owner: Member 1   Purpose: Survive 5 minutes being chased by police

using UnityEngine;

namespace NeonRush
{
    public class PoliceChaseMode : GameModeBase
    {
        private float modeTime = 0f;
        private float targetTime = 300f; // 5 minutes
        private bool modeActive = false;
        private int policeEvaded = 0;
        
        public override string GetModeName() => "POLICE CHASE";
        public override string GetModeDescription() => "Survive 5 minutes!";

        public override void Initialize()
        {
            // Spawn multiple police vehicles
            for (int i = 0; i < 3; i++)
            {
                GameObject police = new GameObject("PoliceVehicle_" + i);
                police.transform.position = new Vector3(Random.Range(-10f, 10f), 1.5f, 50f + i * 20f);
                PoliceVehicle pv = police.AddComponent<PoliceVehicle>();
                pv.maxChaseSpeed = 35f + (i * 3f);
            }
        }

        public override void StartMode()
        {
            modeActive = true;
            modeTime = 0f;
        }

        public override void UpdateMode()
        {
            if (!modeActive) return;
            
            modeTime += Time.deltaTime;
            
            if (modeTime >= targetTime)
            {
                modeActive = false;
            }
        }

        public override bool IsComplete()
        {
            return modeTime >= targetTime;
        }

        public override void EndMode()
        {
            modeActive = false;
        }

        public override int GetFinalScore()
        {
            return Mathf.FloorToInt(modeTime * 10f) + (policeEvaded * 500);
        }

        public override int GetCreditsEarned()
        {
            return 800 + (policeEvaded * 100);
        }
    }
}
