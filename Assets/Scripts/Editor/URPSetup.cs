// Module: URP Setup Tool   Owner: Member 1   Purpose: Configure URP rendering and post-processing for neon cyberpunk visuals

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

namespace NeonRush
{
    /// <summary>
    /// Editor tool to create URP assets and configure rendering for Neon Rush
    /// </summary>
    public class URPSetup : EditorWindow
    {
        [MenuItem("Tools/Neon Rush/Setup URP Rendering")]
        public static void SetupURPAssets()
        {
            // Create URP asset if needed
            string urpAssetPath = "Assets/Settings/NeonRush_UniversalRenderPipelineAsset.asset";
            string urpRendererPath = "Assets/Settings/NeonRush_UniversalRenderer.asset";
            
            if (!System.IO.Directory.Exists("Assets/Settings"))
            {
                System.IO.Directory.CreateDirectory("Assets/Settings");
            }

            UniversalRenderPipelineAsset urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(urpAssetPath);
            
            if (urpAsset == null)
            {
                // Create renderer first
                var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, urpRendererPath);
                
                // Create URP asset
                urpAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                AssetDatabase.CreateAsset(urpAsset, urpAssetPath);
                
                Debug.Log($"Created URP assets at {urpAssetPath}");
            }

            // Assign to Graphics settings
            GraphicsSettings.defaultRenderPipeline = urpAsset;
            QualitySettings.renderPipeline = urpAsset;
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("✅ URP rendering configured successfully");
        }

        [MenuItem("Tools/Neon Rush/Create Post-Processing Volume")]
        public static void CreatePostProcessingVolume()
        {
            // Create a global volume for post-processing
            GameObject volumeObj = new GameObject("PostProcessing Volume");
            Volume volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;

            // Create volume profile
            string profilePath = "Assets/Settings/NeonRush_PostProcessing.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
                
                // Add post-processing effects for neon cyberpunk look
                
                // Bloom for neon glow
                if (!profile.Has<Bloom>())
                {
                    var bloom = profile.Add<Bloom>();
                    bloom.intensity.value = 0.5f;
                    bloom.intensity.overrideState = true;
                    bloom.threshold.value = 0.8f;
                    bloom.threshold.overrideState = true;
                    bloom.scatter.value = 0.7f;
                    bloom.scatter.overrideState = true;
                }

                // Tonemapping for HDR
                if (!profile.Has<Tonemapping>())
                {
                    var tonemapping = profile.Add<Tonemapping>();
                    tonemapping.mode.value = TonemappingMode.ACES;
                    tonemapping.mode.overrideState = true;
                }

                // Vignette for focus
                if (!profile.Has<Vignette>())
                {
                    var vignette = profile.Add<Vignette>();
                    vignette.intensity.value = 0.25f;
                    vignette.intensity.overrideState = true;
                    vignette.smoothness.value = 0.4f;
                    vignette.smoothness.overrideState = true;
                }

                // Color adjustments for neon look
                if (!profile.Has<ColorAdjustments>())
                {
                    var colorAdj = profile.Add<ColorAdjustments>();
                    colorAdj.contrast.value = 10f;
                    colorAdj.contrast.overrideState = true;
                    colorAdj.saturation.value = 15f;
                    colorAdj.saturation.overrideState = true;
                }

                // Chromatic aberration for speed feel
                if (!profile.Has<ChromaticAberration>())
                {
                    var chrom = profile.Add<ChromaticAberration>();
                    chrom.intensity.value = 0.3f;
                    chrom.intensity.overrideState = true;
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"Created post-processing profile at {profilePath}");
            }

            volume.profile = profile;
            volume.sharedProfile = profile;

            Selection.activeGameObject = volumeObj;
            EditorGUIUtility.PingObject(volumeObj);
            
            Debug.Log("✅ Post-processing volume created with neon cyberpunk settings");
        }

        [MenuItem("Tools/Neon Rush/Create Neon Skybox")]
        public static void CreateNeonSkybox()
        {
            // Create gradient skybox material
            string skyboxPath = "Assets/Materials/NeonSkybox.mat";
            Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>(skyboxPath);
            
            if (skyboxMat == null)
            {
                skyboxMat = new Material(Shader.Find("Skybox/Gradient"));
                AssetDatabase.CreateAsset(skyboxMat, skyboxPath);
                
                // Dark purple to cyan gradient (cyberpunk night sky)
                skyboxMat.SetColor("_Top", new Color(0.2f, 0.0f, 0.4f, 1f)); // Dark purple
                skyboxMat.SetColor("_Middle", new Color(0.1f, 0.1f, 0.3f, 1f)); // Deep blue
                skyboxMat.SetColor("_Bottom", new Color(0.0f, 0.2f, 0.3f, 1f)); // Dark cyan
                skyboxMat.SetFloat("_Exponent", 1.5f);
                
                AssetDatabase.SaveAssets();
                Debug.Log($"Created neon skybox at {skyboxPath}");
            }

            // Apply to scene
            RenderSettings.skybox = skyboxMat;
            DynamicGI.UpdateEnvironment();
            
            Debug.Log("✅ Neon skybox created and applied");
        }
    }
}
#endif
