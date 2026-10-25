// Module: Combat   Owner: Member 1   Purpose: Style points and combo multiplier system

using UnityEngine;

namespace NeonRush
{
    public enum StyleAction
    {
        Drift,
        NearMiss,
        AirTime,
        Overtake,
        TrickLand,
        EnemyDestroy,
        ShardCollect
    }

    public class StyleComboSystem : MonoBehaviour
    {
        [Header("Combo Settings")]
        public int comboMultiplier = 1;
        public float comboTimer = 0f;
        public float comboTimeout = 3f; // Reset after 3s without action
        public int totalStylePoints = 0;
        
        private int consecutiveActions = 0;
        
        void Update()
        {
            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        public void RegisterStyleAction(StyleAction action)
        {
            int basePoints = GetBasePoints(action);
            int earnedPoints = basePoints * comboMultiplier;
            
            totalStylePoints += earnedPoints;
            consecutiveActions++;
            comboTimer = comboTimeout;
            
            // Increase multiplier every 3 actions
            if (consecutiveActions % 3 == 0)
            {
                comboMultiplier++;
                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ShowNotification($"COMBO x{comboMultiplier}!", 1.5f);
                }
            }
            
            // Award credits to GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(earnedPoints);
            }
            
            Debug.Log($"[StyleCombo] {action} +{earnedPoints} (x{comboMultiplier})");
        }

        int GetBasePoints(StyleAction action)
        {
            switch (action)
            {
                case StyleAction.Drift: return 25;
                case StyleAction.NearMiss: return 50;
                case StyleAction.AirTime: return 75;
                case StyleAction.Overtake: return 100;
                case StyleAction.TrickLand: return 150;
                case StyleAction.EnemyDestroy: return 200;
                case StyleAction.ShardCollect: return 50;
                default: return 10;
            }
        }

        void ResetCombo()
        {
            if (comboMultiplier > 1)
            {
                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ShowNotification("COMBO ENDED", 1f);
                }
            }
            
            comboMultiplier = 1;
            consecutiveActions = 0;
            comboTimer = 0f;
        }

        public int GetComboMultiplier()
        {
            return comboMultiplier;
        }

        public float GetComboTimeRemaining()
        {
            return comboTimer;
        }
    }
}
