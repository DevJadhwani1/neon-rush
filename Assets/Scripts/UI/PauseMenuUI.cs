// Module: UI   Owner: Member 3   Purpose: In-race pause menu with safe timeScale management and loading guard

using UnityEngine;

namespace NeonRush
{
    public class PauseMenuUI : MonoBehaviour
    {
        private bool isPaused = false;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle tipStyle;
        private Texture2D dimTex;

        void Start()
        {
            dimTex = new Texture2D(1, 1);
            dimTex.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.07f, 0.85f));
            dimTex.Apply();
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
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
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

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 60,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter
                };
                titleStyle.normal.textColor = new Color(0f, 0.95f, 1f);

                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 24,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                tipStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    alignment = TextAnchor.MiddleCenter
                };
                tipStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f, 0.8f);
            }

            // Backdrop darkening overlay
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimTex);

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.45f;

            GUI.Label(new Rect(0, cy - 140, Screen.width, 70), "RACE PAUSED", titleStyle);
            GUI.Label(new Rect(0, cy - 80, Screen.width, 30), "CYBER CIRCUITS FROZEN", tipStyle);

            float btnW = 280f;
            float btnH = 50f;
            float btnX = cx - (btnW * 0.5f);

            if (GUI.Button(new Rect(btnX, cy - 30, btnW, btnH), "RESUME", buttonStyle))
            {
                TogglePause();
            }

            if (GUI.Button(new Rect(btnX, cy + 35, btnW, btnH), "RESTART RACE", buttonStyle))
            {
                isPaused = false;
                Time.timeScale = 1f;
                SceneLoader.LoadRace();
            }

            if (GUI.Button(new Rect(btnX, cy + 100, btnW, btnH), "MAIN MENU", buttonStyle))
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
        }
    }
}
