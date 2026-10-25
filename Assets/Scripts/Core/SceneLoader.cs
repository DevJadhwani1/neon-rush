// Module: Core   Owner: Member 3   Purpose: Professional async scene loader with real normalized progress, permanent race controls panel, and rotating gameplay tips carousel

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace NeonRush
{
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;
        public static SceneLoader Instance => instance;

        [Header("State")]
        private bool isLoading = false;
        public bool IsLoading => isLoading;

        // UI & Animation State
        private float displayProgress = 0f;
        private string currentStatusMessage = "INITIALIZING CIRCUIT...";
        private int currentTipIndex = 0;
        private float tipTimer = 0f;
        private const float TIP_INTERVAL = 2.6f;

        private Texture2D overlayBgTex;
        private Texture2D cardBgTex;
        private Texture2D barBgTex;
        private Texture2D barFillTex;
        private Texture2D borderTex;
        private Texture2D keycapTex;

        // Cached GUI Styles
        private GUIStyle brandStyle;
        private GUIStyle subBrandStyle;
        private GUIStyle percentStyle;
        private GUIStyle statusStyle;
        private GUIStyle panelHeaderStyle;
        private GUIStyle keycapStyle;
        private GUIStyle controlDescStyle;
        private GUIStyle tipHeaderStyle;
        private GUIStyle tipBodyStyle;

        public static readonly string[] GAMEPLAY_TIPS = new string[]
        {
            "Hit [SPACE] to activate Anti-Gravity Hover and glide effortlessly over circuit hazards.",
            "Tap and hold [LEFT SHIFT] while steering into sharp bends to charge your Drift Nitro tiers.",
            "Blue Nitro triggers an immediate burst; hold the drift longer for Orange and Pink tiers!",
            "Collect Quantum Shards scattered on the track to maximize your final post-race credit payouts.",
            "Visit the Garage from the Main Menu to upgrade Engine Speed, Handling, Boost, and Armor.",
            "Draft directly in an opponent's wake to activate Slipstream acceleration and slingshot past.",
            "Press [R] at any point during a race to trigger a rapid track restart.",
            "Sequential checkpoints validate your lap times — out-of-order passage is strictly rejected!"
        };

        public static float NormalizeProgress(float rawOpProgress)
        {
            return Mathf.Clamp01(rawOpProgress / 0.9f);
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitTextures();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        void InitTextures()
        {
            if (overlayBgTex == null)
            {
                overlayBgTex = new Texture2D(1, 1);
                overlayBgTex.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.06f, 0.98f));
                overlayBgTex.Apply();
            }

            if (cardBgTex == null)
            {
                cardBgTex = new Texture2D(1, 1);
                cardBgTex.SetPixel(0, 0, new Color(0.06f, 0.08f, 0.15f, 0.92f));
                cardBgTex.Apply();
            }

            if (barBgTex == null)
            {
                barBgTex = new Texture2D(1, 1);
                barBgTex.SetPixel(0, 0, new Color(0.08f, 0.1f, 0.16f, 0.95f));
                barBgTex.Apply();
            }

            if (barFillTex == null)
            {
                barFillTex = new Texture2D(1, 1);
                barFillTex.SetPixel(0, 0, new Color(0f, 0.95f, 1f, 1f)); // Neon cyan
                barFillTex.Apply();
            }

            if (borderTex == null)
            {
                borderTex = new Texture2D(1, 1);
                borderTex.SetPixel(0, 0, new Color(1f, 0.15f, 0.85f, 0.85f)); // Neon magenta border
                borderTex.Apply();
            }

            if (keycapTex == null)
            {
                keycapTex = new Texture2D(1, 1);
                keycapTex.SetPixel(0, 0, new Color(0.12f, 0.16f, 0.28f, 0.9f));
                keycapTex.Apply();
            }
        }

        public static void EnsureLoaderExists()
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneLoader");
                go.AddComponent<SceneLoader>();
            }
        }

        public static void LoadMainMenu()
        {
            EnsureLoaderExists();
            if (instance != null && !instance.isLoading)
            {
                instance.StartCoroutine(instance.LoadSceneAsyncRoutine("MainMenu", false));
            }
        }

        public static void LoadRace()
        {
            EnsureLoaderExists();
            if (instance != null && !instance.isLoading)
            {
                instance.StartCoroutine(instance.LoadSceneAsyncRoutine("GravShiftNeonCircuit", true));
            }
        }

        IEnumerator LoadSceneAsyncRoutine(string sceneName, bool isRaceScene)
        {
            isLoading = true;
            displayProgress = 0f;
            currentTipIndex = Random.Range(0, GAMEPLAY_TIPS.Length);
            tipTimer = 0f;
            currentStatusMessage = isRaceScene ? "ESTABLISHING QUANTUM CIRCUIT..." : "INITIALIZING MENU MATRIX...";

            // Ensure timeScale is normal during transition
            Time.timeScale = 1f;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Loading);
            }

            Debug.Log($"[SceneLoader] Asynchronous loading initiated: {sceneName}");

            // Initial buffer to render loading overlay
            yield return new WaitForSecondsRealtime(0.15f);

            AsyncOperation op = null;
            try
            {
                if (isRaceScene)
                {
                    op = SceneManager.LoadSceneAsync(1);
                }
                else
                {
                    op = SceneManager.LoadSceneAsync(sceneName);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SceneLoader] LoadSceneAsync error: {ex.Message}");
            }

            if (op == null)
            {
                Debug.LogWarning($"[SceneLoader] Failed async load for '{sceneName}'. Attempting direct fallback.");
                try
                {
                    SceneManager.LoadScene(isRaceScene ? 1 : 0);
                }
                catch (System.Exception ex2)
                {
                    Debug.LogError($"[SceneLoader] Fallback load also failed: {ex2.Message}");
                }
                isLoading = false;
                yield break;
            }

            // Keep scene from activating automatically until smooth progress completes
            op.allowSceneActivation = false;

            // Normalize Unity's 0.0 -> 0.9 raw progress to 0.0 -> 1.0 real progress
            while (op.progress < 0.9f || displayProgress < 0.95f)
            {
                float targetNormalizedProgress = NormalizeProgress(op.progress);

                // Smoothly animate displayProgress towards the real async progress
                displayProgress = Mathf.MoveTowards(displayProgress, targetNormalizedProgress, Time.unscaledDeltaTime * 1.5f);

                if (displayProgress < 0.35f)
                {
                    currentStatusMessage = "SYNCHRONIZING TELEMETRY STREAM...";
                }
                else if (displayProgress < 0.75f)
                {
                    currentStatusMessage = "COMPILING SHADERS & STREAMING GEOMETRY...";
                }
                else
                {
                    currentStatusMessage = "CIRCUIT READY — ACTIVATING INTERFACE...";
                }

                yield return null;
            }

            displayProgress = 1f;
            currentStatusMessage = "SYSTEMS ONLINE";
            yield return new WaitForSecondsRealtime(0.2f);

            // Activate loaded scene
            op.allowSceneActivation = true;

            // Wait for activation completion
            while (!op.isDone)
            {
                yield return null;
            }

            // Post-activation fade transition
            yield return new WaitForSecondsRealtime(0.1f);
            isLoading = false;
            Debug.Log($"[SceneLoader] Scene '{sceneName}' successfully loaded and activated.");
        }

        void Update()
        {
            if (isLoading)
            {
                tipTimer += Time.unscaledDeltaTime;
                if (tipTimer >= TIP_INTERVAL)
                {
                    tipTimer = 0f;
                    currentTipIndex = (currentTipIndex + 1) % GAMEPLAY_TIPS.Length;
                }
            }
        }

        void OnGUI()
        {
            if (!isLoading) return;

            // Set GUI depth so loading screen renders atop all scene GUIs
            GUI.depth = -1000;
            InitTextures();

            if (brandStyle == null)
            {
                brandStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.042f), 34, 58),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                brandStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                subBrandStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.016f), 15, 24),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                subBrandStyle.normal.textColor = new Color(1f, 0.15f, 0.85f);

                percentStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight
                };
                percentStyle.normal.textColor = Color.white;

                statusStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
                statusStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                panelHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter
                };
                panelHeaderStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                keycapStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight
                };
                keycapStyle.normal.textColor = new Color(1f, 0.15f, 0.85f);

                controlDescStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft
                };
                controlDescStyle.normal.textColor = Color.white;

                tipHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                tipHeaderStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                tipBodyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
                tipBodyStyle.normal.textColor = new Color(0.9f, 0.92f, 0.96f, 0.9f);
            }

            // Background dark backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayBgTex);

            float cx = Screen.width * 0.5f;

            // Header Branding
            float topY = Mathf.Max(30f, Screen.height * 0.08f);
            GUI.Label(new Rect(0, topY, Screen.width, 60), "NEON RUSH", brandStyle);
            GUI.Label(new Rect(0, topY + 54, Screen.width, 30), "GRID RUNNERS // QUANTUM TELEMETRY", subBrandStyle);

            // Progress Bar dimensions
            float barW = Mathf.Clamp(Screen.width * 0.68f, 340f, 740f);
            float barH = 22f;
            float barX = cx - (barW * 0.5f);
            float barY = topY + 115f;

            // Status & Percentage
            int percent = Mathf.Clamp(Mathf.RoundToInt(displayProgress * 100f), 0, 100);
            GUI.Label(new Rect(barX, barY - 26, barW * 0.75f, 22), currentStatusMessage, statusStyle);
            GUI.Label(new Rect(barX + (barW * 0.7f), barY - 26, barW * 0.3f, 22), $"{percent}%", percentStyle);

            // Progress Bar Rendering
            GUI.DrawTexture(new Rect(barX - 2, barY - 2, barW + 4, barH + 4), borderTex);
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), barBgTex);
            float filledW = barW * Mathf.Clamp01(displayProgress);
            GUI.DrawTexture(new Rect(barX, barY, filledW, barH), barFillTex);

            // PERMANENT RACE CONTROLS PANEL (Required Feature)
            float controlsCardW = barW;
            float controlsCardH = 160f;
            float controlsCardX = barX;
            float controlsCardY = barY + 45f;

            GUI.DrawTexture(new Rect(controlsCardX, controlsCardY, controlsCardW, controlsCardH), cardBgTex);
            GUI.Label(new Rect(controlsCardX, controlsCardY + 8, controlsCardW, 24), "⚡ RACE CONTROLS ⚡", panelHeaderStyle);

            string[,] controlsLegend = new string[,]
            {
                { "[W] / [↑]", "Accelerate" },
                { "[A] / [D]", "Steer Left / Right" },
                { "[S] / [↓]", "Brake & Reverse" },
                { "[LEFT SHIFT]", "Drift Nitro" },
                { "[SPACE]", "Anti-Gravity Hover" },
                { "[ESC] / [P]", "Pause / Resume" },
                { "[R]", "Quick Restart" }
            };

            float col1X = controlsCardX + 20f;
            float col1KeyW = 120f;
            float col1DescW = (controlsCardW * 0.5f) - 150f;

            float col2X = controlsCardX + (controlsCardW * 0.5f) + 10f;
            float col2KeyW = 120f;
            float col2DescW = (controlsCardW * 0.5f) - 150f;

            float rowStartY = controlsCardY + 36f;
            float rowH = 26f;

            for (int i = 0; i < controlsLegend.GetLength(0); i++)
            {
                bool isCol2 = (i >= 4);
                int rowIndex = isCol2 ? (i - 4) : i;

                float kX = isCol2 ? col2X : col1X;
                float kW = isCol2 ? col2KeyW : col1KeyW;
                float dW = isCol2 ? col2DescW : col1DescW;
                float rY = rowStartY + (rowIndex * rowH);

                GUI.Label(new Rect(kX, rY, kW, 22), controlsLegend[i, 0], keycapStyle);
                GUI.Label(new Rect(kX + kW + 8, rY, dW, 22), controlsLegend[i, 1], controlDescStyle);
            }

            // ROTATING GAMEPLAY TIPS CAROUSEL
            float tipCardH = 65f;
            float tipCardY = controlsCardY + controlsCardH + 18f;

            GUI.DrawTexture(new Rect(controlsCardX, tipCardY, controlsCardW, tipCardH), cardBgTex);
            GUI.Label(new Rect(controlsCardX, tipCardY + 6, controlsCardW, 20), $"[TACTICAL BRIEFING {currentTipIndex + 1}/{GAMEPLAY_TIPS.Length}]", tipHeaderStyle);
            GUI.Label(new Rect(controlsCardX + 15, tipCardY + 28, controlsCardW - 30, 30), GAMEPLAY_TIPS[currentTipIndex], tipBodyStyle);
        }

        void OnDestroy()
        {
            if (overlayBgTex != null) { Destroy(overlayBgTex); overlayBgTex = null; }
            if (cardBgTex != null) { Destroy(cardBgTex); cardBgTex = null; }
            if (barBgTex != null) { Destroy(barBgTex); barBgTex = null; }
            if (barFillTex != null) { Destroy(barFillTex); barFillTex = null; }
            if (borderTex != null) { Destroy(borderTex); borderTex = null; }
            if (keycapTex != null) { Destroy(keycapTex); keycapTex = null; }
        }
    }
}
