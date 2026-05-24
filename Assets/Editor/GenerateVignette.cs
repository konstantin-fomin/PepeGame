// GenerateVignette.cs
// Одноразовый Editor-скрипт: генерирует радиальную виньетку 512x512.
// Запуск: Tools → Generate Vignette
// После успешного запуска можно удалить.

using UnityEngine;
using UnityEditor;
using System.IO;

public static class GenerateVignette
{
    [MenuItem("Tools/Generate Vignette")]
    public static void Run()
    {
        int size = 512;
        float cx = size * 0.5f;
        float cy = size * 0.5f;
        float maxRadius = Mathf.Sqrt(cx * cx + cy * cy);

        // Центральная прозрачная зона: 40% радиуса
        float innerRadius = 0.4f;

        // Цвет краёв: тёмно-янтарный
        float r = 0.15f;
        float g = 0.1f;
        float b = 0.02f;
        float maxAlpha = 0.8f;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / maxRadius;

                float alpha = 0f;

                if (dist > innerRadius)
                {
                    float t = (dist - innerRadius) / (1f - innerRadius);
                    t = Mathf.Clamp01(t);

                    // Плавная кривая (smoothstep)
                    t = t * t * (3f - 2f * t);

                    alpha = t * maxAlpha;
                }

                tex.SetPixel(x, y, new Color(r, g, b, alpha));
            }
        }

        tex.Apply();

        string dir = "Assets/UI";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string path = "Assets/UI/vignette.png";
        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(path, png);
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

        // Настроить импорт: Sprite, Read/Write, без сжатия для прозрачности
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        Debug.Log($"[GenerateVignette] Готово: {path} ({size}x{size})");
    }
}
