using System.IO;
using UnityEngine;
using UnityEditor;

public static class HospitalMaterials
{
    public static Shader GetLitShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        return shader != null ? shader : Shader.Find("Standard");
    }

    public static Material CreateMaterial(string name, Color color, float smoothness = 0.2f, float metallic = 0.0f)
    {
        EnsureFolders();
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(GetLitShader());
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        return mat;
    }

    public static Material CreateEmissiveMaterial(string name, Color baseColor, Color emissionColor, float intensity = 2.0f)
    {
        Material mat = CreateMaterial(name, baseColor, 0.4f, 0.0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor * intensity);
        return mat;
    }

    public static Material CreateTiledFloorMaterial()
    {
        EnsureFolders();
        string texPath = "Assets/Materials/Hospital_Floor_Tiles.png";
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color tileLight = new Color(0.76f, 0.79f, 0.81f);
        Color tileAlt = new Color(0.73f, 0.76f, 0.78f);
        Color grout = new Color(0.52f, 0.55f, 0.58f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isBorder = (x < 3 || y < 3 || (x >= 127 && x <= 129) || (y >= 127 && y <= 129));
                if (isBorder)
                {
                    tex.SetPixel(x, y, grout);
                }
                else
                {
                    bool checker = ((x < 128) ^ (y < 128));
                    tex.SetPixel(x, y, checker ? tileLight : tileAlt);
                }
            }
        }
        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (importedTex != null)
        {
            importedTex.wrapMode = TextureWrapMode.Repeat;
        }

        Material mat = CreateMaterial("Hospital_Floor_Mat", Color.white, 0.45f, 0.0f);
        mat.SetTexture("_BaseMap", importedTex);
        mat.SetTexture("_MainTex", importedTex);
        mat.SetTextureScale("_BaseMap", new Vector2(10f, 10f));
        mat.SetTextureScale("_MainTex", new Vector2(10f, 10f));
        return mat;
    }

    public static Material CreateECGScreenMaterial()
    {
        EnsureFolders();
        string texPath = "Assets/Materials/Medical_ECG_Screen.png";
        int width = 512;
        int height = 340;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color bg = new Color(0.02f, 0.06f, 0.07f);
        Color grid = new Color(0.05f, 0.16f, 0.14f);
        Color trace = new Color(0.10f, 0.98f, 0.48f);
        Color warn = new Color(0.95f, 0.35f, 0.25f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isGrid = (x % 32 == 0) || (y % 32 == 0);
                tex.SetPixel(x, y, isGrid ? grid : bg);
            }
        }

        int midY = height / 2;
        for (int x = 20; x < width - 20; x++)
        {
            int cycle = x % 140;
            int offsetY = 0;
            if (cycle > 45 && cycle <= 52) offsetY = (cycle - 45) * 4;
            else if (cycle > 52 && cycle <= 58) offsetY = 28 - (cycle - 52) * 5;
            else if (cycle > 62 && cycle <= 66) offsetY = -(cycle - 62) * 6;
            else if (cycle > 66 && cycle <= 73) offsetY = -24 + (cycle - 66) * 16;
            else if (cycle > 73 && cycle <= 81) offsetY = 88 - (cycle - 73) * 15;
            else if (cycle > 81 && cycle <= 86) offsetY = -32 + (cycle - 81) * 6;
            else if (cycle > 96 && cycle <= 106) offsetY = (int)(Mathf.Sin((cycle - 96) / 10f * Mathf.PI) * 18f);

            int py = Mathf.Clamp(midY + offsetY, 4, height - 5);
            for (int dy = -2; dy <= 2; dy++)
            {
                tex.SetPixel(x, py + dy, trace);
            }
        }

        for (int x = 20; x < 160; x++)
            for (int y = height - 34; y < height - 22; y++)
                tex.SetPixel(x, y, trace);

        for (int x = width - 150; x < width - 20; x++)
            for (int y = height - 34; y < height - 22; y++)
                tex.SetPixel(x, y, warn);

        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        Material mat = CreateEmissiveMaterial("Medical_ECG_Screen_Mat", Color.white, Color.white, 1.35f);
        mat.SetTexture("_BaseMap", importedTex);
        mat.SetTexture("_MainTex", importedTex);
        mat.SetTexture("_EmissionMap", importedTex);
        return mat;
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
    }
}