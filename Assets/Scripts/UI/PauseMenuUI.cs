// Module: UI   Owner: Member 3   Purpose: In-race pause menu with safe timeScale management, controls reference card, and loading guards

using UnityEngine;

namespace NeonRush
{
    public class PauseMenuUI : MonoBehaviour
    {
        private bool isPaused = false;
        private GUIStyle titleStyle;
        private GUIStyle subTitleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle sectionHeaderStyle;
        private GUIStyle controlKeyStyle;
        private GUIStyle controlDescStyle;

        private Texture2D dimTex;
        private Texture2D cardTex;

        void Start()
        {
            EnsureTextures();
        }

        void EnsureTextures()
        {
            if (dimTex == null)
            {
                dimTex = new Texture2D(1, 1);
                dimTex.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.07f, 0.88f));
                dimTex.Apply();
            }

            if (cardTex == null)
            {
                cardTex = new Texture2D(1, 1);
                cardTex.SetPixel(0, 0, new Color(0.06f, 0.09f, 0.16f, 0.92f));
                cardTex.Apply();
            }
        }

        void Update()
        {
            // Do not allow pausing while SceneLoader is actively loading a scene
            if (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading)
            {
                if (isPaused)
                {
                    isPaused = false;
                    Time.timeScale = 1f;
                }
                return;
            }

            // ESC or P to toggle pause
            if (GameInput.GetPauseDown())
            {
                TogglePause();
            }
        }

        public bool IsPaused => isPaused;

        public void Resume()
        {
            if (isPaused) TogglePause();
            else Time.timeScale = 1f;
        }

        public void TogglePause()
        {
            // If currently loading, ignore
            if (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading) return;

            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(isPaused ? GameState.Paused : GameState.Racing);
            }
        }

        void OnGUI()
        {
            // Do not draw pause menu if not paused or while loading screen is up
            if (!isPaused || (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading)) return;

            GUI.depth = -100;
            EnsureTextures();

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 52,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter
                };
                titleStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                subTitleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter
                };
                subTitleStyle.normal.textColor = new Color(1f, 0.15f, 0.85f);

                sectionHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter
                };
                sectionHeaderStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                controlKeyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight
                };
                controlKeyStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                controlDescStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft
                };
                controlDescStyle.normal.textColor = Color.white;
            }

            // Backdrop darkening overlay
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimTex);

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.45f;

            GUI.Label(new Rect(0, cy - 220, Screen.width, 60), "RACE PAUSED", titleStyle);
            GUI.Label(new Rect(0, cy - 165, Screen.width, 30), "CYBERNETIC SYSTEMS FROZEN", subTitleStyle);

            // Controls Reference Card (Required Phase 5 Feature)
            float cardW = Mathf.Clamp(Screen.width * 0.62f, 480f, 660f);
            float cardH = 175f;
            float cardX = cx - (cardW * 0.5f);
            float cardY = cy - 130f;

            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), cardTex);
            GUI.Label(new Rect(cardX, cardY + 8, cardW, 25), "⚡ RACE CONTROLS REFERENCE ⚡", sectionHeaderStyle);

            string[,] controls = new string[,]
            {
                { "[W] / [↑]", "Drive & Accelerate" },
                { "[A] / [D]", "Steer Left & Right" },
                { "[S] / [↓]", "Brake & Reverse" },
                { "[LEFT SHIFT]", "Charge Drift Nitro" },
                { "[SPACE]", "Anti-Gravity Hover" },
                { "[R]", "Quick Restart Race" },
                { "[ESC] / [P]", "Pause & Resume" }
            };

            float col1X = cardX + 20f;
            float col1KeyW = 120f;
            float col1DescW = 180f;

            float col2X = cardX + (cardW * 0.5f) + 10f;
            float col2KeyW = 120f;
            float col2DescW = 180f;

            float rowStartY = cardY + 38f;
            float rowHeight = 28f;

            for (int i = 0; i < controls.GetLength(0); i++)
            {
                bool isCol2 = (i >= 4);
                int rowIndex = isCol2 ? (i - 4) : i;

                float xKey = isCol2 ? col2X : col1X;
                float keyW = isCol2 ? col2KeyW : col1KeyW;
                float descW = isCol2 ? col2DescW : col1DescW;
                float rowY = rowStartY + (rowIndex * rowHeight);

                GUI.Label(new Rect(xKey, rowY, keyW, 24), controls[i, 0], controlKeyStyle);
                GUI.Label(new Rect(xKey + keyW + 8, rowY, descW, 24), controls[i, 1], controlDescStyle);
            }

            // Action Buttons
            float btnW = 260f;
            float btnH = 46f;
            float btnX = cx - (btnW * 0.5f);
            float btnStartY = cardY + cardH + 20f;

            if (GUI.Button(new Rect(btnX, btnStartY, btnW, btnH), "RESUME", buttonStyle))
            {
                TogglePause();
            }

            if (GUI.Button(new Rect(btnX, btnStartY + 55f, btnW, btnH), "RESTART RACE", buttonStyle))
            {
                isPaused = false;
                Time.timeScale = 1f;
                SceneLoader.LoadRace();
            }

            if (GUI.Button(new Rect(btnX, btnStartY + 110f, btnW, btnH), "MAIN MENU", buttonStyle))
            {
                isPaused = false;
                Time.timeScale = 1f;
                SceneLoader.LoadMainMenu();
            }
        }

        void OnDestroy()
        {
            // Defensive cleanup to avoid stuck timeScale
            Time.timeScale = 1f;

            if (dimTex != null) { Destroy(dimTex); dimTex = null; }
            if (cardTex != null) { Destroy(cardTex); cardTex = null; }
        }
    }
}
