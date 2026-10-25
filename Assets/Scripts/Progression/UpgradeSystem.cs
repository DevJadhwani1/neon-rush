// Module: Progression   Owner: Member 1   Purpose: Vehicle upgrade management (4 stats × 3 levels)

using UnityEngine;

namespace NeonRush
{
    public enum UpgradeType
    {
        Engine,    // +15% maxSpeed per level
        Handling,  // +10% turnSpeed per level
        Boost,     // +0.5s antiGravityDuration per level
        Armor      // +20 maxHealth per level
    }

    public class UpgradeSystem : MonoBehaviour
    {
        public const int MAX_UPGRADE_LEVEL = 3;
        
        // Upgrade costs per level (level 1, 2, 3)
        public static readonly int[] ENGINE_COSTS = { 500, 1000, 2000 };
        public static readonly int[] HANDLING_COSTS = { 400, 800, 1600 };
        public static readonly int[] BOOST_COSTS = { 600, 1200, 2400 };
        public static readonly int[] ARMOR_COSTS = { 300, 600, 1200 };

        public static int GetUpgradeCost(UpgradeType type, int currentLevel)
        {
            if (currentLevel >= MAX_UPGRADE_LEVEL) return 0;
            
            int[] costs = null;
            switch (type)
            {
                case UpgradeType.Engine: costs = ENGINE_COSTS; break;
                case UpgradeType.Handling: costs = HANDLING_COSTS; break;
                case UpgradeType.Boost: costs = BOOST_COSTS; break;
                case UpgradeType.Armor: costs = ARMOR_COSTS; break;
            }
            
            return (costs != null && currentLevel < costs.Length) ? costs[currentLevel] : 0;
        }

        public static bool CanAffordUpgrade(UpgradeType type, int currentLevel, int credits)
        {
            int cost = GetUpgradeCost(type, currentLevel);
            return cost > 0 && credits >= cost;
        }

        public static bool PurchaseUpgrade(string vehicleId, UpgradeType type)
        {
            SaveData data = SaveSystem.Load();
            int[] levels = SaveSystem.GetUpgradeLevels(vehicleId);
            
            int currentLevel = levels[(int)type];
            if (currentLevel >= MAX_UPGRADE_LEVEL)
            {
                Debug.Log("[UpgradeSystem] Already max level for " + type);
                return false;
            }
            
            int cost = GetUpgradeCost(type, currentLevel);
            if (data.credits < cost)
            {
                Debug.Log("[UpgradeSystem] Not enough credits. Need " + cost + ", have " + data.credits);
                return false;
            }
            
            // Deduct credits and increment upgrade level
            data.credits -= cost;
            
            SaveSystem.SetUpgradeLevel(vehicleId, (int)type, currentLevel + 1);
            data = SaveSystem.Load(); // Reload to get updated data
            
            SaveSystem.Save(data);
            Debug.Log("[UpgradeSystem] Purchased " + type + " level " + (currentLevel + 1) + " for " + cost + " credits");
            return true;
        }

        public static float GetSpeedMultiplier(int engineLevel)
        {
            return 1f + (engineLevel * 0.15f); // 1.0, 1.15, 1.30, 1.45
        }

        public static float GetHandlingMultiplier(int handlingLevel)
        {
            return 1f + (handlingLevel * 0.10f); // 1.0, 1.10, 1.20, 1.30
        }

        public static float GetBoostDurationBonus(int boostLevel)
        {
            return boostLevel * 0.5f; // 0, 0.5, 1.0, 1.5
        }

        public static int GetArmorBonus(int armorLevel)
        {
            return armorLevel * 20; // 0, 20, 40, 60
        }
    }
}
