#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class UpgradeTextImporter : EditorWindow
{
    private TextAsset dataFile;

    [MenuItem("Tools/Upgrade Text Importer")]
    public static void Open()
    {
        GetWindow<UpgradeTextImporter>("Upgrade Text Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Upgrade Text Importer", EditorStyles.boldLabel);

        dataFile = (TextAsset)EditorGUILayout.ObjectField(
            "Data File (.txt or .csv)",
            dataFile,
            typeof(TextAsset),
            false
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Import Upgrades"))
        {
            if (dataFile == null)
            {
                Debug.LogError("Data file not assigned");
                return;
            }

            Import(dataFile.text);
        }
    }

    private void Import(string text)
    {
        string[] lines = text.Split('\n');
        Dictionary<string, List<Upgrade>> map = new();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string line = lines[i].Trim();
            string[] cols = line.Split(',');

            if (cols.Length < 9)
            {
                Debug.LogError($"Invalid line {i + 1}: {line}");
                continue;
            }

            string rank = cols[0].Trim();
            string slot = cols[1].Trim();
            string id = cols[2].Trim();
            string title = cols[3].Trim();
            string desc = cols[4].Trim();

            int price = int.Parse(cols[5]);
            int click = int.Parse(cols[6]);
            int passive = int.Parse(cols[7]);
            float duration = float.Parse(cols[8]);

            Upgrade upg = new Upgrade
            {
                id = id,
                title = title,
                description = desc,
                basePrice = price,
                clickBonus = click,
                passiveBonus = passive,
                specialDuration = duration
            };

            string key = $"{rank}_{slot}";

            if (!map.ContainsKey(key))
                map[key] = new List<Upgrade>();

            map[key].Add(upg);
        }

        foreach (var pair in map)
        {
            string assetPath = $"Assets/GameData/Slots/{pair.Key}.asset";
            SlotBranchConfig config =
                AssetDatabase.LoadAssetAtPath<SlotBranchConfig>(assetPath);

            if (config == null)
            {
                Debug.LogError($"SlotBranchConfig not found: {assetPath}");
                continue;
            }

            Undo.RecordObject(config, "Import Upgrades");
            config.upgrades = pair.Value;
            EditorUtility.SetDirty(config);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Text import completed");
    }
}
#endif
