using UnityEngine;

public class CarVisuals : MonoBehaviour
{
    private Transform visualRoot;
    private Transform chassis;
    private Transform[] wheels = new Transform[4];
    private Transform[] exhausts = new Transform[2];
    private Light[] exhaustLights = new Light[2];
    private VehicleController vehicleCtrl;
    private Rigidbody rb;

    private Material bodyMat;
    private Material glassMat;
    private Material metalMat;
    private Material cyanGlowMat;
    private Material magentaGlowMat;

    void Awake()
    {
        vehicleCtrl = GetComponent<VehicleController>();
        rb = GetComponent<Rigidbody>();
        LoadMaterials();
        BuildCarModel();
    }

    void LoadMaterials()
    {
        bodyMat = Resources.Load<Material>("VehicleBodyMaterial") ?? LoadMatFromPath("Assets/Materials/VehicleBodyMaterial.mat");
        glassMat = LoadMatFromPath("Assets/Materials/GlassMaterial.mat");
        metalMat = LoadMatFromPath("Assets/Materials/DarkMetalMaterial.mat");
        cyanGlowMat = LoadMatFromPath("Assets/Materials/NeonCyanGlow.mat");
        magentaGlowMat = LoadMatFromPath("Assets/Materials/NeonMagentaGlow.mat");
    }

    Material LoadMatFromPath(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
#else
        return null;
#endif
    }

    void SafeDestroy(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    public void BuildCarModel()
    {
        // 1. Remove old primitive placeholder children
        string[] oldNames = { "Body", "Cockpit", "Wing_Left", "Wing_Right", "VisualRoot" };
        foreach (string name in oldNames)
        {
            Transform t = transform.Find(name);
            if (t != null) SafeDestroy(t.gameObject);
        }

        // 2. Create VisualRoot to allow dynamic body roll/bank on turns
        GameObject vrObj = new GameObject("VisualRoot");
        vrObj.transform.SetParent(transform, false);
        vrObj.transform.localPosition = Vector3.zero;
        visualRoot = vrObj.transform;

        // 3. Main Chassis (Sleek sports car wedge)
        GameObject chObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chObj.name = "Chassis";
        chObj.transform.SetParent(visualRoot, false);
        chObj.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        chObj.transform.localScale = new Vector3(2.2f, 0.45f, 4.0f);
        SetMat(chObj, bodyMat);
        RemoveCollider(chObj);
        chassis = chObj.transform;

        // 4. Front Hood & Splitter (Aggressive tapered nose)
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "FrontNose";
        nose.transform.SetParent(chassis, false);
        nose.transform.localPosition = new Vector3(0f, -0.15f, 0.55f);
        nose.transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);
        nose.transform.localScale = new Vector3(0.95f, 0.55f, 0.45f);
        SetMat(nose, bodyMat);
        RemoveCollider(nose);

        GameObject splitter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        splitter.name = "FrontSplitter";
        splitter.transform.SetParent(chassis, false);
        splitter.transform.localPosition = new Vector3(0f, -0.45f, 0.65f);
        splitter.transform.localScale = new Vector3(1.05f, 0.15f, 0.35f);
        SetMat(splitter, metalMat);
        RemoveCollider(splitter);

        // 5. Cockpit & Tinted Glass Canopy
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Canopy";
        cabin.transform.SetParent(chassis, false);
        cabin.transform.localPosition = new Vector3(0f, 0.65f, 0.05f);
        cabin.transform.localScale = new Vector3(0.68f, 0.75f, 0.48f);
        SetMat(cabin, glassMat ?? metalMat);
        RemoveCollider(cabin);

        // Windshield slope
        GameObject windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
        windshield.name = "Windshield";
        windshield.transform.SetParent(chassis, false);
        windshield.transform.localPosition = new Vector3(0f, 0.45f, 0.35f);
        windshield.transform.localRotation = Quaternion.Euler(-32f, 0f, 0f);
        windshield.transform.localScale = new Vector3(0.66f, 0.55f, 0.35f);
        SetMat(windshield, glassMat ?? metalMat);
        RemoveCollider(windshield);

        // Roof scoop
        GameObject scoop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        scoop.name = "RoofScoop";
        scoop.transform.SetParent(chassis, false);
        scoop.transform.localPosition = new Vector3(0f, 1.05f, -0.05f);
        scoop.transform.localScale = new Vector3(0.25f, 0.25f, 0.35f);
        SetMat(scoop, metalMat);
        RemoveCollider(scoop);

        // 6. Neon Headlights (Front Left & Right)
        CreateHeadlight(chassis, new Vector3(-0.38f, 0.05f, 0.62f), -12f);
        CreateHeadlight(chassis, new Vector3(0.38f, 0.05f, 0.62f), 12f);

        // 7. Full-Width Glowing Rear Tail Light
        GameObject tailLight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tailLight.name = "TailLightBar";
        tailLight.transform.SetParent(chassis, false);
        tailLight.transform.localPosition = new Vector3(0f, 0.1f, -0.52f);
        tailLight.transform.localScale = new Vector3(0.9f, 0.15f, 0.08f);
        SetMat(tailLight, magentaGlowMat ?? cyanGlowMat);
        RemoveCollider(tailLight);

        // 8. Dual Jet Thruster Exhausts
        exhausts[0] = CreateExhaust(chassis, new Vector3(-0.25f, -0.08f, -0.54f), out exhaustLights[0]);
        exhausts[1] = CreateExhaust(chassis, new Vector3(0.25f, -0.08f, -0.54f), out exhaustLights[1]);

        // 9. Rear Racing Spoiler
        GameObject strutL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strutL.transform.SetParent(chassis, false);
        strutL.transform.localPosition = new Vector3(-0.35f, 0.55f, -0.42f);
        strutL.transform.localScale = new Vector3(0.06f, 0.65f, 0.15f);
        SetMat(strutL, metalMat);
        RemoveCollider(strutL);

        GameObject strutR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strutR.transform.SetParent(chassis, false);
        strutR.transform.localPosition = new Vector3(0.35f, 0.55f, -0.42f);
        strutR.transform.localScale = new Vector3(0.06f, 0.65f, 0.15f);
        SetMat(strutR, metalMat);
        RemoveCollider(strutR);

        GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wing.name = "SpoilerWing";
        wing.transform.SetParent(chassis, false);
        wing.transform.localPosition = new Vector3(0f, 0.95f, -0.45f);
        wing.transform.localRotation = Quaternion.Euler(-5f, 0f, 0f);
        wing.transform.localScale = new Vector3(1.15f, 0.12f, 0.35f);
        SetMat(wing, metalMat);
        RemoveCollider(wing);

        // 10. 4 Wide Racing Wheels / Hover Thruster Pods
        wheels[0] = CreateWheel(visualRoot, "Wheel_FL", new Vector3(-1.22f, 0.32f, 1.35f));
        wheels[1] = CreateWheel(visualRoot, "Wheel_FR", new Vector3(1.22f, 0.32f, 1.35f));
        wheels[2] = CreateWheel(visualRoot, "Wheel_RL", new Vector3(-1.25f, 0.38f, -1.35f), 1.15f);
        wheels[3] = CreateWheel(visualRoot, "Wheel_RR", new Vector3(1.25f, 0.38f, -1.35f), 1.15f);

        // 11. Seat & Exit points
        Transform seat = transform.Find("DriverSeat");
        if (seat == null)
        {
            GameObject sObj = new GameObject("DriverSeat");
            sObj.transform.SetParent(transform, false);
            sObj.transform.localPosition = new Vector3(0f, 0.5f, 0.1f);
        }
        else
        {
            seat.localPosition = new Vector3(0f, 0.5f, 0.1f);
        }

        Transform exit = transform.Find("ExitPoint");
        if (exit == null)
        {
            GameObject eObj = new GameObject("ExitPoint");
            eObj.transform.SetParent(transform, false);
            eObj.transform.localPosition = new Vector3(-2.4f, 0f, 0f);
        }
    }

    void CreateHeadlight(Transform parent, Vector3 localPos, float angleY)
    {
        GameObject hl = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hl.name = "HeadlightMesh";
        hl.transform.SetParent(parent, false);
        hl.transform.localPosition = localPos;
        hl.transform.localRotation = Quaternion.Euler(0f, angleY, 0f);
        hl.transform.localScale = new Vector3(0.22f, 0.12f, 0.18f);
        SetMat(hl, cyanGlowMat);
        RemoveCollider(hl);
    }

    Transform CreateExhaust(Transform parent, Vector3 localPos, out Light flareLight)
    {
        GameObject ex = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ex.name = "ExhaustThruster";
        ex.transform.SetParent(parent, false);
        ex.transform.localPosition = localPos;
        ex.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ex.transform.localScale = new Vector3(0.24f, 0.22f, 0.24f);
        SetMat(ex, cyanGlowMat);
        RemoveCollider(ex);

        GameObject lObj = new GameObject("ExhaustGlow");
        lObj.transform.SetParent(ex.transform, false);
        lObj.transform.localPosition = new Vector3(0f, -0.6f, 0f);
        flareLight = lObj.AddComponent<Light>();
        flareLight.type = LightType.Point;
        flareLight.range = 3.5f;
        flareLight.intensity = 2.5f;
        flareLight.color = new Color(0f, 0.9f, 1f);

        return ex.transform;
    }

    Transform CreateWheel(Transform parent, string name, Vector3 localPos, float scaleMult = 1f)
    {
        GameObject wObj = new GameObject(name);
        wObj.transform.SetParent(parent, false);
        wObj.transform.localPosition = localPos;

        GameObject tire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tire.name = "Tire";
        tire.transform.SetParent(wObj.transform, false);
        tire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        tire.transform.localScale = new Vector3(0.72f * scaleMult, 0.24f, 0.72f * scaleMult);
        SetMat(tire, metalMat);
        RemoveCollider(tire);

        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "NeonRim";
        rim.transform.SetParent(wObj.transform, false);
        rim.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        rim.transform.localScale = new Vector3(0.48f * scaleMult, 0.26f, 0.48f * scaleMult);
        SetMat(rim, cyanGlowMat);
        RemoveCollider(rim);

        return wObj.transform;
    }

    void SetMat(GameObject obj, Material mat)
    {
        if (mat == null) return;
        MeshRenderer r = obj.GetComponent<MeshRenderer>();
        if (r != null) r.sharedMaterial = mat;
    }

    void RemoveCollider(GameObject obj)
    {
        Collider c = obj.GetComponent<Collider>();
        if (c != null) SafeDestroy(c);
    }

    void Update()
    {
        if (visualRoot == null) return;

        float forwardSpeed = (vehicleCtrl != null) ? vehicleCtrl.CurrentSpeed : 0f;
        bool isHovering = (vehicleCtrl != null && vehicleCtrl.isAntiGravityActive);

        // 1. Wheel Rotation (spin when moving on ground)
        if (!isHovering)
        {
            float wheelSpin = forwardSpeed * 45f * Time.deltaTime;
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] != null) wheels[i].Rotate(Vector3.right, wheelSpin, Space.Self);
            }
        }
        else
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] != null)
                {
                    float targetZAngle = (i % 2 == 0) ? -25f : 25f;
                    Quaternion targetRot = Quaternion.Euler(0f, 0f, targetZAngle);
                    wheels[i].localRotation = Quaternion.Slerp(wheels[i].localRotation, targetRot, 6f * Time.deltaTime);
                }
            }
        }

        // 2. Chassis Banking into Turns
        float turnInput = GameInput.GetHorizontal();
        float targetRoll = -turnInput * 6.5f;
        float targetPitch = (forwardSpeed > 2f) ? -1.5f : (forwardSpeed < -1f ? 2.5f : 0f);

        if (isHovering)
        {
            targetRoll = -turnInput * 12f;
            targetPitch = -3f;
        }

        Quaternion desiredChassisRot = Quaternion.Euler(targetPitch, 0f, targetRoll);
        chassis.localRotation = Quaternion.Slerp(chassis.localRotation, desiredChassisRot, 8f * Time.deltaTime);

        // 3. Exhaust thruster dynamic flares
        float flareIntensity = Mathf.Clamp(Mathf.Abs(forwardSpeed) / 12f, 0.5f, 3.5f);
        if (isHovering) flareIntensity = 5.5f;

        Color thrusterColor = isHovering ? new Color(1f, 0.1f, 0.85f) : new Color(0f, 0.9f, 1f);
        for (int i = 0; i < exhaustLights.Length; i++)
        {
            if (exhaustLights[i] != null)
            {
                exhaustLights[i].intensity = flareIntensity;
                exhaustLights[i].color = thrusterColor;
                exhaustLights[i].range = isHovering ? 6f : 3.5f;
            }
        }
    }
}
