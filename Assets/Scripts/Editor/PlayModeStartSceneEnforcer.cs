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
            EnforceBuildSettings();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                DisableGameViewGizmos();
            }
        }

        [MenuItem("Tools/Neon Rush/Disable Game View Gizmos")]
        public static void DisableGameViewGizmos()
        {
            try
            {
                var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor") ??
                                   System.Type.GetType("UnityEditor.PlayModeView,UnityEditor");
                if (gameViewType != null)
                {
                    var views = Resources.FindObjectsOfTypeAll(gameViewType);
                    foreach (var v in views)
                    {
                        var prop = gameViewType.GetProperty("gizmos", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (prop != null && prop.CanWrite)
                        {
                            prop.SetValue(v, false, null);
                        }
                    }
                }
            }
            catch { }
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

        public static void EnforceBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            bool hasMenu = false;
            bool hasRace = false;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == "Assets/Scenes/MainMenu.unity" && scenes[i].enabled) hasMenu = true;
                if (scenes[i].path == "Assets/Scenes/GravShiftNeonCircuit.unity" && scenes[i].enabled) hasRace = true;
            }

            if (!hasMenu || !hasRace)
            {
                EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
                {
                    new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                    new EditorBuildSettingsScene("Assets/Scenes/GravShiftNeonCircuit.unity", true)
                };
                Debug.Log("[PlayModeStartSceneEnforcer] Enforced EditorBuildSettings scenes for MainMenu and GravShiftNeonCircuit");
            }
        }
    }
}
#endif
