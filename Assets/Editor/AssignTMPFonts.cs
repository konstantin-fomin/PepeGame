// AssignTMPFonts.cs
// Editor-скрипт: назначает TMP Font Assets на все TextMeshProUGUI в сцене.
// PressStart2P → titleText на UpgradeCardView
// BarlowCondensed-Bold → все остальные TMP объекты
// Запуск: Tools → Assign TMP Fonts

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Reflection;

public static class AssignTMPFonts
{
    [MenuItem("Tools/Assign TMP Fonts")]
    public static void Run()
    {
        TMP_FontAsset pressStart = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/PressStart2P-Regular SDF.asset");
        TMP_FontAsset barlow = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Fonts/BarlowCondensed-Bold SDF.asset");

        if (pressStart == null)
        {
            Debug.LogError("[AssignTMPFonts] PressStart2P SDF not found!");
            return;
        }

        if (barlow == null)
        {
            Debug.LogError("[AssignTMPFonts] BarlowCondensed-Bold SDF not found!");
            return;
        }

        // Собираем titleText field IDs из UpgradeCardView
        System.Collections.Generic.HashSet<int> titleTextIDs =
            new System.Collections.Generic.HashSet<int>();

        UpgradeCardView[] cards = Object.FindObjectsByType<UpgradeCardView>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        FieldInfo titleField = typeof(UpgradeCardView).GetField(
            "titleText", BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (UpgradeCardView card in cards)
        {
            if (titleField != null)
            {
                TMP_Text title = titleField.GetValue(card) as TMP_Text;
                if (title != null)
                {
                    titleTextIDs.Add(title.GetInstanceID());
                    Debug.Log($"[AssignTMPFonts] Title found: {title.gameObject.name} (ID {title.GetInstanceID()})");
                }
            }
        }

        // Назначаем шрифты всем TMP
        TextMeshProUGUI[] allTMP = Object.FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        int pressCount = 0;
        int barlowCount = 0;

        foreach (TextMeshProUGUI tmp in allTMP)
        {
            Undo.RecordObject(tmp, "Assign TMP Font");

            if (titleTextIDs.Contains(tmp.GetInstanceID()))
            {
                tmp.font = pressStart;
                pressCount++;
                Debug.Log($"[AssignTMPFonts] PressStart2P → {tmp.gameObject.name}");
            }
            else
            {
                tmp.font = barlow;
                barlowCount++;
                Debug.Log($"[AssignTMPFonts] BarlowCondensed → {tmp.gameObject.name}");
            }

            EditorUtility.SetDirty(tmp);
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[AssignTMPFonts] Done: {pressCount} PressStart2P, {barlowCount} BarlowCondensed");
    }
}
