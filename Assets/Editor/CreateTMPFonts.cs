// CreateTMPFonts.cs
// Editor-скрипт: создаёт TMP Font Asset из TTF файлов в Assets/Fonts/
// Запуск: Tools → Create TMP Font Assets

using UnityEngine;
using UnityEditor;
using TMPro;
using TMPro.EditorUtilities;

public static class CreateTMPFonts
{
    [MenuItem("Tools/Create TMP Font Assets")]
    public static void Run()
    {
        CreateFontAsset("Assets/Fonts/PressStart2P-Regular.ttf", "Assets/Fonts/PressStart2P-Regular SDF.asset");
        CreateFontAsset("Assets/Fonts/BarlowCondensed-Bold.ttf", "Assets/Fonts/BarlowCondensed-Bold SDF.asset");
    }

    private static void CreateFontAsset(string fontPath, string outputPath)
    {
        // Проверяем, не создан ли уже
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath);
        if (existing != null)
        {
            Debug.Log($"[CreateTMPFonts] Already exists: {outputPath}");
            return;
        }

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"[CreateTMPFonts] Font not found: {fontPath}");
            return;
        }

        // Создаём TMP Font Asset
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
        if (fontAsset == null)
        {
            Debug.LogError($"[CreateTMPFonts] Failed to create font asset from: {fontPath}");
            return;
        }

        AssetDatabase.CreateAsset(fontAsset, outputPath);

        // Atlas texture сохраняем как sub-asset
        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        // Material сохраняем как sub-asset
        if (fontAsset.material != null)
        {
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[CreateTMPFonts] Created: {outputPath}");
    }
}
