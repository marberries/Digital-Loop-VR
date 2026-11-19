using UnityEngine;
using UnityEditor;

public class HospitalEnvironmentBuilder : EditorWindow
{
    [MenuItem("Tools/Палата/1. Построить стены, пол и свет")]
    public static void BuildEnvironment()
    {
        // Создание базовых материалов больницы (URP Lit)
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        Material floorMat = GetOrCreateMaterial("Hospital_Floor_Mat", litShader, new Color(0.72f, 0.75f, 0.77f), 0.35f);
        Material wallUpperMat = GetOrCreateMaterial("Hospital_Wall_Upper_Mat", litShader, new Color(0.91f, 0.92f, 0.90f), 0.1f);
        Material wallLowerMat = GetOrCreateMaterial("Hospital_Wall_Lower_Mat", litShader, new Color(0.32f, 0.52f, 0.48f), 0.25f);
        Material baseboardMat = GetOrCreateMaterial("Hospital_Baseboard_Mat", litShader, new Color(0.22f, 0.25f, 0.26f), 0.2f);
        Material ceilingMat = GetOrCreateMaterial("Hospital_Ceiling_Mat", litShader, new Color(0.95f, 0.95f, 0.95f), 0.05f);

        GameObject envRoot = GameObject.Find("Hospital_Environment");
        if (envRoot == null) envRoot = new GameObject("Hospital_Environment");
        envRoot.transform.position = Vector3.zero;
        envRoot.transform.localScale = Vector3.one;

        // 1. Пол (10x10 метров)
        GameObject floor = GameObject.Find("Plane");
        if (floor == null) floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Plane";
        floor.transform.SetParent(envRoot.transform);
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = Vector3.one;
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        // 2. Основные стены (верхняя светлая часть) и потолок
        SetupBox("Wall_Back", envRoot.transform, new Vector3(0f, 1.6f, -5.0f), new Vector3(10.2f, 3.2f, 0.2f), wallUpperMat);
        SetupBox("Wall_Front", envRoot.transform, new Vector3(0f, 1.6f, 5.0f), new Vector3(10.2f, 3.2f, 0.2f), wallUpperMat);
        SetupBox("Wall_Left", envRoot.transform, new Vector3(-5.0f, 1.6f, 0f), new Vector3(0.2f, 3.2f, 10.2f), wallUpperMat);
        SetupBox("Wall_Right", envRoot.transform, new Vector3(5.0f, 1.6f, 0f), new Vector3(0.2f, 3.2f, 10.2f), wallUpperMat);
        SetupBox("Ceiling", envRoot.transform, new Vector3(0f, 3.2f, 0f), new Vector3(10.2f, 0.2f, 10.2f), ceilingMat);

        // 3. Нижние больничные панели (покраска до высоты 1.35м)
        SetupBox("Wall_Panel_Back", envRoot.transform, new Vector3(0f, 0.675f, -4.89f), new Vector3(10.0f, 1.35f, 0.03f), wallLowerMat);
        SetupBox("Wall_Panel_Front", envRoot.transform, new Vector3(0f, 0.675f, 4.89f), new Vector3(10.0f, 1.35f, 0.03f), wallLowerMat);
        SetupBox("Wall_Panel_Left", envRoot.transform, new Vector3(-4.89f, 0.675f, 0f), new Vector3(0.03f, 1.35f, 10.0f), wallLowerMat);
        SetupBox("Wall_Panel_Right", envRoot.transform, new Vector3(4.89f, 0.675f, 0f), new Vector3(0.03f, 1.35f, 10.0f), wallLowerMat);

        // 4. Плинтусы по периметру
        SetupBox("Baseboard_Back", envRoot.transform, new Vector3(0f, 0.06f, -4.87f), new Vector3(10.0f, 0.12f, 0.04f), baseboardMat);
        SetupBox("Baseboard_Front", envRoot.transform, new Vector3(0f, 0.06f, 4.87f), new Vector3(10.0f, 0.12f, 0.04f), baseboardMat);
        SetupBox("Baseboard_Left", envRoot.transform, new Vector3(-4.87f, 0.06f, 0f), new Vector3(0.04f, 0.12f, 10.0f), baseboardMat);
        SetupBox("Baseboard_Right", envRoot.transform, new Vector3(4.87f, 0.06f, 0f), new Vector3(0.04f, 0.12f, 10.0f), baseboardMat);

        // 5. Настройка мягкого больничного освещения
        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            Light dl = dirLight.GetComponent<Light>();
            if (dl != null) dl.intensity = 0.45f;
        }

        GameObject pointLight = GameObject.Find("Point Light");
        if (pointLight == null)
        {
            pointLight = new GameObject("Point Light");
            pointLight.AddComponent<Light>().type = LightType.Point;
        }
        pointLight.transform.SetParent(envRoot.transform);
        pointLight.transform.position = new Vector3(-1.2f, 2.85f, -2.5f);
        Light pl = pointLight.GetComponent<Light>();
        pl.color = new Color(0.92f, 0.96f, 1.0f);
        pl.intensity = 3.2f;
        pl.range = 12f;

        Debug.Log("Окружение палаты (стены, панели, пол и свет) успешно настроено.");
    }

    private static void SetupBox(string objName, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj == null) obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = objName;
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
    }

    public static Material GetOrCreateMaterial(string matName, Shader shader, Color color, float smoothness)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = $"Assets/Materials/{matName}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }
}