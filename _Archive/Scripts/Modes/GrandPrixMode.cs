// Module: Modes   Owner: Member 2   Purpose: Grand Prix - race against 3 AI rivals

using UnityEngine;

namespace NeonRush
{
    public class GrandPrixMode : GameModeBase
    {
        private RaceManager raceManager;
        private bool modeActive = false;
        private int finalPosition = 1;
        
        public override string GetModeName() => "GRAND PRIX";
        public override string GetModeDescription() => "Race against 3 AI opponents!";

        public override void Initialize()
        {
            raceManager = FindAnyObjectByType<RaceManager>();
            if (raceManager != null)
            {
                raceManager.totalLaps = 3;
                raceManager.totalRacers = 4;
            }
            
            // Spawn 3 AI racers
            SpawnAIRacers();
        }

        void SpawnAIRacers()
        {
            Vector3[] spawnPositions = {
                new Vector3(-5f, 1.5f, -5f),
                new Vector3(0f, 1.5f, -7f),
                new Vector3(5f, 1.5f, -5f)
            };
            
            for (int i = 0; i < 3; i++)
            {
                GameObject aiRacer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                aiRacer.name = "AIRacer_" + (i + 1);
                aiRacer.transform.position = spawnPositions[i];
                aiRacer.transform.localScale = new Vector3(2.4f, 0.9f, 4.4f);
                
                Renderer rend = aiRacer.GetComponent<Renderer>();
                if (rend != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(Random.value, Random.value, Random.value);
                    rend.material = mat;
                }
                
                AIRacer aiScript = aiRacer.AddComponent<AIRacer>();
                // Spawn with varying speeds - player should finish 2nd-3rd normally
                aiScript.baseSpeed = 32f + (i * 1.5f); // 32, 33.5, 35 km/h
                
                if (raceManager != null)
                {
                    raceManager.RegisterRacer(aiRacer);
                }
            }
        }

        public override void StartMode()
        {
            modeActive = true;
            
            if (raceManager != null)
            {
                raceManager.StartRace();
            }
        }

        public override void UpdateMode()
        {
            if (!modeActive) return;
            
            if (raceManager != null)
            {
                finalPosition = raceManager.GetPlayerPosition();
            }
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
            return (5 - finalPosition) * 1000; // 1st: 4000, 2nd: 3000, etc.
        }

        public override int GetCreditsEarned()
        {
            int[] positionRewards = { 1000, 600, 300, 100 };
            return positionRewards[Mathf.Clamp(finalPosition - 1, 0, 3)];
        }
    }
}
