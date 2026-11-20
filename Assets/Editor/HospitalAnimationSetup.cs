using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DigitalCurtain.Hospital;

public class HospitalAnimationSetup : EditorWindow
{
    [MenuItem("Tools/Hospital Ward/4. Attach Animations & Live Effects")]
    public static void AttachAnimations()
    {
        int attachedCount = 0;

        // 1. Setup ECG Monitor Animation, Sweep Cursor & Heartbeat Audio
        GameObject terminal = GameObject.Find("Med_Terminal");
        if (terminal != null)
        {
            ECGMonitorController ecg = terminal.GetComponent<ECGMonitorController>();
            if (ecg == null) ecg = terminal.AddComponent<ECGMonitorController>();

            Transform headTr = terminal.transform.Find("Terminal_Monitor_Head");
            if (headTr != null)
            {
                Transform screenTr = headTr.Find("Monitor_Screen");
                Transform ledTr = headTr.Find("Monitor_PowerLED");

                if (screenTr != null) ecg.screenRenderer = screenTr.GetComponent<Renderer>();
                if (ledTr != null) ecg.ledRenderer = ledTr.GetComponent<Renderer>();

                Transform oldCursor = headTr.Find("ECG_Sweep_Cursor");
                if (oldCursor != null) DestroyImmediate(oldCursor.gameObject);

                GameObject cursor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cursor.name = "ECG_Sweep_Cursor";
                cursor.transform.SetParent(headTr);
                cursor.transform.localPosition = new Vector3(-0.23f, 0.01f, -0.075f);
                cursor.transform.localRotation = Quaternion.identity;
                cursor.transform.localScale = new Vector3(0.012f, 0.042f, 0.005f);
                DestroyImmediate(cursor.GetComponent<Collider>());

                Material cursorMat = HospitalMaterials.CreateEmissiveMaterial(
                    "ECG_Cursor_Mat",
                    new Color(0.2f, 1.0f, 0.5f),
                    new Color(0.2f, 1.0f, 0.5f),
                    3.5f
                );
                cursor.GetComponent<Renderer>().sharedMaterial = cursorMat;
                ecg.sweepCursor = cursor.transform;
            }
            attachedCount++;
        }

        // 2. Setup Flickering Ceiling Light (Fixture #3)
        GameObject fixture = GameObject.Find("Ceiling_Light_Fixture_3");
        if (fixture != null)
        {
            FluorescentFlicker flicker = fixture.GetComponent<FluorescentFlicker>();
            if (flicker == null) flicker = fixture.AddComponent<FluorescentFlicker>();

            flicker.fixtureLight = fixture.GetComponentInChildren<Light>();
            Transform tubeL = fixture.transform.Find("Fluorescent_Tube_Left");
            Transform tubeR = fixture.transform.Find("Fluorescent_Tube_Right");
            if (tubeL != null && tubeR != null)
            {
                flicker.tubeRenderers = new Renderer[]
                {
                    tubeL.GetComponent<Renderer>(),
                    tubeR.GetComponent<Renderer>()
                };
            }
            attachedCount++;
        }

        // 3. Setup Patient Breathing, Detailed 22-Part Neural Headset & Blackout Fade
        GameObject cameraOffset = GameObject.Find("Camera Offset");
        if (cameraOffset != null)
        {
            PatientBreathingAndIntro oldIntro = cameraOffset.GetComponent<PatientBreathingAndIntro>();
            if (oldIntro != null) DestroyImmediate(oldIntro);
        }

        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            if (cam != null) cam.nearClipPlane = 0.05f;

            PatientBreathingAndIntro intro = mainCamera.GetComponent<PatientBreathingAndIntro>();
            if (intro == null) intro = mainCamera.AddComponent<PatientBreathingAndIntro>();

            if (terminal != null)
            {
                intro.ecgMonitor = terminal.GetComponent<ECGMonitorController>();
            }

            Transform legacyVisor = mainCamera.transform.Find("Neural_Headset_Visor");
            if (legacyVisor != null) DestroyImmediate(legacyVisor.gameObject);

            Transform existingAssembly = mainCamera.transform.Find("Neural_Headset_Assembly");
            if (existingAssembly != null) DestroyImmediate(existingAssembly.gameObject);

            BuildDetailedNeuralHeadset(mainCamera.transform, intro);

            Transform existingFade = mainCamera.transform.Find("Blackout_Overlay");
            if (existingFade != null) DestroyImmediate(existingFade.gameObject);

            GameObject fadeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fadeQuad.name = "Blackout_Overlay";
            fadeQuad.transform.SetParent(mainCamera.transform);
            fadeQuad.transform.localPosition = new Vector3(0f, 0f, 0.06f);
            fadeQuad.transform.localRotation = Quaternion.identity;
            fadeQuad.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
            DestroyImmediate(fadeQuad.GetComponent<Collider>());

            Material fadeMat = new Material(HospitalMaterials.GetLitShader());
            fadeMat.SetFloat("_Surface", 1f);
            fadeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fadeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fadeMat.SetInt("_ZWrite", 0);
            fadeMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            fadeMat.renderQueue = 3000;
            fadeMat.SetColor("_BaseColor", new Color(0f, 0f, 0f, 0f));

            Renderer fadeRenderer = fadeQuad.GetComponent<Renderer>();
            fadeRenderer.sharedMaterial = fadeMat;
            intro.blackoutOverlay = fadeRenderer;
            attachedCount++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[HospitalAnimationSetup] Configured {attachedCount}/3 systems with 22-part neural headset and formant breathing audio.");
    }

    private static void BuildDetailedNeuralHeadset(Transform cameraParent, PatientBreathingAndIntro intro)
    {
        Material shellMat = HospitalMaterials.CreateMaterial("Headset_Shell_Mat", new Color(0.11f, 0.12f, 0.14f), 0.72f, 0.45f);
        Material trimSilverMat = HospitalMaterials.CreateMaterial("Headset_Silver_Mat", new Color(0.72f, 0.75f, 0.79f), 0.90f, 0.92f);
        Material foamMat = HospitalMaterials.CreateMaterial("Headset_Foam_Mat", new Color(0.05f, 0.05f, 0.06f), 0.12f, 0.0f);
        Material frontGlassMat = HospitalMaterials.CreateMaterial("Headset_FrontGlass_Mat", new Color(0.03f, 0.04f, 0.06f), 0.96f, 0.85f);
        Material cyanOpticMat = HospitalMaterials.CreateEmissiveMaterial("Headset_CyanOptic_Mat", new Color(0.1f, 0.95f, 1.0f), new Color(0.1f, 0.95f, 1.0f), 3.6f);
        Material strapMat = HospitalMaterials.CreateMaterial("Headset_Strap_Mat", new Color(0.18f, 0.19f, 0.21f), 0.22f, 0.0f);
        Material innerHudMat = HospitalMaterials.CreateHeadsetHUDMaterial();

        Material innerTintMat = HospitalMaterials.CreateTransparentEmissiveMaterial(
            "Headset_InnerTint_Mat",
            new Color(0.03f, 0.08f, 0.12f, 0f),
            new Color(0.02f, 0.12f, 0.18f),
            0.35f
        );

        GameObject headset = new GameObject("Neural_Headset_Assembly");
        headset.transform.SetParent(cameraParent);
        headset.transform.localPosition = new Vector3(0.05f, 0.34f, 0.44f);
        headset.transform.localRotation = Quaternion.Euler(18f, -30f, 8f);
        intro.headsetAssembly = headset.transform;

        // --- EXTERIOR HARDWARE (visible at z = 0.36m in 3/4 view; cleanly clips behind nearClipPlane when worn at z = -0.02m) ---
        BuildHeadsetPart(headset.transform, "Visor_Front_Body", PrimitiveType.Cube, new Vector3(0f, 0.002f, 0.025f), Vector3.zero, new Vector3(0.20f, 0.095f, 0.055f), shellMat);
        BuildHeadsetPart(headset.transform, "Visor_Brow_Shield", PrimitiveType.Cube, new Vector3(0f, 0.056f, 0.01f), new Vector3(8f, 0f, 0f), new Vector3(0.215f, 0.026f, 0.085f), shellMat);
        BuildHeadsetPart(headset.transform, "Visor_Cheek_L", PrimitiveType.Cube, new Vector3(-0.064f, -0.044f, 0.015f), new Vector3(0f, 0f, -10f), new Vector3(0.075f, 0.024f, 0.075f), shellMat);
        BuildHeadsetPart(headset.transform, "Visor_Cheek_R", PrimitiveType.Cube, new Vector3( 0.064f, -0.044f, 0.015f), new Vector3(0f, 0f,  10f), new Vector3(0.075f, 0.024f, 0.075f), shellMat);

        // Front tinted glass plate, glowing cyan neural bar & dual corner tracking cameras
        BuildHeadsetPart(headset.transform, "Front_Glass_Plate", PrimitiveType.Cube, new Vector3(0f, 0.004f, 0.054f), Vector3.zero, new Vector3(0.188f, 0.080f, 0.006f), frontGlassMat);
        BuildHeadsetPart(headset.transform, "Front_Neural_Optic_Strip", PrimitiveType.Cube, new Vector3(0f, 0.024f, 0.058f), Vector3.zero, new Vector3(0.155f, 0.007f, 0.004f), cyanOpticMat);
        BuildHeadsetPart(headset.transform, "Tracking_Cam_Housing_L", PrimitiveType.Cylinder, new Vector3(-0.082f, 0.032f, 0.055f), new Vector3(90f, 0f, 0f), new Vector3(0.022f, 0.008f, 0.022f), trimSilverMat);
        BuildHeadsetPart(headset.transform, "Tracking_Cam_Lens_L", PrimitiveType.Cylinder, new Vector3(-0.082f, 0.032f, 0.060f), new Vector3(90f, 0f, 0f), new Vector3(0.012f, 0.004f, 0.012f), frontGlassMat);
        BuildHeadsetPart(headset.transform, "Tracking_Cam_Housing_R", PrimitiveType.Cylinder, new Vector3( 0.082f, 0.032f, 0.055f), new Vector3(90f, 0f, 0f), new Vector3(0.022f, 0.008f, 0.022f), trimSilverMat);
        BuildHeadsetPart(headset.transform, "Tracking_Cam_Lens_R", PrimitiveType.Cylinder, new Vector3( 0.082f, 0.032f, 0.060f), new Vector3(90f, 0f, 0f), new Vector3(0.012f, 0.004f, 0.012f), frontGlassMat);

        // Side silver hinges, temple arms, glowing status LEDs, top strap & neural cables
        BuildHeadsetPart(headset.transform, "Side_Hinge_L", PrimitiveType.Cube, new Vector3(-0.104f, 0.008f, 0.015f), Vector3.zero, new Vector3(0.016f, 0.034f, 0.034f), trimSilverMat);
        BuildHeadsetPart(headset.transform, "Side_Hinge_R", PrimitiveType.Cube, new Vector3( 0.104f, 0.008f, 0.015f), Vector3.zero, new Vector3(0.016f, 0.034f, 0.034f), trimSilverMat);
        BuildHeadsetPart(headset.transform, "Temple_Arm_L", PrimitiveType.Cube, new Vector3(-0.106f, 0.008f, -0.065f), Vector3.zero, new Vector3(0.014f, 0.028f, 0.14f), shellMat);
        BuildHeadsetPart(headset.transform, "Temple_Arm_R", PrimitiveType.Cube, new Vector3( 0.106f, 0.008f, -0.065f), Vector3.zero, new Vector3(0.014f, 0.028f, 0.14f), shellMat);
        BuildHeadsetPart(headset.transform, "Side_Status_LED_L", PrimitiveType.Sphere, new Vector3(-0.114f, 0.008f, 0.015f), Vector3.zero, new Vector3(0.010f, 0.010f, 0.010f), cyanOpticMat);
        BuildHeadsetPart(headset.transform, "Side_Status_LED_R", PrimitiveType.Sphere, new Vector3( 0.114f, 0.008f, 0.015f), Vector3.zero, new Vector3(0.010f, 0.010f, 0.010f), cyanOpticMat);
        BuildHeadsetPart(headset.transform, "Top_Head_Strap", PrimitiveType.Cube, new Vector3(0f, 0.068f, -0.055f), new Vector3(-12f, 0f, 0f), new Vector3(0.038f, 0.010f, 0.14f), strapMat);

        // --- INTERIOR GOGGLE FRAME & LENSES (positioned at z = +0.095..0.11m so they frame the screen when worn) ---
        BuildHeadsetPart(headset.transform, "Goggle_Foam_Top", PrimitiveType.Cube, new Vector3(0f, 0.043f, 0.095f), Vector3.zero, new Vector3(0.18f, 0.016f, 0.02f), foamMat);
        BuildHeadsetPart(headset.transform, "Goggle_Foam_Bottom", PrimitiveType.Cube, new Vector3(0f, -0.043f, 0.095f), Vector3.zero, new Vector3(0.18f, 0.016f, 0.02f), foamMat);
        BuildHeadsetPart(headset.transform, "Goggle_Nose_Bridge", PrimitiveType.Cube, new Vector3(0f, -0.032f, 0.095f), Vector3.zero, new Vector3(0.024f, 0.022f, 0.02f), foamMat);
        BuildHeadsetPart(headset.transform, "Goggle_Foam_Left", PrimitiveType.Cube, new Vector3(-0.077f, 0f, 0.095f), Vector3.zero, new Vector3(0.022f, 0.095f, 0.02f), foamMat);
        BuildHeadsetPart(headset.transform, "Goggle_Foam_Right", PrimitiveType.Cube, new Vector3( 0.077f, 0f, 0.095f), Vector3.zero, new Vector3(0.022f, 0.095f, 0.02f), foamMat);

        GameObject innerHud = BuildHeadsetPart(headset.transform, "Visor_Inner_HUD", PrimitiveType.Quad, new Vector3(0f, 0f, 0.102f), Vector3.zero, new Vector3(0.155f, 0.088f, 1f), innerHudMat);
        intro.innerHudRenderer = innerHud.GetComponent<Renderer>();

        GameObject innerGlass = BuildHeadsetPart(headset.transform, "Visor_Inner_TintGlass", PrimitiveType.Quad, new Vector3(0f, 0f, 0.108f), Vector3.zero, new Vector3(0.165f, 0.095f, 1f), innerTintMat);
        intro.innerGlassRenderer = innerGlass.GetComponent<Renderer>();
    }

    private static GameObject BuildHeadsetPart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.Euler(localEuler);
        obj.transform.localScale = localScale;
        Collider col = obj.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }
}