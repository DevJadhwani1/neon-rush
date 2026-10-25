// Module: Editor   Owner: Member 3   Purpose: One-click setup for all scenes, data assets, and build settings

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace NeonRush
{
    public class NeonRushSetup : EditorWindow
    {
        [MenuItem("Tools/Neon Rush/Setup Everything")]
        public static void SetupEverything()
        {
            if (EditorUtility.DisplayDialog("Neon Rush Setup", 
                "This will create MainMenu scene, all data assets, and set Build Settings. Continue?", 
                "Yes", "No"))
            {
                CreateMainMenuScene();
                CreateAllDataAssets();
                SetBuildSettings();
                EditorUtility.DisplayDialog("Setup Complete", "Neon Rush is ready! Open MainMenu scene and press Play.", "OK");
            }
        }

        static void CreateMainMenuScene()
        {
            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Add MenuRoot with MainMenuUI
            GameObject menuRoot = new GameObject("MenuRoot");
            menuRoot.AddComponent<MainMenuUI>();
            
            // Add GameBootstrap to ensure managers exist
            GameObject bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();
            
            // Add EventSystem for UI
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Save scene
            string scenePath = "Assets/Scenes/MainMenu.unity";
            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }
            
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log("[NeonRushSetup] Created MainMenu scene at " + scenePath);
        }

        static void CreateAllDataAssets()
        {
            // Ensure Data folders exist
            if (!Directory.Exists("Assets/Data"))
            {
                Directory.CreateDirectory("Assets/Data");
            }
            if (!Directory.Exists("Assets/Data/Vehicles"))
            {
                Directory.CreateDirectory("Assets/Data/Vehicles");
            }
            if (!Directory.Exists("Assets/Data/Tracks"))
            {
                Directory.CreateDirectory("Assets/Data/Tracks");
            }
            
            // Create vehicle data assets
            CreateVehicleData("VOLT", "Balanced Interceptor", "cyan", 0, 40f, 48f, 115f);
            CreateVehicleData("BRICK", "Heavy Tank", "magenta", 1500, 32f, 40f, 95f);
            CreateVehicleData("BLADE", "Speed Demon", "lime", 3000, 50f, 55f, 130f);
            
            // Create track theme assets
            CreateTrackTheme("NEON_DOWNTOWN", "Neon Downtown", 0, "cyan", "magenta");
            CreateTrackTheme("FOUNDRY_INFERNO", "Foundry Inferno", 2000, "orange", "red");
            CreateTrackTheme("SKY_HIGHWAY", "Sky Highway", 4000, "purple", "white");
            
            AssetDatabase.Refresh();
            Debug.Log("[NeonRushSetup] Created all data assets.");
        }

        static void CreateVehicleData(string id, string displayName, string colorName, int price, float maxSpeed, float accel, float turn)
        {
            string path = $"Assets/Data/Vehicles/{id}.asset";
            if (File.Exists(path)) return; // Don't overwrite
            
            VehicleData data = ScriptableObject.CreateInstance<VehicleData>();
            data.id = id;
            data.displayName = displayName;
            data.description = $"The {displayName} - {colorName} racer";
            data.price = price;
            data.maxSpeed = maxSpeed;
            data.acceleration = accel;
            data.turnRate = turn;
            data.boostPower = 1.0f;
            data.boostDuration = 3.5f;
            data.maxArmor = 100;
            
            // Set color based on name
            if (colorName == "cyan") data.bodyColor = new Color(0f, 0.9f, 1f);
            else if (colorName == "magenta") data.bodyColor = new Color(1f, 0.1f, 0.85f);
            else if (colorName == "lime") data.bodyColor = new Color(0.5f, 1f, 0f);
            
            data.glowColor = data.bodyColor;
            
            // Set shape preset
            if (id == "VOLT") data.shapePreset = VehicleShape.Interceptor;
            else if (id == "BRICK") data.shapePreset = VehicleShape.Brick;
            else if (id == "BLADE") data.shapePreset = VehicleShape.Blade;
            
            AssetDatabase.CreateAsset(data, path);
        }

        static void CreateTrackTheme(string id, string displayName, int price, string primary, string secondary)
        {
            string path = $"Assets/Data/Tracks/{id}.asset";
            if (File.Exists(path)) return;
            
            TrackTheme theme = ScriptableObject.CreateInstance<TrackTheme>();
            theme.id = id;
            theme.displayName = displayName;
            theme.unlockPrice = price;
            theme.seed = id.GetHashCode();
            
            // Set colors
            if (primary == "cyan") theme.primaryNeonColor = new Color(0f, 0.9f, 1f);
            else if (primary == "orange") theme.primaryNeonColor = new Color(1f, 0.6f, 0f);
            else if (primary == "purple") theme.primaryNeonColor = new Color(0.8f, 0.2f, 1f);
            
            if (secondary == "magenta") theme.secondaryNeonColor = new Color(1f, 0.1f, 0.85f);
            else if (secondary == "red") theme.secondaryNeonColor = new Color(1f, 0.2f, 0f);
            else if (secondary == "white") theme.secondaryNeonColor = new Color(1f, 1f, 1f);
            
            theme.fogColor = theme.primaryNeonColor * 0.3f;
            theme.hazardDensityMultiplier = (id == "FOUNDRY_INFERNO") ? 1.5f : 1.0f;
            theme.lengthMultiplier = (id == "SKY_HIGHWAY") ? 1.2f : 1.0f;
            
            AssetDatabase.CreateAsset(theme, path);
        }

        static void SetBuildSettings()
        {
            // Get current build settings
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[2];
            scenes[0] = new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true);
            
            // Try to find the race scene
            string racePath = "Assets/,.unity";
            if (File.Exists(racePath))
            {
                scenes[1] = new EditorBuildSettingsScene(racePath, true);
            }
            else
            {
                racePath = "Assets/Scenes/GravShiftNeonCircuit.unity";
                scenes[1] = new EditorBuildSettingsScene(racePath, true);
            }
            
            EditorBuildSettings.scenes = scenes;
            
            // Set product name and company
            PlayerSettings.companyName = "Gridlock Studios";
            PlayerSettings.productName = "Neon Rush";
            
            Debug.Log("[NeonRushSetup] Build settings configured.");
        }
    }
}
#endif
