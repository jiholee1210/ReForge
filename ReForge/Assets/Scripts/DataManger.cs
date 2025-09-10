using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class DataManger : MonoBehaviour
{
    public static DataManger Instance { get; private set; }

    [SerializeField] private Button save;

    public Dictionary<int, UnitData> unitDataDict;
    public Dictionary<int, OutsourcingData> outsourcingDataDict;
    public Dictionary<int, ProjectData> projectDataDict;
    public Dictionary<int, TempUpgradeData> tempUpgradeDataDict;
    public Dictionary<int, PermUpgradeData> permUpgradeDataDict;
    public Dictionary<int, RelicData> relicDataDict;

    public Unit unit;
    public Goods goods;
    public TempUpgrade tempUpgrade;
    public PermUpgrade permUpgrade;
    public ShopUnit shopUnit;
    public Auto auto;
    public Work work;
    public Relic relic;

    private Task waitForUnitData;
    private Task waitForOutsourcingData;
    private Task waitForProjectData;
    private Task waitForTempUpgradeData;
    private Task waitForPermUpgradeData;
    private Task waitForRelicData;

    private string unitPath;
    private string goodsPath;
    private string tempUpgradePath;
    private string permUpgradePath;
    private string shopUnitPath;
    private string autoPath;
    private string workPath;
    private string relicPath;

    private Dictionary<Type, (object Instance, string path)> saveDict = new();

    private KeyValuePair<int, PermUpgradeData> baseGold;

    public static event Action OnTryReset;

    private float curTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        waitForUnitData = LoadUnitData();
        waitForOutsourcingData = LoadOutsourcingData();
        waitForProjectData = LoadProjectData();
        waitForTempUpgradeData = LoadTempUpgradeData();
        waitForPermUpgradeData = LoadPermUpgradeData();
        waitForRelicData = LoadRelicData();
        
        DataSetting();
    }

    void Start()
    {
        if (permUpgrade.complete.Contains(11)) UIManager.Instance.ActiveRelic();
        StartCoroutine(AutoSave());
        save.onClick.AddListener(() => SaveAll());
    }

    private IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(180f);
            SaveAll();
        }
    }

    private void Register()
    {
        saveDict[typeof(Unit)] = (unit, unitPath);
        saveDict[typeof(Goods)] = (goods, goodsPath);
        saveDict[typeof(TempUpgrade)] = (tempUpgrade, tempUpgradePath);
        saveDict[typeof(PermUpgrade)] = (permUpgrade, permUpgradePath);
        saveDict[typeof(ShopUnit)] = (shopUnit, shopUnitPath);
        saveDict[typeof(Auto)] = (auto, autoPath);
        saveDict[typeof(Work)] = (work, workPath);
        saveDict[typeof(Relic)] = (relic, relicPath);  
    }

    private void DataSetting()
    {
        unit = new();
        goods = new();
        tempUpgrade = new();
        permUpgrade = new();
        shopUnit = new();
        auto = new();
        work = new();
        relic = new();

        unitPath = Path.Combine(Application.persistentDataPath, "unit.json");
        goodsPath = Path.Combine(Application.persistentDataPath, "goods.json");
        tempUpgradePath = Path.Combine(Application.persistentDataPath, "tempUpgrade.json");
        permUpgradePath = Path.Combine(Application.persistentDataPath, "permUpgrade.json");
        shopUnitPath = Path.Combine(Application.persistentDataPath, "shopUnit.json");
        autoPath = Path.Combine(Application.persistentDataPath, "auto.json");
        workPath = Path.Combine(Application.persistentDataPath, "work.json");
        relicPath = Path.Combine(Application.persistentDataPath, "relic.json");

        Register();

        foreach (var item in saveDict)
        {
            if (!File.Exists(item.Value.path))
            {
                string json = JsonUtility.ToJson(item.Value.Instance, true);
                File.WriteAllText(item.Value.path, json);
            }
            else
            {
                string json = File.ReadAllText(item.Value.path);
                JsonUtility.FromJsonOverwrite(json, item.Value.Instance);
            }
        }
    }

    public void SaveAll()
    {
        foreach (var item in saveDict)
        {
            string json = JsonUtility.ToJson(item.Value.Instance, true);
            File.WriteAllText(item.Value.path, json);
        }
    }

    public void LoadAll()
    {
        foreach (var item in saveDict)
        {
            string json = File.ReadAllText(item.Value.path);
            JsonUtility.FromJsonOverwrite(json, item.Value.Instance);
        }
    }

    private async Task LoadUnitData()
    {
        unitDataDict = new Dictionary<int, UnitData>();
        var handle = Addressables.LoadAssetsAsync<UnitData>("Unit", unit =>
        {
            unitDataDict[unit.id] = unit;
        });

        await handle.Task;
    }

    private async Task LoadOutsourcingData()
    {
        outsourcingDataDict = new Dictionary<int, OutsourcingData>();
        var handle = Addressables.LoadAssetsAsync<OutsourcingData>("Outsourcing", outsourcing =>
        {
            outsourcingDataDict[outsourcing.id] = outsourcing;
        });

        await handle.Task;
    }

    private async Task LoadProjectData()
    {
        projectDataDict = new Dictionary<int, ProjectData>();
        var handle = Addressables.LoadAssetsAsync<ProjectData>("Project", project =>
        {
            projectDataDict[project.id] = project;
        });

        await handle.Task;
    }

    private async Task LoadTempUpgradeData()
    {
        tempUpgradeDataDict = new Dictionary<int, TempUpgradeData>();
        var handle = Addressables.LoadAssetsAsync<TempUpgradeData>("TempUpgrade", tempUpgrade =>
        {
            tempUpgradeDataDict[tempUpgrade.id] = tempUpgrade;
        });

        await handle.Task;
    }

    private async Task LoadPermUpgradeData()
    {
        permUpgradeDataDict = new Dictionary<int, PermUpgradeData>();
        var handle = Addressables.LoadAssetsAsync<PermUpgradeData>("PermUpgrade", permUpgrade =>
        {
            permUpgradeDataDict[permUpgrade.id] = permUpgrade;
        });
        
        await handle.Task;
    }

    private async Task LoadRelicData()
    {
        relicDataDict = new Dictionary<int, RelicData>();
        var handel = Addressables.LoadAssetsAsync<RelicData>("Relic", relic =>
        {
            relicDataDict[relic.id] = relic;
        });

        await handel.Task;
    }

    public Task WaitForLoadingUnitData()
    {
        return waitForUnitData;
    }

    public Task WaitForLoadingOutsourcingData()
    {
        return waitForOutsourcingData;
    }

    public Task WaitForLoadingProjectData()
    {
        return waitForProjectData;
    }

    public Task WaitForLoadingTempUpgradeData()
    {
        return waitForTempUpgradeData;
    }

    public Task WaitForLoadingPermUpgradeData()
    {
        return waitForPermUpgradeData;
    }

    public Task WaitForLoadingRelicData()
    {
        return waitForRelicData;
    }

    public UnitData GetUnitData(int id)
    {
        return unitDataDict[id];
    }

    public OutsourcingData GetOutsourcingData(int id)
    {
        return outsourcingDataDict[id];
    }

    public ProjectData GetProjectData(int id)
    {
        return projectDataDict[id];
    }

    public TempUpgradeData GetTempUpgradeData(int id)
    {
        return tempUpgradeDataDict[id];
    }

    public PermUpgradeData GetPermUpgradeData(int id)
    {
        return permUpgradeDataDict[id];
    }

    public RelicData GetRelicData(int id)
    {
        return relicDataDict[id];
    }

    public void ResetData()
    {
        baseGold = permUpgradeDataDict
                .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.BaseGold);

        unit.Reset();
        goods.Reset(permUpgrade.complete.Contains(baseGold.Key) ? Mathf.RoundToInt(baseGold.Value.value) : 0);
        shopUnit.Reset();
        tempUpgrade.Reset();
        auto.Reset();
        work.Reset();
        SaveAll();

        if (permUpgradeDataDict.ContainsKey(11)) UIManager.Instance.ActiveRelic();

        UIManager.Instance.SetGoldText();
        UIManager.Instance.SetFragText();
        OnTryReset?.Invoke();
    }
}

public class Unit
{
    public List<UnitInfo> units = new();

    public void Reset()
    {
        units = new();
    }
}

[Serializable]
public class UnitInfo
{
    public int id;
    public int upgrade;
    public int place;
}

public class Goods
{
    public int gold = 500;
    public int frag;

    public void Reset(int baseGold)
    {
        gold = 500 + baseGold;
        frag = 0;
    }
}

public class ShopUnit
{
    public List<int> canBuy = new();

    public ShopUnit()
    {
        canBuy.Add(0);
    }

    public void Reset()
    {
        canBuy = new()
        {
            0
        };
    }
}

public class TempUpgrade
{
    public int[] upgrade = new int[4];
    public void Reset()
    {
        upgrade = new int[4];
    }
}

public class PermUpgrade
{
    public int upPoint;
    public List<int> complete = new();
    public int open;
}

public class Auto
{
    public int buyLevel;
    public int upgradeLevel;
    public int upgradeGrade;
    public bool autoBuyOn = false;
    public bool autoUpgradeOn = false;

    public void Reset()
    {
        buyLevel = 0;
        upgradeLevel = 0;
        upgradeGrade = 0;
        autoBuyOn = false;
        autoUpgradeOn = false;
    }
}

public class Work
{
    public int outsourcingID = -1;
    public int projectID = -1;
    public int curOut;
    public int curProject;
    public int outsourcingMax = 15;
    public List<int> completeProject = new();

    public void Reset()
    {
        outsourcingID = -1;
        projectID = -1;
        curOut = 0;
        curProject = 0;
        outsourcingMax = 15;
        completeProject = new();
    }
}

public class Relic
{
    public int[] relics = new int[16];
    public int count;
}
