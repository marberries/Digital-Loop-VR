using UnityEngine;
using UnityEditor;

public class HospitalBedAndIVBuilder : EditorWindow
{
    [MenuItem("Tools/Hospital Ward/2. Build Detailed Bed & IV Stand")]
    public static void BuildBedAndIV()
    {
        Material steelMat = HospitalMaterials.CreateMaterial("Bed_Steel_Mat", new Color(0.62f, 0.65f, 0.68f), 0.78f, 0.85f);
        Material chromeMat = HospitalMaterials.CreateMaterial("Hospital_Chrome_Mat", new Color(0.82f, 0.84f, 0.87f), 0.92f, 0.95f);
        Material rubberMat = HospitalMaterials.CreateMaterial("Wheel_Rubber_Mat", new Color(0.12f, 0.13f, 0.14f), 0.25f, 0.0f);
        Material sheetMat = HospitalMaterials.CreateMaterial("Bed_Sheet_Mat", new Color(0.91f, 0.93f, 0.95f), 0.15f, 0.0f);
        Material blanketMat = HospitalMaterials.CreateMaterial("Bed_Blanket_Mat", new Color(0.32f, 0.46f, 0.56f), 0.12f, 0.0f);
        Material woodMat = HospitalMaterials.CreateMaterial("Bed_Wood_Mat", new Color(0.76f, 0.73f, 0.68f), 0.32f, 0.0f);
        Material ivFluidMat = HospitalMaterials.CreateMaterial("IV_Fluid_Bag_Mat", new Color(0.75f, 0.89f, 0.92f), 0.85f, 0.1f);
        Material ivTubeMat = HospitalMaterials.CreateMaterial("IV_Tube_Mat", new Color(0.85f, 0.90f, 0.92f), 0.75f, 0.0f);

        DestroyGlobal("Corner_Trim_Left");
        DestroyGlobal("Corner_Trim_Right");
        DestroyGlobal("Hospital_Bed_Assembly");

        GameObject bedRoot = new GameObject("Hospital_Bed_Assembly");
        bedRoot.transform.position = new Vector3(-1.50f, 0f, -3.80f);
        bedRoot.transform.rotation = Quaternion.identity;
        bedRoot.transform.localScale = Vector3.one;

        BuildPart(bedRoot.transform, "Bed_Main_Frame", PrimitiveType.Cube, new Vector3(0f, 0.34f, 0f), Vector3.zero, new Vector3(1.16f, 0.07f, 2.12f), steelMat);
        BuildPart(bedRoot.transform, "Bed_Lower_Crossbar", PrimitiveType.Cube, new Vector3(0f, 0.18f, 0f), Vector3.zero, new Vector3(0.95f, 0.04f, 1.65f), steelMat);

        Vector2[] cornerOffsets = new Vector2[]
        {
            new Vector2(-0.52f,  0.98f),
            new Vector2( 0.52f,  0.98f),
            new Vector2(-0.52f, -0.98f),
            new Vector2( 0.52f, -0.98f)
        };

        for (int i = 0; i < cornerOffsets.Length; i++)
        {
            float cx = cornerOffsets[i].x;
            float cz = cornerOffsets[i].y;
            BuildPart(bedRoot.transform, $"Bed_Leg_{i + 1}", PrimitiveType.Cylinder, new Vector3(cx, 0.21f, cz), Vector3.zero, new Vector3(0.055f, 0.13f, 0.055f), chromeMat);
            BuildPart(bedRoot.transform, $"Bed_Caster_Wheel_{i + 1}", PrimitiveType.Cylinder, new Vector3(cx, 0.045f, cz), new Vector3(0f, 0f, 90f), new Vector3(0.09f, 0.025f, 0.09f), rubberMat);
        }

        GameObject mattress = GameObject.Find("Bed");
        if (mattress == null) mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mattress.name = "Bed";
        mattress.transform.SetParent(bedRoot.transform);
        mattress.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        mattress.transform.localRotation = Quaternion.identity;
        mattress.transform.localScale = new Vector3(1.10f, 0.15f, 2.04f);
        mattress.GetComponent<Renderer>().sharedMaterial = sheetMat;

        GameObject blanket = GameObject.Find("Bed_Blanket");
        if (blanket == null) blanket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blanket.name = "Bed_Blanket";
        blanket.transform.SetParent(bedRoot.transform);
        blanket.transform.localPosition = new Vector3(0f, 0.54f, 0.34f);
        blanket.transform.localRotation = Quaternion.identity;
        blanket.transform.localScale = new Vector3(1.13f, 0.045f, 1.34f);
        blanket.GetComponent<Renderer>().sharedMaterial = blanketMat;

        BuildPart(bedRoot.transform, "Bed_Blanket_Fold", PrimitiveType.Cube, new Vector3(0f, 0.555f, -0.26f), Vector3.zero, new Vector3(1.14f, 0.055f, 0.18f), sheetMat);

        GameObject pillow = GameObject.Find("Bed_Pillow");
        if (pillow == null) pillow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillow.name = "Bed_Pillow";
        pillow.transform.SetParent(bedRoot.transform);
        pillow.transform.localPosition = new Vector3(0f, 0.58f, -0.72f);
        pillow.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
        pillow.transform.localScale = new Vector3(0.68f, 0.11f, 0.38f);
        pillow.GetComponent<Renderer>().sharedMaterial = sheetMat;

        GameObject headboard = GameObject.Find("Bed_Headboard");
        if (headboard == null) headboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headboard.name = "Bed_Headboard";
        headboard.transform.SetParent(bedRoot.transform);
        headboard.transform.localPosition = new Vector3(0f, 0.68f, -1.07f);
        headboard.transform.localRotation = Quaternion.identity;
        headboard.transform.localScale = new Vector3(1.16f, 0.72f, 0.045f);
        headboard.GetComponent<Renderer>().sharedMaterial = woodMat;

        BuildPart(bedRoot.transform, "Headboard_Post_L", PrimitiveType.Cylinder, new Vector3(-0.59f, 0.56f, -1.07f), Vector3.zero, new Vector3(0.04f, 0.56f, 0.04f), chromeMat);
        BuildPart(bedRoot.transform, "Headboard_Post_R", PrimitiveType.Cylinder, new Vector3( 0.59f, 0.56f, -1.07f), Vector3.zero, new Vector3(0.04f, 0.56f, 0.04f), chromeMat);

        BuildPart(bedRoot.transform, "Bed_Footboard", PrimitiveType.Cube, new Vector3(0f, 0.48f, 1.07f), Vector3.zero, new Vector3(1.16f, 0.38f, 0.045f), woodMat);
        BuildPart(bedRoot.transform, "Footboard_Post_L", PrimitiveType.Cylinder, new Vector3(-0.59f, 0.42f, 1.07f), Vector3.zero, new Vector3(0.04f, 0.42f, 0.04f), chromeMat);
        BuildPart(bedRoot.transform, "Footboard_Post_R", PrimitiveType.Cylinder, new Vector3( 0.59f, 0.42f, 1.07f), Vector3.zero, new Vector3(0.04f, 0.42f, 0.04f), chromeMat);

        BuildSideRail(bedRoot.transform, "Bed_SideRail_Left", -0.59f, chromeMat, steelMat);
        BuildSideRail(bedRoot.transform, "Bed_SideRail_Right", 0.59f, chromeMat, steelMat);

        // Капельница строго слева от кровати
        RebuildIVStand(new Vector3(-2.30f, 0f, -3.95f), chromeMat, rubberMat, ivFluidMat, ivTubeMat);

        Debug.Log("[HospitalBedAndIVBuilder] Detailed bed and left-side IV stand built cleanly.");
    }

    private static void BuildSideRail(Transform parent, string name, float xPos, Material chromeMat, Material steelMat)
    {
        GameObject rail = new GameObject(name);
        rail.transform.SetParent(parent);
        rail.transform.localPosition = new Vector3(xPos, 0.52f, -0.25f);

        BuildPart(rail.transform, "Rail_TopBar", PrimitiveType.Cylinder, new Vector3(0f, 0.14f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.024f, 0.52f, 0.024f), chromeMat);
        BuildPart(rail.transform, "Rail_BottomBar", PrimitiveType.Cylinder, new Vector3(0f, -0.08f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.024f, 0.52f, 0.024f), chromeMat);
        BuildPart(rail.transform, "Rail_Post_Front", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0.46f), Vector3.zero, new Vector3(0.02f, 0.11f, 0.02f), chromeMat);
        BuildPart(rail.transform, "Rail_Post_Mid", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0f), Vector3.zero, new Vector3(0.02f, 0.11f, 0.02f), chromeMat);
        BuildPart(rail.transform, "Rail_Post_Back", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, -0.46f), Vector3.zero, new Vector3(0.02f, 0.11f, 0.02f), chromeMat);
        BuildPart(rail.transform, "Rail_Latch", PrimitiveType.Cube, new Vector3(0f, 0.14f, 0.48f), Vector3.zero, new Vector3(0.035f, 0.045f, 0.05f), steelMat);
    }

    private static void RebuildIVStand(Vector3 worldPos, Material chromeMat, Material rubberMat, Material fluidMat, Material tubeMat)
    {
        GameObject ivStand = GameObject.Find("IV_Stand_Full");
        if (ivStand != null) DestroyImmediate(ivStand);

        ivStand = new GameObject("IV_Stand_Full");
        ivStand.transform.position = worldPos;
        ivStand.transform.rotation = Quaternion.Euler(0f, 20f, 0f);
        ivStand.transform.localScale = Vector3.one;

        BuildPart(ivStand.transform, "IV_Base_Hub", PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), Vector3.zero, new Vector3(0.10f, 0.05f, 0.10f), chromeMat);
        BuildPart(ivStand.transform, "IV_Base_Cross_A", PrimitiveType.Cube, new Vector3(0f, 0.07f, 0f), Vector3.zero, new Vector3(0.56f, 0.03f, 0.04f), chromeMat);
        BuildPart(ivStand.transform, "IV_Base_Cross_B", PrimitiveType.Cube, new Vector3(0f, 0.07f, 0f), new Vector3(0f, 90f, 0f), new Vector3(0.56f, 0.03f, 0.04f), chromeMat);

        Vector3[] wheelOffsets = new Vector3[]
        {
            new Vector3( 0.26f, 0.03f, 0f),
            new Vector3(-0.26f, 0.03f, 0f),
            new Vector3(0f, 0.03f,  0.26f),
            new Vector3(0f, 0.03f, -0.26f)
        };
        for (int i = 0; i < wheelOffsets.Length; i++)
        {
            BuildPart(ivStand.transform, $"IV_Wheel_{i + 1}", PrimitiveType.Sphere, wheelOffsets[i], Vector3.zero, new Vector3(0.055f, 0.055f, 0.055f), rubberMat);
        }

        BuildPart(ivStand.transform, "IV_Pole_Lower", PrimitiveType.Cylinder, new Vector3(0f, 0.62f, 0f), Vector3.zero, new Vector3(0.032f, 0.56f, 0.032f), chromeMat);
        BuildPart(ivStand.transform, "IV_Height_Collar", PrimitiveType.Cylinder, new Vector3(0f, 1.18f, 0f), Vector3.zero, new Vector3(0.048f, 0.035f, 0.048f), rubberMat);
        BuildPart(ivStand.transform, "IV_Pole_Upper", PrimitiveType.Cylinder, new Vector3(0f, 1.58f, 0f), Vector3.zero, new Vector3(0.020f, 0.42f, 0.020f), chromeMat);
        BuildPart(ivStand.transform, "IV_Hook_Bar", PrimitiveType.Cylinder, new Vector3(0f, 1.98f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.015f, 0.16f, 0.015f), chromeMat);

        BuildPart(ivStand.transform, "IV_Saline_Bag", PrimitiveType.Cube, new Vector3(0.13f, 1.80f, 0f), Vector3.zero, new Vector3(0.11f, 0.22f, 0.045f), fluidMat);
        BuildPart(ivStand.transform, "IV_Drip_Chamber", PrimitiveType.Cylinder, new Vector3(0.13f, 1.62f, 0f), Vector3.zero, new Vector3(0.024f, 0.045f, 0.024f), tubeMat);
        BuildPart(ivStand.transform, "IV_Tube_Vertical", PrimitiveType.Cylinder, new Vector3(0.13f, 1.24f, 0f), Vector3.zero, new Vector3(0.008f, 0.34f, 0.008f), tubeMat);
        BuildPart(ivStand.transform, "IV_Tube_To_Bed", PrimitiveType.Cylinder, new Vector3(0.32f, 0.76f, 0.05f), new Vector3(12f, 0f, -56f), new Vector3(0.008f, 0.26f, 0.008f), tubeMat);
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

    private static void DestroyGlobal(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null) DestroyImmediate(obj);
    }
}