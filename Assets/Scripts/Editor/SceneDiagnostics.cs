#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonRush.EditorTools
{
    public static class SceneDiagnostics
    {
        public static void DiagnoseScenes()
        {
            Debug.Log("=== SCENE DIAGNOSTICS START ===");
            Debug.Log($"SceneManager.sceneCountInBuildSettings: {SceneManager.sceneCountInBuildSettings}");
            
            Debug.Log($"EditorBuildSettings.scenes count: {EditorBuildSettings.scenes.Length}");
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var s = EditorBuildSettings.scenes[i];
                Debug.Log($"EditorBuildSettings[{i}]: path='{s.path}', enabled={s.enabled}, guid={s.guid}");
            }

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                Debug.Log($"BuildIndex[{i}] -> ScenePath='{path}'");
            }

            int indexFromName = SceneUtility.GetBuildIndexByScenePath("GravShiftNeonCircuit");
            Debug.Log($"GetBuildIndexByScenePath('GravShiftNeonCircuit'): {indexFromName}");

            int indexFromFullName = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GravShiftNeonCircuit.unity");
            Debug.Log($"GetBuildIndexByScenePath('Assets/Scenes/GravShiftNeonCircuit.unity'): {indexFromFullName}");

            // Try setting EditorBuildSettings.scenes
            var newScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GravShiftNeonCircuit.unity", true)
            };
            EditorBuildSettings.scenes = newScenes;
            Debug.Log($"After assignment, EditorBuildSettings.scenes count: {EditorBuildSettings.scenes.Length}");
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var s = EditorBuildSettings.scenes[i];
                Debug.Log($"After assign EditorBuildSettings[{i}]: path='{s.path}', enabled={s.enabled}, guid={s.guid}");
            }
            Debug.Log($"After assign SceneManager.sceneCountInBuildSettings: {SceneManager.sceneCountInBuildSettings}");
            Debug.Log($"After assign GetBuildIndexByScenePath('Assets/Scenes/GravShiftNeonCircuit.unity'): {SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GravShiftNeonCircuit.unity")}");
            Debug.Log($"After assign GetBuildIndexByScenePath('GravShiftNeonCircuit'): {SceneUtility.GetBuildIndexByScenePath("GravShiftNeonCircuit")}");

            // Also inspect BuildProfile API members
            var buildProfileType = System.Type.GetType("UnityEditor.Build.Profile.BuildProfile, UnityEditor");
            if (buildProfileType != null)
            {
                Debug.Log("BuildProfile methods/props:");
                foreach (var p in buildProfileType.GetProperties())
                {
                    Debug.Log($"Prop: {p.Name} ({p.PropertyType})");
                }
                foreach (var m in buildProfileType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    Debug.Log($"Static Method: {m.Name}");
                }
            }

            Debug.Log("=== SCENE DIAGNOSTICS END ===");
        }
    }
}
#endif
