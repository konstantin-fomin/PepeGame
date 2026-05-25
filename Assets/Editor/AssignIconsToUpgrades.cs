// AssignIconsToUpgrades.cs
// Editor-скрипт: назначает иконки на апгрейды по совпадению title в имени файла.
// Спрайты из Assets/Art/UI/Icons/{Click,Passive,Special}/
// Assets из Assets/Data/Slots/{Rank}_{Slot}.asset
// Запуск: Tools → Assign Icons To Upgrades

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class AssignIconsToUpgrades
{
    private const string ICONS_ROOT = "Assets/Art/UI/Icons";
    private const string SLOTS_FOLDER = "Assets/Data/Slots";

    [MenuItem("Tools/Assign Icons To Upgrades")]
    public static void Run()
    {
        // Шаг 1: загрузить спрайты по папкам
        Dictionary<string, List<SpriteEntry>> spritesBySlot = new Dictionary<string, List<SpriteEntry>>();
        spritesBySlot["Click"] = LoadSprites(ICONS_ROOT + "/Click");
        spritesBySlot["Passive"] = LoadSprites(ICONS_ROOT + "/Passive");
        spritesBySlot["Special"] = LoadSprites(ICONS_ROOT + "/Special");

        Debug.Log("[AssignIcons] Loaded sprites: Click=" + spritesBySlot["Click"].Count +
            " Passive=" + spritesBySlot["Passive"].Count +
            " Special=" + spritesBySlot["Special"].Count);

        // Шаг 2: загрузить все SlotBranchConfig assets
        string[] assetGuids = AssetDatabase.FindAssets("t:SlotBranchConfig", new[] { SLOTS_FOLDER });

        if (assetGuids.Length == 0)
        {
            Debug.LogError("[AssignIcons] No SlotBranchConfig assets found in " + SLOTS_FOLDER);
            return;
        }

        int totalAssigned = 0;
        int totalMissing = 0;

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            SlotBranchConfig config = AssetDatabase.LoadAssetAtPath<SlotBranchConfig>(assetPath);

            if (config == null)
            {
                Debug.LogError("[AssignIcons] Failed to load: " + assetPath);
                continue;
            }

            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // Определить тип слота из имени asset (Intern_Click → Click)
            string slotKey = null;
            if (assetName.Contains("_Click")) slotKey = "Click";
            else if (assetName.Contains("_Passive")) slotKey = "Passive";
            else if (assetName.Contains("_Special")) slotKey = "Special";

            if (slotKey == null)
            {
                Debug.LogWarning("[AssignIcons] Cannot determine slot type for: " + assetName);
                continue;
            }

            List<SpriteEntry> sprites = spritesBySlot[slotKey];

            // Определить rank из имени asset (Intern_Click → Intern)
            string rank = assetName.Replace("_" + slotKey, "");

            if (config.upgrades == null || config.upgrades.Count == 0)
            {
                Debug.LogWarning("[AssignIcons] " + assetName + ": no upgrades, skipping");
                continue;
            }

            int assignedInAsset = 0;

            for (int i = 0; i < config.upgrades.Count; i++)
            {
                Upgrade upgrade = config.upgrades[i];

                if (string.IsNullOrEmpty(upgrade.title))
                {
                    Debug.LogWarning("[AssignIcons] " + assetName + "[" + i + "]: empty title, skipping");
                    continue;
                }

                // Ищем спрайт где имя файла содержит rank И title
                Sprite found = null;
                string foundName = null;

                foreach (SpriteEntry entry in sprites)
                {
                    // Имя файла: "09_Junior_Click_Горящие клавиши"
                    // Проверяем: содержит rank И title
                    if (entry.fileName.Contains(rank) && entry.fileName.Contains(upgrade.title))
                    {
                        found = entry.sprite;
                        foundName = entry.fileName;
                        break;
                    }
                }

                if (found != null)
                {
                    upgrade.icon = found;
                    assignedInAsset++;
                    totalAssigned++;
                    Debug.Log("[AssignIcons] " + assetName + "[" + i + "] \"" + upgrade.title + "\" ← " + foundName);
                }
                else
                {
                    upgrade.icon = null;
                    totalMissing++;
                    Debug.LogWarning("[AssignIcons] " + assetName + "[" + i + "] \"" + upgrade.title + "\" — NO MATCH");
                }
            }

            EditorUtility.SetDirty(config);
        }

        AssetDatabase.SaveAssets();

        Debug.Log("[AssignIcons] Done: " + totalAssigned + " assigned, " + totalMissing + " missing");
    }

    private static List<SpriteEntry> LoadSprites(string folderPath)
    {
        List<SpriteEntry> result = new List<SpriteEntry>();

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        // Собираем пути и сортируем по имени файла
        List<string> paths = new List<string>();
        foreach (string guid in guids)
        {
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }
        paths.Sort((a, b) => string.Compare(
            Path.GetFileName(a), Path.GetFileName(b), System.StringComparison.Ordinal));

        foreach (string path in paths)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning("[AssignIcons] Cannot load sprite: " + path);
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);
            SpriteEntry entry = new SpriteEntry();
            entry.sprite = sprite;
            entry.fileName = fileName;
            result.Add(entry);
        }

        return result;
    }

    private struct SpriteEntry
    {
        public Sprite sprite;
        public string fileName;
    }
}
