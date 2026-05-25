// RemoveCheckerboard.cs
// Editor-скрипт: удаляет шахматный фон (белые и серые клетки) из иконок.
// Запуск: Tools → Remove Checkerboard Background

using System.IO;
using UnityEngine;
using UnityEditor;

public static class RemoveCheckerboard
{
    private const string ICONS_ROOT = "Assets/Art/UI/Icons";

    [MenuItem("Tools/Remove Checkerboard Background")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ICONS_ROOT });
        int processed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.EndsWith(".png"))
                continue;

            string dir = Path.GetDirectoryName(path).Replace("\\", "/");
            if (dir != ICONS_ROOT + "/Click" &&
                dir != ICONS_ROOT + "/Passive" &&
                dir != ICONS_ROOT + "/Special")
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            // Шаг 1: isReadable + reimport
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            // Шаг 2: загрузить текстуру
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogWarning("[RemoveCheckerboard] Cannot load: " + path);
                continue;
            }

            int w = tex.width;
            int h = tex.height;
            Color32[] pixels = tex.GetPixels32();

            // Шаг 3: убрать белые и серые клетки
            int removed = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                byte r = pixels[i].r;
                byte g = pixels[i].g;
                byte b = pixels[i].b;

                bool isWhite = r > 240 && g > 240 && b > 240;
                bool isGray = r > 180 && g > 180 && b > 180 && r < 210 && g < 210 && b < 210;

                if (isWhite || isGray)
                {
                    pixels[i] = new Color32(0, 0, 0, 0);
                    removed++;
                }
            }

            if (removed == 0)
            {
                Debug.Log("[RemoveCheckerboard] No checkerboard pixels: " + path);
                continue;
            }

            // Шаг 4: сохранить PNG
            Texture2D newTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            newTex.SetPixels32(pixels);
            newTex.Apply();

            byte[] png = newTex.EncodeToPNG();
            string absolutePath = Path.GetFullPath(path);
            File.WriteAllBytes(absolutePath, png);
            Object.DestroyImmediate(newTex);

            // Шаг 5: reimport
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.SaveAndReimport();

            processed++;
            Debug.Log(string.Format("[RemoveCheckerboard] {0}: removed {1} pixels ({2}%)",
                path, removed, removed * 100 / (w * h)));
        }

        AssetDatabase.Refresh();
        Debug.Log("[RemoveCheckerboard] Done: " + processed + " files");
    }
}
