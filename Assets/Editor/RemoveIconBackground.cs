// RemoveIconBackground.cs
// Editor-скрипт: удаляет цветной фон иконок, делая его прозрачным.
// Определяет цвет фона по 4 углам, обнуляет alpha у близких пикселей.
// Запуск: Tools → Remove Icon Backgrounds

using System.IO;
using UnityEngine;
using UnityEditor;

public static class RemoveIconBackground
{
    private const int COLOR_THRESHOLD = 30;
    private const string ICONS_ROOT = "Assets/Art/UI/Icons";

    [MenuItem("Tools/Remove Icon Backgrounds")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ICONS_ROOT });
        int processed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.EndsWith(".png"))
                continue;

            // Только файлы из Click/Passive/Special (не подпапки глубже)
            string dir = Path.GetDirectoryName(path).Replace("\\", "/");
            if (dir != ICONS_ROOT + "/Click" &&
                dir != ICONS_ROOT + "/Passive" &&
                dir != ICONS_ROOT + "/Special")
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            // Шаг 1: сделать readable + reimport
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            // Шаг 2: загрузить текстуру
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogWarning("[RemoveBG] Cannot load: " + path);
                continue;
            }

            int w = tex.width;
            int h = tex.height;
            Color32[] pixels = tex.GetPixels32();

            // Шаг 3: определить цвет фона по 4 углам
            Color32 tl = pixels[(h - 1) * w + 0];         // top-left
            Color32 tr = pixels[(h - 1) * w + (w - 1)];   // top-right
            Color32 bl = pixels[0];                         // bottom-left
            Color32 br = pixels[w - 1];                     // bottom-right

            int bgR = (tl.r + tr.r + bl.r + br.r) / 4;
            int bgG = (tl.g + tr.g + bl.g + br.g) / 4;
            int bgB = (tl.b + tr.b + bl.b + br.b) / 4;

            Debug.Log(string.Format("[RemoveBG] {0}: bg=({1},{2},{3}) size={4}x{5}",
                path, bgR, bgG, bgB, w, h));

            // Шаг 4: обнулить alpha у пикселей, близких к фону
            int removed = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                int dr = pixels[i].r - bgR;
                int dg = pixels[i].g - bgG;
                int db = pixels[i].b - bgB;
                int dist = dr * dr + dg * dg + db * db;

                if (dist < COLOR_THRESHOLD * COLOR_THRESHOLD)
                {
                    pixels[i] = new Color32(0, 0, 0, 0);
                    removed++;
                }
            }

            if (removed == 0)
            {
                Debug.Log("[RemoveBG] No background pixels found: " + path);
                continue;
            }

            // Шаг 5: записать обратно
            Texture2D newTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            newTex.SetPixels32(pixels);
            newTex.Apply();

            byte[] png = newTex.EncodeToPNG();
            string absolutePath = Path.GetFullPath(path);
            File.WriteAllBytes(absolutePath, png);
            Object.DestroyImmediate(newTex);

            // Шаг 6: reimport с финальными настройками
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.SaveAndReimport();

            processed++;
            Debug.Log(string.Format("[RemoveBG] {0}: removed {1} pixels ({2}%)",
                path, removed, removed * 100 / (w * h)));
        }

        AssetDatabase.Refresh();
        Debug.Log("[RemoveBG] Done: " + processed + " files imported and processed");
    }
}
