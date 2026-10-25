using UnityEngine;

public class TrackBuilder : MonoBehaviour
{
    private Material trackMat;
    private Material barrierMat;
    private Material hazardMat;
    private Material boostMat;
    private Material cyanGlowMat;
    private Material magentaGlowMat;
    private PhysicsMaterial bouncePhysMat;

    void Awake()
    {
        LoadMaterials();
        BuildFullCircuit();
    }

    void LoadMaterials()
    {
        trackMat = LoadMatFromPath("Assets/Materials/TrackMaterial.mat");
        barrierMat = LoadMatFromPath("Assets/Materials/BarrierNeonMaterial.mat");
        hazardMat = LoadMatFromPath("Assets/Materials/HazardMaterial.mat");
        boostMat = LoadMatFromPath("Assets/Materials/BoostPadMaterial.mat");
        cyanGlowMat = LoadMatFromPath("Assets/Materials/NeonCyanGlow.mat");
        magentaGlowMat = LoadMatFromPath("Assets/Materials/NeonMagentaGlow.mat");

        bouncePhysMat = new PhysicsMaterial("BarrierBounce")
        {
            bounciness = 0.40f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum,
            dynamicFriction = 0.05f,
            staticFriction = 0.05f
        };
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

    public void BuildFullCircuit()
    {
        Transform trackParent = transform.Find("ProceduralTrack");
        if (trackParent != null) SafeDestroy(trackParent.gameObject);

        GameObject rootObj = new GameObject("ProceduralTrack");
        rootObj.transform.SetParent(transform, false);
        Transform root = rootObj.transform;

        // Hide legacy static placeholder elements to prevent overlapping
        string[] oldGroups = { "Track", "Platforms", "Ramps", "StartArea", "FinishArea", "Facility", "VehicleArea" };
        foreach (string groupName in oldGroups)
        {
            Transform t = transform.Find(groupName);
            if (t != null) t.gameObject.SetActive(false);
            GameObject ro = GameObject.Find(groupName);
            if (ro != null && ro != gameObject) ro.SetActive(false);
        }

        // =========================================================================
        // SECTOR 1: LAUNCH HANGAR & HIGH-SPEED SPLIT FORK (Z = -35 to 65, X = 0)
        // =========================================================================
        // Start Hangar bay
        CreateRoadSlab(root, "StartHangar", new Vector3(0f, -0.5f, -15f), new Vector3(24f, 2f, 40f));
        CreateWall(root, "HangarWall_L", new Vector3(-12f, 3.5f, -15f), new Vector3(1f, 6f, 40f));
        CreateWall(root, "HangarWall_R", new Vector3(12f, 3.5f, -15f), new Vector3(1f, 6f, 40f));
        CreateWall(root, "HangarWall_Back", new Vector3(0f, 3.5f, -35f), new Vector3(24f, 6f, 1f));

        // Start Gantry Overhead Lights
        CreateStartGantry(root, new Vector3(0f, 0f, -5f));

        // CHECKPOINT 0: Start Gantry
        CreateCheckpointGate(root, "Checkpoint_00", 0, false, new Vector3(0f, 0f, 0f), new Vector3(20f, 8f, 4f), new Vector3(0f, 0.5f, 0f), Quaternion.identity);

        // Straightaway to Fork
        CreateRoadSlab(root, "Sec1_Straight", new Vector3(0f, -0.5f, 20f), new Vector3(18f, 2f, 32f));
        CreateGuardRail(root, "Rail_Sec1_L", new Vector3(-9f, 1.5f, 20f), new Vector3(0.6f, 3.0f, 32f));
        CreateGuardRail(root, "Rail_Sec1_R", new Vector3(9f, 1.5f, 20f), new Vector3(0.6f, 3.0f, 32f));

        CreateBoostPad(root, new Vector3(0f, 0.52f, 12f), 28f);
        CreateShard(root, new Vector3(0f, 1.5f, 5f));
        CreateShard(root, new Vector3(-3f, 1.5f, 15f));
        CreateShard(root, new Vector3(3f, 1.5f, 25f));

        // EXACTLY TWO ANTI-GRAVITY CELLS — BOTH IN THE STARTING SECTION (ONLY WAY TO EARN MORE HOVER)
        CreateEnergyZone(root, new Vector3(-4.5f, 1.8f, 18f));
        CreateEnergyZone(root, new Vector3(6.5f, 1.8f, 48f));

        // CHECKPOINT 1: Pre-Fork
        CreateCheckpointGate(root, "Checkpoint_01", 1, false, new Vector3(0f, 0f, 35f), new Vector3(20f, 8f, 4f), new Vector3(0f, 0.5f, 35f), Quaternion.identity);

        // Split Road Fork (Z = 35 to 65): Left Lane (Underpass), Right Lane (Bypass)
        CreateRoadSlab(root, "Fork_Left_Lane", new Vector3(-6.5f, -0.5f, 50f), new Vector3(8f, 2f, 30f));
        CreateGuardRail(root, "Rail_Fork_L_Outer", new Vector3(-10.5f, 1.5f, 50f), new Vector3(0.6f, 3.0f, 30f));
        CreateGuardRail(root, "Rail_Fork_Divider", new Vector3(0f, 1.5f, 50f), new Vector3(0.8f, 3.0f, 30f));
        CreateRoadSlab(root, "Fork_Right_Lane", new Vector3(6.5f, -0.5f, 50f), new Vector3(8f, 2f, 30f));
        CreateGuardRail(root, "Rail_Fork_R_Outer", new Vector3(10.5f, 1.5f, 50f), new Vector3(0.6f, 3.0f, 30f));

        // Left lane has rotating sweeper hazard; Right lane has Boost Pad
        CreateRotatingSweeper(root, "ForkSweeper", new Vector3(-6.5f, 0.5f, 50f), 7f, 75f);
        CreateBoostPad(root, new Vector3(6.5f, 0.52f, 50f), 30f);
        CreateShard(root, new Vector3(-6.5f, 1.5f, 42f));
        CreateShard(root, new Vector3(-6.5f, 1.5f, 58f));
        CreateShard(root, new Vector3(6.5f, 1.5f, 58f));

        // CHECKPOINT 2: Fork Exit
        CreateCheckpointGate(root, "Checkpoint_02", 2, false, new Vector3(0f, 0f, 65f), new Vector3(24f, 8f, 4f), new Vector3(0f, 0.5f, 65f), Quaternion.identity);

        // =========================================================================
        // SECTOR 2: DOUBLE HAIRPIN CHICANE & DRIFT CANYON (Z = 65 to 175)
        // =========================================================================
        // Turn 1 Warning Billboard
        CreateTurnChevronSign(root, new Vector3(8f, 1.5f, 62f), 35f, true);

        // Turn 1 (Right Sweep): Curves out from X = 0 to X = 28m at Z = 100m
        CreateRoadSlab(root, "Turn1_Sweep1", new Vector3(5f, -0.5f, 75f), new Vector3(18f, 2f, 22f), 25f);
        CreateGuardRail(root, "Rail_Turn1_Inner1", new Vector3(-4f, 1.5f, 75f), new Vector3(0.6f, 3.0f, 23f), 25f);
        CreateGuardRail(root, "Rail_Turn1_Outer1", new Vector3(14f, 1.5f, 75f), new Vector3(0.6f, 3.0f, 23f), 25f);

        CreateRoadSlab(root, "Turn1_Sweep2", new Vector3(16f, -0.5f, 92f), new Vector3(18f, 2f, 22f), 45f);
        CreateGuardRail(root, "Rail_Turn1_Inner2", new Vector3(6.5f, 1.5f, 92f), new Vector3(0.6f, 3.0f, 23f), 45f);
        CreateGuardRail(root, "Rail_Turn1_Outer2", new Vector3(25.5f, 1.5f, 92f), new Vector3(0.6f, 3.0f, 23f), 45f);

        // Turn 1 Apex Cyber-Pillar & Sweeper
        CreateRotatingSweeper(root, "Apex1_Sweeper", new Vector3(10f, 0.5f, 95f), 9.5f, 60f);

        // 4 Quantum Shards around apex inner line
        CreateShard(root, new Vector3(9f, 1.5f, 82f));
        CreateShard(root, new Vector3(14f, 1.5f, 90f));
        CreateShard(root, new Vector3(18f, 1.5f, 98f));
        CreateShard(root, new Vector3(22f, 1.5f, 106f));

        // Drift straight along X = 28m
        CreateRoadSlab(root, "Chicane_Straight", new Vector3(26f, -0.5f, 115f), new Vector3(18f, 2f, 28f));
        CreateGuardRail(root, "Rail_Chicane_L", new Vector3(17f, 1.5f, 115f), new Vector3(0.6f, 3.0f, 28f));
        CreateGuardRail(root, "Rail_Chicane_R", new Vector3(35f, 1.5f, 115f), new Vector3(0.6f, 3.0f, 28f));
        CreateBoostPad(root, new Vector3(26f, 0.52f, 115f), 30f);
        CreateShard(root, new Vector3(26f, 1.5f, 115f));

        // CHECKPOINT 3: Chicane Straight
        CreateCheckpointGate(root, "Checkpoint_03", 3, false, new Vector3(26f, 0f, 115f), new Vector3(20f, 8f, 4f), new Vector3(26f, 0.5f, 115f), Quaternion.identity);

        // Turn 2 Warning Billboard (Turn Left!)
        CreateTurnChevronSign(root, new Vector3(32f, 1.5f, 126f), -45f, false);

        // Turn 2 (Left Switchback): Sweeps from X = 28 back to X = -12 at Z = 150
        CreateRoadSlab(root, "Turn2_Sweep1", new Vector3(16f, -0.5f, 138f), new Vector3(18f, 2f, 24f), -35f);
        CreateGuardRail(root, "Rail_Turn2_Inner", new Vector3(5.5f, 1.5f, 138f), new Vector3(0.6f, 3.0f, 25f), -35f);
        CreateGuardRail(root, "Rail_Turn2_Outer", new Vector3(26.5f, 1.5f, 138f), new Vector3(0.6f, 3.0f, 25f), -35f);

        // Laser Gate crossing Turn 2 exit
        CreateLaserGate(root, "ChicaneGate", new Vector3(14f, 0f, 142f), 18f);

        CreateRoadSlab(root, "Turn2_Sweep2", new Vector3(2f, -0.5f, 156f), new Vector3(18f, 2f, 22f), -20f);
        CreateGuardRail(root, "Rail_Turn2_Inner2", new Vector3(-7.5f, 1.5f, 156f), new Vector3(0.6f, 3.0f, 23f), -20f);
        CreateGuardRail(root, "Rail_Turn2_Outer2", new Vector3(11.5f, 1.5f, 156f), new Vector3(0.6f, 3.0f, 23f), -20f);

        CreateShard(root, new Vector3(6f, 1.5f, 150f));
        CreateShard(root, new Vector3(0f, 1.5f, 160f));

        // Return curve to center X = 0 at Z = 175
        CreateRoadSlab(root, "Chicane_Exit", new Vector3(0f, -0.5f, 172f), new Vector3(18f, 2f, 18f));
        CreateGuardRail(root, "Rail_Exit_L", new Vector3(-9f, 1.5f, 172f), new Vector3(0.6f, 3.0f, 18f));
        CreateGuardRail(root, "Rail_Exit_R", new Vector3(9f, 1.5f, 172f), new Vector3(0.6f, 3.0f, 18f));

        // CHECKPOINT 4: Chicane Exit
        CreateCheckpointGate(root, "Checkpoint_04", 4, false, new Vector3(0f, 0f, 172f), new Vector3(20f, 8f, 4f), new Vector3(0f, 0.5f, 172f), Quaternion.identity);

        // =========================================================================
        // SECTOR 3: HIGH-ALTITUDE SKY SKYWAY & SUPERSONIC GAP JUMP (Z = 175 to 295)
        // =========================================================================
        // Incline launch ramp from Y = 0 to Y = 11m (Length 34m, angle ~18 deg)
        float rampAngle = -17.5f;
        CreateRoadSlab(root, "SkyInclineRamp", new Vector3(0f, 5.2f, 192f), new Vector3(16f, 2f, 36f), 0f, rampAngle);
        CreateGuardRail(root, "Rail_Ramp_L", new Vector3(-8f, 7.0f, 192f), new Vector3(0.6f, 3.0f, 36f), 0f, rampAngle);
        CreateGuardRail(root, "Rail_Ramp_R", new Vector3(8f, 7.0f, 192f), new Vector3(0.6f, 3.0f, 36f), 0f, rampAngle);

        // High Sky Launch Platform at Y = 11m
        CreateRoadSlab(root, "SkyLaunchDeck", new Vector3(0f, 10.5f, 215f), new Vector3(16f, 2f, 16f));
        CreateGuardRail(root, "Rail_Launch_L", new Vector3(-8f, 12.5f, 215f), new Vector3(0.6f, 3.0f, 16f));
        CreateGuardRail(root, "Rail_Launch_R", new Vector3(8f, 12.5f, 215f), new Vector3(0.6f, 3.0f, 16f));
        CreateBoostPad(root, new Vector3(0f, 11.52f, 216f), 32f);

        // CHECKPOINT 5: Sky Launch Deck
        CreateCheckpointGate(root, "Checkpoint_05", 5, false, new Vector3(0f, 10.5f, 215f), new Vector3(18f, 8f, 4f), new Vector3(0f, 11.5f, 215f), Quaternion.identity);

        // --- MEGA SKY GAP (Z = 223 to 265, Gap = 42m!) ---
        // Laser pit hazard underneath
        CreateHazard(root, "BottomlessLaserPit", new Vector3(0f, 0.2f, 244f), new Vector3(26f, 0.5f, 44f));

        // Floating Speed Ring in mid-air gap!
        CreateSpeedRing(root, new Vector3(0f, 13.5f, 244f), Quaternion.identity);

        // Arc of floating shards across the sky gap
        CreateShard(root, new Vector3(0f, 13.8f, 234f));
        CreateShard(root, new Vector3(0f, 14.5f, 244f));
        CreateShard(root, new Vector3(0f, 13.8f, 254f));

        // Elevated Landing Runway at Y = 11m with entry catch funnels
        CreateRoadSlab(root, "SkyLandingDeck", new Vector3(0f, 10.5f, 280f), new Vector3(20f, 2f, 32f));
        CreateGuardRail(root, "Funnel_Land_L", new Vector3(-12.5f, 12.5f, 263f), new Vector3(0.6f, 3.2f, 8f), -22f);
        CreateGuardRail(root, "Funnel_Land_R", new Vector3(12.5f, 12.5f, 263f), new Vector3(0.6f, 3.2f, 8f), 22f);
        CreateGuardRail(root, "Rail_Land_L", new Vector3(-10f, 12.5f, 280f), new Vector3(0.6f, 3.2f, 32f));
        CreateGuardRail(root, "Rail_Land_R", new Vector3(10f, 12.5f, 280f), new Vector3(0.6f, 3.2f, 32f));

        // CHECKPOINT 6: Sky Landing Deck
        CreateCheckpointGate(root, "Checkpoint_06", 6, false, new Vector3(0f, 10.5f, 280f), new Vector3(22f, 8f, 4f), new Vector3(0f, 11.5f, 280f), Quaternion.identity);

        // =========================================================================
        // SECTOR 4: DESCENDING CORKSCREW & POLICE CYBER-GAUNTLET (Z = 295 to 420)
        // =========================================================================
        // Descent Corkscrew Ramp: Y = 11m down to Y = 0.5m while sweeping to X = 12m
        float descentAngle = 16.5f;
        CreateRoadSlab(root, "CorkscrewDescent", new Vector3(6f, 5.5f, 314f), new Vector3(16f, 2f, 38f), 15f, descentAngle);
        CreateGuardRail(root, "Rail_Desc_L", new Vector3(-2f, 7.2f, 314f), new Vector3(0.6f, 3.0f, 38f), 15f, descentAngle);
        CreateGuardRail(root, "Rail_Desc_R", new Vector3(14f, 7.2f, 314f), new Vector3(0.6f, 3.0f, 38f), 15f, descentAngle);

        CreateRoadSlab(root, "CorkscrewBottom", new Vector3(2f, -0.5f, 340f), new Vector3(16f, 2f, 20f), -12f);
        CreateGuardRail(root, "Rail_Cork_L", new Vector3(-6.5f, 1.5f, 340f), new Vector3(0.6f, 3.0f, 20f), -12f);
        CreateGuardRail(root, "Rail_Cork_R", new Vector3(10.5f, 1.5f, 340f), new Vector3(0.6f, 3.0f, 20f), -12f);
        CreateShard(root, new Vector3(4f, 1.5f, 335f));

        // CHECKPOINT 7: Gauntlet Entry
        CreateCheckpointGate(root, "Checkpoint_07", 7, false, new Vector3(0f, 0f, 350f), new Vector3(20f, 8f, 4f), new Vector3(0f, 0.5f, 350f), Quaternion.identity);

        // The Cyber-Gauntlet Straight (Z = 350 to 420, X = 0)
        CreateRoadSlab(root, "Gauntlet_Straight", new Vector3(0f, -0.5f, 385f), new Vector3(18f, 2f, 72f));
        CreateGuardRail(root, "Rail_Gaunt_L", new Vector3(-9f, 1.5f, 385f), new Vector3(0.6f, 3.0f, 72f));
        CreateGuardRail(root, "Rail_Gaunt_R", new Vector3(9f, 1.5f, 385f), new Vector3(0.6f, 3.0f, 72f));

        // Gauntlet Hazard 1: Pulsing Laser Gate
        CreateLaserGate(root, "GauntletGate_01", new Vector3(0f, 0f, 360f), 18f);

        // Gauntlet Hazard 2: Twin Rotating Laser Sweepers
        CreateRotatingSweeper(root, "GauntletSweeper_L", new Vector3(-4.5f, 0.5f, 382f), 8f, -70f);
        CreateRotatingSweeper(root, "GauntletSweeper_R", new Vector3(4.5f, 0.5f, 382f), 8f, 70f);

        // Gauntlet Hazard 3: Second Laser Gate
        CreateLaserGate(root, "GauntletGate_02", new Vector3(0f, 0f, 405f), 18f);

        CreateBoostPad(root, new Vector3(-4f, 0.52f, 395f), 30f);
        CreateShard(root, new Vector3(4f, 1.5f, 370f));
        CreateShard(root, new Vector3(-4f, 1.5f, 375f));
        CreateShard(root, new Vector3(0f, 1.5f, 392f));
        CreateShard(root, new Vector3(4f, 1.5f, 412f));

        // CHECKPOINT 8: Gauntlet Exit / Slalom
        CreateCheckpointGate(root, "Checkpoint_08", 8, false, new Vector3(0f, 0f, 420f), new Vector3(20f, 8f, 4f), new Vector3(0f, 0.5f, 420f), Quaternion.identity);

        // =========================================================================
        // SECTOR 5: SLALOM S-CURVES, TRIPLE BOOST FINALE & COLOSSEUM ARCH (Z = 420 to 520)
        // =========================================================================
        // Slalom Left Curve
        CreateRoadSlab(root, "Slalom_Left", new Vector3(-6f, -0.5f, 435f), new Vector3(16f, 2f, 24f), -16f);
        CreateGuardRail(root, "Rail_Slalom1_L", new Vector3(-14f, 1.5f, 435f), new Vector3(0.6f, 3.0f, 24f), -16f);
        CreateGuardRail(root, "Rail_Slalom1_R", new Vector3(2f, 1.5f, 435f), new Vector3(0.6f, 3.0f, 24f), -16f);
        CreateShard(root, new Vector3(-6f, 1.5f, 435f));

        // Slalom Right Curve
        CreateRoadSlab(root, "Slalom_Right", new Vector3(0f, -0.5f, 455f), new Vector3(16f, 2f, 22f), 16f);
        CreateGuardRail(root, "Rail_Slalom2_L", new Vector3(-8f, 1.5f, 455f), new Vector3(0.6f, 3.0f, 22f), 16f);
        CreateGuardRail(root, "Rail_Slalom2_R", new Vector3(8f, 1.5f, 455f), new Vector3(0.6f, 3.0f, 22f), 16f);
        CreateShard(root, new Vector3(0f, 1.5f, 455f));

        // Triple Boost Drag Runway (Z = 465 to 520, X = 0)
        CreateRoadSlab(root, "FinalRunway", new Vector3(0f, -0.5f, 495f), new Vector3(20f, 2f, 62f));
        CreateGuardRail(root, "Rail_Run_L", new Vector3(-10f, 1.5f, 495f), new Vector3(0.6f, 3.0f, 62f));
        CreateGuardRail(root, "Rail_Run_R", new Vector3(10f, 1.5f, 495f), new Vector3(0.6f, 3.0f, 62f));

        // Triple Sequential Boost Pads: 25 m/s -> 30 m/s -> 35 m/s!
        CreateBoostPad(root, new Vector3(0f, 0.52f, 472f), 26f);
        CreateBoostPad(root, new Vector3(0f, 0.52f, 488f), 30f);
        CreateBoostPad(root, new Vector3(0f, 0.52f, 504f), 35f);

        CreateShard(root, new Vector3(-3.5f, 1.5f, 475f));
        CreateShard(root, new Vector3(3.5f, 1.5f, 490f));
        CreateShard(root, new Vector3(0f, 1.5f, 508f));

        // Grand Colosseum Victory Finish Arch at Z = 515m (Checkpoint 9)
        CreateFinishArch(root, new Vector3(0f, 0f, 515f));

        NeonRush.RaceManager rm = FindAnyObjectByType<NeonRush.RaceManager>();
        if (rm != null)
        {
            rm.RefreshCheckpoints();
        }

        Debug.Log("Full 550m Grand Prix Circuit successfully generated with hairpins, checkpoints, sky jumps, speed rings, collectibles, and dynamic hazards!");
    }

    void CreateRoadSlab(Transform parent, string name, Vector3 pos, Vector3 size, float rotY = 0f, float rotX = 0f)
    {
        GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slab.name = name;
        slab.transform.SetParent(parent, false);
        slab.transform.position = pos;
        slab.transform.rotation = Quaternion.Euler(rotX, rotY, 0f);
        slab.transform.localScale = size;
        SetMat(slab, trackMat);
    }

    void CreateGuardRail(Transform parent, string name, Vector3 pos, Vector3 size, float rotY = 0f, float rotX = 0f)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = name;
        rail.transform.SetParent(parent, false);
        rail.transform.position = pos;
        rail.transform.rotation = Quaternion.Euler(rotX, rotY, 0f);
        rail.transform.localScale = size;
        SetMat(rail, barrierMat ?? magentaGlowMat ?? cyanGlowMat);

        Collider c = rail.GetComponent<Collider>();
        if (c != null) c.material = bouncePhysMat;
    }

    void CreateWall(Transform parent, string name, Vector3 pos, Vector3 size)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.position = pos;
        wall.transform.localScale = size;
        SetMat(wall, trackMat);
    }

    void CreateHazard(Transform parent, string name, Vector3 pos, Vector3 size)
    {
        GameObject haz = GameObject.CreatePrimitive(PrimitiveType.Cube);
        haz.name = name;
        haz.tag = "Hazard";
        haz.transform.SetParent(parent, false);
        haz.transform.position = pos;
        haz.transform.localScale = size;
        SetMat(haz, hazardMat);
    }

    void CreateBoostPad(Transform parent, Vector3 pos, float boostPower, float rotY = 0f)
    {
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = "BoostPad";
        pad.transform.SetParent(parent, false);
        pad.transform.position = pos;
        pad.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        pad.transform.localScale = new Vector3(6f, 0.08f, 4f);
        SetMat(pad, boostMat ?? cyanGlowMat);

        Collider col = pad.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        BoostPad bp = pad.AddComponent<BoostPad>();
        bp.boostForce = boostPower;
    }

    void CreateEnergyZone(Transform parent, Vector3 pos)
    {
        GameObject ez = new GameObject("AntiGravCellPickup");
        ez.transform.SetParent(parent, false);
        ez.transform.position = pos;

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "CellCore";
        core.transform.SetParent(ez.transform, false);
        core.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        SetMat(core, magentaGlowMat ?? cyanGlowMat);
        Collider cc = core.GetComponent<Collider>();
        if (cc != null) SafeDestroy(cc);

        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "ContainmentRing";
        ring.transform.SetParent(ez.transform, false);
        ring.transform.localScale = new Vector3(2.4f, 0.15f, 2.4f);
        SetMat(ring, cyanGlowMat);
        Collider rc = ring.GetComponent<Collider>();
        if (rc != null) SafeDestroy(rc);

        SphereCollider col = ez.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2.0f;

        ez.AddComponent<EnergyPickup>();
    }

    void CreateShard(Transform parent, Vector3 pos)
    {
        GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shard.name = "QuantumShard";
        shard.transform.SetParent(parent, false);
        shard.transform.position = pos;
        shard.transform.rotation = Quaternion.Euler(45f, 45f, 45f);
        shard.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        SetMat(shard, cyanGlowMat);

        shard.AddComponent<QuantumShard>();
    }

    void CreateRotatingSweeper(Transform parent, string name, Vector3 pivotPos, float armLength, float speed)
    {
        GameObject sweeperPivot = new GameObject(name);
        sweeperPivot.transform.SetParent(parent, false);
        sweeperPivot.transform.position = pivotPos;

        // Central glowing column
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "PivotPillar";
        pillar.transform.SetParent(sweeperPivot.transform, false);
        pillar.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        pillar.transform.localScale = new Vector3(1.4f, 1.2f, 1.4f);
        SetMat(pillar, magentaGlowMat ?? hazardMat);
        Collider pc = pillar.GetComponent<Collider>();
        if (pc != null) SafeDestroy(pc);

        // Sweeping Hazard Arm
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "HazardLaserArm";
        arm.tag = "Hazard";
        arm.transform.SetParent(sweeperPivot.transform, false);
        arm.transform.localPosition = new Vector3(armLength * 0.5f, 0.4f, 0f);
        arm.transform.localScale = new Vector3(armLength, 0.35f, 0.45f);
        SetMat(arm, hazardMat);

        RotatingHazard rh = sweeperPivot.AddComponent<RotatingHazard>();
        rh.rotationSpeed = speed;

        // Warning red light
        GameObject lightObj = new GameObject("SweeperLight");
        lightObj.transform.SetParent(sweeperPivot.transform, false);
        lightObj.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        Light sl = lightObj.AddComponent<Light>();
        sl.type = LightType.Point;
        sl.color = Color.red;
        sl.range = 10f;
        sl.intensity = 3.5f;
    }

    void CreateLaserGate(Transform parent, string name, Vector3 pos, float width)
    {
        GameObject gate = new GameObject(name);
        gate.transform.SetParent(parent, false);
        gate.transform.position = pos;

        // Left post
        GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        postL.name = "Post_L";
        postL.transform.SetParent(gate.transform, false);
        postL.transform.localPosition = new Vector3(-width * 0.5f, 2.5f, 0f);
        postL.transform.localScale = new Vector3(1.2f, 5f, 1.2f);
        SetMat(postL, barrierMat ?? cyanGlowMat);

        // Right post
        GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        postR.name = "Post_R";
        postR.transform.SetParent(gate.transform, false);
        postR.transform.localPosition = new Vector3(width * 0.5f, 2.5f, 0f);
        postR.transform.localScale = new Vector3(1.2f, 5f, 1.2f);
        SetMat(postR, barrierMat ?? cyanGlowMat);

        // Pulsing Laser Beam
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "PulsingBeam";
        beam.tag = "Hazard";
        beam.transform.SetParent(gate.transform, false);
        beam.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        beam.transform.localScale = new Vector3(width - 1.2f, 1.8f, 0.4f);
        SetMat(beam, hazardMat);

        GameObject lightObj = new GameObject("GateLight");
        lightObj.transform.SetParent(beam.transform, false);
        Light gl = lightObj.AddComponent<Light>();
        gl.type = LightType.Point;
        gl.color = Color.red;
        gl.range = 12f;
        gl.intensity = 4.0f;

        beam.AddComponent<PulsingLaserGate>();
    }

    void CreateSpeedRing(Transform parent, Vector3 pos, Quaternion rot)
    {
        GameObject ringObj = new GameObject("SpeedRing");
        ringObj.transform.SetParent(parent, false);
        ringObj.transform.position = pos;
        ringObj.transform.rotation = rot;

        float radius = 5.0f;
        int segments = 8;
        for (int i = 0; i < segments; i++)
        {
            float angle = (i * 360f / segments) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = $"RingSeg_{i}";
            seg.transform.SetParent(ringObj.transform, false);
            seg.transform.localPosition = new Vector3(x, y, 0f);
            seg.transform.localRotation = Quaternion.Euler(0f, 0f, (i * 360f / segments) + 9f);
            seg.transform.localScale = new Vector3(0.5f, 4.0f, 0.5f);
            SetMat(seg, cyanGlowMat);

            Collider sc = seg.GetComponent<Collider>();
            if (sc != null) SafeDestroy(sc);
        }

        // Center trigger box
        BoxCollider ringCol = ringObj.AddComponent<BoxCollider>();
        ringCol.isTrigger = true;
        ringCol.size = new Vector3(radius * 1.8f, radius * 1.8f, 3.0f);

        GameObject rLight = new GameObject("RingLight");
        rLight.transform.SetParent(ringObj.transform, false);
        Light l = rLight.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0f, 0.95f, 1f);
        l.range = 16f;
        l.intensity = 4.5f;

        ringObj.AddComponent<SpeedRing>();
    }

    void CreateTurnChevronSign(Transform parent, Vector3 pos, float rotY, bool isRight)
    {
        GameObject signObj = new GameObject("TurnChevronSign");
        signObj.transform.SetParent(parent, false);
        signObj.transform.position = pos;
        signObj.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        // Signboard backing
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "SignBoard";
        board.transform.SetParent(signObj.transform, false);
        board.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        board.transform.localScale = new Vector3(6.0f, 2.2f, 0.3f);
        SetMat(board, magentaGlowMat ?? cyanGlowMat);

        Collider bc = board.GetComponent<Collider>();
        if (bc != null) SafeDestroy(bc);

        // Chevrons
        for (int i = -1; i <= 1; i++)
        {
            GameObject chev = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chev.name = $"Chevron_{i}";
            chev.transform.SetParent(board.transform, false);
            chev.transform.localPosition = new Vector3(i * 1.6f, 0f, -0.2f);
            chev.transform.localRotation = Quaternion.Euler(0f, 0f, isRight ? -45f : 45f);
            chev.transform.localScale = new Vector3(0.35f, 1.4f, 0.2f);
            SetMat(chev, cyanGlowMat);

            Collider cc = chev.GetComponent<Collider>();
            if (cc != null) SafeDestroy(cc);
        }

        BoxCollider trig = signObj.AddComponent<BoxCollider>();
        trig.isTrigger = true;
        trig.size = new Vector3(18f, 6f, 18f);

        TurnSign ts = signObj.AddComponent<TurnSign>();
        ts.turnRight = isRight;
    }

    GameObject CreateCheckpointGate(Transform parent, string name, int index, bool isFinish, Vector3 pos, Vector3 triggerSize, Vector3 safeRespawnPos, Quaternion safeRespawnRot, float rotY = 0f)
    {
        GameObject cpObj = new GameObject(name);
        cpObj.transform.SetParent(parent, false);
        cpObj.transform.position = pos;
        cpObj.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        BoxCollider trigger = cpObj.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, triggerSize.y * 0.5f, 0f);
        trigger.size = triggerSize;

        NeonRush.Checkpoint cp = cpObj.AddComponent<NeonRush.Checkpoint>();
        cp.SetCheckpointData(index, isFinish, safeRespawnPos, safeRespawnRot);

        float halfW = triggerSize.x * 0.5f;
        CreateArchColumn(cpObj.transform, new Vector3(-halfW, triggerSize.y * 0.5f, 0f), triggerSize.y);
        CreateArchColumn(cpObj.transform, new Vector3(halfW, triggerSize.y * 0.5f, 0f), triggerSize.y);

        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "GantryBeam";
        beam.transform.SetParent(cpObj.transform, false);
        beam.transform.localPosition = new Vector3(0f, triggerSize.y + 0.3f, 0f);
        beam.transform.localScale = new Vector3(triggerSize.x + 1.6f, 0.6f, 0.8f);
        SetMat(beam, isFinish ? magentaGlowMat : cyanGlowMat);
        Collider bc = beam.GetComponent<Collider>();
        if (bc != null) SafeDestroy(bc);

        return cpObj;
    }

    void CreateStartGantry(Transform parent, Vector3 pos)
    {
        GameObject gantry = new GameObject("StartGantry");
        gantry.transform.SetParent(parent, false);
        gantry.transform.position = pos;

        CreateArchColumn(gantry.transform, new Vector3(-10f, 4.5f, 0f), 9f);
        CreateArchColumn(gantry.transform, new Vector3(10f, 4.5f, 0f), 9f);

        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "OverheadBeam";
        beam.transform.SetParent(gantry.transform, false);
        beam.transform.localPosition = new Vector3(0f, 9.2f, 0f);
        beam.transform.localScale = new Vector3(21f, 1.0f, 1.5f);
        SetMat(beam, cyanGlowMat);
        Collider bc = beam.GetComponent<Collider>();
        if (bc != null) SafeDestroy(bc);
    }

    void CreateFinishArch(Transform parent, Vector3 centerPos)
    {
        GameObject finishObj = new GameObject("FinishGate");
        finishObj.transform.SetParent(parent, false);
        finishObj.transform.position = centerPos;

        CreateArchColumn(finishObj.transform, new Vector3(-10f, 6.5f, 0f), 13f);
        CreateArchColumn(finishObj.transform, new Vector3(10f, 6.5f, 0f), 13f);

        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "OverheadBeam";
        beam.transform.SetParent(finishObj.transform, false);
        beam.transform.localPosition = new Vector3(0f, 13.2f, 0f);
        beam.transform.localScale = new Vector3(21.5f, 1.4f, 2.0f);
        SetMat(beam, magentaGlowMat ?? cyanGlowMat);
        Collider c1 = beam.GetComponent<Collider>();
        if (c1 != null) SafeDestroy(c1);

        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "FinishSign";
        sign.transform.SetParent(finishObj.transform, false);
        sign.transform.localPosition = new Vector3(0f, 11.5f, 0f);
        sign.transform.localScale = new Vector3(16f, 1.4f, 0.4f);
        SetMat(sign, cyanGlowMat);
        Collider c2 = sign.GetComponent<Collider>();
        if (c2 != null) SafeDestroy(c2);

        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = "FinishLineMarking";
        line.transform.SetParent(finishObj.transform, false);
        line.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        line.transform.localScale = new Vector3(19f, 0.05f, 2.0f);
        SetMat(line, cyanGlowMat);
        Collider c3 = line.GetComponent<Collider>();
        if (c3 != null) SafeDestroy(c3);

        BoxCollider goalBox = finishObj.AddComponent<BoxCollider>();
        goalBox.isTrigger = true;
        goalBox.center = new Vector3(0f, 5f, 0f);
        goalBox.size = new Vector3(20f, 10f, 4f);
        finishObj.AddComponent<GoalTrigger>();

        NeonRush.Checkpoint cp = finishObj.AddComponent<NeonRush.Checkpoint>();
        cp.SetCheckpointData(9, true, new Vector3(0f, 0.5f, 500f), Quaternion.identity);

        GameObject l1 = new GameObject("ArchLight_L");
        l1.transform.SetParent(finishObj.transform, false);
        l1.transform.localPosition = new Vector3(-9f, 12f, -2f);
        Light spot1 = l1.AddComponent<Light>();
        spot1.type = LightType.Spot;
        spot1.color = new Color(0f, 0.9f, 1f);
        spot1.intensity = 5f;
        spot1.range = 30f;
        spot1.spotAngle = 75f;
        l1.transform.localRotation = Quaternion.Euler(50f, 20f, 0f);

        GameObject l2 = new GameObject("ArchLight_R");
        l2.transform.SetParent(finishObj.transform, false);
        l2.transform.localPosition = new Vector3(9f, 12f, -2f);
        Light spot2 = l2.AddComponent<Light>();
        spot2.type = LightType.Spot;
        spot2.color = new Color(1f, 0.1f, 0.85f);
        spot2.intensity = 5f;
        spot2.range = 30f;
        spot2.spotAngle = 75f;
        l2.transform.localRotation = Quaternion.Euler(50f, -20f, 0f);
    }

    void CreateArchColumn(Transform parent, Vector3 localPos, float height)
    {
        GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cube);
        col.name = "Pillar";
        col.transform.SetParent(parent, false);
        col.transform.localPosition = localPos;
        col.transform.localScale = new Vector3(1.8f, height, 1.8f);
        SetMat(col, barrierMat ?? cyanGlowMat);
        Collider c = col.GetComponent<Collider>();
        if (c != null) SafeDestroy(c);
    }

    void SetMat(GameObject obj, Material mat)
    {
        if (mat == null) return;
        MeshRenderer r = obj.GetComponent<MeshRenderer>();
        if (r != null) r.sharedMaterial = mat;
    }
}
