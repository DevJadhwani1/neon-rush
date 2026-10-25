#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NeonRush.EditorTools
{
    [InitializeOnLoad]
    public static class PlayModeStartSceneEnforcer
    {
        private const string MAIN_MENU_SCENE_PATH = "Assets/Scenes/MainMenu.unity";

        static PlayModeStartSceneEnforcer()
        {
            EnforceStartScene();
        }

        [MenuItem("Tools/Neon Rush/Set MainMenu as PlayMode Start Scene")]
        public static void EnforceStartScene()
        {
            var mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MAIN_MENU_SCENE_PATH);
            if (mainScene != null)
            {
                if (EditorSceneManager.playModeStartScene != mainScene)
                {
                    EditorSceneManager.playModeStartScene = mainScene;
                    Debug.Log($"[PlayModeStartSceneEnforcer] Successfully set EditorSceneManager.playModeStartScene to '{MAIN_MENU_SCENE_PATH}'");
                }
            }
            else
            {
                Debug.LogWarning($"[PlayModeStartSceneEnforcer] Could not locate start scene at '{MAIN_MENU_SCENE_PATH}'");
            }
        }
    }
}
#endif
