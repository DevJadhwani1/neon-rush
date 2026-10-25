// Module: Core   Owner: Member 3   Purpose: Async scene loading with fade transitions

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace NeonRush
{
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;
        private CanvasGroup fadeCanvasGroup;
        private bool isLoading = false;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                CreateFadeOverlay();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void CreateFadeOverlay()
        {
            // Create a Canvas for fade overlay
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create black panel
            GameObject panelObj = new GameObject("FadePanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            UnityEngine.UI.Image img = panelObj.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.black;
            
            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            fadeCanvasGroup = panelObj.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        public static void LoadMainMenu()
        {
            Debug.Log("[NeonRush] SceneLoader.LoadMainMenu() called");
            if (instance != null && !instance.isLoading)
            {
                instance.StartCoroutine(instance.LoadSceneAsync("MainMenu"));
            }
            else if (instance == null)
            {
                Debug.LogWarning("[NeonRush] SceneLoader instance is null, loading directly");
                SceneManager.LoadScene("MainMenu");
            }
        }

        public static void LoadRace()
        {
            Debug.Log("[NeonRush] SceneLoader.LoadRace() called");
            if (instance != null && !instance.isLoading)
            {
                instance.StartCoroutine(instance.LoadSceneAsync("GravShiftNeonCircuit", true));
            }
            else if (instance == null)
            {
                Debug.LogWarning("[NeonRush] SceneLoader instance is null, loading directly");
                SceneManager.LoadScene(1);
            }
        }

        IEnumerator LoadSceneAsync(string sceneName, bool isRaceScene = false)
        {
            isLoading = true;
            Debug.Log($"[NeonRush] Starting scene load: {sceneName}");
            
            // Fade to black
            yield return FadeOut(0.5f);
            
            // Start loading
            AsyncOperation op = null;
            try
            {
                if (isRaceScene)
                {
                    Debug.Log("[NeonRush] Loading race scene at index 1");
                    op = SceneManager.LoadSceneAsync(1);
                }
                else
                {
                    Debug.Log($"[NeonRush] Loading scene by name: {sceneName}");
                    op = SceneManager.LoadSceneAsync(sceneName);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NeonRush] Exception starting scene load: {ex.Message}");
            }
            
            if (op != null)
            {
                op.allowSceneActivation = false;
                
                // Wait until loaded
                while (op.progress < 0.9f)
                {
                    yield return null;
                }
                
                Debug.Log("[NeonRush] Scene loaded, activating");
                
                // Activate scene
                op.allowSceneActivation = true;
                
                yield return new WaitForSeconds(0.2f);
                Debug.Log("[NeonRush] Scene activated");
            }
            else
            {
                Debug.LogError("[NeonRush] Failed to start scene load operation! Recovering from black screen.");
                yield return FadeIn(0.25f);
                isLoading = false;
                yield break;
            }
            
            // Fade from black
            yield return FadeIn(0.5f);
            
            isLoading = false;
            Debug.Log("[NeonRush] Scene load complete");
        }

        IEnumerator FadeOut(float duration)
        {
            if (fadeCanvasGroup == null) yield break;
            
            fadeCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 1f;
        }

        IEnumerator FadeIn(float duration)
        {
            if (fadeCanvasGroup == null) yield break;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}
