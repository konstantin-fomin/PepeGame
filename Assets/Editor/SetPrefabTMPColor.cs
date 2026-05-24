// SetPrefabTMPColor.cs
// Editor-скрипт: устанавливает color = кремовый #FFF5D6 для всех TMP в префабах карточек.
// Запуск: Tools → Set Prefab TMP Color

using UnityEngine;
using UnityEditor;
using TMPro;

public static class SetPrefabTMPColor
{
    private static readonly string[] prefabPaths =
    {
        "Assets/Prefab/CardButton1.prefab",
        "Assets/Prefab/CardButton2.prefab",
        "Assets/Prefab/CardButton3.prefab"
    };

    [MenuItem("Tools/Set Prefab TMP Color")]
    public static void Run()
    {
        Color cream = new Color(1.0f, 0.96f, 0.84f, 1.0f);
        int total = 0;

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[SetPrefabTMPColor] Prefab not found: {path}");
                continue;
            }

            TextMeshProUGUI[] tmps = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (TextMeshProUGUI tmp in tmps)
            {
                tmp.color = cream;
                EditorUtility.SetDirty(tmp);
                total++;
                Debug.Log($"[SetPrefabTMPColor] {path} → {tmp.gameObject.name} set to #FFF5D6");
            }

            EditorUtility.SetDirty(prefab);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[SetPrefabTMPColor] Done: {total} TMP components updated");
    }
}
