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
        public GameState CurrentState => currentState;
        
        [Header("Session Data")]
        public string selectedVehicleId = "VOLT"; // default to free vehicle
        public string selectedTrackId = "NEON_DOWNTOWN";
        public string selectedMode = "GRAND_PRIX";
        
        [Header("Race State")]
        public bool isGameActive = false;
        public float raceTime = 0f;
        public int score = 0;
        public bool hasWon = false;
        public int shardsCollected = 0;
        public int totalShards = 0;
        
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
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateScore(score);
            }
        }

        public void RegisterShard()
        {
            totalShards++;
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateShards(shardsCollected, totalShards);
            }
        }

        public void AddCollectedShard(int points = 50)
        {
            shardsCollected++;
            AddScore(points);

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateShards(shardsCollected, totalShards);
                GameUIManager.Instance.ShowNotification($"🔷 SHARD ACQUIRED! ({shardsCollected}/{totalShards}) +{points} PTS", 1.8f);
            }
        }

        public void OnCourseCompleted()
        {
            if (hasWon) return;

            hasWon = true;
            isGameActive = false;
            ChangeState(GameState.Finish);

            if (GameUIManager.Instance != null)
            {
                int stars = 1;
                float shardRatio = (totalShards > 0) ? (float)shardsCollected / totalShards : 0f;
                if (raceTime <= 65f && shardRatio >= 0.5f)
                {
                    stars = 3;
                }
                else if (raceTime <= 85f || shardRatio >= 0.3f)
                {
                    stars = 2;
                }

                GameUIManager.Instance.ShowVictoryScreen(raceTime, score, stars, shardsCollected, totalShards);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayVictorySound();
            }
        }

        public void OnPlayerDied()
        {
            isGameActive = false;
            Debug.Log("Player eliminated!");

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowGameOverScreen("PLAYER ELIMINATED");
            }
        }

        public void OnVehicleDestroyed()
        {
            isGameActive = false;
            Debug.Log("Vehicle destroyed!");

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowGameOverScreen("VEHICLE DESTROYED");
            }
        }

        public void ResetSession()
        {
            score = 0;
            raceTime = 0f;
            isGameActive = false;
            hasWon = false;
            shardsCollected = 0;
            totalShards = 0;
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
