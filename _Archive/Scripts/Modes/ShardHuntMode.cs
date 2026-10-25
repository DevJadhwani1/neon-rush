// Module: Modes   Owner: Member 1   Purpose: Collect 50 shards in under 5 minutes

using UnityEngine;

namespace NeonRush
{
    public class ShardHuntMode : GameModeBase
    {
        private int shardsCollected = 0;
        private int targetShards = 50;
        private float timeLimit = 300f; // 5 minutes
        private float modeTime = 0f;
        private bool modeActive = false;
        
        public override string GetModeName() => "SHARD HUNT";
        public override string GetModeDescription() => "Collect 50 shards in 5 minutes!";

        public override void Initialize()
        {
            // Spawn extra shards around the track
            for (int i = 0; i < targetShards; i++)
            {
                float z = Random.Range(0f, 500f);
                float x = Random.Range(-15f, 15f);
                float y = 2f;
                
                GameObject shard = new GameObject("Shard_" + i);
                shard.transform.position = new Vector3(x, y, z);
                
                QuantumShard shardScript = shard.AddComponent<QuantumShard>();
            }
            
            Debug.Log($"[ShardHuntMode] Spawned {targetShards} shards");
        }

        public override void StartMode()
        {
            modeActive = true;
            modeTime = 0f;
            shardsCollected = 0;
        }

        public override void UpdateMode()
        {
            if (!modeActive) return;
            
            modeTime += Time.deltaTime;
            
            // Check for shard collection via GameManager score
            if (GameManager.Instance != null)
            {
                // Each shard is 50 points
                shardsCollected = GameManager.Instance.score / 50;
            }
            
            // Check win condition
            if (shardsCollected >= targetShards)
            {
                modeActive = false;
            }
            
            // Check time limit
            if (modeTime >= timeLimit)
            {
                modeActive = false;
            }
        }

        public override bool IsComplete()
        {
            return !modeActive;
        }

        public override void EndMode()
        {
            modeActive = false;
        }

        public override int GetFinalScore()
        {
            return shardsCollected * 100 + Mathf.FloorToInt((timeLimit - modeTime) * 10f);
        }

        public override int GetCreditsEarned()
        {
            int baseReward = shardsCollected * 20;
            int timeBonus = (modeTime < timeLimit) ? Mathf.FloorToInt((timeLimit - modeTime) * 5f) : 0;
            return baseReward + timeBonus;
        }
    }
}
