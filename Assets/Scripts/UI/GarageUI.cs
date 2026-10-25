// Module: UI   Owner: Member 1   Purpose: Vehicle selection and upgrade interface

using UnityEngine;

namespace NeonRush
{
    public class GarageUI : MonoBehaviour
    {
        private int selectedVehicleIndex = 0;
        private string[] vehicleIds = { "volt", "brick", "blade" };
        private VehicleData[] vehicleData = new VehicleData[3];
        
        private SaveData saveData;
        private Rect screenRect;
        
        void Start()
        {
            saveData = SaveSystem.Load();
            LoadVehicleData();
            
            // Pre-select vehicle from GameManager
            if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.selectedVehicleId))
            {
                for (int i = 0; i < vehicleIds.Length; i++)
                {
                    if (vehicleIds[i] == GameManager.Instance.selectedVehicleId)
                    {
                        selectedVehicleIndex = i;
                        break;
                    }
                }
            }
        }

        void LoadVehicleData()
        {
            for (int i = 0; i < vehicleIds.Length; i++)
            {
                VehicleData data = Resources.Load<VehicleData>("VehicleData/" + vehicleIds[i]);
                if (data == null)
                {
                    Debug.LogWarning("[GarageUI] Could not load VehicleData/" + vehicleIds[i]);
                }
                vehicleData[i] = data;
            }
        }

        void OnGUI()
        {
            screenRect = new Rect(0, 0, Screen.width, Screen.height);
            
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 48;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperCenter;
            titleStyle.normal.textColor = new Color(0f, 0.9f, 1f); // Cyan
            
            GUI.Label(new Rect(0, 40, Screen.width, 60), "GARAGE", titleStyle);
            
            // Credits display
            GUIStyle creditsStyle = new GUIStyle(GUI.skin.label);
            creditsStyle.fontSize = 24;
            creditsStyle.alignment = TextAnchor.UpperRight;
            creditsStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(Screen.width - 250, 20, 230, 30), "CREDITS: " + saveData.credits, creditsStyle);
            
            DrawVehicleSelection();
            DrawUpgradePanel();
            DrawBottomButtons();
        }

        void DrawVehicleSelection()
        {
            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.35f;
            
            GUIStyle vehicleStyle = new GUIStyle(GUI.skin.box);
            vehicleStyle.fontSize = 28;
            vehicleStyle.fontStyle = FontStyle.Bold;
            vehicleStyle.alignment = TextAnchor.MiddleCenter;
            
            // Left arrow
            if (GUI.Button(new Rect(centerX - 300, centerY - 40, 60, 80), "<"))
            {
                selectedVehicleIndex = (selectedVehicleIndex - 1 + vehicleIds.Length) % vehicleIds.Length;
            }
            
            // Vehicle display
            VehicleData current = vehicleData[selectedVehicleIndex];
            if (current != null)
            {
                vehicleStyle.normal.textColor = new Color(0f, 0.9f, 1f);
                GUI.Box(new Rect(centerX - 200, centerY - 80, 400, 160), current.displayName, vehicleStyle);
                
                GUIStyle statsStyle = new GUIStyle(GUI.skin.label);
                statsStyle.fontSize = 16;
                statsStyle.alignment = TextAnchor.UpperLeft;
                statsStyle.normal.textColor = Color.white;
                
                string stats = $"SPEED: {current.maxSpeed}\n" +
                               $"ACCEL: {current.acceleration}\n" +
                               $"TURN: {current.turnRate}\n" +
                               $"BOOST: {current.boostPower}";
                
                GUI.Label(new Rect(centerX - 180, centerY - 40, 360, 100), stats, statsStyle);
                
                // Lock indicator if not unlocked
                if (current.price > 0 && !saveData.unlockedVehicles.Contains(current.id))
                {
                    GUIStyle lockStyle = new GUIStyle(GUI.skin.label);
                    lockStyle.fontSize = 20;
                    lockStyle.alignment = TextAnchor.UpperCenter;
                    lockStyle.normal.textColor = Color.red;
                    GUI.Label(new Rect(centerX - 100, centerY + 50, 200, 30), "🔒 LOCKED - " + current.price + " CR", lockStyle);
                }
            }
            
            // Right arrow
            if (GUI.Button(new Rect(centerX + 240, centerY - 40, 60, 80), ">"))
            {
                selectedVehicleIndex = (selectedVehicleIndex + 1) % vehicleIds.Length;
            }
        }

        void DrawUpgradePanel()
        {
            float centerX = Screen.width * 0.5f;
            float startY = Screen.height * 0.6f;
            
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 18;
            labelStyle.normal.textColor = Color.white;
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
            
            string vehicleId = vehicleIds[selectedVehicleIndex];
            int[] upgradeLevels = SaveSystem.GetUpgradeLevels(vehicleId);
            
            UpgradeType[] types = { UpgradeType.Engine, UpgradeType.Handling, UpgradeType.Boost, UpgradeType.Armor };
            string[] names = { "ENGINE", "HANDLING", "BOOST", "ARMOR" };
            
            for (int i = 0; i < types.Length; i++)
            {
                float y = startY + (i * 40);
                int level = upgradeLevels[i];
                int cost = UpgradeSystem.GetUpgradeCost(types[i], level);
                
                string levelBar = "[" + new string('■', level) + new string('□', UpgradeSystem.MAX_UPGRADE_LEVEL - level) + "]";
                GUI.Label(new Rect(centerX - 300, y, 200, 30), names[i] + " " + levelBar, labelStyle);
                
                if (level < UpgradeSystem.MAX_UPGRADE_LEVEL)
                {
                    bool canAfford = saveData.credits >= cost;
                    GUI.enabled = canAfford;
                    
                    if (GUI.Button(new Rect(centerX - 90, y, 200, 30), "UPGRADE - " + cost + " CR", buttonStyle))
                    {
                        if (UpgradeSystem.PurchaseUpgrade(vehicleId, types[i]))
                        {
                            saveData = SaveSystem.Load(); // Reload after purchase
                        }
                    }
                    
                    GUI.enabled = true;
                }
                else
                {
                    GUI.Label(new Rect(centerX - 90, y, 200, 30), "MAX LEVEL", labelStyle);
                }
            }
        }

        void DrawBottomButtons()
        {
            float centerX = Screen.width * 0.5f;
            float bottomY = Screen.height - 100;
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 24;
            buttonStyle.fontStyle = FontStyle.Bold;
            
            // Select button
            VehicleData current = vehicleData[selectedVehicleIndex];
            bool isUnlocked = (current.price == 0) || saveData.unlockedVehicles.Contains(current.id);
            
            if (isUnlocked)
            {
                if (GUI.Button(new Rect(centerX - 120, bottomY, 240, 50), "SELECT VEHICLE", buttonStyle))
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.selectedVehicleId = vehicleIds[selectedVehicleIndex];
                    }
                    // Return to main menu
                    SceneLoader.LoadMainMenu();
                }
            }
            else
            {
                // Purchase button
                bool canBuy = saveData.credits >= current.price;
                GUI.enabled = canBuy;
                
                if (GUI.Button(new Rect(centerX - 120, bottomY, 240, 50), "BUY - " + current.price + " CR", buttonStyle))
                {
                    saveData.credits -= current.price;
                    saveData.unlockedVehicles.Add(current.id);
                    SaveSystem.Save(saveData);
                    saveData = SaveSystem.Load();
                }
                
                GUI.enabled = true;
            }
            
            // Back button
            if (GUI.Button(new Rect(centerX - 360, bottomY, 200, 50), "BACK", buttonStyle))
            {
                SceneLoader.LoadMainMenu();
            }
        }
    }
}
