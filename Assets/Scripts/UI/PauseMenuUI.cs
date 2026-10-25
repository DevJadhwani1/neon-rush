// Module: UI   Owner: Member 3   Purpose: In-race pause menu

using UnityEngine;

namespace NeonRush
{
    public class PauseMenuUI : MonoBehaviour
    {
        private bool isPaused = false;
        
        void Update()
        {
            // ESC or P to pause
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }

        void TogglePause()
        {
            isPaused = !isPaused;
            
            Time.timeScale = isPaused ? 0f : 1f;
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(isPaused ? GameState.Paused : GameState.Racing);
            }
        }

        void OnGUI()
        {
            if (!isPaused) return;
            
            // Darken background
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 64;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperCenter;
            titleStyle.normal.textColor = new Color(0f, 0.9f, 1f);
            
            GUI.Label(new Rect(0, 100, Screen.width, 80), "PAUSED", titleStyle);
            
            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 28;
            buttonStyle.fontStyle = FontStyle.Bold;
            
            if (GUI.Button(new Rect(centerX - 150, centerY - 60, 300, 50), "RESUME", buttonStyle))
            {
                TogglePause();
            }
            
            if (GUI.Button(new Rect(centerX - 150, centerY + 10, 300, 50), "RESTART", buttonStyle))
            {
                Time.timeScale = 1f;
                SceneLoader.LoadRace();
            }
            
            if (GUI.Button(new Rect(centerX - 150, centerY + 80, 300, 50), "MAIN MENU", buttonStyle))
            {
                Time.timeScale = 1f;
                SceneLoader.LoadMainMenu();
            }
        }
    }
}
