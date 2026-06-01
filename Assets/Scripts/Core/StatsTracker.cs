using UnityEngine;

public class StatsTracker : MonoBehaviour
{
    public static StatsTracker Instance;

    // Ключи PlayerPrefs
    private const string KEY_PLAYTIME  = "Stats_Playtime";
    private const string KEY_KPI       = "Stats_TotalKpi";
    private const string KEY_CLICKS    = "Stats_Clicks";
    private const string KEY_UPGRADES  = "Stats_Upgrades";

    public float TotalPlaytimeSeconds { get; private set; }
    public long  TotalKpiEarned       { get; private set; }
    public int   TotalClicks          { get; private set; }
    public int   UpgradesBought       { get; private set; }

    private float saveTimer = 0f;
    private const float SAVE_INTERVAL = 30f;
    private bool isTracking = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Load();
    }

    private void Update()
    {
        if (!isTracking) return;

        TotalPlaytimeSeconds += Time.deltaTime;

        saveTimer += Time.deltaTime;
        if (saveTimer >= SAVE_INTERVAL)
        {
            saveTimer = 0f;
            Save();
        }
    }

    public void StartTracking() => isTracking = true;
    public void StopTracking()  => isTracking = false;

    public void AddKpi(long amount)
    {
        if (amount > 0) TotalKpiEarned += amount;
    }

    public void AddClick()    => TotalClicks++;
    public void AddUpgrade()  => UpgradesBought++;

    public void Save()
    {
        PlayerPrefs.SetFloat(KEY_PLAYTIME, TotalPlaytimeSeconds);
        PlayerPrefs.SetString(KEY_KPI,     TotalKpiEarned.ToString());
        PlayerPrefs.SetInt(KEY_CLICKS,     TotalClicks);
        PlayerPrefs.SetInt(KEY_UPGRADES,   UpgradesBought);
        PlayerPrefs.Save();
    }

    public void ResetStats()
    {
        TotalPlaytimeSeconds = 0;
        TotalKpiEarned       = 0;
        TotalClicks          = 0;
        UpgradesBought       = 0;
        Save();
    }

    private void Load()
    {
        TotalPlaytimeSeconds = PlayerPrefs.GetFloat(KEY_PLAYTIME, 0f);

        if (!long.TryParse(PlayerPrefs.GetString(KEY_KPI, "0"), out long kpiVal)) kpiVal = 0;
        TotalKpiEarned = kpiVal;

        TotalClicks    = PlayerPrefs.GetInt(KEY_CLICKS,   0);
        UpgradesBought = PlayerPrefs.GetInt(KEY_UPGRADES, 0);
    }

    private void OnApplicationPause(bool pause) { if (pause) Save(); }
    private void OnApplicationQuit()            { Save(); }
}
