// TrimIconSprites.cs
// Editor-скрипт: обрезает PNG иконок по непрозрачным пикселям.
// Перезаписывает файл обрезанным PNG, затем переимпортирует как Single sprite.
// Запуск: Tools → Trim Icon Sprites

using System.IO;
using UnityEngine;
using UnityEditor;

public static class TrimIconSprites
{
    [MenuItem("Tools/Trim Icon Sprites")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/UI/Icons" });
        int trimmed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            // Шаг 1: isReadable, Point, Uncompressed → reimport
            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            // Шаг 2: загрузить текстуру
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogWarning("[TrimIcons] Cannot load: " + path);
                continue;
            }

            // Шаг 3: получить пиксели
            int w = tex.width;
            int h = tex.height;
            Color32[] pixels = tex.GetPixels32();

            // Шаг 4: найти bounds (alpha > 10)
            int minX = w, maxX = 0, minY = h, maxY = 0;
            bool hasOpaque = false;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a > 10)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                        hasOpaque = true;
                    }
                }
            }

            // Шаг 5: пропустить если полностью прозрачный
            if (!hasOpaque)
            {
                Debug.LogWarning("[TrimIcons] Fully transparent: " + path);
                continue;
            }

            int newW = maxX - minX + 1;
            int newH = maxY - minY + 1;

            // Пропустить если уже обрезан
            if (newW == w && newH == h)
            {
                Debug.Log("[TrimIcons] Already trimmed: " + path);
                continue;
            }

            // Шаг 6: создать обрезанную текстуру
            Texture2D newTex = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
            Color32[] newPixels = new Color32[newW * newH];

            for (int y = 0; y < newH; y++)
            {
                for (int x = 0; x < newW; x++)
                {
                    newPixels[y * newW + x] = pixels[(y + minY) * w + (x + minX)];
                }
            }

            newTex.SetPixels32(newPixels);
            newTex.Apply();

            // Шаг 7: перезаписать PNG
            byte[] png = newTex.EncodeToPNG();
            string absolutePath = Path.GetFullPath(path);
            File.WriteAllBytes(absolutePath, png);
            Object.DestroyImmediate(newTex);

            // Шаг 8: переимпортировать как Single sprite
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = false;
            importer.SaveAndReimport();

            trimmed++;
            Debug.Log(string.Format("[TrimIcons] {0}: {1}x{2} → {3}x{4}",
                path, w, h, newW, newH));
        }

        AssetDatabase.Refresh();
        Debug.Log("[TrimIcons] Done: " + trimmed + " files trimmed");
    }
}
