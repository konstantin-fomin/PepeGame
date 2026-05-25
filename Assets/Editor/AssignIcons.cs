// AssignIcons.cs
// Editor-скрипт: назначает иконки из Assets/Art/UI/Icons/{type}/
// на апгрейды в Assets/Data/Slots/Intern_{type}.asset по индексу.
// Запуск: Tools → Assign Icons To Slots

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class AssignIcons
{
    [MenuItem("Tools/Assign Icons To Slots")]
    public static void Run()
    {
        int total = 0;

        total += AssignSlot("Click",   "Assets/Art/UI/Icons/Click",   "Assets/Data/Slots/Intern_Click.asset");
        total += AssignSlot("Passive", "Assets/Art/UI/Icons/Passive", "Assets/Data/Slots/Intern_Passive.asset");
        total += AssignSlot("Special", "Assets/Art/UI/Icons/Special", "Assets/Data/Slots/Intern_Special.asset");

        AssetDatabase.SaveAssets();
        Debug.Log("[AssignIcons] Done: " + total + " icons assigned");
    }

    private static int AssignSlot(string label, string iconFolder, string slotPath)
    {
        // Загрузить спрайты из папки
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { iconFolder });
        List<Sprite> sprites = new List<Sprite>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Только прямые дочерние, не из подпапок
            string dir = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            if (dir != iconFolder) continue;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                sprites.Add(sprite);
        }

        sprites.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        if (sprites.Count == 0)
        {
            Debug.LogWarning("[AssignIcons] No sprites in " + iconFolder);
            return 0;
        }

        // Загрузить SlotBranchConfig
        Object slotObj = AssetDatabase.LoadAssetAtPath<Object>(slotPath);
        if (slotObj == null)
        {
            Debug.LogError("[AssignIcons] Slot not found: " + slotPath);
            return 0;
        }

        SerializedObject so = new SerializedObject(slotObj);
        SerializedProperty upgrades = so.FindProperty("upgrades");

        if (upgrades == null || !upgrades.isArray)
        {
            Debug.LogError("[AssignIcons] No upgrades array in " + slotPath);
            return 0;
        }

        int count = Mathf.Min(upgrades.arraySize, sprites.Count);
        int assigned = 0;

        for (int i = 0; i < count; i++)
        {
            SerializedProperty upg = upgrades.GetArrayElementAtIndex(i);
            SerializedProperty icon = upg.FindPropertyRelative("icon");
            SerializedProperty title = upg.FindPropertyRelative("title");

            if (icon == null) continue;

            icon.objectReferenceValue = sprites[i];
            assigned++;

            string t = title != null ? title.stringValue : "?";
            Debug.Log("[AssignIcons] " + label + "[" + i + "] " + t + " → " + sprites[i].name);
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(slotObj);

        return assigned;
    }
}
