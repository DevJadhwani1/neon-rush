// Module: UI   Owner: Member 3   Purpose: Professional startup sequence (Studio Splash, Game Title) and Main Menu (Race, Track Select, Garage, Settings, Quit)

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
        private Texture2D cardTex;

        // Cached GUI Styles
        private GUIStyle studioStyle;
        private GUIStyle subStyle;
        private GUIStyle skipStyle;
        private GUIStyle gameTitleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle promptStyle;
        private GUIStyle hintStyle;
        private GUIStyle menuTitleStyle;
        private GUIStyle menuSubStyle;
        private GUIStyle menuButtonStyle;
        private GUIStyle creditsStyle;
        private GUIStyle cardStyle;
        private GUIStyle lockStyle;
        private GUIStyle labelStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetDomain()
        {
            hasCompletedStartupSequence = false;
        }

        void Awake()
        {
            EnsureTextures();
        }

        void Start()
        {
            Debug.Log("[NeonRush] MainMenuUI loaded");
            Time.timeScale = 1f;
            EnsureEventSystem();
            EnsureTextures();

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

        void EnsureTextures()
        {
            if (blackTex == null)
            {
                blackTex = new Texture2D(1, 1);
                blackTex.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.06f, 1f));
                blackTex.Apply();
            }

            if (cardTex == null)
            {
                cardTex = new Texture2D(1, 1);
                cardTex.SetPixel(0, 0, new Color(0.06f, 0.08f, 0.15f, 0.9f));
                cardTex.Apply();
            }
        }

        void Update()
        {
            titlePulseTimer += Time.unscaledDeltaTime * 3f;

            if (currentState == MenuState.Splash)
            {
                splashTimer += Time.unscaledDeltaTime;
                // Allow skipping splash with Space, Enter, or Mouse click after a brief buffer (0.6s)
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
            GUI.depth = 0;
            EnsureTextures();
            InitStyles();

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

        void InitStyles()
        {
            if (studioStyle != null) return;

            studioStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.038f), 32, 54),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.016f), 16, 24),
                alignment = TextAnchor.MiddleCenter
            };

            skipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.LowerRight
            };

            gameTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.065f), 52, 92),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            gameTitleStyle.normal.textColor = new Color(0f, 0.95f, 1f);

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.024f), 22, 36),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            subtitleStyle.normal.textColor = new Color(1f, 0.15f, 0.85f);

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.02f), 18, 28),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.LowerCenter
            };
            hintStyle.normal.textColor = new Color(0.5f, 0.6f, 0.7f, 0.8f);

            menuTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 68,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
            menuTitleStyle.normal.textColor = new Color(0f, 0.95f, 1f);

            menuSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
            menuSubStyle.normal.textColor = new Color(1f, 0.15f, 0.85f);

            menuButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            creditsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight
            };
            creditsStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

            cardStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            cardStyle.normal.textColor = Color.white;

            lockStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
            lockStyle.normal.textColor = new Color(1f, 0.25f, 0.35f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = Color.white;
        }

        void DrawStudioSplash()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTex);

            // Compute smooth fade curve (fade in: 0-0.8s, hold: 0.8-2.4s, fade out: 2.4-3.2s)
            float alpha = 1f;
            if (splashTimer < 0.8f) alpha = Mathf.Clamp01(splashTimer / 0.8f);
            else if (splashTimer > 2.4f) alpha = Mathf.Clamp01((SPLASH_DURATION - splashTimer) / 0.8f);

            studioStyle.normal.textColor = new Color(0f, 0.95f, 1f, alpha);
            subStyle.normal.textColor = new Color(1f, 0.15f, 0.85f, alpha * 0.9f);
            skipStyle.normal.textColor = new Color(0.6f, 0.7f, 0.8f, alpha * 0.6f);

            float centerY = Screen.height * 0.45f;
            GUI.Label(new Rect(0, centerY - 40, Screen.width, 60), "HYPERION INTERACTIVE", studioStyle);
            GUI.Label(new Rect(0, centerY + 25, Screen.width, 35), "VIRTUAL RACING ARCHITECTURE", subStyle);

            GUI.Label(new Rect(Screen.width - 240, Screen.height - 40, 220, 30), "[SPACE / CLICK TO SKIP]", skipStyle);
        }

        void DrawGameTitle()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTex);

            float centerY = Screen.height * 0.36f;

            GUI.Label(new Rect(0, centerY - 60, Screen.width, 90), "NEON RUSH", gameTitleStyle);
            GUI.Label(new Rect(0, centerY + 40, Screen.width, 45), "— GRID RUNNERS —", subtitleStyle);

            // Pulsing "Press Any Key" prompt
            float pulse = (Mathf.Sin(titlePulseTimer) + 1f) * 0.5f;
            Color promptColor = Color.Lerp(new Color(0.7f, 0.9f, 1f, 0.4f), new Color(1f, 1f, 0.2f, 1f), pulse);
            promptStyle.normal.textColor = promptColor;

            GUI.Label(new Rect(0, Screen.height * 0.72f, Screen.width, 50), "PRESS ANY KEY OR CLICK TO ENTER", promptStyle);
            GUI.Label(new Rect(0, Screen.height - 45, Screen.width, 30), "KEYBOARD / CONTROLLER READY  •  UNITY 6 ARCADE ENGINE", hintStyle);
        }

        void DrawMainMenu()
        {
            float centerX = Screen.width * 0.5f;
            float startY = Screen.height * 0.46f;

            GUI.Label(new Rect(0, 70, Screen.width, 85), "NEON RUSH", menuTitleStyle);
            GUI.Label(new Rect(0, 160, Screen.width, 35), "GRID RUNNERS", menuSubStyle);

            float btnW = 340f;
            float btnH = 55f;
            float btnX = centerX - (btnW * 0.5f);

            if (GUI.Button(new Rect(btnX, startY, btnW, btnH), "RACE", menuButtonStyle))
            {
                currentState = MenuState.TrackSelect;
            }

            if (GUI.Button(new Rect(btnX, startY + 70, btnW, btnH), "GARAGE", menuButtonStyle))
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

            if (GUI.Button(new Rect(btnX, startY + 140, btnW, btnH), "SETTINGS", menuButtonStyle))
            {
                currentState = MenuState.Settings;
            }

            if (GUI.Button(new Rect(btnX, startY + 210, btnW, btnH), "QUIT", menuButtonStyle))
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }

            // Credits display
            SaveData data = SaveSystem.Load();
            GUI.Label(new Rect(Screen.width - 270, 20, 250, 30), "CREDITS: " + data.credits + " CR", creditsStyle);
        }

        void DrawTrackSelect()
        {
            GUI.Label(new Rect(0, 45, Screen.width, 60), "SELECT CIRCUIT", menuTitleStyle);

            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.42f;

            // Track selection arrows
            if (GUI.Button(new Rect(centerX - 330, centerY, 70, 70), "<", menuButtonStyle))
            {
                selectedTrackIndex = (selectedTrackIndex - 1 + trackNames.Length) % trackNames.Length;
            }

            // Track display card
            GUI.Box(new Rect(centerX - 240, centerY, 480, 70), trackNames[selectedTrackIndex], cardStyle);

            SaveData data = SaveSystem.Load();
            bool isUnlocked = trackPrices[selectedTrackIndex] == 0 || data.unlockedTracks.Contains(trackIds[selectedTrackIndex]);

            if (!isUnlocked)
            {
                GUI.Label(new Rect(centerX - 150, centerY + 85, 300, 35), "🔒 LOCKED - " + trackPrices[selectedTrackIndex] + " CR", lockStyle);
            }

            if (GUI.Button(new Rect(centerX + 260, centerY, 70, 70), ">", menuButtonStyle))
            {
                selectedTrackIndex = (selectedTrackIndex + 1) % trackNames.Length;
            }

            // Start race or purchase
            float actionBtnW = 320f;
            float actionBtnH = 55f;
            float actionBtnX = centerX - (actionBtnW * 0.5f);

            if (isUnlocked)
            {
                if (GUI.Button(new Rect(actionBtnX, centerY + 160, actionBtnW, actionBtnH), "START RACE", menuButtonStyle))
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
                if (GUI.Button(new Rect(actionBtnX, centerY + 160, actionBtnW, actionBtnH), "UNLOCK - " + trackPrices[selectedTrackIndex] + " CR", menuButtonStyle))
                {
                    data.credits -= trackPrices[selectedTrackIndex];
                    data.unlockedTracks.Add(trackIds[selectedTrackIndex]);
                    SaveSystem.Save(data);
                }
                GUI.enabled = true;
            }

            // Back button
            if (GUI.Button(new Rect(50, Screen.height - 90, 180, 48), "BACK", menuButtonStyle))
            {
                currentState = MenuState.Main;
            }
        }

        void DrawSettings()
        {
            GUI.Label(new Rect(0, 45, Screen.width, 60), "SETTINGS", menuTitleStyle);

            SaveData data = SaveSystem.Load();
            float centerX = Screen.width * 0.5f;
            float startY = 180;

            // Master Volume
            GUI.Label(new Rect(centerX - 280, startY, 180, 30), "Master Volume", labelStyle);
            data.masterVolume = GUI.HorizontalSlider(new Rect(centerX - 80, startY + 8, 300, 30), data.masterVolume, 0f, 1f);

            // Music Volume
            GUI.Label(new Rect(centerX - 280, startY + 60, 180, 30), "Music Volume", labelStyle);
            data.musicVolume = GUI.HorizontalSlider(new Rect(centerX - 80, startY + 68, 300, 30), data.musicVolume, 0f, 1f);

            // SFX Volume
            GUI.Label(new Rect(centerX - 280, startY + 120, 180, 30), "SFX Volume", labelStyle);
            data.sfxVolume = GUI.HorizontalSlider(new Rect(centerX - 80, startY + 128, 300, 30), data.sfxVolume, 0f, 1f);

            // Camera Shake
            GUI.Label(new Rect(centerX - 280, startY + 180, 180, 30), "Camera Shake", labelStyle);
            data.cameraShake = GUI.Toggle(new Rect(centerX - 80, startY + 180, 100, 30), data.cameraShake, data.cameraShake ? "ON" : "OFF");

            SaveSystem.Save(data);

            // Back button
            if (GUI.Button(new Rect(50, Screen.height - 90, 180, 48), "BACK", menuButtonStyle))
            {
                currentState = MenuState.Main;
            }
        }

        void OnDestroy()
        {
            if (blackTex != null) { Destroy(blackTex); blackTex = null; }
            if (cardTex != null) { Destroy(cardTex); cardTex = null; }
        }
    }
}
