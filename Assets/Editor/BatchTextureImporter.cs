// BatchTextureImporter.cs
// Editor-скрипт: применяет настройки импорта ко всем иконкам в Assets/UI/Icons.
// Запуск: Tools → Apply Icons Import Settings

using UnityEngine;
using UnityEditor;

public static class BatchTextureImporter
{
    [MenuItem("Tools/Apply Icons Import Settings")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/UI/Icons" });
        int updated = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;

            importer.SaveAndReimport();
            updated++;
        }

        Debug.Log($"[BatchTextureImporter] Done: {updated} textures updated");
    }
}
