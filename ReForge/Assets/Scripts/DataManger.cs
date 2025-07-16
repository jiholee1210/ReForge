using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DataManger : MonoBehaviour
{
    public static DataManger Instance { get; private set; }

    public Dictionary<int, UnitData> unitDataDict;
    public Dictionary<int, OutsourcingData> outsourcingDataDict;
    public Dictionary<int, ProjectData> projectDataDict;
    public Dictionary<int, TempUpgradeData> tempUpgradeDataDict;
    public Dictionary<int, PermUpgradeData> permUpgradeDataDict;

    public Unit unit;
    public Goods goods;
    public TempUpgrade tempUpgrade;
    public PermUpgrade permUpgrade;
    public ShopUnit shopUnit;

    private Task waitForUnitData;
    private Task waitForOutsourcingData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;

        unit = new();
        goods = new();
        tempUpgrade = new();
        permUpgrade = new();
        shopUnit = new();

        waitForUnitData = LoadUnitData();
        waitForOutsourcingData = LoadOutsourcingData();
        //LoadProjectData();
        //LoadTempUpgradeData();
        //LoadPermUpgradeData();

        goods.gold += 100000;
    }

    void Start()
    {

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

    private async void LoadProjectData()
    {
        projectDataDict = new Dictionary<int, ProjectData>();
        var handle = Addressables.LoadAssetsAsync<ProjectData>("Project", project =>
        {
            projectDataDict[project.id] = project;
        });

        await handle.Task;
    }

    private async void LoadTempUpgradeData()
    {
        tempUpgradeDataDict = new Dictionary<int, TempUpgradeData>();
        var handle = Addressables.LoadAssetsAsync<TempUpgradeData>("TempUpgrade", tempUpgrade =>
        {
            tempUpgradeDataDict[tempUpgrade.id] = tempUpgrade;
        });

        await handle.Task;
    }

    private async void LoadPermUpgradeData()
    {
        permUpgradeDataDict = new Dictionary<int, PermUpgradeData>();
        var handle = Addressables.LoadAssetsAsync<PermUpgradeData>("PermUpgrade", permUpgrade =>
        {
            permUpgradeDataDict[permUpgrade.id] = permUpgrade;
        });

        await handle.Task;
    }

    public Task WaitForLoadingUnitData()
    {
        return waitForUnitData;
    }

    public Task WaitForLoadingOutsourcingData()
    {
        return waitForOutsourcingData;
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
}

public class Unit
{
    public List<UnitInfo> units = new();
}

public class UnitInfo
{
    public int id;
    public int upgrade;
    public int place;
}

public class Goods
{
    public int gold;
}

public class ShopUnit
{
    public List<int> canBuy = new();
}

public class TempUpgrade
{
    public List<int> canUpgrade = new();
    public List<int> complete = new();
}

public class PermUpgrade
{
    public List<int> canUpgrade = new();
    public List<int> complete = new();
}
