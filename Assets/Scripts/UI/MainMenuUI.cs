// Module: UI   Owner: Member 3   Purpose: Main menu with RACE, GARAGE, SETTINGS, QUIT

using UnityEngine;

namespace NeonRush
{
    public class MainMenuUI : MonoBehaviour
    {
        private enum MenuState { Main, TrackSelect, Settings }
        private MenuState currentState = MenuState.Main;
        private int selectedTrackIndex = 0;
        private string[] trackIds = { "NEON_DOWNTOWN", "CYBER_SPEEDWAY", "GRID_CIRCUIT" };
        private string[] trackNames = { "NEON DOWNTOWN", "CYBER SPEEDWAY", "GRID CIRCUIT" };
        private int[] trackPrices = { 0, 1000, 2000 }; // First track free
        
        void Start()
        {
            Debug.Log("[NeonRush] MainMenuUI loaded");
            Time.timeScale = 1f;
            EnsureEventSystem();
        }

        void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject evtSystem = new GameObject("EventSystem");
                evtSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                evtSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[NeonRush] Created EventSystem for MainMenu");
            }
        }

        void OnGUI()
        {
            // Main menu enabled
            
            switch (currentState)
            {
                case MenuState.Main:
                    DrawMainMenu();
                    break;
                case MenuState.TrackSelect:
                    DrawTrackSelect();
                    break;
                case MenuState.Settings:
                    DrawSettings();
                    break;
            }
        }

        void DrawMainMenu()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 72;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperCenter;
            titleStyle.normal.textColor = new Color(0f, 0.9f, 1f);
            
            GUI.Label(new Rect(0, 80, Screen.width, 90), "NEON RUSH", titleStyle);
            
            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label);
            subtitleStyle.fontSize = 28;
            subtitleStyle.alignment = TextAnchor.UpperCenter;
            subtitleStyle.normal.textColor = new Color(1f, 0.1f, 0.85f);
            
            GUI.Label(new Rect(0, 180, Screen.width, 40), "GRID RUNNERS", subtitleStyle);
            
            float centerX = Screen.width * 0.5f;
            float startY = Screen.height * 0.5f;
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 32;
            buttonStyle.fontStyle = FontStyle.Bold;
            
            if (GUI.Button(new Rect(centerX - 180, startY, 360, 60), "RACE", buttonStyle))
            {
                currentState = MenuState.TrackSelect;
            }
            
            // Garage button removed - will rebuild in new UI system
            // if (GUI.Button(new Rect(centerX - 180, startY + 80, 360, 60), "GARAGE", buttonStyle))
            {
                SceneLoader.LoadMainMenu(); // TODO: LoadGarage when scene exists
            }
            
            if (GUI.Button(new Rect(centerX - 180, startY + 160, 360, 60), "SETTINGS", buttonStyle))
            {
                currentState = MenuState.Settings;
            }
            
            if (GUI.Button(new Rect(centerX - 180, startY + 240, 360, 60), "QUIT", buttonStyle))
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            
            // Credits display
            SaveData data = SaveSystem.Load();
            GUIStyle creditsStyle = new GUIStyle(GUI.skin.label);
            creditsStyle.fontSize = 20;
            creditsStyle.alignment = TextAnchor.UpperRight;
            creditsStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(Screen.width - 250, 20, 230, 30), "CREDITS: " + data.credits, creditsStyle);
        }

        void DrawTrackSelect()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 48;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperCenter;
            titleStyle.normal.textColor = new Color(0f, 0.9f, 1f);
            
            GUI.Label(new Rect(0, 40, Screen.width, 60), "SELECT TRACK", titleStyle);
            
            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.4f;
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 28;
            
            // Track selection arrows
            if (GUI.Button(new Rect(centerX - 350, centerY, 80, 80), "<", buttonStyle))
            {
                selectedTrackIndex = (selectedTrackIndex - 1 + trackNames.Length) % trackNames.Length;
            }
            
            // Track display card
            GUIStyle cardStyle = new GUIStyle(GUI.skin.box);
            cardStyle.fontSize = 32;
            cardStyle.fontStyle = FontStyle.Bold;
            cardStyle.alignment = TextAnchor.MiddleCenter;
            cardStyle.normal.textColor = Color.white;
            
            GUI.Box(new Rect(centerX - 250, centerY, 500, 80), trackNames[selectedTrackIndex], cardStyle);
            
            SaveData data = SaveSystem.Load();
            bool isUnlocked = trackPrices[selectedTrackIndex] == 0 || data.unlockedTracks.Contains(trackIds[selectedTrackIndex]);
            
            if (!isUnlocked)
            {
                GUIStyle lockStyle = new GUIStyle(GUI.skin.label);
                lockStyle.fontSize = 24;
                lockStyle.alignment = TextAnchor.UpperCenter;
                lockStyle.normal.textColor = Color.red;
                GUI.Label(new Rect(centerX - 150, centerY + 100, 300, 40), "🔒 LOCKED - " + trackPrices[selectedTrackIndex] + " CR", lockStyle);
            }
            
            if (GUI.Button(new Rect(centerX + 270, centerY, 80, 80), ">", buttonStyle))
            {
                selectedTrackIndex = (selectedTrackIndex + 1) % trackNames.Length;
            }
            
            // Start race or purchase
            buttonStyle.fontSize = 32;
            if (isUnlocked)
            {
                if (GUI.Button(new Rect(centerX - 150, centerY + 180, 300, 60), "START RACE", buttonStyle))
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.selectedTrackId = trackIds[selectedTrackIndex];
                        GameManager.Instance.selectedMode = "GRAND_PRIX";
                    }
                    Debug.Log("[MainMenuUI] Starting race on " + trackNames[selectedTrackIndex]);
                    SceneLoader.LoadRace();
                }
            }
            else
            {
                bool canBuy = data.credits >= trackPrices[selectedTrackIndex];
                GUI.enabled = canBuy;
                if (GUI.Button(new Rect(centerX - 150, centerY + 180, 300, 60), "BUY - " + trackPrices[selectedTrackIndex] + " CR", buttonStyle))
                {
                    data.credits -= trackPrices[selectedTrackIndex];
                    data.unlockedTracks.Add(trackIds[selectedTrackIndex]);
                    SaveSystem.Save(data);
                }
                GUI.enabled = true;
            }
            
            // Back button
            buttonStyle.fontSize = 24;
            if (GUI.Button(new Rect(50, Screen.height - 100, 200, 50), "BACK", buttonStyle))
            {
                currentState = MenuState.Main;
            }
        }

        void DrawSettings()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 48;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperCenter;
            titleStyle.normal.textColor = new Color(0f, 0.9f, 1f);
            
            GUI.Label(new Rect(0, 40, Screen.width, 60), "SETTINGS", titleStyle);
            
            SaveData data = SaveSystem.Load();
            float centerX = Screen.width * 0.5f;
            float startY = 200;
            
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 24;
            labelStyle.normal.textColor = Color.white;
            
            // Master Volume
            GUI.Label(new Rect(centerX - 300, startY, 200, 30), "Master Volume", labelStyle);
            data.masterVolume = GUI.HorizontalSlider(new Rect(centerX - 80, startY + 5, 300, 30), data.masterVolume, 0f, 1f);
            
            // Music Volume
            GUI.Label(new Rect(centerX - 300, startY + 60, 200, 30), "Music Volume", labelStyle);
            data.musicVolume = GUI.HorizontalSlider(new Rect(centerX - 80, startY + 65, 300, 30), data.musicVolume, 0f, 1f);
            
            // SFX Volume
            GUI.Label(new Rect(centerX - 300, startY + 120, 200, 30), "SFX Volume", labelStyle);
            data.sfxVolume = GUI.HorizontalSlider(new Rect(centerX - 80, startY + 125, 300, 30), data.sfxVolume, 0f, 1f);
            
            // Camera Shake
            GUI.Label(new Rect(centerX - 300, startY + 180, 200, 30), "Camera Shake", labelStyle);
            data.cameraShake = GUI.Toggle(new Rect(centerX - 80, startY + 180, 100, 30), data.cameraShake, data.cameraShake ? "ON" : "OFF");
            
            SaveSystem.Save(data);
            
            // Back button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 24;
            if (GUI.Button(new Rect(50, Screen.height - 100, 200, 50), "BACK", buttonStyle))
            {
                currentState = MenuState.Main;
            }
        }
    }
}
