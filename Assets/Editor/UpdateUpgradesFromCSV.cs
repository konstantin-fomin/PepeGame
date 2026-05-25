// UpdateUpgradesFromCSV.cs
// Editor-скрипт: обновляет данные апгрейдов в SlotBranchConfig assets
// из Assets/Upgrades/upgrades.txt (CSV).
// Запуск: Tools → Update Upgrades From CSV

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class UpdateUpgradesFromCSV
{
    private const string CSV_PATH = "Assets/Upgrades/upgrades.txt";
    private const string SLOTS_FOLDER = "Assets/Data/Slots/";

    [MenuItem("Tools/Update Upgrades From CSV")]
    public static void Run()
    {
        // Шаг 1: прочитать CSV
        string fullPath = Path.GetFullPath(CSV_PATH);
        if (!File.Exists(fullPath))
        {
            Debug.LogError("[UpdateUpgrades] CSV not found: " + fullPath);
            return;
        }

        string[] lines = File.ReadAllLines(fullPath, System.Text.Encoding.UTF8);

        // Парсинг: rank,slot,id,title,description,price,click,passive,duration
        // Группируем по Rank_Slot
        Dictionary<string, List<string[]>> groups = new Dictionary<string, List<string[]>>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            if (line.StartsWith("rank,"))
                continue;

            string[] parts = line.Split(',');
            if (parts.Length < 9)
            {
                Debug.LogWarning("[UpdateUpgrades] Bad line " + (i + 1) + ": " + line);
                continue;
            }

            string rank = parts[0].Trim();
            string slot = parts[1].Trim();
            string key = rank + "_" + slot;

            if (!groups.ContainsKey(key))
                groups[key] = new List<string[]>();

            groups[key].Add(parts);
        }

        Debug.Log("[UpdateUpgrades] Parsed " + groups.Count + " groups from CSV");

        // Шаг 2: обновить каждый asset
        int totalUpdated = 0;

        foreach (KeyValuePair<string, List<string[]>> kvp in groups)
        {
            string assetName = kvp.Key;
            List<string[]> rows = kvp.Value;
            string assetPath = SLOTS_FOLDER + assetName + ".asset";

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                Debug.LogError("[UpdateUpgrades] Asset not found: " + assetPath);
                continue;
            }

            SerializedObject so = new SerializedObject(asset);
            SerializedProperty upgrades = so.FindProperty("upgrades");

            if (upgrades == null || !upgrades.isArray)
            {
                Debug.LogError("[UpdateUpgrades] No upgrades array in " + assetPath);
                continue;
            }

            int count = Mathf.Min(upgrades.arraySize, rows.Count);

            for (int i = 0; i < count; i++)
            {
                string[] parts = rows[i];
                // parts: rank,slot,id,title,description,price,click,passive,duration
                string id = parts[2].Trim();
                string title = parts[3].Trim();
                string description = parts[4].Trim();
                int price = int.Parse(parts[5].Trim());
                int click = int.Parse(parts[6].Trim());
                int passive = int.Parse(parts[7].Trim());
                float duration = float.Parse(parts[8].Trim());

                SerializedProperty upg = upgrades.GetArrayElementAtIndex(i);

                SerializedProperty pId = upg.FindPropertyRelative("id");
                SerializedProperty pTitle = upg.FindPropertyRelative("title");
                SerializedProperty pDesc = upg.FindPropertyRelative("description");
                SerializedProperty pPrice = upg.FindPropertyRelative("basePrice");
                SerializedProperty pClick = upg.FindPropertyRelative("clickBonus");
                SerializedProperty pPassive = upg.FindPropertyRelative("passiveBonus");
                SerializedProperty pDuration = upg.FindPropertyRelative("specialDuration");

                if (pId != null) pId.stringValue = id;
                if (pTitle != null) pTitle.stringValue = title;
                if (pDesc != null) pDesc.stringValue = description;
                if (pPrice != null) pPrice.intValue = price;
                if (pClick != null) pClick.intValue = click;
                if (pPassive != null) pPassive.intValue = passive;
                if (pDuration != null) pDuration.floatValue = duration;

                totalUpdated++;

                Debug.Log(string.Format("[UpdateUpgrades] {0}[{1}] {2}: {3} price={4} click={5} passive={6} dur={7}",
                    assetName, i, id, title, price, click, passive, duration));
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[UpdateUpgrades] Done: " + totalUpdated + " upgrades updated");
    }
}
