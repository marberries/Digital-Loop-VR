using UnityEngine;
using UnityEditor;

public class HospitalFurnitureBuilder : EditorWindow
{
    [MenuItem("Tools/Палата/2. Расставить мебель и терминал")]
    public static void BuildFurnitureAndTerminal()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        Material metalMat = HospitalEnvironmentBuilder.GetOrCreateMaterial("Hospital_Metal_Mat", litShader, new Color(0.55f, 0.58f, 0.60f), 0.75f);
        Material sheetMat = HospitalEnvironmentBuilder.GetOrCreateMaterial("Hospital_Sheet_Mat", litShader, new Color(0.88f, 0.90f, 0.92f), 0.15f);
        Material blanketMat = HospitalEnvironmentBuilder.GetOrCreateMaterial("Hospital_Blanket_Mat", litShader, new Color(0.36f, 0.48f, 0.56f), 0.1f);
        Material woodMat = HospitalEnvironmentBuilder.GetOrCreateMaterial("Hospital_Wood_Mat", litShader, new Color(0.76f, 0.74f, 0.70f), 0.3f);
        Material darkPlasticMat = HospitalEnvironmentBuilder.GetOrCreateMaterial("Hospital_DarkPlastic_Mat", litShader, new Color(0.14f, 0.16f, 0.18f), 0.4f);

        // Светящийся экран терминала и линия пульса
        Material screenMat = HospitalEnvironmentBuilder.GetOrCreateMaterial("Terminal_Screen_Mat", litShader, new Color(0.03f, 0.08f, 0.09f), 0.8f);
        Material ecgGlowMat = HospitalEnvironmentBuilder.GetOrCreateMaterial("Terminal_ECG_Glow_Mat", litShader, new Color(0.1f, 0.95f, 0.45f), 0.5f);
        ecgGlowMat.EnableKeyword("_EMISSION");
        ecgGlowMat.SetColor("_EmissionColor", new Color(0.1f, 1.0f, 0.45f) * 2.2f);

        // Удаление старых кривых бортиков, если они остались
        DestroyIfExists("Corner_Trim_Left");
        DestroyIfExists("Corner_Trim_Right");

        // Корневой объект кровати с единичным масштабом (1,1,1)
        GameObject bedRoot = GameObject.Find("Hospital_Bed_Assembly");
        if (bedRoot == null) bedRoot = new GameObject("Hospital_Bed_Assembly");
        bedRoot.transform.position = Vector3.zero;
        bedRoot.transform.localScale = Vector3.one;

        // 1. Детализированная кровать у задней стены (центр X = -1.5, Z = -3.8)
        // Металлическая рама и 4 ножки
        CreateOrUpdatePart("Bed_Frame", bedRoot.transform, new Vector3(-1.5f, 0.34f, -3.8f), new Vector3(1.18f, 0.08f, 2.12f), metalMat);
        CreateOrUpdatePart("Bed_Leg_FL", bedRoot.transform, new Vector3(-2.04f, 0.16f, -2.79f), new Vector3(0.06f, 0.32f, 0.06f), metalMat);
        CreateOrUpdatePart("Bed_Leg_FR", bedRoot.transform, new Vector3(-0.96f, 0.16f, -2.79f), new Vector3(0.06f, 0.32f, 0.06f), metalMat);
        CreateOrUpdatePart("Bed_Leg_BL", bedRoot.transform, new Vector3(-2.04f, 0.16f, -4.81f), new Vector3(0.06f, 0.32f, 0.06f), metalMat);
        CreateOrUpdatePart("Bed_Leg_BR", bedRoot.transform, new Vector3(-0.96f, 0.16f, -4.81f), new Vector3(0.06f, 0.32f, 0.06f), metalMat);

        // Матрас (используется существующий объект Bed)
        GameObject bed = CreateOrUpdatePart("Bed", bedRoot.transform, new Vector3(-1.5f, 0.46f, -3.8f), new Vector3(1.12f, 0.16f, 2.06f), sheetMat);

        // Изголовье, низкая спинка в ногах и боковые поручни
        CreateOrUpdatePart("Bed_Headboard", bedRoot.transform, new Vector3(-1.5f, 0.65f, -4.87f), new Vector3(1.22f, 1.15f, 0.06f), woodMat);
        CreateOrUpdatePart("Bed_Footboard", bedRoot.transform, new Vector3(-1.5f, 0.44f, -2.73f), new Vector3(1.22f, 0.46f, 0.05f), woodMat);
        CreateOrUpdatePart("Bed_Rail_Left", bedRoot.transform, new Vector3(-2.10f, 0.56f, -4.05f), new Vector3(0.03f, 0.18f, 1.15f), metalMat);
        CreateOrUpdatePart("Bed_Rail_Right", bedRoot.transform, new Vector3(-0.90f, 0.56f, -4.05f), new Vector3(0.03f, 0.18f, 1.15f), metalMat);

        // Подушка и одеяло
        CreateOrUpdatePart("Bed_Pillow", bedRoot.transform, new Vector3(-1.5f, 0.59f, -4.52f), new Vector3(0.68f, 0.12f, 0.40f), sheetMat);
        CreateOrUpdatePart("Bed_Blanket", bedRoot.transform, new Vector3(-1.5f, 0.56f, -3.45f), new Vector3(1.14f, 0.05f, 1.32f), blanketMat);

        // 2. Прикроватная тумбочка справа и баночка с таблетками на ней
        GameObject nightstand = CreateOrUpdatePart("Nightstand", null, new Vector3(-0.52f, 0.32f, -4.15f), new Vector3(0.50f, 0.64f, 0.50f), woodMat);
        GameObject pillBottle = GameObject.Find("Capsule");
        if (pillBottle != null)
        {
            pillBottle.transform.SetParent(null);
            pillBottle.transform.position = new Vector3(-0.52f, 0.72f, -4.15f);
            pillBottle.transform.rotation = Quaternion.identity;
        }

        // 3. Капельница слева в прямой видимости игрока
        GameObject ivStand = GameObject.Find("IV_Stand_Full");
        if (ivStand != null)
        {
            ivStand.transform.SetParent(null);
            ivStand.transform.position = new Vector3(-2.35f, 0f, -3.65f);
            ivStand.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
        }

        // 4. Медицинский шкаф у левой стены
        GameObject medCabinet = GameObject.Find("Med_Cabinet");
        if (medCabinet != null)
        {
            medCabinet.transform.SetParent(null);
            medCabinet.transform.position = new Vector3(-4.45f, 1.0f, -1.2f);
            medCabinet.transform.rotation = Quaternion.identity;
            medCabinet.GetComponent<Renderer>().sharedMaterial = metalMat;
        }

        // 5. Медицинский терминал с кардиограммой прямо перед пациентом
        BuildMedicalTerminal(darkPlasticMat, metalMat, screenMat, ecgGlowMat);

        // 6. Позиция парализованного игрока на подушке (обзор на терминал, капельницу и тумбочку)
        GameObject xrOrigin = GameObject.Find("XR Origin (VR)");
        GameObject cameraOffset = GameObject.Find("Camera Offset");
        GameObject mainCamera = GameObject.Find("Main Camera");

        if (xrOrigin != null)
        {
            xrOrigin.transform.position = new Vector3(-1.5f, 0.90f, -4.52f);
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

        Debug.Log("Мебель палаты, медицинский терминал и обзор игрока успешно настроены.");
    }

    private static void BuildMedicalTerminal(Material bodyMat, Material standMat, Material screenMat, Material glowMat)
    {
        GameObject termRoot = GameObject.Find("Med_Terminal");
        if (termRoot == null) termRoot = new GameObject("Med_Terminal");

        // Устанавливается в ногах кровати, слегка развернут к лицу пациента
        termRoot.transform.position = new Vector3(-1.18f, 0f, -2.05f);
        termRoot.transform.rotation = Quaternion.Euler(0f, -18f, 0f);
        termRoot.transform.localScale = Vector3.one;

        // Колесная база и стойка
        CreateOrUpdateLocalPart("Terminal_Base", termRoot.transform, new Vector3(0f, 0.04f, 0f), Quaternion.identity, new Vector3(0.54f, 0.08f, 0.54f), standMat);
        CreateOrUpdateLocalPart("Terminal_Pole", termRoot.transform, new Vector3(0f, 0.58f, 0f), Quaternion.identity, new Vector3(0.07f, 1.08f, 0.07f), standMat);

        // Корпус монитора и экран
        CreateOrUpdateLocalPart("Terminal_Casing", termRoot.transform, new Vector3(0f, 1.22f, 0f), Quaternion.Euler(-12f, 0f, 0f), new Vector3(0.58f, 0.42f, 0.14f), bodyMat);
        GameObject screen = CreateOrUpdateLocalPart("Terminal_Screen", termRoot.transform, new Vector3(0f, 1.23f, -0.072f), Quaternion.Euler(-12f, 0f, 0f), new Vector3(0.50f, 0.34f, 0.01f), screenMat);

        // Светящаяся линия пульса (кардиограмма) на экране
        CreateOrUpdateLocalPart("ECG_Line_Left", screen.transform, new Vector3(-0.24f, 0f, -0.6f), Quaternion.identity, new Vector3(0.42f, 0.03f, 0.2f), glowMat);
        CreateOrUpdateLocalPart("ECG_Peak_Up", screen.transform, new Vector3(0.02f, 0.14f, -0.6f), Quaternion.Euler(0f, 0f, -68f), new Vector3(0.32f, 0.03f, 0.2f), glowMat);
        CreateOrUpdateLocalPart("ECG_Peak_Down", screen.transform, new Vector3(0.11f, -0.04f, -0.6f), Quaternion.Euler(0f, 0f, 68f), new Vector3(0.38f, 0.03f, 0.2f), glowMat);
        CreateOrUpdateLocalPart("ECG_Line_Right", screen.transform, new Vector3(0.32f, 0f, -0.6f), Quaternion.identity, new Vector3(0.32f, 0.03f, 0.2f), glowMat);

        // Источник звука для сердцебиения
        if (termRoot.GetComponent<AudioSource>() == null)
        {
            AudioSource audio = termRoot.AddComponent<AudioSource>();
            audio.spatialBlend = 1.0f;
            audio.loop = true;
            audio.volume = 0.5f;
        }
    }

    private static GameObject CreateOrUpdatePart(string objName, Transform parent, Vector3 worldPos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj == null) obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = objName;
        obj.transform.SetParent(parent);
        obj.transform.position = worldPos;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }

    private static GameObject CreateOrUpdateLocalPart(string objName, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 localScale, Material mat)
    {
        Transform existing = parent.Find(objName);
        GameObject obj = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = objName;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = localRot;
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }

    private static void DestroyIfExists(string objName)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null) DestroyImmediate(obj);
    }
}