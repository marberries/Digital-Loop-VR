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

        // 3. Setup Patient Breathing, Neural Headset Visor & Blackout Fade on Main Camera
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
            if (cam != null) cam.nearClipPlane = 0.1f;

            PatientBreathingAndIntro intro = mainCamera.GetComponent<PatientBreathingAndIntro>();
            if (intro == null) intro = mainCamera.AddComponent<PatientBreathingAndIntro>();

            if (terminal != null)
            {
                intro.ecgMonitor = terminal.GetComponent<ECGMonitorController>();
            }

            Transform existingVisor = mainCamera.transform.Find("Neural_Headset_Visor");
            if (existingVisor != null) DestroyImmediate(existingVisor.gameObject);

            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Neural_Headset_Visor";
            visor.transform.SetParent(mainCamera.transform);
            visor.transform.localPosition = new Vector3(0f, 0.45f, 0.42f);
            visor.transform.localRotation = Quaternion.identity;
            visor.transform.localScale = new Vector3(0.55f, 0.24f, 0.10f);
            DestroyImmediate(visor.GetComponent<Collider>());
            visor.GetComponent<Renderer>().sharedMaterial =
                HospitalMaterials.CreateMaterial("Neural_Headset_Mat", new Color(0.08f, 0.09f, 0.11f), 0.85f, 0.9f);
            intro.headsetVisor = visor.transform;

            Transform existingFade = mainCamera.transform.Find("Blackout_Overlay");
            if (existingFade != null) DestroyImmediate(existingFade.gameObject);

            GameObject fadeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fadeQuad.name = "Blackout_Overlay";
            fadeQuad.transform.SetParent(mainCamera.transform);
            fadeQuad.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            fadeQuad.transform.localRotation = Quaternion.identity;
            fadeQuad.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
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
        Debug.Log($"[HospitalAnimationSetup] Successfully configured {attachedCount}/3 live systems (ECG Monitor, Ceiling Flicker, Patient Intro).");
    }
}