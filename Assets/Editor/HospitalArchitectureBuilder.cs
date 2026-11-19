using UnityEngine;
using UnityEditor;

public class HospitalArchitectureBuilder : EditorWindow
{
    [MenuItem("Tools/Hospital Ward/1. Build Architecture & Ceiling Lights")]
    public static void BuildArchitecture()
    {
        Material floorMat = HospitalMaterials.CreateTiledFloorMaterial();
        Material wallUpperMat = HospitalMaterials.CreateMaterial("Hospital_Wall_Upper_Mat", new Color(0.92f, 0.93f, 0.91f), 0.12f);
        Material wallLowerMat = HospitalMaterials.CreateMaterial("Hospital_Wall_Lower_Mat", new Color(0.30f, 0.50f, 0.46f), 0.25f);
        Material moldingMat = HospitalMaterials.CreateMaterial("Hospital_Molding_Mat", new Color(0.84f, 0.86f, 0.85f), 0.35f);
        Material baseboardMat = HospitalMaterials.CreateMaterial("Hospital_Baseboard_Mat", new Color(0.20f, 0.23f, 0.24f), 0.25f);
        Material ceilingMat = HospitalMaterials.CreateMaterial("Hospital_Ceiling_Mat", new Color(0.95f, 0.96f, 0.96f), 0.08f);
        Material fixtureFrameMat = HospitalMaterials.CreateMaterial("Ceiling_Fixture_Frame_Mat", new Color(0.65f, 0.68f, 0.70f), 0.7f, 0.8f);
        Material tubeGlowMat = HospitalMaterials.CreateEmissiveMaterial("Ceiling_Light_Tube_Mat", Color.white, new Color(0.92f, 0.97f, 1.0f), 2.8f);
        Material doorWoodMat = HospitalMaterials.CreateMaterial("Hospital_Door_Mat", new Color(0.75f, 0.72f, 0.67f), 0.3f);
        Material chromeMat = HospitalMaterials.CreateMaterial("Hospital_Chrome_Mat", new Color(0.80f, 0.82f, 0.85f), 0.9f, 0.95f);
        Material glassMat = HospitalMaterials.CreateMaterial("Hospital_Window_Glass_Mat", new Color(0.62f, 0.76f, 0.82f), 0.85f, 0.1f);

        GameObject envRoot = GameObject.Find("Hospital_Environment");
        if (envRoot == null) envRoot = new GameObject("Hospital_Environment");
        envRoot.transform.position = Vector3.zero;
        envRoot.transform.localScale = Vector3.one;

        GameObject floor = GameObject.Find("Plane");
        if (floor == null) floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Plane";
        floor.transform.SetParent(envRoot.transform);
        floor.transform.position = Vector3.zero;
        floor.transform.rotation = Quaternion.identity;
        floor.transform.localScale = Vector3.one;
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        BuildBox("Wall_Back", envRoot.transform, new Vector3(0f, 1.6f, -5.0f), new Vector3(10.2f, 3.2f, 0.2f), wallUpperMat);
        BuildBox("Wall_Front", envRoot.transform, new Vector3(0f, 1.6f, 5.0f), new Vector3(10.2f, 3.2f, 0.2f), wallUpperMat);
        BuildBox("Wall_Left", envRoot.transform, new Vector3(-5.0f, 1.6f, 0f), new Vector3(0.2f, 3.2f, 10.2f), wallUpperMat);
        BuildBox("Wall_Right", envRoot.transform, new Vector3(5.0f, 1.6f, 0f), new Vector3(0.2f, 3.2f, 10.2f), wallUpperMat);
        BuildBox("Ceiling", envRoot.transform, new Vector3(0f, 3.2f, 0f), new Vector3(10.2f, 0.2f, 10.2f), ceilingMat);

        BuildBox("Wall_Panel_Back", envRoot.transform, new Vector3(0f, 0.65f, -4.89f), new Vector3(10.0f, 1.30f, 0.03f), wallLowerMat);
        BuildBox("Wall_Panel_Front", envRoot.transform, new Vector3(0f, 0.65f, 4.89f), new Vector3(10.0f, 1.30f, 0.03f), wallLowerMat);
        BuildBox("Wall_Panel_Left", envRoot.transform, new Vector3(-4.89f, 0.65f, 0f), new Vector3(0.03f, 1.30f, 10.0f), wallLowerMat);
        BuildBox("Wall_Panel_Right", envRoot.transform, new Vector3(4.89f, 0.65f, 0f), new Vector3(0.03f, 1.30f, 10.0f), wallLowerMat);

        BuildBox("Wall_Molding_Back", envRoot.transform, new Vector3(0f, 1.32f, -4.88f), new Vector3(10.0f, 0.05f, 0.04f), moldingMat);
        BuildBox("Wall_Molding_Front", envRoot.transform, new Vector3(0f, 1.32f, 4.88f), new Vector3(10.0f, 0.05f, 0.04f), moldingMat);
        BuildBox("Wall_Molding_Left", envRoot.transform, new Vector3(-4.88f, 1.32f, 0f), new Vector3(0.04f, 0.05f, 10.0f), moldingMat);
        BuildBox("Wall_Molding_Right", envRoot.transform, new Vector3(4.88f, 1.32f, 0f), new Vector3(0.04f, 0.05f, 10.0f), moldingMat);

        BuildBox("Baseboard_Back", envRoot.transform, new Vector3(0f, 0.06f, -4.87f), new Vector3(10.0f, 0.12f, 0.04f), baseboardMat);
        BuildBox("Baseboard_Front", envRoot.transform, new Vector3(0f, 0.06f, 4.87f), new Vector3(10.0f, 0.12f, 0.04f), baseboardMat);
        BuildBox("Baseboard_Left", envRoot.transform, new Vector3(-4.87f, 0.06f, 0f), new Vector3(0.04f, 0.12f, 10.0f), baseboardMat);
        BuildBox("Baseboard_Right", envRoot.transform, new Vector3(4.87f, 0.06f, 0f), new Vector3(0.04f, 0.12f, 10.0f), baseboardMat);

        BuildWardDoor(envRoot.transform, new Vector3(1.8f, 0f, 4.86f), doorWoodMat, chromeMat, glassMat);

        GameObject oldPoint = GameObject.Find("Point Light");
        if (oldPoint != null) DestroyImmediate(oldPoint);

        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            Light dl = dirLight.GetComponent<Light>();
            if (dl != null) dl.intensity = 0.25f;
        }

        Vector3[] fixturePositions = new Vector3[]
        {
            new Vector3(-1.5f, 3.08f, -3.0f),
            new Vector3( 2.0f, 3.08f, -3.0f),
            new Vector3(-1.5f, 3.08f,  1.5f),
            new Vector3( 2.0f, 3.08f,  1.5f)
        };

        for (int i = 0; i < fixturePositions.Length; i++)
        {
            BuildCeilingFixture(envRoot.transform, $"Ceiling_Light_Fixture_{i + 1}", fixturePositions[i], fixtureFrameMat, tubeGlowMat, i == 0);
        }

        Debug.Log("[HospitalArchitectureBuilder] Architecture, door, and 4 ceiling light fixtures built.");
    }

    private static void BuildWardDoor(Transform parent, Vector3 pos, Material doorMat, Material chromeMat, Material glassMat)
    {
        Transform existing = parent.Find("Hospital_Ward_Door");
        if (existing != null) DestroyImmediate(existing.gameObject);

        GameObject doorRoot = new GameObject("Hospital_Ward_Door");
        doorRoot.transform.SetParent(parent);
        doorRoot.transform.position = pos;

        BuildLocalBox("Door_Frame_Left", doorRoot.transform, new Vector3(-0.56f, 1.08f, 0f), new Vector3(0.08f, 2.16f, 0.12f), chromeMat);
        BuildLocalBox("Door_Frame_Right", doorRoot.transform, new Vector3(0.56f, 1.08f, 0f), new Vector3(0.08f, 2.16f, 0.12f), chromeMat);
        BuildLocalBox("Door_Frame_Top", doorRoot.transform, new Vector3(0f, 2.16f, 0f), new Vector3(1.20f, 0.08f, 0.12f), chromeMat);

        BuildLocalBox("Door_Leaf", doorRoot.transform, new Vector3(0f, 1.06f, 0f), new Vector3(1.04f, 2.10f, 0.06f), doorMat);
        BuildLocalBox("Door_Window_Glass", doorRoot.transform, new Vector3(0.18f, 1.48f, -0.032f), new Vector3(0.28f, 0.62f, 0.01f), glassMat);
        BuildLocalBox("Door_Window_Frame", doorRoot.transform, new Vector3(0.18f, 1.48f, -0.028f), new Vector3(0.32f, 0.66f, 0.015f), chromeMat);

        BuildLocalBox("Door_Lock_Plate", doorRoot.transform, new Vector3(-0.38f, 1.02f, -0.036f), new Vector3(0.05f, 0.18f, 0.015f), chromeMat);
        BuildLocalBox("Door_Handle_Neck", doorRoot.transform, new Vector3(-0.38f, 1.05f, -0.065f), new Vector3(0.025f, 0.025f, 0.05f), chromeMat);
        BuildLocalBox("Door_Handle_Lever", doorRoot.transform, new Vector3(-0.32f, 1.05f, -0.09f), new Vector3(0.14f, 0.022f, 0.022f), chromeMat);
    }

    private static void BuildCeilingFixture(Transform parent, string name, Vector3 pos, Material frameMat, Material glowMat, bool castShadows)
    {
        Transform existing = parent.Find(name);
        if (existing != null) DestroyImmediate(existing.gameObject);

        GameObject fixture = new GameObject(name);
        fixture.transform.SetParent(parent);
        fixture.transform.position = pos;

        BuildLocalBox("Fixture_Housing", fixture.transform, Vector3.zero, new Vector3(0.68f, 0.06f, 1.36f), frameMat);
        BuildLocalBox("Fluorescent_Tube_Left", fixture.transform, new Vector3(-0.16f, -0.035f, 0f), new Vector3(0.06f, 0.04f, 1.24f), glowMat);
        BuildLocalBox("Fluorescent_Tube_Right", fixture.transform, new Vector3(0.16f, -0.035f, 0f), new Vector3(0.06f, 0.04f, 1.24f), glowMat);

        GameObject lightObj = new GameObject("Fixture_Light");
        lightObj.transform.SetParent(fixture.transform);
        lightObj.transform.localPosition = new Vector3(0f, -0.18f, 0f);
        Light lightComp = lightObj.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.color = new Color(0.93f, 0.97f, 1.0f);
        lightComp.intensity = 2.4f;
        lightComp.range = 8.5f;
        lightComp.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
    }

    private static GameObject BuildBox(string name, Transform parent, Vector3 worldPos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null) obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = worldPos;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }

    private static GameObject BuildLocalBox(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }
}