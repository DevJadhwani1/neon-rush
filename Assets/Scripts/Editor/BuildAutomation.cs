#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonRush.EditorTools
{
    public static class BuildAutomation
    {
        [MenuItem("Tools/Neon Rush/Build Standalone OSX")]
        public static void BuildStandaloneOSX()
        {
            string[] scenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/GravShiftNeonCircuit.unity"
            };

            string outputPath = "Builds/OSX/NeonRush.app";

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development
            };

            Debug.Log($">>> Starting StandaloneOSX Development Build to {outputPath}...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($">>> BUILD SUCCESSFUL! Total size: {summary.totalSize} bytes, Duration: {summary.totalTime.TotalSeconds:F1}s");
            }
            else
            {
                Debug.LogError($">>> BUILD FAILED! Errors: {summary.totalErrors}, Warnings: {summary.totalWarnings}");
            }
        }
    }
}
#endif
