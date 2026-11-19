using UnityEngine;
using UnityEditor;

public class HospitalPropsAndTerminalBuilder : EditorWindow
{
    [MenuItem("Tools/Hospital Ward/3. Build Cabinet, Nightstand, Terminal & Camera")]
    public static void BuildPropsAndTerminal()
    {
        Material woodMat = HospitalMaterials.CreateMaterial("Bed_Wood_Mat", new Color(0.76f, 0.73f, 0.68f), 0.32f, 0.0f);
        Material cabinetSteelMat = HospitalMaterials.CreateMaterial("Cabinet_Steel_Mat", new Color(0.78f, 0.81f, 0.83f), 0.45f, 0.35f);
        Material chromeMat = HospitalMaterials.CreateMaterial("Hospital_Chrome_Mat", new Color(0.82f, 0.84f, 0.87f), 0.92f, 0.95f);
        Material darkPlasticMat = HospitalMaterials.CreateMaterial("Terminal_Body_Mat", new Color(0.14f, 0.16f, 0.18f), 0.35f, 0.1f);
        Material rubberMat = HospitalMaterials.CreateMaterial("Wheel_Rubber_Mat", new Color(0.12f, 0.13f, 0.14f), 0.25f, 0.0f);
        Material amberBottleMat = HospitalMaterials.CreateMaterial("Pill_Bottle_Amber_Mat", new Color(0.78f, 0.38f, 0.08f), 0.75f, 0.0f);
        Material whiteCapMat = HospitalMaterials.CreateMaterial("Pill_Cap_White_Mat", new Color(0.95f, 0.95f, 0.96f), 0.3f, 0.0f);
        Material ecgScreenMat = HospitalMaterials.CreateECGScreenMaterial();
        Material powerLedMat = HospitalMaterials.CreateEmissiveMaterial("Terminal_LED_Mat", Color.green, new Color(0.1f, 1.0f, 0.4f), 3.0f);

        BuildDetailedNightstand(new Vector3(-0.54f, 0f, -4.15f), woodMat, chromeMat);
        SetupPillBottle(new Vector3(-0.54f, 0.655f, -4.15f), amberBottleMat, whiteCapMat);
        BuildDetailedMedicalCabinet(new Vector3(-4.52f, 0f, -1.85f), cabinetSteelMat, chromeMat);
        BuildMedicalTerminal(new Vector3(-1.12f, 0f, -2.05f), darkPlasticMat, chromeMat, rubberMat, ecgScreenMat, powerLedMat);
        SetupPatientCamera();

        Debug.Log("[HospitalPropsAndTerminalBuilder] Nightstand, pill bottle, cabinet, medical terminal, and patient camera configured.");
    }

    [MenuItem("Tools/Hospital Ward/Build All (Complete Ward)")]
    public static void BuildCompleteWard()
    {
        HospitalArchitectureBuilder.BuildArchitecture();
        HospitalBedAndIVBuilder.BuildBedAndIV();
        BuildPropsAndTerminal();
        Debug.Log("[Hospital Ward] Complete hospital ward generated successfully!");
    }

    private static void BuildDetailedNightstand(Vector3 worldPos, Material woodMat, Material chromeMat)
    {
        GameObject ns = GameObject.Find("Nightstand");
        if (ns != null) DestroyImmediate(ns);

        ns = new GameObject("Nightstand");
        ns.transform.position = worldPos;
        ns.transform.rotation = Quaternion.identity;

        BuildPart(ns.transform, "NS_Plinth", PrimitiveType.Cube, new Vector3(0f, 0.03f, 0f), Vector3.zero, new Vector3(0.44f, 0.06f, 0.44f), woodMat);
        BuildPart(ns.transform, "NS_Body", PrimitiveType.Cube, new Vector3(0f, 0.34f, 0f), Vector3.zero, new Vector3(0.48f, 0.56f, 0.48f), woodMat);
        BuildPart(ns.transform, "NS_TopCounter", PrimitiveType.Cube, new Vector3(0f, 0.635f, 0f), Vector3.zero, new Vector3(0.51f, 0.03f, 0.51f), woodMat);

        BuildPart(ns.transform, "NS_Drawer_Front", PrimitiveType.Cube, new Vector3(0f, 0.51f, 0.245f), Vector3.zero, new Vector3(0.43f, 0.14f, 0.02f), woodMat);
        BuildPart(ns.transform, "NS_Drawer_Handle", PrimitiveType.Cube, new Vector3(0f, 0.51f, 0.268f), Vector3.zero, new Vector3(0.12f, 0.018f, 0.025f), chromeMat);

        BuildPart(ns.transform, "NS_Door_Front", PrimitiveType.Cube, new Vector3(0f, 0.25f, 0.245f), Vector3.zero, new Vector3(0.43f, 0.34f, 0.02f), woodMat);
        BuildPart(ns.transform, "NS_Door_Handle", PrimitiveType.Cube, new Vector3(-0.15f, 0.32f, 0.268f), Vector3.zero, new Vector3(0.018f, 0.11f, 0.025f), chromeMat);
    }

    private static void SetupPillBottle(Vector3 worldPos, Material bottleMat, Material capMat)
    {
        GameObject bottle = GameObject.Find("Capsule");
        if (bottle == null) bottle = new GameObject("Capsule");

        for (int i = bottle.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(bottle.transform.GetChild(i).gameObject);
        }

        MeshRenderer rootMr = bottle.GetComponent<MeshRenderer>();
        if (rootMr != null) DestroyImmediate(rootMr);
        MeshFilter rootMf = bottle.GetComponent<MeshFilter>();
        if (rootMf != null) DestroyImmediate(rootMf);

        bottle.transform.SetParent(null);
        bottle.transform.position = worldPos;
        bottle.transform.rotation = Quaternion.identity;
        bottle.transform.localScale = Vector3.one;

        BuildPart(bottle.transform, "Bottom", PrimitiveType.Cylinder, new Vector3(0f, 0.045f, 0f), Vector3.zero, new Vector3(0.055f, 0.045f, 0.055f), bottleMat);
        BuildPart(bottle.transform, "Label_Band", PrimitiveType.Cylinder, new Vector3(0f, 0.045f, 0f), Vector3.zero, new Vector3(0.057f, 0.024f, 0.057f), capMat);
        BuildPart(bottle.transform, "Cap", PrimitiveType.Cylinder, new Vector3(0f, 0.10f, 0f), Vector3.zero, new Vector3(0.062f, 0.014f, 0.062f), capMat);
    }

    private static void BuildDetailedMedicalCabinet(Vector3 worldPos, Material steelMat, Material chromeMat)
    {
        GameObject cab = GameObject.Find("Med_Cabinet");
        if (cab != null) DestroyImmediate(cab);

        cab = new GameObject("Med_Cabinet");
        cab.transform.position = worldPos;
        cab.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        cab.transform.localScale = Vector3.one;

        BuildPart(cab.transform, "Cabinet_Plinth", PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), Vector3.zero, new Vector3(1.06f, 0.08f, 0.48f), steelMat);
        BuildPart(cab.transform, "Cabinet_Body", PrimitiveType.Cube, new Vector3(0f, 1.04f, 0f), Vector3.zero, new Vector3(1.10f, 1.92f, 0.50f), steelMat);
        BuildPart(cab.transform, "Cabinet_TopCornice", PrimitiveType.Cube, new Vector3(0f, 2.02f, 0f), Vector3.zero, new Vector3(1.14f, 0.05f, 0.54f), steelMat);

        BuildPart(cab.transform, "Cabinet_Door_Left", PrimitiveType.Cube, new Vector3(-0.265f, 1.04f, 0.255f), Vector3.zero, new Vector3(0.51f, 1.82f, 0.02f), steelMat);
        BuildPart(cab.transform, "Cabinet_Door_Right", PrimitiveType.Cube, new Vector3(0.265f, 1.04f, 0.255f), Vector3.zero, new Vector3(0.51f, 1.82f, 0.02f), steelMat);

        BuildPart(cab.transform, "Cabinet_Handle_Left", PrimitiveType.Cube, new Vector3(-0.055f, 1.06f, 0.28f), Vector3.zero, new Vector3(0.02f, 0.24f, 0.03f), chromeMat);
        BuildPart(cab.transform, "Cabinet_Handle_Right", PrimitiveType.Cube, new Vector3(0.055f, 1.06f, 0.28f), Vector3.zero, new Vector3(0.02f, 0.24f, 0.03f), chromeMat);
        BuildPart(cab.transform, "Cabinet_KeyLock", PrimitiveType.Cylinder, new Vector3(0.055f, 1.24f, 0.268f), new Vector3(90f, 0f, 0f), new Vector3(0.025f, 0.01f, 0.025f), chromeMat);
    }

    private static void BuildMedicalTerminal(Vector3 worldPos, Material bodyMat, Material chromeMat, Material rubberMat, Material screenMat, Material ledMat)
    {
        GameObject term = GameObject.Find("Med_Terminal");
        if (term != null) DestroyImmediate(term);

        term = new GameObject("Med_Terminal");
        term.transform.position = worldPos;
        term.transform.rotation = Quaternion.Euler(0f, -18f, 0f);
        term.transform.localScale = Vector3.one;

        BuildPart(term.transform, "Terminal_Base", PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), Vector3.zero, new Vector3(0.54f, 0.05f, 0.54f), chromeMat);
        Vector3[] wheelPos = new Vector3[]
        {
            new Vector3(-0.22f, 0.025f, -0.22f),
            new Vector3( 0.22f, 0.025f, -0.22f),
            new Vector3(-0.22f, 0.025f,  0.22f),
            new Vector3( 0.22f, 0.025f,  0.22f)
        };
        for (int i = 0; i < wheelPos.Length; i++)
        {
            BuildPart(term.transform, $"Terminal_Wheel_{i + 1}", PrimitiveType.Sphere, wheelPos[i], Vector3.zero, new Vector3(0.05f, 0.05f, 0.05f), rubberMat);
        }

        BuildPart(term.transform, "Terminal_Column", PrimitiveType.Cylinder, new Vector3(0f, 0.58f, 0f), Vector3.zero, new Vector3(0.065f, 0.54f, 0.065f), chromeMat);
        BuildPart(term.transform, "Terminal_AccessoryTray", PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.06f), Vector3.zero, new Vector3(0.36f, 0.025f, 0.22f), bodyMat);

        GameObject monitorPivot = new GameObject("Terminal_Monitor_Head");
        monitorPivot.transform.SetParent(term.transform);
        monitorPivot.transform.localPosition = new Vector3(0f, 1.24f, 0f);
        monitorPivot.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);

        BuildPart(monitorPivot.transform, "Monitor_Casing", PrimitiveType.Cube, Vector3.zero, Vector3.zero, new Vector3(0.60f, 0.44f, 0.13f), bodyMat);
        BuildPart(monitorPivot.transform, "Monitor_Screen", PrimitiveType.Cube, new Vector3(0f, 0.01f, -0.067f), Vector3.zero, new Vector3(0.52f, 0.35f, 0.01f), screenMat);
        BuildPart(monitorPivot.transform, "Monitor_PowerLED", PrimitiveType.Sphere, new Vector3(0.24f, -0.19f, -0.068f), Vector3.zero, new Vector3(0.016f, 0.016f, 0.016f), ledMat);

        AudioSource audio = term.AddComponent<AudioSource>();
        audio.spatialBlend = 1.0f;
        audio.loop = true;
        audio.volume = 0.5f;
    }

    private static void SetupPatientCamera()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin (VR)");
        GameObject cameraOffset = GameObject.Find("Camera Offset");
        GameObject mainCamera = GameObject.Find("Main Camera");

        if (xrOrigin != null)
        {
            xrOrigin.transform.position = new Vector3(-1.50f, 0.92f, -4.52f);
            xrOrigin.transform.rotation = Quaternion.Euler(11f, 3f, 0f);
        }
        if (cameraOffset != null)
        {
            cameraOffset.transform.localPosition = Vector3.zero;
            cameraOffset.transform.localRotation = Quaternion.identity;
        }
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }
    }

    private static GameObject BuildPart(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.Euler(localEuler);
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }
}