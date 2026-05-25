// ReorganizeFolders.cs
// Editor-скрипт: реорганизует структуру папок проекта.
// Запуск: Tools → Reorganize Project Folders

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class ReorganizeFolders
{
    [MenuItem("Tools/Reorganize Project Folders")]
    public static void Run()
    {
        // ШАГ 1 — Создать папки
        CreateFolderRecursive("Assets/Art/UI/Icons/Click");
        CreateFolderRecursive("Assets/Art/UI/Icons/Passive");
        CreateFolderRecursive("Assets/Art/UI/Icons/Special");
        CreateFolderRecursive("Assets/Art/UI/Backgrounds");
        CreateFolderRecursive("Assets/Art/UI/Elements");
        CreateFolderRecursive("Assets/Data/Ranks");
        CreateFolderRecursive("Assets/Data/Slots");
        CreateFolderRecursive("Assets/Prefabs/Cards");
        CreateFolderRecursive("Assets/Prefabs/UI");
        CreateFolderRecursive("Assets/_Scenes");
        CreateFolderRecursive("Assets/Fonts");

        int moved = 0;

        // ШАГ 2 — Переместить файлы

        // Icons
        moved += MoveAllAssets("Assets/UI/Icons/CLICK",   "Assets/Art/UI/Icons/Click");
        moved += MoveAllAssets("Assets/UI/Icons/PASSIVE", "Assets/Art/UI/Icons/Passive");
        moved += MoveAllAssets("Assets/UI/Icons/SPECIAL", "Assets/Art/UI/Icons/Special");

        // Backgrounds
        moved += MoveAllAssets("Assets/UI/Backgrounds", "Assets/Art/UI/Backgrounds");

        // Elements (отдельные файлы)
        moved += MoveSingleAsset("Assets/UI/KPI.png",              "Assets/Art/UI/Elements/KPI.png");
        moved += MoveSingleAsset("Assets/UI/padlock.png",          "Assets/Art/UI/Elements/padlock.png");
        moved += MoveSingleAsset("Assets/UI/vignette.png",         "Assets/Art/UI/Elements/vignette.png");
        moved += MoveSingleAsset("Assets/UI/price_background1.png", "Assets/Art/UI/Elements/price_background1.png");
        moved += MoveSingleAsset("Assets/UI/price_background2.png", "Assets/Art/UI/Elements/price_background2.png");
        moved += MoveSingleAsset("Assets/UI/price_background3.png", "Assets/Art/UI/Elements/price_background3.png");

        // GameData → Data
        moved += MoveAllAssets("Assets/GameData/Ranks", "Assets/Data/Ranks");
        moved += MoveAllAssets("Assets/GameData/Slots", "Assets/Data/Slots");

        // Prefabs
        moved += MoveSingleAsset("Assets/Prefab/CardButton1.prefab", "Assets/Prefabs/Cards/CardButton1.prefab");
        moved += MoveSingleAsset("Assets/Prefab/CardButton2.prefab", "Assets/Prefabs/Cards/CardButton2.prefab");
        moved += MoveSingleAsset("Assets/Prefab/CardButton3.prefab", "Assets/Prefabs/Cards/CardButton3.prefab");
        moved += MoveAllAssets("Assets/Prefab/UI", "Assets/Prefabs/UI");

        // Scenes
        moved += MoveSingleAsset("Assets/Scenes/MainScene.unity", "Assets/_Scenes/MainScene.unity");

        AssetDatabase.Refresh();
        Debug.Log($"[ReorganizeFolders] Done: {moved} files moved");
    }

    private static void CreateFolderRecursive(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string folderName = Path.GetFileName(path);

        if (!AssetDatabase.IsValidFolder(parent))
            CreateFolderRecursive(parent);

        AssetDatabase.CreateFolder(parent, folderName);
        Debug.Log($"[ReorganizeFolders] Created folder: {path}");
    }

    private static int MoveAllAssets(string sourceFolder, string destFolder)
    {
        if (!AssetDatabase.IsValidFolder(sourceFolder))
        {
            Debug.LogWarning($"[ReorganizeFolders] Source folder not found: {sourceFolder}");
            return 0;
        }

        string[] guids = AssetDatabase.FindAssets("", new[] { sourceFolder });
        int count = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Пропускаем подпапки и ассеты из вложенных папок (только прямые дочерние)
            string assetDir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
            if (assetDir != sourceFolder)
                continue;

            string fileName = Path.GetFileName(assetPath);
            string destPath = destFolder + "/" + fileName;

            count += MoveSingleAsset(assetPath, destPath);
        }

        return count;
    }

    private static int MoveSingleAsset(string source, string dest)
    {
        if (!File.Exists(source) && !AssetDatabase.IsValidFolder(source))
        {
            Debug.LogWarning($"[ReorganizeFolders] Not found: {source}");
            return 0;
        }

        if (File.Exists(dest) || AssetDatabase.IsValidFolder(dest))
        {
            Debug.LogWarning($"[ReorganizeFolders] Already exists: {dest}");
            return 0;
        }

        string result = AssetDatabase.MoveAsset(source, dest);

        if (string.IsNullOrEmpty(result))
        {
            Debug.Log($"[ReorganizeFolders] Moved: {source} → {dest}");
            return 1;
        }
        else
        {
            Debug.LogError($"[ReorganizeFolders] Failed: {source} → {dest} ({result})");
            return 0;
        }
    }
}
