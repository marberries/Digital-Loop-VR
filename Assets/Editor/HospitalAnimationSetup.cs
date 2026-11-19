using UnityEngine;
using UnityEditor;
using DigitalCurtain.Hospital;

public class HospitalAnimationSetup : EditorWindow
{
    [MenuItem("Tools/Hospital Ward/4. Attach Animations & Live Effects")]
    public static void AttachAnimations()
    {
        // 1. Setup ECG Monitor Animation & Heartbeat Audio
        GameObject terminal = GameObject.Find("Med_Terminal");
        if (terminal != null)
        {
            ECGMonitorController ecg = terminal.GetComponent<ECGMonitorController>();
            if (ecg == null) ecg = terminal.AddComponent<ECGMonitorController>();

            Transform screenTr = terminal.transform.Find("Terminal_Monitor_Head/Monitor_Screen");
            Transform ledTr = terminal.transform.Find("Terminal_Monitor_Head/Monitor_PowerLED");

            if (screenTr != null) ecg.screenRenderer = screenTr.GetComponent<Renderer>();
            if (ledTr != null) ecg.ledRenderer = ledTr.GetComponent<Renderer>();
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
        }

        // 3. Setup Patient Breathing, Neural Headset Visor & Blackout Fade
        GameObject cameraOffset = GameObject.Find("Camera Offset");
        GameObject mainCamera = GameObject.Find("Main Camera");

        if (cameraOffset != null && mainCamera != null)
        {
            PatientBreathingAndIntro intro = cameraOffset.GetComponent<PatientBreathingAndIntro>();
            if (intro == null) intro = cameraOffset.AddComponent<PatientBreathingAndIntro>();

            if (terminal != null)
            {
                intro.ecgMonitor = terminal.GetComponent<ECGMonitorController>();
            }

            // Create Neural Headset Visor Mesh above camera
            Transform existingVisor = mainCamera.transform.Find("Neural_Headset_Visor");
            if (existingVisor != null) DestroyImmediate(existingVisor.gameObject);

            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Neural_Headset_Visor";
            visor.transform.SetParent(mainCamera.transform);
            visor.transform.localPosition = new Vector3(0f, 0.35f, 0.25f);
            visor.transform.localRotation = Quaternion.identity;
            visor.transform.localScale = new Vector3(0.32f, 0.14f, 0.08f);
            DestroyImmediate(visor.GetComponent<Collider>());
            visor.GetComponent<Renderer>().sharedMaterial =
                HospitalMaterials.CreateMaterial("Neural_Headset_Mat", new Color(0.08f, 0.09f, 0.11f), 0.85f, 0.9f);
            intro.headsetVisor = visor.transform;

            // Create Transparent Blackout Quad in front of camera for smooth scene fade
            Transform existingFade = mainCamera.transform.Find("Blackout_Overlay");
            if (existingFade != null) DestroyImmediate(existingFade.gameObject);

            GameObject fadeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fadeQuad.name = "Blackout_Overlay";
            fadeQuad.transform.SetParent(mainCamera.transform);
            fadeQuad.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            fadeQuad.transform.localRotation = Quaternion.identity;
            fadeQuad.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            DestroyImmediate(fadeQuad.GetComponent<Collider>());

            Material fadeMat = new Material(HospitalMaterials.GetLitShader());
            fadeMat.SetFloat("_Surface", 1f); // Transparent in URP
            fadeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fadeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fadeMat.SetInt("_ZWrite", 0);
            fadeMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            fadeMat.renderQueue = 3000;
            fadeMat.SetColor("_BaseColor", new Color(0f, 0f, 0f, 0f));

            Renderer fadeRenderer = fadeQuad.GetComponent<Renderer>();
            fadeRenderer.sharedMaterial = fadeMat;
            intro.blackoutOverlay = fadeRenderer;
        }

        Debug.Log("[HospitalAnimationSetup] ECG pulse, audio beep, flickering light, breathing, and headset intro attached!");
    }
}
