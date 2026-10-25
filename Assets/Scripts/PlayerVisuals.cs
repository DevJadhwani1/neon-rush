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
        playerMat = Resources.Load<Material>("PlayerMaterial") ?? LoadMatFromPath("Assets/Materials/PlayerMaterial.mat");
        metalMat = LoadMatFromPath("Assets/Materials/DarkMetalMaterial.mat");
        goldVisorMat = LoadMatFromPath("Assets/Materials/GoldVisorMaterial.mat");
        cyanGlowMat = LoadMatFromPath("Assets/Materials/NeonCyanGlow.mat");
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
