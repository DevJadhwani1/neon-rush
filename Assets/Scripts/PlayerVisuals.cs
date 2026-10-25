using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    private Transform visualRoot;
    private Material playerMat;
    private Material metalMat;
    private Material goldVisorMat;
    private Material cyanGlowMat;

    void Awake()
    {
        LoadMaterials();
        BuildPlayerModel();
    }

    void LoadMaterials()
    {
        playerMat = LoadMat("PlayerMaterial", new Color(0f, 0.7f, 0.9f), Color.black, 0.4f, 0.6f);
        metalMat = LoadMat("DarkMetalMaterial", new Color(0.2f, 0.22f, 0.28f), Color.black, 0.8f, 0.7f);
        goldVisorMat = LoadMat("GoldVisorMaterial", new Color(1f, 0.85f, 0.2f), new Color(1f, 0.85f, 0.2f) * 1.5f, 0.9f, 0.9f);
        cyanGlowMat = LoadMat("NeonCyanGlow", new Color(0f, 0.95f, 1f), new Color(0f, 0.95f, 1f) * 2.5f);
    }

    Material LoadMat(string matName, Color fallbackColor, Color fallbackEmission, float metallic = 0.5f, float smoothness = 0.5f)
    {
        Material m = Resources.Load<Material>($"Materials/{matName}") ?? Resources.Load<Material>(matName);
#if UNITY_EDITOR
        if (m == null) m = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{matName}.mat");
#endif
        if (m == null)
        {
            Shader s = Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            if (s != null)
            {
                m = new Material(s);
                m.name = matName;
                m.color = fallbackColor;
                m.SetFloat("_Metallic", metallic);
                m.SetFloat("_Glossiness", smoothness);
                if (fallbackEmission.maxColorComponent > 0.01f)
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", fallbackEmission);
                }
            }
        }
        return m;
    }

    void SafeDestroy(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }

    public void BuildPlayerModel()
    {
        MeshRenderer baseRenderer = GetComponent<MeshRenderer>();
        if (baseRenderer != null) baseRenderer.enabled = false;

        Transform oldRoot = transform.Find("PlayerVisualRoot");
        if (oldRoot != null) SafeDestroy(oldRoot.gameObject);

        GameObject vrObj = new GameObject("PlayerVisualRoot");
        vrObj.transform.SetParent(transform, false);
        vrObj.transform.localPosition = Vector3.zero;
        visualRoot = vrObj.transform;

        // Helmet & Glowing Visor (FRONT)
        GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        helmet.name = "Helmet";
        helmet.transform.SetParent(visualRoot, false);
        helmet.transform.localPosition = new Vector3(0f, 1.48f, 0f);
        helmet.transform.localScale = new Vector3(0.48f, 0.52f, 0.48f);
        SetMat(helmet, metalMat);
        RemoveCollider(helmet);

        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "Visor_FRONT";
        visor.transform.SetParent(helmet.transform, false);
        visor.transform.localPosition = new Vector3(0f, 0.04f, 0.42f);
        visor.transform.localScale = new Vector3(0.72f, 0.42f, 0.35f);
        SetMat(visor, goldVisorMat ?? cyanGlowMat);
        RemoveCollider(visor);

        GameObject hlObj = new GameObject("HelmetLight");
        hlObj.transform.SetParent(helmet.transform, false);
        hlObj.transform.localPosition = new Vector3(0f, 0.1f, 0.55f);
        Light hl = hlObj.AddComponent<Light>();
        hl.type = LightType.Spot;
        hl.range = 15f;
        hl.spotAngle = 55f;
        hl.intensity = 2.0f;
        hl.color = new Color(1f, 0.95f, 0.8f);

        // Torso Armor & Core (FRONT)
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso";
        torso.transform.SetParent(visualRoot, false);
        torso.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        torso.transform.localScale = new Vector3(0.62f, 0.65f, 0.36f);
        SetMat(torso, playerMat);
        RemoveCollider(torso);

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        core.name = "ChestCore_FRONT";
        core.transform.SetParent(torso.transform, false);
        core.transform.localPosition = new Vector3(0f, 0.18f, 0.52f);
        core.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        core.transform.localScale = new Vector3(0.28f, 0.12f, 0.28f);
        SetMat(core, cyanGlowMat);
        RemoveCollider(core);

        // Twin Jetpack (BACK)
        GameObject jetpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        jetpack.name = "Jetpack_BACK";
        jetpack.transform.SetParent(torso.transform, false);
        jetpack.transform.localPosition = new Vector3(0f, 0.05f, -0.6f);
        jetpack.transform.localScale = new Vector3(0.75f, 0.75f, 0.35f);
        SetMat(jetpack, metalMat);
        RemoveCollider(jetpack);

        CreateJetNozzle(jetpack.transform, new Vector3(-0.25f, -0.45f, 0f));
        CreateJetNozzle(jetpack.transform, new Vector3(0.25f, -0.45f, 0f));

        // Limbs
        CreateLimb(visualRoot, "Arm_L", new Vector3(-0.42f, 0.92f, 0f), new Vector3(0.18f, 0.58f, 0.18f), playerMat);
        CreateLimb(visualRoot, "Arm_R", new Vector3(0.42f, 0.92f, 0f), new Vector3(0.18f, 0.58f, 0.18f), playerMat);
        CreateLimb(visualRoot, "Leg_L", new Vector3(-0.18f, 0.35f, 0f), new Vector3(0.22f, 0.68f, 0.22f), metalMat);
        CreateLimb(visualRoot, "Leg_R", new Vector3(0.18f, 0.35f, 0f), new Vector3(0.22f, 0.68f, 0.22f), metalMat);

        VehicleInteraction vi = FindAnyObjectByType<VehicleInteraction>();
        if (vi != null && vi.isInVehicle)
        {
            Renderer[] allRenderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var r in allRenderers)
            {
                if (r != null) r.enabled = false;
            }
            Light[] allLights = visualRoot.GetComponentsInChildren<Light>(true);
            foreach (var l in allLights)
            {
                if (l != null) l.enabled = false;
            }
        }
    }

    void CreateJetNozzle(Transform parent, Vector3 localPos)
    {
        GameObject n = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        n.name = "JetNozzle";
        n.transform.SetParent(parent, false);
        n.transform.localPosition = localPos;
        n.transform.localScale = new Vector3(0.22f, 0.25f, 0.22f);
        SetMat(n, cyanGlowMat);
        RemoveCollider(n);
    }

    void CreateLimb(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        limb.name = name;
        limb.transform.SetParent(parent, false);
        limb.transform.localPosition = localPos;
        limb.transform.localScale = scale;
        SetMat(limb, mat);
        RemoveCollider(limb);
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
}
