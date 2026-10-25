// Module: Core   Owner: Member 3   Purpose: Central game state machine and session data manager

using UnityEngine;
using System;

namespace NeonRush
{
    public enum GameState
    {
        Splash,
        Title,
        MainMenu,
        Loading,
        Intro,
        Countdown,
        Racing,
        Paused,
        Finish,
        Results
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Current State")]
        public GameState currentState = GameState.MainMenu;
        
        [Header("Session Data")]
        public string selectedVehicleId = "VOLT"; // default to free vehicle
        public string selectedTrackId = "NEON_DOWNTOWN";
        public string selectedMode = "GRAND_PRIX";
        
        [Header("Race State")]
        public bool isGameActive = false;
        public float raceTime = 0f;
        public int score = 0;
        
        // Events for state changes
        public event Action<GameState, GameState> OnStateChanged; // oldState, newState

        void Awake()
        {
            // Singleton with DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            // Initialize based on current scene
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("MainMenu"))
            {
                ChangeState(GameState.MainMenu);
            }
        }

        void Update()
        {
            // Update race time if racing
            if (currentState == GameState.Racing && isGameActive)
            {
                raceTime += Time.deltaTime;
            }
        }

        public void ChangeState(GameState newState)
        {
            if (newState == currentState) return;
            
            GameState oldState = currentState;
            currentState = newState;
            
            // Handle state transitions
            switch (newState)
            {
                case GameState.Racing:
                    isGameActive = true;
                    raceTime = 0f;
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                    
                case GameState.Finish:
                case GameState.Results:
                    isGameActive = false;
                    Time.timeScale = 1f;
                    break;
                    
                default:
                    Time.timeScale = 1f;
                    break;
            }
            
            // Fire event
            OnStateChanged?.Invoke(oldState, newState);
        }

        public void AddScore(int amount)
        {
            score += amount;
        }

        public void ResetSession()
        {
            score = 0;
            raceTime = 0f;
            isGameActive = false;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                // Clean up events
                OnStateChanged = null;
            }
        }
    }
}
