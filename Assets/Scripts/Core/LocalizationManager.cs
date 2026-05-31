// LocalizationManager.cs
// Version: 2026-05-31 v1.0
// Purpose: RU/EN localization. Loads Resources/localization_en (CSV: ID,Section,Context,RU,EN),
//          exposes Get(id) + card helpers, persists choice, auto-detects browser language on WebGL.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public enum Language { RU, EN }

    public static LocalizationManager Instance { get; private set; }

    public Language CurrentLanguage { get; private set; } = Language.RU;
    public event Action OnLanguageChanged;

    private const string ResourceName = "localization_en";
    private const string PrefKey = "selectedLanguage";

    // Raw parsed CSV rows (column order resolved from the header).
    private readonly List<string[]> rows = new List<string[]>();
    private int colId = 0, colSection = 1, colContext = 2, colRu = 3, colEn = 4;

    private readonly Dictionary<string, string> translations = new Dictionary<string, string>();
    private readonly Dictionary<string, string> cardTitles = new Dictionary<string, string>();
    private readonly Dictionary<string, string> cardDescriptions = new Dictionary<string, string>();

    [DllImport("__Internal")]
    private static extern string GetBrowserLanguage();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCsv();

        string saved = PlayerPrefs.GetString(PrefKey, "");
        if (saved == "RU") CurrentLanguage = Language.RU;
        else if (saved == "EN") CurrentLanguage = Language.EN;
        else CurrentLanguage = DetectBrowserLanguage();

        RebuildDictionary();
    }

    // ================= PUBLIC API =================

    public void SetLanguage(Language lang)
    {
        CurrentLanguage = lang;
        PlayerPrefs.SetString(PrefKey, lang.ToString());
        PlayerPrefs.Save();
        RebuildDictionary();
        OnLanguageChanged?.Invoke();
    }

    public string Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return string.Empty;
        return translations.TryGetValue(id, out var val) ? val : id;
    }

    /// <summary>True only if a non-empty translation exists for this id.</summary>
    public bool TryGet(string id, out string value)
    {
        value = null;
        if (string.IsNullOrEmpty(id)) return false;
        return translations.TryGetValue(id, out value) && !string.IsNullOrEmpty(value);
    }

    /// <summary>Localized card title for an upgrade id, or null if none (caller falls back to asset value).</summary>
    public string GetCardTitle(string upgradeId)
    {
        if (string.IsNullOrEmpty(upgradeId)) return null;
        return cardTitles.TryGetValue(upgradeId, out var v) ? v : null;
    }

    public string GetCardDescription(string upgradeId)
    {
        if (string.IsNullOrEmpty(upgradeId)) return null;
        return cardDescriptions.TryGetValue(upgradeId, out var v) ? v : null;
    }

    /// <summary>Returns "LOC_0287" -> "LOC_0288" for offset 1 (used by active events whose four
    /// fields occupy consecutive ids). Returns null if the id is not in the LOC_#### form.</summary>
    public string OffsetId(string baseId, int offset)
    {
        if (string.IsNullOrEmpty(baseId)) return null;
        Match m = Regex.Match(baseId, @"^([A-Za-z_]+)(\d+)$");
        if (!m.Success) return null;
        int num;
        if (!int.TryParse(m.Groups[2].Value, out num)) return null;
        int width = m.Groups[2].Value.Length;
        return m.Groups[1].Value + (num + offset).ToString().PadLeft(width, '0');
    }

    // ================= INTERNAL =================

    private void LoadCsv()
    {
        rows.Clear();
        TextAsset asset = Resources.Load<TextAsset>(ResourceName);
        if (asset == null)
        {
            Debug.LogError($"[Localization] Resources/{ResourceName} not found");
            return;
        }

        List<string[]> parsed = ParseCsv(asset.text);
        if (parsed.Count == 0)
        {
            Debug.LogError("[Localization] CSV parsed to 0 rows");
            return;
        }

        string[] header = parsed[0];
        for (int i = 0; i < header.Length; i++)
        {
            switch (header[i].Trim().ToUpperInvariant())
            {
                case "ID": colId = i; break;
                case "SECTION": colSection = i; break;
                case "CONTEXT": colContext = i; break;
                case "RU": colRu = i; break;
                case "EN": colEn = i; break;
            }
        }

        for (int r = 1; r < parsed.Count; r++)
            rows.Add(parsed[r]);
    }

    private void RebuildDictionary()
    {
        translations.Clear();
        cardTitles.Clear();
        cardDescriptions.Clear();

        bool en = CurrentLanguage == Language.EN;

        foreach (string[] row in rows)
        {
            string id = Field(row, colId);
            if (string.IsNullOrEmpty(id)) continue;

            string ru = Field(row, colRu);
            string enVal = Field(row, colEn);
            string value = en ? enVal : ru;
            if (en && string.IsNullOrEmpty(value)) value = ru; // graceful fallback to RU

            translations[id] = value;

            if (Field(row, colSection) == "CARDS")
            {
                string ctx = Field(row, colContext);
                string cardId = ParseCardId(ctx);
                if (!string.IsNullOrEmpty(cardId))
                {
                    if (ctx.Contains("название")) cardTitles[cardId] = value;
                    else if (ctx.Contains("описание")) cardDescriptions[cardId] = value;
                }
            }
        }
    }

    private static string Field(string[] row, int col)
    {
        return (col >= 0 && col < row.Length) ? row[col] : string.Empty;
    }

    // Context looks like "Карточка 58 (Intern/Click) - название" -> "58".
    private static string ParseCardId(string context)
    {
        if (string.IsNullOrEmpty(context)) return null;
        Match m = Regex.Match(context, @"(\d+)");
        return m.Success ? m.Groups[1].Value : null;
    }

    private Language DetectBrowserLanguage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            string lang = GetBrowserLanguage();
            if (string.IsNullOrEmpty(lang)) return Language.RU;
            if (lang.ToLower().StartsWith("ru")) return Language.RU;
            return Language.EN;
        }
        catch
        {
            return Language.RU;
        }
#else
        return Language.RU;
#endif
    }

    // RFC4180-ish parser: quote-aware, handles "" escapes and quoted commas/newlines.
    private static List<string[]> ParseCsv(string text)
    {
        var result = new List<string[]>();
        if (string.IsNullOrEmpty(text)) return result;

        if (text.Length > 0 && text[0] == (char)0xFEFF) text = text.Substring(1); // strip BOM

        int i = 0, n = text.Length;
        var field = new StringBuilder();
        var record = new List<string>();
        bool inQuotes = false;

        while (i < n)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < n && text[i + 1] == '"') { field.Append('"'); i += 2; continue; }
                    inQuotes = false; i++; continue;
                }
                field.Append(c); i++; continue;
            }

            if (c == '"') { inQuotes = true; i++; continue; }
            if (c == ',') { record.Add(field.ToString()); field.Clear(); i++; continue; }
            if (c == '\r') { i++; continue; }
            if (c == '\n')
            {
                record.Add(field.ToString()); field.Clear();
                result.Add(record.ToArray()); record = new List<string>();
                i++; continue;
            }
            field.Append(c); i++;
        }

        if (field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            result.Add(record.ToArray());
        }
        return result;
    }
}
