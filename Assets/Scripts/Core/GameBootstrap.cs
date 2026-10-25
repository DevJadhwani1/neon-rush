// Module: Core   Owner: Member 3   Purpose: Ensures GameManager exists when entering any scene

using UnityEngine;

namespace NeonRush
{
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            // If GameManager doesn't exist, create it
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
                
                // Also add SceneLoader
                GameObject slObj = new GameObject("SceneLoader");
                slObj.AddComponent<SceneLoader>();
                
                Debug.Log("[GameBootstrap] Created GameManager and SceneLoader.");
            }
            
            // This script can be destroyed after bootstrap
            Destroy(gameObject);
        }
    }
}
