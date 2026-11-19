using UnityEngine;
using UnityEditor;

public class RoomBuilder : EditorWindow
{
    [MenuItem("Tools/Выровнять палату")]
    public static void AlignHospitalRoom()
    {
        GameObject bed = GameObject.Find("Bed");
        GameObject trimLeft = GameObject.Find("Corner_Trim_Left");
        GameObject trimRight = GameObject.Find("Corner_Trim_Right");
        GameObject headboard = GameObject.Find("Bed_Headboard");
        GameObject pillow = GameObject.Find("Bed_Pillow");
        GameObject blanket = GameObject.Find("Bed_Blanket");
        GameObject nightstand = GameObject.Find("Nightstand");
        GameObject pillBottle = GameObject.Find("Capsule");
        GameObject ivStand = GameObject.Find("IV_Stand_Full");
        GameObject medCabinet = GameObject.Find("Med_Cabinet");
        GameObject pointLight = GameObject.Find("Point Light");
        GameObject xrOrigin = GameObject.Find("XR Origin (VR)");
        GameObject cameraOffset = GameObject.Find("Camera Offset");
        GameObject mainCamera = GameObject.Find("Main Camera");

        // 1. Основа кровати у задней стены палаты
        if (bed != null)
        {
            bed.transform.SetParent(null);
            bed.transform.position = new Vector3(-2.0f, 0.25f, -3.7f);
            bed.transform.rotation = Quaternion.identity;
            bed.transform.localScale = new Vector3(1.2f, 0.5f, 2.2f);
        }

        // 2. Бортики и изголовье (открепляются от масштаба кровати)
        if (trimLeft != null)
        {
            trimLeft.transform.SetParent(null);
            trimLeft.transform.position = new Vector3(-2.62f, 0.32f, -3.7f);
            trimLeft.transform.rotation = Quaternion.identity;
            trimLeft.transform.localScale = new Vector3(0.04f, 0.16f, 2.2f);
        }

        if (trimRight != null)
        {
            trimRight.transform.SetParent(null);
            trimRight.transform.position = new Vector3(-1.38f, 0.32f, -3.7f);
            trimRight.transform.rotation = Quaternion.identity;
            trimRight.transform.localScale = new Vector3(0.04f, 0.16f, 2.2f);
        }

        if (headboard != null)
        {
            headboard.transform.SetParent(null);
            headboard.transform.position = new Vector3(-2.0f, 0.65f, -4.84f);
            headboard.transform.rotation = Quaternion.identity;
            headboard.transform.localScale = new Vector3(1.28f, 1.3f, 0.08f);
        }

        // 3. Подушка и одеяло
        if (pillow != null)
        {
            pillow.transform.SetParent(null);
            pillow.transform.position = new Vector3(-2.0f, 0.56f, -4.45f);
            pillow.transform.rotation = Quaternion.identity;
            pillow.transform.localScale = new Vector3(0.7f, 0.12f, 0.42f);
        }

        if (blanket != null)
        {
            blanket.transform.SetParent(null);
            blanket.transform.position = new Vector3(-2.0f, 0.52f, -3.35f);
            blanket.transform.rotation = Quaternion.identity;
            blanket.transform.localScale = new Vector3(1.18f, 0.06f, 1.45f);
        }

        // 4. Тумбочка справа от кровати и баночка с таблетками ровно на ней
        if (nightstand != null)
        {
            nightstand.transform.SetParent(null);
            nightstand.transform.position = new Vector3(-1.05f, 0.3f, -3.55f);
            nightstand.transform.rotation = Quaternion.identity;
            nightstand.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
        }

        if (pillBottle != null)
        {
            pillBottle.transform.SetParent(null);
            pillBottle.transform.position = new Vector3(-1.05f, 0.68f, -3.55f);
            pillBottle.transform.rotation = Quaternion.identity;
        }

        // 5. Капельница слева от кровати и медицинский шкаф у левой стены
        if (ivStand != null)
        {
            ivStand.transform.SetParent(null);
            ivStand.transform.position = new Vector3(-2.95f, 0f, -3.55f);
            ivStand.transform.rotation = Quaternion.identity;
        }

        if (medCabinet != null)
        {
            medCabinet.transform.SetParent(null);
            medCabinet.transform.position = new Vector3(-4.55f, 1.0f, -1.5f);
            medCabinet.transform.rotation = Quaternion.identity;
        }

        // 6. Верхний свет палаты над зоной койки
        if (pointLight != null)
        {
            pointLight.transform.position = new Vector3(-2.0f, 2.7f, -3.2f);
        }

        // 7. Камера от первого лица: пациент лежит на подушке и видит палату, тумбочку и капельницу
        if (xrOrigin != null)
        {
            xrOrigin.transform.position = new Vector3(-2.0f, 0.85f, -4.45f);
            xrOrigin.transform.rotation = Quaternion.Euler(14f, 12f, 0f);
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

        Debug.Log("Палата и камера от первого лица успешно настроены!");
    }
}