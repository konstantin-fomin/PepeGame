// ImportUpgrades.cs
// Editor-скрипт: импортирует апгрейды из Assets/Upgrades/upgrades.txt
// в SlotBranchConfig assets (Assets/Data/Slots/{Rank}_{Slot}.asset).
// Очищает массив upgrades и заполняет заново по rank+slot, порядок по id.
// Запуск: Tools → Import Upgrades From TXT

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class ImportUpgrades
{
    private const string TXT_PATH = "Assets/Upgrades/upgrades.txt";
    private const string SLOTS_FOLDER = "Assets/Data/Slots/";

    [MenuItem("Tools/Import Upgrades From TXT")]
    public static void Run()
    {
        string fullPath = Path.Combine(Application.dataPath, "..", TXT_PATH);
        fullPath = Path.GetFullPath(fullPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError("[ImportUpgrades] File not found: " + fullPath);
            return;
        }

        string[] lines = File.ReadAllLines(fullPath, System.Text.Encoding.UTF8);

        // Парсинг CSV: rank,slot,id,title,description,price,click,passive,duration
        // Группируем по ключу Rank_Slot
        Dictionary<string, List<ParsedUpgrade>> groups = new Dictionary<string, List<ParsedUpgrade>>();
        int parsedCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            // Пропуск заголовка (любой формат)
            if (line.StartsWith("id,") || line.StartsWith("id\t") ||
                line.StartsWith("rank,") || line.StartsWith("rank\t"))
                continue;

            // Автоопределение разделителя
            char separator = line.Contains('\t') ? '\t' : ',';
            string[] parts = line.Split(separator);

            if (parts.Length < 9)
            {
                Debug.LogWarning("[ImportUpgrades] Skipping line " + (i + 1) + " (" + parts.Length + " columns): " + line);
                continue;
            }

            // Определяем порядок колонок:
            // TSV (новый): id, rank, upgradeType, title, description, basePrice, clickBonus, passiveBonus, specialDuration
            // CSV (старый): rank, slot, id, title, description, price, click, passive, duration
            bool isTsvOrder = separator == '\t';

            string rank;
            string slot;
            ParsedUpgrade parsed = new ParsedUpgrade();

            if (isTsvOrder)
            {
                // id, rank, upgradeType, title, description, basePrice, clickBonus, passiveBonus, specialDuration
                parsed.id = parts[0].Trim();
                rank = parts[1].Trim();
                slot = parts[2].Trim();
                parsed.title = parts[3].Trim();
                parsed.description = parts[4].Trim();

                if (!int.TryParse(parts[5].Trim(), out parsed.basePrice))
                {
                    Debug.LogWarning("[ImportUpgrades] Bad price on line " + (i + 1) + ": " + parts[5]);
                    continue;
                }

                if (!int.TryParse(parts[6].Trim(), out parsed.clickBonus))
                    parsed.clickBonus = 0;

                float passiveFloat;
                if (float.TryParse(parts[7].Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out passiveFloat))
                    parsed.passiveBonus = (int)passiveFloat;
                else
                    parsed.passiveBonus = 0;

                if (!float.TryParse(parts[8].Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out parsed.specialDuration))
                    parsed.specialDuration = 0f;
            }
            else
            {
                // rank, slot, id, title, description, price, click, passive, duration
                rank = parts[0].Trim();
                slot = parts[1].Trim();
                parsed.id = parts[2].Trim();
                parsed.title = parts[3].Trim();
                parsed.description = parts[4].Trim();

                if (!int.TryParse(parts[5].Trim(), out parsed.basePrice))
                {
                    Debug.LogWarning("[ImportUpgrades] Bad price on line " + (i + 1) + ": " + parts[5]);
                    continue;
                }

                if (!int.TryParse(parts[6].Trim(), out parsed.clickBonus))
                    parsed.clickBonus = 0;

                if (!int.TryParse(parts[7].Trim(), out parsed.passiveBonus))
                    parsed.passiveBonus = 0;

                if (!float.TryParse(parts[8].Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out parsed.specialDuration))
                    parsed.specialDuration = 0f;
            }

            string key = rank + "_" + slot;

            if (!groups.ContainsKey(key))
                groups[key] = new List<ParsedUpgrade>();

            groups[key].Add(parsed);
            parsedCount++;
        }

        Debug.Log("[ImportUpgrades] Parsed " + parsedCount + " upgrades in " + groups.Count + " groups");

        // Найти все SlotBranchConfig assets
        string[] assetGuids = AssetDatabase.FindAssets("t:SlotBranchConfig", new[] { SLOTS_FOLDER.TrimEnd('/') });

        if (assetGuids.Length == 0)
        {
            Debug.LogError("[ImportUpgrades] No SlotBranchConfig assets found in " + SLOTS_FOLDER);
            return;
        }

        int totalAssets = 0;
        int totalUpgrades = 0;

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            SlotBranchConfig config = AssetDatabase.LoadAssetAtPath<SlotBranchConfig>(assetPath);

            if (config == null)
            {
                Debug.LogError("[ImportUpgrades] Failed to load: " + assetPath);
                continue;
            }

            // Имя asset = Rank_Slot (например Intern_Click)
            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // Очистить массив
            if (config.upgrades == null)
                config.upgrades = new List<Upgrade>();
            else
                config.upgrades.Clear();

            // Заполнить из CSV если есть данные для этого ключа
            if (groups.ContainsKey(assetName))
            {
                List<ParsedUpgrade> rows = groups[assetName];

                // Сортировка по id (числовая)
                rows.Sort((a, b) =>
                {
                    int ia, ib;
                    bool aOk = int.TryParse(a.id, out ia);
                    bool bOk = int.TryParse(b.id, out ib);
                    if (aOk && bOk) return ia.CompareTo(ib);
                    return string.Compare(a.id, b.id, System.StringComparison.Ordinal);
                });

                foreach (ParsedUpgrade row in rows)
                {
                    Upgrade upgrade = new Upgrade();
                    upgrade.id = row.id;
                    upgrade.title = row.title;
                    upgrade.description = row.description;
                    upgrade.basePrice = row.basePrice;
                    upgrade.clickBonus = row.clickBonus;
                    upgrade.passiveBonus = row.passiveBonus;
                    upgrade.specialDuration = row.specialDuration;
                    upgrade.icon = null;

                    config.upgrades.Add(upgrade);
                }

                Debug.Log("[ImportUpgrades] " + assetName + ": " + rows.Count + " upgrades");
                totalUpgrades += rows.Count;
            }
            else
            {
                Debug.LogWarning("[ImportUpgrades] " + assetName + ": 0 upgrades (no data in TXT)");
            }

            EditorUtility.SetDirty(config);
            totalAssets++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[ImportUpgrades] Done: " + totalUpgrades + " upgrades across " + totalAssets + " assets");
    }

    private struct ParsedUpgrade
    {
        public string id;
        public string title;
        public string description;
        public int basePrice;
        public int clickBonus;
        public int passiveBonus;
        public float specialDuration;
    }
}
