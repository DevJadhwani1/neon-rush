// Module: UI   Owner: Member 3   Purpose: Game mode selection screen

using UnityEngine;

namespace NeonRush
{
    public class ModeSelectionUI : MonoBehaviour
    {
        private string[] modeNames = { "TIME TRIAL", "GRAND PRIX", "POLICE CHASE" };
        private string[] modeDescriptions = {
            "Complete the sprint course as fast as possible!",
            "Race against 3 AI opponents!",
            "Survive 5 minutes of police pursuit!"
        };
        private int selectedModeIndex = 0;
        
        void OnGUI()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 48;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.UpperCenter;
            titleStyle.normal.textColor = new Color(0f, 0.9f, 1f);
            
            GUI.Label(new Rect(0, 40, Screen.width, 60), "SELECT MODE", titleStyle);
            
            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.4f;
            
            // Mode selection arrows
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 28;
            
            if (GUI.Button(new Rect(centerX - 350, centerY, 80, 80), "<", buttonStyle))
            {
                selectedModeIndex = (selectedModeIndex - 1 + modeNames.Length) % modeNames.Length;
            }
            
            // Mode display
            GUIStyle modeStyle = new GUIStyle(GUI.skin.box);
            modeStyle.fontSize = 32;
            modeStyle.fontStyle = FontStyle.Bold;
            modeStyle.alignment = TextAnchor.MiddleCenter;
            modeStyle.normal.textColor = Color.white;
            
            GUI.Box(new Rect(centerX - 250, centerY, 500, 80), modeNames[selectedModeIndex], modeStyle);
            
            GUIStyle descStyle = new GUIStyle(GUI.skin.label);
            descStyle.fontSize = 20;
            descStyle.alignment = TextAnchor.UpperCenter;
            descStyle.normal.textColor = Color.white;
            descStyle.wordWrap = true;
            
            GUI.Label(new Rect(centerX - 250, centerY + 100, 500, 60), modeDescriptions[selectedModeIndex], descStyle);
            
            if (GUI.Button(new Rect(centerX + 270, centerY, 80, 80), ">", buttonStyle))
            {
                selectedModeIndex = (selectedModeIndex + 1) % modeNames.Length;
            }
            
            // Start button
            buttonStyle.fontSize = 32;
            if (GUI.Button(new Rect(centerX - 150, centerY + 180, 300, 60), "START RACE", buttonStyle))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.selectedMode = modeNames[selectedModeIndex];
                }
                SceneLoader.LoadRace();
            }
            
            // Back button
            buttonStyle.fontSize = 24;
            if (GUI.Button(new Rect(50, Screen.height - 100, 200, 50), "BACK", buttonStyle))
            {
                SceneLoader.LoadMainMenu();
            }
        }
    }
}
