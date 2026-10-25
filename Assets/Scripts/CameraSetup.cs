using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    void Awake()
    {
        SetupCameras();
    }

    public void SetupCameras()
    {
        GameObject player = GameObject.Find("Player");
        GameObject vehicle = GameObject.Find("Vehicle");
        Camera mainCam = Camera.main;
        GameObject vCamObj = GameObject.Find("VehicleCamera");

        // 1. Setup Main Camera for Player Foot Mode
        if (mainCam != null)
        {
            SimpleCameraFollow mainFollow = mainCam.GetComponent<SimpleCameraFollow>();
            if (mainFollow == null)
            {
                mainFollow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();
            }

            mainFollow.isVehicleCamera = false;
            if (player != null)
            {
                mainFollow.target = player.transform;
            }
            mainFollow.distance = 3.5f;
            mainFollow.height = 2.0f;
            mainFollow.lookAheadDistance = 2.0f;
            mainFollow.lookUpOffset = 1.2f;
            mainFollow.preventWallClipping = true;
            mainFollow.SnapToTarget();
        }

        // 2. Setup Vehicle Camera for Vehicle Racing Mode
        if (vCamObj != null)
        {
            SimpleCameraFollow vFollow = vCamObj.GetComponent<SimpleCameraFollow>();
            if (vFollow == null)
            {
                vFollow = vCamObj.AddComponent<SimpleCameraFollow>();
            }

            vFollow.isVehicleCamera = true;
            if (vehicle != null)
            {
                vFollow.target = vehicle.transform;
            }
            vFollow.distance = 6.0f;
            vFollow.height = 2.2f;
            vFollow.lookAheadDistance = 6.0f;
            vFollow.lookUpOffset = 1.0f;
            vFollow.preventWallClipping = true;
            vFollow.SnapToTarget();
            vCamObj.SetActive(false);
        }
    }
}
