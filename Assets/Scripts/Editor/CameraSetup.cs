// Module: Camera Setup   Owner: Member 1   Purpose: Create Cinemachine cameras for dynamic racing views

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;

namespace NeonRush
{
    /// <summary>
    /// Editor tool to create Cinemachine virtual cameras for racing
    /// </summary>
    public class CameraSetup : EditorWindow
    {
        [MenuItem("Tools/Neon Rush/Setup Race Cameras")]
        public static void SetupRaceCameras()
        {
            // Find player ship
            ShipInput playerInput = FindAnyObjectByType<ShipInput>();
            Transform playerShip = playerInput != null ? playerInput.transform : null;

            if (playerShip == null)
            {
                Debug.LogWarning("No player ship found. Instantiate a ship prefab with ShipInput first.");
                return;
            }

            // Create Cinemachine brain on main camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
            }

            // Create virtual camera container
            GameObject vcamContainer = new GameObject("VirtualCameras");

            // 1. Chase Camera (main racing view)
            CreateChaseCamera(vcamContainer.transform, playerShip);

            // 2. Cockpit Camera (first-person view)
            CreateCockpitCamera(vcamContainer.transform, playerShip);

            // 3. Cinematic Camera (track overview)
            CreateCinematicCamera(vcamContainer.transform, playerShip);

            Selection.activeGameObject = vcamContainer;
            Debug.Log("✅ Race cameras created with Cinemachine");
        }

        static void CreateChaseCamera(Transform parent, Transform target)
        {
            GameObject vcamObj = new GameObject("CM_ChaseCamera");
            vcamObj.transform.SetParent(parent);

            CinemachineCamera vcam = vcamObj.AddComponent<CinemachineCamera>();
            vcam.Priority.Value = 10; // Highest priority (active by default)
            vcam.Follow = target;
            vcam.LookAt = target;

            // Add Transposer for follow
            CinemachineFollow follow = vcamObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 3f, -8f);
            follow.Damping = new Vector3(0.5f, 0.5f, 0.5f);

            // Add rotation composer for look
            CinemachineRotationComposer rotComposer = vcamObj.AddComponent<CinemachineRotationComposer>();
            rotComposer.Damping = new Vector3(0.5f, 0.5f, 0.5f);

            Debug.Log("Created chase camera (priority 10)");
        }

        static void CreateCockpitCamera(Transform parent, Transform target)
        {
            GameObject vcamObj = new GameObject("CM_CockpitCamera");
            vcamObj.transform.SetParent(parent);

            CinemachineCamera vcam = vcamObj.AddComponent<CinemachineCamera>();
            vcam.Priority.Value = 5; // Lower priority (switch with input)
            vcam.Follow = target;
            vcam.LookAt = target;

            // Add Transposer for cockpit position
            CinemachineFollow follow = vcamObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 0.8f, 0.5f);
            follow.Damping = new Vector3(0.1f, 0.1f, 0.1f);

            // Add rotation composer
            CinemachineRotationComposer rotComposer = vcamObj.AddComponent<CinemachineRotationComposer>();
            rotComposer.Damping = new Vector3(0.1f, 0.1f, 0.1f);

            // Wider FOV for cockpit
            vcam.Lens.FieldOfView = 90f;

            Debug.Log("Created cockpit camera (priority 5)");
        }

        static void CreateCinematicCamera(Transform parent, Transform target)
        {
            GameObject vcamObj = new GameObject("CM_CinematicCamera");
            vcamObj.transform.SetParent(parent);
            vcamObj.transform.position = new Vector3(0f, 50f, 0f);
            vcamObj.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            CinemachineCamera vcam = vcamObj.AddComponent<CinemachineCamera>();
            vcam.Priority.Value = 1; // Lowest priority
            vcam.Follow = target;

            // Add Transposer for cinematic high angle
            CinemachineFollow follow = vcamObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 30f, -20f);
            follow.Damping = new Vector3(1f, 1f, 1f);

            Debug.Log("Created cinematic camera (priority 1)");
        }
    }
}
#endif
