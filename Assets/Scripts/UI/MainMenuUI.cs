// Module: UI   Owner: Member 3   Purpose: Main menu with RACE, GARAGE, SETTINGS, QUIT

using UnityEngine;

namespace NeonRush
{
    public class MainMenuUI : MonoBehaviour
    {
        private enum MenuState { Splash, Title, Main, TrackSelect, Settings }
        private MenuState currentState = MenuState.Splash;
        private int selectedTrackIndex = 0;
        private string[] trackIds = { "NEON_DOWNTOWN", "CYBER_SPEEDWAY", "GRID_CIRCUIT" };
        private string[] trackNames = { "NEON DOWNTOWN", "CYBER SPEEDWAY", "GRID CIRCUIT" };
        private int[] trackPrices = { 0, 1000, 2000 }; // First track free

        // Startup sequence state
        private float splashTimer = 0f;
        private const float SPLASH_DURATION = 3.2f;
        private static bool hasCompletedStartupSequence = false;
        private float titlePulseTimer = 0f;
        private float stateEnterTime = 0f;
        private Texture2D blackTex;
        private Texture2D glowTex;
        
        void Start()
        {
            Debug.Log("[NeonRush] MainMenuUI loaded");
            Time.timeScale = 1f;
            EnsureEventSystem();

            blackTex = new Texture2D(1, 1);
            blackTex.SetPixel(0, 0, new Color(0.03f, 0.04f, 0.08f, 1f));
            blackTex.Apply();

            glowTex = new Texture2D(1, 1);
            glowTex.SetPixel(0, 0, Color.white);
            glowTex.Apply();

            // If player already completed startup sequence in this session (e.g. returning from race), go to Main directly
            if (hasCompletedStartupSequence)
            {
                currentState = MenuState.Main;
                if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.MainMenu);
            }
            else
            {
                currentState = MenuState.Splash;
                splashTimer = 0f;
                stateEnterTime = Time.realtimeSinceStartup;
                if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Splash);
            }
        }

        void Update()
        {
            titlePulseTimer += Time.unscaledDeltaTime * 3f;

            if (currentState == MenuState.Splash)
            {
                splashTimer += Time.unscaledDeltaTime;
                // Allow skipping splash with Space, Enter, or Mouse click after a brief buffer (0.5s)
                if (splashTimer >= SPLASH_DURATION || (splashTimer > 0.6f && (Input.anyKeyDown || Input.GetMouseButtonDown(0))))
                {
                    AdvanceToTitle();
                }
            }
            else if (currentState == MenuState.Title)
            {
                // Prevent accidental skip on the exact same frame splash was skipped
                if (Time.realtimeSinceStartup - stateEnterTime > 0.4f)
                {
                    if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                    {
                        AdvanceToMain();
                    }
                }
            }
        }

        void AdvanceToTitle()
        {
            currentState = MenuState.Title;
            stateEnterTime = Time.realtimeSinceStartup;
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Title);
            Debug.Log("[NeonRush] Startup: Advanced to Title");
        }

        void AdvanceToMain()
        {
            hasCompletedStartupSequence = true;
            currentState = MenuState.Main;
            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.MainMenu);
            Debug.Log("[NeonRush] Startup: Advanced to Main Menu");
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
            switch (currentState)
            {
                case MenuState.Splash:
                    DrawStudioSplash();
                    break;
                case MenuState.Title:
                    DrawGameTitle();
                    break;
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

        void DrawStudioSplash()
        {
            // Dark futuristic backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTex);

            // Compute smooth fade curve (fade in: 0-0.8s, hold: 0.8-2.4s, fade out: 2.4-3.2s)
            float alpha = 1f;
            if (splashTimer < 0.8f) alpha = Mathf.Clamp01(splashTimer / 0.8f);
            else if (splashTimer > 2.4f) alpha = Mathf.Clamp01((SPLASH_DURATION - splashTimer) / 0.8f);

            Color studioColor = new Color(0f, 0.95f, 1f, alpha);
            Color subColor = new Color(1f, 0.15f, 0.85f, alpha * 0.9f);
            Color dimColor = new Color(0.6f, 0.7f, 0.8f, alpha * 0.6f);

            GUIStyle studioStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.038f), 32, 54),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            studioStyle.normal.textColor = studioColor;

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.016f), 16, 24),
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = subColor;

            GUIStyle skipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.LowerRight
            };
            skipStyle.normal.textColor = dimColor;

            float centerY = Screen.height * 0.45f;
            GUI.Label(new Rect(0, centerY - 40, Screen.width, 60), "HYPERION INTERACTIVE", studioStyle);
            GUI.Label(new Rect(0, centerY + 25, Screen.width, 35), "VIRTUAL RACING ARCHITECTURE", subStyle);

            GUI.Label(new Rect(Screen.width - 240, Screen.height - 40, 220, 30), "[SPACE / CLICK TO SKIP]", skipStyle);
        }

        void DrawGameTitle()
        {
            // Dark backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTex);

            float centerY = Screen.height * 0.36f;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.065f), 52, 92),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0f, 0.95f, 1f);

            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.024f), 22, 36),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            subtitleStyle.normal.textColor = new Color(1f, 0.15f, 0.85f);

            GUI.Label(new Rect(0, centerY - 60, Screen.width, 90), "NEON RUSH", titleStyle);
            GUI.Label(new Rect(0, centerY + 40, Screen.width, 45), "— GRID RUNNERS —", subtitleStyle);

            // Pulsing "Press Any Key" prompt
            float pulse = (Mathf.Sin(titlePulseTimer) + 1f) * 0.5f;
            Color promptColor = Color.Lerp(new Color(0.7f, 0.9f, 1f, 0.4f), new Color(1f, 1f, 0.2f, 1f), pulse);

            GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.02f), 18, 28),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            promptStyle.normal.textColor = promptColor;

            GUI.Label(new Rect(0, Screen.height * 0.72f, Screen.width, 50), "PRESS ANY KEY OR CLICK TO ENTER", promptStyle);

            GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.LowerCenter
            };
            hintStyle.normal.textColor = new Color(0.5f, 0.6f, 0.7f, 0.8f);
            GUI.Label(new Rect(0, Screen.height - 45, Screen.width, 30), "KEYBOARD / CONTROLLER READY  •  UNITY 6 ARCADE ENGINE", hintStyle);
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
            
            if (GUI.Button(new Rect(centerX - 180, startY + 80, 360, 60), "GARAGE", buttonStyle))
            {
                var garageUI = FindAnyObjectByType<GarageUI>();
                if (garageUI != null)
                {
                    garageUI.enabled = true;
                    enabled = false;
                }
                else
                {
                    gameObject.AddComponent<GarageUI>();
                    enabled = false;
                }
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
